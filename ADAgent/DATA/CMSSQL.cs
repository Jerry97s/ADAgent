using ADAgent.DATA;
using ADAgent.UTIL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADAgent
{
    enum DB_CMD
    {
        SELECT,
        UPDATE,
        DELETE,
        INSERT

    }
    class CMSSQL
    {
        private static SqlConnection[] conn = null;

        public DBInfo pDBInfo;

        public bool bAutoM = false;
        bool bNext = false;
        int nNowIdx = 0;
        string sNowCarno = "";
        int nBaseTimerChk = 0;
        int nOperTimerChk = 0;
        string sSqlCmd;
        string sSqlCmd_Prev;
        string sSqlCMd_Mg;
        string sSqlCmd_Update;
        int nUploadCnt = 0;
        //string sSqlCmd_Insert_Pay;
        string sDB;
        int nDBIdx = 0;
        SqlCommand[] Cmd = new SqlCommand[10];
        SqlDataReader[] reader = new SqlDataReader[10];
        DB_CMD eCmd;

        System.Timers.Timer pBaseTimer = new System.Timers.Timer();
        System.Timers.Timer pPrevTimer = new System.Timers.Timer();

        //sCarno_Reg, sStartDttm, sEndDttm, sRegDiv, sGroupNm, sUserNm, sTelno

        public delegate void DF_Reg(string sCarno_Reg, string sStartDttm, string sEndDttm, string sRegDiv, string sGroupNm, string sUserNm, string sTelno, string sLotArea, string sAreaArray);
        public DF_Reg dfReg = null;

        public delegate void DF_Visit(string sCarno_Visit, string sStartDttm, string sEndDttm, string sLotArea);
        public DF_Visit dfVisit = null;

        public delegate bool DF_Cal(string sCarno, int nFee, string sOutDt = "", string sPaydt = "", string sCredit = "", bool bPrev = false);
        public DF_Cal dfCal = null;

        public delegate bool DF_Re_Cal(string sCarno, int nFee, string sPaydt = "", string sApproval = "", bool bPrev = false);
        public DF_Re_Cal dfReCal = null;

        public delegate void DF_CarOut(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sOutEqpm, int nService);
        public DF_CarOut dfCarOut = null;

        public delegate void DF_CarOut_ID(int nAID, string sCarno);
        public DF_CarOut_ID dfCarOutID = null;

        public delegate bool DF_Auto_Minab();
        public DF_Auto_Minab dfAutoMinab = null;

        public delegate void DF_SetLog(string sDB, string sLog);
        public DF_SetLog dfSetLog = null;

        public delegate void DF_RegUp();
        public DF_RegUp dfRegUp = null;

        public delegate void DF_Migration(int nEqpm, string sCarno, string sInDttm, string sImgPath);
        public DF_Migration dfMigration = null;

        public delegate void DF_PassTrns(string sCarno, string sOutDttm, string sPic, string sOutEqpm);
        public DF_PassTrns dfPassTrns = null;

        public static string DBConnString { get; private set; }

        public bool bDB_Cmd_Run_Stat = false;


        private static int errorBoxCount = 0;

        /// <summary>

        /// 생성자

        /// </summary>

        public CMSSQL(int nIdx)
        {

            nNowIdx = nIdx;

            pDBInfo.nDBType = 1;
            pDBInfo.sID = CIni.Load("ADA_DB", "DBID", "0", CData.sDBPath);
            pDBInfo.sPW = CIni.Load("ADA_DB", "DBPW", "0", CData.sDBPath);
            pDBInfo.sBaseDB = CIni.Load("ADA_DB", "BASEDB", "0", CData.sDBPath);
            pDBInfo.sOperDB = CIni.Load("ADA_DB", "OPERDB", "0", CData.sDBPath);
            pDBInfo.sIP = CIni.Load("ADA_DB", "DBIP", "0", CData.sDBPath);
            pDBInfo.nPort = int.Parse(CIni.Load("ADA_DB", "DBPort", "0", CData.sDBPath));

            pDBInfo.bStatus = new bool[10];

            for (int i = 0; i < 10; i++)
            {
                pDBInfo.bStatus[i] = new bool();
                pDBInfo.bStatus[i] = false;
            }
            conn = new SqlConnection[10];
            //pDBInfo.bStatus[0] = false;
            //pDBInfo.bStatus[1] = false;

            pDBInfo.stTableinfo.sTotalTable = CIni.Load("ADA_DB_TABLE", "InOut", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sPayTable = CIni.Load("ADA_DB_TABLE", "Pay", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sRegTable = CIni.Load("ADA_DB_TABLE", "Reg", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sDiscountTable = CIni.Load("ADA_DB_TABLE", "Discount", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sLocateClm = CIni.Load("ADA_DB_TABLE", "Locate", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sFranClm= CIni.Load("ADA_DB_TABLE", "Fran", "0", CData.sDBPath);
            pDBInfo.stTableinfo.sTerminalClm = CIni.Load("ADA_DB_TABLE", "Terminal", "0", CData.sDBPath);
            CData.sAirPort = CIni.Load("ADA_DB_TABLE", "AirPort", "0", CData.sDBPath);


            pBaseTimer.Interval = 2000;
            pBaseTimer.Elapsed += DB_Base_Timer_Event;

        }




        public SqlConnection BaseDBConn

        {

            get

            {

                if (!ConnectToDB(0))
                {

                    return null;
                }

                return conn[0];

            }

        }
        public SqlConnection OperDBConn

        {

            get

            {

                if (!ConnectToDB(1))
                {

                    return null;
                }

                return conn[1];

            }

        }


        /// <summary>

        /// Database 접속 시도

        /// </summary>

        /// <returns></returns>

        public bool ConnectToDB(int nIdx, bool bPrev = false)
        {
            switch (nIdx)
            {
                case 0:
                    sDB = pDBInfo.sBaseDB;
                    if (bPrev)
                        sDB = pDBInfo.sOperDB;
                    break;
                default:
                    sDB = pDBInfo.sOperDB;
                    break;

            }

            DBConnString = "Server=" + pDBInfo.sIP + "; Database=" + sDB + "; uid=" + pDBInfo.sID + "; pwd=" + pDBInfo.sPW;

            if (conn[nIdx] != null)
                conn[nIdx] = null;

            //서버명, DB명, id,pw
            conn[nIdx] = new SqlConnection(DBConnString);

            if (dfSetLog != null)
                dfSetLog(sDB, DBConnString);

            try
            {

                if (IsDBConnected(nIdx))
                {

                    conn[nIdx].Open();

                    if (conn[nIdx].State == System.Data.ConnectionState.Open)
                    {

                        pDBInfo.bStatus[nIdx] = true;
                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Connect : {" + DBConnString + "} Success");

                        if (dfSetLog != null)
                            dfSetLog(sDB, "DB Connect : {" + DBConnString + "} Success");
                    }
                    else
                    {
                        pDBInfo.bStatus[nIdx] = false;

                    }

                }
                //CLog.LOG(LOG_TYPE.DB, "DB Connect : {" + "Server=" + pDBInfo.sIP + "; Database=" + pDBInfo.sDB + "; uid=" + pDBInfo.sID + "; pwd=" + pDBInfo.sPW + "}");


            }
            catch (SqlException e)
            {

                errorBoxCount++;

                if (errorBoxCount == 1)

                {

                    CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx + " ConnectToDB Error : {" + e.ToString() + "}");

                }

            }
            finally
            {
            }

            return pDBInfo.bStatus[nIdx];

        }



        /// <summary>

        /// Database Open 여부 확인

        /// </summary>

        public static bool IsDBConnected(int nIdx)
        {
            {

                if (conn[nIdx].State != System.Data.ConnectionState.Open)

                {

                    return true;

                }

                return false;

            }

        }
        public void DB_Prev_Timer_Event(object sender, System.Timers.ElapsedEventArgs e)
        {
            bAutoM = true;



            if (nBaseTimerChk <= 60)
            {
                if (Cmd_Run(1))
                {
                    pBaseTimer.Stop();

                }

            }
            else
            {
                dfAutoMinab();

                pBaseTimer.Stop();
            }

            CLog.LOG(LOG_TYPE.DB_M, "AutoMinab=" + bAutoM.ToString() + " Timer Tick=" + nBaseTimerChk.ToString());

            nBaseTimerChk++;
        }
        public void DB_Base_Timer_Event(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (CData.garOpt1[1] == 0)
            {
                bAutoM = true;



                if (nBaseTimerChk <= 60)
                {
                    if (Cmd_Run(1))
                    {
                        pBaseTimer.Stop();

                    }

                }
                else
                {
                    dfAutoMinab();

                    pBaseTimer.Stop();
                }

                CLog.LOG(LOG_TYPE.DB_M, "AutoMinab=" + bAutoM.ToString() + " Timer Tick=" + nBaseTimerChk.ToString());
            }
            else
            {
                if (nBaseTimerChk <= 60)
                {
                    if (Cmd_Run(1, true, 1))
                    {
                        pBaseTimer.Stop();

                    }

                }
                else
                {
                    pBaseTimer.Stop();
                }

            }
            nBaseTimerChk++;
            
        }

        public void DB_Base_Timer_Stop()
        {
            pBaseTimer.Stop();
        }

        // Beh 0 -> CAL, 1 -> REG
        public void DB_SELECT(int nBeh, string sCond, bool bPrevCar = false, string sOutEqpm = "", int nReaderIdx = 0, string sLotArea = "")
        {
            bool bFlag = false;

            if (Cmd[nReaderIdx] != null)
                Cmd[nReaderIdx] = null;
            //if (Cmd[1] != null)
            //    Cmd[1] = null;

            eCmd = DB_CMD.SELECT;

            sSqlCmd = "Select ";
            sSqlCmd_Prev = "Select ";
            switch (nBeh)
            {
                case 0:
                    sSqlCmd += "acPlate1, dtValidStartDate, dtValidEndDate, acUserName from " + pDBInfo.stTableinfo.sRegTable + " where iLotArea = " + pDBInfo.stTableinfo.sLocateClm + ";";
                    Cmd[0] = new SqlCommand(sSqlCmd, conn[0]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select Reg Start");

                    Cmd_Run(0);

                    break;
                case 1:
                    pBaseTimer.Stop();
                    nBaseTimerChk = 0;
                    bAutoM = false;
                    //convert(nvarchar(50), dtPayDate, 126) Like '%06-03%'
                    if (bPrevCar)
                    {
                        sSqlCmd += "top 1 acPlate1, dIncome, dtOutDate, iID, acGoOutPicName from " + pDBInfo.stTableinfo.sTotalTable + " where iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and iOutEqpm = " + pDBInfo.stTableinfo.sAirPort + "  order by dtOutDate desc;";
                    }
                    else
                    {
                        sSqlCmd += "top 1 acPlate1, dIncome, iCredit, dtOutDate, dtPayDate from " + pDBInfo.stTableinfo.sTotalTable + " where acPlate1 = '" + sCond + "' and iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and convert(nvarchar(50), dtPayDate, 126) Like '%" + DateTime.Now.ToString("MM-dd") + "%' order by dtPayDate desc;"; //iLotArea 36
                    }

                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);
                    pBaseTimer.Start();
                    break;
                case 2:
                    if (!bPrevCar)
                    {
                        sSqlCmd += "acPlate1, dIncome, dtOutDate, iCredit, acGoOutPicName, iOutEqpm, dServiceA from " + pDBInfo.stTableinfo.sTotalTable + " where iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and dtOutDate >= Convert(DateTime, '" + DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss") + "') and dtOutDate is not null order by dtOutDate desc;";
                        Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                        CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select CarPlate Start");
                        
                    }
                    else
                    {

                        //sSqlCmd = "Select top 1 CarPlate, Fee, OriginTrnsDate, ApprovalNo from " + pDBInfo.stTableinfo.sPayTable + " where iLotarea = "+ pDBInfo.stTableinfo.sLocateClm + " CarPlate = '"+ sCond + "' and convert(nvarchar(50), OriginTrnsDate, 126) Like '%" + DateTime.Now.ToString("MM-dd") + "%' order by OriginTrnsDate desc;";
                        sSqlCmd += "acPlate1, dIncome, dtPayDate, iCredit, iID, iPayClient, dServiceA from " + pDBInfo.stTableinfo.sTotalTable + " where iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and dtPayDate is not null and dtPayDate >= Convert(DateTime, '" + DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss") + "') order by dtPaydate desc;";
                        Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                        CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select In_Car Start");
                    }
                    Cmd_Run(0, true, 0, eCmd, nReaderIdx);
                    break;
                case 3:
                    sSqlCmd += "top 1 iID from " + pDBInfo.stTableinfo.sTotalTable + " where iLotarea = " + pDBInfo.stTableinfo.sLocateClm + " and acPlate1 = '" + sCond + "' order by dtInDate desc;";
                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select CarID Start");
                    Cmd_Run(1, false, 1);
                    break;
                case 4:
                    //--select acPlate1, dtInDate,acEntrancePicName from TCKTTRNS where dtOutDate is null and dtInDate > Convert(date, '2022-06-10');
                    //select* from TCKTTRNS where dtInDate > Convert(date, '2022-06-01');
                    string[] sMg_Date = sCond.Split('|');
                    //if(pDBInfo.)
                    //sSqlCmd += " iInEqpm, acPlate1, dtInDate, acEntrancePicName from " + pDBInfo.stTableinfo.sTotalTable + " where iLotarea = " + pDBInfo.stTableinfo.sLocateClm + 
                    //    " and dtOutDate is null and dtInDate > Convert(date, '" + sMg_Date[0] + "') and dtInDate < Convert(date, '" + sMg_Date[1] + "');";
                    if (bPrevCar)
                    {
                        sSqlCmd += " BB.* from (select iEqpm, acPlate, dtTrnsDate, acPicName, iInOutStatus, ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER " +
        "from LPRTRNS where dtTrnsDate > Convert(datetime, '" + sMg_Date[0] + "' + ' " + sMg_Date[1] + "') and dtTrnsDate < CONVERT(datetime, '" + sMg_Date[2] + "' + ' " + sMg_Date[3] + "') and iLotArea = " + pDBInfo.stTableinfo.sLocateClm + ") BB WHERE REG_ORDER = 1 AND iInOutStatus = 0 order by BB.dtTrnsDate asc";
                    }
                    else
                    {
                        sSqlCmd += " BB.* from (select iEqpm, acPlate, dtTrnsDate, acPicName, iInOutStatus, ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER " +
"from LPRTRNS where dtTrnsDate > Convert(date, '" + sMg_Date[0] + "') and dtTrnsDate < CONVERT(date, '" + sMg_Date[1] + "') and iLotArea = " + pDBInfo.stTableinfo.sLocateClm + ") BB WHERE REG_ORDER = 1 AND iInOutStatus = 0 order by BB.dtTrnsDate asc";
                    }
                    //sSqlCmd = "Select iInEqpm, acPlate1, dtInDate, acEntrancePicName from " + pDBInfo.stTableinfo.sTotalTable + " where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                    //    " and dtOutDate is null and dtInDate > Convert(date, '" + DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd") + "');";

                    //dtInDate > Convert(date, '2022-07-05')

                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);

                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select InCar Start");
                    Cmd_Run(1, false, 2, eCmd, 1);
                    break;
                case 5:
                    sSqlCmd += " acPlate1, dIncome, dtOutDate, iCredit, acGoOutPicName, iOutEqpm, dServiceA  from lprtrns where iLotarea = " + pDBInfo.stTableinfo.sLocateClm + " and dtTrnsDate > Convert(datetime, '" + DateTime.Now.AddMinutes(-4).ToString("yyyy-MM-dd HH:mm:ss") + "') and iInOutStatus = 1 order by dtTrnsDate asc;";
                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select CarID Start");
                    Cmd_Run(1, false, 3);
                    break;
                case 6:
                    nBaseTimerChk = 0;
                    bAutoM = false;
                    string[] sMg_Dif_Date = sCond.Split('|');
                    //convert(nvarchar(50), dtPayDate, 126) Like '%06-03%'
                    if (!bPrevCar)
                    {
                        sSqlCmd += "BB.* from (select iEqpm, acPlate, dtTrnsDate, acPicName, iInOutStatus, ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER " +
    "from LPRTRNS where dtTrnsDate > Convert(datetime, '" + DateTime.Now.ToString("yyyy-MM-dd") + "' + ' " + DateTime.Now.AddMinutes(-4).ToString("HH:mm:ss") + "') and iLotArea = " + pDBInfo.stTableinfo.sLocateClm + ") BB WHERE REG_ORDER = 1 AND iInOutStatus = 0 order by BB.dtTrnsDate desc";
                    }
                    else
                    {
                        sSqlCmd += "iEqpm, acPlate, dtTrnsDate, acPicName from lprtrns where iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and dtTrnsDate > Convert(datetime, '" + DateTime.Now.AddMinutes(-4).ToString("yyyy-MM-dd HH:mm:ss") + "') and iInOutStatus = 0 order by dtTrnsDate asc;";
                    }
                    //                    select BB.*from(
                    //    select

                    //        acPlate
                    //        , iInOutStatus
                    //        , dtTrnsDate
                    //        , ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER

                    //    from LPRTRNS

                    //    where dtTrnsDate > Convert(date, '2022-06-12')

                    //    and dtTrnsDate < CONVERT(date, '2022-07-16')

                    //    and iLotArea = 37
                    //) BB
                    //WHERE REG_ORDER = 1 AND iInOutStatus = 0 order by BB.dtTrnsDate desc // 직원용
                    CLog.LOG(LOG_TYPE.DB_M, "#6 DB Select = " + sSqlCmd);

                    Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                    Cmd_Run(1, false, 2, eCmd, nReaderIdx);
                    break;
                case 7:
                    nBaseTimerChk = 0;
                    bAutoM = false;
                    //convert(nvarchar(50), dtPayDate, 126) Like '%06-03%'
                    sSqlCmd += "top 1 CarPlate, sApprovalDate, Fee, ApprovalNo, sAcquirer, sIssuers from " + pDBInfo.stTableinfo.sPayTable + " where CarPlate = '" + sCond + "' and Locate = " + pDBInfo.stTableinfo.sLocateClm + " and convert(nvarchar(50), sApprovalDate, 126) Like '%" + DateTime.Now.ToString("MM-dd") + "%' order by sApprovalDate desc;"; //iLotArea 36, 07-11 되는거 확인

                    //                    select BB.*from(
                    //    select

                    //        acPlate
                    //        , iInOutStatus
                    //        , dtTrnsDate
                    //        , ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER

                    //    from LPRTRNS

                    //    where dtTrnsDate > Convert(date, '2022-06-12')

                    //    and dtTrnsDate < CONVERT(date, '2022-07-16')

                    //    and iLotArea = 37
                    //) BB
                    //WHERE REG_ORDER = 1 AND iInOutStatus = 0 order by BB.dtTrnsDate desc // 직원용
                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);
                    Cmd_Run(1, false, 3, eCmd);
                    break;
                case 8:
                    sSqlCmd += "acPlate1, dtOutDate, acGoOutPicName, iOutEqpm from PASSTRNS where iLotArea = "+ pDBInfo.stTableinfo.sLocateClm + 
                        " and dtOutDate >= Convert(DateTime, '" + DateTime.Now.AddMinutes(-3).ToString("yyyy-MM-dd HH:mm:ss") + "') order by dtOutDate desc;";
                    Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select CarPlate Start");
                    Cmd_Run(0, true, 2, eCmd, nReaderIdx);
                    break;
                case 9:
                    string[] sReg_Date = sCond.Split('|');
                    //                    select a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acTelNo2
                    //from CUSTDEF a, GROUPDEF b
                    //where a.iGroup = b.iGroup
                    sSqlCmd += " distinct a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acMemo, a.iLotArea, a.areadefArray from CUSTDEF a, GROUPDEF b" +
                        " where a.iGroup = b.iGroup and a.iModifiedDate >= CONVERT(dateTime, '" + sReg_Date[0] + "') and a.iModifiedDate <= CONVERT(dateTime, '" + sReg_Date[1] + "') and a.del_yn = 0;";
                    Cmd[0] = new SqlCommand(sSqlCmd, conn[0]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select Reg_Manu_Down Start");
                    Cmd_Run(2, false, 0, eCmd, 0);
                    break;
                case 10:
                    //                    select a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acTelNo2
                    //from CUSTDEF a, GROUPDEF b
                    //where a.iGroup = b.iGroup
                    sSqlCmd += " distinct a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acMemo, a.iLotArea, a.areadefArray from CUSTDEF a, GROUPDEF b" +
                        " where a.iGroup = b.iGroup and a.iModifiedDate >= CONVERT(dateTime, '" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd 00:00:00") + "') and a.del_yn = 0;";
                    Cmd[0] = new SqlCommand(sSqlCmd, conn[0]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select Reg_Auto_Down Start");
                    Cmd_Run(2, false, 0, eCmd, 0);
                    break;
                case 11:
                    //                    select a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acTelNo2
                    //from CUSTDEF a, GROUPDEF b
                    //where a.iGroup = b.iGroup
                    sSqlCmd += " AC_PLATE, ENTVHCL_RESVE_DT, LVVHCL_RESVE_DT, I_LOT_AREA from RESVE_RCEPT order by CREATE_TM desc";

                    Cmd[1] = new SqlCommand(sSqlCmd, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Select Visit_Manu_Down Start");
                    Cmd_Run(2, false, 0, eCmd, 1);
                    break;
                case 12:
                    
                    //select a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acTelNo2
                    //from CUSTDEF a, GROUPDEF b
                    //where a.iGroup = b.iGroup
                    //iEqpm, acPlate, dtTrnsDate, acPicName

                    if (bPrevCar)
                        sSqlCmd += "iEqpm, acPlate, dtTrnsDate, acPicName, iInOutStatus, ROW_NUMBER() over(partition by acPlate order by dtTrnsDate DESC) as REG_ORDER from lprtrns where iInOutStatus = 0 and iLotArea = " + pDBInfo.stTableinfo.sLocateClm + " and acPicName Like '%수동%' and dtTrnsDate >= Convert(date, '" + DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd") + "')  order by dtTrnsDate desc";
                    else
                        sSqlCmd += "iInEqpm, acPlate1, dtInDate, acEntrancePicName, iInClient, iInEqpm from PASSTRNS where iLotArea = 12 and dtInDate >= Convert(DateTime, '" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "') and dtOutDate is null and dtPayDate is null and acEntrancePicName is null order by dtInDate;";
                    
                    CLog.LOG(LOG_TYPE.DB_M, "#12 DB Select = " + sSqlCmd);

                    Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                    Cmd_Run(1, false, 2, eCmd, nReaderIdx);

                    break;
                case 13:
                    //                    select a.acPlate1, a.dtValidStartDate, a.dtValidEndDate, a.acEmpNO, acGroupName1, a.acUserName, a.acTelNo2
                    //from CUSTDEF a, GROUPDEF b
                    //where a.iGroup = b.iGroup
                    sSqlCmd += " iInEqpm, acPlate1, dtInDate, acEntrancePicName, iInClient, iInEqpm from PASSTRNS where iLotArea = 12 and dtInDate >= Convert(DateTime, '" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "') and dtOutDate is null and dtPayDate is null order by dtInDate desc;";

                    CLog.LOG(LOG_TYPE.DB_M, "#13 DB Select = " + sSqlCmd);

                    Cmd[nReaderIdx] = new SqlCommand(sSqlCmd, conn[nReaderIdx]);
                    Cmd_Run(1, false, 2, eCmd, nReaderIdx);
                    break;
                default:
                    break;
            }
            if (dfSetLog != null)
                dfSetLog(sDB, sSqlCmd);

            CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " m  Select Timer Start & Query : {" + sSqlCmd + "}");
        }

        public void DB_UPDATE(int nBeh, List<string> arData, int nDiv = 0)
        {

            //UPDATE TCKTTRNS Set iPayEqpm = 2, iPaymentType = 1, iInOutStatus = 1, iOutEqpm = 2, iOutClient = 2, acPlate1 = '62고4331', acPlate2 = '62고4331',
            //dtOutDate = '2022-05-13 00:13:00', acOutTime = '00:13', iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-05-13 00:14:20', dtPaymentDate = '2022-05-13 00:13:00',
            //dFee = 750, dIncome = 750, dParkingAmount = 750, acPayTime = '00:14', dInsffcntPayOut = 0, dPaid = 0, dChange = 0, iServiceB = 0, dServiceB = 0, iServiceC = 0,
            //dServiceC = 0, iOperator = 0, iAccountFlag = 1, iPayClient = 2, iCredit = '26181333', dtMgmntDate = '2022-05-13 00:00:00', acCarStayHours = '003:06', iRate = 1,
            //acRateKeyName = '일반요금', acGoOutPicName = 'CH03_20220513001358_62고4331.jpg', iSrvrupdtFlag = 0, iVoidUseFlag = 0, dShortAmount = 0, acMemo = ''  Where iID = 190389
            TimeSpan dateDiff;
            
            char pad = '0';
            //arData.Add(json["cardMemberNo"].ToString());             //0
            //arData.Add(json["bankTransDttm"].ToString());            //1
            //arData.Add(json["cardTradeMedia"].ToString());           //2
            //arData.Add(json["cardApprovalNo"].ToString());           //3
            //arData.Add(json["cardIssueCorpNm"].ToString());          //4
            //arData.Add(json["cardNo"].ToString());                   //5
            //arData.Add(json["cardPurchCorpNm"].ToString());          //6
            //arData.Add(json["receiptTy"].ToString());                //7
            //arData.Add(json["cmmsonAmt"].ToString());                //8
            //arData.Add(json["carNo"].ToString());                    //9
            //arData.Add(json["inDttm"].ToString());                   //10
            //arData.Add(json["cardTradeNo"].ToString());              //11
            //arData.Add(json["receiptAmt"].ToString());               //12
            //arData.Add(json["receiptWay"].ToString());               //13
            //arData.Add(json["receiptWorker"].ToString());            //14
            //arData.Add(json["cardInstallmentMonth"].ToString());     //15
            // 13 할인금액
            // 14 총금액

            //arData.Add(json["extraAmt"].ToString());               //? 0
            //arData.Add(json["discountAmt"].ToString());               //? 1
            //arData.Add(json["carNo"].ToString());               //? 2
            //arData.Add(json["inDttm"].ToString());               //? 3
            //arData.Add(json["reqAmt"].ToString());               //? 4
            //arData.Add(json["outDttm"].ToString());               //? 5
            //arData.Add(json["inTy"].ToString());               //? 6


            if (Cmd[1] != null)
                Cmd[1] = null;

            eCmd = DB_CMD.UPDATE;
            switch (nBeh)
            {
                case 0:
                    try
                    {
                        //                        Update top(1) tckttrns Set iPaymentType = 1, iInOutStatus = 0,
                        //iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-08-29 14:00:00', dtPaymentDate = '2022-08-29 14:00:00', dFee = 5500,
                        //dIncome = 5500, dParkingAmount = 5500, acPayTime = '14:00', dInsffcntPayOut = 0, dPaid = 0,
                        //dChange = 0, iOperator = 0, iAccountFlag = 1, iPayClient = 7, iCredit = '',
                        //dtMgmntDate = '2022-08-29 00:00:00', acCarStayHours = '000:00', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0,
                        //dShortAmount = 0, acMemo = '' Where iLotarea = 16 and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '2022-08-29 10:58:00') and
                        //dtInDate <= Convert(dateTime, '2022-08-29 11:02:00') and acPlate1 = '12가1234')

                        dateDiff = DateTime.Parse(DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00")) - DateTime.Parse(DateTime.Parse(arData[10]).ToString("yyyy-MM-dd HH:mm:00"));

                        sSqlCmd_Update = "Update top (1) " + pDBInfo.stTableinfo.sTotalTable + " Set iPaymentType = 1," +
                          " iCardType = 0, iPaymentMode = 0, dtPayDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPaymentDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") +
                        "', dFee = " + arData[12] + ", dIncome = " + arData[12] + ", dParkingAmount = " + arData[12] + ", acPayTime = '" + arData[1].Substring(11, 5) + "', dInsffcntPayOut = 0, dPaid = 0" +
                        ", dChange = 0, iOperator = 0, iAccountFlag = 1, iPayClient = " + arData[16] + ", iPayEqpm = " + arData[16] + ", iCredit = ''" +
                        ", dtMgmntDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '" + ((int)dateDiff.TotalHours).ToString().PadLeft(3, pad) + ":" + ((int)dateDiff.Minutes).ToString().PadLeft(2, pad) + "', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                        ", dShortAmount = 0, acMemo = '' Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                        " and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '"+ DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                        " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "');"; // 아직 뭘넣을지 모르곘음 05.29
                    }
                    catch (Exception ex)
                    {
                        CLog.LOG(LOG_TYPE.ERR, "DB Exception = " + ex.ToString());
                    }
                    finally
                    {
                    }
                    break;
                case 1:
                    try
                    {
                        dateDiff = DateTime.Parse(DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00")) - DateTime.Parse(DateTime.Parse(arData[10]).ToString("yyyy-MM-dd HH:mm:00"));
                        CLog.LOG(LOG_TYPE.SCREEN, "dateDiff = " + dateDiff.ToString());
                        if (nDiv == 0) //사전 안한출차
                        {

                            //                            Update top(1) tckttrns Set iPaymentType = 1, iInOutStatus = 0,
                            //iCardType = 0, iPaymentMode = 0, dtOutDate = '2022-08-29 14:00:00', dtPayDate = '2022-08-29 14:00:00', dtPaymentDate = '2022-08-29 14:00:00', dFee = 5500,
                            //dIncome = 5500, dParkingAmount = 5500, acPayTime = '14:00', dInsffcntPayOut = 0, dPaid = 0,
                            //dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = 4, iPayClient = 4, iCredit = '',
                            //dtMgmntDate = '2022-08-29 00:00:00', acCarStayHours = '000:00', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0,
                            //dShortAmount = 0, acMemo = '' Where iLotarea = 16 and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '2022-08-29 10:58:00') and
                            //dtInDate <= Convert(dateTime, '2022-08-29 11:02:00') and acPlate1 = '12가1234')

                            sSqlCmd_Update = "Update top (1) " + pDBInfo.stTableinfo.sTotalTable + " Set iPaymentType = 1," +
                                                      " iCardType = 0, iPaymentMode = 0, dtOutDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPayDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPaymentDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") +
                                                    "', dFee = " + arData[12] + ", dIncome = " + arData[12] + ", dParkingAmount = " + arData[12] + ", acPayTime = '" + arData[1].Substring(11, 5) + "', dInsffcntPayOut = 0, dPaid = 0" +
                                                    ", dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = " + arData[16] + ", iOutEqpm = " + arData[16] + " iPayClient = " + arData[16] + ", iPayEqpm = " + arData[16] + " iCredit = ''" +
                                                    ", dtMgmntDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '" + ((int)dateDiff.TotalHours).ToString().PadLeft(3, pad) + ":" + ((int)dateDiff.Minutes).ToString().PadLeft(2, pad) + "', iRate = 1, acRateKeyName = '사전정산', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                                                    ", dShortAmount = 0, acMemo = '' Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                                                    " and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                                                    " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "');"; // 아직 뭘넣을지 모르곘음 05.29

                        }
                        else if(nDiv == 1) //사전 후 출차
                        {

                            sSqlCmd_Update = "Update top (1) " + pDBInfo.stTableinfo.sTotalTable + " Set iPaymentType = 1," +
                          " iCardType = 0, iPaymentMode = 0, dtOutDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") +
                        "', dInsffcntPayOut = 0, dPaid = 0, dFee = " + arData[12] + ", dIncome = " + arData[12] + ", dParkingAmount = " + arData[12] +
                        ", dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = " + arData[16] + ", iOutEqpm = " + arData[16] + ", iCredit = ''" +
                        ", dtMgmntDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '000:00', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                        ", dShortAmount = 0, acMemo = '' Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                        " and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                        " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "');"; // 아직 뭘넣을지 모르곘음 05.29

                        }
                        else // 사전 후 추가요금 결제 출차
                        {

                            
                            
                            sSqlCmd_Update = "Update top (1) " + pDBInfo.stTableinfo.sTotalTable + " Set iPaymentType = 1," +
                          " iCardType = 0, iPaymentMode = 0, dtOutDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPayDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPaymentDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd HH:mm:00") +
                        "', dFee += " + arData[12] +
                        ", dIncome += " + arData[12] +
                        ", dParkingAmount += " + arData[12] +
                        ", acPayTime = '" + arData[1].Substring(11, 5) + "', dInsffcntPayOut = 0, dPaid = 0" +
                        ", dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = " + arData[16] + ", iOutEqpm = " + arData[16] + ", iPayClient = CASE WHEN iPayClient = 0 Then " + arData[16] + " Else (select TOP 1 iPayClient from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "') and" + 
                        " dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "') End, " +
                        "iPayEqpm = CASE WHEN iPayEqpm = 0 Then " + arData[16] + " Else (select TOP 1 iPayEqpm from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "') and" +
                        " dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "') End, iCredit = ''" +
                        ", dtMgmntDate = '" + DateTime.Parse(arData[1]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '" + ((int)dateDiff.TotalHours).ToString().PadLeft(3, pad) + ":" + ((int)dateDiff.Minutes).ToString().PadLeft(2, pad) + "', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                        ", dShortAmount = 0, acMemo = '' Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                        " and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                        " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[10]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[9] + "');"; // 아직 뭘넣을지 모르곘음 05.29
                            //" + dateDiff.Hours.ToString("HHH") + ":" + (dateDiff.Minutes % 60).ToString("mm") + "

                        }
                    }
                    catch (Exception ex)
                    {
                        CLog.LOG(LOG_TYPE.ERR, "DB Exception = " + ex.ToString());
                    }
                    finally
                    {
                    }
                    break;
                case 2:
                    try
                    {
                        //    Update top(1) PASSTRNS Set iPayEqpm = 4, iPaymentType = 1, iInOutStatus = 1, iOutEqpm = 4, iOutClient = 4, acPlate1 = '서울83사3910',
                        //    acPlate2 = '서울83사3910', dtOutDate = '2022-08-10 03:55:07', acOutTime = '03:55', iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-08-10 03:55:07', dtPaymentDate = '2022-08-10 03:55:07',
                        //    dFee = 0, dIncome = 0, dParkingAmount = 0, acPayTime = '03:55', dInsffcntPayOut = 0, dPaid = 0,
                        //    dChange = 0, iOperator = 0, iAccountFlag = 1, iPayClient = 4, iCredit = ''
                        //    , dtMgmntDate = '2022-08-10 03:55:07', acCarStayHours = '000:00', iRate = 1, acGoOutPicName = '', iSrvrupdtFlag = 0, iVoidUseFlag = 0
                        //    , dShortAmount = 0 Where iLotarea = 4 and dtInDate > Convert(datetime, '2022-08-09' + ' 13:14:07');
                        dateDiff = DateTime.Parse(DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00")) - DateTime.Parse(DateTime.Parse(arData[3]).ToString("yyyy-MM-dd HH:mm:00"));
                        sSqlCmd_Update = "Update top (1) passtrns Set iPaymentType = 1," +
                                                  " iCardType = 0, iPaymentMode = 0, dtOutDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPayDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPaymentDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") +
                                                "', dFee = 0, dIncome = 0, dPlarkingAmount = 0, acPayTime = '" + arData[5].Substring(11, 5) + "', dInsffcntPayOut = 0, dPaid = 0" +
                                                ", dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = " + arData[9] + ", iOutEqpm = " + arData[9] + ", iPayClient = " + arData[9] + ", iPayEqpm = " + arData[9] + ", iCredit = ''" +
                                                ", dtMgmntDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '" + ((int)dateDiff.TotalHours).ToString().PadLeft(3, pad) + ":" + ((int)dateDiff.Minutes).ToString().PadLeft(2, pad) + "', iRate = 1, acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                                                ", dShortAmount = 0 Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                                                " and iID = (select TOP 1 iID from passtrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[3]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                                                " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[3]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[2] + "');";
                    }
                    catch (Exception ex)
                    {
                        CLog.LOG(LOG_TYPE.ERR, "DB Exception = " + ex.ToString());
                    }
                    finally
                    {
                    }
                    break;
                case 3:
                    try
                    {
                        //    Update top(1) PASSTRNS Set iPayEqpm = 4, iPaymentType = 1, iInOutStatus = 1, iOutEqpm = 4, iOutClient = 4, acPlate1 = '서울83사3910',
                        //    acPlate2 = '서울83사3910', dtOutDate = '2022-08-10 03:55:07', acOutTime = '03:55', iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-08-10 03:55:07', dtPaymentDate = '2022-08-10 03:55:07',
                        //    dFee = 0, dIncome = 0, dParkingAmount = 0, acPayTime = '03:55', dInsffcntPayOut = 0, dPaid = 0,
                        //    dChange = 0, iOperator = 0, iAccountFlag = 1, iPayClient = 4, iCredit = ''
                        //    , dtMgmntDate = '2022-08-10 03:55:07', acCarStayHours = '000:00', iRate = 1, acGoOutPicName = '', iSrvrupdtFlag = 0, iVoidUseFlag = 0
                        //    , dShortAmount = 0 Where iLotarea = 4 and dtInDate > Convert(datetime, '2022-08-09' + ' 13:14:07');
                        //arData.Add(json["extraAmt"].ToString());               //? 0
                        //arData.Add(json["discountAmt"].ToString());               //? 1
                        //arData.Add(json["carNo"].ToString());               //? 2
                        //arData.Add(json["inDttm"].ToString());               //? 3
                        //arData.Add(json["reqAmt"].ToString());               //? 4
                        //arData.Add(json["outDttm"].ToString());               //? 5
                        //arData.Add(json["partAmt"].ToString());               //? 6
                        //arData.Add(json["empNo"].ToString());               //? 7
                        //arData.Add(json["inTy"].ToString());               //? 8
                        //OutEqpm //9
                        dateDiff = DateTime.Parse(DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00")) - DateTime.Parse(DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00"));
                        CLog.LOG(LOG_TYPE.SCREEN, "dateDiff = " + dateDiff.ToString());

                        sSqlCmd_Update = "Update top (1) " + pDBInfo.stTableinfo.sTotalTable + " Set iPaymentType = 1," +
                                                      " iCardType = 0, iPaymentMode = 0, dtOutDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPayDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") + "', dtPaymentDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd HH:mm:00") +
                                                    "', dFee += 0, dIncome += 0, dParkingAmount += 0, acPayTime = '" + arData[5].Substring(11, 5) + "', dInsffcntPayOut = 0, dPaid = 0" +
                                                    ", dChange = 0, iOperator = 0, iAccountFlag = 1, iOutClient = " + arData[9] + ", iOutEqpm = " + arData[9] + " iPayClient = " + arData[9] + ", iPayEqpm = " + arData[9] + " iCredit = ''" +
                                                    ", dtMgmntDate = '" + DateTime.Parse(arData[5]).ToString("yyyy-MM-dd 00:00:00") + "', acCarStayHours = '"+ ((int)dateDiff.TotalHours).ToString().PadLeft(3, pad) + ":" + ((int)dateDiff.Minutes).ToString().PadLeft(2, pad) + "', iRate = 1, acRateKeyName = '일반요금', acGoOutPicName = null, iSrvrupdtFlag = 0, iVoidUseFlag = 0" +
                                                    ", dShortAmount = 0, acMemo = '' " +
                                                    "Where iLotarea = " + pDBInfo.stTableinfo.sLocateClm +
                                                    " and iID = (select TOP 1 iID from tckttrns where dtInDate >= Convert(dateTime, '" + DateTime.Parse(arData[3]).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss") + "')" +
                                                    " and dtInDate <= Convert(dateTime, '" + DateTime.Parse(arData[3]).AddMinutes(+2).ToString("yyyy-MM-dd HH:mm:ss") + "') and acPlate1 = '" + arData[2] + "');"; // 아직 뭘넣을지 모르곘음 05.29

                    }
                    catch (Exception ex)
                    {
                        CLog.LOG(LOG_TYPE.ERR, "DB Exception = " + ex.ToString());
                    }
                    finally
                    {
                    }
                    break;
            }
            try
            {
                Cmd[1] = new SqlCommand(sSqlCmd_Update, conn[1]);
            }
            catch (Exception)
            {

            }
            finally
            {
            }
            CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Update Timer Start & Query : {" + sSqlCmd_Update + "}");
            Cmd_Run(1, false, 0, eCmd, 1);

        }

        public void DB_INSERT(int nBeh, List<string> arData, bool bPass = false, int nReaderIdx = 0)
        {
            if (Cmd[1] != null)
                Cmd[1] = null;
            string sSqlCmd_Insert_InCar1 = "";
            string sSqlCmd_Insert_InCar2 = "";
            string sSqlCmd_Insert_Pay = "";
            string sSqlCmd_Insert_OutCar = "";
            //INSERT INTO CREDITINCOME(Locate, ClientNo, dtMgmtDate, CardCompCode, dtTrnsDate, FranChiseNo, RecordType, CreditCardNo,
            //OriginTrnsDate, ApprovalNo, CardType, FeeRate, CarPlate, Fee, InsMonth, sTrack, sApprovalDate, sAcquirer, sIssuers, TerminalNo)
            //VALUES(23, 2, '2022-05-13 00:14:20', 02, '2022-05-13 00:14:20', '00086841321', 11, '536510**********', '2022-05-13 00:14:20', '26181333', 0, 1,
            //'62고4331', 750, 0, '536510**********', '2022-05-13 00:14:20', 'KB카드', '카카오뱅크', '37164891') 19

            eCmd = DB_CMD.INSERT;
            switch (nBeh)
            {
                //1234567  length - 4 3 4
                case 0:
                    //(iLotArea, iInClient, iInEqpm, dtInDate, acInTime, iInOutStatus, iTicket, dtMgmntDate, acPlate1, acUserName, acEntrancePicName, iCardRate, iRate, acPlate2, iVoidUseFlag, dEventCardNo, iExtendLotArea) VALUES(64,3,3,'2022-02-18 11:41:00','11:41',0,18114123,'2022-02-18','01너9141','01너9141','CH04_20220218114123_01너9141.jpg',0,0,'',0,0,64)
                    //(iLotArea, iInClient, iInEqpm, dtInDate, acInTime, iInOutStatus, iTicket, dtMgmntDate, acPlate1, acUserName, acTelNo, acEntrancePicName, iCardRate, iRate, iExtendLotArea, iCardType) VALUES(3,8,8,'2022-07-20 17:40:00','17:40',0,20174021,'2022-07-20','38라7778','7778','20220720','CH20_20220720174021_38라7778.jpg',0,1,3,0)
                    try
                    {
                        if (bPass)
                        {
                            sSqlCmd_Insert_InCar1 = "INSERT INTO PassTrns (iLotArea, iInClient, acPlate1, dtInDate, dtMgmntDate, iGroup, acUserName, acInTime, iCardType, iInOutStatus, iInEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", " + arData[8] + ", '" +
                            arData[2] + "', '" + arData[3] + "', '" + DateTime.Now.ToString("yyyy-MM-dd ") + "00:00:00" + "', 1, '정기무료', '" + arData[3].Substring(11, 5) + "', 1, 0, " + arData[8] + ")";
//                            sSqlCmd_Insert_InCar1 = "INSERT INTO PassTrns (iLotArea, iInClient, acPlate1, dtInDate, dtMgmntDate, iGroup, acUserName, acInTime, iCardType, iInOutStatus, iInEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", 1, '" +
//arData[2] + "', '" + arData[3] + "', '" + DateTime.Now.ToString("yyyy-MM-dd ") + "00:00:00" + "', 1, '정기무료', '" + arData[3].Substring(11, 5) + "', 1, 0, 1)";
                        }
                        else
                        {
                            CLog.LOG(LOG_TYPE.SCREEN, "arData[2]" + arData[2] + "arData[3] = " + arData[3]);

                            sSqlCmd_Insert_InCar1 = "INSERT INTO " + pDBInfo.stTableinfo.sTotalTable + " (iLotArea, iInClient, iInEqpm, iPaymentType, sTax, dtInDate, acInTime, iInOutStatus, iTicket, dtMgmntDate, acPlate1, acUserName, acTelNo, acEntrancePicName, iCardRate, iRate, iExtendLotArea, iCardType) VALUES(" +
    pDBInfo.stTableinfo.sLocateClm + ", " + arData[8] + ", " + arData[8] + ", 0, null, '" + arData[3] + "', '" + arData[3].Substring(11, 5) + "', 0, " + ((((arData[3].Replace(":", "")).Replace("-", "")).Substring(6, 2)) + (((arData[3].Replace(":", "")).Replace("-", "")).Substring(9, 4))) + DateTime.Now.ToString("ss") +
    ", '" + arData[3].Substring(0, 10) + "', '" + arData[2] + "', '" + arData[2].Substring(arData[2].Length - 4) + "', '" + (arData[3].Replace("-", "")).Substring(0, 8) + "', '', 0, 1, " + pDBInfo.stTableinfo.sLocateClm + ", 0)";
   //                         sSqlCmd_Insert_InCar1 = "INSERT INTO " + pDBInfo.stTableinfo.sTotalTable + " (iLotArea, iInClient, iInEqpm, iPaymentType, sTax, dtInDate, acInTime, iInOutStatus, iTicket, dtMgmntDate, acPlate1, acUserName, acTelNo, acEntrancePicName, iCardRate, iRate, iExtendLotArea, iCardType) VALUES(" +
   //pDBInfo.stTableinfo.sLocateClm + ", 1, 1, 1, 0, '" + arData[3] + "', '" + arData[3].Substring(11, 5) + "', 0, " + ((((arData[3].Replace(":", "")).Replace("-", "")).Substring(6, 2)) + (((arData[3].Replace(":", "")).Replace("-", "")).Substring(9, 4))) + DateTime.Now.ToString("ss") +
   //", '" + arData[3].Substring(0, 10) + "', '" + arData[2] + "', '" + arData[2].Substring(arData[2].Length - 4) + "', '" + (arData[3].Replace("-", "")).Substring(0, 8) + "', '', 0, 1, " + pDBInfo.stTableinfo.sLocateClm + ", 1)";
                        }

                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_InCar1, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_InCar1 + "}");
                    Cmd_Run(1, false, 0, eCmd, 1);
                    break;
                case 1:
                    sSqlCmd_Insert_Pay = "INSERT INTO " + pDBInfo.stTableinfo.sPayTable + "(Locate, ClientNo, dtMgmtDate, CardCompCode, dtTrnsDate, FranChiseNo, RecordType, CreditCardNo" +
    ", OriginTrnsDate, ApprovalNo, CardType, FeeRate, CarPlate, Fee, InsMonth, sTrack, sApprovalDate, sAcquirer, sIssuers, TerminalNo) VALUES(" + pDBInfo.stTableinfo.sLocateClm +
    ", 2, '" + arData[1] + "', 02, '" + arData[1] + "', '" + pDBInfo.stTableinfo.sFranClm + "', 11, '" + arData[5] + "', '" + arData[1] + "', '" + arData[3] + "', 0, 1, '" +
    arData[9] + "', " + arData[11] + ", 0, '" + arData[5] + "', '" + arData[1] + "', '" + arData[6] + "', '" + arData[4] + "', '')";
                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_Pay, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_Pay + "}");
                    Cmd_Run(1, false, 0, eCmd);
                    break;
                case 2:
                    //INSERT INTO LPRTRNS(iLotArea, acPlate, dtTrnsDate, iInOutStatus, iCardType, acPicName, iRecognitionFlg, iEqpm) Values(16, '09루7548', '2022-07-25 06:43:38', 0, 1, 'CH70_20220725064337_09루7548.jpg', 0, 1)
                    sSqlCmd_Insert_InCar2 = "INSERT INTO LPRTRNS (iLotArea, acPlate, dtTrnsDate, iInOutStatus, iCardType, acPicName, iRecognitionFlg, iEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", '" +
                    arData[2] + "', '" + arData[3] + "', 0, 1, '',0, " + arData[8] + ")";
                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_InCar2, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_InCar2 + "}");
                    Cmd_Run(1, false, 0, eCmd, 1);
                    break;
                case 3:
                    try
                    {
                        //UPDATE TCKTTRNS Set iPaymentType = 1, iInOutStatus = 1,
                        //iOutEqpm = 3, iOutClient = 3, acPlate1 = '32부4767', acPlate2 = '32부4767',
                        //dtOutDate = '2022-07-26 05:03:00', acOutTime = '05:03', iPayEqpm = 3, iPayClient = 3,
                        //iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-07-26 05:03:01',
                        //dtPaymentDate = '2022-07-26 05:03:01', dFee = 0, dIncome = 0,
                        //dParkingAmount = 0, acPayTime = '05:03', dInsffcntPayOut = 0,
                        //dPaid = 0, dChange = 0, iServiceB = 0, dServiceB = 0, iServiceC = 0,
                        //dServiceC = 0, iOperator = 0, iAccountFlag = 0, dtMgmntDate = '2022-07-26 00:00:00',
                        //acCarStayHours = '000:03', iRate = 1, acRateKeyName = '일반', acGoOutPicName = 'CH74_20220726050301_32부4767.jpg',
                        //iSrvrupdtFlag = 0, iVoidUseFlag = 0, dShortAmount = 0, dMisc2 = 0, acMemo = '회차차량', acTelNo = ''  Where iID = 21746834

                        if (bPass)
                        {
                            sSqlCmd_Insert_OutCar = "INSERT INTO PASSTRNS (iLotArea, acPlate1, dtOutDate, iInOutStatus, iCardType, acGoOutPicName, dParkingAmount, iOutEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", '" +
arData[2] + "', '" + arData[5] + "', 1, 1, '',0, 4)";
                        }
                        else
                        {
                            sSqlCmd_Insert_OutCar = "INSERT INTO LPRTRNS (iLotArea, acPlate, dtTrnsDate, iInOutStatus, iCardType, acPicName, iRecognitionFlg, iEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", '" +
                                arData[2] + "', '" + arData[5] + "', 1, 1, '',0, 4)";
                        }

                        Cmd[1] = new SqlCommand(sSqlCmd_Insert_OutCar, conn[1]);
                        CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_OutCar + "}");
                        Cmd_Run(1, false, 0, eCmd);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_OutCar, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_OutCar + "}");
                    Cmd_Run(1, false, 0, eCmd);
                    break;
                case 4:
                    try
                    {
                        //UPDATE TCKTTRNS Set iPaymentType = 1, iInOutStatus = 1,
                        //iOutEqpm = 3, iOutClient = 3, acPlate1 = '32부4767', acPlate2 = '32부4767',
                        //dtOutDate = '2022-07-26 05:03:00', acOutTime = '05:03', iPayEqpm = 3, iPayClient = 3,
                        //iCardType = 0, iPaymentMode = 0, dtPayDate = '2022-07-26 05:03:01',
                        //dtPaymentDate = '2022-07-26 05:03:01', dFee = 0, dIncome = 0,
                        //dParkingAmount = 0, acPayTime = '05:03', dInsffcntPayOut = 0,
                        //dPaid = 0, dChange = 0, iServiceB = 0, dServiceB = 0, iServiceC = 0,
                        //dServiceC = 0, iOperator = 0, iAccountFlag = 0, dtMgmntDate = '2022-07-26 00:00:00',
                        //acCarStayHours = '000:03', iRate = 1, acRateKeyName = '일반', acGoOutPicName = 'CH74_20220726050301_32부4767.jpg',
                        //iSrvrupdtFlag = 0, iVoidUseFlag = 0, dShortAmount = 0, dMisc2 = 0, acMemo = '회차차량', acTelNo = ''  Where iID = 21746834


                        sSqlCmd_Insert_OutCar = "INSERT INTO PASSTRNS (iLotArea, acPlate1, dtInDate, iInOutStatus, iCardType, acGoOutPicName, dParkingAmount, iOutEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", '" +
arData[2] + "', '" + arData[5] + "', 0, 1, '',0, 4)";
                        Cmd[1] = new SqlCommand(sSqlCmd_Insert_OutCar, conn[1]);
                        CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_OutCar + "}");
                        Cmd_Run(1, false, 0, eCmd);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_OutCar, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_OutCar + "}");
                    Cmd_Run(1, false, 0, eCmd);
                    break;
                case 5:
                    try
                    {
                        //arData.Add(json["extraAmt"].ToString());               //? 0
                        //arData.Add(json["discountAmt"].ToString());               //? 1
                        //arData.Add(json["carNo"].ToString());               //? 2
                        //arData.Add(json["inDttm"].ToString());               //? 3
                        //arData.Add(json["reqAmt"].ToString());               //? 4
                        //arData.Add(json["outDttm"].ToString());               //? 5
                        //arData.Add(json["partAmt"].ToString());               //? 6
                        //arData.Add(json["empNo"].ToString());               //? 7
                        //arData.Add(json["inTy"].ToString());               //? 8

                        sSqlCmd_Insert_InCar2 = "INSERT INTO LPRTRNS (iLotArea, acPlate, dtTrnsDate, iInOutStatus, iCardType, acPicName, iRecognitionFlg, iEqpm) VALUES(" + pDBInfo.stTableinfo.sLocateClm + ", '" +
                        arData[2] + "', '" + arData[5] + "', 1, 1, '',0, " + arData[9] + ")";

                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    Cmd[1] = new SqlCommand(sSqlCmd_Insert_InCar1, conn[1]);
                    CLog.LOG(LOG_TYPE.DB_M, "#" + nBeh + " DB Insert Timer Start & Query : {" + sSqlCmd_Insert_InCar1 + "}");
                    Cmd_Run(1, false, 0, eCmd, 1);
                    break;
                default:
                    break;
            }

        }

        //Cmd = 1 S, 2 = U, 3 = I
        public bool Cmd_Run(int nIdx, bool bPrev = false, int nPrevIdx = 0, DB_CMD dCMD = DB_CMD.SELECT, int nReaderIdx = 0)
        {
            string sID = "";
            string sCarno = "";
            string sStartDate = "";
            string sEndDate = "";
            string sUserName = "";
            string sPayDate = "";
            string sCredit = "";
            int nFee = 0;
            float fFee = 0;
            int nCnt = 1;
            bool[] bSend = new bool[10];
            bSend[nReaderIdx] = new bool();

            

            CLog.LOG(LOG_TYPE.DB_M, "#" + nReaderIdx.ToString() + " DB Cmd_Run #1");

            //Cmd[nIdx].ExecuteReader();
            try
            {
                //if (nIdx == 0 && CData.ucReg != null)
                //{
                //    CData.ucReg.Lsv_Clear();
                //}

                try
                {
                    if (reader[nReaderIdx] != null)
                    {

                        reader[nReaderIdx].Close();
                        reader[nReaderIdx].Dispose();
                        reader[nReaderIdx] = null;
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                }
                try
                {
                    //reader[nIdx] = new SqlDataReader;
                    reader[nReaderIdx] = Cmd[nReaderIdx].ExecuteReader();
                }
                catch (Exception)
                {

                }
                finally
                {
                }
                if (dCMD == DB_CMD.SELECT)
                {
                    if (!bPrev)
                    {
                        switch (nIdx)
                        {
                            case 0:
                                while (reader[0].Read())
                                {
                                    try
                                    {
                                        //sSqlCmd += "acPlate1, dtValidStartDate, dtValidEndDate, acUserName from " + pDBInfo.stTableinfo.sRegTable + ";";
                                        sCarno = reader[0].GetString(0);

                                    }
                                    catch (Exception)
                                    {
                                        sCarno = "";
                                    }
                                    finally
                                    {
                                    }

                                    try
                                    {
                                        sStartDate = reader[0].GetString(1);
                                    }
                                    catch (Exception)
                                    {
                                        sStartDate = "";
                                    }
                                    finally
                                    {
                                    }
                                    try
                                    {
                                        sEndDate = reader[0].GetString(2);
                                    }
                                    catch (Exception)
                                    {
                                        sEndDate = "";
                                    }
                                    finally
                                    {
                                    }
                                    try
                                    {
                                        sUserName = reader[0].GetString(3);
                                    }
                                    catch (Exception)
                                    {
                                        sUserName = "";
                                    }
                                    finally
                                    {
                                    }
                                    if (CData.ucReg != null)
                                    {
                                        CData.ucReg.Lsv_Show(nCnt, sCarno, sStartDate, sEndDate, sUserName);
                                        nCnt++;
                                    }

                                    //if (dfReg != null)
                                    //{
                                    //    dfReg(nIdx, sCarno, sStartDate, sEndDate, sUserName);
                                    //    CLog.LOG(LOG_TYPE.DB, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfReg Send");
                                    //}

                                }
                                bSend[nIdx] = true;
                                if (dfSetLog != null)
                                    dfSetLog(sDB, "Select Reg Success :{Cnt = " + (nCnt - 1).ToString() + "}");
                                break;
                            case 1:
                                if (nPrevIdx == 0)
                                {
                                    string sDttm = "";
                                    string sOutDttm = "";
                                    while (reader[1].Read())
                                    {
                                        sCarno = reader[1].GetString(0);
                                        nFee = reader[1].GetInt32(1);
                                        try
                                        {
                                            sCredit = reader[1].GetString(2);
                                        }
                                        catch (Exception)
                                        {
                                            sCredit = "";
                                        }
                                        finally
                                        {
                                        }
                                        try
                                        {
                                            sDttm = reader[1].GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        catch (Exception)
                                        {
                                            //CLog.LOG(LOG_TYPE.ERR, "sDttm Err : " + ex.ToString());
                                            sDttm = "T";
                                        }
                                        finally
                                        {
                                        }
                                        try
                                        {
                                            sOutDttm = reader[1].GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss");
                                            //reader[1].GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        catch (Exception)
                                        {
                                            sOutDttm = "T";
                                        }
                                        finally
                                        {
                                        }

                                        //if(sOutDttm == "T" && sDttm)
                                        if (sDttm == "T")
                                            return false;

                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run sCarno=" + sCarno + "nFee=" + nFee.ToString());
                                        //if (nFee == 0)
                                        //{
                                        if (dfCal != null)
                                        {
                                            bSend[nIdx] = true;
                                            dfCal(sCarno, nFee, sOutDttm, sDttm, sCredit);
                                            bAutoM = false;
                                            CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #1 dfCal Send");
                                        }
                                        //}
                                        //else
                                        //{
                                        //    if (dfReCal != null)
                                        //    {
                                        //        bSend[nIdx] = true;

                                        //        DB_SELECT(6, sCarno, false);
                                        //        bAutoM = false;
                                        //        CLog.LOG(LOG_TYPE.DB, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfReCal Send");
                                        //    }
                                        //}


                                    }
                                    if (dfSetLog != null)
                                        dfSetLog(sDB, "Select Pay Success :{" + "sCarno = " + sCarno + "&nFee = " + nFee.ToString() + "}");
                                }
                                else if (nPrevIdx == 1)
                                {
                                    while (reader[1].Read())
                                    {
                                        int nID = 0;
                                        string sCarno_Out = "";
                                        try
                                        {
                                            nID = reader[1].GetInt32(0);
                                        }
                                        catch (Exception)
                                        {
                                            nID = 0;
                                        }
                                        finally
                                        {
                                        }
                                        try
                                        {
                                            sCarno_Out = reader[1].GetString(1);
                                        }
                                        catch (Exception)
                                        {
                                            sCarno_Out = "";
                                        }
                                        finally
                                        {
                                        }
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run ID=" + sID + " Carno=" + sCarno_Out);
                                        bSend[nIdx] = true;
                                        bAutoM = false;
                                        //CData.sID = sID;
                                        if (dfCarOutID != null)
                                            dfCarOutID(nID, sCarno_Out);


                                    }
                                    if (dfSetLog != null)
                                        dfSetLog(sDB, "Select ID Success :{" + "sID = " + sID + "}");
                                }
                                else if (nPrevIdx == 2)
                                {
                                    int nInEqpm = 0;
                                    string sInCarno = "";
                                    string sInDttm = "";
                                    string sImgPath = "";
                                    int nTemp1 = 0;
                                    int nTemp2 = 0;
                                    int nInCnt = 0;
                                    while (reader[nReaderIdx].Read())
                                    {
                                        try
                                        {
                                            nInEqpm = reader[nReaderIdx].GetInt16(0);

                                        }
                                        catch (Exception)
                                        {
                                            nInEqpm = 0;
                                        }
                                        finally
                                        {
                                        }
                                        try
                                        {
                                            nInCnt++;
                                            sInCarno = reader[nReaderIdx].GetString(1);
                                            
                                        }
                                        catch (Exception)
                                        {
                                            sInCarno = "No_Detection";
                                            nInCnt--;
                                        }
                                        finally
                                        {
                                        }

                                        try
                                        {
                                            //sInDttm = reader[1].GetString(2);
                                            sInDttm = reader[nReaderIdx].GetDateTime(2).ToString("yyyy-MM-dd HH:mm:00");
                                            nInCnt++;
                                        }
                                        catch (Exception)
                                        {
                                            sInDttm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:00");
                                            nInCnt--;
                                        }
                                        finally
                                        {
                                        }
                                        try
                                        {
                                            sImgPath = reader[nReaderIdx].GetString(3);
                                            nInCnt++;
                                        }
                                        catch (Exception)
                                        {
                                            sImgPath = "";
                                            nInCnt--;
                                        }
                                        finally
                                        {
                                        }
                                        //try
                                        //{
                                        //    nTemp1 = reader[nReaderIdx].GetInt32(4);
                                        //    nInCnt++;
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    nTemp1 = 0;
                                        //    nInCnt--;
                                        //}
                                        //try
                                        //{
                                        //    nTemp2 = reader[nReaderIdx].GetInt32(5);
                                        //    nInCnt++;
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    nTemp2 = 0;

                                        //}


                                        if (dfMigration != null)
                                        {
                                            bSend[nIdx] = true;
                                            dfMigration(nInEqpm, sInCarno, sInDttm, sImgPath);
                                            bAutoM = false;
                                            CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run { nInEqpm=" + nInEqpm.ToString() + " &sInCarno=" + sInCarno + "&sInDttm=" + sInDttm + "&sImgPath=" + sImgPath + "}");
                                        }

                                    }
                                    if (dfSetLog != null)
                                        dfSetLog(sDB, "Select Migration Success : { nInEqpm=" + nInEqpm.ToString() + " &sInCarno=" + sInCarno + "&sInDttm=" + sInDttm + "&sImgPath=" + sImgPath + "}");
                                }
                                else if (nPrevIdx == 3)
                                {
                                    while (reader[1].Read())
                                    {
                                        sCarno = reader[1].GetString(0);
                                        nFee = reader[1].GetInt32(1);
                                        try
                                        {
                                            sCredit = reader[1].GetString(2);
                                        }
                                        catch (Exception)
                                        {
                                            sCredit = "";
                                        }
                                        finally
                                        {
                                        }
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run sCarno=" + sCarno + "nFee=" + nFee.ToString());

                                        if (dfReCal != null)
                                        {
                                            bSend[nIdx] = true;

                                            DB_SELECT(6, sCarno, false);
                                            bAutoM = false;
                                            CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfReCal Send");
                                        }



                                    }
                                    if (dfSetLog != null)
                                        dfSetLog(sDB, "Select Pay Success :{" + "sCarno = " + sCarno + "&nFee = " + nFee.ToString() + "}");

                                }
                                break;
                            case 2:


                                while (reader[0].Read())
                                {
                                    string sCarno_Reg = "";
                                    string sStartDttm = "";
                                    string sEndDttm = "";
                                    string sRegDiv = "";
                                    string sGroupNm = "";
                                    string sUserNm = "";
                                    string sMemo = "";
                                    string sLotArea = "";
                                    string sAreaArray = "";
                                    int nRegCnt = 0;
                                    Application.DoEvents();

                                    try
                                    {
                                        //sSqlCmd += "acPlate1, dtValidStartDate, dtValidEndDate, acUserName from " + pDBInfo.stTableinfo.sRegTable + ";";
                                        sCarno_Reg = reader[0].GetString(nRegCnt);
                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sCarno_Reg = "";
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sStartDttm = reader[0].GetString(nRegCnt);

                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sStartDttm = "";
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sEndDttm = reader[0].GetString(nRegCnt);

                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sEndDttm = "";
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sRegDiv = reader[0].GetString(nRegCnt);
                                    }
                                    catch (Exception)
                                    {
                                        sRegDiv = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sGroupNm = reader[0].GetString(4);
                                    }
                                    catch (Exception)
                                    {
                                        sGroupNm = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sUserNm = reader[0].GetString(5);
                                    }
                                    catch (Exception)
                                    {
                                        sUserNm = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sMemo = reader[0].GetString(6);
                                    }
                                    catch (Exception)
                                    {
                                        sMemo = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {

                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        //if(sTelno == "")
                                        sLotArea = reader[0].GetInt32(7).ToString();
                                    }
                                    catch (Exception)
                                    {
                                        sLotArea = "1";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sAreaArray = reader[0].GetString(8);
                                    }
                                    catch (Exception)
                                    {
                                        sAreaArray = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    if (dfReg != null)
                                    {
                                        dfReg(sCarno_Reg, sStartDttm, sEndDttm, sRegDiv, sGroupNm, sUserNm, sMemo, sLotArea, sAreaArray);
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfReg Send");
                                    }
                                }

                                //bSend[nIdx] = true;
                                if (dfSetLog != null)
                                    dfSetLog(sDB, "Select Reg Success :{Cnt = " + (nCnt - 1).ToString() + "}");

                                if (dfRegUp != null)
                                    dfRegUp();
                                break;
                            case 3:


                                while (reader[0].Read())
                                {
                                    string sCarno_Visit = "";
                                    string sStartDttm_Visit = "";
                                    string sEndDttm = "";
                                    string sLotArea = "";
                                    int nRegCnt = 0;
                                    Application.DoEvents();

                                    try
                                    {
                                        //sSqlCmd += "acPlate1, dtValidStartDate, dtValidEndDate, acUserName from " + pDBInfo.stTableinfo.sRegTable + ";";
                                        sCarno_Visit = reader[0].GetString(nRegCnt);
                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sCarno_Visit = "";
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sStartDttm_Visit = reader[0].GetString(nRegCnt);

                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sStartDttm_Visit = "";
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sEndDttm = reader[0].GetString(nRegCnt);

                                    }
                                    catch (Exception)
                                    {
                                        nRegCnt--;
                                        sEndDttm = "";
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        //if(sTelno == "")
                                        sLotArea = reader[0].GetInt32(7).ToString();
                                    }
                                    catch (Exception)
                                    {
                                        sLotArea = "1";
                                        nRegCnt--;
                                    }
                                    
                                    if (dfVisit != null)
                                    {
                                        dfVisit(sCarno_Visit, sStartDttm_Visit, sEndDttm, sLotArea);
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfReg Send");
                                    }
                                }

                                //bSend[nIdx] = true;
                                if (dfSetLog != null)
                                    dfSetLog(sDB, "Select Reg Success :{Cnt = " + (nCnt - 1).ToString() + "}");

                                if (dfRegUp != null)
                                    dfRegUp();
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (nPrevIdx)
                        {
                            case 0:

                                while (reader[nReaderIdx].Read())
                                {
                                    string sCarno_Prev = "";
                                    int nFee_Prev = 0;
                                    int nService = 0;
                                    string sApproval_Prev = "";
                                    string sDttm = "";
                                    int nID = 0;
                                    string sFile = "";
                                    string sOutEqpm = "";
                                    int nRegCnt = 0;
                                    try
                                    {
                                        sCarno = reader[nReaderIdx].GetString(0);
                                    }
                                    catch (Exception)
                                    {
                                        sCarno = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;
                                    //if (sCarno == sNowCarno)
                                    //{
                                    //    bNext = true;
                                    //    break;
                                    //}

                                    try
                                    {
                                        nFee_Prev = (int)reader[nReaderIdx].GetInt32(1);
                                    }
                                    catch (Exception ex)
                                    {
                                        nFee_Prev = 0;
                                        CLog.LOG(LOG_TYPE.ERR, "#Prev Err=" + ex.ToString());
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;



                                    nRegCnt++;
                                    try
                                    {
                                        sDttm = reader[nReaderIdx].GetDateTime(2).ToString("yyyy-MM-dd HH:mm:00");
                                       
                                    }
                                    catch (Exception)
                                    {
                                        sDttm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:00");
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }
                                    nRegCnt++;

                                    try
                                    {
                                        sApproval_Prev = reader[nReaderIdx].GetString(3);
                                    }
                                    catch (Exception)
                                    {
                                        sApproval_Prev = "";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }

                                    nRegCnt++;
                                    if (CData.garOpt1[1] == 0)
                                    {
                                        try
                                        {
                                            sFile = reader[nReaderIdx].GetString(4);
                                        }
                                        catch (Exception)
                                        {
                                            sFile = "";
                                            nRegCnt--;
                                        }
                                        finally
                                        {
                                        }

                                    }
                                    else
                                    {
                                        try
                                        {
                                            nID = (int)reader[nReaderIdx].GetInt64(4);
                                        }
                                        catch (Exception)
                                        {
                                            nID = 0;
                                            nRegCnt--;
                                        }
                                        finally
                                        {
                                        }
                                    }
                                    nRegCnt++;
                                    try
                                    {
                                        sOutEqpm = reader[nReaderIdx].GetInt16(5).ToString();
                                    }
                                    catch (Exception)
                                    {
                                        sOutEqpm = "1";
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }

                                    nRegCnt++;

                                    //if (CData.garOpt1[1] == 1)
                                    //{
                                    try
                                    {
                                        nService = (int)reader[nReaderIdx].GetInt32(6);
                                    }
                                    catch (Exception)
                                    {
                                        nService = 0;
                                        nRegCnt--;
                                    }
                                    finally
                                    {
                                    }

                                    //}


                                    bNext = false;
                                    sNowCarno = sCarno;
                                    CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run sCarno=" + sCarno + "&nFee=" + nFee_Prev.ToString() + "&Approval=" + sApproval_Prev + "&dttm=" + sDttm + "&ID=" + nID + "&File=" + sFile + "&OutEqpm=" + sOutEqpm + "&Service=" + nService.ToString());

                                    if (dfCarOut != null)
                                    {
                                        bSend[nIdx] = true;
                                        //if (CData.garOpt1[3] == 1)
                                        dfCarOut(sCarno, nFee_Prev, sApproval_Prev, sDttm, nID, sFile, sOutEqpm, nService);
                                        //else
                                        //    dfCarOut(sCarno, nFee_Prev, sApproval_Prev, sDttm, nID, "");
                                        bAutoM = false;
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfCal Send");
                                    }

                                }

                                if (dfSetLog != null)
                                    dfSetLog(sDB, "Select Pay Success :{" + "sCarno = " + sCarno + "}");

                                break;
                            case 1:
                                while (reader[1].Read())
                                {
                                    sCarno = reader[1].GetString(0);
                                    //fFee = reader[1].GetDouble(1);
                                    try
                                    {
                                        nFee = (int)reader[1].GetInt32(1);
                                    }
                                    catch (Exception)
                                    {
                                        nFee = 0;
                                    }
                                    finally
                                    {
                                    }

                                    sPayDate = reader[1].GetDateTime(2).ToString();

                                    try
                                    {
                                        sCredit = reader[1].GetString(3);
                                    }
                                    catch (Exception)
                                    {
                                        sCredit = "";
                                    }
                                    finally
                                    {
                                    }
                                    CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run sCarno=" + sCarno + "nFee=" + nFee.ToString() + " sPaydt=" + sPayDate + " sApproval=" + sCredit);
                                    //"CarPlate, dIncome, dtPayDate, iCredit
                                    if (dfCal != null)
                                    {
                                        bSend[nIdx] = true;
                                        dfCal(sCarno, nFee, sPayDate, sCredit);
                                        bNext = false;
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfCal Send");
                                    }

                                }

                                if (dfSetLog != null)
                                    dfSetLog(sDB, "Select Pay Success :{" + "sCarno = " + sCarno + "&nFee = " + nFee.ToString() + "}");
                                break;
                            case 2:
                                string sCarno_Pass = "";
                                string sOutDttm = "";
                                string sPic = "";
                                string sOutEqpm_Pass = "";
                                while (reader[nReaderIdx].Read())
                                {

                                    try
                                    {
                                        sCarno_Pass = (string)reader[nReaderIdx].GetValue(0);
                                        //fFee = reader[1].GetDouble(1);
                                    }
                                    catch (Exception)
                                    {
                                        sCarno_Pass = "";
                                    }
                                    finally
                                    {
                                    }

                                    try
                                    {
                                        sOutDttm = reader[nReaderIdx].GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    catch (Exception)
                                    {
                                        sOutDttm = "";
                                    }
                                    finally
                                    {
                                    }

                                    try
                                    {
                                        sPic = reader[nReaderIdx].GetString(2);
                                    }
                                    catch (Exception)
                                    {
                                        sPic = "";
                                    }
                                    finally
                                    {
                                    }

                                    try
                                    {
                                        sOutEqpm_Pass = reader[nReaderIdx].GetInt16(3).ToString();
                                    }
                                    catch (Exception)
                                    {
                                        sOutEqpm_Pass = "1";
                                    }
                                    finally
                                    {
                                    }

                                    CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run sCarno=" + sCarno_Pass + "&OutDttm=" + sOutDttm + "&Pic=" + sPic + "&OutEqpm=" + sOutEqpm_Pass);
                                    //"CarPlate, dIncome, dtPayDate, iCredit
                                    if (dfPassTrns != null)
                                    {
                                        bSend[nIdx] = true;
                                        dfPassTrns(sCarno_Pass, sOutDttm, sPic, sOutEqpm_Pass);
                                        CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB Cmd_Run #2 dfPassTrns");
                                    }

                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                else if (dCMD == DB_CMD.UPDATE)
                {
                    CData.sID = "";
                    if (dfSetLog != null)
                        dfSetLog(sDB, "Select Update Success");

                }
                else
                {
                    if (dfSetLog != null)
                        dfSetLog(sDB, "Select Insert Success}");

                }
                CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + " DB #3 reader Close");
                //reader[nIdx].Close();
                return bSend[nIdx];
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.DB_M, "#" + nIdx.ToString() + "Cmd_Run Error : {" + ex.ToString() + "}");
                if (nIdx == 0 || nIdx == 1)
                {

                        Close(nIdx);

                    pDBInfo.bStatus[0] = false;
                }
               
                if (dfSetLog != null)
                    dfSetLog(sDB, "Cmd_Run Error : {" + ex.ToString() + "}");

                bDB_Cmd_Run_Stat = false;

                return false;
            }
            finally
            {
            }

            return true;
        }

        /// <summary>

        /// Database 해제

        /// </summary>

        public void Close(int nIdx)

        {
            try
            {
                if (IsDBConnected(nIdx))
                {
                    conn[nIdx].Dispose();
                    conn[nIdx].Close();
                    conn[nIdx] = null;
                    pDBInfo.bStatus[0] = false;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }

        }
    }
}

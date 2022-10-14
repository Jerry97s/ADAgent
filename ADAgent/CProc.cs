using ADAgent.DATA;
using ADAgent.NET;
using ADAgent.TPMS;
using ADAgent.UTIL;
using iNervMng.TPMS;
using LPR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ADAgent
{
    class CProc
    {
        public CTPMS pTPMS = null;
        public CLPR pLPR = null;

        public CStation[] pStation = null;
        public CStation[] pStation_GT = null;
        int nServerPing = 0;
        public CMSSQL pMSSQL = null;
        public CWS pWS = null;
        int[] nReTokenCnt = new int[10];
        int[] nReLoginCnt = new int[10];
        int[] nReHealthCnt = new int[10];
        CDB pSDB = null;
        int nNowIdx = 0;
        int nNet_Proc = 0;
        int nStackCnt = 0;
        public static string[] sTPMS_Use = new string[10];
        string sFull_Proc = "";
        public int nNetLPRCnt = 0;
        public int nTPMSCnt = 0;
        //public int nNetGTCnt = 0;
        //int nReTokenCnt = 0;
        //int nReWsLoginCnt = 0;
        string sPrevCarno = "";
        bool bRegDown = false;
        int nManuTrns = 0;
        int nNowHour = 999;
        System.Timers.Timer pTPMSTimer = new System.Timers.Timer();

        System.Timers.Timer pWSTimer = new System.Timers.Timer();

        System.Timers.Timer pDBTimer = new System.Timers.Timer();

        System.Timers.Timer pSDBTimer = new System.Timers.Timer();

        System.Timers.Timer pOraDBTimer = new System.Timers.Timer();

        Stopwatch pSw = new Stopwatch();
        Stopwatch pSw_DB = new Stopwatch();
        public LPRInfo st_LprInfo;
        public TpmsInfo st_TpmsInfo_Proc;
        public NetInfo[] st_NetInfo_LPR_Proc;
        public NetInfo[] st_NetInfo_GT_Proc;
        public WSInfo st_WsInfo_Proc;

        public delegate void DF_All_Send_Stat(int nDiv, bool bChk);
        public DF_All_Send_Stat dfAllSendStat = null;

        public delegate void DF_LPR_Stat(int nIdx_Div, int nIdx, bool bStat, string sIOType);
        public DF_LPR_Stat dfLPRStat = null;

        public delegate void DF_TPMS_Proc(string sID, string sLog);
        public DF_TPMS_Proc dfTPMSProc = null;

        public delegate void DF_DB_Proc(string sDB, string sLog);
        public DF_DB_Proc dfDBProc = null;

        public delegate void DF_ODB_Proc(string sDB, string sLog);
        public DF_ODB_Proc dfODBProc = null;

        public delegate void DF_LPR_Proc(string sID, string sLog);
        public DF_LPR_Proc dfLPRProc = null;

        public delegate void DF_GT_Proc(string sMatch, bool bIO, string sLog);
        public DF_GT_Proc dfGTProc = null;


        public delegate void DF_Stack_Cnt();
        public DF_Stack_Cnt dfStackCnt = null;

        public delegate void DF_Proc_WS(string sID, bool bIO, string sCmd);
        public DF_Proc_WS dfProcWs = null;

        public delegate void DF_Proc_Mig(int nEqpm, string sCarno, string sInDttm, string sPath);
        public DF_Proc_Mig dfProcMig = null;

        public delegate void DF_Proc_Mig_Out(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sOutEqpm, int nService);
        public DF_Proc_Mig_Out dfProcMigOut = null;


        public delegate void DF_Proc_Mig_Pass(string sCarno, string sDttm, string sFile, string sOutEqpm);
        public DF_Proc_Mig_Pass dfProcMigPass = null;

        public delegate void DF_Send_Reg(string sCarno_Reg, string sStartDttm, string sEndDttm, string sRegDiv, string sGroupNm, string sUserNm, string sTelno);
        public DF_Send_Reg df_Send_Reg = null;

        public delegate void DF_RegUp_Proc();
        public DF_RegUp_Proc dfRegUpProc = null;
        //dfParse


        public void Connect_Combine(int nIdx)
        {
            //pTPMS = new CTPMS[nTPMSCnt];
            if (CData.garOpt1[5] == 1)
            {
                if (pLPR != null)
                {
                    pLPR = null;
                }

                pLPR = new CLPR();
                pLPR.st_LprInfo = st_LprInfo;


                nNowIdx = nIdx;

                pStation = new CStation[nNetLPRCnt];

                pStation_GT = new CStation[nNetLPRCnt];

                if (st_TpmsInfo_Proc.bUse)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (sTPMS_Use[i] == st_TpmsInfo_Proc.sID)
                        {
                            return;
                        }
                    }
                    if (pTPMS != null)
                    {
                        //pTPMSTimer.Stop();
                        pTPMS.st_ParkingInfo.bStatus = false;
                        pTPMS = null;
                    }

                    pTPMS = new CTPMS(nIdx);

                    pSDB = new CDB();
                    pSDB.InitDatabase();
                    //pSDB.dfDBCnt = Stack_Cnt;

                    pTPMS.st_TpmsInfo = st_TpmsInfo_Proc;
                    sTPMS_Use[nIdx] = "";
                    sTPMS_Use[nIdx] = st_TpmsInfo_Proc.sID;

                    pTPMS.dfLogTPMS = Log_TPMS;

                    pTPMS.dfStack = Set_Stack;
                    pTPMS.dfStackIMG = Set_Stack_IMG;
                    pTPMS.dfNotNormal = NotNoraml_InProc;
                    CData.bTPMSUse = true;


                    if (pWS != null)
                    {
                        pWS.Close();
                        pWS = null;
                    }

                    pWS = new CWS(nIdx);
                    //pWS
                    st_WsInfo_Proc.bStatus = pWS.Connect("ws://" + CData.sTpmsIP + ":" + CData.sTpmsPort + "/tpms/carmonitoring.do", pTPMS.st_TpmsInfo.sID);

                    pWS.dfProc_GT = WS_Proc;
                    pWS.dfProcList = WS_Proc_List;
                    pTPMSTimer.Interval += 5000;
                    pTPMSTimer.Elapsed += new System.Timers.ElapsedEventHandler(TpmsTimer_Work);
                    pTPMSTimer.Start();

                    pWSTimer.Interval += 5000;
                    pWSTimer.Elapsed += new System.Timers.ElapsedEventHandler(WSTimer_Work);
                    pWSTimer.Start();

                    pSDBTimer.Interval += 3000;
                    pSDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(SdbTimer_Work);
                    pSDBTimer.Start();

                    pSw.Stop();
                }


                //if (CData.garOpt1[1] == 0)
                //{
                if (pMSSQL != null)
                {
                    if (pMSSQL.pDBInfo.bStatus[0] || pMSSQL.pDBInfo.bStatus[1])
                    {
                        pDBTimer.Stop();
                        pMSSQL.pDBInfo.bStatus[0] = false;
                        pMSSQL.pDBInfo.bStatus[1] = false;
                        pMSSQL = null;
                    }


                }

                pMSSQL = new CMSSQL(nIdx);

                //bDBUse = true;

                pMSSQL.dfCal = Cal_Mu_InProc;
                //pMSSQL.
                //pMSSQL.dfReg = Reg_Send;
                pMSSQL.dfAutoMinab = pTPMS.Auto_Minab;
                pMSSQL.dfSetLog = Log_DB;
                pMSSQL.dfMigration = Migration_InProc;
                pMSSQL.dfCarOutID = CarOut_AID_InProc;
                pMSSQL.dfCarOut = Migration_Out_InProc;
                pMSSQL.dfPassTrns = Migration_Pass_InProc;
                //}
                //else
                //{
                //    //CData.pMSSQL = new CMSSQL(nIdx);



                //    //CData.pMSSQL.dfCal = Cal_Mu_InProc;
                //    ////pMSSQL.
                //    ////pMSSQL.dfReg = Reg_Send;
                //    //CData.pMSSQL.dfAutoMinab = pTPMS.Auto_Minab;
                //    //CData.pMSSQL.dfSetLog = Log_DB;
                //    //CData.pMSSQL.dfMigration = Migration_InProc;
                //    //CData.pMSSQL.dfCarOutID = CarOut_AID_InProc;
                //    //CData.pMSSQL.dfCarOut = PrevCarOut;
                //    //CData.pMSSQL.dfPassTrns = PrevPassOut;
                //}

                CData.bDBUse = true;

                if (nNowIdx == CData.nInProcIdx || nNowIdx == CData.nOutProcIdx)
                {
                    pDBTimer.Interval += 5000;
                    pDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(DBTimer_Work);
                    pDBTimer.Start();
                    CLog.LOG(LOG_TYPE.SCREEN, "#" + nNowIdx.ToString() + " DB Timer Set Suc");
                }




             





                for (int i = 0; i < nNetLPRCnt; i++)
                {

                    if (pStation[i] != null)
                    {
                        pStation[i].Close();
                        pStation = null;
                    }

                    if (pStation_GT[i] != null)
                    {
                        pStation_GT[i].Close();
                        pStation_GT = null;
                    }
                    pStation[i] = new CStation();
                    pStation_GT[i] = new CStation();
                    for (int a = 0; a < nNetLPRCnt ; a++)
                    {
                        
                        pStation[i].st_NetInfo = st_NetInfo_LPR_Proc[a];



                        if ( st_NetInfo_LPR_Proc[a].bUse)
                        {
                            switch (st_NetInfo_LPR_Proc[a].nType)
                            {
                                case 0:
                                    //pStation[i].Init_Server(0, a, nNowIdx);
                                    break;
                                case 1:
                                    //pStation[i].Init_Client(0, a, nNowIdx);
                                    break;
                                default:
                                    break;
                            }
                            //pStation[i].dfConStats = Station_Stat;
                            pStation[i].dfParse += pLPR.LPR_Parse;

                            pStation[i].dfParse_GT = Log_GT;
                        }
                        
                        pStation_GT[i].st_NetInfo = st_NetInfo_GT_Proc[a];

                        if (st_NetInfo_GT_Proc[a].bUse)
                        {
                            switch (st_NetInfo_GT_Proc[a].nType)
                            {
                                case 0:
                                    pStation_GT[a].Init_Server(1, nNowIdx, a);
                                    break;
                                case 1:
                                    pStation_GT[a].Init_Client(1, nNowIdx, a);
                                    break;
                                default:
                                    break;
                            }
                            pStation_GT[i].dfConStats = Station_Stat;
                        }
                    }


                }


                //IO_CHK;
                pLPR.dfSetIOCar = IO_CHK;
                pLPR.dfSetLog = Log_LPR;
            }
            else
            {
                if (st_TpmsInfo_Proc.bUse)
                {

                    if (pTPMS != null)
                    {
                        //pTPMSTimer.Stop();
                        pTPMS.st_ParkingInfo.bStatus = false;
                        pTPMS = null;
                    }

                    pTPMS = new CTPMS(0);

                    pSDB = new CDB();
                    pSDB.InitDatabase();
                    //pSDB.dfDBCnt = Stack_Cnt;

                    pTPMS.st_TpmsInfo = st_TpmsInfo_Proc;
                    sTPMS_Use[0] = "";
                    sTPMS_Use[0] = st_TpmsInfo_Proc.sID;

                    pTPMS.dfLogTPMS = Log_TPMS;

                    pTPMS.dfStack = Set_Stack;
                    pTPMS.dfStackIMG = Set_Stack_IMG;

                    CData.bTPMSUse = true;


                    if (pWS != null)
                    {
                        pWS.Close();
                        pWS = null;
                    }

                    pWS = new CWS(0);

                    st_WsInfo_Proc.bStatus = pWS.Connect("ws://" + CData.sTpmsIP + ":" + CData.sTpmsPort + "/tpms/carmonitoring.do", pTPMS.st_TpmsInfo.sID);
                    //pWS.dfProc_GT = WS_Proc;
                    pWS.dfProcList = WS_Proc_List;
                    //pTPMSTimer.Interval += 5000;
                    //pTPMSTimer.Elapsed += new System.Timers.ElapsedEventHandler(TpmsTimer_Work);
                    //pTPMSTimer.Start();

                    //pWSTimer.Interval += 5000;
                    //pWSTimer.Elapsed += new System.Timers.ElapsedEventHandler(WSTimer_Work);
                    //pWSTimer.Start();

                    pSDBTimer.Interval += 1000;
                    pSDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(SdbTimer_Work);
                    pSDBTimer.Start();

                    pSw.Stop();

                }

                if (!CData.bDBUse)
                {

                    CData.pMSSQL = new CMSSQL(0);

                    CData.bDBUse = true;

                    CData.pMSSQL.dfCal = Cal_Mu_InProc;
                    //CData.pMS
                    CData.pMSSQL.dfReg = Reg_Send;
                    CData.pMSSQL.dfSetLog = Log_DB;
                    CData.pMSSQL.dfCarOut = PrevCarOut;
                    CData.pMSSQL.dfPassTrns = PrevPassOut;
                    CData.pMSSQL.dfRegUp = RegUp_InProc;


                    pDBTimer.Interval += 5000;
                    pDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(DBTimer_Work);
                    pDBTimer.Start();

                    CLog.LOG(LOG_TYPE.SCREEN, "#" + nNowIdx.ToString() + " DB Timer Set Suc");
                    

                    //pDBTimer.Interval += 1000;
                    //pDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(DBTimer_Work);
                    //pDBTimer.Start();

                }
                //if (CData.garOpt1[0] == 1)
                //{
                //    if (!CData.bOraDBUse)
                //    {
                //        CData.pODB = new COraDB(nIdx);

                //        CData.bOraDBUse = true;

                //        CData.pODB.dfSelect = ODB_Select_InProc;

                //        pOraDBTimer.Interval += 2000;
                //        pOraDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(OraDBTimer_Work);
                //        pOraDBTimer.Start();
                //    }
                //}

            }

            CLog.LOG(LOG_TYPE.PROC, "Proc Idx #" + nIdx + "Combine & Start");



        }

        private void RegUp_InProc()
        {
            if (dfRegUpProc != null)
                dfRegUpProc();
        }
        private void PrevPassOut(string sCarno, string sOutDttm, string sPic, string sOutEqpm = "0")
        {
            string sFile = "";
            string sFull = "";
            bool bImg = false;
            bool bFlag = false;
            CLog.LOG(LOG_TYPE.SCREEN, "Pass #0");
            
            if (sCarno.Length > 3)
            {
                CLog.LOG(LOG_TYPE.SCREEN, "PrevsCarno = " + sPrevCarno + " & sCarno = " + sCarno);
                if (sPrevCarno != sCarno)
                {
                    try
                    {
                        CData.pDB.Insert_IOCar(0, "", sCarno);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }
                    try
                    {
                        if (CData.garOpt1[3] == 0)
                        {
                            if ((CData.garOpt1[1] == 0))
                            {
                                if (sPic.Length > 3 && sPic.IndexOf("수동") == -1 && sPic.IndexOf("?") == -1)
                                {
                                    string[] sImgPath = sPic.Split('_');
                                    sFull = sPic;
                                    CLog.LOG(LOG_TYPE.SCREEN, "Full=" + sFull);
#if DEBUG
                                sFile = "D:\\test_in.jpg";
                                sFull = "D:\\test_in.jpg";
#endif

                                    pSw.Stop();
                                    pSw.Reset();
                                    pSw.Start();
                                    try
                                    {

                                        while (pSw.Elapsed.Seconds < 40)
                                        {
                                            Application.DoEvents();

                                            if (File.Exists(sFull))
                                            {
                                                CLog.LOG(LOG_TYPE.DEBUG, "#1 File Exist = True & SearchTm = " + pSw.Elapsed.Seconds.ToString());
                                                pSw.Stop();
                                                pSw.Reset();
                                                pSw.Start();
                                                bImg = true;

                                                break;
                                            }
                                            //CLog.LOG(LOG_TYPE.SCREEN, "Now For Cnt = " + i.ToString());
                                            Thread.Sleep(10);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                                    }
                                    finally
                                    {
                                    }

                                    bFlag = true;

                                    pSw.Stop();
                                    pSw.Reset();
                                    pSw.Start();
                                    try
                                    {
                                        CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " IMG_UP_NORMAL_OUT TX" + " : { Pic=" + sPic + " Exist=" + bImg.ToString());
                                        while (pSw.Elapsed.Seconds < 40)
                                        {
                                            Application.DoEvents();

                                            if (bFlag = pTPMS.IMG_UP_OUT(sCarno, sPic, sFull, bImg))
                                            {
                                                CLog.LOG(LOG_TYPE.SCREEN, "IMG Out Flag = " + bFlag.ToString() + " StopWatch.Second = " + pSw.Elapsed.Seconds.ToString());
                                                break;
                                            }

                                            if (!pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                                                break;

                                            if (!bImg)
                                                break;

                                            Thread.Sleep(10);
                                        }
                                        pSw.Stop();
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
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    try
                    {
                        pTPMS.Mu_Out(sCarno, sOutDttm);
                        sPrevCarno = sCarno;

                        if (sCarno != "")
                        {
                            pTPMS.CLEAR_OUT(CData.pDB.Select_IOCar_ID(sCarno).ToString());
                            CData.pDB.Delete_IOCar(sCarno);
                        }

                    }
                    catch (Exception ex)
                    {
                        CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                    }
                    finally
                    {
                    }

                    //                    try
                    //                    {
                    //\\

                    //                        pTPMS.Cal_Mu(sCarno, 0, sOutDttm, sApproval, ((CData.garOpt1[3] == 1) ? false : true));

                    //                        //pTPMS
                    //                        //CData.pDB.Delete_IOCar(sCarno);
                    //                    }
                    //                    catch (Exception ex)
                    //                    {
                    //                        CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                    //                    }

                    //CData.pDB.Update_PrevID(sCarno, nID);
                    //CData.pDB.Update_PrevCar(sCarno);

                }
            }
        }

        private bool ODB_Select_InProc(LST_REG st_REG)
        {

            try
            {



                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
            }
        }
        private bool Cal_Mu_InProc(string sCarno, int nFee, string sPayDttm = "", string sOutdt = "", string sCredit = "", bool bPrev = false)
        {
            //try
            //{
            //    if (bPrev)
            //        CData.pDB.Insert_IOCar(0, "", sCarno);
            //}
            //catch (Exception ex)
            //{
            //    CLog.LOG(LOG_TYPE.ERR, ex.ToString());
            //}
            //CData.
            CData.pMSSQL.bAutoM = false;
            CData.pMSSQL.DB_Base_Timer_Stop();
            int nFee_InProc = CData.pDB.Select_IOCar_FEE(sCarno);
            try
            {
                if (sPayDttm == "T" || sPayDttm == "")
                {
                    if (nFee_InProc > 0)
                        pTPMS.Mu_Out(sCarno, sOutdt);
                }
                else
                {
                    if (sOutdt != "" || sOutdt != "T")
                    {
                        pTPMS.Mu_Out(sCarno, sPayDttm);
                    }
                }
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, ex.ToString());
            }
            finally
            {
            }

            try
            {
                if (sPayDttm == "T" || sPayDttm == "")
                {
                    if (nFee_InProc == 0)
                    {
                        pTPMS.Cal_Mu(sCarno, 0, sOutdt, sCredit, false, true);
                    }
                    else
                    {
                        if (nFee == 0)
                            pTPMS.Cal_Mu(sCarno, nFee_InProc, sOutdt, sCredit, false, true);
                        else
                            pTPMS.Cal_Mu(sCarno, nFee - nFee_InProc, sOutdt, sCredit, false, false);
                    }
                }
                else
                {
                    pTPMS.Cal_Mu(sCarno, nFee, sPayDttm, sCredit, false, false);
                }

            }
            catch (Exception ex)
            {
                CData.bParse = false;
                CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                //CData.pDB.Delete_IOCar(sCarno);
            }
            finally
            {
            }

            CData.bParse = false;
            return true;
        }

        private void CarOut_AID_InProc(int nAID, string sCarno)
        {
            CData.pDB.Update_IOCar_AID(nAID, sCarno);
        }

        private void Cal_InPorc(string sCarno, int nFee, string sPaydt = "", string sApproval = "", bool bPrev = false)
        {
            if (nFee == 0)
                pTPMS.Cal_Mu(sCarno, nFee, sPaydt, sApproval, bPrev);

        }

        private void WS_Proc(string sID, WS_CMD eCmd, string sBeh)
        {
            bool bIO = false;

            switch (eCmd)
            {
                case WS_CMD.STT_1_2011:
                case WS_CMD.STT_1_3002:
                    sBeh = "RMUp";
                    break;
                case WS_CMD.STT_1_2012:
                case WS_CMD.STT_1_3003:
                    sBeh = "RMDn";
                    break;
                case WS_CMD.STT_1_2013:
                case WS_CMD.STT_1_3004:
                    sBeh = "RMFix";
                    break;
                case WS_CMD.STT_1_2014:
                case WS_CMD.STT_1_2015:
                    sBeh = "RMUnFix";
                    break;
                case WS_CMD.STT_1_3005:
                case WS_CMD.STT_1_3006:
                    sBeh = "RMReset";
                    break;
                case WS_CMD.STT_2_5001:
                    break;
            }

            switch (eCmd)
            {
                case WS_CMD.STT_1_3002:
                case WS_CMD.STT_1_3003:
                case WS_CMD.STT_1_3004:
                case WS_CMD.STT_1_3005:
                case WS_CMD.STT_1_3006:
                    bIO = true;
                    break;

            }


            if (dfProcWs != null)
                dfProcWs(sID, bIO, sBeh);
        }
        public void WS_Proc_List(string sID, WS_CMD eCmd, List<string> arData)
        {
            string sEqpm = "1";
            string sOutEqpm = "0";
            switch (eCmd)
            {
                case WS_CMD.STT_2_1001:
                    try
                    {
                        switch (arData[7]) //임시로 각 값찾아서 저장
                        {
                            case "T5_Muin2":
                                sOutEqpm = "2";
                                break;
                            case "T9_Kiosk1":
                                sOutEqpm = "3";
                                break;
                            case "T2_Kiosk1":
                                sOutEqpm = "4";
                                break;
                            case "T1_Kiosk1":
                            case "T2_Kiosk2":
                            case "T9_Muin3":
                                sOutEqpm = "5";
                                break;
                            case "T1_Kiosk2":
                            case "T2_Muin3":
                            case "T9_Kiosk2":
                            case "T5_Muin3":
                                sOutEqpm = "6";
                                break;
                            case "T1_Kiosk3":
                            case "T9_Muin4":
                            case "T9_Yuin1":
                                sOutEqpm = "7";
                                break;
                            case "T1_Kiosk4":
                            case "T2_Muin5":
                                sOutEqpm = "8";
                                break;
                            case "T1_Kiosk5":
                                sOutEqpm = "9";
                                break;
                            case "T1_Yuin1":
                                sOutEqpm = "10";
                                break;
                            case "T1_Yuin2":
                            case "T2_Muin6":
                                sOutEqpm = "11";
                                break;
                            case "T1_Muin4":
                                sOutEqpm = "12";
                                break;
                            case "T1_Muin5":
                            case "T2_Yuin1":
                            case "T2_Yuin2":
                            case "T2_Yuin3":
                                sOutEqpm = "13";
                                break;
                            default:
                                sOutEqpm = "4";
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        sOutEqpm = "4";
                    }
                    finally
                    {
                    }
                    arData.Add(sOutEqpm);
                    //arData.Add(CData.pMSSQL.pDBInfo.stTableinfo.sPrevClm);
                    //CData.pDB.Insert_IOCar(0, "", arData[2], 1, 0, int.Parse(arData[4]));

                    //CData.pMSSQL.DB_INSERT(3, arData);
                    //if (arData[6] == "DEF")
                    //{
                    //    CData.pMSSQL.DB_UPDATE(0, arData);
                    //}
                    if (arData[8] != "DEF")
                    {
                            CData.pMSSQL.DB_UPDATE(2, arData);
                    }
                    else
                    {
                        if (arData[4] == "0")
                            CData.pMSSQL.DB_UPDATE(3, arData);
                    }
                    //CData.pMSSQL.DB_SELECT(3, arData[2], false);

                    //try { }
                    //CData.pDB.Update_IOCar_FEE(int.Parse(arData[4]), arData[2]);

                    //CData.pDB.Update_IOCar_AID(int.Parse(arData[4]), arData[2]);

                    break;
                case WS_CMD.STT_2_1002:
                    
                    //arData.Add(CData.pDB.Select_IOCar_AID(arData[9]).ToString());
                    //try
                    //{
                    //    CLog.LOG(LOG_TYPE.SCREEN, "CAL #1" + arData[11]);
                    //    arData.Add(CData.pDB.Select_IOCar_FEE(arData[9]).ToString());

                    //}
                    //catch(Exception ex)
                    //{

                    //}

                    //try
                    //{
                    //    CLog.LOG(LOG_TYPE.SCREEN, "CAL #2" + arData[11]);
                    //    arData.Add(st_LprInfo.nEqpm.ToString());
                    //}
                    //catch(Exception ex)
                    //{

                    //}
                    //{ "cardMemberNo":"840498859","bankTransDttm":"","cardTradeMedia":"1",
                    //"cardApprovalNo":"00862948","cardIssueCorpNm":"현대비자개인","cardNo":"4017-6200-****-690*","cardPurchCorpNm":"현대카드사",
                    //"receiptTy":"DEF","cmmsonAmt":"4090","carNo":"399부8346","inDttm":"2022-08-19 17:01:24","cardTradeNo":"221263877428",
                    //"receiptAmt":"45000","receiptWay":"CARD","receiptWorker":"T9_Kiosk2","cardInstallmentMonth":"0"}
                    try
                    {
                        switch (arData[14]) //임시로 각 값찾아서 저장
                        {
                            case "T5_Muin2":
                                sOutEqpm = "2";
                                break;
                            case "T9_Kiosk1":
                                sOutEqpm = "3";
                                break;
                            case "T2_Kiosk1":
                                sOutEqpm = "4";
                                break;
                            case "T1_Kiosk1":
                            case "T2_Kiosk2":
                            case "T9_Muin3":
                                sOutEqpm = "5";
                                break;
                            case "T1_Kiosk2":
                            case "T2_Muin3":
                            case "T9_Kiosk2":
                            case "T5_Muin3":
                                sOutEqpm = "6";
                                break;
                            case "T1_Kiosk3":
                            case "T9_Muin4":
                            case "T9_Yuin1":
                                sOutEqpm = "7";
                                break;
                            case "T1_Kiosk4":
                            case "T2_Muin5":
                                sOutEqpm = "8";
                                break;
                            case "T1_Kiosk5":
                                sOutEqpm = "9";
                                break;
                            case "T1_Yuin1":
                                sOutEqpm = "10";
                                break;
                            case "T1_Yuin2":
                            case "T2_Muin6":
                                sOutEqpm = "11";
                                break;
                            case "T1_Muin4":
                                sOutEqpm = "12";
                                break;
                            case "T1_Muin5":
                            case "T2_Yuin1":
                            case "T2_Yuin2":
                            case "T2_Yuin3":
                                sOutEqpm = "13";
                                break;
                            default:
                                sOutEqpm = "4";
                                break;
                        }
                    }
                    catch(Exception)
                    {
                        sOutEqpm = "4";
                    }
                    finally
                    {
                    }
                    arData.Add(sOutEqpm);
                    if (arData[1] == "" || arData[1] == null)
                        CLog.LOG(LOG_TYPE.SCREEN, "결제시간 없음으로 쿼리 실행 X");
                    else
                    {
                        CLog.LOG(LOG_TYPE.SCREEN, "CAL #3" + arData[11]);
                        if (arData[14].IndexOf("Kiosk") != -1)
                            CData.pMSSQL.DB_UPDATE(0, arData);
                        else
                        {
                            CData.pMSSQL.DB_UPDATE(1, arData, 2);
                        }

                    }
                    //CData.pDB.Delete_IOCar(arData[9]);
                    break;
                case WS_CMD.STT_2_1003:
                    if (CData.pMSSQL.pDBInfo.stTableinfo.sLocateClm == "3")
                        sEqpm = "8";
                    try
                    {
                        string sInEqpm = "1";
                        
                        //arData.Add(CData.pMSSQL.pDBInfo.stTableinfo.sPrevClm);

                        switch (arData[6]) //임시로 각 값찾아서 저장
                        {
                            case "T1_Muin2":
                            case "T2_Muin2":
                            case "T34_Muin6":
                            case "T34_Muin7":
                                sInEqpm = "2";
                                break;
                            case "T1_MUin3":
                            case "T4_Muin2":
                                sInEqpm = "3";
                                break;
                            case "T4_Muin3":
                                sInEqpm = "12";
                                break;
                            case "T4_Muin16":
                                sInEqpm = "8";
                                break;
                            case "T4_Muin17":
                                sInEqpm = "10";
                                break;
                            case "T33_Muin2":
                                sInEqpm = "5";
                                break;
                            default:
                                sInEqpm = "1";
                                break;
                        }

                        arData.Add(sInEqpm);

                        if (CData.garOpt1[5] == 1)
                        {
                                pMSSQL.DB_INSERT(0, arData, ((arData[7] == "DEF") ? false : true));
                                pMSSQL.DB_INSERT(2, arData);
                            
                        }
                        else
                        {
                            CLog.LOG(LOG_TYPE.SCREEN, "1003 CData Insert");
                            CData.pMSSQL.DB_INSERT(0, arData, ((arData[7] == "DEF") ? false : true));
                            CData.pMSSQL.DB_INSERT(2, arData);

                        }

                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }

                    //CData.pMSSQL.DB_INSERT(2, arData);
                    break;
                default:
                    break;
            }
        }

        private void PrevCarOut(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sOutEqpm = "0", int nService = 0)
        {
            bool bFlag = false;
            bool bImg = false;
            int nNowFee = 0;
            string sFull = "";

            //string sDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //string sDate = "";
            //sDate = sDttm.Replace(" 오후", "");
            //string[] sDate1 = sDttm.Split(':');
            //string sDate2 = "";
            //string sDate3 = sDate1[0].Substring(sDate1[0].Length - 1);

            //if(sDttm.Contains("오후"))
            //    sDate3 = (int.Parse(sDate3) + 12).ToString();

            //sDate2 = sDate.Substring(0, 10) + sDate3 + sDate1[1] + sDate[2];

            
            if (sCarno.Length > 3)
            {
                CLog.LOG(LOG_TYPE.SCREEN, "PrevID = " + CData.pDB.Select_PrevCarno(nNowIdx.ToString()) + " & nID = " + nID.ToString());

                if (CData.garOpt1[1] == 1)
                {
                    if ((CData.pDB.Select_PrevCarno(sOutEqpm) == nID.ToString()))
                    {
                        return;
                    }
                }
                    CData.pDB.Insert_IOCar(0, "", sCarno);

                try
                {
                    if (CData.garOpt1[3] == 0)
                    {
                        if (sFile.Length > 3 && sFile.IndexOf("수동") == -1 && sFile.IndexOf("?") == -1)
                        {
                            //string[] sImgPath = sFile.Split('_');
                            sFull = sFile;
                            CLog.LOG(LOG_TYPE.SCREEN, "Full=" + sFull);
#if DEBUG
                        sFile = "D:\\test_in.jpg";
                        sFull = "D:\\test_in.jpg";
#endif

                            pSw.Stop();
                            pSw.Reset();
                            pSw.Start();
                            try
                            {

                                while (pSw.Elapsed.Seconds < 40)
                                {
                                    Application.DoEvents();

                                    if (File.Exists(sFull))
                                    {
                                        CLog.LOG(LOG_TYPE.DEBUG, "#1 File Exist = True & SearchTm = " + pSw.Elapsed.Seconds.ToString());
                                        pSw.Stop();
                                        pSw.Reset();
                                        pSw.Start();
                                        bImg = true;

                                        break;
                                    }
                                    //CLog.LOG(LOG_TYPE.SCREEN, "Now For Cnt = " + i.ToString());
                                    Thread.Sleep(10);
                                }
                            }
                            catch (Exception ex)
                            {
                                CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                            }
                            finally
                            {
                            }
                            bFlag = true;

                            pSw.Stop();
                            pSw.Reset();
                            pSw.Start();
                            try
                            {
                                CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " IMG_UP_NORMAL_OUT TX" + " : { Pic=" + sFile + " Exist=" + bImg.ToString());
                                while (pSw.Elapsed.Seconds < 40)
                                {
                                    Application.DoEvents();

                                    if (bFlag = pTPMS.IMG_UP_OUT(sCarno, sFile, sFull, bImg))
                                    {
                                        CLog.LOG(LOG_TYPE.SCREEN, "IMG Out Flag = " + bFlag.ToString() + " StopWatch.Second = " + pSw.Elapsed.Seconds.ToString());
                                        break;
                                    }

                                    if (!pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                                        break;

                                    if (!bImg)
                                        break;

                                    Thread.Sleep(10);
                                }
                                pSw.Stop();
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
                catch (Exception)
                {

                }
                finally
                {
                }

                try
                {
                    pTPMS.Mu_Out(sCarno, sDttm);

                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                }
                finally
                {
                }

                try
                {
                    //if (sApproval.Length > 8)
                    //{
                    //    sApproval = sApproval.Trim();
                    //    sApproval = sApproval.Replace(" ", "");
                    //    sApproval.Substring(0, 8);
                    //}
                    nNowFee = nFee;
                    CLog.LOG(LOG_TYPE.SCREEN, "#0 nowFee=" + nNowFee);
                    if (CData.garOpt1[1] == 0)
                    {
                        //PartAmt 제외 및 정산요금만 남음 ex) Amt = 1000 ->
                        
                        nNowFee = CData.pDB.Select_IOCar_PartAmt(sCarno); 

                        if (nFee == nNowFee)
                            nNowFee = 0;
                        else
                        {
                            if (nFee > nNowFee)
                                nNowFee = (nFee - nNowFee);
                            else
                                nNowFee = (nNowFee - nFee);
                        }


                    }
                        nNowFee += nService;
                    
                    CLog.LOG(LOG_TYPE.SCREEN, "#1 nFee-nowFee=" + (nFee - nNowFee).ToString());
                    CLog.LOG(LOG_TYPE.SCREEN, "#1 nowFee=" + nNowFee);

                    //}
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                }
                finally
                {
                }

                try
                {
                    CLog.LOG(LOG_TYPE.SCREEN, "#2 Approval=" + sApproval);
                    pTPMS.Cal_Mu(sCarno, nNowFee, sDttm, sApproval, ((CData.garOpt1[1] == 0) ? false : true), ((nNowFee == 0) ? true : false));

                    //pTPMS
                    //CData.pDB.Delete_IOCar(sCarno);
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                }
                finally
                {
                }

                CData.pDB.Update_PrevID(sCarno, nID, sOutEqpm);
                    //CData.pDB.Update_PrevCar(sCarno);

                
            }


            //if (CData.pMSSQL.pDBInfo.bStatus[0] && pTPMS.sLastOutCar[0] != "F")
            //    CData.pMSSQL.DB_SELECT(2, sCarno);
        }

        private void Migration_Pass_InProc(string sCarno, string sDttm, string sFile, string sOutEqpm)
        {
            if (dfProcMigPass != null)
                dfProcMigPass(sCarno, sDttm, sFile, sOutEqpm);
        }

        private void Migration_Out_InProc(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sOutEqpm, int nService = 0)
        {
            if (dfProcMigOut != null)
                dfProcMigOut(sCarno, nFee, sApproval, sDttm, nID, sFile, sOutEqpm, nService);
        }

        private void Migration_InProc(int nEqpm, string sInCarno, string sInDttm, string sImgPath)
        {
            if (dfProcMig != null)
                dfProcMig(nEqpm, sInCarno, sInDttm, sImgPath);
        }

        public void Migration_SendTPMS(string sInCarno, string sInDttm, string sFull_Rcv, string sImgPath, ref string[] sSub)
        {
            //sSub

#if DEBUG
            sImgPath = "D:\\test_in.jpg";
            sFull_Rcv = "D:\\test_in.jpg";
#endif
            IO_CHK(true, sInCarno, sInDttm, sFull_Rcv, sImgPath, ref sSub, "");
        }

        public void Migration_Out_SendTPMS(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sPath, string sOutEqpm = "99", int nService = 0)
        {
            //sSub

#if DEBUG

            sFile = "D:\\test_in.jpg";
#endif
            try
            {
                PrevCarOut(sCarno, nFee, sApproval, sDttm, nID, sFile, sOutEqpm, nService);
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }

        public void Migration_Pass_SendTPMS(string sCarno, string sDttm, string sFile)
        {
            //sSub

#if DEBUG

            sFile = "D:\\test_in.jpg";
#endif
            try
            {
                PrevPassOut(sCarno, sDttm, sFile);
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }

        public void Reg_Down(string sDate)
        {
            try
            {
                
                if(CData.garOpt1[5] == 1)
                {
                    if (pMSSQL != null)
                    {
                        pMSSQL.DB_SELECT(9, sDate, false, "", 0);
                    }
                }
                else
                {
                    if (CData.pMSSQL != null)
                    {
                        CData.pMSSQL.DB_SELECT(9, sDate, false, "", 0);
                    }
                }

            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }

        private void Reg_Send(string sCarno_Reg, string sStartDttm, string sEndDttm, string sRegDiv, string sGroupNm, string sUserNm, string sMemo, string sArea, string sAreaArray)
        {
            //if (df_Send_Reg != null)
            //{
            //    df_Send_Reg(sCarno_Reg, sStartDttm, sEndDttm, sRegDiv, sGroupNm, sUserNm, sTelno);
            //}

            try
            {
                //Application.DoEvents();
                CData.pDB.Insert_RegCar(sCarno_Reg, sStartDttm, sEndDttm, sRegDiv, sGroupNm, sUserNm, sMemo, sArea, sAreaArray);
            }
            catch (Exception)
            {

            }
            finally
            {
            }        //CData.pDB.

            //CData.pDB.Select_IOCar_AID
        }

        private void Station_Stat(int nDiv, bool bChk, int nNet, int nNIdx, int nWhere, bool bDif, string sStartDttm, string sEndDttm)
        {
            string sStat = "";

            if (bDif)
            {
                if (!CData.bMig)
                {
                    //CData.pMSSQL.DB_SELECT(6, sStartDttm + "|" + sEndDttm);
                }
            }

            try
            {
                if (nDiv == 0)
                {
                    switch (nNet)
                    {
                        case 0:
                            CLog.LOG(LOG_TYPE.STATION, "#" + nNIdx.ToString() + "#" + nWhere.ToString() + " S Listen (" + st_NetInfo_LPR_Proc[nNIdx].nPort.ToString() + ") " + ((bChk == true) ? "Start" : "Stop"));
                            break;
                        case 1:
                            CLog.LOG(LOG_TYPE.STATION, "#" + nNIdx.ToString() + "#" + nWhere.ToString() + " C Connect (" + st_NetInfo_LPR_Proc[nNIdx].sIP + ":" + st_NetInfo_LPR_Proc[nNIdx].nPort.ToString() + ") " + ((bChk == true) ? "Success" : "Failed"));
                            break;
                        default:
                            break;
                    }

                    if (dfLPRStat != null)
                        dfLPRStat(nWhere, nNIdx, bChk, CData.arIOType[st_NetInfo_LPR_Proc[nNIdx].stLPRInfo.nIOType]);
                }
                else
                {
                    switch (nNet)
                    {
                        case 0:
                            CLog.LOG(LOG_TYPE.GT, "GT #" + nNIdx.ToString() + "#" + nWhere.ToString() + " S Listen (" + pStation_GT[nWhere].st_NetInfo.nPort.ToString() + ") " + ((bChk == true) ? "Start" : "Stop"));
                            break;
                        case 1:
                            CLog.LOG(LOG_TYPE.GT, "GT #" + nNIdx.ToString() + "#" + nWhere.ToString() + " C Connect (" + pStation_GT[nWhere].st_NetInfo.sIP + ":" + pStation_GT[nWhere].st_NetInfo.nPort.ToString() + ") " + ((bChk == true) ? "Success" : "Failed"));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }



        }

        private void Migration_Dif_Time()
        {

        }

        private void Station_GT_Stat(bool bChk, int nNet, int nNIdx, int nWhere)
        {
            string sStat = "";



            //if (dfLPRStat != null)
            //    dfLPRStat(nWhere, nNIdx, bChk, );



        }


        private void Connect_Solve(int nIdx)
        {
            if (pTPMS != null)
            {
                //pTPMSTimer.Stop();
                pTPMS.st_ParkingInfo.bStatus = false;
                pTPMS = null;
            }

            if (pStation != null)
            {
                pStation[nIdx].Close();
                pStation = null;
            }

            if (pLPR != null)
                pLPR = null;

        }


        //dfSetIOCar(nIdx, ((arData[4] == "0") ? true : false), arData[5], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        public void IO_CHK(bool bIO, string sCarno, string sDate, string sFull, string sFileNm, ref string[] sSubRcv, string sDiv = "", bool bPass = false)
        {

            bool bImg = false;
            string sBody = "";
            string sBody_End = "";
            string sHeader = "";
            string sUrl = "";
            int nLen = 0;
            int nFileChk = 0;
            bool bLogin = false;
            bool bCal = false;
            bool bFlag = true;

            //if (CData.bParse)
            //{
            //    StringBuilder sb = new StringBuilder();

            //    try
            //    {
            //        Application.DoEvents();
            //        Set_Stack((bIO == true) ? TPMS_CMD.MU_IN : TPMS_CMD.MU_OUT, pTPMS.st_TpmsInfo.sID, sb, sFileNm);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //    Application.DoEvents();
            //}
            //경기12가1234
            CData.bParse = true;

            try
            {


                if (sCarno != "No_Detection")
                {
                    if (sCarno.Length > 9)
                        sCarno = sCarno.Substring(sCarno.Length - 4);
                }
                else
                {
                    sCarno = "미인식";
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }
            //if(sSubRcv)
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    //int i = 0;
                    if (sSubRcv[i] != null)
                    {
                        if (sSubRcv[i].Length > 2)
                        {
                            StringBuilder sbInOut = new StringBuilder();
                            Set_Stack((bIO == true) ? TPMS_CMD.MU_IN : TPMS_CMD.MU_OUT, pTPMS.st_TpmsInfo.sID, sbInOut, sSubRcv[i]);
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }

            //CLog.LOG(LOG_TYPE.DEBUG, "Full : " + sFull);
            //while (nFileChk <= 30)
            //{
            if (CData.garOpt1[3] == 0)
            {
                CLog.LOG(LOG_TYPE.DEBUG, "Full : " + sFull);
                if (!bPass)
                {
                    if (sFull.Length > 3 && sFull.IndexOf("수동") == -1 && sFull.IndexOf("?") == -1)
                    {
                        pSw.Stop();
                        pSw.Reset();
                        pSw.Start();

                        try
                        {

                            while (pSw.Elapsed.Seconds < 40)
                            {
                                Application.DoEvents();

                                if (File.Exists(sFull))
                                {
                                    CLog.LOG(LOG_TYPE.DEBUG, "#1 File Exist = True & SearchTm = " + pSw.Elapsed.Seconds.ToString());
                                    pSw.Stop();
                                    pSw.Reset();
                                    pSw.Start();
                                    bImg = true;
                                    string a = "1111";
                                    
                                    break;
                                }
                                //CLog.LOG(LOG_TYPE.SCREEN, "Now For Cnt = " + i.ToString());
                                Thread.Sleep(10);
                            }
                        }
                        catch (Exception ex)
                        {
                            CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                        }
                        finally
                        {
                        }

                        bFlag = true;


                        pSw.Stop();
                        pSw.Reset();
                        pSw.Start();

                        try
                        {
                            CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + ((bIO == true) ? " IMG_UP_NORMAL_IN TX" : " IMG_UP_NORMAL_OUT TX") + " : { Pic=" + sFileNm + " Exist=" + bImg.ToString());

                            while (pSw.Elapsed.Seconds < 40)
                            {
                                Application.DoEvents();
                                if (bIO)
                                {
                                    if (bFlag = pTPMS.IMG_UP_IN(sCarno, sFileNm, sFull, bImg))
                                    {
                                        CLog.LOG(LOG_TYPE.SCREEN, "IMG IN Flag = " + bFlag.ToString() + " StopWatch.Second = " + pSw.Elapsed.Seconds.ToString());
                                        break;
                                    }


                                }
                                else
                                {
                                    if (bFlag = pTPMS.IMG_UP_OUT(sCarno, sFileNm, sFull, bImg))
                                    {
                                        CLog.LOG(LOG_TYPE.SCREEN, "IMG Out Flag = " + bFlag.ToString() + " StopWatch.Second = " + pSw.Elapsed.Seconds.ToString());
                                        break;
                                    }
                                }

                                if (!pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                                    break;

                                if (!bImg)
                                    break;

                                Thread.Sleep(10);
                            }
                            pSw.Stop();

                            //    nFileChk++;
                            //}
                            //if (!CData.bLogVr)
                            //{
                            //    if (!pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                            //        return;
                            //}
                        }
                        catch (Exception ex)
                        {
                            CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                        }
                        finally
                        {
                        }
                    }
                }
            }
            if (bIO)
            {
                try
                {
                    pTPMS.Mu_In(sCarno, sDate);
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                }
                finally
                {
                }
            }
            else
            {
                try
                {

                    CLog.LOG(LOG_TYPE.DB_M, "AutoMinab=" + CData.pMSSQL.bAutoM.ToString());

                    CData.pMSSQL.DB_Base_Timer_Stop();

                    CLog.LOG(LOG_TYPE.DB_M, "Timer Stop");

                    if (CData.pMSSQL != null)
                    {
                        if (CData.pMSSQL.bAutoM == true)
                        {
                            if (pTPMS.sLastOutCar[0].Length > 1 || pTPMS.sLastOutCar[1].Length > 1)
                            {
                                //pTPMS.Auto_Minab();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                }
                finally
                {
                }

                try
                {
                    pTPMS.Mu_Out(sCarno, sDate);
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                }
                finally
                {
                }

                try
                {
                    if (pTPMS.st_TpmsInfo.sSecID == "50")
                    {
                        pTPMS.Cal_Mu(sCarno, 0, "", "", false, true);
                        //pTPMS.CLEAR_OUT(CData.pDB.);
                        //CData.pDB.Delete_IOCar(sCarno);
                    }
                    else
                    {

                        if (CData.pMSSQL.pDBInfo.bStatus[0] && pTPMS.sLastOutCar[0] != "F")
                            CData.pMSSQL.DB_SELECT(1, sCarno);


                    }
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, "IMG Err = " + ex.ToString());

                }
                finally
                {
                }
            }

            CData.bParse = false;
        }

        private void Log_TPMS(string sID, string sLog)
        {
            //if (dfTPMSProc != null)
            //    dfTPMSProc(sID, sLog);

            if (CData.ucTpms != null)
                CData.ucTpms.Lsv_Show(sID, sLog);

        }

        private void Log_DB(string sDB, string sLog)
        {
            //if (dfDBProc != null)
            //    dfDBProc(sDB, "#" + nNowIdx.ToString() + " " + sLog);

            if (CData.ucMssql != null)
                CData.ucMssql.Lsv_Show(sDB, "#" + nNowIdx.ToString() + " " + sLog);
        }


        private void Log_ODB(string sDB, string sLog)
        {
            //if (dfODBProc != null)
            //    dfODBProc(sDB, sLog);
        }

        private void Log_LPR(string sID, string sLog)
        {
            //if (dfLPRProc != null)
            //    dfLPRProc(sID, sLog);
        }

        private void Log_GT(string sID, bool bIO, string sLog)
        {
            //if (dfGTProc != null)
            //    dfGTProc(sID, bIO, sLog);
        }

        private void Set_Stack(TPMS_CMD eCmd, string sID, StringBuilder sData, string sCarno = "")
        {
            pSDB.Insert_Stack(eCmd.ToString(), sID, sCarno, "IO");
        }
        private void Set_Stack_IMG(TPMS_CMD eCmd, string sID, StringBuilder sData, string sNm, string sFull, bool bExist)
        {
            pSDB.Insert_Stack(eCmd.ToString(), sID, sData.ToString(), sNm + "|" + sFull + "|" + bExist.ToString());
        }

        public void InCar_Migration(string sStartDttm, string sEndDttm)
        {
            if (CData.garOpt1[5] == 0)
                CData.pMSSQL.DB_SELECT(4, sStartDttm + "|" + sEndDttm, false, "");
            else
                pMSSQL.DB_SELECT(4, sStartDttm + "|" + sEndDttm, false, "");
        }

        public void InCar_Migration_Time(string sStartDttm, string sEndDttm)
        {
            if (CData.garOpt1[5] == 0)
                CData.pMSSQL.DB_SELECT(4, sStartDttm + "|" + sEndDttm, true);
            else
                pMSSQL.DB_SELECT(4, sStartDttm + "|" + sEndDttm, true);
        }


        private void TpmsTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            //pTPMSTimer.Stop();
            pTPMSTimer.Stop();

            bool bRegDown = false;
            bool bPingResult = false;

            Ping pingSender = new Ping();

            try
            {
                for (int i = 0; i < 10; i++)
                {
                        switch (pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                        {
                            case false:
                                nReTokenCnt[i] = 0;
                                if (!CData.bLogVr)
                                    pTPMS.Login();
                                break;
                            case true:
                            //pTPMSTimer.Interval = 5000;

                            try
                            {
                                PingReply reply = pingSender.Send(CData.sTpmsIP, 5000);


                                switch (reply.Status)
                                {
                                    case IPStatus.Success:
                                        bPingResult = true;
                                        nServerPing = 0;
                                        break;
                                    case IPStatus.TimedOut:
                                        bPingResult = false;
                                        nServerPing++;
                                        break;
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
                                //if (nReHealthCnt[i] >= 12)
                                //{
                                //    CLog.LOG(LOG_TYPE.SCREEN, "HealthChk");
                                //    pProc[i].pTPMS.HealthChk();
                                //    nReHealthCnt[i]++;
                                //}
                            }
                            catch (Exception)
                            {

                            }
                            finally
                            {
                            }

                            if (nServerPing > 10)
                                {
                                    CLog.LOG(LOG_TYPE.SCREEN, "Tpms Stat = false & ServerPing > 10");
                                    pTPMS.st_TpmsInfo.stPkInfo.bStatus = false;
                                }

                                if (nReTokenCnt[i] >= 300)
                                {
                                    CLog.LOG(LOG_TYPE.SCREEN, "#" + i.ToString() + " Retoken Run Suc");
                                    pTPMS.Re_Token(TPMS_CMD.LOGIN);
                                    nReTokenCnt[i] = 0;

                                }


                                nReTokenCnt[i]++;
                                nReHealthCnt[i]++;
                                break;
                            default:
                                break;
                        }
                }

            }
            catch (Exception)
            {

            }
            finally
            {
            }
            pTPMSTimer.Start();

            if (dfAllSendStat != null)
                dfAllSendStat(0, pTPMS.st_TpmsInfo.stPkInfo.bStatus);

            //pTPMSTimer.Start();
        }

        private void WSTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            //pWSTimer.Stop();
            byte[] bSend = new byte[2];
            bSend[0] = 137;
            bSend[1] = 0;

            switch (st_WsInfo_Proc.bStatus)
            {
                case false:
                    st_WsInfo_Proc.bStatus = pWS.Connect("ws://" + CData.sTpmsIP + ":" + CData.sTpmsPort + "/tpms/carmonitoring.do", pTPMS.st_TpmsInfo.sID);
                    break;
                case true:
                    st_WsInfo_Proc.bStatus = pWS.CheckAlive();
                    break;
                default:
                    break;
            }
            //pWSTimer.Start();
        }

        private void DBTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            pDBTimer.Stop();
            if (CData.garOpt1[5] == 1)
            {
                try
                {
                    switch (pMSSQL.pDBInfo.bStatus[0])
                    {
                        case false:
                            pDBTimer.Interval = 5000;
                            
                            if (pMSSQL.pDBInfo.sBaseDB != "")
                                pMSSQL.ConnectToDB(0, (CData.garOpt1[1] == 1) ? true : false);

                            if (pMSSQL.pDBInfo.sOperDB != "")
                                pMSSQL.ConnectToDB(1);

                            if (nNowIdx == 0)
                            {
                                for (int i = 2; i < 10; i++)
                                {
                                    try
                                    {

                                        pMSSQL.ConnectToDB(i);

                                    }
                                    catch(Exception)
                                    {

                                    }
                                    finally
                                    {
                                    }
                                }

                            }
                            break;
                        case true:
                            pDBTimer.Interval = 10000;
                            try
                            {
                                if (CData.garOpt1[2] == 0)
                                {
                                    for (int i = 0; i < 10; i++)
                                    {
                                        if (pStation[i] != null)
                                        {
                                            if (pStation[i].st_NetInfo.stLPRInfo.bUse)
                                            {
                                                if (pStation[i].st_NetInfo.stLPRInfo.nIOType == 0 && CData.nInProcIdx == nNowIdx)
                                                {
                                                    //CLog.LOG(LOG_TYPE.SCREEN, "idx = 0 & SELECT 6");

                                                    pMSSQL.DB_SELECT(6, "", true, "", 1);

                                                    //pMSSQL.DB_SELECT(12, "", true, "", 1);
                                                    //pMSSQL.DB_SELECT(13, "", true, "", 1);
                                                    if (nManuTrns >= 6)
                                                    {
                                                        pMSSQL.DB_SELECT(12, "", true, "", 1, st_TpmsInfo_Proc.sLotArea);
                                                        nManuTrns = 0;
                                                    }
                                                }
                                                else if (pStation[i].st_NetInfo.stLPRInfo.nIOType == 1 && CData.nOutProcIdx == nNowIdx)
                                                {
                                                    //else if (pStation[i].st_NetInfo.stLPRInfo.nIOType == 1 && CData.nOutProcIdx == nNowIdx)
                                                    pMSSQL.DB_SELECT(2, "", ((CData.garOpt1[1] == 0) ? false : true), pStation[i].st_NetInfo.stLPRInfo.nEqpm.ToString(), nNowIdx + 1, st_TpmsInfo_Proc.sLotArea);

                                                    if (CData.garOpt1[1] == 0)
                                                        pMSSQL.DB_SELECT(8, "", false, pStation[i].st_NetInfo.stLPRInfo.nEqpm.ToString(), nNowIdx + 1, st_TpmsInfo_Proc.sLotArea);

                                                    if (CData.sAirPort == "51")
                                                        pMSSQL.DB_SELECT(8, "", false, pStation[i].st_NetInfo.stLPRInfo.nEqpm.ToString(), nNowIdx + 1, st_TpmsInfo_Proc.sLotArea);


                                                }
                                                //CLog.LOG(LOG_TYPE.SCREEN, "DB Timer Event Tick #1");
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception)
                            {
                            }
                            finally
                            {

                            }
                            break;
                        default:
                            break;
                    }
                    nManuTrns++;
                }
                catch (Exception)
                {

                }
                finally
                {
                }
            }
            else
            {
                
                switch (CData.pMSSQL.pDBInfo.bStatus[0])
                {
                    case false:
                        pDBTimer.Interval = 5000;

                        //if (CData.pMSSQL.pDBInfo.sBaseDB != "")
                        //    CData.pMSSQL.ConnectToDB(0, (CData.garOpt1[1] == 1) ? true : false);


                        if (CData.pMSSQL.pDBInfo.sBaseDB != "")
                            CData.pMSSQL.ConnectToDB(0, false);

                        if (CData.pMSSQL.pDBInfo.sOperDB != "")
                            CData.pMSSQL.ConnectToDB(1);


                            if (nNowIdx == 0)
                            {
                                for (int i = 2; i < 10; i++)
                                {
                                    CData.pMSSQL.ConnectToDB(i);
                                }
                            }

                        break;
                    case true:
                        
                        if (CData.garOpt1[1] == 0)
                        {
                            if (CData.garOpt1[2] == 0)
                            {
                                //pDBTimer.Interval = 60000;
                                //if (pMSSQL.pDBInfo.bStatus[1])
                                //    pMSSQL.DB_SELECT(0, "");
                            }
                        }
                        else
                        {

                            try
                            {
                                if (CData.garOpt1[0] == 0)
                                {
                                    pDBTimer.Interval = 1000;
                                    for (int i = 0; i < 10; i++)
                                    {
                                        if (pStation[i] != null)
                                        {
                                            if (pStation[i].st_NetInfo.stLPRInfo.nIOType == 1 && pStation[i].st_NetInfo.stLPRInfo.bUse)
                                            {
                                                if (CData.garOpt1[1] == 0)
                                                {
                                                    CData.pMSSQL.DB_SELECT(2, "", true, pStation[i].st_NetInfo.stLPRInfo.nEqpm.ToString(), nNowIdx);
                                                }


                                            }
                                            else if (pStation[i].st_NetInfo.stLPRInfo.nIOType == 0)
                                            {

                                                //if (nNowIdx == 0)
                                                //    CData.pMSSQL.DB_SELECT(6, "", false, "", 1);

                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    pDBTimer.Interval = 10000; //

                                    //if(nNowHour == 999)
                                    //{
                                    //    nNowHour = int.Parse(DateTime.Now.ToString("HH"));
                                    //}
                                    //999 => 14, 14 => 15
                                    if(nNowHour == int.Parse(DateTime.Now.ToString("HH")))
                                    {

                                        CData.nHourChecker++;

                                    }
                                    else
                                    {
                                        CData.nHourChecker = 0;
                                    }

                                    CLog.LOG(LOG_TYPE.SCREEN, "PrevHour=" + nNowHour.ToString() + "&NowHour=" + int.Parse(DateTime.Now.ToString("HH")).ToString() + "&HourChecker=" + CData.nHourChecker.ToString());
                                    nNowHour = int.Parse(DateTime.Now.ToString("HH"));

                                    //switch (int.Parse(DateTime.Now.ToString("HH")) % 2)
                                    //{
                                    //    case 0:
                                    //        CData.nHourChecker++;
                                    //        break;
                                    //    default:
                                    //        CData.nHourChecker = 0;
                                    //        break;
                                    //}

                                    if (CData.nHourChecker == 1)
                                    {
                                        CData.pDB.Delete_Reg();
                                        CData.pMSSQL.DB_SELECT(10, "", false, "", 0);
                                    }

                                }

                            }
                            catch (Exception)
                            {

                            }
                            finally
                            {
                            }
                        }
                        break;
                    default:
                        break;
                }


            }
            try
            {
                if (dfAllSendStat != null)
                    dfAllSendStat(1, (CData.garOpt1[5] == 0) ? CData.pMSSQL.pDBInfo.bStatus[0] : pMSSQL.pDBInfo.bStatus[0]);
            }
            catch (Exception)
            {

            }
            finally
            {
            }




            pDBTimer.Start();
        }

        private void OraDBTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            pOraDBTimer.Stop();
            switch (CData.pODB.st_oraDBInfo.bStatus)
            {
                case false:
                    CData.pODB.Open();
                    break;
                case true:
                    break;
                default: 
                    break;
            }

            if (dfAllSendStat != null)
                dfAllSendStat(2, CData.pODB.st_oraDBInfo.bStatus);

            pOraDBTimer.Start();

        }
        public string Stack_Use()
        {
            try
            {
                string sRows = pSDB.Select_Stack_Use();

                return sRows;
            }
            catch(Exception)
            {
                return "";
            }
            finally
            {
            }

        }

        public void Stack_Del()
        {
            try
            {
                pSDB.Delete_Stack();
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }
        public bool TPMS_Run(TPMS_CMD eCmd, string sParam, string sImg = "", bool bPass = false)
        {
            bool bStackSuc = false;
            TPMS_CMD eTimer_Cmd = eCmd;
            switch (eCmd)
            {
                case TPMS_CMD.IMG_UP_NORMAL_IN:
                    string[] sRows_Col_In = sImg.Split('|');
                    try
                    {
                        bStackSuc = pTPMS.SendCmd_Builder(TPMS_CMD.IMG_UP_NORMAL_IN, sParam, sRows_Col_In[0], sRows_Col_In[1], bool.Parse(sRows_Col_In[2]));
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }
                    bStackSuc = true;
                    break;
                case TPMS_CMD.IMG_UP_NORMAL_OUT:
                    string[] sRows_Col_Out = sImg.Split('|');
                    try
                    {
                        bStackSuc = pTPMS.SendCmd_Builder(TPMS_CMD.IMG_UP_NORMAL_OUT, sParam, sRows_Col_Out[0], sRows_Col_Out[1], bool.Parse(sRows_Col_Out[2]));
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                    }
                    bStackSuc = true;
                    break;
                case TPMS_CMD.MU_OUT:
                    bStackSuc = pTPMS.SendCmd_Builder(TPMS_CMD.MU_OUT, sParam);
                    break;
                case TPMS_CMD.MU_IN:
                    bStackSuc = pTPMS.SendCmd_Builder(TPMS_CMD.MU_IN, sParam);
                    break;
                case TPMS_CMD.CAL_MU:
                    bStackSuc = pTPMS.SendCmd_Builder(TPMS_CMD.CAL_MU, sParam);
                    break;
                default:
                    //pTPMS.SendCmd_Bulider();
                    break;
            }

            return bStackSuc;
        }

        private void NotNoraml_InProc(string sCarno, string sRsType)
        {
            try
            {

                //pTPMS.Cal_Mu(sCarno, 0, "", "", false, true);

                //CData.pDB.Delete_IOCar(sCarno);
            }
            catch(Exception)
            {
                CData.bParse = false;
            }
            finally
            {
            }
            CData.bParse = false;
        }

        private void SdbTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            bool bStackSuc = false;
            pSDBTimer.Stop();
            try
            {
                CData.nStackCnt = pSDB.Select_Cnt_Stack();
                if (dfStackCnt != null)
                    dfStackCnt();
            }
            catch (Exception)
            {

            }
            finally
            {
            }

            pSDBTimer.Start();
        }
    }
}

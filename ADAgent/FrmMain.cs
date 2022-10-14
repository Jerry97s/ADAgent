using ADAgent.DATA;
using ADAgent.NET;
using ADAgent.TPMS;
using ADAgent.UTIL;
using iNervMCS.UTIL;
using iNervMng.TPMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADAgent
{
    public partial class FrmMain : Form
    {
        CStation pStation = null;

        CMSSQL pMS = null;
        CProc[] pProc = null;
        CDB pDB = null;
        int nCnt = 0;
        string sPrev_Row = "";
        int nServerPing = 0;
        //List<Label> lblChk = new List<Label>();
        Label[,] lblChk = new Label[10, 10];
        string[] sGt_Use = new string[10];
        Thread[] thread;
        public delegate void DF_SecTimerEvent();
        int[] nReTokenCnt = new int[10];
        int[] nReLoginCnt = new int[10];
        int[] nReHealthCnt = new int[10];
        int nReCmdCnt = 0;
        int nRegCnt = 0;
        bool bExitApp = false;
        int nTrayCnt = 0;
        int nLsvCnt = 0;
        bool[] bPrevStat_TPMS = new bool[10];
        bool[] bPrevStat_MSSQL = new bool[10];
        System.Timers.Timer pTPMSTimer = new System.Timers.Timer();

        System.Timers.Timer pWSTimer = new System.Timers.Timer();

        System.Timers.Timer pSDBTimer = new System.Timers.Timer();
        
        System.Timers.Timer pStatTimer = new System.Timers.Timer();

        System.Timers.Timer pTrayTimer = new System.Timers.Timer();

        public FrmMain()
        {
            CData.pDB = new CDB();

            InitializeComponent();

            CData.pDB.InitDatabase();

            
            LoadSetting();

        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            CLog.LOG(LOG_TYPE.SCREEN, "Agent Start");
            Agent_Base_Set();

            Control_Set();

            Agent_Action_Set();

            niAgent.ContextMenuStrip = cmsAG;

            btnGTUP.Click += btnGT_CMD_Click;
            btnGTDN.Click += btnGT_CMD_Click;
            btnGTFIX.Click += btnGT_CMD_Click;
            btnGTUnFIX.Click += btnGT_CMD_Click;
            btnGTRESET.Click += btnGT_CMD_Click;

            txtMgStart.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd|00:00:00");
            txtMgEnd.Text = DateTime.Now.AddDays(+1).ToString("yyyy-MM-dd|23:59:59");

            CData.ucReg.txtRegStart.Text = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
            CData.ucReg.txtRegEnd.Text = DateTime.Now.AddDays(+1).ToString("yyyy-MM-dd 23:59:59");
            pSDBTimer.Interval += 1000; 
            pSDBTimer.Elapsed += new System.Timers.ElapsedEventHandler(SdbTimer_Work);
            pSDBTimer.Start();
            if (CData.garOpt1[7] == 0)
            {

                pTPMSTimer.Interval += 3000;
                pTPMSTimer.Elapsed += new System.Timers.ElapsedEventHandler(TpmsTimer_Work);
                pTPMSTimer.Start();

                pWSTimer.Interval += 5000;
                pWSTimer.Elapsed += new System.Timers.ElapsedEventHandler(WSTimer_Work);
                pWSTimer.Start();

                pStatTimer.Interval += 5000;
                pStatTimer.Elapsed += new System.Timers.ElapsedEventHandler(Agent_Stat);
                pStatTimer.Start();

                pTrayTimer.Interval += 1000;
                pTrayTimer.Elapsed += new System.Timers.ElapsedEventHandler(Auto_Tray);
                pTrayTimer.Start();
            }


            if (CData.garOpt1[0] == 1)
                stripAgentMain.Text = "정기권";
        }

        private void REG_UPLOAD_MAIN(string sCarno, string sStart, string sEnd, string sRegDiv, string sGroupNm, string sUserNm, string sTelno, string sArea, string sAreaArray)
        {
            try
            {
                pProc[0].pTPMS.ALL_INSERT(sCarno, sStart, sEnd, sRegDiv, sGroupNm, sUserNm, sTelno, sArea, sAreaArray);
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }
        private void Agent_Combine(int nIdx)
        {
            if (pProc[nIdx] == null)
                return;

            //pProc[nIdx].Connect_Combine(nIdx, );
            

        }

        private void Agent_Base_Set()
        {
            CFunc.CheckDir(CData.sSetDir);
            if (!File.Exists(CData.sTpmsPath)) //파일 존재하지 않음면
            {
                
                Agent_Tpms_Base_Set();
            }
            if (!File.Exists(CData.sDBPath)) //파일 존재하지 않음면
            {
                Agent_DB_Base_Set();
            }

            if (!File.Exists(CData.sTpmsIDPath)) //파일 존재하지 않음면
            {
                Agent_TpmsID_Base_Set();
            }
            if (!File.Exists(CData.sLPRPath)) //파일 존재하지 않음면
            {
                Agent_LPR_Base_Set();
            }
            if (!File.Exists(CData.sRMPath)) //파일 존재하지 않음면
            {
                Agent_RM_Base_Set();
            }

            pProc = new CProc[10];

            CData.ucReg = new UC_Reg_Lst();
            CData.ucReg.dfRegDown = Main_Reg_Down;
            CData.ucReg.dfRegUpload = Main_Reg_Select;

            CData.ucTpms = new UC_Tpms_Lst();
            CData.ucMssql = new UC_Mssql_Lst();

            if (CData.ucTpms != null)
            {
                groupBox1.Controls.Add(CData.ucTpms);

                CData.ucTpms.Visible = true;
                CData.ucTpms.Location = new Point(336, 15);
            }
            if (CData.ucMssql != null)
            {
                groupBox2.Controls.Add(CData.ucMssql);
                CData.ucMssql.Visible = true;
                CData.ucMssql.Location = new Point(335, 15);
            }

            CFunc.CheckDir(CData.sDataDir);
        }

        private void Main_Reg_Select()
        {
            try
            {
                if (CData.pDB != null)
                    CData.pDB.Select_Reg();
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }
        private void Agent_LPR_Base_Set()
        {
            //                public int nNetType;
            //public string sIP;
            //public int nPort;
            //public int nIOType;
            //public string sID;
            for (int i = 0; i < 10; i++)
            {
                CIni.Save("ADA_LPR_"+ i.ToString(), "LPRNet", "0", CData.sLPRPath);
                CIni.Save("ADA_LPR_"+ i.ToString(), "LPRIP", "127.0.0.1", CData.sLPRPath);
                CIni.Save("ADA_LPR_"+ i.ToString(), "LPRPORT", "80", CData.sLPRPath);
                CIni.Save("ADA_LPR_"+ i.ToString(), "LPRIO", "0", CData.sLPRPath);
                CIni.Save("ADA_LPR_"+ i.ToString(), "LPRID", "0", CData.sLPRPath);
                CIni.Save("ADA_LPR_" + i.ToString(), "LPREqpm", "0", CData.sLPRPath);
                CIni.Save("ADA_LPR_" + i.ToString(), "LPRFolder", "C", CData.sLPRPath);
                CIni.Save("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath);
            }
        }

        private void Agent_Tpms_Base_Set()
        {
            CIni.Save("ADA_TPMS", "TpmsIP", "127.0.0.1", CData.sTpmsPath);
            CIni.Save("ADA_TPMS", "TpmsPORT", "80", CData.sTpmsPath);
            CIni.Save("ADA_TPMS", "TpmsType", "1", CData.sTpmsPath);

            //for (int i = 1; i < 10; i++)
            //{
            //    CIni.Save("ADA_TPMS_USE_1", "TpmsID", "ID", CData.sIniPath);
            //    CIni.Save("ADA_TPMS_USE_1", "bUse", "0", CData.sIniPath);
            //}
        }
        private void Agent_TpmsID_Base_Set()
        {
            //CIni.Save("ADA_TPMS_USE", "CNT", "0", CData.sTpmsIDPath);
            for (int i = 0; i < 10; i++)
            {
                CIni.Save("ADA_TPMS_USE_"+i.ToString(), "ID", "Muin", CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + i.ToString(), "SecID", "0", CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + i.ToString(), "LotArea", "0", CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + i.ToString(), "Use", "0", CData.sTpmsIDPath);
            }
        }
        private void Agent_DB_Base_Set()
        {
            CIni.Save("ADA_DB", "DBIP", "127.0.0.1", CData.sDBPath);
            CIni.Save("ADA_DB", "DBPort", "443", CData.sDBPath);
            //CIni.Save("ADA_DB", "D", "orcl", CData.sSetDir);
            CIni.Save("ADA_DB", "DBID", "sa", CData.sDBPath);
            CIni.Save("ADA_DB", "DBPW", "c1441", CData.sDBPath);
            CIni.Save("ADA_DB", "BASEDB", "DH", CData.sDBPath);
            CIni.Save("ADA_DB", "OPERDB", "DH", CData.sDBPath);

            CIni.Save("ADA_DB_TABLE", "InOut", "tbl", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Pay", "tbl", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Reg", "tbl", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Discount", "tbl", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Locate", "column", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Fran", "column", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Terminal", "column", CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "AirPort", "column", CData.sDBPath);

            CIni.Save("ADA_DB_ORACLE", "OracleIP", "127.0.0.1", CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OraclePort", "443", CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OracleSrc", "DH", CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OracleID", "sa", CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OraclePW", "c1441", CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "BASEDB", "DH", CData.sDBPath);
        }

        private void Agent_RM_Base_Set()
        {
            for (int i = 0; i < 10; i++)
            {
                CIni.Save("ADA_RM_"+ i.ToString(), "RMNet", "0", CData.sRMPath);
                CIni.Save("ADA_RM_"+ i.ToString(), "RMIP", "127.0.0.1", CData.sRMPath);
                CIni.Save("ADA_RM_"+ i.ToString(), "RMPort", "80", CData.sRMPath);
                CIni.Save("ADA_RM_"+ i.ToString(), "RMIO", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath);
                CIni.Save("ADA_RM_"+ i.ToString(), "RMUp", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "RMDn", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "RMFix", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "RMUnFix", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "RMReset", "0", CData.sRMPath);
                CIni.Save("ADA_RM_" + i.ToString(), "Use", "0", CData.sRMPath);
            }
        }

        private void Agent_Action_Set()
        {
            TPMS_Set();
            TPMSID_Set();
            Remote_Set();
            DB_Set();
            ODB_Set();

        }

        private void Control_Set()
        {
            btnSetting.Click += btnSetting_Click;

            lblLPR000.Visible = false;
            lblLPR001.Visible = false;
            lblLPR002.Visible = false;
            lblLPR003.Visible = false;
            lblLPR004.Visible = false;

            lblLPR100.Visible = false;
            lblLPR101.Visible = false;
            lblLPR102.Visible = false;
            lblLPR103.Visible = false;
            lblLPR104.Visible = false;

            lblLPR200.Visible = false;
            lblLPR201.Visible = false;
            lblLPR202.Visible = false;
            lblLPR203.Visible = false;
            lblLPR204.Visible = false;

            lblLPR300.Visible = false;
            lblLPR301.Visible = false;
            lblLPR302.Visible = false;
            lblLPR303.Visible = false;
            lblLPR304.Visible = false;

            lblLPR400.Visible = false;
            lblLPR401.Visible = false;
            lblLPR402.Visible = false;
            lblLPR403.Visible = false;
            lblLPR404.Visible = false;

            lblLPR500.Visible = false;
            lblLPR501.Visible = false;
            lblLPR502.Visible = false;
            lblLPR503.Visible = false;
            lblLPR504.Visible = false;

            lblLPR600.Visible = false;
            lblLPR601.Visible = false;
            lblLPR602.Visible = false;
            lblLPR603.Visible = false;
            lblLPR604.Visible = false;

            grpLPR0.Visible = false;
            //grpLPR1.Visible = false;
            //grpLPR2.Visible = false;
            //grpLPR2.Visible = false;
            //grpLPR3.Visible = false;
            //grpLPR4.Visible = false;
            //grpLPR5.Visible = false;
            //grpLPR6.Visible = false;
        }


        private void TPMS_Set()
        {
            CData.sTpmsIP = CIni.Load("ADA_TPMS", "TpmsIP", "0  ", CData.sTpmsPath);
            CData.sTpmsPort = CIni.Load("ADA_TPMS", "TpmsPORT", "0", CData.sTpmsPath);
            CData.nTpmsType = int.Parse(CIni.Load("ADA_TPMS", "TpmsType", "0", CData.sTpmsPath));

            txtTPMSIP.Text = CData.sTpmsIP;
            txtTPMSPORT.Text = CData.sTpmsPort;
        }

        private void TPMSID_Set()
        {
            try
            {
                string sMenuNm = "";
                if (CData.garOpt1[5] == 1)
                {
                    //thread = new tread[10];
                    for (int i = 0; i < 10; i++)
                    {
                        int nChk = 0;
                        int nGTCnt = 0;
                        
                        if ((CIni.Load("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath) == "1") ? true : false)
                        {

                            //if ((CIni.Load("ADA_LPR_" + i.ToString(), "LPRIO", "0", CData.sLPRPath) == "0") ? true : false)
                            //{

                                pProc[i] = new CProc();

                                pProc[i].st_TpmsInfo_Proc.sID = CIni.Load("ADA_LPR_" + i.ToString(), "LPRID", "0", CData.sLPRPath);
                                for (int a = 0; a < 10; a++)
                                {

                                    if (pProc[i].st_TpmsInfo_Proc.sID == CIni.Load("ADA_TPMS_USE_" + a.ToString(), "ID", "0", CData.sTpmsIDPath))
                                    {
                                        pProc[i].st_TpmsInfo_Proc.sSecID = CIni.Load("ADA_TPMS_USE_" + a.ToString(), "SecID", "0", CData.sTpmsIDPath);
                                    
                                    if (i == 0)
                                        sMenuNm = pProc[i].st_TpmsInfo_Proc.sID;
                                    else
                                        sMenuNm += ", " + pProc[i].st_TpmsInfo_Proc.sID;

                                    pProc[i].st_TpmsInfo_Proc.sLotArea = CIni.Load("ADA_TPMS_USE_" + a.ToString(), "LotArea", "0", CData.sTpmsIDPath);
                                    pProc[i].st_TpmsInfo_Proc.bUse = (CIni.Load("ADA_TPMS_USE_" + a.ToString(), "Use", "0", CData.sTpmsIDPath) == "1") ? true : false;
                                        pProc[i].st_TpmsInfo_Proc.stPkInfo.nParkCount = 100;
                                        pProc[i].st_TpmsInfo_Proc.stPkInfo.bStatus = false;
                                        pProc[i].st_TpmsInfo_Proc.bUse = true;
                                        //pProc[i].dfTPMSProc = WriteServerLog;
                                        pProc[i].dfStackCnt = Agent_Stack_CntChk;
                                        pProc[i].dfProcWs = WS_Proc_InMain;
                                    if (i == 0)
                                        txtTPMSID.Text = pProc[i].st_TpmsInfo_Proc.sID;
                                    else
                                        txtTPMSID.Text += ", " + pProc[i].st_TpmsInfo_Proc.sID;
                                    txtTPMSSEC.Text = pProc[i].st_TpmsInfo_Proc.sSecID;

                                        for (int b = 0; b < 10; b++)
                                        {
                                            if (CIni.Load("ADA_LPR_" + b.ToString(), "LPRID", "0", CData.sLPRPath) == CIni.Load("ADA_TPMS_USE_" + a.ToString(), "ID", "0", CData.sTpmsIDPath))
                                                pProc[i].nNetLPRCnt++;
                                        }
                                    }



                                    //Agent_Combine(i);
                                }
                            pProc[i].st_NetInfo_LPR_Proc = new NetInfo[pProc[i].nNetLPRCnt + 1];
                            pProc[i].st_NetInfo_GT_Proc = new NetInfo[pProc[i].nNetLPRCnt];
                            for (int b = 0; b < pProc[i].nNetLPRCnt; b++)
                            {

                                pProc[i].st_NetInfo_LPR_Proc[b] = new NetInfo();
                                pProc[i].st_NetInfo_GT_Proc[b] = new NetInfo();
                                for (int a = 0; a < 10; a++)
                                {
                                    if (CIni.Load("ADA_RM_" + a.ToString(), "Use", "0", CData.sRMPath) == "1" )
                                    {
                                        if (CIni.Load("ADA_LPR_" + i.ToString(), "LPRID", "0", CData.sLPRPath) == CIni.Load("ADA_RM_" + a.ToString(), "RMMatch", "0", CData.sRMPath))
                                        {
                                            pProc[i].st_NetInfo_GT_Proc[b].nType = int.Parse(CIni.Load("ADA_RM_" + a.ToString(), "RMNet", "0", CData.sRMPath));
                                            pProc[i].st_NetInfo_GT_Proc[b].sIP = CIni.Load("ADA_RM_" + a.ToString(), "RMIP", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].nPort = int.Parse(CIni.Load("ADA_RM_" + a.ToString(), "RMPORT", "0", CData.sRMPath));
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sMatchID = CIni.Load("ADA_RM_" + a.ToString(), "RMMatch", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.nIOType = int.Parse(CIni.Load("ADA_RM_" + a.ToString(), "RMIO", "0", CData.sRMPath));
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sUp = CIni.Load("ADA_RM_" + a.ToString(), "RMUp", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sDn = CIni.Load("ADA_RM_" + a.ToString(), "RMDn", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sFix = CIni.Load("ADA_RM_" + a.ToString(), "RMFix", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sUnFix = CIni.Load("ADA_RM_" + a.ToString(), "RMUnFix", "0", CData.sRMPath);
                                            pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.sReset = CIni.Load("ADA_RM_" + a.ToString(), "RMReset", "0", CData.sRMPath);

                                            pProc[i].st_NetInfo_GT_Proc[b].bUse = true;
                                        }
                                    }

                                    if (pProc[i].st_TpmsInfo_Proc.sID == CIni.Load("ADA_LPR_" + a.ToString(), "LPRID", "0", CData.sLPRPath))
                                    {
                                        pProc[i].st_NetInfo_LPR_Proc[b].nType = int.Parse(CIni.Load("ADA_LPR_" + a.ToString(), "LPRNet", "0", CData.sLPRPath));
                                        pProc[i].st_NetInfo_LPR_Proc[b].sIP = CIni.Load("ADA_LPR_" + a.ToString(), "LPRIP", "0", CData.sLPRPath);
                                        pProc[i].st_NetInfo_LPR_Proc[b].nPort = int.Parse(CIni.Load("ADA_LPR_" + a.ToString(), "LPRPORT", "0", CData.sLPRPath));
                                        pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nIOType = int.Parse(CIni.Load("ADA_LPR_" + a.ToString(), "LPRIO", "0", CData.sLPRPath));

                                        pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.sFolder = CIni.Load("ADA_LPR_" + a.ToString(), "LPRFolder", "0", CData.sLPRPath);
                                        pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nEqpm = int.Parse(CIni.Load("ADA_LPR_" + a.ToString(), "LPREqpm", "0", CData.sLPRPath));
                                        pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.bUse = true;
                                        pProc[i].st_LprInfo.sFolder = CIni.Load("ADA_LPR_" + a.ToString(), "LPRFolder", "0", CData.sLPRPath);
                                        pProc[i].st_LprInfo.nEqpm = int.Parse(CIni.Load("ADA_LPR_" + a.ToString(), "LPREqpm", "", CData.sLPRPath));
                                        pProc[i].st_LprInfo.bUse = true;

                                        if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nIOType == 0 && CData.nInProcIdx == 999)
                                        {
                                            CData.nInProcIdx = i;
                                            CLog.LOG(LOG_TYPE.SCREEN, "InProcIdx=" + CData.nInProcIdx.ToString() + " SET");
                                        }
                                        else if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nIOType == 1 && CData.nOutProcIdx == 999)
                                        {
                                            CData.nOutProcIdx = i;
                                            CLog.LOG(LOG_TYPE.SCREEN, "OutProcIdx=" + CData.nOutProcIdx.ToString() + " SET");
                                        }
                                        //pProc[i].st_NetInfo_LPR_Proc.
                                        //pProc[i].stLPRInfo.bUse = true;

                                        pProc[i].dfLPRStat = LPR_Stat;
                                        //pProc[i].dfLPRProc = WriteLPRLog;
                                        //pProc[i].dfDBProc = WriteDBLog;
                                        //pProc[i].dfODBProc = WriteODBLog;
                                        pProc[i].dfGTProc = WriteGTLog;
                                        pProc[i].dfProcMig = Migration_InMain;
                                        pProc[i].dfProcMigOut = Migration_Out_InMain;
                                        pProc[i].dfProcMigPass = Migration_Pass_InMain;
                                        //pProc[i].dfIOCHKProc = IO_CHK_Main;
                                        //lblChk.Add(new List());
                                        //lblChk[b].ForeColor = System.Drawing.Color.Green;




                                    }

                                }
                            }
                            if (CData.garOpt1[7] == 1)
                            {
                                Thread threads = new Thread(new ThreadStart(() => pProc[i].Connect_Combine(i)));
                                threads.Start();
                                threads.Join();
                            }
                            else
                                pProc[i].Connect_Combine(i);

                            bPrevStat_TPMS[i] = false;
                            bPrevStat_MSSQL[i] = false;
                            //pProc[i].dfAllSendStat = Agent_Stat;
                                //pProc[i].df_Send_Reg = CData.ucReg.Lsv_Show;
                                pProc[i].nNetLPRCnt = 0;

                                nCnt++;
                            //}
                            
                        }
                    }
                }
                else
                {
                    pProc[0] = new CProc();
                    pProc[0].st_TpmsInfo_Proc.sID = CIni.Load("ADA_TPMS_USE_0", "ID", "0", CData.sTpmsIDPath);
                    pProc[0].st_TpmsInfo_Proc.sSecID = CIni.Load("ADA_TPMS_USE_0", "SecID", "0", CData.sTpmsIDPath);
                    pProc[0].st_TpmsInfo_Proc.sLotArea = CIni.Load("ADA_TPMS_USE_0", "LotArea", "0", CData.sTpmsIDPath);
                    pProc[0].st_TpmsInfo_Proc.bUse = (CIni.Load("ADA_TPMS_USE_0", "Use", "0", CData.sTpmsIDPath) == "1") ? true : false;
                    pProc[0].st_TpmsInfo_Proc.stPkInfo.nParkCount = 100;
                    pProc[0].st_TpmsInfo_Proc.stPkInfo.bStatus = false;
                    pProc[0].st_TpmsInfo_Proc.bUse = true;
                    //pProc[0].dfTPMSProc = WriteServerLog;
                    pProc[0].dfStackCnt = Agent_Stack_CntChk;
                    txtTPMSID.Text = pProc[0].st_TpmsInfo_Proc.sID;
                    txtTPMSSEC.Text = pProc[0].st_TpmsInfo_Proc.sSecID;
                    //pProc[0].dfDBProc = WriteDBLog;
                    pProc[0].Connect_Combine(0);
                    //pProc[0].dfAllSendStat = Agent_Stat;
                    CData.pDB.dfDBUPLOAD = REG_UPLOAD_MAIN;
                    pProc[0].dfRegUpProc = Main_Reg_Select;

                    sMenuNm = pProc[0].st_TpmsInfo_Proc.sID;


                    //pProc[0].df_Send_Reg = 

                }

                stripAgentMain.Text = sMenuNm;
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "TPMS_ID_Set-Err" + ex.ToString());
            }
            finally
            {
            }

        }

        private void Combine_Main(int nIdx)
        {
            pProc[nIdx].Connect_Combine(nIdx);
        }

        private void LPR_Stat(int nIdx_Div, int nIdx, bool bStat, string sIOType)
        {
            this.Invoke(new Action(delegate ()
            {
                lblChk[nIdx_Div, nIdx] = new Label();

                if (bStat)
                    lblChk[nIdx_Div, nIdx].ForeColor = System.Drawing.Color.Green;
                else
                    lblChk[nIdx_Div, nIdx].ForeColor = System.Drawing.Color.Red;

                //switch()

                switch (nIdx_Div)
                {
                    case 0:
                        grpLPR0.Visible = true;
                        switch (nIdx)
                        {
                            case 0:
                                lblLPR000.Visible = true;
                                lblLPR000.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR000.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 1:
                                lblLPR001.Visible = true;
                                lblLPR001.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR001.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 2:
                                lblLPR002.Visible = true;
                                lblLPR002.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR002.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 3:
                                lblLPR003.Visible = true;
                                lblLPR003.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR003.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 4:
                                lblLPR004.Visible = true;
                                lblLPR004.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR004.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                        }
                        break;
                    case 1:
                        switch (nIdx)
                        {
                            case 0:
                                lblLPR100.Visible = true;
                                lblLPR100.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR100.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 1:
                                lblLPR101.Visible = true;
                                lblLPR101.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR101.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 2:
                                lblLPR102.Visible = true;
                                lblLPR102.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR102.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 3:
                                lblLPR103.Visible = true;
                                lblLPR103.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR103.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 4:
                                lblLPR104.Visible = true;
                                lblLPR104.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR104.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 2:
                        switch (nIdx)
                        {
                            case 0:
                                lblLPR200.Visible = true;
                                lblLPR200.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR200.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 1:   
                                lblLPR201.Visible = true;
                                lblLPR201.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR201.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 2:   
                                lblLPR202.Visible = true;
                                lblLPR202.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR202.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 3:   
                                lblLPR203.Visible = true;
                                lblLPR203.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR203.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 4:   
                                lblLPR204.Visible = true;
                                lblLPR204.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR204.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:
                        switch (nIdx)
                        {
                            case 0:
                                lblLPR300.Visible = true;
                                lblLPR300.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR300.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 1:   
                                lblLPR301.Visible = true;
                                lblLPR301.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR301.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 2:   
                                lblLPR302.Visible = true;
                                lblLPR302.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR302.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 3:   
                                lblLPR303.Visible = true;
                                lblLPR303.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR303.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 4:   
                                lblLPR304.Visible = true;
                                lblLPR304.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR304.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 4:
                        switch (nIdx)
                        {
                            case 0:
                                lblLPR400.Visible = true;
                                lblLPR400.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR400.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 1:   
                                lblLPR401.Visible = true;
                                lblLPR401.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR401.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 2:   
                                lblLPR402.Visible = true;
                                lblLPR402.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR402.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 3:   
                                lblLPR403.Visible = true;
                                lblLPR403.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR403.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            case 4:
                                lblLPR404.Visible = true;
                                lblLPR404.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                                lblLPR404.Text = "#" + nIdx_Div.ToString() + "#" + nIdx.ToString() + sIOType;
                                break;
                            default:
                                break;
                        }
                        break;
                    //06.16 나중에 ID별 분류작업 예정
                    //case 1:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR100.Visible = true;
                    //            lblLPR100.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:
                    //            lblLPR101.Visible = true;
                    //            lblLPR101.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:
                    //            lblLPR102.Visible = true;
                    //            lblLPR102.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:
                    //            lblLPR103.Visible = true;
                    //            lblLPR103.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:
                    //            lblLPR104.Visible = true;
                    //            lblLPR104.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    //case 2:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR200.Visible = true;
                    //            lblLPR200.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:   
                    //            lblLPR201.Visible = true;
                    //            lblLPR201.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:   
                    //            lblLPR202.Visible = true;
                    //            lblLPR202.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:   
                    //            lblLPR203.Visible = true;
                    //            lblLPR203.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:   
                    //            lblLPR204.Visible = true;
                    //            lblLPR204.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    //case 3:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR300.Visible = true;
                    //            lblLPR300.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:   
                    //            lblLPR301.Visible = true;
                    //            lblLPR301.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:   
                    //            lblLPR302.Visible = true;
                    //            lblLPR302.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:   
                    //            lblLPR303.Visible = true;
                    //            lblLPR303.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:   
                    //            lblLPR304.Visible = true;
                    //            lblLPR304.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    //case 4:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR400.Visible = true;
                    //            lblLPR400.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:   
                    //            lblLPR401.Visible = true;
                    //            lblLPR401.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:   
                    //            lblLPR402.Visible = true;
                    //            lblLPR402.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:   
                    //            lblLPR403.Visible = true;
                    //            lblLPR403.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:   
                    //            lblLPR404.Visible = true;
                    //            lblLPR404.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    //case 5:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR500.Visible = true;
                    //            lblLPR500.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:   
                    //            lblLPR501.Visible = true;
                    //            lblLPR501.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:   
                    //            lblLPR502.Visible = true;
                    //            lblLPR502.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:   
                    //            lblLPR503.Visible = true;
                    //            lblLPR503.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:   
                    //            lblLPR504.Visible = true;
                    //            lblLPR504.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    //case 6:
                    //    switch (nIdx)
                    //    {
                    //        case 0:
                    //            lblLPR600.Visible = true;
                    //            lblLPR600.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 1:   
                    //            lblLPR601.Visible = true;
                    //            lblLPR601.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 2:   
                    //            lblLPR602.Visible = true;
                    //            lblLPR602.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 3:   
                    //            lblLPR603.Visible = true;
                    //            lblLPR603.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        case 4:   
                    //            lblLPR604.Visible = true;
                    //            lblLPR604.ForeColor = lblChk[nIdx_Div, nIdx].ForeColor;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;

                    default:
                        break;
                
            }
            }));
               

        }

        private void DB_Set()
        {
            txtDBIP.Text = CIni.Load("ADA_DB", "DBIP", "0", CData.sDBPath);
            txtDBPORT.Text = CIni.Load("ADA_DB", "DBPort", "0", CData.sDBPath);
            txtBASEDB.Text = CIni.Load("ADA_DB", "BASEDB", "0", CData.sDBPath);
            txtOPERDB.Text = CIni.Load("ADA_DB", "OPERDB", "0", CData.sDBPath);
        }

        private void ODB_Set()
        {

            //CIni.Save("ADA_DB_ORACLE", "OracleIP", "127.0.0.1", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePort", "443", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleSrc", "DH", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleID", "sa", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePW", "c1441", CData.sDBPath);
            txtODBIP.Text = CIni.Load("ADA_DB_ORACLE", "OracleIP", "0", CData.sDBPath);
            txtODBPORT.Text = CIni.Load("ADA_DB_ORACLE", "OraclePort", "0", CData.sDBPath);
            txtODBSRC.Text = CIni.Load("ADA_DB_ORACLE", "OracleSrc", "0", CData.sDBPath);
            txtBASEODB.Text = CIni.Load("ADA_DB_ORACLE", "BASEDB", "0", CData.sDBPath);
        }

        private void Remote_Set()
        {
            int nCnt = 0;
            
            for(int i = 0; i < 10; i++)
            {
                //sGt_Use[i] = new string();
                sGt_Use[i] = "False";
                if (CIni.Load("ADA_RM_" + i.ToString(), "Use", "0", CData.sRMPath) == "1")
                {
                    cmbGTList.Items.Add(((CIni.Load("ADA_RM_" + i.ToString(), "RMIO", "0", CData.sRMPath) == "0") ? "입구" : "출구")+ "_" + (CIni.Load("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath).Substring(CIni.Load("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath).Length - 1)));
                    sGt_Use[i] = CIni.Load("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath);
                    nCnt++;
                }
            }

            if(nCnt == 0)
            {
                cmbGTList.Items.Add("No List");
                
            }
            cmbGTList.SelectedIndex = 0;

        }

        private void Agent_Stack_CntChk()
        {
            this.Invoke(new Action(delegate ()
            {
                try
                {


                    lblStackCnt.Text = "S : " + CData.nStackCnt.ToString();
                }
                catch (Exception)
                {

                }
                finally
                {
                }
            }));
        }

        //private void Agent_Stat(int nDiv, bool bChk)

        private void Agent_Stat(object sender, System.Timers.ElapsedEventArgs e)
        {

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (pProc[i] != null)
                    {
                        if (pProc[i].st_TpmsInfo_Proc.bUse)
                        {
                            if (pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus != bPrevStat_TPMS[i])
                            {
                                if (pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                                {
                                    lblTPMS.Image = Properties.Resources.icon_blue;

                                }
                                else
                                {
                                    lblTPMS.Image = Properties.Resources.icon_red;
                                }
                            }

                            bPrevStat_TPMS[i] = pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus;
                        }


                        if (CData.garOpt1[5] == 1)
                        {
                            if (pProc[i].pMSSQL != null)
                            {
                                if (pProc[i].pMSSQL.pDBInfo.bStatus[0] != bPrevStat_MSSQL[i])
                                {
                                    if (pProc[i].pMSSQL.pDBInfo.bStatus[0])
                                    {
                                        lblDB.Image = Properties.Resources.icon_blue;
                                    }
                                    else
                                    {
                                        lblDB.Image = Properties.Resources.icon_red;
                                    }

                                }

                                bPrevStat_MSSQL[i] = pProc[i].pMSSQL.pDBInfo.bStatus[0];
                            }
                        }
                        else
                        {
                            if (CData.pMSSQL != null)
                            {
                                if (CData.pMSSQL.pDBInfo.bStatus[0] != bPrevStat_MSSQL[i])
                                {
                                    if (CData.pMSSQL.pDBInfo.bStatus[0])
                                    {
                                        lblDB.Image = Properties.Resources.icon_blue;
                                    }
                                    else
                                    {
                                        lblDB.Image = Properties.Resources.icon_red;
                                    }

                                }

                                bPrevStat_MSSQL[i] = CData.pMSSQL.pDBInfo.bStatus[0];
                            }
                        }

                    }
                    
                }
                catch (Exception)
                {

                    throw;
                }

                //switch (nDiv)
                //{
                //    case 0:
                //        if (bChk)
                //            lblTPMS.Image = Properties.Resources.icon_blue;
                //        else
                //            lblTPMS.Image = Properties.Resources.icon_red;
                //        break;
                //    case 1:
                //        if (bChk)
                //            lblDB.Image = Properties.Resources.icon_blue;
                //        else
                //            lblDB.Image = Properties.Resources.icon_red;
                //        break;
                //    case 2:
                //        //if (bChk)
                //        //    lblODB.Image = Properties.Resources.icon_blue;
                //        //else
                //        //    lblODB.Image = Properties.Resources.icon_red;
                //        break;
                //}

            }

            if (nLsvCnt > 120)
            {
                CData.ucTpms.Lsv_Clear();
                CData.ucMssql.Lsv_Clear();

                CLog.LOG(LOG_TYPE.SCREEN, "Lsv_Clear");
                nLsvCnt = 0;

            }

            nLsvCnt++;

            //this.Invoke(new Action(delegate ()
            //{
            //    switch (nDiv)
            //    {
            //        case 0:
            //            if (bChk)
            //                lblTPMS.Image = Properties.Resources.icon_blue;
            //            else
            //                lblTPMS.Image = Properties.Resources.icon_red;
            //            break;
            //        case 1:
            //            if (bChk)
            //                lblDB.Image = Properties.Resources.icon_blue;
            //            else
            //                lblDB.Image = Properties.Resources.icon_red;
            //            break;
            //        case 2:
            //            if (bChk)
            //                lblODB.Image = Properties.Resources.icon_blue;
            //            else
            //                lblODB.Image = Properties.Resources.icon_red;
            //            break;
            //    }
            //}));
        }

        //private void WriteServerLog(string sID, string sLog)
        //{
        //    this.Invoke(new MethodInvoker(delegate ()
        //    {
        //        string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sID, sLog };
        //        ListViewItem lviAdd = new ListViewItem(sLsvItem);
        //        lsvSvLog.Items.Insert(0, lviAdd);
        //    }));
        //}
        //private void WriteDBLog(string sDB, string sLog)
        //{
        //    this.Invoke(new MethodInvoker(delegate ()
        //    {
        //        string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sDB, sLog };
        //        ListViewItem lviAdd = new ListViewItem(sLsvItem);
        //        lsvDBLog.Items.Insert(0, lviAdd);
        //    }));
        //}

        //private void WriteODBLog(string sDB, string sLog)
        //{
        //    this.Invoke(new MethodInvoker(delegate ()
        //    {
        //        string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sDB, sLog };
        //        ListViewItem lviAdd = new ListViewItem(sLsvItem);
        //        lsvODBLog.Items.Insert(0, lviAdd);
        //    }));
        //}

        //private void WriteLPRLog(string sDiv, string sLog)
        //{
        //    this.Invoke(new Action(delegate ()
        //    {
        //        string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sDiv, sLog };
        //        ListViewItem lviAdd = new ListViewItem(sLsvItem);
        //        lsvLPRLog.Items.Insert(0, lviAdd);
        //        //this.Invoke.
        //    }));

        //}
        private void WriteGTLog(string sMatch, bool bIO, string sLog)
        {
            this.Invoke(new Action(delegate ()
            {
                string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sMatch, ((bIO) ? "입구" : "출구") + " " + sLog };
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvGTLog.Items.Insert(0, lviAdd);
            }));
        }

        private void Migration_InMain(int nEqpm, string sInCarno, string sInDttm, string sPath)
        {
            bool bPass = false;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    
                    if (pProc[i] != null && pProc[i].st_TpmsInfo_Proc.bUse)
                    {

                        for (int b = 0; b < 10; b++)
                        {

                            if (CIni.Load("ADA_LPR_" + b.ToString(), "Use", "0", CData.sLPRPath) == "1")
                            {
                                if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.bUse == true)
                                {
                                    if (nEqpm == 0)
                                        nEqpm = pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nEqpm;
                                    if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nEqpm == nEqpm)
                                    {
                                        string sFull = "";
                                        string[] sRcv = new string[10];
                                        if (sPath != "")
                                        {
                                            string[] sImgPath = sPath.Split('_');
                                            sFull = pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.sFolder + @":\" + sImgPath[1].Substring(0, 8) + @"\" + sPath;
                                            
                                        }
                                        pProc[i].Migration_SendTPMS(sInCarno, sInDttm, sFull, sPath, ref sRcv);
                                        
                                    }
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

            }
            CData.bMig = false;
        }

        private void Migration_Out_InMain(string sCarno, int nFee, string sApproval, string sDttm, int nID, string sFile, string sOutEqpm, int nService)
        {
            bool bPass = false;
            string sFull = "";
            CLog.LOG(LOG_TYPE.SCREEN, "Out Main #0");
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (pProc[i] != null && pProc[i].st_TpmsInfo_Proc.bUse)
                    {
                        for (int b = 0; b < 1; b++)
                        {

                            if (CIni.Load("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath) == "1")
                            {
                                if (pProc[i].pStation[b] != null && pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.bUse == true)
                                {
                                    if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nEqpm == int.Parse(sOutEqpm))
                                    {
                                        CLog.LOG(LOG_TYPE.SCREEN, "Out Main #1");

                                        if (CData.garOpt1[1] == 0)
                                        {
                                            //sFull = "";!
                                            if (sFile != "")
                                            {
                                                string[] sImgPath = sFile.Split('_');
                                                sFull = pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.sFolder + @":\" + sImgPath[1].Substring(0, 8) + @"\" + sFile;
                                            }
                                            string[] sRcv = new string[10];
                                        }
                                        else
                                            sFull = "";
                                        pProc[i].Migration_Out_SendTPMS(sCarno, nFee, sApproval, sDttm, nID, sFull, sFile, sOutEqpm, nService);
                                        bPass = true;
                                    }
                                }
                            }
                        }
                        if (bPass)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, ex.ToString());
                }
                finally
                {
                }

            }
            CData.bMig = false;
        }

        private void Migration_Pass_InMain(string sCarno, string sDttm, string sFile, string sOutEqpm)
        {
            bool bPass = false;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (pProc[i] != null && pProc[i].st_TpmsInfo_Proc.bUse)
                    {
                        for (int b = 0; b < 1; b++)
                        {

                            if (CIni.Load("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath) == "1")
                            {
                                if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.bUse == true)
                                {
                                    if (pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.nEqpm == int.Parse(sOutEqpm))
                                    {
                                        string sFull = "";
                                        if (sFile != "")
                                        {
                                            string[] sImgPath = sFile.Split('_');
                                            sFull = pProc[i].st_NetInfo_LPR_Proc[b].stLPRInfo.sFolder + @":\" + sImgPath[1].Substring(0, 8) + @"\" + sFile;
                                        }
                                        pProc[i].Migration_Pass_SendTPMS(sCarno, sDttm, sFile);
                                        bPass = true;
                                    }
                                }
                            }
                        }
                        if (bPass)
                            break;


                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                }

            }
            CData.bMig = false;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            FrmSetting frmSet = new FrmSetting();
            frmSet.ShowDialog();
            frmSet.dfTPMSReSetting += TPMS_Set;
            frmSet.dfDBReSetting += DB_Set;
            return;
        
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "F1")
            {
                string sTemp = @"CH35#XXXXXXXX1300#\20220808\CH35_20220808110551_XXXXXXXX1300.jpgCH35#316러4370#\20220808\CH35_20220808110619_316러4370";
                pProc[0].pLPR.LPR_Parse("", true, sTemp, "D", false);
            }
            else if(e.KeyCode.ToString() == "F2")
            {
                string sTemp = "{ \"result\":false,\"httpcode\":401,\"msg\":\"로그인 인증 정보를 찾을 수 없습니다.\",\"httpstatus\":\"UNAUTHORIZED\"}";
                pProc[0].pTPMS.ProcResponse(TPMS_CMD.MU_OUT, sTemp);
            }
            else if (e.KeyCode.ToString() == "F3")
            {
                string sTemp = @"CH04#115서9298#\20220712\CH04_20220712075023_115서9298.jpg";
                pProc[1].pLPR.LPR_Parse("#1#0", true, sTemp, "D", true);
            }
            else if ( e.KeyCode.ToString() == "F4")
            {
                //string sDate = "D:\Img\test_out.jpg";
                //pProc[0].pTPMS.SendCmd_Builder(TPMS_CMD.IMG_UP_NORMAL_OUT, "name=file&filename=12%25ea%25b0%25801523.jpg", "12%ea%b0%801523.jpg", "D:\test_out.jpg", true);
                pProc[0].pTPMS.Cal_Mu("115서9298", 500, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "");
            }
            else if (e.KeyCode.ToString() == "F5")
            {
                try
                {


                    string sTemp = @"CH04#" + ((txtTestCarno.Text == "") ? "12가1234" : txtTestCarno.Text) + @"#\20220712\CH04_20220712075023_" + ((txtTestCarno.Text == "") ? "12가1234" : txtTestCarno.Text) + ".jpg";
                    pProc[int.Parse(((txtTestIDX.Text == "") ? "0" : txtTestIDX.Text))].pLPR.LPR_Parse("1#0", true, sTemp, "D", true);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("입차 에러 : " + ex.ToString());
                }
                finally
                {
                }
            }
            else if (e.KeyCode.ToString() == "F6")
            {
                try
                {
                    string sTemp = "CH04#" + ((txtTestCarno.Text == "") ? "12가1235" : txtTestCarno.Text) + @"#\20220712\CH04_20220712075023_" + ((txtTestCarno.Text == "") ? "12가1235" : txtTestCarno.Text) + ".jpg"; 
                    pProc[int.Parse(((txtTestIDX.Text == "") ? "2" : txtTestIDX.Text))].pLPR.LPR_Parse("#1#0", true, sTemp, "D", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("출차 에러 : " + ex.ToString());
                }
        }
            else if (e.KeyCode.ToString() == "F7")
            {
                try
                {
                    pProc[int.Parse(((txtTestIDX.Text == "") ? "0" : txtTestIDX.Text))].pTPMS.Cal_Mu(((txtTestCarno.Text == "") ? "미인식" : txtTestCarno.Text),
                        int.Parse(((txtTestMoney.Text == "") ? "0" : txtTestMoney.Text)),
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        "", false, true, ((chkPayWay.Checked) ? true : false));
                }
                catch(Exception ex)
                {
                    MessageBox.Show("수납 에러 : " + ex.ToString());
                }
            }
            else if (e.KeyCode.ToString() == "F8")
            {
                List<string> arData = new List<string>();
                //JObject jObjData;
                //jObjData = (JObject)json["data"];
                arData.Add("");               //?
                arData.Add("2022-07-10 18:46:10");            //할인금액
                arData.Add("");                  //차량번호
                arData.Add("1234567");              //입차시간
                arData.Add("카카오뱅크");             //이용금액
                arData.Add("123456**********");               //출차시간
                arData.Add("KB국민");    //입차타입
                arData.Add("");    //입차타입
                arData.Add("");    //입차타입
                arData.Add("53?6522");    //입차타입
                arData.Add("");    //입차타입
                arData.Add("600");
                arData.Add("");//입차타입
                arData.Add("0");
                arData.Add("0");

                //arData.Add(json["cardMemberNo"].ToString());               //?            0
                //arData.Add(json["bankTransDttm"].ToString());            //       1
                //arData.Add(json["cardTradeMedia"].ToString());                  //2  
                //arData.Add(json["cardApprovalNo"].ToString());              //    3
                //arData.Add(json["cardIssueCorpNm"].ToString());             //    4
                //arData.Add(json["cardNo"].ToString());               //   5
                //arData.Add(json["cardPurchCorpNm"].ToString());    // 6
                //arData.Add(json["receiptTy"].ToString());    //??   7
                //arData.Add(json["cmmsonAmt"].ToString());    //?  8
                //arData.Add(json["carNo"].ToString());    //차번   9
                //arData.Add(json["cardTradeNo"].ToString());    //?    10
                //arData.Add(json["receiptAmt"].ToString());    11
                //arData.Add(json["cardInstallmentMonth"].ToString());//?     12

                pProc[0].WS_Proc_List(pProc[0].pTPMS.st_TpmsInfo.sID, WS_CMD.STT_1_1002 ,arData);
            }
            else if (e.KeyCode.ToString() == "F9")
            {
                pProc[0].InCar_Migration_Time(txtMgStart.Text, txtMgEnd.Text);
            }
            //CH01_20220526080003_
        }
        
        private void btnFrmReg_Click(object sender, EventArgs e)
        {
            FrmReg frmReg = new FrmReg();
            frmReg.ShowDialog();
            return;
        }

        private void chkOnlyLog_CheckedChanged(object sender, EventArgs e)
        {
            CData.bLogVr = chkOnlyLog.Checked;
        }

        private void Main_Reg_Down(string sDate)
        {
            //CData.pMSSQL.DB_SELECT();

            CData.pDB.Delete_Reg();
            pProc[0].Reg_Down(sDate);

        }

        private void LoadSetting()
        {
            CFunc.CheckDir(CData.sSetDir);

            CData.pDB.Load_Opt1();
            //CData.garOpt1.Add(1);

            int i = 0;
            if (CData.garOpt1.Count < CData.garOpt1_Name.Length)
            {
                i = CData.garOpt1.Count;

                for (; i < CData.garOpt1_Name.Length; ++i)
                {
                    CData.garOpt1.Add(0);
                    CData.pDB.Insert_Opt1(i + 1, 0);
                }
            }
        }

        private void WS_Proc_InMain(string sID, bool bIO, string sCmd)
        {
            bool bGT_Pass = false;
            string sCmd_M = "";

            for (int i = 0; i < 10; i++)
            {
                
                for (int b = 0; b < 1; b++)
                {
                    try
                    {

                        if (sGt_Use[i] == sID)
                        {
                            if (CIni.Load("ADA_RM_" + i.ToString(), "Use", "0", CData.sRMPath) == "1")
                            {
                                if (pProc[i].pStation_GT[b].st_NetInfo.stGtInfo.nIOType == ((bIO) ? 0 : 1))
                                {

                                    pProc[i].pStation_GT[b].Gt_Cmd_Send(CIni.Load("ADA_RM_" + (i).ToString(), sCmd, "0", CData.sRMPath));
                                    bGT_Pass = true;
                                    WriteGTLog("#" + i.ToString() + "#" + b.ToString(), ((pProc[i].pStation_GT[b].st_NetInfo.stGtInfo.nIOType == 0) ? true : false), "TX : " + sCmd);
                                    break;
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
                }
                if (bGT_Pass)
                    break;
            }
        }

        private void btnGT_CMD_Click(object sender, EventArgs e)
        {
            bool bGT_Pass = false;
            string sCmd = "";

            if (sender == btnGTUP)
                sCmd = "RMUp";
            else if (sender == btnGTDN)
                sCmd = "RMDn";
            else if (sender == btnGTFIX)
                sCmd = "RMFix";
            else if (sender == btnGTUnFIX)
                sCmd = "RMUnFix";
            else
                sCmd = "RMReset";



            for (int i = 0; i < 10; i++)
            {
                for (int b = 0; b < 1; b++)
                {
                    if (sGt_Use[i] == CIni.Load("ADA_RM_" + cmbGTList.SelectedIndex.ToString(), "RMMatch", "0", CData.sRMPath))
                    {
                        if (pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.nIOType == int.Parse(CIni.Load("ADA_RM_" + cmbGTList.SelectedIndex.ToString(), "RMIO", "0", CData.sRMPath)))
                        {
                            pProc[i].pStation_GT[b].Gt_Cmd_Send(CIni.Load("ADA_RM_" + cmbGTList.SelectedIndex.ToString(), sCmd, "0", CData.sRMPath));
                            WriteGTLog("#" + i.ToString() + "#" + b.ToString(), ((pProc[i].st_NetInfo_GT_Proc[b].stGtInfo.nIOType == 0) ? true : false), "TX : " + sCmd);
                            bGT_Pass = true;
                            break;
                        }
                    }
                }
                if (bGT_Pass)
                    break;
            }
        }


        private void SdbTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {

            bool bStackSuc = false;
            pSDBTimer.Stop();
            TPMS_CMD eTimer_Cmd;

            //CData.nStackCnt = CData.p.Select_Cnt_Stack();

            if (CData.nStackCnt > 0)
            {
                string[] sRows = pProc[0].Stack_Use().Split('!');

                try
                {
                    if (!CData.bParse)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            if (pProc[i] != null)
                            {
                                if (sRows[1] == pProc[i].pTPMS.st_TpmsInfo.sID && pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                                {
                                    if (nReCmdCnt < 2)
                                    {
                                        switch (sRows[0])
                                        {
                                            case "IMG_UP_NORMAL_IN":
                                                try
                                                {
                                                    //bStackSuc = pProc[i].TPMS_Run(TPMS_CMD.IMG_UP_NORMAL_IN, sRows[2], sRows[3]);
                                                    bStackSuc = true;

                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                                bStackSuc = true;
                                                break;
                                            case "IMG_UP_NORMAL_OUT":
                                                try
                                                {
                                                    string[] sRows_Col_Out = sRows[3].Split('|');
                                                    //bStackSuc = pProc[i].TPMS_Run(TPMS_CMD.IMG_UP_NORMAL_OUT, sRows[2], sRows[3]);

                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                                bStackSuc = true;
                                                break;
                                            case "MU_OUT":
                                                try
                                                {
                                                    string[] sTemp = new string[10];
                                                    if (sRows[3] == "IO")
                                                    {
                                                        try
                                                        {
                                                            pProc[i].pStation[0].RecvComplete(sRows[2], true);
                                                        }
                                                        catch(Exception)
                                                        {
                                                            throw;
                                                        }
                                                        bStackSuc = true;
                                                    }
                                                    else
                                                        bStackSuc = pProc[i].TPMS_Run(TPMS_CMD.MU_OUT, sRows[2]);

                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                                bStackSuc = true;
                                                break;
                                            case "MU_IN":
                                                try
                                                {
                                                    string[] sTemp = new string[10];

                                                    if (sRows[3] == "IO")
                                                    {
                                                        try
                                                        {
                                                            //pProc[i].pStation[0].dfParse("#0#0", true, sRows[2], pProc[i].pStation[0].st_NetInfo.stLPRInfo.sFolder, false);
                                                            pProc[i].pStation[0].RecvComplete(sRows[2], true);
                                                            bStackSuc = true;
                                                        }
                                                        catch (Exception)
                                                        {
                                                            throw;
                                                        }
                                                    }
                                                    else
                                                        bStackSuc = pProc[i].TPMS_Run(TPMS_CMD.MU_IN, sRows[2]);
                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                                bStackSuc = true;
                                                break;
                                            case "CAL_MU":
                                                try
                                                {
                                                    bStackSuc = pProc[i].TPMS_Run(TPMS_CMD.CAL_MU, sRows[2]);
                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                                bStackSuc = true; //임시로 전부 true 걸어놓음
                                                break;
                                            case "CLEAR_OUT":
                                                break;
                                            default:
                                                //pTPMS.SendCmd_Bulider();
                                                break;
                                        }
                                        if (sPrev_Row == sRows[2])
                                            nReCmdCnt++;
                                        else
                                            nReCmdCnt = 0;

                                        sPrev_Row = sRows[2];


                                    }
                                    else
                                    {
                                        nReCmdCnt = 0;
                                        bStackSuc = true;
                                    }


                                    if (bStackSuc)
                                    {
                                        pProc[0].Stack_Del();
                                    }



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

            }
            pSDBTimer.Start();
        }

        private void TpmsTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            pTPMSTimer.Stop();

            bool bRegDown = false;
            bool bPingResult = false;
            
            Ping pingSender = new Ping();
            
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (pProc[i] != null)
                    {
                        switch (pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus)
                        {
                            case false:
                                nReTokenCnt[i] = 0;
                                if (!CData.bLogVr)
                                    pProc[i].pTPMS.Login();
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
                                    pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus = false;
                                }

                                if (nReTokenCnt[i] >= 300)
                                {
                                    CLog.LOG(LOG_TYPE.SCREEN, "#" + i.ToString() + " Retoken Run Suc");
                                    pProc[i].pTPMS.Re_Token(TPMS_CMD.LOGIN);
                                    nReTokenCnt[i] = 0;

                                }

                                if(DateTime.Now.ToString("HH") == "01")
                                {
                                    if(nRegCnt == 0)
                                    {
                                       //pProc[0].pTPMS.
                                    }
                                    
                                }
                                else
                                {
                                    nRegCnt = 0;
                                }

                                nReTokenCnt[i]++;
                                nReHealthCnt[i]++;
                                break;
                            default:
                                break;
                        }

 

                        //Agent_Stat(0, pProc[i].pTPMS.st_TpmsInfo.stPkInfo.bStatus);
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
        }

        private void WSTimer_Work(object sender, System.Timers.ElapsedEventArgs e)
        {
            pWSTimer.Stop();
            byte[] bSend = new byte[2];
            bSend[0] = 137;
            bSend[1] = 0;

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (pProc[i] != null)
                    {
                        switch (pProc[i].pWS.st_WsInfo.bStatus)
                        {
                            case false:
                                pProc[i].pWS.Connect("ws://" + CData.sTpmsIP + ":" + CData.sTpmsPort + "/tpms/carmonitoring.do", pProc[i].pTPMS.st_TpmsInfo.sID);
                                break;
                            case true:

                                pProc[i].pWS.CheckAlive();

                                CLog.LOG(LOG_TYPE.WSK_WS, "Ws Status = " + pProc[i].pWS.st_WsInfo.bStatus.ToString());

                                if (nReLoginCnt[i] >= 120)
                                {

                                    CLog.LOG(LOG_TYPE.SCREEN, "ReLogin Run");
                                    pProc[i].st_WsInfo_Proc.bStatus = pProc[i].pWS.Connect("ws://" + CData.sTpmsIP + ":" + CData.sTpmsPort + "/tpms/carmonitoring.do", pProc[i].pTPMS.st_TpmsInfo.sID);
                                    nReLoginCnt[i] = 0;

                                }

                                nReLoginCnt[i]++;
                                break;
                            default:
                                break;
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

            pWSTimer.Start();
        }

        private void Auto_Tray(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                pTrayTimer.Stop();

                if (nTrayCnt >= 10)
                {
                    activate(false);
                    pTrayTimer.Close();
                    pTrayTimer = null;
                }
                else
                {
                    nTrayCnt++;
                    pTrayTimer.Start();
                }

            }
            catch (Exception)
            {

            }
            finally
            {
            }

        }
        private void btnMigration_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("");
            CData.bMig = true;
            pProc[0].InCar_Migration(txtMgStart.Text, txtMgEnd.Text);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bExitApp)
            {
                e.Cancel = true;
                activate(false);
            }
        }

        private void activate(bool bActive) //트레이 연계
        {
            if (bActive)
            {
                this.Visible = true; //창을 보이지 않게 한다.
                this.ShowIcon = true; //작업표시줄에서 제거. 
                niAgent.Visible = false; //트레이 아이콘을 표시한다.
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Visible = false; //창을 보이지 않게 한다.
                this.ShowIcon = false; //작업표시줄에서 제거.
                niAgent.Visible = true; //트레이 아이콘을 표시한다.
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void 닫기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {


                bExitApp = true;
                niAgent.Visible = false;
                Application.Exit();
            }
            catch (Exception)
            {

            }
            finally
            {
            }

        }

        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activate(true);
        }

        private void niAgent_DoubleClick(object sender, EventArgs e)
        {
            activate(true);
        }
    }
}

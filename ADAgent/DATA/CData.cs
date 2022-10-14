using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using DH;
using ADAgent.UTIL;
using System.Net;

namespace ADAgent.DATA
{
    public struct TpmsInfo
    {
        public string sID;
        public string sSecID;
        public string sLotArea;
        public ParkingInfo stPkInfo;
        public bool bUse;
    }
    public struct WSInfo
    {
        public bool bStatus;
    }


    public struct NetInfo
    {
        public int nType;
        public string sIP;
        public int nPort;
        public int nCNt;
        public LPRInfo stLPRInfo;
        public GtInfo stGtInfo;
        public bool bUse;
    }

    public struct LPRInfo
    {
        public int nNetType;
        public string sIP;
        public int nPort;
        public int nIOType;
        public string sID;
        public int nEqpm;
        public string sFolder;
        public bool bUse;
    }

    //public static string[] arParkInfoString = { "sectnId", "parkCount", "empNo", "statPayMachine", "statPrinter", "statPaper", "statIcCard", "statTrafficCard", "statBar", "statCamera", "statOutLpr", "statOutGate", "InLprId", "InGateId", "statInLpr", "statInGate" };
    //public static string[] arParkInfo = { "19", "52", "0", "정상" };
    public struct TableInfo
    {
        public string sInTable;
        public string sOutTable;
        public string sTotalTable;
        public string sPayTable;
        public string sRegTable;
        public string sDiscountTable;
        public string sLocateClm;
        public string sFranClm;
        public string sTerminalClm;
        public string sAirPort;
        //public string s
    }
    public struct DBInfo
    {
        // 0 -> Oracle, 1 -> MSSQL
        public int nDBType;
        public string sID;
        public string sPW;
        public string sBaseDB;
        public string sOperDB;
        public TableInfo stTableinfo;
        public string sIP;
        public int nPort;
        public bool[] bStatus;
    }
    public struct OraDBInfo
    {
        // 0 -> Oracle, 1 -> MSSQL
        public string sIP;
        public int nPort;
        public string sSrc;
        public string sID;
        public string sPW;
        public string sBASE;
        public bool bStatus;
    }

    public struct ParkingInfo
    {
        public int nParkCount;
        public bool bStatus;
    }

    public struct GtInfo
    {
        public string sMatchID;
        public int nIOType;
        public string sUp;
        public string sDn;
        public string sFix;
        public string sUnFix;
        public string sReset;
        public bool bUse;

    }

    public struct LST_REG
    {

        public string sPeriodId;
        public string sCarno;
        public string sUseYN;

    }

    public struct LST_REG_FREE
    {
        public string sCarno;
        public string sStartDate;
        public string sEndDate;
        public string sFreeTyCd;
        public string sExCardNo;
        public string sInCardNo;
    }



    class CData
    {
        public static CMSSQL pMSSQL = null;
        public static CDB pDB = null;
        public static COraDB pODB = null;

        public static string sSetDir = Environment.CurrentDirectory + @"\SET";
        public static string sSignDir = Environment.CurrentDirectory + @"\SIGN";

        public static string sTpmsPath = sSetDir + @"\Set_TPMS.Ini";
        public static string sTpmsIDPath = sSetDir + @"\Set_TPMS_ID.Ini";
        public static string sDBPath = sSetDir + @"\Set_DB.Ini";
        public static string sLPRPath = sSetDir + @"\Set_LPR.Ini";
        public static string sRMPath = sSetDir + @"\Set_RM.Ini";

        public static string sDataDir = Environment.CurrentDirectory + @"\DATA";
        public static string sStackDB = sDataDir + "\\" + "Stack.db";
        public static string sSetDB = sSetDir + "\\" + "setting.db";
        public static string sIOCarDB = sDataDir + "\\" + "IOCar.db";
        public static string sRegDB = sDataDir + "\\" + "Reg.db";
        public static string[] arSetName = { "MNG", "WS" };
        public static string[] arCalType = { "Muin", "Kiosk" };

        public static List<int> garOpt1 = new List<int>();

        public static string[] garOpt1_Name = { "정기권 1/6 오토 다운 & 업로드",               //0
                                                "사전정산 전용",                               //1
                                                "마이그레이션",                                //2
                                                "사진 미사용",                                 //3
                                                "LPR 리트라이 후 마이그레이션 사용",           //4
                                                "입출차 DB 연동",                              //5
                                                "수동입차 데이터 연동",                        //6
                                                "멀티쓰레드 사용"                              //7
                        
                                                                                         };
//        statBillIn
//statCoinOut
//statBillOut
        public static string[] arParkInfoString = { "sectnId", "parkCount", "empNo", "statPayMachine", "statPrinter", "statPaper", "statIcCard", "statTrafficCard", "statBar", "statCamera", "statOutLpr", "statOutGate", "InLprId", "InGateId", "statInLpr", "statInGate", "statCoinIn", "statBillIn", "statCoinOut", "statBillOut" };

        //Param:sectnId=19&carNo=%E C%84%9C%EC%9A%B812%EA%B0%801234&inGateNo=1&inTy=DEF&inDttm=2021-07-23 16:06:32&fullPayDiv=PRT003&recon1Id=&recon2Id=&returnVal=21072316063200000002&nomIn=%EC%A0%95%EC%83%81

        //Param:inOutId=&sectnId=27&carNo=78%EC%A0%801049&outGateNo=1&outTy=OUT1&outDttm=2021-07-12 07:41:14&Photos=99db7f41-e191-4bf3-a77b-ebc1e212531e&&empNo=T27_Muin1

        public static string[] arInParam = { "sectnId", "carNo", "inGateNo", "inTy", "inDttm", "fullPayDiv", "recon1Id", "recon2Id", "returnVal", "nomIn", "exCarNo" };

        public static string[] arInParamData = { "sectnId", "carNo", "1", "DEF", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "PRT003", "", "", "21072316063200000002", "%EC%A0%95%EC%83%81" };

        public static string[] arOutParam = { "inOutId", "sectnId", "carNo", "outGateNo", "outTy", "outDttm", "Photos", "empNo", "nomOut", "preStn" };
        //Param:inOutId=&sectnId=25&carNo=42%EC%84%9C6305&outGateNo=1&outTy=OUT1&outDttm=2022-05-24 18:34:59&Photos=26b33580-ac3d-4de9-9235-ae759d1654d8&&empNo=T25_Muin1&nomOut=%EC%A0%95%EC%83%81&preStn=N
        public static string[] arOutParamData = { "", "", "", "1", "OUT1", "", "Photos", "empNo", "%EC%A0%95%EC%83%81", "N" };

        //00-sectnId", "01-parkCount", "02-empNo", "03-statPayMachine"

        public static string[] arPwd = { "qetu1357", "suwon6582", "roqkfwk00!" };

        public static string[] arNetType = { "Server", "Client" };

        public static string[] arIOType = { "입구", "출구", "입구후방", "입구전용", "출구전용" };

        public static string[] arGTType = { "Normal" };
        

        public static int[] arUse = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static string[,] arMuinID = new string[10, 3];

        public static int nInProcIdx = 999;
        public static int nOutProcIdx = 999;
        public static bool gbSendReceipt = false;
        public static int nHourChecker = 0;
        public static int nMinChecker = 0;
        public static string sAirPort = "1";
        public static string lastData = "";

        public static string ETX = "\x03";

        public static int[] nCheckThr = null;

        public static int nCntMcs = 0;
        public static int nCntPdc = 0;
        public static int gnConfigType = 0;

        public static int nDBCnt = 0;
        public static int nStackCnt = 0;

        public static int nCntInkey = 0;
        public static int nCntOutkey = 0;
        public static string sDate = "";
        public static int nParkCnt = 10;
        public static string sTpmsIP = "";
        public static string sTpmsPort = "";
        public static int nTpmsType = 0;
        public static string sLastDate = "";
        public static string sID = "";
        public static bool bTPMSUse = false;
        public static bool bDBUse = false;
        public static bool bOraDBUse = false;
        public static bool bWsUse = false;
        public static bool bLogVr = false;
        public static bool bParse = false;
        public static bool bChkIn = false;
        //public static string sPrevCarno = "";
        public static UC_Reg_Lst ucReg = null;
        public static UC_Tpms_Lst ucTpms = null;
        public static UC_Mssql_Lst ucMssql = null;
        public static bool bMig = false;
        public static bool bDBSELECT = false;
        public static string SetKey(bool bIO)
        {
            string sKey = "";
            string sNowDate;
            int nNow = 0;
            int nLast = 0;
            string sInCnt = nCntInkey.ToString();
            string sOutCnt = nCntOutkey.ToString();

            char pad = '0';

            sInCnt = sInCnt.PadLeft(8, pad);
            sOutCnt = sOutCnt.PadLeft(8, pad);
            //DateTime T1 = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //DateTime T2 = DateTime.Parse("2021-08-01");
            sNowDate = DateTime.Now.ToString("yyyyMMdd");
            //sLastDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //MessageBox.Show(sDate);

            if (sLastDate == "")
                sLastDate = DateTime.Now.ToString("yyyyMMdd");


            nNow = (int.Parse(sNowDate));
            nLast = (int.Parse(sLastDate));
            //MessageBox.Show((Convert.ToInt32(sNowDate.ToString()) - Convert.ToInt32(sLastDate.ToString())).ToString());
            //DateTime T2 = DateTime.Parse("2021-08-01");
            if (nLast == 0)
            {
                nLast = nNow;
            }

            if ((nNow - nLast) >= 1)
            {
                nCntInkey = 0;
                nCntOutkey = 0;
            }
            


            if (bIO)
            {

                sKey = DateTime.Now.ToString("yyMMddHHmmss") + sInCnt;
                nCntInkey++;

            }
            else
            {
                sKey = DateTime.Now.ToString("yyMMddHHmmss") + sOutCnt;
                nCntOutkey++;
            }

            return sKey;
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Web;
using System.Windows.Forms;
using ADAgent.DATA;
using ADAgent.UTIL;
using DH.NET;
using Newtonsoft.Json.Linq;

namespace ADAgent.TPMS
{
    enum TPMS_CMD
    {
        LOGIN = 0,
        CHK_HEALTH,
        IN,
        IN_REG,
        IN_PAY,
        IN_NEW_NO_OUT,
        IMG_UP_NORMAL_IN,
        IMG_UP_NORMAL_OUT,

        OUT_NOKEY,

        IN_EDIT,
        NO_OUT_INS,
        MU_OUT,
        OUT,
        OUT_FS,
        OUT_REG,
        OUT_DEL,
        OUT_REFUND,

        IN_DEL,

        MU_IN,
        MU_IN_REG,
        MU_IN_LST,
        CAL_MU,

        LST_REG,
        LST_REG_NEXT,
        LST_REG_FREE,
        LST_WORKER,
        AUTO_MINAB,
        CLEAR_OUT,
        FREE_INSERT,
        FREE_UPDATE,
        VISIT_INSERT,
        VISIT_UPDATE,
        ALL_INSERT
    }


    enum TPMS_LOGIN_STAT
    {
        READY = 0,
        SUCC,
        FAIL
    }

    class CTPMS
    {
        string sServer = "";
        int nPort = 80;
        int nReSendCnt = 0;
        string sLastInCarno;
        public string[] sLastOutCar = new string[2];
        string sDbDate;
        string sTpmsID;
        string sToken = "";
        int nLoginType = 0;
        int nCntCheckNow = 0;
        int nNowIdx = 0;
        int nTryLogin = 0;
        bool bReLogin = false;
        string sIn_Pic = "";
        string sOut_Pic = "";
        Dictionary<string, string> dcLogin = new Dictionary<string, string>();
        StringBuilder sbLastParam = new StringBuilder();

        TPMS_LOGIN_STAT eLogin = TPMS_LOGIN_STAT.READY;
        public TpmsInfo st_TpmsInfo;
        public ParkingInfo st_ParkingInfo;

        public delegate void DF_ParseTPMS(string sRCV, int nCntCheck);
        public DF_ParseTPMS dfParseTPMS = null;

        public delegate void DF_StatusTPMS(int nMachine,  int nCntCheck);
        public DF_StatusTPMS dfStatusTPMS = null;

        public delegate void Df_LogTPMS (string sID, string sLog);
        public Df_LogTPMS dfLogTPMS = null;

        //TPMS_CMD eCmd, Dictionary<string, string> dcParam, string sNm, string sFull, bool bExist)

        public delegate void Df_Stack(TPMS_CMD eCmd, string sID, StringBuilder sbData, string sCarno = "");
        public Df_Stack dfStack = null;

        public delegate void Df_Stack_IMG(TPMS_CMD eCmd, string sID, StringBuilder sbData, string sNm, string sFull, bool bExist);
        public Df_Stack_IMG dfStackIMG = null;

        public delegate void Df_Not_Normal(string sCarno, string sRsType);
        public Df_Not_Normal dfNotNormal = null;

        public CTPMS(int nIdx)
        {
            nNowIdx = nIdx;
            SERVER = CData.sTpmsIP;
            PORT = int.Parse(CData.sTpmsPort);
            nLoginType = CData.nTpmsType;
            sLastOutCar[0] = "0";
            sLastOutCar[1] = "0";
        }
        public string SERVER
        {
            get { return sServer; }
            set { sServer = value; }
        }

        public int PORT
        {
            get { return nPort; }
            set { nPort = value; }
        }
        
        public string Url_Re(TPMS_CMD eCmd)
        {
            string sUrl = "";
            switch (eCmd)
            {
                case TPMS_CMD.IMG_UP_NORMAL_IN:
                    sUrl = "/tpms/api/scm/recon/muin_upload.json";
                    break;
                case TPMS_CMD.IMG_UP_NORMAL_OUT:
                    sUrl = "/tpms/api/scm/file/inout/upload.json";
                    break;
            }
            return sUrl;
        }

        public string Token_Get()
        {
            return sToken;
        }

        public bool Login()
        {
            Dictionary<string, string> dcParam = new Dictionary<string, string>();
            string sLogData = "";

            switch (nLoginType)
            {
                case 1://서버타입1 - 부천
                    dcParam.Add("tMemberId", st_TpmsInfo.sID);
                    dcParam.Add("tPassword", CData.arPwd[0]);
                    sLogData = "tMemberId : " + st_TpmsInfo.sID + " & " + "tPassword : " + CData.arPwd[0];
                    break;
                case 2://서버타입2 - 수원/양주(패스워드 다름)
                    dcParam.Add("empNo", st_TpmsInfo.sID);
                    dcParam.Add("password", CData.arPwd[1]);
                    sLogData = "empNo : " + st_TpmsInfo.sID + " & " + "password : " + CData.arPwd[1];
                    break;
                case 3:
                case 4: //평택
                    dcParam.Add("empNo", st_TpmsInfo.sID);
                    dcParam.Add("password", CData.arPwd[2]);
                    sLogData = "empNo : " + st_TpmsInfo.sID + " & " + "password : " + CData.arPwd[2];
                    break;
            }

            dcParam.Add("tDeviceId", "01");

            try
            {
                eLogin = TPMS_LOGIN_STAT.READY;
                //MessageBox.Show(sToken);
                //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx+" Login TX : {" + "param : {" + sLogData + "}}");
                //dfStatusTPMS(nCntCheckNow);
                dcLogin = dcParam;
                //dfLogTPMS(st_TpmsInfo.sID, "Login TX : {" + "param : {" + sLogData + "}}");
                if (!SendCmd(TPMS_CMD.LOGIN, dcParam))
                    return false;
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " Login ex: " + e.Message);
            }
            finally
            {
            }
            return true;
        }
        //public static string[] arInParam = { "sectnId", "carNo", "inGateNo", "inTy", "inDttm", "fullPayDiv", "recon1Id", "recon2Id", "returnVal", "nomIn" };
        //Param:sectnId=19&carNo=%E C%84%9C%EC%9A%B812%EA%B0%801234&inGateNo=1&inTy=DEF&inDttm=2021-07-23 16:06:32&fullPayDiv=PRT003&recon1Id=&recon2Id=&returnVal=21072316063200000002&nomIn=%EC%A0%95%EC%83%81
        //sectnId : 18 carNo : 70?5972 inGateNo : 1inTy : DEF inDttm : 2022-05-24 10:28:39 fullPayDiv : PRT003 recon1Id :  recon2Id :  returnVal : 1 nomIn : %EC%A0%95%EC%83%81 

        public bool Mu_In(string sCarno, string sInDttm)
        {
            sLastInCarno = sCarno;
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            //byte[] btData;
            dcParam1.Add(CData.arInParam[0], st_TpmsInfo.sSecID);
            sb.Append(CData.arInParam[0] + " = " + st_TpmsInfo.sSecID + " ");


            //sCarno = HttpUtility.UrlEncode(sCarno);
            dcParam1.Add(CData.arInParam[1], sCarno);
            sb.Append(CData.arInParam[1] + " = " + sCarno + " ");

            //dcParam1.Add("empNo", CData.arPcsInfo[0].sPDC_ID);
            dcParam1.Add(CData.arInParam[2], "1");
            sb.Append(CData.arInParam[2] + " = " + "1");

            for (int i = 3; i < CData.arInParam.Length; i++)
            {
                if (i == 8)
                {
                    string SetKey = CData.SetKey(true);
                    //MessageBox.Show(SetKey);
                    dcParam1.Add(CData.arInParam[i], SetKey);
                    sb.Append(CData.arInParam[i] + " = " + SetKey + " ");

                }
                else if (i == 4)
                {
                    dcParam1.Add(CData.arInParam[i], sInDttm);
                    sb.Append(CData.arInParam[i] + " = " + sInDttm + " ");
                }
                else if (i == 6)
                {
                    dcParam1.Add(CData.arInParam[i], CData.pDB.Select_IOCar_Img(sCarno));
                    sb.Append(CData.arInParam[i] + " = " + CData.pDB.Select_IOCar_Img(sCarno) + " ");
                }
                else if (i == 10)
                {
                    dcParam1.Add(CData.arInParam[i], sCarno);
                    sb.Append(CData.arInParam[i] + " = " + sCarno + " ");
                }
                else
                {
                    dcParam1.Add(CData.arInParam[i], CData.arInParamData[i]);
                    sb.Append(CData.arInParam[i] + " = " + CData.arInParamData[i] + " ");
                }

            }

            //dcParam

            CData.arInParamData[6] = "";

            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " MU_IN TX : {" + sb.ToString() + "}");
            //dfLogTPMS(st_TpmsInfo.sID, "MU_IN TX : {" + sb.ToString() + "}");
            if (!SendCmd(TPMS_CMD.MU_IN, dcParam1, sCarno))
                return false;

            
            return true;
        }

        public bool IMG_UP_IN(string sCarno, string sNm, string sFull, bool bExist)
        {
            string sBody = "";
            string sBody_End = "";
            string sHeader = "";
            int nLen = 0;
            bool bRcvImg = false;
            
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            //sBody = "Content-Disposition: form-data; name=file; filename=" + sNm + "Content-Type: image/png";
            //sBody_End = "-----------------------------junche421e05d2--";

            //nLen = sBody.Length + sBody_End.Length + nLength;

            //sHeader = "POST " + sUrl + " HTTP/1.0 Content-Type: multipart/form-data, boundary=---------------------------junche421e05d2 Content-Length: " + nLen + "X-TPMS-AUTH-TOKEN: " + pTPMS.Token_Get() + sBody;

            //else
            //{
            //    sHeader = "POST " + sUrl + "  HTTP/1.0 Content-Type: multipart/form-data, boundary=---------------------------junche421e05d2";

            //    if (bLogin)
            //        sHeader = sHeader + "X-TPMS-AUTH-TOKEN: " + pTPMS.Token_Get();
            //}
            dcParam1.Add("name", "file");
            dcParam1.Add("filename", sNm);
            CData.pDB.Insert_IOCar(0, "", sCarno);
            //dfLogTPMS(st_TpmsInfo.sID, "IMG_UP_NORMAL_IN TX : {Pic=" + sNm + "&Exist=" + bExist.ToString() + "}");
            if (!SendCmd_IMG(TPMS_CMD.IMG_UP_NORMAL_IN, dcParam1, sNm, sFull, bExist, sCarno))
                return false;

            return true;
        }
        
        //PIC:20220526_182358_39구7031.jpg Exist: True


        //public static string[] arOutParam = { "inOutId", "sectnId", "carNo", "outGateNo", "outTy", "outDttm", "Photos", "empNo" };
        //Param:inOutId=&sectnId=25&carNo=42%EC%84%9C6305&outGateNo=1&outTy=OUT1&outDttm=2022-05-24 18:34:59&Photos=26b33580-ac3d-4de9-9235-ae759d1654d8&&empNo=T25_Muin1&nomOut=%EC%A0%95%EC%83%81&preStn=N
        public bool Mu_Out(string sCarno, string sOutDttm, bool bPrev = false)
        {
            sLastOutCar[1] = sCarno;
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();

            dcParam1.Add(CData.arOutParam[0], CData.arOutParamData[0]);
            sb.Append(CData.arOutParam[0] + "=" + CData.arOutParamData[0] + "");

            dcParam1.Add(CData.arOutParam[1], st_TpmsInfo.sSecID);
            sb.Append(CData.arOutParam[1] + " = " + st_TpmsInfo.sSecID + " ");

            dcParam1.Add(CData.arOutParam[2], sCarno);
            sb.Append(CData.arOutParam[2] + " = " + sCarno + " ");

            for (int i = 3; i < CData.arOutParam.Length; i++)
            {
                if (i == 7)
                {
                    dcParam1.Add(CData.arOutParam[i], st_TpmsInfo.sID);
                    sb.Append(CData.arOutParam[i] + " = " + st_TpmsInfo.sID + " ");
                }
                else if (i == 5)
                {
                    dcParam1.Add(CData.arOutParam[i], sOutDttm);
                    sb.Append(CData.arOutParam[i] + " = " + sOutDttm + " ");
                }
                else if(i == 6)
                {
                    
                    dcParam1.Add(CData.arOutParam[i], ((bPrev) ? "" : CData.pDB.Select_IOCar_Img(sCarno)));
                    sb.Append(CData.arOutParam[i] + " = " + ((bPrev) ? "" : CData.pDB.Select_IOCar_Img(sCarno)) + " ");
                }
                else
                {
                    dcParam1.Add(CData.arOutParam[i], CData.arOutParamData[i]);
                    sb.Append(CData.arOutParam[i] + " = " + CData.arOutParamData[i] + " ");
                }
            }
            dcParam1.Add("amanoYn", ((CData.garOpt1[1] == 1) ? "Y" : "O"));
            sb.Append(CData.arOutParam[2] + " = " + sCarno + " ");
            CData.arOutParamData[6] = "";

            //dcParam
            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " MU_OUT TX : {" + sb.ToString() + "}");
            //dfLogTPMS(st_TpmsInfo.sID, "MU_OUT TX : {" + sb.ToString() + "}");
            if (!SendCmd(TPMS_CMD.MU_OUT, dcParam1, sCarno))
                return false;

            return true;
        }

        public bool IMG_UP_OUT(string sCarno, string sNm, string sFull, bool bExist)
        {
            string sBody = "";
            string sBody_End = "";
            string sHeader = "";
            int nLen = 0;
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            //sBody = "Content-Disposition: form-data; name=file; filename=" + sNm + "Content-Type: image/png";
            //sBody_End = "-----------------------------junche421e05d2--";

            //nLen = sBody.Length + sBody_End.Length + nLength;

            //sHeader = "POST " + sUrl + " HTTP/1.0 Content-Type: multipart/form-data, boundary=---------------------------junche421e05d2 Content-Length: " + nLen + "X-TPMS-AUTH-TOKEN: " + pTPMS.Token_Get() + sBody;

            //else
            //{
            //    sHeader = "POST " + sUrl + "  HTTP/1.0 Content-Type: multipart/form-data, boundary=---------------------------junche421e05d2";

            //    if (bLogin)
            //        sHeader = sHeader + "X-TPMS-AUTH-TOKEN: " + pTPMS.Token_Get();


            //}
            dcParam1.Add("name", "file");
            dcParam1.Add("filename", sNm);
            //dfLogTPMS(st_TpmsInfo.sID, "IMG_UP_NORMAL_OUT TX :{Pic=" + sNm + "&Exist=" + bExist.ToString() + "}");
            CData.pDB.Insert_IOCar(0, "", sCarno, 1, 0);
            if (!SendCmd_IMG(TPMS_CMD.IMG_UP_NORMAL_OUT, dcParam1, sNm, sFull, bExist, sCarno))
                return false;



            return true;
        }

        public bool Cal_Mu(string sCarno, int nFee, string sPaydt = "", string sCredit = "", bool bPrev = false, bool bFree = false, bool bTest = false, bool bPayWay = false)
        {

            //Param: receiptId = &inOutId = 130397768 & receiptWay = CARD & receiptTy = DEF & receiptDivCd = RET - 005 & receiptAmt = 0 & prepayTicketAmt = 0 &
            //nanumTicketAmt = 0 & receiptParkingSectnId = 103 & receiptWorkerId = T39_Muin1 & receiptDttm = 2022 - 05 - 23 11:58 & cardApprovalAmt = 0 & cardTradeDivCd = &cardTradeMedia = 1 &
            //cardApprovalNo = &cardSaleDttmVal = 20220523115800 & cardTradeNo = &cardMemberNo = &cardDeviceNo = null & cardIssueCorpNm = &cardPurchCorpNm = &returnVal = &totalCnt = &unPayInOutIds =
            string sReceiptDttm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sCardSaleDttm = DateTime.Now.ToString("yyyyMMddHHmmss");
            string sPay = "CARD";
            string sPaySend = "RET-005";

            if (sPaydt != "")
            {
                sReceiptDttm = sPaydt;
            }

            if (!bFree)
            {

                if (sCredit.Length < 2)
                {
                    sPay = "CASH";
                    sPaySend = "RET-001";
                }
                else
                {
                    sPay = "CARD";
                    sPaySend = "RET-005";
                }

            }
            else
            {
                sPay = "FREE";
                sPaySend = "RET-019";
            }
            sPay = "CARD";

            if (bPrev)
                sPaySend = "RET-016";

            if(bTest)
            {
                sPay = ((bPayWay) ? "CARD" : "CASH");
            }
            

            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            dcParam1.Add("receiptId", ""); //0
            CLog.LOG(LOG_TYPE.SCREEN, "CAL_MU : " + sCarno + "&Approval=" + sCredit + "&Fee=" + nFee.ToString());
            dcParam1.Add("inOutId", CData.pDB.Select_IOCar_ID(sCarno).ToString()); //1
            dcParam1.Add("receiptWay", sPay); //2
            dcParam1.Add("receiptTy", "DEF"); //3
            dcParam1.Add("receiptDivCd", "RET-024"); //4
            dcParam1.Add("receiptAmt", nFee.ToString());//5
            dcParam1.Add("prepayTicketAmt", "0");//6
            dcParam1.Add("nanumTicketAmt", "0");//7
            dcParam1.Add("receiptParkingSectnId", st_TpmsInfo.sSecID);//8
            dcParam1.Add("receiptWorkerId", st_TpmsInfo.sID);//9
            dcParam1.Add("receiptDttm", sReceiptDttm);//10
            dcParam1.Add("cardApprovalAmt", nFee.ToString());//11
            dcParam1.Add("cardTradeDivCd", "");//12
            dcParam1.Add("cardTradeMedia", (sPay == "CARD") ? "6" : "1");//13
            dcParam1.Add("cardApprovalNo", "");//14

            dcParam1.Add("cardSaleDttmVal", sCardSaleDttm);
            dcParam1.Add("cardTradeNo", "");
            dcParam1.Add("cardMemberNo", "");
            dcParam1.Add("cardDeviceNo", "");
            dcParam1.Add("cardIssueCorpNm", "");
            dcParam1.Add("returnVal", "");
            dcParam1.Add("totalCnt", "");
            dcParam1.Add("unPayInOutIds", "");
            dcParam1.Add("amanoYn", (CData.garOpt1[1] == 1) ? "Y" : "O");

            //dcParam
            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " CAL_MU TX : {receiptId=&inOutId=" + sLastOutCar[0] + "&receiptWay="+ sPay + "&receiptTy=DEF&receiptDivCd="+ sPaySend + "&receiptAmt=0&prepayTicketAmt=0" +
            //    "&nanumTicketAmt=0&receiptParkingSectnId="+ st_TpmsInfo.sSecID + "&receiptWorkerId=" + st_TpmsInfo.sID + "&receiptDttm=" + sReceiptDttm + "&cardApprovalAmt=" + nFee.ToString() + "&cardTradeDivCd=" +
            //    "&cardTradeMedia=1&cardApprovalNo=&cardSaleDttmVal="+ sCardSaleDttm + "&cardTradeNo=&cardMemberNo=&cardDeviceNo=null&cardIssueCorpNm=&cardPurchCorpNm=&returnVal=&totalCnt=&unPayInOutIds=}");
            //dfLogTPMS(st_TpmsInfo.sID, "CAL_MU TX : {receiptId=&inOutId=" + sLastOutCar[0] + "&receiptWay=" + sPay + "&receiptTy=DEF&receiptDivCd=" + sPaySend + "&receiptAmt=0&prepayTicketAmt=0" +
            //    "&nanumTicketAmt=0&receiptParkingSectnId=" + st_TpmsInfo.sSecID + "&receiptWorkerId=" + st_TpmsInfo.sID + "&receiptDttm=" + sReceiptDttm + "&cardApprovalAmt=" + nFee.ToString() + "&cardTradeDivCd=" +
            //    "&cardTradeMedia=1&cardApprovalNo=&cardSaleDttmVal=" + sCardSaleDttm + "&cardTradeNo=&cardMemberNo=&cardDeviceNo=null&cardIssueCorpNm=&cardPurchCorpNm=&returnVal=&totalCnt=&unPayInOutIds=}");
            if (!SendCmd(TPMS_CMD.CAL_MU, dcParam1, sCarno))
                return false;

            sLastOutCar[0] = "";

            return true;
        }
        //PIC:20220526_182358_39구7031.jpg Exist: True

        //17:05:14.33 TX CMD:CHK_HEALTH(idx: 0) Stat:8 응답완료 Param:sectnId=19&parkCount=52&empNo=T19_Muin1&statPayMachine=%EC%A0%95%EC%83%81&statPrinter=%EC%A0%95%EC%83%81&statPaper=%EC%A0%95%EC%83%81&statIcCard=%EC%A0%95%EC%83%81&statTrafficCard=%EC%A0%95%EC%83%81&statBar=%EC%A0%95%EC%83%81&statCamera=%EC%A0%95%EC%83%81&statOutLpr=%EC%A0%95%EC%83%81&statOutGate=%EC%A0%95%EC%83%81&InLprId=1&InGateId=1&statInLpr=%EC%A0%95%EC%83%81&statInGate=%EC%A0%95%EC%83%81


        //  aKey = Array("00-sectnId", "01-parkCount", "02-empNo", "03-statPayMachine", "04-statPrinter", "05-statPaper", _
        //             "06-statIcCard", "07-statTrafficCard", "08-statBar", "09-statCamera", "10-statOutLpr", _
        //             "11-statOutGate", "12-InLprId", "13-InGateId", "14-statInLpr", "15-statInGate")


        //aVal = Array("00-" & GtParkInfo.sSite_ID, "01-" & GtINFO.Parking_Area, "02-" & sID, "03-" & sOK, "04-" & sOK, "05-" & sOK, _
        //             "06-" & sOK, "07-" & sOK, "08-" & sOK, "09-" & sOK, "10-" & sOK, _
        //             "11-" & sOK, "12-" & "1", "13-" & "1", "14-" & sOK, "15-" & sOK)
        //Call Server_Send("CHK_HEALTH", aKey, aVal, "", "", nTK_index, "")

        //sURL = "/tpms/api/stm/inout/health.json"


        public bool HealthChk()
        {
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            //foreach()
            //{

            //}
            dcParam1.Add(CData.arParkInfoString[0], st_TpmsInfo.sSecID);
            sb.Append(CData.arParkInfoString[0] + " : " + st_TpmsInfo.sSecID + " ");

            dcParam1.Add(CData.arParkInfoString[1], CData.nParkCnt.ToString());
            sb.Append(CData.arParkInfoString[1] + " : " + CData.nParkCnt.ToString() + " ");

            //dcParam1.Add("empNo", CData.arPcsInfo[0].sPDC_ID);
            dcParam1.Add(CData.arParkInfoString[2], st_TpmsInfo.sID);
            sb.Append(CData.arParkInfoString[2] + " : " + st_TpmsInfo.sID);


                dcParam1.Add(CData.arParkInfoString[3], st_TpmsInfo.sSecID);
                sb.Append(CData.arParkInfoString[3] + " : " + st_TpmsInfo.sSecID + " ");
            for (int i = 4; i <= 19; i++)
            {
                dcParam1.Add(CData.arParkInfoString[i], "정상");
                sb.Append(CData.arParkInfoString[i] + " : " + "정상");
            }

            Console.WriteLine(dcParam1);

            //dcParam


            SendCmd(TPMS_CMD.CHK_HEALTH, dcParam1);
            
            return true;
        }

        public bool Auto_Minab()
        {
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();

            dcParam1.Add(CData.arOutParam[0], sLastOutCar[0]);
            sb.Append(CData.arOutParam[0] + "=" + sLastOutCar[0] + " ");

            dcParam1.Add(CData.arOutParam[1], st_TpmsInfo.sSecID);
            sb.Append(CData.arOutParam[1] + "=" + st_TpmsInfo.sSecID + " ");

            dcParam1.Add(CData.arOutParam[2], sLastOutCar[1]);
            sb.Append(CData.arOutParam[2] + "=" + sLastOutCar[1]);

            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + "  Auto_Minab TX : {" + sb.ToString() + "}");

            if (!SendCmd(TPMS_CMD.AUTO_MINAB, dcParam1))
                return false;

            sLastOutCar[0] = "F";
            return true;
        }

//        [13:21:23.45 ]
//        [TX CMD:LST_REG(idx: 0) Stat:8 응답완료 Param:sectnId=27&is=2022-06-01 00:00:00&ie=2022-06-30 23:59:59&limit=5000&page=1]
        
//[13:21:36.62 ]
//[TX CMD:LST_REG_FREE(idx: 0) Stat:8 응답완료 Param:sectnId=27]
//[13:21:43.13 ]
//[TX CMD:LST_WORKER(idx: 0) Stat:8 응답완료 Param:base=]
//[13:21:23.45 ]
//[TX CMD:LST_REG(idx: 0) Stat:8 응답완료 Param:sectnId=27&is=2022-06-01 00:00:00&ie=2022-06-30 23:59:59&limit=5000&page=1]
//[13:21:36.62 ]
//[TX CMD:LST_REG_FREE(idx: 0) Stat:8 응답완료 Param:sectnId=27]
//[13:21:43.13 ]
//[TX CMD:LST_WORKER(idx: 0) Stat:8 응답완료 Param:base=]

        public bool LST_REG()
        {
            string sIs = DateTime.Now.ToString("yyyy-MM-01 00:00:00");
            string sIe = DateTime.Now.ToString("yyyy-MM-") + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + " 23:59:59";
            //DateTime.Now.toSt
            string sPay = "CARD";
            string sPaySend = "RET-005";

            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            dcParam1.Add("sectnId", st_TpmsInfo.sSecID);
            dcParam1.Add("is", st_TpmsInfo.sSecID);
            dcParam1.Add("ie", st_TpmsInfo.sSecID);
            dcParam1.Add("limit", "5000");
            dcParam1.Add("page", "1");


            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + "  Auto_Minab TX : {" + sb.ToString() + "}");

            if (!SendCmd(TPMS_CMD.LST_REG, dcParam1))
                return false;


            return true;
        }

        public bool LST_REG_FREE() //
        {
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            dcParam1.Add("sectnId", st_TpmsInfo.sSecID);
            //dcPat

            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + "  Auto_Minab TX : {" + sb.ToString() + "}");

            if (!SendCmd(TPMS_CMD.LST_REG_FREE, dcParam1))
                return false;


            return true;
        }

        public bool ALL_INSERT(string sCarno, string sStart, string sEnd, string sRegDiv, string sGroupNm, string sUserNm, string sMemo, string sArea, string sAreaArray)
        {
            
            string sSecID = "";
            string sFct = "";

            if (sRegDiv.IndexOf("상주") != -1)
                sFct = "FCT-001";
            else if (sRegDiv.IndexOf("방문") != -1)
                sFct = "FCT-002";
            else if (sRegDiv.IndexOf("월정") != -1)
                sFct = "FCT-003";
            else
                sFct = "FCT-001";

            string[] sSectnID = sAreaArray.Split(',');

#if DEBUG

#endif

            for (int i = 0; i < sSectnID.Length; i++)
            {
                try
                {
                    Dictionary<string, string> dcParam1 = new Dictionary<string, string>();
                    sSectnID[i].Replace(" ", "");
                    switch (sSectnID[i])
                    {
                        case "1":
                            sSecID = "1";
                            break;
                        case "2":
                            sSecID = "2";
                            break;
                        case "3":
                            sSecID = "3";
                            break;
                        case "4":
                            sSecID = "4";
                            break;
                        case "8":
                            sSecID = "5";
                            break;
                        case "9":
                            sSecID = "6";
                            break;
                        case "10":
                            sSecID = "7";
                            break;
                        case "11":
                            sSecID = "8";
                            break;
                        case "12":
                            sSecID = "9";
                            break;
                        case "13":
                            sSecID = "10";
                            break;
                        case "14":
                            sSecID = "11";
                            break;
                        case "15":
                            sSecID = "12";
                            break;
                        case "16":
                            sSecID = "13";
                            break;
                        default:
                            sSecID = "0";
                            break;
                    }

                    dcParam1.Add("carNo", sCarno);
                    dcParam1.Add("carOwnerNm", sUserNm);
                    dcParam1.Add("sectnId", sSecID);                
                    
                    dcParam1.Add("startDt", sStart + " 00:00:00");
                    dcParam1.Add("endDt", sEnd + " 23:59:59");
                    dcParam1.Add("note", sMemo);
                    dcParam1.Add("tyCd", sFct);
                    dcParam1.Add("carKind", "");
                    dcParam1.Add("groupId", "");
                    dcParam1.Add("groupNm", sGroupNm);
                    //0.06초마다 한건 올림 -> delay 를 0.3초로 올려야하나 얘기
                    //Thread.Sleep(300);
                    //CLog.LOG
                    //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + "  Auto_Minab TX : {" + sb.ToString() + "}");

                    SendCmd(TPMS_CMD.ALL_INSERT, dcParam1);

                }
                catch (Exception)
                {

                }
                finally
                {
                }
            }


            return true;
        }

        public bool FREE_UPDATE()
        {
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            dcParam1.Add("sectnId", st_TpmsInfo.sSecID);


            //CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + "  Auto_Minab TX : {" + sb.ToString() + "}");

            if (!SendCmd(TPMS_CMD.LST_REG_FREE, dcParam1))
                return false;


            return true;
        }

        public bool CLEAR_OUT(string sInID)
        {
            Dictionary<string, string> dcParam1 = new Dictionary<string, string>();

            dcParam1.Add("inOutId", sInID);


            CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " CLEAR_OUT TX=inOutId{" + sInID + "}");

            if (!SendCmd(TPMS_CMD.CLEAR_OUT, dcParam1))
                return false;


            return true;

        }


        //public bool LST_REG

        public bool SendCmd_IMG(TPMS_CMD eCmd, Dictionary<string, string> dcParam, string sNm, string sFull, bool bExist, string sCarno = "")
        {
            bool bRes = false;
            string sUrl = GetCmdUrl(eCmd);
            string sRes = "";

                //HealthChk();
                bool bFirst = true;
                StringBuilder sbParam = new StringBuilder();
                foreach (KeyValuePair<string, string> param in dcParam)
                {
                    if (!bFirst)
                        sbParam.Append("&" + param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                    else
                    {
                        sbParam.Append(param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                        bFirst = false;
                    }
                }


            try
            {
                if (!st_TpmsInfo.stPkInfo.bStatus)
                {
                    string sCmd_Img = "";
                    string[] sImg_Full = sFull.Split('\\');
                    for (int i = 0; i < sImg_Full.Length - 1; i++)
                    {
                        sImg_Full[i] = sImg_Full[i] + "\\\\";

                    }
                    for (int i = 0; i < sImg_Full.Length; i++)
                    {
                        sCmd_Img += sImg_Full[i];
                    }
                    //sFull = sCmd_Img;
                    //for(i)
                    if (dfStackIMG != null)
                        dfStackIMG(eCmd, st_TpmsInfo.sID, sbParam, sNm, sCmd_Img, bExist);

                    return false;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }

            if (dfLogTPMS != null)
                    dfLogTPMS(st_TpmsInfo.sID, eCmd.ToString() + " TX : {" + sbParam + "}");
            //MessageBox.Show(sUrl + "::" + sbParam.ToString());

            //Re_Token(eCmd);
            try
            {
                CLog.LOG(LOG_TYPE.SCREEN, "Img #0");
                sRes = HTTP.POST(sUrl, sbParam, sNm, sFull, ref sToken, bExist);
                    CLog.LOG(LOG_TYPE.SCREEN, "Img #1");
                if (sRes == "" && bExist)
                {
                    CLog.LOG(LOG_TYPE.SCREEN, "IMG Res = " + sRes);
                    return false;
                }

                    CLog.LOG(LOG_TYPE.SCREEN, "Img #2");
                    ProcResponse(eCmd, sRes, sCarno);
                    CLog.LOG(LOG_TYPE.SCREEN, "Img #3");

                //HTTP.POST(sUrl, sbParam.ToString(), sToken, ProcResponse);
                //HTTP.POST(sUrl, sbParam.ToString(), "", Parse);

                //CLog.LOG(LOG_TYPE.SERVER, "TX : " + sRes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            finally
            {
            }
            return true;
        }

        public bool SendCmd(TPMS_CMD eCmd, Dictionary<string, string> dcParam, string sCarno = "")
        {
            try
            {
                //HealthChk();
                bool bFirst = true;
                int nCnt = 0;
                StringBuilder sbParam = new StringBuilder();
                foreach (KeyValuePair<string, string> param in dcParam)
                {
                    //if (!bFirst)
                    //    sbParam.Append("&" + param.Key + "=" + param.Value);
                    //else
                    //{
                    //    sbParam.Append(param.Key + "=" + param.Value);
                    //    bFirst = false;
                    //}

                    if (!bFirst)
                        sbParam.Append("&" + param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                    else
                    {
                        if (eCmd != TPMS_CMD.CAL_MU)
                        {
                            sbParam.Append(param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                            bFirst = false;
                        }
                        else
                        {
                            sbParam.Append(param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                            bFirst = false;
                            nCnt++;
                        }
                    }

                }
                sbLastParam = sbParam;

                string sUrl = GetCmdUrl(eCmd);
                string sRes = "";
                CLog.LOG(LOG_TYPE.SERVER, "Url=" + sUrl);
                //MessageBox.Show(sUrl + "::" + sbParam.ToString());
                if (CData.garOpt1[1] == 0)
                {
                    if (eCmd != TPMS_CMD.LOGIN && !st_TpmsInfo.stPkInfo.bStatus)
                    {
                        if (dfStack != null)
                            dfStack(eCmd, st_TpmsInfo.sID, sbParam);
                        return false;
                    }
                }
                CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " " + eCmd.ToString() + " TX : {" + sbParam + "}");
                if (dfLogTPMS != null)
                    dfLogTPMS(st_TpmsInfo.sID, eCmd.ToString() + " TX : {" + sbParam + "}");

                if (!CData.bLogVr)
                {
                    sRes = HTTP.POST(sUrl, sbParam, ref sToken);
                    ProcResponse(eCmd, sRes, sCarno);
                }
                //HTTP.POST(sUrl, sbParam.ToString(), sToken, ProcResponse);
                //HTTP.POST(sUrl, sbParam.ToString(), "", Parse);

                //CLog.LOG(LOG_TYPE.SERVER, "TX : " + sRes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            finally
            {
            }

            return true;
        }

        public bool SendCmd_Builder(TPMS_CMD eCmd, string sData, string sNm = "", string sFull = "", bool bExist = false)
        {
            string sUrl = GetCmdUrl(eCmd);
            //string sReSave = "";
            string sRes = "";
            bool bStackChk = false;
            //MessageBox.Show(sUrl + "::" + sbParam.ToString());


            switch (eCmd)
            {
                case TPMS_CMD.IMG_UP_NORMAL_IN:
                case TPMS_CMD.IMG_UP_NORMAL_OUT:
                    break;
                case TPMS_CMD.MU_IN:
                    string sCmd_In = "";
                    string[] sImg_In = sData.Split('&');
                    sImg_In[6] = sImg_In[6] + CData.arInParamData[6];
                    //CData.arInParamData[6] = "";
                    for (int i = 0; i < sImg_In.Length; i++)
                    {
                        if (i != sImg_In.Length - 1)
                            sCmd_In += sImg_In[i] + "&";
                        else
                            sCmd_In += sImg_In[i];
                    }
                    sData = sCmd_In;
                    break;
                case TPMS_CMD.MU_OUT:
                    string sCmd_Out = "";
                    string[] sImg_Out = sData.Split('&');
                    sImg_Out[6] = CData.arOutParamData[6];
                    for (int i = 0; i < sImg_Out.Length; i++)
                    {
                        if (i != sImg_Out.Length - 1)
                            sCmd_Out += sImg_Out[i] + "&";
                        else
                            sCmd_Out += sImg_Out[i];
                    }
                    sData = sCmd_Out;
                    break;
                case TPMS_CMD.CAL_MU:
                    string sCmd_Cal = "";
                    string[] sImg_Cal = sData.Split('&');
                    sImg_Cal[1] = sLastOutCar[0];
                    for (int i = 0; i < sImg_Cal.Length; i++)
                    {
                        if (i != sImg_Cal.Length - 1)
                            sCmd_Cal += sImg_Cal[i] + "&";
                        else
                            sCmd_Cal += sImg_Cal[i];
                    }
                    sData = sCmd_Cal;
                    break;
                case TPMS_CMD.AUTO_MINAB:
                    string sCmd_Min = "";
                    string[] sImg_Min = sData.Split('&');
                    sImg_Min[6] = sLastOutCar[0];
                    for (int i = 0; i < sImg_Min.Length; i++)
                    {
                        if (i != sImg_Min.Length -1)
                            sCmd_Min += sImg_Min[i] + "&";
                        else
                            sCmd_Min += sImg_Min[i];
                    }
                    sData = sCmd_Min;
                    break;
                default:
                    break;
            }

            CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " " + eCmd.ToString() + " TX : {" + sData + "}");
            if (dfLogTPMS != null)
                dfLogTPMS(st_TpmsInfo.sID, eCmd.ToString() + " TX : {" + sData + "}");


            if (eCmd == TPMS_CMD.IMG_UP_NORMAL_IN || eCmd == TPMS_CMD.IMG_UP_NORMAL_OUT)
            {
                sRes = HTTP.POST(sUrl, sData, sNm, sFull, ref sToken, bExist);
            }
            else
            {
                sRes = HTTP.POST(sUrl, sData, ref sToken);
            }


            if (sRes == "" || sRes == null)
                return false;

            ProcResponse(eCmd, sRes);


            return true;
        }


        public bool Re_Token(TPMS_CMD eCmd)
        {
            bool bFirst = true;
            sToken = "";
            StringBuilder sbParam = new StringBuilder();
            foreach (KeyValuePair<string, string> param in dcLogin)
            {
                if (!bFirst)
                    sbParam.Append("&" + param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                else
                {
                    sbParam.Append(param.Key + "=" + HttpUtility.UrlEncode(param.Value));
                    bFirst = false;
                }

            }

            string sUrl = GetCmdUrl(TPMS_CMD.LOGIN);
            HTTP.POST(sUrl, sbParam, ref sToken);

            CLog.LOG(LOG_TYPE.SCREEN, "Token Update");
            return true;
        }


        public void ProcResponse(TPMS_CMD eCmd, string sRes, string sCarno = "")
        {
            //Rcv: { "result":true,"httpstatus":"ACCEPTED","data":{ "inRsltTypes":["NORMAL"],"inOutId":130402439,"totalUnpaidAmt":0,"totalCnt":0,"virtualAccount":"","returnVal":"22052314295800000030"},"httpcode":202}]
            //{ "result":true,"data":{ "inRsltTypes":["NORMAL"],"inOutId":389507,"totalUnpaidAmt":"0","totalCnt":"0","virtualAccount":"","returnVal":"22052509355700000002","name":"무인1_통복시장","id":"T18_Muin1","deviceId":""},"httpcode":202,"msg":"정상처리 되었습니다.","httpstatus":"ACCEPTED"}
            //CLog.LOG(LOG_TYPE.SERVER, sRes);
            //string sLogType = "";

            bool bResult = false;
            bool bRetoken = false;
            bool bPeriod = false;
            string sRsType = "";
            JObject jObj;
            JObject jObjData;
            JArray jObjArray;
            try
            {
                if (sRes != null)
                {

                    if (eCmd != TPMS_CMD.AUTO_MINAB)
                    {
                        jObj = JObject.Parse(sRes);
                        //MessageBox.Show((string)jObjData["inOutId"].ToString());
                        bResult = bool.Parse((string)jObj["result"]);


                        if (!bResult)
                        {
                            //MessageBox.Show((string)jObj["httpstatus"]);
                            if ((string)jObj["httpstatus"] == "UNAUTHORIZED")
                            {
                                nTryLogin++;

                                //CLog.LOG(LOG_TYPE.SCREEN, "Last Token : " + sToken);

                                bRetoken = Re_Token(eCmd);
                                //Mu_Out();
                                string sUrl = GetCmdUrl(eCmd);
                                string sResult = HTTP.POST(sUrl, sbLastParam, ref sToken);
                                ProcResponse(eCmd, sResult);
                                sbLastParam.Clear();
                                return;
                            }

                        }
                        else
                        {
                            st_TpmsInfo.stPkInfo.bStatus = true;
                        }


                        //int nHttpCode = (int)jObj["httpcode"];
                        //{ "result":false,"httpcode":401,"msg":"로그인 인증 정보를 찾을 수 없습니다.","httpstatus":"UNAUTHORIZED"}
                        switch (eCmd)
                        {
                            case TPMS_CMD.LOGIN:
                            case TPMS_CMD.CHK_HEALTH:
                                {
                                    if (bResult)
                                    {
                                        //로그인 성공
                                        eLogin = TPMS_LOGIN_STAT.SUCC;
                                        //MessageBox.Show(nCntCheckNow + " :: " + bResult.ToString() + " :: " + CData.arCheckTpms[nCntCheckNow].ToString());
                                    }
                                    else
                                    {
                                        //로그인 실패
                                        eLogin = TPMS_LOGIN_STAT.FAIL;
                                    }
                                }
                                break;
                            case TPMS_CMD.MU_IN:
                                jObjData = (JObject)jObj["data"];
                                CData.arInParamData[6] = "";
                                CData.pDB.Delete_IOCar(sCarno);
                                break;
                            case TPMS_CMD.MU_OUT:
                            case TPMS_CMD.OUT:
                                jObjData = (JObject)jObj["data"];
                                int nInOutId = (int)jObjData["inOutId"];
                                jObjArray = (JArray)jObjData["inRsltTypes"];
                                int nReqAmt = (int)jObjData["reqAmt"];

                                int nPartAmt = (int)jObjData["partAmt"];
                                CData.pDB.Update_IOCar_PartAmt(nPartAmt, sCarno);
                                sLastOutCar[0] = nInOutId.ToString();
                                CLog.LOG(LOG_TYPE.SCREEN, "MU_OUT : " + nInOutId.ToString() + "&" + sCarno);
                                CData.pDB.Update_IOCar_ID(nInOutId, sCarno);
                                for(int i = 0; i < jObjArray.Count; i++)
                                {
                                    if (jObjArray[i].ToString() == "NORMAL")
                                    {
                                        if (jObjArray.Count > 1)
                                        {
                                            for (int a = 0; a < jObjArray.Count; a++)
                                            {
                                                if (jObjArray[a].ToString() != "VISIT")
                                                    bPeriod = true;
                                            }
                                        }

                                        CData.pDB.Update_IOCar_FEE(nReqAmt, sCarno);
                                    }
                                    else if(jObjArray[i].ToString() == "PREPAY")
                                    {
                                        CData.pDB.Update_IOCar_PartAmt(nPartAmt, sCarno);
                                        CLog.LOG(LOG_TYPE.SCREEN, "Prepay partamt=" + nPartAmt);
                                    }
                                    else
                                    {
                                        bPeriod = true;
                                    }
                                }

                                //Console.WriteLine(sTp);
                                //if ((string)jObjData["inRsltTypes"] != "[\"NORMAL\"]")
                                //{
                                //    sRsType = (string)jObjData["fileId"];
                                //    bPeriod = true;
                                //}
                                break;
                            case TPMS_CMD.IMG_UP_NORMAL_IN:
                                //{"result":true,"data":{"reconId":2895,"reconCarNo":null,"reconSts":"C_SUCC","reconUrl":"/tpms/upload//reconfiles/67/679850a6cc19005723d1653e4d05039cedd37386ba3d7ae9b140d5f3b5087f6d.jpg","fileName":"20220526_182358_39??7031.jpg","name":"와부_제1공영_무인1","id":"T8_Muin1","deviceId":""},"msg":"정상처리 되었습니다.","httpstatus":"ACCEPTED","httpcode":202}]
                                jObjData = (JObject)jObj["data"];
                                CData.arInParamData[6] = (string)jObjData["reconId"];
                                CData.pDB.Update_IOCar_IMG(CData.arInParamData[6], sCarno);
                                break;
                            case TPMS_CMD.IMG_UP_NORMAL_OUT:
                                jObjData = (JObject)jObj["data"];
                                CData.arOutParamData[6] = (string)jObjData["fileId"];
                                CData.pDB.Update_IOCar_IMG(CData.arOutParamData[6], sCarno);

                                break;
                            case TPMS_CMD.LST_REG:
                                JArray jObjList = (JArray)jObj["list"];
                                CData.arOutParamData[6] = (string)jObjList["fileId"];
                                break;
                            case TPMS_CMD.LST_REG_FREE:
                                jObjData = (JObject)jObj["data"];
                                //CData.arOutParamData[6] = (string)jObjData["fileId"];
                                break;
                            case TPMS_CMD.CAL_MU:
                                sLastOutCar[0] = "";
                                //CData.pDB.Update_IOCar_ID();
                                //CData.pDB.Delete_IOCar(sCarno);
                                break;
                            case TPMS_CMD.CLEAR_OUT:
                                break;
                            case TPMS_CMD.ALL_INSERT:
                                break;

                        }
                    }

                    //CLog.LOG(LOG_TYPE.SCREEN, "Now2 Token : " + sToken);
                    CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " " + eCmd.ToString() + " RX : " + sRes);
                    if (dfLogTPMS != null)
                        dfLogTPMS(st_TpmsInfo.sID, eCmd.ToString() + " RX : {" + sRes + "}");
                    nTryLogin = 0;
                }
                else
                {
                    sToken = "";
                    st_TpmsInfo.stPkInfo.bStatus = false;
                }
            }
            catch (Exception e)
            {
                if (eCmd != TPMS_CMD.ALL_INSERT)
                {
                    if (!bResult && !bRetoken)
                    {
                        nTryLogin++;
                        CLog.LOG(LOG_TYPE.ERR, "#" + nNowIdx + " ProcResponse Error: " + e.Message);
                        CLog.LOG(LOG_TYPE.SERVER, "#" + nNowIdx + " ProcResponse res: " + sRes);
                        if (dfLogTPMS != null)
                        {
                            //st_TpmsInfo.stPkInfo.bStatus = false;
                            dfLogTPMS(st_TpmsInfo.sID, eCmd.ToString() + " RX : { Error Check Log }");
                        }
                    }
                }
                //CData.arCheckTpms[0] = false;
            }
            finally
            {
            }

            if (nTryLogin >= 5)
            {
                st_TpmsInfo.stPkInfo.bStatus = false;
            }

            if (dfStatusTPMS != null)
            {
                dfStatusTPMS(0, nCntCheckNow);
            }

            if(bPeriod)
            {
                if(dfNotNormal != null)
                {
                    dfNotNormal(sCarno, sRsType);
                }

            }

            if (CData.garOpt1[1] == 0)
            {
                if (eCmd == TPMS_CMD.CAL_MU)
                {
                    if (sCarno != "")
                    {
                        CLEAR_OUT(CData.pDB.Select_IOCar_ID(sCarno).ToString());
                        CData.pDB.Delete_IOCar(sCarno);
                    }
                }
            }
        }

        

        public string GetCmdUrl(TPMS_CMD eCmd)
        {
            string sUrl = "";

            switch(eCmd)
            {
                case TPMS_CMD.LOGIN:
                    {
                        switch(nLoginType)
                        {
                            case 1://부천
                                sUrl = "/tpms/api/scm/login.json";
                                break;
                            case 2://수원
                            case 3: //평택
                            case 4:
                                sUrl = "/tpms/api/actionLogin.json";
                                //sUrl = "/tpms/api/actionLoginCheck.json";
                                break;
                        }    
                    }
                    break;
                case TPMS_CMD.IN:
                case TPMS_CMD.IN_REG:
                case TPMS_CMD.IN_PAY:
                case TPMS_CMD.IN_NEW_NO_OUT:
                case TPMS_CMD.OUT_NOKEY:
                    //일반입차,정기입차,선불입차,미납등록(서버키없음 수동삽입),'일반출차(서버키 없이 계산할때 정기권,수동계산 등등)
                    sUrl = "/tpms/api/stm/inout/add.json";
                    break;
                case TPMS_CMD.IN_EDIT:
                case TPMS_CMD.NO_OUT_INS:
                
                case TPMS_CMD.OUT:
                case TPMS_CMD.OUT_FS:
                case TPMS_CMD.OUT_REG:
                case TPMS_CMD.OUT_DEL:
                case TPMS_CMD.OUT_REFUND:
                    //입차정보 수정 - 유인 출차, 수납전송 없음
                    //미납등록(서버키있음),일반출차,무인출차,정기출차,삭제처리,환불출차
                    sUrl = "/tpms/api/stm/inout/edit.json";
                    break;
                case TPMS_CMD.MU_OUT:
                    sUrl = "/tpms/api/stm/inout/muin_edit.json";

                    break;
                case TPMS_CMD.IN_DEL:
                    //입차 취소
                    sUrl = "/tpms/api/stm/inout/delete.json";
                    break;
                case TPMS_CMD.MU_IN:
                case TPMS_CMD.MU_IN_REG:
                
                    //무인 입차, 무인 정기권 입차
                    sUrl = "/tpms/api/stm/inout/muin_add.json";
                    break;

                case TPMS_CMD.MU_IN_LST:
                    sUrl = "/tpms/api/stm/inout/muin_list.json";
                    break;

                case TPMS_CMD.LST_REG:
                case TPMS_CMD.LST_REG_NEXT:
                    sUrl = "/tpms/api/stm/period/list_sale.json";
                    break;
                case TPMS_CMD.CHK_HEALTH:
                    sUrl = "/tpms/api/stm/inout/health.json";
                    break;
                case TPMS_CMD.AUTO_MINAB:
                    sUrl = "/tpms/api/stm/inout/muin_unpayProc.json";
                    break;
                case TPMS_CMD.CAL_MU:
                    sUrl = "/tpms/api/stm/receipt/muin_add.json";
                    break;
                case TPMS_CMD.IMG_UP_NORMAL_IN:
                    sUrl = "/tpms/api/scm/recon/muin_upload.json";
                    //sUrl = "/tpms/api/scm/file/inout/upload.json";
                    break;
                case TPMS_CMD.IMG_UP_NORMAL_OUT:
                    sUrl = "/tpms/api/scm/file/inout/upload.json";
                    //sUrl = "/tpms/api/scm/file/inout/upload.json";
                    break;
                case TPMS_CMD.LST_REG_FREE:
                    sUrl = "/tpms/api/stm/freecar/muin_list.json";
                    break;
                case TPMS_CMD.LST_WORKER:
                    sUrl = "/tpms/api/scm/emp/list.json";
                    break;
                case TPMS_CMD.CLEAR_OUT:
                    sUrl = "/tpms/api/stm/inout/muin_finishedOutYn.json";
                    break;
                case TPMS_CMD.ALL_INSERT:
                    sUrl = "/tpms/api/stm/freecar/allInsert.json";
                    break;
                case TPMS_CMD.FREE_UPDATE:
                    sUrl = "/tpms/api/stm/freecar/updateAction.json";
                    break;
                case TPMS_CMD.VISIT_INSERT:
                    sUrl = "/tpms/api/stm/visitcar/insertAction.json";
                    break;
                case TPMS_CMD.VISIT_UPDATE:
                    sUrl = "/tpms/api/stm/visitcar/updateAction.json";
                    break;

            }

            sUrl = string.Format("http://{0}:{1}{2}", CData.sTpmsIP, CData.sTpmsPort, sUrl);

            return sUrl;
        }
    }
}

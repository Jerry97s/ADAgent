using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ADAgent.DATA;
using ADAgent.UTIL;
using DH.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iNervMng.TPMS
{
    public enum WS_CMD
    {
        STT_1_1001 = 0,
        STT_1_1002,
        STT_1_1003,
        STT_1_1004,
        STT_1_1005,
        STT_1_1006,
        STT_1_2001,
        STT_1_2002,
        STT_1_2011,
        STT_1_2012,
        STT_1_2013,
        STT_1_2014,
        STT_1_2015,
        STT_1_3001,
        STT_1_3002,
        STT_1_3003,
        STT_1_3004,
        STT_1_3005,
        STT_1_3006,
        STT_2_1001,
        STT_2_1002,
        STT_2_1003,
        STT_2_3001,
        STT_2_3002,
        STT_2_3003,
        STT_2_3004,
        STT_2_4001,
        STT_2_5001,
        STT_2_5002,
        STT_2_5003,
        STT_2_5004,
        STT_2_6001,
        STT_2_7001,
        STT_2_8001,
        STT_2_8002,
        STT_2_9001
    }

    class CWS
    {
        CWebSocket pWs = null;
        string sUrl;
        string sLoginID;
        int nNowIdx = 0;
        int nAliveTime = 5000;
        System.Timers.Timer tAliveTimer = null;
        public WSInfo st_WsInfo;
        int nReconnTime = 5000;
        System.Timers.Timer tReconnTimer = null;
        List<string> arOutData = new List<string>();
        public delegate void DF_ProcWS_List(string sID, WS_CMD eCmd, List<string> arData);
        public delegate void DF_ProcWS(string sID, WS_CMD eCmd, string sData);
        public delegate void DF_ProcStatWS(bool bChk);
        public DF_ProcWS_List dfProcList = null;
        public DF_ProcWS dfProc_GT = null;
        public DF_ProcStatWS dfProcStat = null;

        public CWS(int nIdx)
        {
            nNowIdx = nIdx;

        }
        public bool Connect(string sUrl, string sID,int nAliveTime = 5000)
        {
            bool bRes = false;
            try
            {
                DisConnect();

                this.nAliveTime = nAliveTime;
                this.sUrl = sUrl;               //"ws://210.221.94.114:3000/tpms/carmonitoring.do";
                this.sLoginID = sID;
                if(pWs != null)
                {
                    pWs.DisConnect();
                    pWs = null;
                }
                if (pWs == null)
                {
                    pWs = new CWebSocket(nNowIdx);
                }
                pWs.TimeOut = 3000;
                pWs.RcvProc = Tpms_RCV_Process;
                pWs.Connect(new Uri(this.sUrl));
                while (!pWs.IsConnected() && pWs.WS_State == CWebSocket.WS_CON_STATE.CONNECTING)
                {
                    Thread.Sleep(10);
                }
                CLog.LOG(LOG_TYPE.WSK_WS, "WebSocket Connect: " + sUrl + " : " + sLoginID + " : ");
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "WebSocket Connect Failed: " + e.Message);
            }

            bRes = pWs.IsConnected();
            if(bRes)
            {
                //Login
                string sLoginData = "";
                sLoginData = "{ emp_no = \"" + sLoginID + "\", cmd = \"TTS_1_0000\" }";
                CLog.LOG(LOG_TYPE.WSK_WS, "WSK_WS TX : " + sLoginData);
                pWs.Login(sLoginData);
                st_WsInfo.bStatus = true;
                //if(tAliveTimer == null)
                //{
                //    tAliveTimer = new System.Timers.Timer(nAliveTime);
                //    tAliveTimer.Elapsed += Timer_Alive;
                //}

                //tAliveTimer.Start();
            }
            
            return bRes;
        }


        public void DisConnect()
        {
            if(pWs != null)
            {
                pWs.DisConnect();
                pWs = null;
            }
        }

        public void Close()
        {
            if (pWs != null)
            {
                pWs.DisConnect();
                pWs = null;
            }
            if(tAliveTimer != null)
                tAliveTimer.Stop();
        }

        private void Timer_Alive(object sender, System.Timers.ElapsedEventArgs e)
        {
            tAliveTimer.Stop();

            if (CheckAlive())
            {
                if (dfProcStat != null)
                {

                    dfProcStat(true);
                }

                tAliveTimer.Start();
            }
            else
            {
                if (tReconnTimer == null)
                {
                    tReconnTimer = new System.Timers.Timer(nReconnTime);
                    tReconnTimer.Elapsed += Timer_ReConnect;
                }

                tReconnTimer.Start();
            }                    
        }

        private void Timer_ReConnect(object sender, System.Timers.ElapsedEventArgs e)
        {
            tReconnTimer.Stop();
            Connect(sUrl, sLoginID, nAliveTime);
        }

        public bool CheckAlive()
        {
            bool bAlive = false;
            bAlive = pWs.IsConnected();
            st_WsInfo.bStatus = bAlive;
            return bAlive;
        }

        public void Tpms_RCV_Process(string sRcv)
        {
            try
            {
                Console.WriteLine("DF_RCV_Proc: " + sRcv);

                sRcv = sRcv.Trim();

            //sRcv = sRcv.Replace("\0", string.Empty);
            JObject json = JObject.Parse(sRcv);
            JObject jObjData;
            CLog.LOG(LOG_TYPE.WSK_WS, sRcv);
                if (json["cmd"].ToString() == "802")
                {
                    //로그인 성공
                }
                else
                {
                    Enum.TryParse(json["cmd"].ToString(), out WS_CMD cmd);
                    switch (cmd)
                    {
                        case WS_CMD.STT_1_1001:     //출차API실행
                            func_STT_1_1001(json["result"].ToString());
                            break;
                        case WS_CMD.STT_1_1002:     //수납
                            func_STT_1_1002(json["result"].ToString());
                            break;
                        case WS_CMD.STT_1_1003:     //근무지정보 다운
                            func_STT_1_1003();
                            break;
                        case WS_CMD.STT_1_1004:     //요금제 다운
                            func_STT_1_1004();
                            break;
                        case WS_CMD.STT_1_1005:     //정기권 다운
                            func_STT_1_1005();
                            break;
                        case WS_CMD.STT_1_1006:     //무료차량 다운
                            func_STT_1_1006();
                            break;
                        case WS_CMD.STT_1_2001:     //정산기 재가동
                            func_STT_1_2001();
                            break;
                        case WS_CMD.STT_1_2002:     //출구LPR 재가동
                            func_STT_1_2002();
                            break;
                        case WS_CMD.STT_1_2011:     //출구열기
                            func_STT_1_2011();
                            break;
                        case WS_CMD.STT_1_2012:     //출구닫힘
                            func_STT_1_2012();
                            break;
                        case WS_CMD.STT_1_2013:     //출구고정
                            func_STT_1_2013();
                            break;
                        case WS_CMD.STT_1_2014:     //출구고정해제
                            func_STT_1_2014();
                            break;
                        case WS_CMD.STT_1_2015:     //출구리셋
                            func_STT_1_2015();
                            break;
                        case WS_CMD.STT_1_3001:     //입구LPR 재가동
                            func_STT_1_3001();
                            break;
                        case WS_CMD.STT_1_3002:     //입구열기
                            func_STT_1_3002();
                            break;
                        case WS_CMD.STT_1_3003:     //입구닫힘
                            func_STT_1_3003();
                            break;
                        case WS_CMD.STT_1_3004:     //입구고정
                            func_STT_1_3004();
                            break;
                        case WS_CMD.STT_1_3005:     //입구고정해제
                            func_STT_1_3005();
                            break;
                        case WS_CMD.STT_1_3006:     //입구리셋
                            func_STT_1_3006();
                            break;
                        case WS_CMD.STT_2_1001: //출
                            func_STT_2_1001(json["result"].ToString());
                            break;
                        case WS_CMD.STT_2_1002: //결
                            func_STT_2_1002(json["result"].ToString());
                            break;
                        case WS_CMD.STT_2_1003: //입
                            func_STT_2_1003(json["result"].ToString());
                            break;
                        case WS_CMD.STT_2_3001:     //정기권 영수증 인쇄
                            func_STT_2_3001(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_3002:     //일반 영수증 인쇄
                            func_STT_2_3002(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_3003:     //정기권 미납결제(확인필요)
                            func_STT_2_3003(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_3004:     //미납청구 영수증
                            func_STT_2_3004();
                            break;
                        case WS_CMD.STT_2_4001:     //주차 가용대수 수정
                            func_STT_2_4001(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_5001:     //서버할인계산
                            jObjData = (JObject)json["data"];
                            func_STT_2_5001(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_5002:     //서버수동계산
                            func_STT_2_5002();
                            break;
                        case WS_CMD.STT_2_5003:     //KAKAO_MANUAL 출차 처리
                            func_STT_2_5003(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_5004:     //선납권 차감
                            func_STT_2_5004(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_6001:     //차량번호 수정
                            func_STT_2_6001(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_7001:     //차량번호 삭제
                            func_STT_2_7001(json["data"].ToString());
                            break;
                        case WS_CMD.STT_2_8001:     //무인 계산 취소
                            func_STT_2_8001();
                            break;
                        case WS_CMD.STT_2_8002:     //서버 미납처리
                            func_STT_2_8002();
                            break;
                        case WS_CMD.STT_2_9001:     //무인 할인 취소

                            func_STT_2_9001();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "WS_RCV_ERR = " + ex.ToString());
            }
        }

        private void func_STT_1_1001(string sData)
        {
           

        }
        private void func_STT_1_1002(string sData)
        {

            //로그인실행
        }
        private void func_STT_1_1003()
        {
            //근무지정보 다운
        }
        private void func_STT_1_1004()
        {
            //요금제 다운
        }
        private void func_STT_1_1005()
        {
            //정기권 다운
        }
        private void func_STT_1_1006()
        {
            //무료차량 다운
        }
        private void func_STT_1_2001()
        {
            //정산기 재가동
        }
        private void func_STT_1_2002()
        {
            //출구LPR 재가동
        }
        private void func_STT_1_2011()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_2011, "1");
            //출구열기
        }
        private void func_STT_1_2012()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_2012, "2");
            //출구닫힘
        }
        private void func_STT_1_2013()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_2013, "3");
            //출구고정
        }
        private void func_STT_1_2014()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_2014, "4");
            //출구고정해제
        }
        private void func_STT_1_2015()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_2015, "5");
            //출구리셋
        }
        private void func_STT_1_3001()
        {
            //입구LPR 재가동
        }
        private void func_STT_1_3002()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_3002, "1");
            //입구열기
        }
        private void func_STT_1_3003()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_3003, "2");
            //입구닫힘
        }
        private void func_STT_1_3004()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_3004, "3");
            //입구고정
        }
        private void func_STT_1_3005()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_3005, "4");
            //입구고정해제
        }
        private void func_STT_1_3006()
        {
            if (dfProc_GT != null)
                dfProc_GT(sLoginID, WS_CMD.STT_1_3006, "5");
            //입구리셋
        }
        private void func_STT_2_1001(string sData)
        {
            try
            {
                //{ "extraAmt":0,"discountAmt":0,"carNo":"115서9298","inDttm":"2022-07-18 12:30:10","reqAmt":0,"outDttm":"2022-07-18 12:30:27","inTy":"DEF"},"code":"000","cmd":"STT_2_1001"}
                //{ "extraAmt":"0","discountAmt":"0","carNo":"서울72바9773","inDttm":"2022-08-30 13:48:55","reqAmt":"0","outDttm":"2022-08-30 13:55:41","partAmt":"0","empNo":"T5_Muin2","inTy":"PERIOD"}
                List<string> arData = new List<string>();
                JObject json = JObject.Parse(sData);

            //JObject jObjData;
            //jObjData = (JObject)json["data"];

                arData.Add(json["extraAmt"].ToString());               //? 0
                arData.Add(json["discountAmt"].ToString());               //? 1
                arData.Add(json["carNo"].ToString());               //? 2
                arData.Add(json["inDttm"].ToString());               //? 3
                arData.Add(json["reqAmt"].ToString());               //? 4
                arData.Add(json["outDttm"].ToString());               //? 5
                arData.Add(json["partAmt"].ToString());               //? 6
                arData.Add(json["empNo"].ToString());               //? 7
                arData.Add(json["inTy"].ToString());               //? 8

                if (dfProcList != null)
                    dfProcList(sLoginID, WS_CMD.STT_2_1001, arData);

            }
            catch(Exception ex)
            {
                CLog.LOG(LOG_TYPE.WSK_WS, "WS_2_1001 Failed = " + ex.ToString());
            }
        }

        private void func_STT_2_1002(string sData)
        {
            try
            {
                //{ "cardMemberNo":"840498859","bankTransDttm":"","cardTradeMedia":"1",
                //"cardApprovalNo":"00862948","cardIssueCorpNm":"현대비자개인","cardNo":"4017-6200-****-690*","cardPurchCorpNm":"현대카드사",
                //"receiptTy":"DEF","cmmsonAmt":"4090","carNo":"399부8346","inDttm":"2022-08-19 17:01:24","cardTradeNo":"221263877428",
                //"receiptAmt":"45000","receiptWay":"CARD","receiptWorker":"T9_Kiosk2","cardInstallmentMonth":"0"}
                List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
                //JObject jObjData;
                //jObjData = (JObject)json["data"];
                arData.Add(json["cardMemberNo"].ToString());             //0
                arData.Add(json["bankTransDttm"].ToString());            //1
                arData.Add(json["cardTradeMedia"].ToString());           //2
                arData.Add(json["cardApprovalNo"].ToString());           //3
                arData.Add(json["cardIssueCorpNm"].ToString());          //4
                arData.Add(json["cardNo"].ToString());                   //5
                arData.Add(json["cardPurchCorpNm"].ToString());          //6
                arData.Add(json["receiptTy"].ToString());                //7
                arData.Add(json["cmmsonAmt"].ToString());                //8
                arData.Add(json["carNo"].ToString());                    //9
                arData.Add(json["inDttm"].ToString());                   //10
                arData.Add(json["cardTradeNo"].ToString());              //11
                arData.Add(json["receiptAmt"].ToString());               //12
                arData.Add(json["receiptWay"].ToString());               //13
                arData.Add(json["receiptWorker"].ToString());            //14
                arData.Add(json["cardInstallmentMonth"].ToString());     //15
                CLog.LOG(LOG_TYPE.SCREEN, "CAL #0" + arData[9]);

            if (dfProcList != null)
                dfProcList(sLoginID, WS_CMD.STT_2_1002, arData);
        }
            catch(Exception ex)
            {
                CLog.LOG(LOG_TYPE.WSK_WS, "WS_2_1002 Failed = " + ex.ToString());
            }
}
        private void func_STT_2_1003(string sData)
        {
            try
            {
                List<string> arData = new List<string>();
                JObject json = JObject.Parse(sData);
                //JObject jObjData;
                //jObjData = (JObject)json["data"];

                arData.Add(json["extraAmt"].ToString());               //? 0
                arData.Add(json["discountAmt"].ToString());               //? 1
                arData.Add(json["carNo"].ToString());               //? 2
                arData.Add(json["inDttm"].ToString());               //? 3
                arData.Add(json["reqAmt"].ToString());               //? 4
                arData.Add(json["outDttm"].ToString());               //? 5

                arData.Add(json["empNo"].ToString());              //? 6

                arData.Add(json["inTy"].ToString());               //? 7


                if (dfProcList != null)
                    dfProcList(sLoginID, WS_CMD.STT_2_1003, arData);
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.WSK_WS, "WS_2_1003 Failed = " + ex.ToString());
            }
        }

        private void func_STT_2_3001(string sData)
        {
            //정기권 영수증 인쇄
            Console.WriteLine("func_STT_2_3001: " + sData);
            //string[] saDatas = new string[16];
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["periodId"].ToString());               //정기권ID
            arData.Add(json["parkingName"].ToString());            //주차장 명
            arData.Add(json["carNo"].ToString());                  //차량번호
            arData.Add(json["useStartDt"].ToString());             //시작일
            arData.Add(json["useEndDT"].ToString());               //종료일
            arData.Add(json["procStsCd"].ToString());              //처리상태코드
            arData.Add(json["receiptParkingSectn"].ToString());    //수납주차구간ID
            arData.Add(json["receiptWorker"].ToString());          //수납주차요원ID
            arData.Add(json["useAmt"].ToString());                 //이용금액
            arData.Add(json["tagAmt"].ToString());                 //태그금액
            arData.Add(json["receiptDttm"].ToString());           //수납일시
            arData.Add(json["receiptWay"].ToString());            //수납형태
            arData.Add(json["receiptDivCd"].ToString());          //수납구분
            arData.Add(json["receiptAmt"].ToString());            //수납금액

            //정기권 영수증 정보 전송
        }
        private void func_STT_2_3002(string sData)
        {
            //일반 영수증 인쇄
            Console.WriteLine("func_STT_2_3002: " + sData);

            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //입출차 ID
            arData.Add(json["parkingName"].ToString());           //주차장 명
            arData.Add(json["carNo"].ToString());                 //차량번호
            arData.Add(json["discountCd"].ToString());            //할인코드
            arData.Add(json["discountCdNm"].ToString());          //할인명
            arData.Add(json["extraCd"].ToString());               //할증코드
            arData.Add(json["extraCdNm"].ToString());             //할증명
            arData.Add(json["inDttm"].ToString());                //입차시간
            arData.Add(json["outDttm"].ToString());               //출차시간
            arData.Add(json["useTm"].ToString());                 //주차시간
            arData.Add(json["useAmt"].ToString());                //주차요금
            arData.Add(json["discountAmt"].ToString());           //할인요금
            arData.Add(json["extraAmt"].ToString());              //할증요금
            arData.Add(json["reqAmt"].ToString());                //청구금액
            arData.Add(json["fullPayDiv"].ToString());            //PRT001: 완납    PRT002:일부수납     PRT003:미납
            arData.Add(json["virtualAccount"].ToString());        //가상계좌번호

            if(arData[14] == "PRT001")
            {
                //완납
            }
            else
            {
                //일부수납 and 미납: PRT002, PRT003 - 청구서
            }
        }
        private void func_STT_2_3003(string sData)
        {
            //정기권 미납결제(확인필요)
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["periodId"].ToString());               //정기권ID
            arData.Add(json["parkingName"].ToString());            //주차장 명
            arData.Add(json["carNo"].ToString());                  //차량번호
            arData.Add(json["useStartDt"].ToString());             //시작일
            arData.Add(json["useEndDT"].ToString());               //종료일
            arData.Add(json["procStsCd"].ToString());              //처리상태코드
            arData.Add(json["receiptParkingSectn"].ToString());    //수납주차구간ID
            arData.Add(json["receiptWorker"].ToString());          //수납주차요원ID
            arData.Add(json["useAmt"].ToString());                 //이용금액
            arData.Add(json["tagAmt"].ToString());                 //태그금액
            arData.Add( json["receiptDttm"].ToString());           //수납일시
            arData.Add( json["receiptWay"].ToString());            //수납형태
            arData.Add( json["receiptDivCd"].ToString());          //수납구분
            arData.Add( json["receiptAmt"].ToString());            //수납금액
        }
        private void func_STT_2_3004()
        {
            //미납청구 영수증
        }
        private void func_STT_2_4001(string sData)
        {
            //주차 가용대수 수정
            //string[] saDatas = new string[16];
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["parkCount"].ToString());            //수납금액
        }
        private void func_STT_2_5001(string sData)
        {
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //정기권ID
            arData.Add(json["parkingName"].ToString());            //주차장 명
            arData.Add(json["carNo"].ToString());                  //차량번호
            arData.Add(json["useStartDt"].ToString());             //시작일
            arData.Add(json["useEndDT"].ToString());               //종료일
            arData.Add(json["procStsCd"].ToString());              //처리상태코드
            arData.Add(json["receiptParkingSectn"].ToString());    //수납주차구간ID
            arData.Add(json["receiptWorker"].ToString());          //수납주차요원ID
            arData.Add(json["useAmt"].ToString());                 //이용금액
            arData.Add(json["tagAmt"].ToString());                 //태그금액
            arData.Add(json["receiptDttm"].ToString());           //수납일시
            arData.Add(json["receiptWay"].ToString());            //수납형태
            arData.Add(json["receiptDivCd"].ToString());          //수납구분
            arData.Add(json["receiptAmt"].ToString());            //수납금액
            //서버할인계산
            if (dfProcList != null)
                dfProcList(sLoginID, WS_CMD.STT_2_5001, arData);
        }
        private void func_STT_2_5002()
        {
            //서버수동계산
        }
        private void func_STT_2_5003(string sData)
        {
            //KAKAO_MANUAL 출차 처리
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //입출차 ID
            arData.Add(json["parkingName"].ToString());           //주차장 명
            arData.Add(json["carNo"].ToString());                 //차량번호
            arData.Add(json["discountCd"].ToString());            //할인코드
            arData.Add(json["discountCdNm"].ToString());          //할인명
            arData.Add(json["extraCd"].ToString());               //할증코드
            arData.Add(json["extraCdNm"].ToString());             //할증명
            arData.Add(json["inDttm"].ToString());                //입차시간
            arData.Add(json["outDttm"].ToString());               //출차시간
            arData.Add(json["useTm"].ToString());                 //주차시간
            arData.Add(json["useAmt"].ToString());                //주차요금
            arData.Add(json["discountAmt"].ToString());           //할인요금
            arData.Add(json["extraAmt"].ToString());              //할증요금
            arData.Add(json["reqAmt"].ToString());                //청구금액
            arData.Add(json["fullPayDiv"].ToString());            //PRT001: 완납    PRT002:일부수납     PRT003:미납
            arData.Add(json["partAmt"].ToString());               //일부수납금액

            if (dfProc_GT != null)
                dfProcList(sLoginID, WS_CMD.STT_2_5003, arData);
        }
        private void func_STT_2_5004(string sData)
        {
            //선납권 차감
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //입출차 ID
            arData.Add(json["parkingName"].ToString());           //주차장 명
            arData.Add(json["carNo"].ToString());                 //차량번호
            arData.Add(json["discountCd"].ToString());            //할인코드
            arData.Add(json["discountCdNm"].ToString());          //할인명
            arData.Add(json["extraCd"].ToString());               //할증코드
            arData.Add(json["extraCdNm"].ToString());             //할증명
            arData.Add(json["inDttm"].ToString());                //입차시간
            arData.Add(json["outDttm"].ToString());               //출차시간
            arData.Add(json["useTm"].ToString());                 //주차시간
            arData.Add(json["useAmt"].ToString());                //주차요금
            arData.Add(json["discountAmt"].ToString());           //할인요금
            arData.Add(json["extraAmt"].ToString());              //할증요금
            arData.Add(json["prepayTicketAmt"].ToString());       //선납권수납금액
            arData.Add(json["reqAmt"].ToString());                //청구금액
            arData.Add(json["fullPayDiv"].ToString());            //PRT001: 완납    PRT002:일부수납     PRT003:미납
            arData.Add(json["partAmt"].ToString());               //일부수납금액

            if (dfProc_GT != null)
                dfProcList(sLoginID, WS_CMD.STT_2_5004, arData);
        }
        private void func_STT_2_6001(string sData)
        {
            //차량번호 수정
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //입출차 ID
            arData.Add(json["carNo"].ToString());                 //차량번호

            if (dfProc_GT != null)
                dfProcList(sLoginID, WS_CMD.STT_2_6001, arData);
        }
        private void func_STT_2_7001(string sData)
        {
            //차량번호 삭제
            List<string> arData = new List<string>();
            JObject json = JObject.Parse(sData);
            arData.Add(json["inOutId"].ToString());               //입출차 ID

            if (dfProc_GT != null)
                dfProcList(sLoginID, WS_CMD.STT_2_7001, arData);
        }
        private void func_STT_2_8001()
        {
            //무인 계산 취소
        }
        private void func_STT_2_8002()
        {
            //서버 미납처리
        }
        private void func_STT_2_9001()
        {

            //무인 할인 취소
            //if (dfProc != null)
            //    dfProcList(WS_CMD.STT_2_9001, arData);
        }
    }
}

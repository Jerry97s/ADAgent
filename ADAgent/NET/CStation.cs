using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ADAgent.UTIL;
using ADAgent.DATA;
using System.Threading;

namespace ADAgent.NET
{
    public class CStation
    {
        CWinSock pStation = null;
        string sID = "";
        int nNetIdx = 0;
        int nNowIdx = 0;
        int nDivIdx = 0;
        //2359 0001
        string sDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string slastDate = "";
        string sNowDate = "";
        string sNowDate_Hour = "";
        string sNowDate_Time = "";
        int nHealthCnt = 0;
        string IP;
        int PORT;
        int nReCnt = 0;

        bool bExit = false;
        bool bNet = false; // false 서버, true 클라
        int nRetryCnt = 0;

        public string LastRecvStatusData = "";
        public string LastSendData = "";

        public NetInfo st_NetInfo;
        System.Timers.Timer pTimer = new System.Timers.Timer();

        public delegate void DF_Parse(string sDiv, bool bIO, string sRCV, string sFolder, bool bTest, bool bPass);
        public DF_Parse dfParse = null;

        public delegate void DF_Parse_GT(string sDiv, bool bIO, string sRCV);
        public DF_Parse_GT dfParse_GT = null;

        public delegate void DF_ConStats(int nDiv, bool bConn,int nNet, int nIdx_Net, int nIdx, bool bDif, string sStartDttm, string sEndDttm);
        public DF_ConStats dfConStats = null;

        public string ID
        {
            get { return sID; }
            set { sID = value; }
        }

        public void Init_Client(int nDiv, int nNIdx, int nIdx)
        {
            bExit = false;
            nDivIdx = nDiv;
            nNetIdx = nNIdx;
            nNowIdx = nIdx;
            pStation = new CWinSock(this.ConnComplete, this.SendComplete, this.RecvComplete);
            pStation.Connect(st_NetInfo.sIP, st_NetInfo.nPort);

            pTimer.Interval = 2000;
            pTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            pTimer.Start();

        }

        public void Init_Server(int nDiv, int nNIdx, int nIdx)
        {
            nDivIdx = nDiv;
            nNetIdx = nNIdx;
            nNowIdx = nIdx;

            pStation = new CWinSock(this.ConnComplete, this.SendComplete, this.RecvComplete);
            pStation.Listen(st_NetInfo.nPort);
        }

        public void Close()
        {
            bExit = true;

            pTimer.Stop();
            pTimer.Close();


            if (pStation != null)
                pStation.Close();

        }

        public void ReConnect()
        {
            CLog.LOG(LOG_TYPE.STATION, "Not Connected - ReTry Close");
            pStation.Close();
            //System.Threading.Thread.Sleep(1000);

            Stopwatch pStopwatch = new Stopwatch();
            pStopwatch.Start();
            while (pStopwatch.Elapsed.Seconds < 1)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            pStopwatch.Stop();

            CLog.LOG(((nDivIdx == 0) ? LOG_TYPE.STATION : LOG_TYPE.GT), "Not Connected - ReTry Connect");
            pStation.Connect(st_NetInfo.sIP, st_NetInfo.nPort);
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            pTimer.Stop();

            try
            {
                //bCheck = false;
                if (pStation.CONNECT_STATE != 7)
                {

                    dfConStats(nDivIdx, false, st_NetInfo.nType, nNetIdx, nNowIdx, false, "", "");
                    ReConnect();


                }
                else
                {

                    if (dfConStats != null)
                        dfConStats(nDivIdx, true, st_NetInfo.nType, nNetIdx, nNowIdx, false, "", "");

                    //ReConnect(); //일단 주석처리
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }
            pTimer.Start();
            
        }


        public void ConnComplete()
        {
            if (pStation.CONNECT_STATE == 7)
            {
                //접속 성공
                if (dfConStats != null)
                    dfConStats(nDivIdx, true, st_NetInfo.nType, nNetIdx, nNowIdx, false, "", "");

            }
            else
            {
                {
                    //접속 실패
                    if (dfConStats != null)
                        dfConStats(nDivIdx, false, st_NetInfo.nType, nNetIdx, nNowIdx, false, "", "");
                }
            }

        }

        public void SendComplete()
        {
        }

        public void RecvComplete(string sRCV, bool bPass = false)
        {
            string sRCV_Health = "";
            if (dfParse != null)
            {
                try
                {
                    sRCV_Health = sRCV;
                    //sRCV_Health = sRCV.Replace(" ", "");
                    //sRCV_Health = sRCV_Health.Replace(null, "");

                    if (sRCV_Health != "" || sRCV_Health != null)
                    {
                        sDate = DateTime.Now.ToString("mm");
                        sNowDate = DateTime.Now.ToString("yyyy-MM-dd");
                        sNowDate = DateTime.Now.ToString("HH");
                        sNowDate_Time = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss");

                    }

                    CLog.LOG(LOG_TYPE.SCREEN, "Recv-Health : " + sRCV_Health + "&NowDate :" + sNowDate + "NowDate_Time" + sNowDate_Time);

                }
                catch (Exception)
                {

                }
                finally
                {
                }

                dfParse("#" + nNowIdx.ToString() + "#" + nNetIdx.ToString(), (st_NetInfo.stLPRInfo.nIOType == 0) ? true : false, sRCV, st_NetInfo.stLPRInfo.sFolder, false, bPass);
            }
            if (dfParse_GT != null)
                dfParse_GT("#" + nNowIdx.ToString() + "#" + nNetIdx.ToString(), (st_NetInfo.stGtInfo.nIOType == 0) ? true : false, sRCV);
        
        }

        public void Send(bool bAlive, string sCmd)
        {
            string sData = "";
            sData = "OK";

            if (!bAlive)
                sData = sCmd;

            try
            {
                CLog.LOG(LOG_TYPE.STATION, "SendCmd: " + sData);

                //접속 성공
                pStation.Send(sData);
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "Send Failed: " + e.Message);
                pStation.CONNECT_STATE = 0;
            }
            finally
            {
            }
        }

        public bool Gt_Cmd_Send(string sCmd)
        {
            if (pStation == null)
                return false;

            if (pStation.CONNECT_STATE == 7)
            {
                CLog.LOG(LOG_TYPE.GT, ((st_NetInfo.nType == 0) ? "S" : "C") + (st_NetInfo.sIP) + "/" + (st_NetInfo.nPort.ToString()) + " GT TX : " + sCmd);
                pStation.Send(sCmd);
            }

            return true;
        }
        public bool Gt_Close(string sCmd)
        {
            if (pStation == null)
                return false;

            if (pStation.CONNECT_STATE == 7)
            {
                CLog.LOG(LOG_TYPE.GT, ((st_NetInfo.nType == 0) ? "S" : "C") + (st_NetInfo.sIP) + "/" + (st_NetInfo.nPort.ToString()) + " GT TX : " + sCmd);
                pStation.Send(sCmd);
            }

            return true;
        }
        public bool Gt_UpLock(string sCmd)
        {
            if (pStation == null)
                return false;

            if (pStation.CONNECT_STATE == 7)
            {
                CLog.LOG(LOG_TYPE.GT, ((st_NetInfo.nType == 0) ? "S" : "C") + (st_NetInfo.sIP) + "/" + (st_NetInfo.nPort.ToString()) + " GT TX : " + sCmd);
                pStation.Send(sCmd);
            }

            return true;
        }
        public bool Gt_UnLock(string sCmd)
        {
            return true;
        }

        public bool Gt_None(string sCmd)
        {
            return true;
        }

        public bool Gt_Free(string sCmd)
        {
            return true;
        }

    }
}

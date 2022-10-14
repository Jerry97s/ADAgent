using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using ADAgent.UTIL;
using ADAgent.DATA;
using iNervMCS.UTIL;

namespace DH.NET
{
    class CWebSocket
    {
        public enum WS_CON_STATE
        {
            READY = 0,
            CONNECTING = 1,
            CONNECTED = 2,
            CON_FAILED = 3
        }
        public delegate void DF_SendWSStat(int nMach);
        public DF_SendWSStat dF_SendWSStat;
        ClientWebSocket ws = null;
        WebSocket wws = null;
        System.Timers.Timer timer = null;
        bool bTimeOut = false;
        int nTimeOut = 3000;
        int nNowIdx = 0;
        WS_CON_STATE eState = WS_CON_STATE.READY;

        public CWebSocket(int nIdx)
        {
            nNowIdx = nIdx;
        }
        public int TimeOut
        {
            get { return nTimeOut; }
            set { nTimeOut = value; }
        }

        public WS_CON_STATE WS_State
        {
            get { return eState; }
            set { eState = value; }
        }

        public delegate void DF_RecvProc(string sRcv);
        public DF_RecvProc RcvProc = null;
        public async Task Connect(Uri uri)//, DF_RecvProc rcvProc = null)
        {
            try
            {
                ws = new ClientWebSocket();
                //ws.Options.SetRequestHeader("HOST", uri.ToString());
                //ws.Options.SetRequestHeader("Connection", "Upgrade");
                ws.Options.Cookies = new System.Net.CookieContainer(2);
                ws.Options.SetRequestHeader("Sec-WebSocket-Key", "dGhlIHNhbXBsZSBub25jZQ==");

                //ws.Options.SetRequestHeader();
                //ws.Options.SetRequestHeader("Sec-WebSocket-Version", "13");
                //ws.Options.SetRequestHeader("Upgrade", "WebSocket");

                //ws.Options.SetRequestHeader("Keep-Alive", "timeout=300, max=100");
                //ws.Options.SetRequestHeader("Upgrade", "WebSocket");
                //ws.Options.

                //ws.Options.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");

                //ws.Options.SetRequestHeader("Connection", "Keep-Alive");
                //ws.Options.SetRequestHeader("Keep-Alive", "timeout=300, max=100");
                //ws.Options.SetRequestHeader("Sec-WebSocket-Protocol", "ws");

                timer = new System.Timers.Timer(nTimeOut);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                bTimeOut = false;
                WS_State = WS_CON_STATE.CONNECTING;
                //CancellationToken eToken = new CancellationToken();
                await ws.ConnectAsync(uri, CancellationToken.None);
                //wws.
            }
            catch (Exception)
            {
                WS_State = WS_CON_STATE.CON_FAILED;
                if(timer != null)
                {
                    stopConnectTimer();
                }
            }

        }
        public async Task Ping(Uri uri)//, DF_RecvProc rcvProc = null)
        {
            try
            {
                //ws.Options.Cookies = new System.Net.CookieContainer(2);
                //ws.Options.SetRequestHeader("HOST", uri.ToString());
                //ws.Options.SetRequestHeader("Connection", "Upgrade");
                //ws.Options.SetRequestHeader("Sec-WebSocket-Key", "dGhlIHNhbXBsZSBub25jZQ==");

                //ws.Options.SetRequestHeader();
                //ws.Options.SetRequestHeader("Sec-WebSocket-Version", "13");
                //ws.Options.SetRequestHeader("Upgrade", "WebSocket");

                //ws.Options.SetRequestHeader("Keep-Alive", "timeout=300, max=100");
                //ws.Options.SetRequestHeader("Upgrade", "WebSocket");
                //ws.Options.

                //ws.Options.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");

                //ws.Options.SetRequestHeader("Connection", "Keep-Alive");
                //ws.Options.SetRequestHeader("Keep-Alive", "timeout=300, max=100");
                //ws.Options.SetRequestHeader("Sec-WebSocket-Protocol", "ws");
                string sSend = "GET /tpms/carmonitoring.do HTTP/1.1 Host: 210.221.94.114:3000 Upgrade: WebSocket Connection: Upgrade Sec - WebSocket - Key: dGhlIHNhbXBsZSBub25jZQ == Sec - WebSocket - Version: 13 Sec - WebSocket - Extensions: permessage - deflate; client_max_window_bits";

                byte[] sendData = Encoding.UTF8.GetBytes(sSend);

               await ws.SendAsync(new ArraySegment<byte>(sendData), WebSocketMessageType.Text, true, CancellationToken.None);
                //await ws.ConnectAsync(uri, CancellationToken.None);
                //CancellationToken eToken = new CancellationToken();
                //wws.
            }
            catch (Exception)
            {
                WS_State = WS_CON_STATE.CON_FAILED;
                if (timer != null)
                {
                    stopConnectTimer();
                }
            }

        }


        /// <summary>
        /// Connect TimeOut Check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //throw new NotImplementedException();            
            stopConnectTimer();

            bTimeOut = false;
            
            if (ws.State != WebSocketState.Open)
            {
                bTimeOut = true;
                DisConnect();
            }
        }

        private void stopConnectTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        public async Task DisConnect()
        {
            if (ws == null)
                return;
            try
            {

                if (ws.State == WebSocketState.Open)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }

                ws.Abort();
                ws.Dispose();
                ws = null;

                WS_State = WS_CON_STATE.READY;
            }
            catch (Exception e)
            {

                Console.WriteLine("-------- DisConnect Failed: " + e.Message);
            }
        }

        public bool IsConnected()
        {
            try
            {
                if (ws != null)
                {
                    if (ws.State == WebSocketState.Open)
                    {

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;

        }

        public bool IsTimeOut()
        {
            return bTimeOut;
        }

        public async Task Login(string sParam)
        {
            try
            {
                stopConnectTimer();

                WS_State = WS_CON_STATE.CONNECTED;

                Task.WhenAny(Recv(), Send(sParam));

                //await Send(sParam);

            }
            catch (Exception e)
            {
                Console.WriteLine("Login Failed: " + e.Message);
            }
        }

        public async Task Send(string sData)
        {
            if (sData == "" || string.IsNullOrEmpty(sData))
                return;

            string sPacket;
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(sData);
           ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            
            CLog.LOG(LOG_TYPE.WSK_WS, "#" + nNowIdx + " Send Done" + WS_State.ToString());
        }

        public async Task Recv()
        {
            bool bClose = false;

            while ((ws != null) && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseSent))
            {
                byte[] buff = new byte[1024];
                try
                {
                    var res = await ws.ReceiveAsync(new ArraySegment<byte>(buff), CancellationToken.None);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        if (ws != null)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);



                            //bClose = true;
                        }
                        break;
                    }
                    else
                    {

                        if (RcvProc != null)
                        {
                            RcvProc(UTF8Encoding.UTF8.GetString(buff));
                        }
                        //if(dF_SendWSStat != null)
                        //{
                        //    dF_SendWSStat(nCheckNow);
                        //}
                        CLog.LOG(LOG_TYPE.WSK_WS, "#" + nNowIdx + " WSK_WS RX : " + UTF8Encoding.UTF8.GetString(buff));
                    }

                }
                catch(Exception ex)
                {
                    CLog.LOG(LOG_TYPE.ERR, "Ws ERr : " + ex.ToString());
                    DisConnect();
                }
            }

            //if (bClose)
            //    DisConnect();

            Console.WriteLine("WS DisConnected!!");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using iNervMCS.UTIL;

namespace ADAgent.NET
{
    public class AsyncObject
    {
        public byte[] buffer;
        public Socket socket;
        public EndPoint pRemoteEp = null;
        public readonly int size;
        public StringBuilder sb = null;

        public AsyncObject(int bufferSize)
        {
            size = bufferSize;
            buffer = new byte[size];
            sb = new StringBuilder();
            ClearBuffer();
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer, 0, size);
        }
    }

    public class CWinSock
    {
        Socket pSocket = null;
        Socket pClient = null;
        public EndPoint pRemoteEp = null;
        Thread threadAccept = null;

        int nIndex = 0;

        int nSendState = 0;
        int nConnectState = 0;

        string sRemoteIP = "";
        int nRemotePort = 0;

        bool bSCcheck = false;

        public delegate void DeleConnectedComplete();
        private DeleConnectedComplete deleConn = null;

        public delegate void DeleSendComplete();
        private DeleSendComplete deleSend = null;

        public delegate void DeleRecvComplete(string sRecvData, bool bPass = false);
        private DeleRecvComplete deleRecv = null;

        public int INDEX
        { 
            get { return nIndex; }
            set { nIndex = value; }
        }

        public int SEND_STATE
        {
            get { return nSendState; }
            set { nSendState = value; }
        }

        public int CONNECT_STATE
        {
            get { return nConnectState; }
            set { nConnectState = value; }
        }

        public string REMOTE_HOST_IP
        {
            get { return sRemoteIP; }
            set { sRemoteIP = value; }
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return !(pClient.Poll(1, SelectMode.SelectRead)
                                    && pClient.Available == 0);
                }
                catch (Exception)
                {
                    Console.WriteLine("Not Connect!!!!");
                    return false;
                }
                finally
                {
                }
            }
        }

        public CWinSock(DeleConnectedComplete dfnConnComplete = null, DeleSendComplete dfnSendComplete = null, DeleRecvComplete dfnRecvComplete = null)
        {
            deleConn = dfnConnComplete;
            deleSend = dfnSendComplete;
            deleRecv = dfnRecvComplete;
        }

        /// <summary>
        /// TCP SERVER
        /// </summary>
        /// <param name="nPort"></param>
        public void Listen(int nPort)
        {
            try
            {
                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                bSCcheck = false;

                IPEndPoint ep = new IPEndPoint(IPAddress.Any, nPort);
                pSocket.Bind(ep);
                pSocket.Listen(0);
                pSocket.BeginAccept(acceptCallback, null);

                CONNECT_STATE = 2;
            }
            finally
            {
            }

        }

        /// <summary>
        /// TCP CLIENT
        /// </summary>
        /// <param name="sIP"></param>
        /// <param name="nPort"></param>
        public void Connect(string sIP, int nPort)
        {
            try
            {
                sRemoteIP = sIP;
                nRemotePort = nPort;

                bSCcheck = true;

                pClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                pClient.NoDelay = true;

                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(sIP), nPort);
                SocketAsyncEventArgs evArgs = new SocketAsyncEventArgs();
                evArgs.Completed += onConnected;
                evArgs.RemoteEndPoint = ep;

                CONNECT_STATE = 6;
                pClient.ConnectAsync(evArgs);
            }
            finally
            {
            }
        }
        
        /// <summary>
        /// UDP SERVER
        /// </summary>
        /// <param name="nPort"></param>
        public void Bind(int nPort)
        {
            try
            {
                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint ep = new IPEndPoint(IPAddress.Any, nPort);
                pSocket.Bind(ep);

                AsyncObject obj = new AsyncObject(1024);
                obj.socket = pSocket;
                obj.pRemoteEp = new IPEndPoint(IPAddress.None, nPort);
                pSocket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                CONNECT_STATE = 2;
            }
            finally
            {
            }
        }

        /// <summary>
        /// UDP CLIENT
        /// </summary>
        public void Udp()
        {
            try
            {   
            }
            finally
            {
            }
        }

        public void Close()
        {
            try
            {
                if (deleConn != null)
                {
                    CONNECT_STATE = 0;
                    deleConn();
                }

                if (threadAccept != null)
                {
                    threadAccept.Abort();
                }
                if (pSocket != null)
                {
                    if (bSCcheck)
                        pSocket.Shutdown(SocketShutdown.Both);
                    pSocket.Close();
                }
                if(pClient != null)
                {
                    if(bSCcheck)
                        pClient.Shutdown(SocketShutdown.Both);
                    pClient.Close();
                }


            }
            finally
            {
            }
        }

        /// <summary>
        /// TCP SEND
        /// </summary>
        /// <param name="sData"></param>
        public void Send(string sData)
        {
            try
            {
                nSendState = 0;
                byte[] btData = Encoding.ASCII.GetBytes(sData);
                pClient.BeginSend(btData, 0, btData.Length, 0, new AsyncCallback(sendCallback), pClient);

                //DATA.gQueue.QUEUE_SetMode(INDEX, 2);
            }
            catch (SocketException ex)
            {
                CONNECT_STATE = 0;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// UDP SEND
        /// </summary>
        /// <param name="sData"></param>
        public void Send_UDP(string sIP, int nPort, string sData)
        {
            try
            {
                sRemoteIP = sIP;
                nRemotePort = nPort;

                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                AsyncObject obj = new AsyncObject(1024);
                obj.pRemoteEp = new IPEndPoint(IPAddress.Parse(sIP), nPort);
                obj.socket = pSocket;

                byte[] btData = Encoding.ASCII.GetBytes(sData);
                //pSocket.SendTo(btData, obj.pRemoteEp);
                pSocket.BeginSendTo(btData, 0, btData.Length, 0, obj.pRemoteEp, sendToCallback, obj);
                pSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);
                pSocket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdpClient, obj);
            }
            finally
            {
            }
        }

        public void Send_UDP(string sData)
        {
            if(pRemoteEp != null)
            {
                byte[] btData = Encoding.ASCII.GetBytes(sData);
                //pSocket.SendTo(btData, pRemoteEp);
                pSocket.BeginSendTo(btData, 0, btData.Length, 0, pRemoteEp, sendToCallback, pSocket);
                //DATA.gQueue.QUEUE_SetMode(INDEX, 2);
                //pRemoteEp = null;
            }
        }

        // Data Receive
        private void Recv()
        {
        }

        private void acceptCallback(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("acceptCallback");
                pClient = pSocket.EndAccept(ar);
                pSocket.BeginAccept(acceptCallback, null);

                AsyncObject obj = new AsyncObject(1024);
                obj.socket = pClient;
                CONNECT_STATE = 7;
                pClient.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
            }
            finally
            {
            }
        }
        private void onConnected(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    CONNECT_STATE = 7;
                    AsyncObject obj = new AsyncObject(1024);
                    obj.socket = pClient;
                    pClient.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
                }
                else
                {
                    CONNECT_STATE = 0;
                }
            }
            finally
            {
                if (deleConn != null)
                {
                    deleConn();
                }
            }
            
        }
        private void dataReceive(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceive");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceive(ar);
                if(read > 0)
                {
                    CONNECT_STATE = 7;
                    //obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    obj.sb.Append(System.Text.Encoding.Default.GetString(obj.buffer));
                    //string sData = System.Text.Encoding.UTF8.GetString(obj.buffer);
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            deleRecv(obj.sb.ToString());
                            //deleRecv(INDEX, sData.ToString());
                            obj.sb.Clear();
                        }
                    }
                    if (deleConn != null)
                    {
                        deleConn();
                    }
                    obj.socket.BeginReceive(obj.buffer, 0, obj.size, 0, dataReceive, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            finally
            {
            }
        }

        private void dataReceiveUdp(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceiveUdp");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceiveFrom(ar, ref obj.pRemoteEp);
                if (read > 0)
                {
                    CONNECT_STATE = 7;
                    obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            pRemoteEp = obj.pRemoteEp;
                            deleRecv(obj.sb.ToString());
                            obj.sb.Clear();
                        }
                    }
                    obj.socket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            finally
            {
            }
        }

        private void dataReceiveUdpClient(IAsyncResult ar)
        {
            try
            {
                //UTIL.PR("dataReceiveUdp");
                AsyncObject obj = (AsyncObject)ar.AsyncState;
                int read = obj.socket.EndReceiveFrom(ar, ref obj.pRemoteEp);
                if (read > 0)
                {
                    CONNECT_STATE = 7;
                    obj.sb.Append(System.Text.Encoding.UTF8.GetString(obj.buffer));
                    obj.ClearBuffer();
                    if (read < obj.size && read > 0)
                    {
                        if (deleRecv != null)
                        {
                            deleRecv(obj.sb.ToString());
                            obj.sb.Clear();
                        }
                        obj.socket.Close();
                        CONNECT_STATE = 0;
                        return;
                    }
                    obj.socket.BeginReceiveFrom(obj.buffer, 0, obj.size, 0, ref obj.pRemoteEp, dataReceiveUdp, obj);
                }
                else
                {
                    obj.socket.Close();
                    CONNECT_STATE = 0;
                }
            }
            finally
            {
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                nSendState = 1;
                Socket client = (Socket)ar.AsyncState;
                int nSentSize = client.EndSend(ar);
                nSendState = 2;
                if (deleSend != null)
                    deleSend();
            }
            finally
            {
            }
        }

        private void sendToCallback(IAsyncResult ar)
        {
            try
            {
                nSendState = 1;
                Socket client = (Socket)ar.AsyncState;
                int nSentSize = client.EndSendTo(ar);
                nSendState = 2;
                if (deleSend != null)
                    deleSend();
            }
            finally
            {
            }
        }
    }
}
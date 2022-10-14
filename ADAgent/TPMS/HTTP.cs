using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ADAgent.UTIL;
using iNervMCS.UTIL;
using System.Web;
using System.Diagnostics;

namespace DH.NET
{
    public class RequestState
    {
        public string sCmd;
        public byte[] data;
        public HttpWebRequest request;
        
        public RequestState()
        {
            sCmd = "";
            data = null;
            request = null;
        }
    }

    public class HTTP
    {
        public static string sLastError = "";
        public static int nRequestTimeOut = 1000 * 20;
        public static int nCnt = 0;
        public static HttpWebResponse pResponse;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        public static Stopwatch pSw = new Stopwatch();
        public delegate void DeleHttpParsing(string sResponse);
        private static DeleHttpParsing deleHttpParse = null;

        public static string POST(string sUrl, StringBuilder sbData, ref string sToken)
        {
            return POST(sUrl, sbData.ToString(), ref sToken);
        }
        public static string POST(string sUrl, StringBuilder sSendData, string sNm, string sFull, ref string sToken, bool bExist)
        {

            return POST(sUrl, sSendData.ToString(), sNm, sFull, ref sToken, bExist);
        }

        public static string POST(string sUrl, string sData, ref string sToken)
        {
            string response = "";

            try
            {

                byte[] sendData = Encoding.UTF8.GetBytes(sData);
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sUrl);
                httpRequest.Method = "POST";
                httpRequest.Timeout = 30000;
                //httpRequest.Accept = 
                httpRequest.Headers.Add("Cache-Control", "no-cache");
                httpRequest.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, */*";
                //httpRequest.Headers.Add();
                //httpRequest.Host = "10.14.12.112:8080";
                httpRequest.CookieContainer = new CookieContainer(2);
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.UserAgent = ".NET Framework";
                //HT
                httpRequest.AllowAutoRedirect = true;
                //httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.106 Safari/537.36";
                //httpRequest.Referer = sUrl.Substring(0, sUrl.IndexOf("tpms/")+5);
                //httpRequest.Expect = "";
                //httpRequest.
                //KeepAlive = true;
                //httpRequest.AllowAutoRedirect = false;
                //sToken = "eyJpZCI6IlQzM19NdWluMiIsImxvZ2luSWQiOiJUMzNfTXVpbjIiLCJuYW1lIjoi64yA6rWs6rO17ZWtX+yeheq1rCIsInN5c1RpbWUiOiIyMDIyLTA3LTA4IDEyOjA2OjUxIiwiZGV2aWNlSWQiOiJyYW5kb21fZGV2aWNlX3VuaXF1ZV9pZF9pc19oZXJlIiwiZXhwaXJlZCI6ZmFsc2UsImV4cGlyZXMiOiIyMDIyLTA3LTA4IDEzOjA2OjUxIiwicm9sZXMiOlsiUk9MRV9TTVUiXSwidXNlcm5hbWUiOiJUMzNfTXVpbjIifQ==./EBsoyvcbPan+Fw3N4uUxuZFpm6tbz3dY83r/MelbUE=";
                if (sToken != null && sToken != "")
                {
                    httpRequest.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
                }
                httpRequest.ContentLength = sendData.Length;
                //ht
                //string headerTemplate = "empNo=T33_Muin1&password=roqkfwk00&tDeviceId=01";
                //byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(headerTemplate);
                //rs.Write(headerbytes, 0, headerbytes.Length);



                Stream reqStream = httpRequest.GetRequestStream();
                //reqStream.Write(headerbytes, 0, headerbytes.Length);
                reqStream.Write(sendData, 0, sendData.Length);
                reqStream.Close();

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
      

                StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                response = streamReader.ReadToEnd();

                //헤더에서 토큰 추출
                if (sToken == "" || sToken == null)
                {
                    sToken = httpResponse.Headers.Get("X-TPMS-AUTH-TOKEN");

                }

                streamReader.Close();
                httpResponse.Close();
            }
            catch (Exception e)
            {
                //string s = e.Message;
                //throw e;
                CLog.LOG(LOG_TYPE.ERR, " POST Err : " + e.ToString());
                sLastError = "POST: " + e.Message;
            }
            finally
            {
            }

            return response;
        }
        //public static string POST_DIRECT(string sUrl, string sData, string sToken)
        //{
        //    string response = "";

        //    try
        //    {

        //        HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sUrl);
        //        httpRequest.Timeout = nRequestTimeOut;
        //        httpRequest.Headers.Add("Cache-Control", "no-cache");
        //        httpRequest.Accept = @"text/html,application/xhtml+xml,application/xml,image/webp,image/apng,*";
        //        httpRequest.ContentType = "application/x-www-form-urlencoded";
        //        httpRequest.UserAgent = ".NET Framework";
        //        //httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.106 Safari/537.36";
        //        //httpRequest.Referer = sUrl.Substring(0, sUrl.IndexOf("tpms/")+5);
        //        //httpRequest.Expect = "";
        //        //httpRequest.KeepAlive = true;
        //        //httpRequest.AllowAutoRedirect = false;
        //        if (sToken != "")
        //        {
        //            httpRequest.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
        //        }
        //        httpRequest.Method = "POST";

        //        string headerTemplate = sData;
        //        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(headerTemplate);

        //        Stream reqStream = httpRequest.GetRequestStream();
        //        reqStream.Write(headerbytes, 0, headerbytes.Length);
        //        reqStream.Close();

        //        HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        //        StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
        //        response = streamReader.ReadToEnd();

        //        //헤더에서 토큰 추출
        //        if (sToken == "")
        //        {
        //            sToken = httpResponse.Headers.Get("X-TPMS-AUTH-TOKEN");
        //        }
        //        streamReader.Close();
        //        httpResponse.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        //string s = e.Message;
        //        //throw e;
        //        CLog.LOG(LOG_TYPE.ERR, " POST Err : " + e.ToString());
        //        sLastError = "POST: " + e.Message;
        //    }

        //    return response;
        //}


        public static string POST(string sUrl, string sSendData, string sNm, string sFull, ref string sToken, bool bExist)
        {
            //string sHeader = "POST " + sUrl +  sParam & "  HTTP/1.0" & vbCrLf & _
            //      "Content-Type: application/x-www-form-urlencoded; charset=UTF-8" & vbCrLf


            string res = "";
            string sBody = "";
            string boundary = "---------------------------junche421e05d2";
            int bytesRead;
            //string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            //byte[] sendData = Encoding.UTF8.GetBytes(sSendData);
            FileInfo fileIn = new FileInfo(sFull);

            sBody = "-----------------------------junche421e05d2\r\nContent-Disposition: form-data; name=\"file\";" +
                " filename=\"" + sNm + "\"\r\n" +
                 "Content-Type:image/png\r\n\r\n";
            string sBody_End = "\r\n\r\n-----------------------------junche421e05d2--";


            byte[] buffer = new byte[fileIn.Length];
            //      string sHeader = "Content-Type: multipart/form-data, boundary=---------------------------junche421e05d2\r\n" +
            //"Content-Length: " + nLen + "\r\n" +
            //            "X-TPMS-AUTH-TOKEN: " + sToken + "\r\n\r\n" + sBody;
            //
            
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sUrl);

           wr.Timeout = nRequestTimeOut;
            wr.Method = "POST";
            //wr.ContentType = "image/png";
            if (bExist)
            {
                wr.ContentType = "multipart/form-data;boundary=---------------------------junche421e05d2";
                long nLen = sBody.Length + sBody_End.Length + fileIn.Length;
            }
            else
            {
                wr.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            }
           wr.CookieContainer = new CookieContainer(2);
           wr.AllowAutoRedirect = true;

            if (sToken != "")
                wr.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
                //wr.Headers.Add(sBody);

                Stream rs = wr.GetRequestStream();
            //rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = sBody;
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(headerTemplate);
                //rs.Write(headerbytes, 0, headerbytes.Length);
                rs.Write(headerbytes, 0, headerbytes.Length);
            
            if (bExist)
            {
                //for (int i = 0; i < 2; i++)
                //{
                //rs.Write
                using (FileStream fileStream = File.Open(sFull, FileMode.Open))
                {

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }
                
                    
                
                //}
                byte[] bByte_End = System.Text.Encoding.UTF8.GetBytes(sBody_End);
                rs.Write(bByte_End, 0, bByte_End.Length);
                //rs.Write(bByte_End, 0, bByte_End.Length);

            }
        
            //WebResponse wresp = null;
            try
            {
                //rs.
                rs.Close();
                //wresp = wr.GetResponse();
                //Stream stream2 = wresp.GetResponseStream();
                //StreamReader reader2 = new StreamReader(stream2);
                HttpWebResponse httpResponse = (HttpWebResponse)wr.GetResponse();
                StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                
                //res = reader2.ReadToEnd();
                //stream2.Close();
                //reader2.Close();


                //if (wresp != null)
                //{
                //    wresp.Close();
                //    wresp = null;
                //}


                streamReader.Close();
                httpResponse.Close();


                //HttpWebResponse httpResponse = (HttpWebResponse)wr.GetResponse();
                //StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                //Stream stream2 = httpResponse.GetResponseStream();
                //StreamReader reader2 = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                //res = reader2.ReadToEnd();

                //log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "IMG POST ERR = " + ex.ToString());
                //log.Error("Error uploading file", ex);
                //if (wresp != null)
                //{

                //    //wresp.Close();
                //    wresp = null;
                //}
            }
            finally
            {


                //wr = null;
            }
            nCnt++;
            return res;
        }
       
        private static void Sleep_watch()
        {
        }

        private static async Task Send_Img(string sFull, Stream rs, byte[] buffer, int bytesRead)
        {
            for (int i = 0; i < 3; i++)
            {
                using (FileStream fileStream = File.Open(sFull, FileMode.Open))
                {
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static string GET(string url, string param)
        {
            return GET(url + "?" + param);
        }
        public static string GET(string url)
        {
            string request = "";
            return request;
        }

        public static bool POST(string sUrl, string sData, string sToken, DeleHttpParsing deleParseFunc = null)
        {
            try
            {
                deleHttpParse = deleParseFunc;
                //byte[] sendData = UTF8Encoding.UTF8.GetBytes(sData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Timeout = nRequestTimeOut;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                if (sToken != "")
                {
                    request.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
                }
                request.Method = "POST";
                //request.ContentLength = sendData.Length;

                RequestState state = new RequestState();
                state.data = UTF8Encoding.UTF8.GetBytes(sData);
                request.ContentLength = state.data.Length;

                state.request = request;
                //Stream reqStream = request.GetRequestStream();
                //reqStream.Write(sendData, 0, sendData.Length);
                //reqStream.Close();

                //request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sLastError = "POST: " + e.Message;
                return false;
            }
            finally
            {
            }
            return true;
        }

        private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                RequestState state = (RequestState)asynchronousResult.AsyncState;
                HttpWebRequest request = (HttpWebRequest)state.request;

                // End the operation
                Stream postStream = request.EndGetRequestStream(asynchronousResult);

                // Write to the request stream.
                postStream.Write(state.data, 0, state.data.Length);
                postStream.Close();

                // Start the asynchronous operation to get the response
                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sLastError = "GetRequestStreamCallback: " + e.Message;
            }
            finally
            {
            }
        }

        private static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // End the operation
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse, Encoding.Default, true);
                string sResponse = streamRead.ReadToEnd();
                pResponse = response;
                Console.WriteLine(sResponse);
                // Close the stream object
                streamResponse.Close();
                streamRead.Close();
                // Release the HttpWebResponse
                response.Close();

                if (deleHttpParse != null)
                {
                    deleHttpParse(sResponse);
                }

                allDone.Set();
            }
            catch (Exception e)
            {
                sLastError = "GetResponseCallback: " + e.Message;
            }
            finally
            {
            }
        }
    }
}

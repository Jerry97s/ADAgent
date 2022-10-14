using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Web;
using ADAgent.UTIL;
using ADAgent.DATA;
using System.Threading;

namespace LPR
{
    public class CLPR
    {
        int nIdx = 0;
        public LPRInfo st_LprInfo;

        public delegate void DF_SetInOutCar(bool bIO, string sCarno, string sDT, string sFull, string sFileNm, ref string[] sSubRcv, string sDiv, bool bPass);
        public DF_SetInOutCar dfSetIOCar;

        public delegate void DF_SetLog(string sDiv, string sLog);
        public DF_SetLog dfSetLog = null;

        //public delegate void DF_SendIO_ToEF(string sMsg);
        //public DF_SendIO_ToEF dfSend2EF;

        //CH01#70너5972#\20220523\CH01_20220523135830_70너5972.jpg
        public void LPR_Parse(string sDiv, bool bIO, string sRcvData, string sFolder, bool bTest, bool bPass = false)
        {
            string sFull;
            string sFileName;
            string[] sSubRcv = new string[10];
            try
            {
                //CLog.LOG(LOG_TYPE.LPR, "LPR RX 1: " + sRcvData);
                //lstData.Items.Add("RX: " + sRcvData);
                if (dfSetLog != null)
                    dfSetLog(((bTest == false) ? sDiv : "Test"), sRcvData);

                sRcvData = sRcvData.Trim();
                if (sRcvData.Substring(sRcvData.Length - 2) == "OK")
                    sRcvData = sRcvData.Substring(0, sRcvData.Length - 2);
            //if()
            sRcvData = sRcvData.Replace("\r\n", "");
                sRcvData = sRcvData.Replace("\t", "");
                sRcvData = sRcvData.Replace("\n", "");
                sRcvData = sRcvData.Replace(" ", "");
                sRcvData = sRcvData.Replace("\0", string.Empty);

                if (sRcvData != "OK")
                {
                    string[]arFile = sRcvData.Split('\\');
                    string[] arFile_Thr = arFile[2].Split('_');
                    sFileName = arFile[2];



                    int nPos = sRcvData.IndexOf(".jpg");


                    if (nPos != -1)
                    {
                        if (nPos + 4 < sRcvData.Length)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (i == 0)
                                    sSubRcv[i] = sRcvData.Substring(nPos + 4);
                                else
                                {
                                    try
                                    {
                                        if (sSubRcv[i - 1] != null)
                                        {
                                            if (sSubRcv[i - 1].IndexOf(".jpg") != -1)
                                                sSubRcv[i] = sSubRcv[i - 1].Substring(sSubRcv[i - 1].IndexOf(".jpg") + 4);
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
                    }
                    if (nPos == -1)
                        nPos = sRcvData.Length;

                    //if()
                    //if(sRcvData.LastIndexOf("OK"))
                    sRcvData = sRcvData.Substring(0, nPos);
                    CLog.LOG(LOG_TYPE.LPR, "LPR RX 2: " + sRcvData);
                    string[] arFile_Sec = sRcvData.Split('\\');
                    
                    if (bIO)
                        sFileName = arFile_Sec[2]+ ".jpg";
                    else
                        sFileName = arFile_Thr[2];
                    //string sEFSendMsg = HttpUtility.UrlEncode(sRcvData);
                    //pSock_EF.Send(sEFSendMsg);

                    //D:\image\20220523
                    //0      1          2                 

                    //CH01#70너5972#\20220523\CH01_20220523135830_70너5972
                    string[] arData = sRcvData.Split('#');

                    CLog.LOG(LOG_TYPE.LPR, "#0");

                    if (arData.Length > 1)
                    {
                        CLog.LOG(LOG_TYPE.LPR, "#1");
                        string sAck;

                        sAck = arData[0] + "#" + arData[1] + "#" + arData[2];

                        if(arData[1] == "No_Detection" || arData[1] == "")
                        {
                            arData[1] = "미인식";
                        }

                        CLog.LOG(LOG_TYPE.DEBUG, "TX: " + sAck);
                        CLog.LOG(LOG_TYPE.LPR, "#1");
                        //lstData.Items.Add("TX: " + sAck);
                        string[] arDate = arData[2].Split('_');
                        //sFull = sFolder + @":\image\" + arDate[1].Substring(0, 8) + @"\" + arFile[2];
                        sFull = sFolder + @":\" + arDate[1].Substring(0, 8) + @"\" + arFile[2];
                        CLog.LOG(LOG_TYPE.LPR, "LPR Full: " + sFull);

                        string sDate = arDate[1].Substring(0, 4); //yyyy
                        sDate += "-" + arDate[1].Substring(4, 2); //MM
                        sDate += "-" + arDate[1].Substring(6, 2); //dd
                        sDate += " " + arDate[1].Substring(8, 2); // HH
                        sDate += ":" + arDate[1].Substring(10, 2); //mm
                        sDate += ":" + arDate[1].Substring(12, 2); //ss
                        CData.sLastDate = arDate[1].Substring(0,8);
                        if (dfSetIOCar != null)
                        {
                             CLog.LOG(LOG_TYPE.LPR, "#8" + sDate + " " + bIO + " " + st_LprInfo.sID);
                            //dfSetIOCar(bIO, arData[1], sDate, sFull, sFileName);


                            if (bTest)
                            {

                                sDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                if (bIO)
                                {
                                    sFull = "D:\\test_in.jpg";
                                }
                                else
                                    sFull = "D:\\test_out.jpg";


                                string[] arFile_test = sRcvData.Split('\\');
                                CLog.LOG(LOG_TYPE.LPR, "LPR Full: " + sFull);
                            }


                            //Thread.Sleep(1000);
                            //if(CData.bParse)
                            //    Set_Stack
                            dfSetIOCar(bIO, arData[1], sDate, sFull, sFileName, ref sSubRcv, sDiv, bPass);

                        }



                    }
                }
                
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DEBUG, "RecvData Exception: " + e.Message);
                CData.bParse = false;
            }
            finally
            {

            }
        }
    }
}

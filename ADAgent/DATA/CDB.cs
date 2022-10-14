using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DH.DB;
using System.Data;
using System.Windows.Forms;
using ADAgent.UTIL;
using ADAgent.DATA;
using iNervMCS.UTIL;
using System.Data.SQLite;

namespace ADAgent
{
    public enum EPAYMENT_MODE
    {
        NORMAL_FEE,
        REG_FEE
    };

    class CDB
    {   
        CSqlite pDB = new CSqlite();

        int nType = 0;
        int nVer = 0;
        const int VER = 2;
        const int TABLE_CNT = 1;
        string sNowDB = "";

        public delegate void DF_DB_Cnt(int nCnt);
        public DF_DB_Cnt dfDBCnt = null;

        public delegate void DF_DB_Select(string sRows);
        public DF_DB_Select dfDBSelect = null;

        public delegate void DF_REG_UPLOAD(string sCarno, string sStart, string sEnd, string sRegDiv, string sGroupNm, string sUserNm, string sMemo, string sArea, string sAreaArray);
        public DF_REG_UPLOAD dfDBUPLOAD = null;
        public void InitDatabase()
        {
            //string sIniPath = CData.sSetDir + "\\" + "Setting.Ini";
            //nType = CIni.Load("PARK_INFO", "DB_TYPE", 0, sIniPath);
            //nVer = CIni.Load("PGM_INFO", "DB_VER", 0, sIniPath);

            CLog.LOG(LOG_TYPE.DEBUG, "ExistDatabase(StackDB)");
            ExistDatabase(CData.sStackDB);
            ExistDatabase(CData.sIOCarDB);
            ExistDatabase(CData.sRegDB);
            string[][] arStackTable = new string[TABLE_CNT][];
            arStackTable[0] = new string[]{ "tblStack", "no", "I", "CMD", "T128", "ID", "T128", "Param", "T128", "Other", "T128"};

            for (int i=0; i< arStackTable.Length; ++i)
            {
                CheckTable(CData.sStackDB, arStackTable[i]);
            }
            string[] arSetTable = new string[] { "tblMngOpt1", "no", "I", "Value", "I" };
            CheckTable(CData.sSetDB, arSetTable);
            if (!CheckTable(CData.sSetDB, arSetTable))
            {
                InsertDefaultOpt1Value();
            }

            string[] arIOCarTable = new string[] { "tblIOCar", "no", "I", "IOID", "I", "Img" , "T128", "CarNo", "T20", "Stat", "I", "AID", "I", "FEE", "I", "PartAmt", "I"};

            CheckTable(CData.sIOCarDB, arIOCarTable);

            string[] arPrevCarTable = new string[] { "tblPrevCar", "no", "I", "IDX", "I", "Carno", "T128", "ID", "I"};

            if (!CheckTable(CData.sIOCarDB, arPrevCarTable))
            {
                Insert_PrevCar();
            }

            string[] arRegCar = new string[] { "tblRegCar", "no", "I", "Carno", "T128", "StartDttm", "T128", "EndDttm", "T128", "RegDiv", "T128", "GroupNm", "T128", "UserNm", "T128", "Memo", "T128", "Area", "T128", "AreaArray", "T128" };


            CheckTable(CData.sRegDB, arRegCar);

            //string[] arRegCar = new string[] { "tblRegCar", "no", "I", "Carno", "T128", "StartDttm", "T128", "EndDttm", "T128", "RegDiv", "T128", "GroupNm", "T128", "UserNm", "T128", "Memo", "T128", "Area", "T128", "AreaArray", "T128" };


            //CheckTable(CData.sRegDB, arRegCar);
        }

        private void InsertDefaultOpt1Value()
        {
            int i = 0;
            for (i = 0; i < CData.garOpt1_Name.Length; ++i)
            {
                Insert_Opt1(i + 1, 0);
            }
        }

        public bool ExistDatabase(string sDB)
        {
            bool bExist = false;

            try
            {
                if (!CFunc.CheckFile(sDB))
                {
                    pDB.CreateDatabase(sDB);
                    bExist = CFunc.CheckFile(sDB);
                }
                if (pDB.Open(sDB))
                {
                    bExist = true;
                    pDB.Close();
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.DEBUG, "ExistDatabase Excp: " + e.Message);
            }
            finally
            {
            }
            return bExist;
        }

        public bool CheckTable(string sDB, string[] arTable)
        {
            if (!pDB.ExistTable(arTable[0]))
            {
                pDB.BuildTable(sDB, arTable);
                return false;
            }

            return true;
        }

        public void Update_Opt1(int nNo, int nVal)
        {
            //{ "tblMngOpt1", "no", "I", "Value", "I" }
            string query = "";
            query = "UPDATE tblMngOpt1 Set " +
                    "Value=" + nVal.ToString() + " " +
                    "WHERE no=" + nNo + ";";

            try
            {
                CLog.LOG(LOG_TYPE.DATA, "Update_Opt1: " + query);
                pDB.Open(CData.sSetDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "Update_Opt1 Failed: " + e.Message);
            }
            finally
            {
            }
        }

        public int Select_Cnt_Stack()
        {
            string query = "";
            query = "Select * from tblStack";
            int nRows = 0;
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sStackDB);
                nRows = pDB.LoadData(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                nRows = 0;
                return 0;
            }
            finally
            {
            }

            if (dfDBCnt != null)
                dfDBCnt(nRows);

            return nRows;
        }
        public string Select_Stack_Use()
        {
            string query = "";
            query = "Select CMD, ID, Param, Other from tblStack limit 1;";
            string sRows = "";
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sStackDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    sRows = row["CMD"].ToString()+ "!" + row["ID"].ToString() + "!" + row["Param"].ToString() + "!" + row["Other"].ToString();
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                sRows = "";
            }
            finally
            {
            }

            return sRows;

        }

        public string Select_IOCar_Img(string sCarno)
        {
            string query = "";
            query = "Select Img from tblIOCar where Carno = '" + sCarno + "' limit 1;";
            string sImg = "";
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    sImg = row["Img"].ToString();
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                sImg = "";
            }
            finally
            {
            }

            return sImg;
        }
        public int Select_IOCar_ID(string sCarno)
        {
            string query = "";
            query = "Select IOID from tblIOCar where Carno = '" + sCarno + "' limit 1;";
            int sID = 0;
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    sID = int.Parse(row["IOID"].ToString());
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                sID = 0;
            }
            finally
            {
            }

            return sID;
        }
        public int Select_IOCar_AID(string sCarno)
        {
            string query = "";
            query = "Select AID from tblIOCar where Carno = '" + sCarno + "' limit 1;";
            int nID = 0;
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    nID = int.Parse(row["AID"].ToString());
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                nID = 0;
            }
            finally
            {
            }

            return nID;
        }
        public int Select_IOCar_FEE(string sCarno)
        {
            string query = "";
            query = "Select FEE from tblIOCar where Carno = '" + sCarno + "' limit 1;";
            int nFEE = 0;
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    nFEE = int.Parse(row["FEE"].ToString());
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                nFEE = 0;
            }
            finally
            {
            }

            return nFEE;
        }

        public int Select_IOCar_PartAmt(string sCarno)
        {
            string query = "";
            query = "Select PartAmt from tblIOCar where Carno = '" + sCarno + "';";
            int nFEE = 0;
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    nFEE = int.Parse(row["PartAmt"].ToString());
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                nFEE = 0;
            }
            finally
            {
            }
            return nFEE;
        }

        public string Select_PrevCarno(string sIDX)
        {
            string query = "";
            string nPrev_ID = "";
            query = "Select ID from tblPrevCar where IDX = " + sIDX + ";";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    nPrev_ID = row["ID"].ToString();
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                //nPrev_ID = "";
            }
            finally
            {
            }
            return nPrev_ID;
        }

        public void Insert_Stack(string sCmd,string sID, string sParam, string sOther)
        {
            string query = "";
            query = "INSERT INTO tblStack (CMD, ID, Param, Other )VALUES('" +
                sCmd + "', '" +  sID + "', '" + sParam + "', '" + sOther + 
                "');";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sStackDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Insert_IOCar(int nIOID, string sImg, string sCarno, int nStat = 0, int nAID = 0, int nFEE = 0)
        {
            string query = "";
            query = "INSERT INTO tblIOCar (IOID, Img, Carno, Stat, AID, FEE) VALUES(" +
                nIOID + ", '" + sImg + "', '" + sCarno + "', " + nStat + ", " + nAID + " , " + nFEE +
                ");";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();    
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_PrevID(string sCarno, int nID, string sIDX)
        {
            string query = "";
            query = "Update tblPrevCar set Carno = '" + sCarno + "', ID = " + nID + " where IDX = " + sIDX + ";";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();

            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Insert_PrevCar()
        {
            string query = "";
            

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                for(int i = 0; i <= 100; i++)
                {
                    query = "INSERT INTO tblPrevCar (Carno, ID, IDX) VALUES( '12가1234', '1234567', " + i.ToString() + ");";
                    pDB.ExecQuery(query);
                }
                
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Insert_RegCar(string sCarno, string sStart, string sEnd, string sDiv, string sGroupNm, string sUserNm, string sMemo, string sArea, string sAreaArray)
        {
            string query = "";

            query = "INSERT INTO tblRegCar (Carno, StartDttm, EndDttm, RegDiv, GroupNm, UserNm, Memo, Area, AreaArray) VALUES( '" + sCarno + "', '" +
                sStart + "', '" + sEnd + "', '" + sDiv + "', '" + sGroupNm + "', '" + sUserNm + "', '" + sMemo + "', " + sArea + ", '" + sAreaArray +"');";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sRegDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public string Select_Reg()
        {
            //string sCarno, string sStart, string sEnd, string sRegDiv, string sGroupNm, string sUserNm, string sTelno
            string query = "";
            string sCarno = "";
            string sStart = "";
            string sEnd = "";
            string sRegDiv = "";
            string sGroupNm = "";
            string sUserNm = "";
            string sMemo = "";
            string sArea = "";
            string sArrayArea = "";

            query = "Select * from tblRegCar;";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //SQLiteCommand cmd = new SQLiteCommand(query, )
                pDB.Open(CData.sRegDB);
                pDB.LoadData(query);
                foreach (DataRow row in pDB.DATA.Rows)
                {
                    //nPrev_ID = row["ID"].ToString();
                    sCarno = row["Carno"].ToString();
                    sStart = row["StartDttm"].ToString();
                    sEnd = row["EndDttm"].ToString();
                    sRegDiv  = row["RegDiv"].ToString();
                    sGroupNm = row["GroupNm"].ToString();
                    sUserNm  = row["UserNm"].ToString();
                    sMemo = row["Memo"].ToString();
                    sArea = row["Area"].ToString();
                    sArrayArea = row["AreaArray"].ToString();

                    Application.DoEvents();
                    if (dfDBUPLOAD != null)
                        dfDBUPLOAD(sCarno, sStart, sEnd, sRegDiv, sGroupNm, sUserNm, sMemo, sArea, sArrayArea);
                }
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
                //nPrev_ID = "";
            }
            finally
            {
            }

            return "";
        }

        //public void Insert_RegCar()
        //{
        //    string query = "";
        //    query = "INSERT INTO tblPrevCar (Carno, ID) VALUES( '12가1234', '1234567' );";

        //    try
        //    {
        //        CLog.LOG(LOG_TYPE.DB, query + " Success");
        //        //query = "INSERT INTO tblStack VALUES ( 'good' )";
        //        pDB.Open(CData.sIOCarDB);
        //        pDB.ExecQuery(query);
        //        pDB.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        CLog.LOG(LOG_TYPE.ERR, query + " Failed");
        //    }
        //}

        public void Update_IOCar_ID(int nIOID, string sCarno)
        {
            string query = "";
            query = "Update tblIOCar set IOID = " + nIOID.ToString() + 
                " where Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_IOCar_AID(int nAID, string sCarno)
        {
            string query = "";
            query = "Update tblIOCar set AID = " + nAID.ToString() +
                " where Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_IOCar_FEE(int nFEE, string sCarno)
        {
            string query = "";
            query = "Update tblIOCar set FEE = " + nFEE.ToString() +
                " where Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_IOCar_PartAmt(int nFEE, string sCarno)
        {
            string query = "";
            query = "Update tblIOCar set PartAmt = " + nFEE.ToString() +
                " where Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_IOCar_IMG(string sImg, string sCarno)
        {
            string query = "";
            query = "Update tblIOCar set Img = '" + sImg +
                "' where Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Delete_Reg()
        {
            string query = "";
            query = "Delete from tblRegCar;";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sRegDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }
        public void Update_IOCar_Stat(string sCarno, int nStat)
        {
            string query = "";
            query = "Update tblIOCar set Stat =" + nStat.ToString() +
                "where Carno = '" + sCarno +
                "');";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Update_PrevCar(string sCarno)
        {
            string query = "";
            query = "Update tblPrevCar set Carno = '" + sCarno +
                "';";

            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                //query = "INSERT INTO tblStack VALUES ( 'good' )";
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Delete_IOCar(string sCarno)
        {
            string query = "";
            query = "DELETE FROM tblIOCar where CarNo = '" + sCarno +"';";
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sIOCarDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Delete_Stack()
        {
            string query = "";
            query = "DELETE FROM tblStack where no = (select no from tblStack limit 1);";
            try
            {
                CLog.LOG(LOG_TYPE.DB_S, query + " Success");
                pDB.Open(CData.sStackDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception)
            {
                CLog.LOG(LOG_TYPE.ERR, query + " Failed");
            }
            finally
            {
            }
        }

        public void Load_Opt1()
        {
            string query = "";
            int nRows = 0;
            query = "SELECT * FROM tblMngOpt1";
            try
            {
                CLog.LOG(LOG_TYPE.DATA, "Load_Opt1: " + query);
                pDB.Open(CData.sSetDB);
                nRows = pDB.LoadData(query);
                if (nRows > 0)
                {
                    if (CData.garOpt1.Count > 0)
                        CData.garOpt1.Clear();
                    foreach (DataRow row in pDB.DATA.Rows)
                    {
                        int nVal;
                        nVal = int.Parse(row["Value"].ToString());
                        CData.garOpt1.Add(nVal);
                    }
                }
                pDB.Close();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "Load_Opt1 Failed: " + e.Message);
            }
            finally
            {
            }
        }

        public void Insert_Opt1(int nNo, int nVal)
        {
            string query = "";
            query = "INSERT INTO tblMngOpt1 VALUES(" +
                nNo.ToString() + ", " +
                nVal.ToString() +
                ");";

            try
            {
                CLog.LOG(LOG_TYPE.DATA, "Insert_Opt1: " + query);
                pDB.Open(CData.sSetDB);
                pDB.ExecQuery(query);
                pDB.Close();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "Insert_Opt1 Failed: " + e.Message);
            }
            finally
            {
            }
        }

    }
}

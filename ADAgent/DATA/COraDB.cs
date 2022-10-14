
using ADAgent.DATA;
using ADAgent.UTIL;
using System;
using System.Data.OleDb;
using System.Data.OracleClient;

namespace ADAgent
{

    class COraDB
    {
        OleDbConnection TiberoConn = null;
        OracleCommand cmd = null;
        LST_REG st_REG;
        public OraDBInfo st_oraDBInfo;
        public string sOIP = "127.0.0.1";
        public string sOPORT = "8080";
        //string strAccessConn = "Provider=jdbc:tibero:thin;";
        string strAccessConn = "Provider=tbprov.Tbprov.6; ";
        int nNowIDx = 0;

        public delegate bool DF_Select(LST_REG st_REG);
        public DF_Select dfSelect = null;

        public delegate void DF_ODB_Log(string sDB, string sLog);
        public DF_ODB_Log dfODBLog = null;
        //소스 이름과 유저 이름, 패스워드를 입력함 (나중에 오라클 연결할 때 sql문 사용)

        //Data Source  = 본인의 아이피 주소:포트번호/orcl 이다!
        //string sql = "Data Source=" + CData.sOracleIP + ":"+CData.sOraclePort+"/"+CData.sDataSource+";User ID = "+CData.sOracleID+";Password="+ CData.sOraclePW;

        string sql = "";

        public COraDB(int nIdx)
        {
            //txtOraIP.Text = CIni.Load("ADA_DB_ORACLE", "OracleIP", "0", CData.sDBPath);
            //txtOraPORT.Text = CIni.Load("ADA_DB_ORACLE", "OraclePort", "0", CData.sDBPath);
            //txtOraSrc.Text = CIni.Load("ADA_DB_ORACLE", "OracleSrc", "0", CData.sDBPath);
            //txtOraID.Text = CIni.Load("ADA_DB_ORACLE", "OracleID", "0", CData.sDBPath);
            //txtOraPW.Text = CIni.Load("ADA_DB_ORACLE", "OraclePW", "0", CData.sDBPath);
            nNowIDx = nIdx;
            st_oraDBInfo.sIP = CIni.Load("ADA_DB_ORACLE", "OracleIP", "0", CData.sDBPath);
            st_oraDBInfo.nPort = int.Parse(CIni.Load("ADA_DB_ORACLE", "OraclePort", "0", CData.sDBPath));
            st_oraDBInfo.sSrc = CIni.Load("ADA_DB_ORACLE", "OracleSrc", "0", CData.sDBPath);
            st_oraDBInfo.sID = CIni.Load("ADA_DB_ORACLE", "OracleID", "0", CData.sDBPath);
            st_oraDBInfo.sPW = CIni.Load("ADA_DB_ORACLE", "OraclePW", "0", CData.sDBPath);
            st_oraDBInfo.sBASE = CIni.Load("ADA_DB_ORACLE", "BASEDB", "0", CData.sDBPath);
            st_oraDBInfo.bStatus = false;

            strAccessConn += "Location=" + st_oraDBInfo.sIP + "," + st_oraDBInfo.nPort + "," + st_oraDBInfo.sBASE + "; User ID=" + st_oraDBInfo.sID + "; Password=" + st_oraDBInfo.sPW + ";";
            //sql = "Data Source=" + st_oraDBInfo.sIP + ":" + st_oraDBInfo.nPort.ToString() + "/" + st_oraDBInfo.sSrc + ";User ID = " + st_oraDBInfo.sID + ";Password=" + st_oraDBInfo.sPW;

        }
        public bool Open()

        {
            try
            {
                //using System.Data.OracleClient;를 사용하면 쓸 수 있는 함수들
                if (dfODBLog != null)
                    dfODBLog(st_oraDBInfo.sBASE, "ODB Connect(" + strAccessConn + ")");

                try
                {
                    TiberoConn = new OleDbConnection(strAccessConn);
                }
                catch (Exception)
                {
                    throw;
                }                //sql에 저장된 데이터베이스 정보로 연결

                try
                {
                    TiberoConn.Open();
                    st_oraDBInfo.bStatus = true;
                }
                catch (Exception ex)
                {
                    st_oraDBInfo.bStatus = false;
                    CLog.LOG(LOG_TYPE.SCREEN, "#" + nNowIDx.ToString() + " " + ex.ToString());
                }

                


                //OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자
                
                //cmd = new OracleCommand();
                
                //cmd.Connection = OraConn;

                //order by D_DATE DESC 는 D_DATA를 내림차순으로 정렬한다는 뜻!

                //SelectCommand 함수에 쿼리문 넣기

                //Select 테이블 안의 네임1, 테이블 안의 네임2 ... from 테이블이름


                //원하는 코드 응용하여 데이터 베이스를 이용한 무언가의 소스를 집어넣기






                //그리드뷰를 이용하여 데이터가 잘 열렸는지 확인

                
                //CLog.LOG(LOG_TYPE.ODB, "연결 완료");
                if (dfODBLog != null)
                    dfODBLog(st_oraDBInfo.sBASE, "ODB Connect Success");
                return true;
            }
            catch(Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "ERR : " + ex.ToString() + " " + strAccessConn);
                st_oraDBInfo.bStatus = false;
                if (dfODBLog != null)
                    dfODBLog(st_oraDBInfo.sBASE, "ODB Connect(" + strAccessConn + ") Failed");
                return false;
            }

        }

        // 
        //public void DBSelect()
        //{
        //    using (OracleCommand cmd = new OracleCommand())
        //    {
        //        cmd.Connection = OraConn;
        //        cmd.CommandText = "SELECT * from from"
        //    }
        //}

        public bool SelectAction()
        {
            try
            {
                DB_CMD eCmd = DB_CMD.SELECT;
                string sCmd = "SELECT * from custdef";
                //CLog.LOG(LOG_TYPE.ODB, "CmdOracle Set : " + sCmd);
                if (dfODBLog != null)
                    dfODBLog(st_oraDBInfo.sBASE, "ODB Select Query (" + sCmd + ") Start");
                CmdOracle(eCmd, 0, sCmd);
                return true;
            }
            catch (Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "CmdOracle-Err : " + ex.ToString());
                st_oraDBInfo.bStatus = false;
                return false;
            }
        }

       
        public bool CmdOracle(DB_CMD eCmd,int nBeh, string sCmd)
        {

            try
            {
                switch (eCmd)
                {

                    case DB_CMD.SELECT:
                        if(nBeh == 0)
                        {
                            //OracleCommand oC2 = new OracleCommand(sCmd, OraConn);
                            //OracleDataReader oReader = oC2.ExecuteReader();

                            //while(oReader.Read())
                            //{
                            //    st_REG.sCarno = oReader.GetString(0);
                            //    st_REG.sCarno = oReader.GetString(1);
                            //    st_REG.sCarno = oReader.GetString(2);

                            //    if (dfSelect != null)
                            //        dfSelect(st_REG);

                            //}
                        }

                        break;

                }

                //if(CMD == null)
                //{
                //    CMD = "";
                //}
                //cmd.CommandText = CMD;
                //CLog.LOG(LOG_TYPE.SCREEN, "CmdOracle : " + CMD);
                ////밑라인 참조 실행가능한 명령
                ////cmd.CommandText = "insert into exl_m_parking values ('5', '1', '근린공원4호', '경기 화성시 동탄대로21길 19 (영천동)', 37.2072203640951, 127.09663268644, 10, 'card', '카드계산', 120, 1000, 0, 0, '0000', '2359', '', '', '0000', '2359', '화성시청', '031-8059-6538', '24시간운영함')";
                //cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception ex)
            {
                CLog.LOG(LOG_TYPE.ERR, "CmdOracle-Err : " + ex.ToString());
                return false;
            }


        }



        public void Close()
        {
            TiberoConn.Close();
        }



    }
}

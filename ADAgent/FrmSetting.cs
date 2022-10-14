using ADAgent.DATA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADAgent
{
    public partial class FrmSetting : Form
    {

        string[,] sTpmsID = new string[10, 2];
        int nTpmsCnt = 0;
        int nLastTpmsFocus = 0;
        int nLastLprFocus = 0;
        public delegate void DF_TPMSReSetting();
        public delegate void DF_DBReSetting();


        public DF_TPMSReSetting dfTPMSReSetting = null;
        public DF_DBReSetting dfDBReSetting = null;

        public FrmSetting()
        {
            InitializeComponent();
        }

        private void btnTPMSSet_Click(object sender, EventArgs e)
        {
            CIni.Save("ADA_TPMS", "TpmsIP", txtTPMSIP.Text, CData.sTpmsPath);
            CIni.Save("ADA_TPMS", "TpmsPORT", txtTPMSPort.Text, CData.sTpmsPath);
            CIni.Save("ADA_TPMS", "TpmsType", txtTPMSType.Text, CData.sTpmsPath);

            TPMS_textBox_Set();

            if (dfTPMSReSetting != null)
                dfTPMSReSetting();
        }

        private void btnDBSet_Click(object sender, EventArgs e)
        {
            //DB 서버 IP
            CIni.Save("ADA_DB", "DBIP", txtDBIP.Text, CData.sDBPath);

            //DB 서버 PORT
            CIni.Save("ADA_DB", "DBPort", txtDBPort.Text, CData.sDBPath);

            CIni.Save("ADA_DB", "BASEDB", txtBaseDB.Text, CData.sDBPath);

            CIni.Save("ADA_DB", "OPERDB", txtOperDB.Text, CData.sDBPath);

            //DB 서버 ID
            CIni.Save("ADA_DB", "DBID", txtDBID.Text, CData.sDBPath);
            //DB 서버 PW 아마노 MSSQL은 거의 c1441 인듯?? 
            CIni.Save("ADA_DB", "DBPW", txtDBPW.Text, CData.sDBPath);

            DB_textBox_Set();

            if (dfDBReSetting != null)
                dfDBReSetting();
        }

        private void FrmSetting_Load(object sender, EventArgs e)
        {


            cmbLPRCon.Items.AddRange(CData.arNetType);
            cmbLPRIo.Items.AddRange(CData.arIOType);

            cmbLPRCon.SelectedIndex = 0;
            cmbLPRIo.SelectedIndex = 0;

            cmbRemoteCon.Items.AddRange(CData.arNetType);
            cmbRemoteIO.Items.AddRange(CData.arIOType);
            //cmbRemoteType.Items.AddRange(CData.arGTType);

            cmbRemoteCon.SelectedIndex = 0;
            cmbRemoteIO.SelectedIndex = 0;
            //cmbRemoteType.SelectedIndex = 0;


            TPMS_textBox_Set();
            DB_textBox_Set();
            DB_TBL_textBox_Set();
            TPMSID_ListView_Set();
            LPR_ListView_Set();
            RM_ListView_Set();
            DB_Oracle_textBox_Set();

            initControl();
        }

        private void initControl()
        {

            lstOpt1.FullRowSelect = true;
            lstOpt1.GridLines = true;
            lstOpt1.Columns.Add("번호", 50);
            lstOpt1.Columns.Add("옵션", 300);
            lstOpt1.Columns.Add("설정", 100);

            for (int i = 0; i < CData.garOpt1_Name.Length; ++i)
            {
                lstOpt1.Items.Add((i + 1).ToString());
                lstOpt1.Items[i].SubItems.Add(CData.garOpt1_Name[i]);
                if (i < CData.garOpt1.Count)
                    lstOpt1.Items[i].SubItems.Add((CData.garOpt1[i] == 0 ? "X" : "○"));
                else
                {
                    int nVal = 0;
                    if (CData.garOpt1.Count <= i)
                        CData.garOpt1.Add(nVal);
                    CData.garOpt1[i] = nVal;
                    lstOpt1.Items[i].SubItems.Add("X");
                    CDB pDB = new CDB();
                    pDB.Insert_Opt1(i + 1, nVal);
                }
            }
        }

        private void TPMS_textBox_Set()
        {
            txtTPMSIP.Text = CIni.Load("ADA_TPMS", "TpmsIP", "0", CData.sTpmsPath);
            txtTPMSPort.Text = CIni.Load("ADA_TPMS", "TpmsPORT", "0", CData.sTpmsPath);
            txtTPMSType.Text = CIni.Load("ADA_TPMS", "TpmsType", "0", CData.sTpmsPath);
        }

        private void DB_textBox_Set()
        {
            txtDBType.Text = "1";
            txtDBID.Text = CIni.Load("ADA_DB", "DBID", "0", CData.sDBPath);
            txtDBPW.Text = CIni.Load("ADA_DB", "DBPW", "0", CData.sDBPath);
            txtBaseDB.Text = CIni.Load("ADA_DB", "BASEDB", "0", CData.sDBPath);
            txtOperDB.Text = CIni.Load("ADA_DB", "OPERDB", "0", CData.sDBPath);
            txtDBIP.Text = CIni.Load("ADA_DB", "DBIP", "0", CData.sDBPath);
            txtDBPort.Text = CIni.Load("ADA_DB", "DBPort", "0", CData.sDBPath);
        }

        private void DB_TBL_textBox_Set()
        {
            txtTblIO.Text = CIni.Load("ADA_DB_TABLE", "InOut", "Tbl", CData.sDBPath);
            txtTblReg.Text = CIni.Load("ADA_DB_TABLE", "Reg", "Tbl", CData.sDBPath);
            txtTblPay.Text = CIni.Load("ADA_DB_TABLE", "Pay", "Tbl", CData.sDBPath);
            txtTblDis.Text = CIni.Load("ADA_DB_TABLE", "Discount", "Tbl", CData.sDBPath);
            txtParkCode.Text = CIni.Load("ADA_DB_TABLE", "Locate", "column", CData.sDBPath);
            txtFranCode.Text = CIni.Load("ADA_DB_TABLE", "Fran", "column", CData.sDBPath);
            txtTerminalCode.Text = CIni.Load("ADA_DB_TABLE", "Terminal", "column", CData.sDBPath);
            txtAirCode.Text = CIni.Load("ADA_DB_TABLE", "AirPort", "column", CData.sDBPath);
        }
        private void DB_Oracle_textBox_Set()
        {
            //CIni.Save("ADA_DB_ORACLE", "OracleIP", txtOraIP.Text, CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePort", txtOraPORT.Text, CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleSrc", txtOraSrc.Text, CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleID", txtOraID.Text, CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePW", txtOraPW.Text, CData.sDBPath);
            txtOraIP.Text = CIni.Load("ADA_DB_ORACLE", "OracleIP", "0", CData.sDBPath);
            txtOraPORT.Text = CIni.Load("ADA_DB_ORACLE", "OraclePort", "0", CData.sDBPath);
            txtOraSrc.Text = CIni.Load("ADA_DB_ORACLE", "OracleSrc", "0", CData.sDBPath);
            txtOraID.Text = CIni.Load("ADA_DB_ORACLE", "OracleID", "0", CData.sDBPath);
            txtOraPW.Text = CIni.Load("ADA_DB_ORACLE", "OraclePW", "0", CData.sDBPath);
            txtOraBASE.Text = CIni.Load("ADA_DB_ORACLE", "BASEDB", "0", CData.sDBPath);

        }

        private void LPR_ListView_Set()
        {
            lsvLPR.Items.Clear();

            for(int i = 0; i<10; i++)
            {
                string sNetType = cmbLPRCon.Items[int.Parse(CIni.Load("ADA_LPR_" + i.ToString(), "LPRNet", "0", CData.sLPRPath))].ToString();
                string sIP = CIni.Load("ADA_LPR_" + i.ToString(), "LPRIP", "0", CData.sLPRPath);
                string sPort = CIni.Load("ADA_LPR_" + i.ToString(), "LPRPORT", "0", CData.sLPRPath);
                string sIOType = cmbLPRIo.Items[int.Parse(CIni.Load("ADA_LPR_" + i.ToString(), "LPRIO", "0", CData.sLPRPath))].ToString();
                string sID = CIni.Load("ADA_LPR_" + i.ToString(), "LPRID", "0", CData.sLPRPath);
                string sEqpm = CIni.Load("ADA_LPR_" + i.ToString(), "LPREqpm", "0", CData.sLPRPath);
                string sFolder = CIni.Load("ADA_LPR_" + i.ToString(), "LPRFolder", "0", CData.sLPRPath);
                string sUse = ((CIni.Load("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath) == "1") ? "O" : "X");
                string[] sLsvItem = new string[] { i.ToString(), sNetType, sIP, sPort, sIOType, sID, sEqpm, sFolder, sUse };
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvLPR.Items.Add(lviAdd);
            }
            //cmbLPRCon.SelectedIndex = 0;
            //txtNetIP.Text = "";
            //txtNetPORT.Text = "";
            //cmbLPRCon.SelectedIndex = 0;
            //if (cmbTpmsID.Items.Count > 0)
            //    cmbTpmsID.SelectedIndex = 0;
            //txtLPRFolder.Text = "";
            //chkLPRUse.Checked = false;

        }

        private void RM_ListView_Set()
        {
            lsvRemote.Items.Clear();

            for (int i = 0; i < 10; i++)
            {
                string sNetType = cmbRemoteCon.Items[int.Parse(CIni.Load("ADA_RM_" + i.ToString(), "RMNet", "0", CData.sRMPath))].ToString();
                string sIP = CIni.Load("ADA_RM_" + i.ToString(), "RMIP", "0", CData.sRMPath);
                string sPort = CIni.Load("ADA_RM_" + i.ToString(), "RMPORT", "0", CData.sRMPath);
                string sIOType = cmbRemoteIO.Items[int.Parse(CIni.Load("ADA_RM_" + i.ToString(), "RMIO", "0", CData.sRMPath))].ToString();
                string sMatch = CIni.Load("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath);
                string sType = CIni.Load("ADA_RM_" + i.ToString(), "RMUp", "0", CData.sRMPath);
                string sUse = ((CIni.Load("ADA_RM_" + i.ToString(), "Use", "0", CData.sRMPath) == "1") ? "O" : "X");
                string[] sLsvItem = new string[] { i.ToString(), sNetType, sIP, sPort, sIOType, sMatch, sType, sUse };
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvRemote.Items.Add(lviAdd);
            }

        }

        private void TPMSID_ListView_Set()
        {
            lsvTpmsID.Items.Clear();
            cmbTpmsID.Items.Clear();

            cmbRMMatch.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                string sID = CIni.Load("ADA_TPMS_USE_" + i.ToString(), "ID", "0", CData.sTpmsIDPath);
                string sSecID = CIni.Load("ADA_TPMS_USE_" + i.ToString(), "SecID", "0", CData.sTpmsIDPath);
                string sLotArea = CIni.Load("ADA_TPMS_USE_" + i.ToString(), "LotArea", "0", CData.sTpmsIDPath);
                string sUse = ((CIni.Load("ADA_TPMS_USE_" + i.ToString(), "Use", "0", CData.sTpmsIDPath) == "1") ? "O" : "X");
                string[] sLsvItem = new string[] { i.ToString(), sID, sSecID, sLotArea, sUse};
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvTpmsID.Items.Add(lviAdd);

                sTpmsID[i, 0] = CIni.Load("ADA_TPMS_USE_" + i.ToString(), "ID", "0", CData.sTpmsIDPath);

                sTpmsID[i, 1] = CIni.Load("ADA_TPMS_USE_" + i.ToString(), "Use", "0", CData.sTpmsIDPath);

                if (sTpmsID[i, 1] == "1")
                {
                    cmbTpmsID.Items.Add(sTpmsID[i, 0]);
                    cmbRMMatch.Items.Add(sTpmsID[i, 0]);
                }
            }

            txtTpmsID.Text = "";
            txtTpmsSecID.Text = "";
            txtTpmsLot.Text = "";
            chkLPRUse.Checked = false;

            if (cmbTpmsID.Items.Count > 0)
            {
                cmbTpmsID.SelectedIndex = 0;
                cmbRMMatch.SelectedIndex = 0;
            }
            else
            {
                cmbTpmsID.Items.Add("Muin");
                cmbRMMatch.Items.Add("Muin");
            }

            //CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", cmbTpmsID.SelectedItem.ToString(), CData.sLPRPath);

        }

        private void btnTblSave_Click(object sender, EventArgs e)
        {
            CIni.Save("ADA_DB_TABLE", "InOut", txtTblIO.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Pay", txtTblPay.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Reg", txtTblReg.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Discount", txtTblDis.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Locate", txtParkCode.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Fran", txtFranCode.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "Terminal", txtTerminalCode.Text, CData.sDBPath);
            CIni.Save("ADA_DB_TABLE", "AirPort", txtAirCode.Text, CData.sDBPath);
        }

        private void btnTpmsIDSave_Click(object sender, EventArgs e)
        {

        }

        private void btnTpmsIDMod_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (CIni.Load("ADA_LPR_" + i.ToString(), "Use", "0", CData.sLPRPath) == "1")
                    {
                        if (CIni.Load("ADA_LPR_" + i.ToString(), "LPRID", "0", CData.sLPRPath) == CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "ID", "0", CData.sTpmsIDPath))
                        {
                            CIni.Save("ADA_LPR_" + i.ToString(), "LPRID", txtTpmsID.Text, CData.sLPRPath);
                            //if (cmbTpmsID.Items[i].ToString() == CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", "0", CData.sLPRPath))
                            //    cmbTpmsID.SelectedIndex = i;

                        }
                    }

                    if(CIni.Load("ADA_RM_" + i.ToString(), "Use", "0", CData.sRMPath) == "1")
                    {
                        if(CIni.Load("ADA_RM_" + i.ToString(), "RMMatch", "0", CData.sRMPath) == CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "ID", "0", CData.sTpmsIDPath))
                        {
                            CIni.Save("ADA_RM_" + i.ToString(), "RMMatch", txtTpmsID.Text, CData.sRMPath);
                        }
                    }
                }

                CIni.Save("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "ID", txtTpmsID.Text, CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "SecID",txtTpmsSecID.Text, CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "LotArea", txtTpmsLot.Text, CData.sTpmsIDPath);
                CIni.Save("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "Use", ((chkTpmsIDUse.Checked == true) ? "1" : "0"), CData.sTpmsIDPath);



                TPMSID_ListView_Set();
                LPR_ListView_Set();
                RM_ListView_Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btnTpmsIDDel_Click(object sender, EventArgs e)
        {

        }

        private void btnLPRSave_Click(object sender, EventArgs e)
        {

        }

        private void btnLPRMod_Click(object sender, EventArgs e)
        {
            try
            {
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRNet", cmbLPRCon.SelectedIndex.ToString(), CData.sLPRPath);
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRIP", txtNetIP.Text, CData.sLPRPath);
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRPORT", txtNetPORT.Text, CData.sLPRPath);
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRIO", cmbLPRIo.SelectedIndex.ToString(), CData.sLPRPath);
                if((!chkLPRUse.Checked))
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", "0", CData.sLPRPath);
                else
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", cmbTpmsID.SelectedItem.ToString(), CData.sLPRPath);
                CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPREqpm", txtLPREqpm.Text, CData.sLPRPath);
                CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRFolder", txtLPRFolder.Text, CData.sLPRPath);
                    CIni.Save("ADA_LPR_" + lsvLPR.SelectedIndices[0], "Use", ((chkLPRUse.Checked == true) ? "1" : "0"), CData.sLPRPath);
                    LPR_ListView_Set();
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void btnLPRDel_Click(object sender, EventArgs e)
        {

        }
        //CIni.Save("ADA_LPR_"+ i.ToString(), "LPRNet", "0", CData.sLPRPath);
        //        CIni.Save("ADA_LPR_"+ i.ToString(), "LPRIP", "127.0.0.1", CData.sLPRPath);
        //        CIni.Save("ADA_LPR_"+ i.ToString(), "LPRPORT", "80", CData.sLPRPath);
        //        CIni.Save("ADA_LPR_"+ i.ToString(), "LPRIO", "0", CData.sLPRPath);
        //        CIni.Save("ADA_LPR_"+ i.ToString(), "LPRID", "1", CData.sLPRPath);
        //        CIni.Save("ADA_LPR_" + i.ToString(), "LPRFolder", "C", CData.sLPRPath);
        private void lsvLPR_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //ConTYpe = 0 -> S, 1 -> C
                cmbLPRCon.SelectedIndex = int.Parse(CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRNet", "0", CData.sLPRPath));
                txtNetIP.Text = CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRIP", "0", CData.sLPRPath);
                txtNetPORT.Text = CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRPORT", "0", CData.sLPRPath);
                //IOTYpe = 0 -> In, 1 -> Out, 2 -> Back
                cmbLPRIo.SelectedIndex = int.Parse(CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRIO", "0", CData.sLPRPath));
                //for(int i = 0; i<10; i++)
                //{
                //    if (cmbTpmsID.Items[i].)
                //    {
                //        if (cmbTpmsID.Items[i].ToString() == CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", "0", CData.sLPRPath))
                //            cmbTpmsID.SelectedIndex = i;
                //    }
                //        //

                //}
                for (int i = 0; i < cmbTpmsID.Items.Count; i++)
                {
                    if (cmbTpmsID.Items[i].ToString() == CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", "0", CData.sLPRPath))
                        cmbTpmsID.SelectedIndex = i;
                }
                        txtLPREqpm.Text = CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPREqpm", "0", CData.sLPRPath);
                //cmbTpmsID.SelectedIndex = int.Parse(CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRID", "0", CData.sLPRPath));
                txtLPRFolder.Text = CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "LPRFolder", "0", CData.sLPRPath);
                chkLPRUse.Checked = ((CIni.Load("ADA_LPR_" + lsvLPR.SelectedIndices[0], "Use", "0", CData.sLPRPath) == "1") ? true : false);
            }
            catch(Exception)
            {
                cmbLPRCon.SelectedIndex = 0;
                txtNetIP.Text = "";
                txtNetPORT.Text = "";
                cmbLPRIo.SelectedIndex = 0;
                if (cmbTpmsID.Items.Count > 0)
                    cmbTpmsID.SelectedIndex = 0;
                txtLPRFolder.Text = "";
                chkLPRUse.Checked = false;
            }
        }

        private void lsvTpmsID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtTpmsID.Text = CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "ID", "0", CData.sTpmsIDPath);
                txtTpmsSecID.Text = CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "SecID", "0", CData.sTpmsIDPath);
                txtTpmsLot.Text = CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "LotArea", "0", CData.sTpmsIDPath);
                chkTpmsIDUse.Checked = ((CIni.Load("ADA_TPMS_USE_" + lsvTpmsID.SelectedIndices[0], "Use", "0", CData.sTpmsIDPath) == "1")? true : false);
            }
            catch(Exception)
            {
                txtTpmsID.Text = "";
                txtTpmsSecID.Text = "";
                txtTpmsLot.Text = "";
                chkTpmsIDUse.Checked = false;
            }
        }

        private void btnRemoteMod_Click(object sender, EventArgs e)
        {
            try
            {
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMNet", cmbRemoteCon.SelectedIndex.ToString(), CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMIP", txtRemoteIP.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMPort", txtRemotePort.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMIO", cmbRemoteIO.SelectedIndex.ToString(), CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMMatch", cmbRMMatch.SelectedItem.ToString(), CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMUp", txtUpCmd.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMDn", txtDnCmd.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMFix", txtFixCmd.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMUnFix", txtUnFixCmd.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMReset", txtResetCmd.Text, CData.sRMPath);
                CIni.Save("ADA_RM_" + lsvRemote.SelectedIndices[0], "Use", ((chkRemoteUse.Checked == true) ? "1" : "0"), CData.sRMPath);
                RM_ListView_Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void lsvRemote_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbRemoteCon.SelectedIndex = int.Parse(CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMNet", "0", CData.sRMPath));
                txtRemoteIP.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMIP", "0", CData.sRMPath);
                txtRemotePort.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMPORT", "0", CData.sRMPath);
                cmbRemoteIO.SelectedIndex = int.Parse(CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMIO", "0", CData.sRMPath));
                for (int i = 0; i < cmbRMMatch.Items.Count; i++)
                {
                    if (cmbRMMatch.Items[i].ToString() == CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMMatch", "0", CData.sRMPath))
                        cmbRMMatch.SelectedIndex = i;
                }
                txtUpCmd.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMUp", "0", CData.sRMPath);
                txtDnCmd.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMDn", "0", CData.sRMPath);
                txtFixCmd.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMFix", "0", CData.sRMPath);
                txtUnFixCmd.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMUnFix", "0", CData.sRMPath);
                txtResetCmd.Text = CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "RMReset", "0", CData.sRMPath);
                chkRemoteUse.Checked = ((CIni.Load("ADA_RM_" + lsvRemote.SelectedIndices[0], "Use", "0", CData.sRMPath) == "1") ? true : false);
            }
            catch (Exception)
            {
                cmbRemoteCon.SelectedIndex = 0;
                txtRemoteIP.Text = "";
                txtRemotePort.Text = "";
                cmbRemoteIO.SelectedIndex = 0;
                if (cmbRMMatch.Items.Count > 0)
                    cmbRMMatch.SelectedIndex = 0;
                txtUpCmd.Text = "";
                txtDnCmd.Text = "";
                txtFixCmd.Text = "";
                txtUnFixCmd.Text = "";
                txtResetCmd.Text = "";
                chkRemoteUse.Checked = false;            
            }
        }

        private void lstOpt1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstOpt1.SelectedItems.Count > 0)
            {
                if (lstOpt1.Items[lstOpt1.SelectedItems[0].Index].SubItems[2].Text == "○")
                    chkOpt1.Checked = true;
                else
                    chkOpt1.Checked = false;
            }
        }

        private void btnAcceptOpt1_Click(object sender, EventArgs e)
        {
            if (lstOpt1.SelectedItems.Count > 0)
            {
                if (chkOpt1.Checked)
                {
                    CData.garOpt1[lstOpt1.SelectedItems[0].Index] = 1;
                    lstOpt1.Items[lstOpt1.SelectedItems[0].Index].SubItems[2].Text = "○";
                }
                else
                {
                    CData.garOpt1[lstOpt1.SelectedItems[0].Index] = 0;
                    lstOpt1.Items[lstOpt1.SelectedItems[0].Index].SubItems[2].Text = "X";
                }
                CDB pDB = new CDB();
                pDB.Update_Opt1(lstOpt1.SelectedItems[0].Index + 1, CData.garOpt1[lstOpt1.SelectedItems[0].Index]);
            }
        }

        private void btnOraSave_Click(object sender, EventArgs e)
        {
            //CIni.Save("ADA_DB_ORACLE", "OracleIP", "127.0.0.1", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePort", "443", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleSrc", "DH", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OracleID", "sa", CData.sDBPath);
            //CIni.Save("ADA_DB_ORACLE", "OraclePW", "c1441", CData.sDBPath);

            //DB 서버 IP
            CIni.Save("ADA_DB_ORACLE", "OracleIP", txtOraIP.Text, CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OraclePort", txtOraPORT.Text, CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OracleSrc", txtOraSrc.Text, CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OracleID", txtOraID.Text, CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "OraclePW", txtOraPW.Text, CData.sDBPath);
            CIni.Save("ADA_DB_ORACLE", "BASEDB", txtOraBASE.Text, CData.sDBPath);

            DB_Oracle_textBox_Set();
        }

        //private void tabPage4_Click(object sender, EventArgs e)
        //{

        //}
    }
}

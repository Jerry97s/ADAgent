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
    public partial class UC_Reg_Lst : UserControl
    {
        public delegate void DF_Reg_Down(string sDate);
        public DF_Reg_Down dfRegDown = null;

        public delegate void DF_Reg_UPLOAD();
        public DF_Reg_UPLOAD dfRegUpload = null;

        public UC_Reg_Lst()
        {
            InitializeComponent();
        }
        public void Lsv_Clear()
        {
            lsvReg.Items.Clear();
        }

        public void Lsv_Show(int nIdx, string sCarno, string sStartDttm, string sEndDttm, string sUserName)
        {
            string[] sLsvItem = new string[] { nIdx.ToString(), sCarno, sStartDttm, sEndDttm, sUserName };
            ListViewItem lviAdd = new ListViewItem(sLsvItem);
            lsvReg.Items.Add(lviAdd);
        }

        private void btnRegDown_Click(object sender, EventArgs e)
        {
            if (dfRegDown != null)
                dfRegDown(txtRegStart.Text + "|" + txtRegEnd.Text);
        }

        private void btnREGUPLOAD_Click(object sender, EventArgs e)
        {
            if (dfRegUpload != null)
                dfRegUpload();
        }
    }
}

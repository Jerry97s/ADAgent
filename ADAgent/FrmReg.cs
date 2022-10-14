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
    public partial class FrmReg : Form
    {
        public FrmReg()
        {
            InitializeComponent();
        }


        private void FrmReg_Load(object sender, EventArgs e)
        {
            if (CData.ucReg != null)
            {
                Controls.Add(CData.ucReg);
                CData.ucReg.Visible = true;
            }
        }
    }
}

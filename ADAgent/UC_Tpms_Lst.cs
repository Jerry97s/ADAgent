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
    public partial class UC_Tpms_Lst : UserControl
    {

        public UC_Tpms_Lst()
        {
            InitializeComponent();
        }
        public void Lsv_Clear()
        {
            lsvDBLog.Items.Clear();
        }
        public void Lsv_Show(string sDB, string sLog)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sDB, sLog };
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvDBLog.Items.Insert(0, lviAdd);
            }));
        }

    }
}

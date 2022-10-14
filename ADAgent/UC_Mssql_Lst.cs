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
    public partial class UC_Mssql_Lst : UserControl
    {
        public UC_Mssql_Lst()
        {
            InitializeComponent();
        }

        public void Lsv_Clear()
        {
            lsvSvLog.Items.Clear();
        }

        public void Lsv_Show(string sID, string sLog)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                string[] sLsvItem = new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sID, sLog };
                ListViewItem lviAdd = new ListViewItem(sLsvItem);
                lsvSvLog.Items.Insert(0, lviAdd);
            }));
        }

    }
}

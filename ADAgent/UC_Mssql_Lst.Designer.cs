
namespace ADAgent
{
    partial class UC_Mssql_Lst
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.lsvSvLog = new System.Windows.Forms.ListView();
            this.T = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lsvSvLog
            // 
            this.lsvSvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.T,
            this.columnHeader2,
            this.columnHeader7});
            this.lsvSvLog.GridLines = true;
            this.lsvSvLog.HideSelection = false;
            this.lsvSvLog.Location = new System.Drawing.Point(0, 0);
            this.lsvSvLog.Name = "lsvSvLog";
            this.lsvSvLog.Size = new System.Drawing.Size(357, 90);
            this.lsvSvLog.TabIndex = 2;
            this.lsvSvLog.UseCompatibleStateImageBehavior = false;
            this.lsvSvLog.View = System.Windows.Forms.View.Details;
            // 
            // T
            // 
            this.T.Text = "T";
            this.T.Width = 117;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "ID";
            this.columnHeader2.Width = 86;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "S";
            this.columnHeader7.Width = 219;
            // 
            // UC_Mssql_Lst
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lsvSvLog);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UC_Mssql_Lst";
            this.Size = new System.Drawing.Size(358, 90);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvSvLog;
        private System.Windows.Forms.ColumnHeader T;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader7;
    }
}


namespace ADAgent
{
    partial class UC_Tpms_Lst
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
            this.lsvDBLog = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lsvDBLog
            // 
            this.lsvDBLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader8});
            this.lsvDBLog.GridLines = true;
            this.lsvDBLog.HideSelection = false;
            this.lsvDBLog.Location = new System.Drawing.Point(0, 0);
            this.lsvDBLog.Name = "lsvDBLog";
            this.lsvDBLog.Size = new System.Drawing.Size(357, 95);
            this.lsvDBLog.TabIndex = 17;
            this.lsvDBLog.UseCompatibleStateImageBehavior = false;
            this.lsvDBLog.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "T";
            this.columnHeader1.Width = 124;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "DB";
            this.columnHeader3.Width = 84;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "S";
            this.columnHeader8.Width = 236;
            // 
            // UC_Tpms_Lst
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lsvDBLog);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UC_Tpms_Lst";
            this.Size = new System.Drawing.Size(358, 95);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvDBLog;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader8;
    }
}

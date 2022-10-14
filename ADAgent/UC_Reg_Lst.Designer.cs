
namespace ADAgent
{
    partial class UC_Reg_Lst
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
            this.lsvReg = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRegDown = new System.Windows.Forms.Button();
            this.btnREGUPLOAD = new System.Windows.Forms.Button();
            this.txtRegStart = new System.Windows.Forms.TextBox();
            this.txtRegEnd = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lsvReg
            // 
            this.lsvReg.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lsvReg.GridLines = true;
            this.lsvReg.HideSelection = false;
            this.lsvReg.Location = new System.Drawing.Point(3, 44);
            this.lsvReg.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lsvReg.Name = "lsvReg";
            this.lsvReg.Size = new System.Drawing.Size(666, 306);
            this.lsvReg.TabIndex = 0;
            this.lsvReg.UseCompatibleStateImageBehavior = false;
            this.lsvReg.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No";
            this.columnHeader1.Width = 64;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "차번호";
            this.columnHeader2.Width = 118;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "시작";
            this.columnHeader3.Width = 199;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "종료";
            this.columnHeader4.Width = 204;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "사용자명";
            this.columnHeader5.Width = 106;
            // 
            // btnRegDown
            // 
            this.btnRegDown.Location = new System.Drawing.Point(524, 3);
            this.btnRegDown.Name = "btnRegDown";
            this.btnRegDown.Size = new System.Drawing.Size(145, 36);
            this.btnRegDown.TabIndex = 1;
            this.btnRegDown.Text = "정기권 수동다운";
            this.btnRegDown.UseVisualStyleBackColor = true;
            this.btnRegDown.Click += new System.EventHandler(this.btnRegDown_Click);
            // 
            // btnREGUPLOAD
            // 
            this.btnREGUPLOAD.Location = new System.Drawing.Point(373, 3);
            this.btnREGUPLOAD.Name = "btnREGUPLOAD";
            this.btnREGUPLOAD.Size = new System.Drawing.Size(145, 36);
            this.btnREGUPLOAD.TabIndex = 2;
            this.btnREGUPLOAD.Text = "정기권 업로드";
            this.btnREGUPLOAD.UseVisualStyleBackColor = true;
            this.btnREGUPLOAD.Click += new System.EventHandler(this.btnREGUPLOAD_Click);
            // 
            // txtRegStart
            // 
            this.txtRegStart.Location = new System.Drawing.Point(12, 10);
            this.txtRegStart.Name = "txtRegStart";
            this.txtRegStart.Size = new System.Drawing.Size(167, 21);
            this.txtRegStart.TabIndex = 3;
            // 
            // txtRegEnd
            // 
            this.txtRegEnd.Location = new System.Drawing.Point(185, 10);
            this.txtRegEnd.Name = "txtRegEnd";
            this.txtRegEnd.Size = new System.Drawing.Size(167, 21);
            this.txtRegEnd.TabIndex = 4;
            // 
            // UC_Reg_Lst
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtRegEnd);
            this.Controls.Add(this.txtRegStart);
            this.Controls.Add(this.btnREGUPLOAD);
            this.Controls.Add(this.btnRegDown);
            this.Controls.Add(this.lsvReg);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UC_Reg_Lst";
            this.Size = new System.Drawing.Size(672, 356);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lsvReg;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button btnRegDown;
        private System.Windows.Forms.Button btnREGUPLOAD;
        public System.Windows.Forms.TextBox txtRegStart;
        public System.Windows.Forms.TextBox txtRegEnd;
    }
}

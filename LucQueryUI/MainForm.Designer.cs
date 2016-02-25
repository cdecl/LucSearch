namespace LucQueryUI
{
    partial class MainForm
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnQuery = new System.Windows.Forms.Button();
            this.txtDir = new System.Windows.Forms.TextBox();
            this.lblDirectory = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.trResult = new System.Windows.Forms.TreeView();
            this.txtTop = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(440, 12);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(106, 52);
            this.btnQuery.TabIndex = 0;
            this.btnQuery.Text = "실 행";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // txtDir
            // 
            this.txtDir.Location = new System.Drawing.Point(75, 12);
            this.txtDir.Name = "txtDir";
            this.txtDir.Size = new System.Drawing.Size(359, 21);
            this.txtDir.TabIndex = 1;
            // 
            // lblDirectory
            // 
            this.lblDirectory.AutoSize = true;
            this.lblDirectory.Location = new System.Drawing.Point(14, 18);
            this.lblDirectory.Name = "lblDirectory";
            this.lblDirectory.Size = new System.Drawing.Size(55, 12);
            this.lblDirectory.TabIndex = 2;
            this.lblDirectory.Text = "Directory";
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(16, 39);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(418, 52);
            this.txtQuery.TabIndex = 3;
            // 
            // trResult
            // 
            this.trResult.Location = new System.Drawing.Point(16, 97);
            this.trResult.Name = "trResult";
            this.trResult.Size = new System.Drawing.Size(530, 314);
            this.trResult.TabIndex = 4;
            // 
            // txtTop
            // 
            this.txtTop.Location = new System.Drawing.Point(476, 70);
            this.txtTop.Name = "txtTop";
            this.txtTop.Size = new System.Drawing.Size(70, 21);
            this.txtTop.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(441, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "Top";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 423);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTop);
            this.Controls.Add(this.trResult);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.lblDirectory);
            this.Controls.Add(this.txtDir);
            this.Controls.Add(this.btnQuery);
            this.Name = "MainForm";
            this.Text = "LucQuery";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.TextBox txtDir;
        private System.Windows.Forms.Label lblDirectory;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.TreeView trResult;
        private System.Windows.Forms.TextBox txtTop;
        private System.Windows.Forms.Label label1;
    }
}


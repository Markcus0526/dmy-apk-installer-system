namespace ApkInstaller
{
    partial class MinimizeToTrayConfirmForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MinimizeToTrayConfirmForm));
            this.groupBoxAction = new System.Windows.Forms.GroupBox();
            this.radioMinimizeToTray = new System.Windows.Forms.RadioButton();
            this.radioDirectExit = new System.Windows.Forms.RadioButton();
            this.checkNoRetry = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBoxAction.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxAction
            // 
            this.groupBoxAction.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxAction.Controls.Add(this.radioMinimizeToTray);
            this.groupBoxAction.Controls.Add(this.radioDirectExit);
            this.groupBoxAction.Location = new System.Drawing.Point(22, 26);
            this.groupBoxAction.Name = "groupBoxAction";
            this.groupBoxAction.Size = new System.Drawing.Size(301, 102);
            this.groupBoxAction.TabIndex = 0;
            this.groupBoxAction.TabStop = false;
            this.groupBoxAction.Text = "关闭动作";
            // 
            // radioMinimizeToTray
            // 
            this.radioMinimizeToTray.AutoSize = true;
            this.radioMinimizeToTray.Location = new System.Drawing.Point(24, 63);
            this.radioMinimizeToTray.Name = "radioMinimizeToTray";
            this.radioMinimizeToTray.Size = new System.Drawing.Size(97, 17);
            this.radioMinimizeToTray.TabIndex = 1;
            this.radioMinimizeToTray.TabStop = true;
            this.radioMinimizeToTray.Text = "最小化到托盘";
            this.radioMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // radioDirectExit
            // 
            this.radioDirectExit.AutoSize = true;
            this.radioDirectExit.Location = new System.Drawing.Point(24, 31);
            this.radioDirectExit.Name = "radioDirectExit";
            this.radioDirectExit.Size = new System.Drawing.Size(97, 17);
            this.radioDirectExit.TabIndex = 0;
            this.radioDirectExit.TabStop = true;
            this.radioDirectExit.Text = "直接退出程序";
            this.radioDirectExit.UseVisualStyleBackColor = true;
            // 
            // checkNoRetry
            // 
            this.checkNoRetry.AutoSize = true;
            this.checkNoRetry.BackColor = System.Drawing.Color.Transparent;
            this.checkNoRetry.Location = new System.Drawing.Point(27, 150);
            this.checkNoRetry.Name = "checkNoRetry";
            this.checkNoRetry.Size = new System.Drawing.Size(74, 17);
            this.checkNoRetry.TabIndex = 1;
            this.checkNoRetry.Text = "不再提示";
            this.checkNoRetry.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.Location = new System.Drawing.Point(137, 149);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.Location = new System.Drawing.Point(248, 149);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // MinimizeToTrayConfirmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.ForestGreen;
            this.ClientSize = new System.Drawing.Size(349, 191);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.checkNoRetry);
            this.Controls.Add(this.groupBoxAction);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MinimizeToTrayConfirmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "退出";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MinimizeToTrayConfirmForm_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MinimizeToTrayConfirmForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MinimizeToTrayConfirmForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MinimizeToTrayConfirmForm_MouseUp);
            this.groupBoxAction.ResumeLayout(false);
            this.groupBoxAction.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxAction;
        private System.Windows.Forms.RadioButton radioMinimizeToTray;
        private System.Windows.Forms.RadioButton radioDirectExit;
        private System.Windows.Forms.CheckBox checkNoRetry;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
using ApkInstaller.ServiceCorrespond;
namespace ApkInstaller
{
    partial class ApkUpdater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApkUpdater));
            this.prgsDetail = new System.Windows.Forms.ProgressBar();
            this.lblTask = new System.Windows.Forms.Label();
            this.lbl_speed = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // prgsDetail
            // 
            this.prgsDetail.Location = new System.Drawing.Point(12, 37);
            this.prgsDetail.Name = "prgsDetail";
            this.prgsDetail.Size = new System.Drawing.Size(328, 14);
            this.prgsDetail.TabIndex = 0;
            // 
            // lblTask
            // 
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(9, 16);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(177, 13);
            this.lblTask.TabIndex = 1;
            this.lblTask.Text = "更新进度【1/2】 - 下载更新文件";
            // 
            // lbl_speed
            // 
            this.lbl_speed.AutoSize = true;
            this.lbl_speed.Location = new System.Drawing.Point(261, 16);
            this.lbl_speed.Name = "lbl_speed";
            this.lbl_speed.Size = new System.Drawing.Size(64, 13);
            this.lbl_speed.TabIndex = 1;
            this.lbl_speed.Text = "0kb KB/sec";
            this.lbl_speed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ApkUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 67);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.lbl_speed);
            this.Controls.Add(this.prgsDetail);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ApkUpdater";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ApkDataModel.APKLan("APKInstaller");
            this.Load += new System.EventHandler(this.ApkUpdater_Load);
            this.Shown += new System.EventHandler(this.ApkUpdater_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgsDetail;
        private System.Windows.Forms.Label lblTask;
        private System.Windows.Forms.Label lbl_speed;
    }
}


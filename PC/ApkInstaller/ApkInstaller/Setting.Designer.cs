using System.Threading;
namespace ApkInstaller
{
    partial class Setting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting));
            this.chkSetting_ProgramExit = new System.Windows.Forms.CheckBox();
            this.chkSetting_ProgramTask = new System.Windows.Forms.CheckBox();
            this.chkSetting_ProgramOther = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupMainSetting = new System.Windows.Forms.GroupBox();
            this.groupSelectLanguage = new System.Windows.Forms.GroupBox();
            this.lblVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupMainSetting.SuspendLayout();
            this.groupSelectLanguage.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkSetting_ProgramExit
            // 
            resources.ApplyResources(this.chkSetting_ProgramExit, "chkSetting_ProgramExit");
            this.chkSetting_ProgramExit.Name = "chkSetting_ProgramExit";
            this.chkSetting_ProgramExit.UseVisualStyleBackColor = true;
            // 
            // chkSetting_ProgramTask
            // 
            resources.ApplyResources(this.chkSetting_ProgramTask, "chkSetting_ProgramTask");
            this.chkSetting_ProgramTask.Name = "chkSetting_ProgramTask";
            this.chkSetting_ProgramTask.UseVisualStyleBackColor = true;
            // 
            // chkSetting_ProgramOther
            // 
            resources.ApplyResources(this.chkSetting_ProgramOther, "chkSetting_ProgramOther");
            this.chkSetting_ProgramOther.Name = "chkSetting_ProgramOther";
            this.chkSetting_ProgramOther.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbLanguage, "cmbLanguage");
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            resources.GetString("cmbLanguage.Items"),
            resources.GetString("cmbLanguage.Items1")});
            this.cmbLanguage.Name = "cmbLanguage";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ApkInstaller.Properties.Resources.logo_big;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // groupMainSetting
            // 
            this.groupMainSetting.Controls.Add(this.chkSetting_ProgramTask);
            this.groupMainSetting.Controls.Add(this.chkSetting_ProgramExit);
            this.groupMainSetting.Controls.Add(this.chkSetting_ProgramOther);
            resources.ApplyResources(this.groupMainSetting, "groupMainSetting");
            this.groupMainSetting.Name = "groupMainSetting";
            this.groupMainSetting.TabStop = false;
            // 
            // groupSelectLanguage
            // 
            this.groupSelectLanguage.Controls.Add(this.label1);
            this.groupSelectLanguage.Controls.Add(this.cmbLanguage);
            resources.ApplyResources(this.groupSelectLanguage, "groupSelectLanguage");
            this.groupSelectLanguage.Name = "groupSelectLanguage";
            this.groupSelectLanguage.TabStop = false;
            // 
            // lblVersion
            // 
            resources.ApplyResources(this.lblVersion, "lblVersion");
            this.lblVersion.Name = "lblVersion";
            // 
            // Setting
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.groupSelectLanguage);
            this.Controls.Add(this.groupMainSetting);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Setting";
            this.Load += new System.EventHandler(this.Setting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupMainSetting.ResumeLayout(false);
            this.groupSelectLanguage.ResumeLayout(false);
            this.groupSelectLanguage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkSetting_ProgramExit;
        private System.Windows.Forms.CheckBox chkSetting_ProgramTask;
        private System.Windows.Forms.CheckBox chkSetting_ProgramOther;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupMainSetting;
        private System.Windows.Forms.GroupBox groupSelectLanguage;
        private System.Windows.Forms.Label lblVersion;
    }
}
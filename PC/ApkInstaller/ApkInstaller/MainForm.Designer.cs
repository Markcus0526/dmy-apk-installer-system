using System.Threading;
namespace ApkInstaller
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelForm = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnSetting = new System.Windows.Forms.Button();
            this.btnDataAnalysis = new System.Windows.Forms.Button();
            this.btnApplication = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.notifyIconMainFram = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripForTrayIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.btnMinimize = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStripForTrayIcon.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            resources.ApplyResources(this.panelForm, "panelForm");
            this.panelForm.Name = "panelForm";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::ApkInstaller.Properties.Resources.logo;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // btnSetting
            // 
            this.btnSetting.BackColor = System.Drawing.Color.Transparent;
            this.btnSetting.BackgroundImage = global::ApkInstaller.Properties.Resources.setting;
            resources.ApplyResources(this.btnSetting, "btnSetting");
            this.btnSetting.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSetting.FlatAppearance.BorderSize = 0;
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.UseVisualStyleBackColor = false;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnDataAnalysis
            // 
            this.btnDataAnalysis.BackColor = System.Drawing.Color.Transparent;
            this.btnDataAnalysis.Image = global::ApkInstaller.Properties.Resources.statistic;
            resources.ApplyResources(this.btnDataAnalysis, "btnDataAnalysis");
            this.btnDataAnalysis.Name = "btnDataAnalysis";
            this.btnDataAnalysis.UseVisualStyleBackColor = false;
            this.btnDataAnalysis.Click += new System.EventHandler(this.btnDataAnalysis_Click);
            // 
            // btnApplication
            // 
            this.btnApplication.BackColor = System.Drawing.Color.Transparent;
            this.btnApplication.Image = global::ApkInstaller.Properties.Resources.apps;
            resources.ApplyResources(this.btnApplication, "btnApplication");
            this.btnApplication.Name = "btnApplication";
            this.btnApplication.UseVisualStyleBackColor = false;
            this.btnApplication.Click += new System.EventHandler(this.btnApplication_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.BackgroundImage = global::ApkInstaller.Properties.Resources.close;
            resources.ApplyResources(this.btnExit, "btnExit");
            this.btnExit.Name = "btnExit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // notifyIconMainFram
            // 
            this.notifyIconMainFram.ContextMenuStrip = this.contextMenuStripForTrayIcon;
            resources.ApplyResources(this.notifyIconMainFram, "notifyIconMainFram");
            this.notifyIconMainFram.MouseDown += new System.Windows.Forms.MouseEventHandler(this.notifyIconMainFram_MouseDown);
            // 
            // contextMenuStripForTrayIcon
            // 
            this.contextMenuStripForTrayIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRestore,
            this.toolStripMenuItemExit});
            this.contextMenuStripForTrayIcon.Name = "contextMenuStripForTrayIcon";
            resources.ApplyResources(this.contextMenuStripForTrayIcon, "contextMenuStripForTrayIcon");
            // 
            // toolStripMenuItemRestore
            // 
            this.toolStripMenuItemRestore.Name = "toolStripMenuItemRestore";
            resources.ApplyResources(this.toolStripMenuItemRestore, "toolStripMenuItemRestore");
            this.toolStripMenuItemRestore.Click += new System.EventHandler(this.toolStripMenuItemRestore_Click);
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            resources.ApplyResources(this.toolStripMenuItemExit, "toolStripMenuItemExit");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.BackgroundImage = global::ApkInstaller.Properties.Resources.minimize;
            resources.ApplyResources(this.btnMinimize, "btnMinimize");
            this.btnMinimize.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.ForestGreen;
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.panelForm);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnDataAnalysis);
            this.Controls.Add(this.btnApplication);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStripForTrayIcon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnApplication;
        private System.Windows.Forms.Button btnDataAnalysis;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.NotifyIcon notifyIconMainFram;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForTrayIcon;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRestore;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.Button btnMinimize;
    }
}
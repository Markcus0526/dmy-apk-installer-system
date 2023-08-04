namespace ApkInstaller
{
    partial class DataAnalysis
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataAnalysis));
            this.lblUserName = new System.Windows.Forms.Label();
            this.labelTodayStatistics = new System.Windows.Forms.Label();
            this.tabControl_DataAnalysis = new C1.Win.C1Command.C1DockingTab();
            this.tabDataAnalysis_App = new C1.Win.C1Command.C1DockingTabPage();
            this.tabDataAnalysis_Machine = new C1.Win.C1Command.C1DockingTabPage();
            this.labelThisMonthStatistics = new System.Windows.Forms.Label();
            this.pictureUserPhoto = new C1.Win.C1Input.C1PictureBox();
            this.comboSelectProxy = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tabControl_DataAnalysis)).BeginInit();
            this.tabControl_DataAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureUserPhoto)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            resources.ApplyResources(this.lblUserName, "lblUserName");
            this.lblUserName.Name = "lblUserName";
            // 
            // labelTodayStatistics
            // 
            resources.ApplyResources(this.labelTodayStatistics, "labelTodayStatistics");
            this.labelTodayStatistics.Name = "labelTodayStatistics";
            // 
            // tabControl_DataAnalysis
            // 
            resources.ApplyResources(this.tabControl_DataAnalysis, "tabControl_DataAnalysis");
            this.tabControl_DataAnalysis.Controls.Add(this.tabDataAnalysis_App);
            this.tabControl_DataAnalysis.Controls.Add(this.tabDataAnalysis_Machine);
            this.tabControl_DataAnalysis.ItemSize = new System.Drawing.Size(160, 40);
            this.tabControl_DataAnalysis.MultiLine = true;
            this.tabControl_DataAnalysis.Name = "tabControl_DataAnalysis";
            this.tabControl_DataAnalysis.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2010;
            this.tabControl_DataAnalysis.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Silver;
            this.tabControl_DataAnalysis.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Silver;
            // 
            // tabDataAnalysis_App
            // 
            resources.ApplyResources(this.tabDataAnalysis_App, "tabDataAnalysis_App");
            this.tabDataAnalysis_App.Name = "tabDataAnalysis_App";
            // 
            // tabDataAnalysis_Machine
            // 
            resources.ApplyResources(this.tabDataAnalysis_Machine, "tabDataAnalysis_Machine");
            this.tabDataAnalysis_Machine.Name = "tabDataAnalysis_Machine";
            // 
            // labelThisMonthStatistics
            // 
            resources.ApplyResources(this.labelThisMonthStatistics, "labelThisMonthStatistics");
            this.labelThisMonthStatistics.ForeColor = System.Drawing.Color.Black;
            this.labelThisMonthStatistics.Name = "labelThisMonthStatistics";
            // 
            // pictureUserPhoto
            // 
            this.pictureUserPhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureUserPhoto.Image = global::ApkInstaller.Properties.Resources.user;
            resources.ApplyResources(this.pictureUserPhoto, "pictureUserPhoto");
            this.pictureUserPhoto.Name = "pictureUserPhoto";
            this.pictureUserPhoto.TabStop = false;
            // 
            // comboSelectProxy
            // 
            this.comboSelectProxy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comboSelectProxy, "comboSelectProxy");
            this.comboSelectProxy.FormattingEnabled = true;
            this.comboSelectProxy.Name = "comboSelectProxy";
            this.comboSelectProxy.SelectedIndexChanged += new System.EventHandler(this.comboSelectProxy_SelectedIndexChanged);
            this.comboSelectProxy.Click += new System.EventHandler(this.comboSelectProxy_Click);
            // 
            // btnRefresh
            // 
            resources.ApplyResources(this.btnRefresh, "btnRefresh");
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // DataAnalysis
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ControlBox = false;
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.comboSelectProxy);
            this.Controls.Add(this.labelThisMonthStatistics);
            this.Controls.Add(this.tabControl_DataAnalysis);
            this.Controls.Add(this.labelTodayStatistics);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.pictureUserPhoto);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DataAnalysis";
            this.Load += new System.EventHandler(this.DataAnalysis_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabControl_DataAnalysis)).EndInit();
            this.tabControl_DataAnalysis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureUserPhoto)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private C1.Win.C1Input.C1PictureBox pictureUserPhoto;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label labelTodayStatistics;
        private C1.Win.C1Command.C1DockingTab tabControl_DataAnalysis;
        private C1.Win.C1Command.C1DockingTabPage tabDataAnalysis_App;
        private C1.Win.C1Command.C1DockingTabPage tabDataAnalysis_Machine;
        private System.Windows.Forms.Label labelThisMonthStatistics;
        private System.Windows.Forms.ComboBox comboSelectProxy;
        private System.Windows.Forms.Button btnRefresh;
    }
}
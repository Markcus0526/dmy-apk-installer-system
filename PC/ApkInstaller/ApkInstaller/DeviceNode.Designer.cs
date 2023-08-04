namespace ApkInstaller
{
    partial class DeviceNode
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblDeviceNo = new System.Windows.Forms.Label();
            this.lblDeviceBrand = new System.Windows.Forms.Label();
            this.lblDeviceType = new System.Windows.Forms.Label();
            this.btnManagement = new System.Windows.Forms.Button();
            this.labelInstallPosition = new System.Windows.Forms.Label();
            this.cmbInstallPosition = new System.Windows.Forms.ComboBox();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnDetail = new System.Windows.Forms.Button();
            this.prgsConnection = new System.Windows.Forms.ProgressBar();
            this.lblFreeSpaceSDCard = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblFreeSpaceTel = new System.Windows.Forms.Label();
            this.labelBrand = new System.Windows.Forms.Label();
            this.labelModel = new System.Windows.Forms.Label();
            this.picInfo = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelDetail = new System.Windows.Forms.Panel();
            this.prgsUsedSpaceSDCard = new ProgressEx.ProgressEx();
            this.prgsUsedSpaceTel = new ProgressEx.ProgressEx();
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDeviceNo
            // 
            this.lblDeviceNo.AutoSize = true;
            this.lblDeviceNo.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeviceNo.Location = new System.Drawing.Point(50, 22);
            this.lblDeviceNo.Name = "lblDeviceNo";
            this.lblDeviceNo.Size = new System.Drawing.Size(21, 22);
            this.lblDeviceNo.TabIndex = 4;
            this.lblDeviceNo.Text = "1";
            this.lblDeviceNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDeviceBrand
            // 
            this.lblDeviceBrand.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDeviceBrand.Location = new System.Drawing.Point(114, 8);
            this.lblDeviceBrand.Name = "lblDeviceBrand";
            this.lblDeviceBrand.Size = new System.Drawing.Size(100, 18);
            this.lblDeviceBrand.TabIndex = 5;
            this.lblDeviceBrand.Text = "No device";
            this.lblDeviceBrand.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.lblDeviceBrand.Click += new System.EventHandler(this.lblDeviceBrand_Click);
            // 
            // lblDeviceType
            // 
            this.lblDeviceType.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDeviceType.Location = new System.Drawing.Point(114, 38);
            this.lblDeviceType.Name = "lblDeviceType";
            this.lblDeviceType.Size = new System.Drawing.Size(100, 38);
            this.lblDeviceType.TabIndex = 6;
            this.lblDeviceType.Text = "No serial";
            // 
            // btnManagement
            // 
            this.btnManagement.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnManagement.Location = new System.Drawing.Point(8, 79);
            this.btnManagement.Name = "btnManagement";
            this.btnManagement.Size = new System.Drawing.Size(71, 25);
            this.btnManagement.TabIndex = 7;
            this.btnManagement.Text = "管理";
            this.btnManagement.UseVisualStyleBackColor = true;
            this.btnManagement.Click += new System.EventHandler(this.btnManagement_Click);
            // 
            // labelInstallPosition
            // 
            this.labelInstallPosition.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelInstallPosition.Location = new System.Drawing.Point(168, 81);
            this.labelInstallPosition.Name = "labelInstallPosition";
            this.labelInstallPosition.Size = new System.Drawing.Size(120, 23);
            this.labelInstallPosition.TabIndex = 11;
            this.labelInstallPosition.Text = "安装位置";
            this.labelInstallPosition.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbInstallPosition
            // 
            this.cmbInstallPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInstallPosition.FormattingEnabled = true;
            this.cmbInstallPosition.Items.AddRange(new object[] {
            "自动",
            "手机",
            "SD卡"});
            this.cmbInstallPosition.Location = new System.Drawing.Point(296, 79);
            this.cmbInstallPosition.Name = "cmbInstallPosition";
            this.cmbInstallPosition.Size = new System.Drawing.Size(69, 21);
            this.cmbInstallPosition.TabIndex = 12;
            this.cmbInstallPosition.SelectedIndexChanged += new System.EventHandler(this.cmbInstallPosition_SelectedIndexChanged);
            // 
            // btnRestart
            // 
            this.btnRestart.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRestart.Location = new System.Drawing.Point(296, 107);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(69, 25);
            this.btnRestart.TabIndex = 13;
            this.btnRestart.Text = "重启";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnDetail
            // 
            this.btnDetail.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDetail.Location = new System.Drawing.Point(296, 136);
            this.btnDetail.Name = "btnDetail";
            this.btnDetail.Size = new System.Drawing.Size(69, 25);
            this.btnDetail.TabIndex = 14;
            this.btnDetail.Text = "详情";
            this.btnDetail.UseVisualStyleBackColor = true;
            this.btnDetail.Click += new System.EventHandler(this.btnDetail_Click);
            // 
            // prgsConnection
            // 
            this.prgsConnection.Location = new System.Drawing.Point(10, 142);
            this.prgsConnection.Name = "prgsConnection";
            this.prgsConnection.Size = new System.Drawing.Size(270, 15);
            this.prgsConnection.TabIndex = 15;
            // 
            // lblFreeSpaceSDCard
            // 
            this.lblFreeSpaceSDCard.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFreeSpaceSDCard.Location = new System.Drawing.Point(241, 39);
            this.lblFreeSpaceSDCard.Name = "lblFreeSpaceSDCard";
            this.lblFreeSpaceSDCard.Size = new System.Drawing.Size(62, 23);
            this.lblFreeSpaceSDCard.TabIndex = 19;
            this.lblFreeSpaceSDCard.Text = "0.00GB";
            this.lblFreeSpaceSDCard.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.Location = new System.Drawing.Point(34, 107);
            this.lblInfo.MaximumSize = new System.Drawing.Size(250, 30);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(250, 30);
            this.lblInfo.TabIndex = 22;
            this.lblInfo.Text = "This equipment has been connected to this application.";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFreeSpaceTel
            // 
            this.lblFreeSpaceTel.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFreeSpaceTel.Location = new System.Drawing.Point(238, 5);
            this.lblFreeSpaceTel.Name = "lblFreeSpaceTel";
            this.lblFreeSpaceTel.Size = new System.Drawing.Size(65, 23);
            this.lblFreeSpaceTel.TabIndex = 18;
            this.lblFreeSpaceTel.Text = "0.00GB";
            this.lblFreeSpaceTel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelBrand
            // 
            this.labelBrand.AutoSize = true;
            this.labelBrand.Location = new System.Drawing.Point(74, 12);
            this.labelBrand.Name = "labelBrand";
            this.labelBrand.Size = new System.Drawing.Size(34, 13);
            this.labelBrand.TabIndex = 23;
            this.labelBrand.Text = "品牌:";
            // 
            // labelModel
            // 
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(75, 38);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(34, 13);
            this.labelModel.TabIndex = 24;
            this.labelModel.Text = "型号:";
            // 
            // picInfo
            // 
            this.picInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picInfo.Image = global::ApkInstaller.Properties.Resources.cancel;
            this.picInfo.Location = new System.Drawing.Point(10, 112);
            this.picInfo.Name = "picInfo";
            this.picInfo.Size = new System.Drawing.Size(20, 20);
            this.picInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picInfo.TabIndex = 21;
            this.picInfo.TabStop = false;
            this.picInfo.Visible = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackgroundImage = global::ApkInstaller.Properties.Resources.sdcard;
            this.pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox3.Location = new System.Drawing.Point(219, 38);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(16, 21);
            this.pictureBox3.TabIndex = 17;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::ApkInstaller.Properties.Resources.freespace;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox2.Location = new System.Drawing.Point(219, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(16, 33);
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            // 
            // btnStop
            // 
            this.btnStop.BackgroundImage = global::ApkInstaller.Properties.Resources.stop;
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStop.Location = new System.Drawing.Point(139, 79);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(25, 25);
            this.btnStop.TabIndex = 10;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPause
            // 
            this.btnPause.BackgroundImage = global::ApkInstaller.Properties.Resources.pause;
            this.btnPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPause.Location = new System.Drawing.Point(112, 79);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(25, 25);
            this.btnPause.TabIndex = 9;
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackgroundImage = global::ApkInstaller.Properties.Resources.play;
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPlay.Location = new System.Drawing.Point(85, 79);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(24, 25);
            this.btnPlay.TabIndex = 8;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::ApkInstaller.Properties.Resources.mobile;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Location = new System.Drawing.Point(8, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(37, 59);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // panelDetail
            // 
            this.panelDetail.AutoScroll = true;
            this.panelDetail.BackColor = System.Drawing.Color.LightGray;
            this.panelDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDetail.Location = new System.Drawing.Point(10, 169);
            this.panelDetail.Name = "panelDetail";
            this.panelDetail.Size = new System.Drawing.Size(347, 237);
            this.panelDetail.TabIndex = 25;
            this.panelDetail.Visible = false;
            // 
            // prgsUsedSpaceSDCard
            // 
            this.prgsUsedSpaceSDCard.BackColor = System.Drawing.Color.LightGray;
            this.prgsUsedSpaceSDCard.DrawingColor = System.Drawing.Color.Lime;
            this.prgsUsedSpaceSDCard.Location = new System.Drawing.Point(309, 44);
            this.prgsUsedSpaceSDCard.Maximum = 100;
            this.prgsUsedSpaceSDCard.Minimum = 0;
            this.prgsUsedSpaceSDCard.Name = "prgsUsedSpaceSDCard";
            this.prgsUsedSpaceSDCard.PercentageMode = ProgressEx.ProgressEx.PercentageDrawingMode.Center;
            this.prgsUsedSpaceSDCard.Size = new System.Drawing.Size(56, 15);
            this.prgsUsedSpaceSDCard.Step = 1;
            this.prgsUsedSpaceSDCard.TabIndex = 26;
            this.prgsUsedSpaceSDCard.Value = 0;
            // 
            // prgsUsedSpaceTel
            // 
            this.prgsUsedSpaceTel.BackColor = System.Drawing.Color.LightGray;
            this.prgsUsedSpaceTel.DrawingColor = System.Drawing.Color.Lime;
            this.prgsUsedSpaceTel.Location = new System.Drawing.Point(309, 11);
            this.prgsUsedSpaceTel.Maximum = 100;
            this.prgsUsedSpaceTel.Minimum = 0;
            this.prgsUsedSpaceTel.Name = "prgsUsedSpaceTel";
            this.prgsUsedSpaceTel.PercentageMode = ProgressEx.ProgressEx.PercentageDrawingMode.Center;
            this.prgsUsedSpaceTel.Size = new System.Drawing.Size(56, 15);
            this.prgsUsedSpaceTel.Step = 1;
            this.prgsUsedSpaceTel.TabIndex = 27;
            this.prgsUsedSpaceTel.Value = 0;
            // 
            // DeviceNode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.prgsUsedSpaceTel);
            this.Controls.Add(this.prgsUsedSpaceSDCard);
            this.Controls.Add(this.panelDetail);
            this.Controls.Add(this.labelModel);
            this.Controls.Add(this.labelBrand);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.picInfo);
            this.Controls.Add(this.lblFreeSpaceSDCard);
            this.Controls.Add(this.lblFreeSpaceTel);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.prgsConnection);
            this.Controls.Add(this.btnDetail);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.cmbInstallPosition);
            this.Controls.Add(this.labelInstallPosition);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnManagement);
            this.Controls.Add(this.lblDeviceType);
            this.Controls.Add(this.lblDeviceBrand);
            this.Controls.Add(this.lblDeviceNo);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.Name = "DeviceNode";
            this.Size = new System.Drawing.Size(377, 169);
            this.Load += new System.EventHandler(this.DeviceNode_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblDeviceNo;
        private System.Windows.Forms.Label lblDeviceBrand;
        private System.Windows.Forms.Label lblDeviceType;
        private System.Windows.Forms.Button btnManagement;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label labelInstallPosition;
        private System.Windows.Forms.ComboBox cmbInstallPosition;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnDetail;
        private System.Windows.Forms.ProgressBar prgsConnection;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label lblFreeSpaceSDCard;
        private System.Windows.Forms.PictureBox picInfo;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblFreeSpaceTel;
        private System.Windows.Forms.Label labelBrand;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.Panel panelDetail;
        private ProgressEx.ProgressEx prgsUsedSpaceSDCard;
        private ProgressEx.ProgressEx prgsUsedSpaceTel;
    }
}

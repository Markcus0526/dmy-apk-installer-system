using System.Threading;
using System;
namespace ApkInstaller
{
    partial class DeviceManage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceManage));
            this.lblDeviceNo = new System.Windows.Forms.Label();
            this.lblDeviceBrand = new System.Windows.Forms.Label();
            this.lblDeviceType = new System.Windows.Forms.Label();
            this.lblFreeSpaceTel = new System.Windows.Forms.Label();
            this.lblFreeSpaceSDCard = new System.Windows.Forms.Label();
            this.btnApkRefresh = new System.Windows.Forms.Button();
            this.btnAllSelect = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnReturn = new System.Windows.Forms.Button();
            this.lblProgramStatistics = new System.Windows.Forms.Label();
            this.listProgram = new System.Windows.Forms.ListView();
            this.clmProgramName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmInstallTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.pictureScreenShot = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnDeviceRefresh = new System.Windows.Forms.Button();
            this.lbllLoading = new System.Windows.Forms.Label();
            this.lblShotLoading = new System.Windows.Forms.Label();
            this.prgsUsedSpaceTel = new ProgressEx.ProgressEx();
            this.prgsUsedSpaceSDCard = new ProgressEx.ProgressEx();
            ((System.ComponentModel.ISupportInitialize)(this.pictureScreenShot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDeviceNo
            // 
            this.lblDeviceNo.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeviceNo.Location = new System.Drawing.Point(68, 34);
            this.lblDeviceNo.Name = "lblDeviceNo";
            this.lblDeviceNo.Size = new System.Drawing.Size(29, 36);
            this.lblDeviceNo.TabIndex = 1;
            this.lblDeviceNo.Text = "1";
            this.lblDeviceNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDeviceBrand
            // 
            this.lblDeviceBrand.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDeviceBrand.Location = new System.Drawing.Point(100, 16);
            this.lblDeviceBrand.Name = "lblDeviceBrand";
            this.lblDeviceBrand.Size = new System.Drawing.Size(125, 23);
            this.lblDeviceBrand.TabIndex = 2;
            this.lblDeviceBrand.Text = "No device";
            // 
            // lblDeviceType
            // 
            this.lblDeviceType.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDeviceType.Location = new System.Drawing.Point(100, 47);
            this.lblDeviceType.Name = "lblDeviceType";
            this.lblDeviceType.Size = new System.Drawing.Size(125, 59);
            this.lblDeviceType.TabIndex = 3;
            this.lblDeviceType.Text = "No type";
            // 
            // lblFreeSpaceTel
            // 
            this.lblFreeSpaceTel.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFreeSpaceTel.Location = new System.Drawing.Point(276, 19);
            this.lblFreeSpaceTel.Name = "lblFreeSpaceTel";
            this.lblFreeSpaceTel.Size = new System.Drawing.Size(82, 20);
            this.lblFreeSpaceTel.TabIndex = 6;
            this.lblFreeSpaceTel.Text = "0.00GB";
            this.lblFreeSpaceTel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFreeSpaceSDCard
            // 
            this.lblFreeSpaceSDCard.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFreeSpaceSDCard.Location = new System.Drawing.Point(276, 57);
            this.lblFreeSpaceSDCard.Name = "lblFreeSpaceSDCard";
            this.lblFreeSpaceSDCard.Size = new System.Drawing.Size(82, 20);
            this.lblFreeSpaceSDCard.TabIndex = 6;
            this.lblFreeSpaceSDCard.Text = "0.00GB";
            this.lblFreeSpaceSDCard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnApkRefresh
            // 
            this.btnApkRefresh.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnApkRefresh.Location = new System.Drawing.Point(240, 430);
            this.btnApkRefresh.Name = "btnApkRefresh";
            this.btnApkRefresh.Size = new System.Drawing.Size(75, 29);
            this.btnApkRefresh.TabIndex = 9;
            this.btnApkRefresh.Text = "刷新";
            this.btnApkRefresh.UseVisualStyleBackColor = true;
            this.btnApkRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnAllSelect
            // 
            this.btnAllSelect.Font = new System.Drawing.Font("SimSun", 9.75F);
            this.btnAllSelect.Location = new System.Drawing.Point(321, 430);
            this.btnAllSelect.Name = "btnAllSelect";
            this.btnAllSelect.Size = new System.Drawing.Size(71, 29);
            this.btnAllSelect.TabIndex = 9;
            this.btnAllSelect.Text = "全选";
            this.btnAllSelect.UseVisualStyleBackColor = true;
            this.btnAllSelect.Click += new System.EventHandler(this.btnAllSelect_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Font = new System.Drawing.Font("SimSun", 9.75F);
            this.btnUninstall.Location = new System.Drawing.Point(503, 430);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(75, 29);
            this.btnUninstall.TabIndex = 9;
            this.btnUninstall.Text = "卸载";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("SimSun", 9.75F);
            this.btnAdd.Location = new System.Drawing.Point(584, 430);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 29);
            this.btnAdd.TabIndex = 9;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnReturn
            // 
            this.btnReturn.Font = new System.Drawing.Font("SimSun", 9.75F);
            this.btnReturn.Location = new System.Drawing.Point(716, 430);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(75, 29);
            this.btnReturn.TabIndex = 9;
            this.btnReturn.Text = "返回";
            this.btnReturn.UseVisualStyleBackColor = true;
            this.btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            // 
            // lblProgramStatistics
            // 
            this.lblProgramStatistics.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgramStatistics.Location = new System.Drawing.Point(243, 83);
            this.lblProgramStatistics.Name = "lblProgramStatistics";
            this.lblProgramStatistics.Size = new System.Drawing.Size(273, 23);
            this.lblProgramStatistics.TabIndex = 10;
            this.lblProgramStatistics.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listProgram
            // 
            this.listProgram.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmProgramName,
            this.clmVersion,
            this.clmType,
            this.clmSize,
            this.clmInstallTime});
            this.listProgram.Location = new System.Drawing.Point(240, 120);
            this.listProgram.Name = "listProgram";
            this.listProgram.Size = new System.Drawing.Size(551, 304);
            this.listProgram.TabIndex = 11;
            this.listProgram.UseCompatibleStateImageBehavior = false;
            this.listProgram.View = System.Windows.Forms.View.Details;
            // 
            // clmProgramName
            // 
            this.clmProgramName.Text = "软件名";
            this.clmProgramName.Width = 200;
            // 
            // clmVersion
            // 
            this.clmVersion.Text = "版本";
            // 
            // clmType
            // 
            this.clmType.Text = "类型";
            this.clmType.Width = 100;
            // 
            // clmSize
            // 
            this.clmSize.Text = "大小";
            this.clmSize.Width = 100;
            // 
            // clmInstallTime
            // 
            this.clmInstallTime.Text = "安装时间";
            this.clmInstallTime.Width = 120;
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.FileName = "openFileDialog1";
            // 
            // pictureScreenShot
            // 
            this.pictureScreenShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureScreenShot.Location = new System.Drawing.Point(12, 120);
            this.pictureScreenShot.Name = "pictureScreenShot";
            this.pictureScreenShot.Size = new System.Drawing.Size(222, 304);
            this.pictureScreenShot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureScreenShot.TabIndex = 8;
            this.pictureScreenShot.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackgroundImage = global::ApkInstaller.Properties.Resources.usedspace;
            this.pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox3.Location = new System.Drawing.Point(250, 52);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(16, 33);
            this.pictureBox3.TabIndex = 5;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::ApkInstaller.Properties.Resources.freespace;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox2.Location = new System.Drawing.Point(250, 13);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(16, 33);
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ApkInstaller.Properties.Resources.mobile;
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 81);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // btnDeviceRefresh
            // 
            this.btnDeviceRefresh.Font = new System.Drawing.Font("SimSun", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeviceRefresh.Location = new System.Drawing.Point(12, 430);
            this.btnDeviceRefresh.Name = "btnDeviceRefresh";
            this.btnDeviceRefresh.Size = new System.Drawing.Size(75, 29);
            this.btnDeviceRefresh.TabIndex = 12;
            this.btnDeviceRefresh.Text = "刷新";
            this.btnDeviceRefresh.UseVisualStyleBackColor = true;
            this.btnDeviceRefresh.Click += new System.EventHandler(this.btnDeviceRefresh_Click);
            // 
            // lbllLoading
            // 
            this.lbllLoading.AutoSize = true;
            this.lbllLoading.BackColor = System.Drawing.Color.LightGray;
            this.lbllLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbllLoading.Location = new System.Drawing.Point(478, 262);
            this.lbllLoading.Name = "lbllLoading";
            this.lbllLoading.Size = new System.Drawing.Size(54, 13);
            this.lbllLoading.TabIndex = 13;
            this.lbllLoading.Text = "Loading...";
            this.lbllLoading.Visible = false;
            // 
            // lblShotLoading
            // 
            this.lblShotLoading.AutoSize = true;
            this.lblShotLoading.BackColor = System.Drawing.Color.LightGray;
            this.lblShotLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShotLoading.Location = new System.Drawing.Point(89, 262);
            this.lblShotLoading.Name = "lblShotLoading";
            this.lblShotLoading.Size = new System.Drawing.Size(54, 13);
            this.lblShotLoading.TabIndex = 13;
            this.lblShotLoading.Text = "Loading...";
            this.lblShotLoading.Visible = false;
            // 
            // prgsUsedSpaceTel
            // 
            this.prgsUsedSpaceTel.BackColor = System.Drawing.Color.LightGray;
            this.prgsUsedSpaceTel.DrawingColor = System.Drawing.Color.Lime;
            this.prgsUsedSpaceTel.Location = new System.Drawing.Point(364, 22);
            this.prgsUsedSpaceTel.Maximum = 100;
            this.prgsUsedSpaceTel.Minimum = 0;
            this.prgsUsedSpaceTel.Name = "prgsUsedSpaceTel";
            this.prgsUsedSpaceTel.PercentageMode = ProgressEx.ProgressEx.PercentageDrawingMode.Center;
            this.prgsUsedSpaceTel.Size = new System.Drawing.Size(129, 15);
            this.prgsUsedSpaceTel.Step = 1;
            this.prgsUsedSpaceTel.TabIndex = 14;
            this.prgsUsedSpaceTel.Value = 0;
            // 
            // prgsUsedSpaceSDCard
            // 
            this.prgsUsedSpaceSDCard.BackColor = System.Drawing.Color.LightGray;
            this.prgsUsedSpaceSDCard.DrawingColor = System.Drawing.Color.Lime;
            this.prgsUsedSpaceSDCard.Location = new System.Drawing.Point(364, 60);
            this.prgsUsedSpaceSDCard.Maximum = 100;
            this.prgsUsedSpaceSDCard.Minimum = 0;
            this.prgsUsedSpaceSDCard.Name = "prgsUsedSpaceSDCard";
            this.prgsUsedSpaceSDCard.PercentageMode = ProgressEx.ProgressEx.PercentageDrawingMode.Center;
            this.prgsUsedSpaceSDCard.Size = new System.Drawing.Size(129, 15);
            this.prgsUsedSpaceSDCard.Step = 1;
            this.prgsUsedSpaceSDCard.TabIndex = 15;
            this.prgsUsedSpaceSDCard.Value = 0;
            // 
            // DeviceManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 467);
            this.Controls.Add(this.prgsUsedSpaceSDCard);
            this.Controls.Add(this.prgsUsedSpaceTel);
            this.Controls.Add(this.lblShotLoading);
            this.Controls.Add(this.lbllLoading);
            this.Controls.Add(this.btnDeviceRefresh);
            this.Controls.Add(this.listProgram);
            this.Controls.Add(this.lblProgramStatistics);
            this.Controls.Add(this.btnReturn);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUninstall);
            this.Controls.Add(this.btnAllSelect);
            this.Controls.Add(this.btnApkRefresh);
            this.Controls.Add(this.pictureScreenShot);
            this.Controls.Add(this.lblFreeSpaceSDCard);
            this.Controls.Add(this.lblFreeSpaceTel);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.lblDeviceType);
            this.Controls.Add(this.lblDeviceBrand);
            this.Controls.Add(this.lblDeviceNo);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设备管理";
            this.Load += new System.EventHandler(this.DeviceManage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureScreenShot)).EndInit();
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
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label lblFreeSpaceTel;
        private System.Windows.Forms.Label lblFreeSpaceSDCard;
        private System.Windows.Forms.PictureBox pictureScreenShot;
        private System.Windows.Forms.Button btnApkRefresh;
        private System.Windows.Forms.Button btnAllSelect;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.Label lblProgramStatistics;
        private System.Windows.Forms.ListView listProgram;
        private System.Windows.Forms.ColumnHeader clmProgramName;
        private System.Windows.Forms.ColumnHeader clmVersion;
        private System.Windows.Forms.ColumnHeader clmType;
        private System.Windows.Forms.ColumnHeader clmSize;
        private System.Windows.Forms.ColumnHeader clmInstallTime;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.Button btnDeviceRefresh;
        private System.Windows.Forms.Label lbllLoading;
        private System.Windows.Forms.Label lblShotLoading;
        private ProgressEx.ProgressEx prgsUsedSpaceTel;
        private ProgressEx.ProgressEx prgsUsedSpaceSDCard;
    }
}
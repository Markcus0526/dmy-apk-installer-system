namespace ApkInstaller
{
    partial class DeviceSharingCornfrim
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceSharingCornfrim));
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnForceClose = new System.Windows.Forms.Button();
            this.btnShareDevice = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRetry
            // 
            this.btnRetry.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnRetry, "btnRetry");
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.UseVisualStyleBackColor = false;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnForceClose
            // 
            this.btnForceClose.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnForceClose, "btnForceClose");
            this.btnForceClose.Name = "btnForceClose";
            this.btnForceClose.UseVisualStyleBackColor = false;
            this.btnForceClose.Click += new System.EventHandler(this.btnForceClose_Click);
            // 
            // btnShareDevice
            // 
            this.btnShareDevice.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnShareDevice, "btnShareDevice");
            this.btnShareDevice.Name = "btnShareDevice";
            this.btnShareDevice.UseVisualStyleBackColor = false;
            this.btnShareDevice.Click += new System.EventHandler(this.btnShareDevice_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseDown);
            this.lblDescription.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseMove);
            this.lblDescription.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseUp);
            // 
            // DeviceSharingCornfrim
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.ForestGreen;
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnShareDevice);
            this.Controls.Add(this.btnForceClose);
            this.Controls.Add(this.btnRetry);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceSharingCornfrim";
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DeviceSharingCornfrim_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Sharing_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnForceClose;
        private System.Windows.Forms.Button btnShareDevice;
        private System.Windows.Forms.Label lblDescription;
    }
}
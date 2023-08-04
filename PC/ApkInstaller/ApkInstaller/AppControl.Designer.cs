using ApkInstaller.ServiceCorrespond;
using System.Collections.Generic;
using C1.Win.C1Command;
using Newtonsoft.Json;
using System.Threading;
namespace ApkInstaller
{
    partial class AppControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppControl));
            this.tabControl_App = new C1.Win.C1Command.C1DockingTab();
            this.btnAllCheck = new System.Windows.Forms.Button();
            this.listDevices = new System.Windows.Forms.Panel();
            this.timerStatus = new System.Windows.Forms.Timer(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkAutoUninstall = new System.Windows.Forms.CheckBox();
            this.panelManual = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tabControl_App)).BeginInit();
            this.panel2.SuspendLayout();
            this.panelManual.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl_App
            // 
            this.tabControl_App.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tabControl_App.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.tabControl_App, "tabControl_App");
            this.tabControl_App.ItemSize = new System.Drawing.Size(90, 105);
            this.tabControl_App.Name = "tabControl_App";
            this.tabControl_App.Padding = new System.Drawing.Point(6, 10);
            this.tabControl_App.TabLayout = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
            this.tabControl_App.TabsCanFocus = false;
            this.tabControl_App.TabsSpacing = 1;
            this.tabControl_App.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2007;
            this.tabControl_App.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
            this.tabControl_App.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2007Blue;
            // 
            // btnAllCheck
            // 
            resources.ApplyResources(this.btnAllCheck, "btnAllCheck");
            this.btnAllCheck.Image = global::ApkInstaller.Properties.Resources.allcheck;
            this.btnAllCheck.Name = "btnAllCheck";
            this.btnAllCheck.UseVisualStyleBackColor = true;
            this.btnAllCheck.Click += new System.EventHandler(this.btnAllCheck_Click);
            // 
            // listDevices
            // 
            resources.ApplyResources(this.listDevices, "listDevices");
            this.listDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.listDevices.Name = "listDevices";
            // 
            // timerStatus
            // 
            this.timerStatus.Enabled = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.chkAutoUninstall);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(178)))), ((int)(((byte)(227)))));
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // chkAutoUninstall
            // 
            resources.ApplyResources(this.chkAutoUninstall, "chkAutoUninstall");
            this.chkAutoUninstall.Checked = true;
            this.chkAutoUninstall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoUninstall.Name = "chkAutoUninstall";
            this.chkAutoUninstall.UseVisualStyleBackColor = true;
            this.chkAutoUninstall.CheckedChanged += new System.EventHandler(this.chkAutoUninstall_CheckedChanged);
            // 
            // panelManual
            // 
            resources.ApplyResources(this.panelManual, "panelManual");
            this.panelManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelManual.Controls.Add(this.label10);
            this.panelManual.Controls.Add(this.label9);
            this.panelManual.Controls.Add(this.label8);
            this.panelManual.Controls.Add(this.label7);
            this.panelManual.Controls.Add(this.label6);
            this.panelManual.Controls.Add(this.label3);
            this.panelManual.Controls.Add(this.label5);
            this.panelManual.Controls.Add(this.label4);
            this.panelManual.Controls.Add(this.label2);
            this.panelManual.Controls.Add(this.pictureBox1);
            this.panelManual.Controls.Add(this.label1);
            this.panelManual.Name = "panelManual";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.MaximumSize = new System.Drawing.Size(270, 0);
            this.label3.Name = "label3";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.MaximumSize = new System.Drawing.Size(270, 0);
            this.label5.Name = "label5";
            this.label5.Click += new System.EventHandler(this.label2_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.MaximumSize = new System.Drawing.Size(270, 0);
            this.label4.Name = "label4";
            this.label4.Click += new System.EventHandler(this.label2_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.MaximumSize = new System.Drawing.Size(270, 0);
            this.label2.Name = "label2";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Image = global::ApkInstaller.Properties.Resources.pc_usb_phone;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // AppControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.panelManual);
            this.Controls.Add(this.btnAllCheck);
            this.Controls.Add(this.tabControl_App);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.listDevices);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AppControl";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppControl_FormClosing);
            this.Load += new System.EventHandler(this.AppControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabControl_App)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panelManual.ResumeLayout(false);
            this.panelManual.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private C1.Win.C1Command.C1DockingTab tabControl_App;
        private System.Windows.Forms.Button btnAllCheck;
        private System.Windows.Forms.Panel listDevices;
        private System.Windows.Forms.Timer timerStatus;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox chkAutoUninstall;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelManual;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label10;

    }
}
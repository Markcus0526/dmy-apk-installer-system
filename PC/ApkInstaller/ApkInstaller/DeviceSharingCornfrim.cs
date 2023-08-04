using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ApkInstaller.ServiceCorrespond;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace ApkInstaller
{
    public partial class DeviceSharingCornfrim : Form
    {
        public int confirmResult = -1;
        public MainForm mainForm;

        private Boolean m_bDragging = false;
        private Point m_ptCur;
        private Point m_ptFormPoint;  

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
                                                            int nTopRect,
                                                            int nRightRect,
                                                            int nBottomRect,
                                                            int nWidthElipse,
                                                            int nHeightElipse);

        public DeviceSharingCornfrim()
        {
            InitializeComponent();
            this.Text = ApkDataModel.APKLan("APKInstaller");
            RefreshDescription();
            btnRetry.Text = ApkDataModel.APKLan("DeviceSharingRetry");
            btnForceClose.Text = ApkDataModel.APKLan("DeviceSharingForceClose");
            btnShareDevice.Text = ApkDataModel.APKLan("DeviceSharingShare");

            this.TopMost = true;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(2, 2, Width, Height, 10, 10));
        }

        public void RefreshDescription()
        {
            lblDescription.Text = ApkDataModel.APKLan("DeviceSharingDescription") + "\n" +
                String.Format("[{0}]", Program.otherHelperProcessID) + Program.otherHelperProcessPath;
        }

        private void btnShareDevice_Click(object sender, EventArgs e)
        {
            confirmResult = 2;  // share device            
            //mainForm.StartDevicePoll();
            this.Hide();
            mainForm.DeviceSharingFormClosed();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            confirmResult = 0;  // retry            
            this.Hide();
            mainForm.DeviceSharingFormClosed();
        }

        private void Sharing_MouseDown(object sender, MouseEventArgs e)
        {
            m_bDragging = true;
            m_ptCur = Cursor.Position;
            m_ptFormPoint = this.Location;
        }

        private void Sharing_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(m_ptCur));
                this.Location = Point.Add(m_ptFormPoint, new Size(diff));
            }
        }

        private void Sharing_MouseUp(object sender, MouseEventArgs e)
        {
            m_bDragging = false;
        }

        private void btnForceClose_Click(object sender, EventArgs e)
        {
            confirmResult = 1;
            this.Hide();
            mainForm.DeviceSharingFormClosed();
        }

        private void DeviceSharingCornfrim_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(ControlPaint.Light(SystemColors.InactiveBorder, 1.0f)), 5, 5, Width - 10, Height - 10, 5);
        }
    }
}

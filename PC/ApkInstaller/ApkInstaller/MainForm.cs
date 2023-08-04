using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Threading;
using C1.Win.C1Command;
using ApkInstaller.ServiceCorrespond;
using Newtonsoft.Json;
using System.Resources;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management;
using System.IO;

namespace ApkInstaller
{
    public partial class MainForm : Form
    {
        private Boolean m_bDragging = false;
        private Point m_ptCur;
        private Point m_ptFormPoint;
        private Thread m_threadDevPoll;
        public AppControl frmContrl;
        public DataAnalysis frmAnalysis;
        public static Boolean bIsClosing = false;
        public DeviceSharingCornfrim deviceSharingForm = new DeviceSharingCornfrim();
        public MinimizeToTrayConfirmForm exitConfimForm = new MinimizeToTrayConfirmForm();
        public bool bExitFromTray = false;
        public bool bPassExitConfirm = false;


        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
                                                            int nTopRect,
                                                            int nRightRect,
                                                            int nBottomRect,
                                                            int nWidthElipse,
                                                            int nHeightElipse);

        [DllImport("DriverInstaller.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InstallMain();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FreeLibrary(IntPtr hModule);

        IntPtr adbWrapperHandle = IntPtr.Zero;

        public MainForm()
        {
            InitializeComponent();

            SwitchLanguage();

            deviceSharingForm.mainForm = this;
            this.Text = ApkDataModel.APKLan("APKInstaller");
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(2, 2, Width, Height, 10, 10));
            
        }

        #region Control Events
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void MinimizeToTray()
        {
            try
            {                
                notifyIconMainFram.Visible = true;
                notifyIconMainFram.ShowBalloonTip(500);
                this.Hide();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool chkProgramExit, chkNotRetryPrompt;
            String strCheckProgramExit = "false", strCheckNotRetryPrompt = "false";
            
            if (bExitFromTray || bPassExitConfirm)
            {
                return;
            }
                
            try
            {
                strCheckNotRetryPrompt = Program.mINIFileManager.GetINIValue(Program.SECTION_NAME,
                    Program.CHECK_NOT_RETRY_PROMPT, Program.INI_FILE_PATH);

                if (String.IsNullOrEmpty(strCheckNotRetryPrompt))
                {
                    Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                        Program.CHECK_NOT_RETRY_PROMPT, "False", Program.INI_FILE_PATH);
                    strCheckNotRetryPrompt = "False";
                }
                chkNotRetryPrompt = Convert.ToBoolean(strCheckNotRetryPrompt);

                if (chkNotRetryPrompt == false)
                {
                    exitConfimForm.StartPosition = FormStartPosition.CenterScreen;
                    exitConfimForm.bNotRetryPrompt = chkNotRetryPrompt;
                    exitConfimForm.ShowDialog();
                    if (exitConfimForm.confirmResult == false)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("MainForm", "btnExit_Click", ex.Message);
            }

            try
            {
                strCheckProgramExit = Program.mINIFileManager.GetINIValue(Program.SECTION_NAME,
                    Program.CHECK_PROGRAM_EXIT_KEY, Program.INI_FILE_PATH);
                if (String.IsNullOrEmpty(strCheckProgramExit))
                {
                    Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                        Program.CHECK_PROGRAM_EXIT_KEY, "True", Program.INI_FILE_PATH);
                    strCheckProgramExit = "0";
                }
                chkProgramExit = Convert.ToBoolean(strCheckProgramExit);

                if (chkProgramExit)
                {
                    bPassExitConfirm = true;
                    ExitApkInstaller();
                }
                else
                {
                    e.Cancel = true;
                    MinimizeToTray();
                    this.Hide();
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("MainForm", "btnExit_Click", ex.Message);
            }
        }

        private void KillAdbProcess()
        {
            try
            {
                Process[] proc_adb = Process.GetProcessesByName("aiadb");
                proc_adb[0].Kill();
            }
            catch (System.Exception ex)
            {

            }
        }

        private void ExitApkInstaller()
        {
            if (deviceSharingForm != null)
            {
                deviceSharingForm.Close();
            }

            Program.mDeviceList.RemoveAllDevice();
            Program.mDevicePoll.RequestStop();

            if (m_threadDevPoll != null)
            {
                m_threadDevPoll.Join();
                //m_threadDevPoll.Abort();
            }

            if (adbWrapperHandle != IntPtr.Zero)
            {
                FreeLibrary(adbWrapperHandle);
            }

            KillAdbProcess();

            Application.Exit();            
        }

        private void btnApplication_Click(object sender, EventArgs e)
        {
            frmContrl.Visible = true;
            frmAnalysis.Visible = false;            
        }

        private void btnDataAnalysis_Click(object sender, EventArgs e)
        {            
            frmContrl.Visible = false;
            frmAnalysis.Visible = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            frmContrl = new AppControl();
            frmAnalysis = new DataAnalysis();
            frmContrl.TopLevel = false;
            frmAnalysis.TopLevel = false;
            panelForm.Controls.Add(frmContrl);
            panelForm.Controls.Add(frmAnalysis);
            frmAnalysis.Visible = true;
            btnApplication_Click(sender, e);

            adbWrapperHandle = LoadLibrary("ADBWrapper.dll");

            InstallMain();
        }

        public void DeviceSharingFormClosed()
        {
            if (deviceSharingForm.confirmResult == 0)
            {
                CheckAdbPort();
            }
            else if (deviceSharingForm.confirmResult == 1)
            {                
                try
                {
                    Process proc = Process.GetProcessById(Program.otherHelperProcessID);
                    proc.Kill();
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("MainForm", "MainForm()", ex.ToString());
                }
                Thread.Sleep(500);
                CheckAdbPort();
            }
            else
            {
                StartDevicePoll();
            }
        }

        public void StartDevicePoll()
        {
            m_threadDevPoll = new Thread(Program.mDevicePoll.DevicePoll);
            //m_threadDevPoll.IsBackground = true;
            m_threadDevPoll.Start();

            Program.mDevicePoll.InitADBTcpServer();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            Setting frmSetting = new Setting();
            frmSetting.ShowDialog();            
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(ControlPaint.Light(SystemColors.InactiveBorder, 1.0f)), 5, 5, Width - 10, Height - 10, 5);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            m_bDragging = true;
            m_ptCur = Cursor.Position;
            m_ptFormPoint = this.Location;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(m_ptCur));
                this.Location = Point.Add(m_ptFormPoint, new Size(diff));
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            m_bDragging = false;
        }
        #endregion

        private void toolStripMenuItemRestore_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            bIsClosing = true;
            bExitFromTray = true;
            ExitApkInstaller();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            if (Program.chkProgramOther)
                return;
            CheckAdbPort();
        }

        void CheckAdbPortThread()
        {
            AdbPortCheckUtilites portCheckUtilties = new AdbPortCheckUtilites();

            if (portCheckUtilties.CheckIfExistAdbPortUsingProcesses())
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        deviceSharingForm.StartPosition = FormStartPosition.CenterScreen;
                        deviceSharingForm.RefreshDescription();
                        deviceSharingForm.Show();
                    }));
                }
                else
                {
                    deviceSharingForm.StartPosition = FormStartPosition.CenterScreen;
                    deviceSharingForm.RefreshDescription();
                    deviceSharingForm.Show();
                }
            }
            else
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        StartDevicePoll();
                    }));
                }
                else
                {
                    StartDevicePoll();
                }
            }

        }

        public void CheckAdbPort()
        {
            Thread checkPortThread = new Thread(new ThreadStart(CheckAdbPortThread));
            checkPortThread.Start();
        }

        private void notifyIconMainFram_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    this.Show();
                    break;
                case MouseButtons.Right:
                    
                    break;
                case MouseButtons.Middle:
                    break;
                default:
                    break;
            }
        }

        public void SwitchLanguage()
        {
            this.btnDataAnalysis.Text = ApkDataModel.APKLan("DataAnalysis");
            notifyIconMainFram.BalloonTipTitle = ApkDataModel.APKLan("APKInstaller");
            notifyIconMainFram.BalloonTipText = ApkDataModel.APKLan("ProgramMinimized");
            contextMenuStripForTrayIcon.Items[0].Text = ApkDataModel.APKLan("Restore");
            contextMenuStripForTrayIcon.Items[1].Text = ApkDataModel.APKLan("Exit");            
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #region Private Methods
        #endregion
    }
}

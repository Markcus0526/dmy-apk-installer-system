using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ApkInstaller.ServiceCorrespond;
using System.Threading;

namespace ApkInstaller
{
    public enum DeviceStatusIcon
    {
        SUCCESS,
        FAIL,
        WARNING
    }

    public partial class DeviceNode : UserControl
    {
        private Int64 sizeFreeSpaceTel;
        private Int64 sizeFreeSpaceSDCard;
        private float valPercentUsedTel;
        private float valPercentUsedSDCard;

        private List<Label> logLabelList = new List<Label>();
        private List<DeviceLog> logList = new List<DeviceLog>();
        public AppControl appControl;
        public String deviceid;
        
        private ContextMenu panelMenu = new ContextMenu();

        public String DeviceNo
        {
            get
            {
                return this.lblDeviceNo.Text;
            }
            set
            {
                this.lblDeviceNo.Text = value;
            }
        }

        public bool DetailOpened
        {
            get
            {
                return this.panelDetail.Visible;
            }
            set
            {
                this.panelDetail.Visible = value;
            }
        }

        public int InstallLocation
        {
            get
            {
                return this.cmbInstallPosition.SelectedIndex;
            }
            set
            {
                this.cmbInstallPosition.SelectedIndex = value;
            }
        }

        public String DeviceVendor
        {
            get
            {
                return this.lblDeviceBrand.Text;
            }
            set
            {
                this.lblDeviceBrand.Text = value;
            }
        }

        public String DeviceModel
        {
            get
            {
                return this.lblDeviceType.Text;
            }
            set
            {
                lblDeviceType.Text = value;
            }
        }

        public String AndroidDeviceState
        {
            get
            {
                return this.lblInfo.Text;
            }
            set
            {
                lblInfo.Text = value;
            }
        }


        public Int64 FreeSpaceTel
        {
            get
            {
                return sizeFreeSpaceTel;
            }
            set
            {
                sizeFreeSpaceTel = value;
                lblFreeSpaceTel.Text = Program.GetSizeStrAuto(sizeFreeSpaceTel);
            }
        }

        public Int64 FreeSpaceSDCard
        {
            get
            {
                return sizeFreeSpaceSDCard;
            }
            set
            {
                sizeFreeSpaceSDCard = value;
                lblFreeSpaceSDCard.Text = Program.GetSizeStrAuto(sizeFreeSpaceSDCard);
            }
        }

        public float PercentUsedTel
        {
            get
            {
                return valPercentUsedTel;
            }
            set
            {
                valPercentUsedTel = value;
                prgsUsedSpaceTel.Value = (Int32)(valPercentUsedTel * 100);
            }
        }

        public float PercentUsedSDCard
        {
            get
            {
                return valPercentUsedSDCard;
            }
            set
            {
                valPercentUsedSDCard = value;
                prgsUsedSpaceSDCard.Value = (Int32)(valPercentUsedSDCard * 100);
            }
        }

        public float PercentOperationProgress
        {
            get
            {
                return valPercentUsedSDCard;
            }
            set
            {
                valPercentUsedSDCard = value;
                prgsConnection.Value = (Int32)(valPercentUsedSDCard * 100);
            }
        }

        public DeviceNode()
        {
            InitializeComponent();
            SwitchLanguage();
            
        }

        public void SwitchLanguage()
        {
            btnManagement.Text = ApkDataModel.APKLan("DeviceManage");
            labelBrand.Text = ApkDataModel.APKLan("DeviceBrander");
            labelModel.Text = ApkDataModel.APKLan("DeviceModel");
            labelInstallPosition.Text = ApkDataModel.APKLan("DeviceInstallPosition");
            lblInfo.Text = ApkDataModel.APKLan("AlreadyConnected");
            btnRestart.Text = ApkDataModel.APKLan("DeviceRestart");
            btnDetail.Text = ApkDataModel.APKLan("DeviceDetail");

            int tmpIndex = cmbInstallPosition.SelectedIndex;
            cmbInstallPosition.Items.Clear();
            cmbInstallPosition.Items.Add(ApkDataModel.APKLan("DeviceInstallAuto"));
            cmbInstallPosition.Items.Add(ApkDataModel.APKLan("DeviceInstallPhone"));
            cmbInstallPosition.Items.Add(ApkDataModel.APKLan("DeviceInstallSD"));
            cmbInstallPosition.SelectedIndex = tmpIndex;
        }

        public void MakeComponentEnable(bool isEnable)
        {
            cmbInstallPosition.Enabled = isEnable;
        }

        private void DeviceNode_Load(object sender, EventArgs e)
        {
            EnablePlay(false);
            EnablePause(false);
            EnableStop(false);

            lblFreeSpaceTel.Text = ApkDataModel.APKLan("Unknown");
            lblFreeSpaceSDCard.Text = ApkDataModel.APKLan("Unknown");
            lblDeviceBrand.Text = ApkDataModel.APKLan("Unknown");
            lblDeviceType.Text = ApkDataModel.APKLan("Unknown");

            panelMenu.MenuItems.Add(new MenuItem(ApkDataModel.APKLan("Copy"), OnClick_DetailCopy));
            panelMenu.MenuItems.Add(new MenuItem(ApkDataModel.APKLan("Clear"), OnClick_DetailClear));

            this.MouseWheel += new MouseEventHandler(panelDetail_MouseWheel);
            this.panelDetail.MouseWheel += new MouseEventHandler(panelDetail_MouseWheel);

            panelDetail.ContextMenu = panelMenu;
        }

        private void OnClick_DetailCopy(object sender, EventArgs e)
        {
            StringBuilder ret = new StringBuilder();

            foreach (Label item in logLabelList)
            {
                ret.Append(item.Text);
            }

            try
            {
                Clipboard.SetText(ret.ToString());
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private void OnClick_DetailClear(object sender, EventArgs e)
        {
            panelDetail.Controls.Clear();

            logLabelList.Clear();
            logList.Clear();
        }

        public void EnablePlay(bool bEnabled)
        {
            if (bEnabled)
            {
                btnPlay.Image = Properties.Resources.play;
            }
            else
            {
                btnPlay.Image = Properties.Resources.disabled_play;
            }
            btnPlay.Enabled = bEnabled;
        }

        public void EnablePause(bool bEnabled)
        {
            if (bEnabled)
            {
                btnPause.Image = Properties.Resources.pause;
            }
            else
            {
                btnPause.Image = Properties.Resources.disabled_pause;
            }
            btnPause.Enabled = bEnabled;
        }

        public void EnableStop(bool bEnabled)
        {
            if (bEnabled)
            {
                btnStop.Image = Properties.Resources.stop;
            }
            else
            {
                btnStop.Image = Properties.Resources.disabled_stop;
            }
            btnStop.Enabled = bEnabled;
        }

        public void DescribeDeviceLogReport(DeviceLog loginfo)
        {
            Point posData;
            Label newlog = new Label();
            logList.Add(loginfo);
            int labelId = logList.Count() - 1;

            newlog = new Label();
            newlog.Name = "lbllog_" + labelId.ToString();
            newlog.AutoSize = true;
            newlog.BackColor = Color.Transparent;
            newlog.TextAlign = ContentAlignment.TopLeft;
            newlog.Text = loginfo.logtime + " " + loginfo.logtitle + ": " + loginfo.logcontent;

            if (labelId > 0)
            {
                Label prevLabel = logLabelList.LastOrDefault();

                posData = new Point(10, prevLabel.Location.Y + prevLabel.Height + 5);
            }
            else
            {
                posData = new Point(10, 10);
            }
            newlog.Location = posData;

            if (loginfo.logtype == LogType.ERROR)
            {
                newlog.ForeColor = Color.Red;
            }
            else if (loginfo.logtype == LogType.SUCCESS)
            {
                newlog.ForeColor = Color.Green;
            }
            else if (loginfo.logtype == LogType.FAILURE)
            {
                newlog.ForeColor = Color.DarkOrange;
            }
            else if (loginfo.logtype == LogType.WARNING)
            {
                newlog.ForeColor = Color.Red;
            }

            newlog.MaximumSize = new System.Drawing.Size(320, 28);
            

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    logLabelList.Add(newlog);
                    panelDetail.Controls.Add(logLabelList.LastOrDefault());

//                     if (loginfo.seperator)
//                     {
//                         Label seplog = new Label();
//                         logList.Add(new DeviceLog());
//                         labelId = logList.Count() - 1;
//                         seplog.Name = "lbllog_" + labelId.ToString();
//                         seplog.Location = posData;
//                         seplog.BackColor = Color.Transparent;
//                         seplog.TextAlign = ContentAlignment.TopLeft;
//                         seplog.Text = "";
// 
//                         if (labelId > 0)
//                         {
//                             Label prevLabel = logLabelList.LastOrDefault();
// 
//                             posData = new Point(10, prevLabel.Location.Y + prevLabel.Height + 5);
//                         }
//                         else
//                         {
//                             posData = new Point(10, 10);
//                         }
//                         seplog.Location = posData;
//                         seplog.Size = new System.Drawing.Size(290, 10);
// 
//                         logLabelList.Add(seplog);
// 
//                         panelDetail.Controls.Add(logLabelList.LastOrDefault());
//                     }
                }));
            }
            else
            {
                logLabelList.Add(newlog);
                panelDetail.Controls.Add(logLabelList.LastOrDefault());
            }
        }

        private void btnManagement_Click(object sender, EventArgs e)
        {
            AndroidDevice currdev = Program.mDeviceList.FindDevByID(deviceid);

            if (currdev != null && currdev.devStatus == DeviceState.Connected)
            {
                DeviceManage deviceManager = new DeviceManage(this);
                deviceManager.InitComponentsWithDevID(deviceid, DeviceNo);
                deviceManager.ShowDialog();
            }
        }

        private void lblDeviceBrand_Click(object sender, EventArgs e)
        {

        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            lblInfo.Text = ApkDataModel.APKLan("RebootingNow");
            Program.mDeviceList.RebootDevice(deviceid);
        }

        private void cmbInstallPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            AndroidDevice dev = Program.mDeviceList.FindDevByID(deviceid);

            if (dev != null && dev.devStatus == DeviceState.Connected)
            {
                int selectedIndex = this.cmbInstallPosition.SelectedIndex;
                dev.ChangeInstallLocation(selectedIndex);
            }
        }
 
        private void btnPlay_Click(object sender, EventArgs e)
        {
            EnablePlay(false);
            EnablePause(true);
            EnableStop(true);

            AndroidDevice dev = Program.mDeviceList.FindDevByID(deviceid);
            if (dev != null)
            {
                dev.PlayInstall();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            EnablePlay(true);
            EnablePause(false);
            EnableStop(true);

            lblInfo.Text = ApkDataModel.APKLan("Paused");

            AndroidDevice dev = Program.mDeviceList.FindDevByID(deviceid);
            if (dev != null)
            {
                dev.PauseInstall();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EnablePlay(false);
            EnablePause(false);
            EnableStop(false);

            lblInfo.Text = ApkDataModel.APKLan("StopInstall");

            AndroidDevice dev = Program.mDeviceList.FindDevByID(deviceid);

            if (dev != null)
            {
                dev.StopInstall();
            }
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            bool currStatus = panelDetail.Visible;

            for (int i=0; i<appControl.m_arrDevicePanel.Count(); i++)
            {
                DeviceNode nodeitem = appControl.m_arrDevicePanel.ElementAt(i);
                if (nodeitem.DetailOpened)
                {
                    nodeitem.panelDetail.Visible = false;
                    nodeitem.Height -= panelDetail.Height + 10;
                }
            }

            int nDevNo = int.Parse(lblDeviceNo.Text);
            if (currStatus == false)
            {
                panelDetail.Visible = true;
                this.Height += panelDetail.Height + 10;
            }

            RefreshDeviceList();
        }

        public void RefreshDeviceList()
        {
            for (int i = 1; i < appControl.m_arrDevicePanel.Count(); i++)
            {
                DeviceNode nodeitem = appControl.m_arrDevicePanel.ElementAt(i);
                nodeitem.Location = new Point(0, (i > 0) ? (appControl.m_arrDevicePanel[i - 1].Location.Y + appControl.m_arrDevicePanel[i - 1].Height + 2) : 0);
            }
        }

        public void SetMarqueeProgressBar()
        {
            prgsConnection.Style = ProgressBarStyle.Marquee;
            prgsConnection.MarqueeAnimationSpeed = 30;
        }

        public void SetDefaultProgressBar()
        {
            prgsConnection.Style = ProgressBarStyle.Continuous;
            prgsConnection.MarqueeAnimationSpeed = 0;
        }

        public void SetStatusIcon(DeviceStatusIcon status)
        {
            if (status == DeviceStatusIcon.SUCCESS)
            {
                picInfo.Image = Properties.Resources.success;
            }
            else if (status == DeviceStatusIcon.FAIL)
            {
                picInfo.Image = Properties.Resources.error;
            }
            else if (status == DeviceStatusIcon.WARNING)
            {
                picInfo.Image = Properties.Resources.warning;
            }

            picInfo.Visible = true;
        }

        public void ShowDeviceSpace(String deviceid)
        {
            AndroidDevice currDev = Program.mDeviceList.FindDevByID(deviceid);

            if (currDev != null)
            {
                if (currDev.totalSizeTel > 0)
                {
                    FreeSpaceTel = currDev.freeSpaceTel;
                    PercentUsedTel = 1.0f - currDev.freeSpaceTel * 1.0f / currDev.totalSizeTel;
                }
                if (currDev.totalSizeSDCard > 0)
                {
                    FreeSpaceSDCard = currDev.freeSpaceSDCard;
                    PercentUsedSDCard = 1.0f - currDev.freeSpaceSDCard * 1.0f / currDev.totalSizeSDCard;
                }
            }
        }


        public void DisableStatusIcon()
        {
            picInfo.Visible = false;
        }

        private void panelDetail_MouseWheel(object sender, MouseEventArgs e)
        {
            panelDetail.Focus();
            if (e.Delta > 0)
            {

                if (panelDetail.VerticalScroll.Value - 2 >= panelDetail.VerticalScroll.Minimum)
                    panelDetail.VerticalScroll.Value -= 2;
                else
                    panelDetail.VerticalScroll.Value = panelDetail.VerticalScroll.Minimum;
            }
            else
            {
                if (panelDetail.VerticalScroll.Value + 2 <= panelDetail.VerticalScroll.Minimum)
                    panelDetail.VerticalScroll.Value += 2;
                else
                    panelDetail.VerticalScroll.Value = panelDetail.VerticalScroll.Maximum;
            }
        }
    }
}

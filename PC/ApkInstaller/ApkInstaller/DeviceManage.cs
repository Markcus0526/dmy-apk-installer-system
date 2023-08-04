using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using ApkInstaller.ServiceCorrespond;
using System.IO;

namespace ApkInstaller
{
    public partial class DeviceManage : Form
    {
        public Boolean m_bAllSelect = false;
        private DeviceNode m_devNode = null;
        private String m_DeviceID;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
                                                        int nTopRect,
                                                        int nRightRect,
                                                        int nBottomRect,
                                                        int nWidthElipse,
                                                        int nHeightElipse);

        public DeviceManage(DeviceNode devNode)
        {
            InitializeComponent();
            //this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            btnDeviceRefresh.Text = ApkDataModel.APKLan("DeviceManageRefresh");
            btnApkRefresh.Text = ApkDataModel.APKLan("DeviceManageRefresh");
            btnAllSelect.Text = ApkDataModel.APKLan("DeviceManageAllSelect");
            if (Program.LANGUAGEID == (int)ApkDataModel.ApkLanguage.ENGLISH)
                btnAllSelect.Size = new Size(90, 29);
            else
                btnAllSelect.Size = new Size(71, 29);

            btnUninstall.Text = ApkDataModel.APKLan("DeviceManageUninstall");
            btnAdd.Text = ApkDataModel.APKLan("DeviceManageAdd");
            btnReturn.Text = ApkDataModel.APKLan("DeviceManageBack");

            listProgram.Columns[0].Text = ApkDataModel.APKLan("DeviceManageApkName");
            listProgram.Columns[1].Text = ApkDataModel.APKLan("DeviceManageApkVersion");
            listProgram.Columns[2].Text = ApkDataModel.APKLan("DeviceManageApkType");
            listProgram.Columns[3].Text = ApkDataModel.APKLan("DeviceManageApkSize");
            listProgram.Columns[4].Text = ApkDataModel.APKLan("DeviceManageApkInstallTime");
            m_devNode = devNode;
        }

        public void InitComponentsWithDevID(String deviceid, String nDevIndex)
        {
            m_DeviceID = deviceid;
            this.lblDeviceNo.Text = nDevIndex;
        }
        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            String strFilePath = @"";

            dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "APK Files(*.APK)|*.APK";
            dlgOpenFile.Title = @"打开APK文件";
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                strFilePath = dlgOpenFile.FileName;
                Cursor.Current = Cursors.WaitCursor;
                AndroidDevice curdev = Program.mDeviceList.FindDevByID(m_DeviceID);

                if (curdev == null)
                {
                    this.Close();
                    return;
                }
                curdev.bShouldReport = false;
                curdev.InstallApk(strFilePath);
                this.listProgram.Items.Clear();
                LoadAPKData();
                curdev.bShouldReport = true;
                Cursor.Current = Cursors.Default;
            }
        }

        void LoadApkDataThread()
        {
            int freespace = 0;
            AndroidDevice curdev = Program.mDeviceList.FindDevByID(m_DeviceID);

            if (curdev == null)
            {
                this.Close();
                return;
            }

            if (curdev.devStatus != DeviceState.Connected)
            {
                BeginInvoke(new Action(() =>
                {
                    lbllLoading.Visible = false;
                }));
                return;
            }

            try
            {
                curdev.GetSpace();
                curdev.GetInstalledApkInfos();

                BeginInvoke(new Action(() =>
                {
                    this.lblDeviceBrand.Text = curdev.vendor;
                    this.lblDeviceType.Text = curdev.model;
                    if (curdev.freeSpaceTel > 0)
                    {
                        this.lblFreeSpaceTel.Text = Program.GetSizeStrAuto(curdev.freeSpaceTel);
                        freespace = (int)(100 * (1.0f - curdev.freeSpaceTel * 1.0f / curdev.totalSizeTel));

                        if (freespace > this.prgsUsedSpaceTel.Maximum || freespace < this.prgsUsedSpaceTel.Minimum)
                        {
                            //MessageBox.Show("Getting phone free size is incorrect", "Alert");
                        }
                        else
                        {
                            this.prgsUsedSpaceTel.Value = freespace;
                        }
                    }
                    if (curdev.freeSpaceSDCard > 0)
                    {
                        this.lblFreeSpaceSDCard.Text = Program.GetSizeStrAuto(curdev.freeSpaceSDCard);
                        freespace = (int)(100 * (1.0f - curdev.freeSpaceSDCard * 1.0f / curdev.totalSizeSDCard));

                        if (freespace > this.prgsUsedSpaceSDCard.Maximum || freespace < this.prgsUsedSpaceSDCard.Minimum)
                        {
                            //MessageBox.Show("Getting sdcard free size is incorrect", "Alert");
                        }
                        else
                        {
                            this.prgsUsedSpaceSDCard.Value = freespace;
                        }
                    }

                    m_devNode.ShowDeviceSpace(m_DeviceID);

                    this.lblProgramStatistics.Text = String.Format(
                        ApkDataModel.APKLan("ApplicationTotal"),
                        curdev.installedApks.Count(), Program.GetSizeStrAuto(curdev.totalApkSize));

                    String strExePath = System.Windows.Forms.Application.StartupPath;
                    listProgram.AllowColumnReorder = true;
                    listProgram.CheckBoxes = true;

                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    ImageList imageListSmall = new ImageList();
                    ImageList imageListLarge = new ImageList();
                    imageListSmall.ImageSize = new System.Drawing.Size(36, 36);
                    imageListLarge.ImageSize = new System.Drawing.Size(36, 36);

                    for (int i = 0; i < curdev.installedApks.Count(); i++)
                    {
                        ListViewItem item = new ListViewItem(curdev.installedApks[i].appName, 0);
                        item.SubItems.Add(curdev.installedApks[i].versionName);

                        if ((curdev.installedApks[i].flag_app & 1) == 0)
                            item.SubItems.Add(@"用户安装");
                        else
                            item.SubItems.Add(@"系统更新");


                        item.SubItems.Add(Program.GetSizeStrAuto(curdev.installedApks[i].apkSize));

                        DateTime date = start.AddMilliseconds(curdev.installedApks[i].firstInstallTime).ToLocalTime();
                        item.SubItems.Add(date.ToString());
                        item.Checked = false;
                        imageListLarge.Images.Add(curdev.installedApks[i].icon);
                        imageListSmall.Images.Add(curdev.installedApks[i].icon);
                        item.ImageIndex = i;
                        listProgram.Items.Add(item);
                    }

                    listProgram.LargeImageList = imageListLarge;
                    listProgram.SmallImageList = imageListSmall;
                    lbllLoading.Visible = false;
                }));
            }
            catch (System.Exception ex)
            {
                lbllLoading.Visible = false;
                ApkDataModel.WriteLogFile("DeviceManage", "LoadAPKData()", ex.ToString());
            }
        }

        public void LoadAPKData()
        {
            lbllLoading.Visible = true;

            Thread dataThread = new Thread(new ThreadStart(LoadApkDataThread));
            dataThread.Start();
        }

        private void DeviceManage_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Current = Cursors.Default;

            lbllLoading.Text = ApkDataModel.APKLan("Loading");
            lblShotLoading.Text = ApkDataModel.APKLan("Loading");

            LoadAPKData();
            LoadScreenshot();
        }

        private void btnAllSelect_Click(object sender, EventArgs e)
        {
            int nCount = 0;
            nCount = listProgram.Items.Count;

            m_bAllSelect = !m_bAllSelect;
            if (m_bAllSelect)
                btnAllSelect.Text = ApkDataModel.APKLan("DeviceManageAllUnselect");
            else
                btnAllSelect.Text = ApkDataModel.APKLan("DeviceManageAllSelect");
            for (int i = 0; i < nCount; i++)
            {
                if (m_bAllSelect)
                    listProgram.Items[i].Checked = true;
                else
                    listProgram.Items[i].Checked = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!lbllLoading.Visible)
            {
                Cursor.Current = Cursors.WaitCursor;
                this.listProgram.Items.Clear();
                LoadAPKData();
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {

            bool bUninstalled = false;
            AndroidDevice curdev = Program.mDeviceList.FindDevByID(m_DeviceID);

            for (int i = 0; i < listProgram.Items.Count; i++)
            {
                if (listProgram.Items[i].Checked)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    bUninstalled = true;
                    if (curdev == null)
                    {
                        this.Close();
                        return;
                    }
                    curdev.UninstallApk(i, "", false);
                }
            }
            if (bUninstalled)
            {
                this.listProgram.Items.Clear();
                LoadAPKData();
                Cursor.Current = Cursors.Default;
            }
        }

        private void RotateImage(PictureBox pb, Image img, float angle)
        {
            if (img == null || pb.Image == null)
                return;

            Image oldImage = pb.Image;
            pb.Image = AdbImageUtilities.RotateImage(img, angle);
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        private void btnDeviceRefresh_Click(object sender, EventArgs e)
        {
            if (!lblShotLoading.Visible)
            {
                LoadScreenshot();
            }
        }

        void LoadScreenShortThread()
        {
            String imageData;
            AndroidDevice curdev = Program.mDeviceList.FindDevByID(m_DeviceID);

            if (curdev == null)
            {
                this.Close();
                return;
            }

            if (curdev.devStatus != DeviceState.Connected)
            {
                BeginInvoke(new Action(() =>
                {
                    lblShotLoading.Visible = false;
                }));
                return;
            }

            imageData = curdev.ScreenshotCapture();

            if (File.Exists("androidFbImage.bmp"))
            {
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        using (FileStream imgStream = new FileStream(@"androidFbImage.bmp", FileMode.Open, FileAccess.Read))
                        {
                            try
                            {
                                Bitmap imgBmp = new Bitmap(imgStream);
                                pictureScreenShot.Image = imgBmp;
                                if (imgBmp.Width > imgBmp.Height)
                                {
                                    RotateImage(pictureScreenShot, imgBmp, 270);
                                }
                            }
                            catch (System.Exception ex)
                            {
                            	
                            }
                        }
                        lblShotLoading.Visible = false;
                    }));
                }
                catch (System.Exception ex)
                {
                    lblShotLoading.Visible = false;
                    ApkDataModel.WriteLogFile("DeviceManage", "btnDeviceRefresh_Click()", ex.ToString());
                }
            }
        }

        private void LoadScreenshot()
        {
            lblShotLoading.Visible = true;
            Thread screenThread = new Thread(new ThreadStart(LoadScreenShortThread));
            screenThread.Start();
        }

    }
}

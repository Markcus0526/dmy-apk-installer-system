using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using C1.Win.C1Command;
using ApkInstaller.ApkUI;
using ApkInstaller.ServiceCorrespond;
using Newtonsoft.Json;
using ApkInstaller.SQLiteCorrespond.ApkLocalDBTableAdapters;
using ApkInstaller.SQLiteCorrespond;


namespace ApkInstaller
{
    

    public partial class DataAnalysis : Form
    {
        private class DownloadApkImageParam
        {
            public long Index { get; set; }
            public string srcFilePath { get; set; }
            public string targetFilePath { get; set; }
        }

        public DataAnalysis()
        {
            InitializeComponent();

            if (Program.userLevel == ApkDataModel.USERLEVEL.LEVEL1)
                btnRefresh.Visible = false;
            else
                btnRefresh.Visible = true;

            //CallGetApkInstallLog((long)ApkDataModel.USERQUERYLEVEL.LEVELALL);
            CallGetProxyNames();            

            if (Program.userLevel == ApkDataModel.USERLEVEL.LEVEL2)
                comboSelectProxy.Visible = false;

            SwitchLanguage();
        }

        #region members
        private Label[] m_arrLblDeviceName;
        private Label[] m_arrLblThisDay;
        private Label[] m_arrLblThisMonth;
        private PictureBox[] m_arrPicDevice;
        
        private WebClient[] m_arrImgDownClient;
        private AppControl.ApkWorkerStatus[] m_arrImgDownStatus;

        public int m_todayApkCount, m_todayDeviceCount;
        public int m_thisMonthApkCount, m_thisMonthDeviceCount;

        private ApkServiceCall m_apk_install_log_service = new ApkServiceCall();
        private ApkServiceCall m_level2user_service = new ApkServiceCall();

        private List<ApkInstallLogInfo> m_apkInstallLogList;
        
        public List<ApkInstallDeviceStatictics> m_deviceStatisticsList = new List<ApkInstallDeviceStatictics>();
        public List<ApkInstallApkStatictics> m_apkStatisticsList = new List<ApkInstallApkStatictics>();

        public string imgCachePath = Program.LOCAL_APP_PATH + "\\" + Program.IMG_CACHE_PATH;
        public string BaseServerUrl;

        private ApkLocalDB db;
        private ApkSQLManager m_sqlMan = new ApkSQLManager();
        private List<Level2ProxyName> m_level2ProxyNames = new List<Level2ProxyName>();
        private Panel m_loadingPanel;
        private bool proxySelectSelfClicked = false;
        private bool bFirstLoad = true;
        private int prevSelectedProxyIndex = 0;        
        #endregion
          

        #region control events
        private void DataAnalysis_Load(object sender, EventArgs e)
        {
            //CallGetApkInstallLog();

            
        }

        private void comboSelectProxy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (proxySelectSelfClicked)
                return;

            switch (comboSelectProxy.SelectedIndex)
            {
                case 0:
                    CallGetApkInstallLog((long)ApkDataModel.USERQUERYLEVEL.LEVELALL);
                    break;
                case 1:
                    CallGetApkInstallLog((long)ApkDataModel.USERQUERYLEVEL.LEVEL2ALL);
                    break;
                case 2:
                    CallGetApkInstallLog((long)ApkDataModel.USERQUERYLEVEL.ONESELF);
                    break;
                default:
                    Level2ProxyName selectedProxyName = m_level2ProxyNames
                        .Where(m =>
                            m.Name == comboSelectProxy.SelectedItem.ToString()
                        )
                        .FirstOrDefault();
                    CallGetApkInstallLog((long)selectedProxyName.Id);
                    break;
            }
        }
        #endregion


        #region public methods
        public void SwitchLanguage()
        {
            tabDataAnalysis_App.Text = ApkDataModel.APKLan("ApkInstallList");
            tabDataAnalysis_Machine.Text = ApkDataModel.APKLan("DeviceInstallList");
            btnRefresh.Text = ApkDataModel.APKLan("DeviceManageRefresh");
            
            if (Program.userLevel == ApkDataModel.USERLEVEL.LEVEL1)
            {
                this.lblUserName.Text = ApkDataModel.APKLan("FirstGradeAgent") +  Program.UserName; 

                int tmpIndex = comboSelectProxy.SelectedIndex;
                comboSelectProxy.Items.Clear();
                comboSelectProxy.Items.Add(ApkDataModel.APKLan("AllStatistics"));
                comboSelectProxy.Items.Add(ApkDataModel.APKLan("Level2UserStatistics"));
                comboSelectProxy.Items.Add(Program.UserName);
                foreach (Level2ProxyName proxyName in m_level2ProxyNames)
                {
                    comboSelectProxy.Items.Add(proxyName.Name);
                }
                comboSelectProxy.SelectedIndex = tmpIndex;
            }
            else
            {
                //this.lblUserName.Text = ApkDataModel.APKLan("SecondGradeAgent") + Program.UserName; 
                this.lblUserName.Text = Program.UserName; 
            }

            LoadOverheadStatistics();
            LoadApkStatisticsList();
            LoadDeviceStatisticsList();
        }        

        public void CallGetApkInstallLog(long userId)
        {
            Dictionary<string, string> apkparam = new Dictionary<string, string>();
            apkparam.Add("userid", userId.ToString());
            apkparam.Add("token", Program.AUTH_TOKEN);

            Callback_ServiceData callback = new Callback_ServiceData(this.CollectStatistics);

            m_apk_install_log_service.CallApkWorker(
                ApkServiceUri.GetApkInstallLog,
                HttpMethod.GET,
                apkparam,
                callback
            );

            showLoadingPanel(tabControl_DataAnalysis.SelectedIndex);
        }

        public void CallGetProxyNames()
        {
            Dictionary<string, string> apkparam = new Dictionary<string, string>();            
            apkparam.Add("token", Program.AUTH_TOKEN);

            Callback_ServiceData callback = new Callback_ServiceData(this.GetProxyNames);

            m_level2user_service.CallApkWorker(
                ApkServiceUri.GetLevel2UserList,
                HttpMethod.GET,
                apkparam,
                callback
            );
        }

        public void GetProxyNames(ApkResponseData res)
        {
            try
            {
                m_level2ProxyNames = JsonConvert.DeserializeObject<List<Level2ProxyName>>(res.SVCC_DATA.ToString());
                comboSelectProxy.Items.Clear();
                comboSelectProxy.Items.Add(ApkDataModel.APKLan("AllStatistics"));
                comboSelectProxy.Items.Add(ApkDataModel.APKLan("Level2UserStatistics"));
                comboSelectProxy.Items.Add(Program.UserName);
                foreach (Level2ProxyName proxyName in m_level2ProxyNames)
                {
                    comboSelectProxy.Items.Add(proxyName.Name);
                }
                if (bFirstLoad)
                {
                    comboSelectProxy.SelectedIndex = 0;
                    bFirstLoad = false;
                }
                else
                {
                    if (comboSelectProxy.Items.Count > prevSelectedProxyIndex)
                    {
                        comboSelectProxy.SelectedIndex = prevSelectedProxyIndex;                        
                    }
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("DataAnalysis.cs", "GetProxyNames", ex.ToString());
                proxySelectSelfClicked = false;
            }
            proxySelectSelfClicked = false;
        }

        public void CollectStatistics(ApkResponseData res)
        {
            CollectStatisticsOnDevice(res);
            CollectStatisticsOnApk(res);
            LoadOverheadStatistics();
            hideLoadingPanel(tabControl_DataAnalysis.SelectedIndex);
        }

        public void CollectStatisticsOnDevice(ApkResponseData res)
        {
            m_todayDeviceCount = 0;
            m_thisMonthDeviceCount = 0;

            if (res != null)
            {
                try
                {
                    m_apkInstallLogList = JsonConvert.DeserializeObject<List<ApkInstallLogInfo>>(res.SVCC_DATA.ToString());
                }
                catch (System.Exception ex)
                {
                    return;
                }
                

                BaseServerUrl = res.SVCC_BASEURL;

                foreach (ApkInstallLogInfo itemApkInstallLog in m_apkInstallLogList)
                {
                    itemApkInstallLog.TelInfo = itemApkInstallLog.TelType + itemApkInstallLog.TelVendor;
                }

                List<string> telInfos = m_apkInstallLogList.GroupBy(m => m.TelInfo).Select(m => m.Key).ToList();
                DateTime today = DateTime.Today;

                m_deviceStatisticsList.Clear();

                foreach (string telInfo in telInfos)
                {
                    List<ApkInstallLogInfo> subApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInstallLog in m_apkInstallLogList)
                    {
                        if (itemApkInstallLog.TelInfo == telInfo)
                            subApkInstallList.Add(itemApkInstallLog);
                    }

                    List<ApkInstallLogInfo> monthSubApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInsall in subApkInstallList)
                    {
                        if (itemApkInsall.CreateTime.Year == today.Year &&
                            itemApkInsall.CreateTime.Month == today.Month)
                            monthSubApkInstallList.Add(itemApkInsall);
                    }
                    List<string> androidIds = monthSubApkInstallList.GroupBy(m => m.AndroidID).Select(m => m.Key).ToList();
                    ApkInstallDeviceStatictics newDeviceStatistics = new ApkInstallDeviceStatictics();
                    newDeviceStatistics.thisMonthCount = androidIds.Count();

                    List<ApkInstallLogInfo> daySubApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInsall in subApkInstallList)
                    {
                        if (itemApkInsall.CreateTime.Year == today.Year &&
                            itemApkInsall.CreateTime.Month == today.Month &&
                            itemApkInsall.CreateTime.Day == today.Day)
                            daySubApkInstallList.Add(itemApkInsall);
                    }
                    androidIds.Clear();
                    androidIds = daySubApkInstallList.GroupBy(m => m.AndroidID).Select(m => m.Key).ToList();                    
                    newDeviceStatistics.thisDayCount = androidIds.Count();
//                     newDeviceStatistics.thisMonthCount = m_apkInstallLogList
//                         .Where(m =>
//                             m.TelId == telInfo &&
//                             m.CreateTime.Year == today.Year &&
//                             m.CreateTime.Month == today.Month
//                         )
//                         .Count();

//                     newDeviceStatistics.thisDayCount = m_apkInstallLogList
//                         .Where(m =>
//                             m.TelId == telInfo &&
//                             m.CreateTime.Year == today.Year &&
//                             m.CreateTime.Month == today.Month &&
//                             m.CreateTime.Day == today.Day
//                         )
//                         .Count();

                    //var selitem = m_apkInstallLogList.Where(m => m.TelId == telInfo).FirstOrDefault();                    
                    newDeviceStatistics.deviceName = telInfo;
                    m_deviceStatisticsList.Add(newDeviceStatistics);
                                        
                    m_todayDeviceCount += newDeviceStatistics.thisDayCount;
                    m_thisMonthDeviceCount += newDeviceStatistics.thisMonthCount;                    
                }
            }

            LoadDeviceStatisticsList();
        }

        public void CollectStatisticsOnApk(ApkResponseData res)
        {
            m_todayApkCount = 0;
            m_thisMonthApkCount = 0;

            if (res != null)
            {
                try
                {
                    m_apkInstallLogList = JsonConvert.DeserializeObject<List<ApkInstallLogInfo>>(res.SVCC_DATA.ToString());
                }
                catch (System.Exception ex)
                {
                    return;
                }

                List<string> apkids = m_apkInstallLogList.GroupBy(m => m.ApkId).Select(m => m.Key).ToList();
                DateTime today = DateTime.Today;

                m_apkStatisticsList.Clear();

                foreach (string apkid in apkids)
                {
                    List<ApkInstallLogInfo> subApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInstallLog in m_apkInstallLogList)
                    {
                        if (itemApkInstallLog.ApkId == apkid)
                            subApkInstallList.Add(itemApkInstallLog);
                    }

                    List<ApkInstallLogInfo> monthSubApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInsall in subApkInstallList)
                    {
                        if (itemApkInsall.CreateTime.Year == today.Year &&
                            itemApkInsall.CreateTime.Month == today.Month)
                            monthSubApkInstallList.Add(itemApkInsall);
                    }
                    List<string> androidIds = monthSubApkInstallList.GroupBy(m => m.AndroidID).Select(m => m.Key).ToList();
                    ApkInstallApkStatictics newApkStatistics = new ApkInstallApkStatictics();
                    newApkStatistics.thisMonthCount = androidIds.Count();
//                     newApkStatistics.thisMonthCount = m_apkInstallLogList
//                         .Where(m => 
//                             m.ApkId == apkid && 
//                             m.CreateTime.Year == today.Year && 
//                             m.CreateTime.Month == today.Month
//                         )
//                         .Count();

//                     newApkStatistics.thisDayCount = m_apkInstallLogList
//                         .Where(m =>
//                             m.ApkId == apkid &&
//                             m.CreateTime.Year == today.Year &&
//                             m.CreateTime.Month == today.Month &&
//                             m.CreateTime.Day == today.Day
//                         )
//                         .Count();
                    

                    List<ApkInstallLogInfo> daySubApkInstallList = new List<ApkInstallLogInfo>();
                    foreach (ApkInstallLogInfo itemApkInsall in subApkInstallList)
                    {
                        if (itemApkInsall.CreateTime.Year == today.Year &&
                            itemApkInsall.CreateTime.Month == today.Month &&
                            itemApkInsall.CreateTime.Day == today.Day)
                            daySubApkInstallList.Add(itemApkInsall);
                    }
                    androidIds.Clear();
                    androidIds = daySubApkInstallList.GroupBy(m => m.AndroidID).Select(m => m.Key).ToList();
                    newApkStatistics.thisDayCount = androidIds.Count();

                    var selitem = m_apkInstallLogList.Where(m => m.ApkId == apkid).FirstOrDefault();

                    newApkStatistics.apkID = selitem.ApkId;
                    newApkStatistics.apkName = selitem.ApkName;
                    newApkStatistics.imagePath = selitem.ImgPath;

                    m_apkStatisticsList.Add(newApkStatistics);

                    m_todayApkCount += newApkStatistics.thisDayCount;
                    m_thisMonthApkCount += newApkStatistics.thisMonthCount;
                }
            }

            LoadApkStatisticsList();
        }

        public void LoadDeviceStatisticsList()
        {

            int i = 0;

            this.tabDataAnalysis_Machine.Controls.Clear();
            m_arrLblDeviceName = new Label[m_deviceStatisticsList.Count];
            m_arrLblThisDay = new Label[m_deviceStatisticsList.Count];
            m_arrLblThisMonth = new Label[m_deviceStatisticsList.Count];
            m_arrPicDevice = new PictureBox[m_deviceStatisticsList.Count];

            try
            {
                this.tabDataAnalysis_Machine.Controls.Clear();
                foreach (ApkInstallDeviceStatictics itemDeviceStatistics in m_deviceStatisticsList)
                {

                    Point posData = new Point(25 + (i % 5) * 160, 25 + (i / 5) * 90);

                    m_arrLblDeviceName[i] = new Label();
                    m_arrLblDeviceName[i].Location = new Point(posData.X + 50, posData.Y);
                    m_arrLblDeviceName[i].BackColor = Color.Transparent;
                    //m_arrLblDeviceName[i].TextAlign = ContentAlignment.MiddleCenter;
                    m_arrLblDeviceName[i].Text = itemDeviceStatistics.deviceName;
                    m_arrLblDeviceName[i].Size = new System.Drawing.Size(110, 30);
                    //m_arrLblDeviceName[i].Font = new Font(m_arrLblThisMonth[i].Font.FontFamily, 8);
                    
                    m_arrLblThisMonth[i] = new Label();
                    m_arrLblThisMonth[i].Location = new Point(posData.X + 50, posData.Y + 40);
                    m_arrLblThisMonth[i].BackColor = Color.Transparent;
                    //m_arrLblThisMonth[i].TextAlign = ContentAlignment.MiddleCenter;
                    m_arrLblThisMonth[i].Text = String.Format("{0}：{1}", ApkDataModel.APKLan("Today"), itemDeviceStatistics.thisDayCount.ToString());                    
                    m_arrLblThisMonth[i].Size = new System.Drawing.Size(110, 14);
                    m_arrLblThisMonth[i].Font = new Font(m_arrLblThisMonth[i].Font.FontFamily, 8);
                    
                    m_arrLblThisDay[i] = new Label();
                    m_arrLblThisDay[i].Location = new Point(posData.X + 50, posData.Y + 60);
                    m_arrLblThisDay[i].BackColor = Color.Transparent;
                    // m_arrLblThisDay[i].TextAlign = ContentAlignment.MiddleCenter;
                    m_arrLblThisDay[i].Text = String.Format("{0}：{1}", ApkDataModel.APKLan("ThisMonth"), itemDeviceStatistics.thisMonthCount.ToString());
                    m_arrLblThisDay[i].Size = new System.Drawing.Size(110, 14);
                    m_arrLblThisDay[i].Font = new Font(m_arrLblThisDay[i].Font.FontFamily, 8);

                    m_arrPicDevice[i] = new PictureBox();
                    m_arrPicDevice[i].BackColor = Color.Transparent;
                    m_arrPicDevice[i].Image = global::ApkInstaller.Properties.Resources.mobile;
                    m_arrPicDevice[i].Location = posData;
                    m_arrPicDevice[i].Size = new Size(48, 80);

                    this.tabDataAnalysis_Machine.Controls.Add(m_arrLblDeviceName[i]);
                    this.tabDataAnalysis_Machine.Controls.Add(m_arrLblThisDay[i]);
                    this.tabDataAnalysis_Machine.Controls.Add(m_arrLblThisMonth[i]);
                    this.tabDataAnalysis_Machine.Controls.Add(m_arrPicDevice[i]);

                    i++;
                }

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("DataAnalysis", "LoadDeviceStatisticsList()", ex.ToString());
            }
        }

        public void LoadApkStatisticsList()
        {

            int i = 0;
            string[] realImageName;
            string targetPath = Program.LOCAL_APP_PATH + "\\" + Program.IMG_CACHE_PATH;

            this.tabDataAnalysis_App.Controls.Clear();
            m_arrLblDeviceName = new Label[m_apkStatisticsList.Count];
            m_arrLblThisDay = new Label[m_apkStatisticsList.Count];
            m_arrLblThisMonth = new Label[m_apkStatisticsList.Count];
            m_arrPicDevice = new PictureBox[m_apkStatisticsList.Count];

            m_arrImgDownClient = new WebClient[m_apkStatisticsList.Count];
            m_arrImgDownStatus = new AppControl.ApkWorkerStatus[m_apkStatisticsList.Count];

            db = new ApkLocalDB();
            List<DBApkFileInfo> nowApkList = m_sqlMan.GetApkList();

            try
            {
                this.tabDataAnalysis_App.Controls.Clear();
                foreach (ApkInstallApkStatictics itemDeviceStatistics in m_apkStatisticsList)
                {

                    Point posData = new Point(25 + (i % 5) * 160, 25 + (i / 5) * 90);

                    m_arrLblDeviceName[i] = new Label();
                    m_arrLblDeviceName[i].Location = new Point(posData.X + 60, posData.Y);
                    m_arrLblDeviceName[i].BackColor = Color.Transparent;                    
                    m_arrLblDeviceName[i].Text = itemDeviceStatistics.apkName;
                    m_arrLblDeviceName[i].Size = new System.Drawing.Size(100, 14);

                    m_arrLblThisMonth[i] = new Label();
                    m_arrLblThisMonth[i].Location = new Point(posData.X + 60, posData.Y + 20);
                    m_arrLblThisMonth[i].BackColor = Color.Transparent;                    
                    m_arrLblThisMonth[i].Text = String.Format("{0}：{1}", ApkDataModel.APKLan("Today"), itemDeviceStatistics.thisDayCount.ToString());
                    m_arrLblThisMonth[i].Size = new System.Drawing.Size(100, 14);

                    m_arrLblThisDay[i] = new Label();
                    m_arrLblThisDay[i].Location = new Point(posData.X + 60, posData.Y + 40);
                    m_arrLblThisDay[i].BackColor = Color.Transparent;                    
                    m_arrLblThisDay[i].Text = String.Format("{0}：{1}", ApkDataModel.APKLan("ThisMonth"), itemDeviceStatistics.thisMonthCount.ToString());
                    m_arrLblThisDay[i].Size = new System.Drawing.Size(100, 14);

                    m_arrPicDevice[i] = new PictureBox();
                    m_arrPicDevice[i].BackColor = Color.Transparent;
                    //m_arrPicDevice[i].Image = global::ApkInstaller.Properties.Resources.mobile;

                    //realImageName = itemDeviceStatistics.imagePath.Split('/');
                    //m_arrPicDevice[i].Image = Image.FromFile(imgCachePath + realImageName[realImageName.Count() - 1]);
                    

                    m_arrPicDevice[i].Location = posData;
                    m_arrPicDevice[i].Size = new Size(60, 80);

                    this.tabDataAnalysis_App.Controls.Add(m_arrLblDeviceName[i]);
                    this.tabDataAnalysis_App.Controls.Add(m_arrLblThisDay[i]);
                    this.tabDataAnalysis_App.Controls.Add(m_arrLblThisMonth[i]);
                    this.tabDataAnalysis_App.Controls.Add(m_arrPicDevice[i]);


                    // ********************* downloading apk image ***********************************************

                    DBApkFileInfo lfl = nowApkList.Where(m => m.uid == int.Parse(itemDeviceStatistics.apkID)).FirstOrDefault();

                    if (lfl != null && File.Exists(lfl.imgpath))
                    {
                        try
                        {
                            FileStream fs = new FileStream(lfl.imgpath, FileMode.Open, FileAccess.Read);
                            Image apkimg = Image.FromStream(fs);
                            m_arrPicDevice[i].Image = apkimg;
                            fs.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                        	
                        }
                    }
                    else
                    {
                        string[] paths = itemDeviceStatistics.imagePath.Split('/');
                        int nowApkIndex = i;

                        m_arrImgDownClient[i] = new WebClient();
                        m_arrImgDownStatus[i] = AppControl.ApkWorkerStatus.BUSY;
                        m_arrImgDownClient[i].DownloadFileCompleted += (s, e) =>
                        {
                            if (File.Exists(targetPath + paths[paths.Count() - 1]))
                            {
                                try
                                {
                                    FileStream fs = new FileStream(targetPath + paths[paths.Count() - 1], FileMode.Open, FileAccess.Read);
                                    Image apkimg = Image.FromStream(fs);
                                    m_arrPicDevice[nowApkIndex].Image = apkimg;
                                    fs.Dispose();
                                }
                                catch (System.Exception ex)
                                {
                                	
                                }
                            }

                            if (e.Cancelled || e.Error != null)
                            {
                                //File.Delete(lfl.imgpath);
                            }
                            if (lfl != null)
                            {
                                lfl.status &= ~ApkDownStatus.LOADING_IMAGE;
                                lfl.status |= ApkDownStatus.COMPLETED_IMAGE;
                            }

                            m_arrImgDownStatus[nowApkIndex] = AppControl.ApkWorkerStatus.FINISHED;
                        };

                        m_arrImgDownClient[i].DownloadFileAsync(
                            new Uri(BaseServerUrl + itemDeviceStatistics.imagePath),
                            targetPath + paths[paths.Count() - 1]);


                    }

                    i++;
                }

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("DataAnalysis", "LoadApkStatisticsList()", ex.ToString());
            }
        }

        public void LoadOverheadStatistics()
        {
            if (Program.LANGUAGEID == (int)ApkDataModel.ApkLanguage.CHINESE)
            {
                this.labelTodayStatistics.Text = String.Format("今日安装:{0}个应用， 今日出货：{1}部手机",
                m_todayApkCount,
                m_todayDeviceCount);

                this.labelThisMonthStatistics.Text = String.Format("本月总安装:{0}个应用， 本月总出货：{1}部手机",
                    m_thisMonthApkCount,
                    m_thisMonthDeviceCount);
            }
            else
            {
                this.labelTodayStatistics.Text = String.Format("Today {0} applications is installed on {1} devices in total.",
                m_todayApkCount,
                m_todayDeviceCount);

                this.labelThisMonthStatistics.Text = String.Format("This month {0} applications is installed on {1} devices in total.",
                    m_thisMonthApkCount,
                    m_thisMonthDeviceCount);
            }
        }   

        public void showLoadingPanel(int tabIndex)
        {
            comboSelectProxy.Enabled = false;
            C1DockingTabPage currTab;
            if (tabIndex == 0)
            {
                currTab = tabDataAnalysis_App;
            }
            else
            {
                currTab = tabDataAnalysis_Machine;
            }

            currTab.Controls.Clear();
             
            m_loadingPanel = new Panel();
            m_loadingPanel.AutoScroll = true;
            m_loadingPanel.BackColor = Color.Transparent;
            m_loadingPanel.Location = new Point(0, 0);
            m_loadingPanel.Size = new Size(currTab.Width, currTab.Height);

            PictureBox lPic = new PictureBox();
            lPic.Image = Properties.Resources.ajax_loader;
            lPic.Location = new Point((currTab.Width) / 2 - 15, (currTab.Height - lPic.Height) / 2);

            m_loadingPanel.Controls.Add(lPic);

            currTab.Controls.Add(m_loadingPanel);            
        }

        public void hideLoadingPanel(int tabIndex)
        {
            comboSelectProxy.Enabled = true;
            btnRefresh.Enabled = true;
            C1DockingTabPage currTab;
            if (tabIndex == 0)
            {
                currTab = tabDataAnalysis_App;
            }
            else
            {
                currTab = tabDataAnalysis_Machine;
            }
            currTab.Controls.Remove(m_loadingPanel);
        }
        #endregion

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CallGetApkInstallLog((long)ApkDataModel.USERQUERYLEVEL.LEVELALL);
            btnRefresh.Enabled = false;
        }

        private void comboSelectProxy_Click(object sender, EventArgs e)
        {
            if (proxySelectSelfClicked)
                return;

            proxySelectSelfClicked = true;
            prevSelectedProxyIndex = comboSelectProxy.SelectedIndex;            
            CallGetProxyNames();            
        }
    }
}

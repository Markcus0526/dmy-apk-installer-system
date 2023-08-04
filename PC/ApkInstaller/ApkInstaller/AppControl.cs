using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using ApkInstaller.ServiceCorrespond;
using Newtonsoft.Json;
using C1.Win.C1Command;
using ApkInstaller.SQLiteCorrespond;
using System.Net;
using System.IO;
using System.Threading;
using ApkInstaller.ApkUI;
using System.Threading.Tasks;

namespace ApkInstaller
{
    #region Apk Data Type
    #endregion

    public partial class AppControl : Form
    {
        public Boolean m_bAllCheck = false;
        public List<DeviceNode> m_arrDevicePanel = new List<DeviceNode>();

        private List<ApkGroupInfo> m_groupList;
        private C1DockingTabPage[] m_groupTabs;
        public ApkSQLManager m_sqlMan = new ApkSQLManager();
        private Thread m_waitLoadApkThread = null;
        private bool m_bwaitThreadCancel = false;
        private ApkServiceCall m_service = new ApkServiceCall();

        /* Apk File UI Components */
        private ApkPictureBox[] m_arrPicNew;
        private Label[] m_arrLblData;
        public ProgressBar[] m_arrPercent;

        private ApkDownManager[] m_arrApkDownMan;
        private WebClient[] m_arrImgDownClient;

        private Panel m_loadingPanel;
        private int m_currGroupIndex;
        private int m_apkCount = 0;
//        private bool m_GroupLoading = false;

        public const int COUNT = 50;
        public const int DEVICES = 12;

        public enum ApkWorkerStatus
        {
            BUSY = 0x00,
            FINISHED = 0x01
        }

        public class ApkParam
        {
            public object pitem { get; set; }
            public ApkFileInfo item { get; set; }
            public ApkResponseData data { get; set; }
        }

        public class ApkDownParam
        {
            public int pIndex { get; set; }
            public ApkResponseData data { get; set; }
        }

        public class ApkGroupParam
        {
            public object pitem { get; set; }
            public ApkGroupInfo item { get; set; }
            public ApkResponseData data { get; set; }
        }

        #region Constructor
        public AppControl()
        {
            InitializeComponent();
            try
            {
                Program.mDevicePoll.onDeviceChange += new AdbDevicePoll.DeviceChangeHandler(onDeviceChange);
                Program.mDevicePoll.onDeviceReport += new AdbDevicePoll.DeviceReportHandler(onDeviceReport);
                Program.mDevicePoll.onDeviceLoad += new AdbDevicePoll.DeviceInitializeHandler(onDeviceLoad);
                Program.mDevicePoll.onDeviceLoadFail += new AdbDevicePoll.DeviceInitFailHandler(onDeviceLoadFail);
                Program.mDevicePoll.onDeviceDetected += new AdbDevicePoll.DeviceDetectedHandler(onDeviceDetected);

                m_arrPicNew = new ApkPictureBox[0];
                m_arrPercent = new ProgressBar[0];
                m_arrLblData = new Label[0];
                m_arrImgDownClient = new WebClient[0];
                m_arrApkDownMan = new ApkDownManager[0];

                SwitchLanguage();
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "AppControl()", ex.ToString());

            }
        }
        #endregion

        private static void StatusTimer(Object state)
        {

        }

        #region Events

        public delegate void Callback_AllInstallCompleted();

        public void OnCompleted_AllInstall()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    MakeControlComponentEnable(true);
                }));
            }
            else
            {
                MakeControlComponentEnable(true);
            }
            //MessageBox.Show("批量安装完成", ApkDataModel.APKLan("Notice"));
        }

        private void MakeControlComponentEnable(bool isEnable)
        {
            this.btnAllCheck.Enabled = isEnable;
            for (int i = 0; i < m_arrDevicePanel.Count; i++)
            {
                DeviceNode nodeItem = m_arrDevicePanel.ElementAt(i);
                nodeItem.MakeComponentEnable(isEnable);
            }
        }

        private void btnAllCheck_Click(object sender, EventArgs e)
        {
            try
            {
                int completeCnt = m_sqlMan.m_apkList.Where(m => m.status.HasFlag(ApkDownStatus.COMPLETED_FILE)).Count();
                if (completeCnt != m_sqlMan.m_apkList.Count)
                {
                    MessageBox.Show(String.Format(ApkDataModel.APKLan("MSG_STILL_DOWNLOAD"), (m_sqlMan.m_apkList.Count - completeCnt).ToString()), 
                        ApkDataModel.APKLan("APKInstaller"));
                    return;
                }

                if (Program.mDeviceList.GetConnectedDeviceCount() < 1)
                {
                    MessageBox.Show(ApkDataModel.APKLan("MSG_NEED_DEVICE"), ApkDataModel.APKLan("APKInstaller"));
                    return;
                }

                this.btnAllCheck.Enabled = false;
                MakeControlComponentEnable(false);

                List<ApkAdbParam> apkparams = new List<ApkAdbParam>();
                for (int i = 0; i < completeCnt; i++)
                {
                    if (String.IsNullOrEmpty(m_sqlMan.m_apkList[i].package))
                    {
                        continue;
                    }

                    ApkAdbParam aparam = new ApkAdbParam();
                    aparam.uid = m_sqlMan.m_apkList[i].uid;
                    aparam.name = m_sqlMan.m_apkList[i].name;
                    aparam.filepath = m_sqlMan.m_apkList[i].filepath;
                    aparam.vcode = m_sqlMan.m_apkList[i].vcode;
                    aparam.vname = m_sqlMan.m_apkList[i].vname;
                    aparam.package = m_sqlMan.m_apkList[i].package;

                    apkparams.Add(aparam);
                }

                Program.m_callbackInstComp = new Callback_AllInstallCompleted(this.OnCompleted_AllInstall);

                Program.mDeviceList.InstallApk(tabControl_App.SelectedTab.Text, apkparams, chkAutoUninstall.Checked);
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "btnAllCheck_Click()", ex.ToString());
            }
        }

        delegate void OnInstallStart(int nDevNo);

        void DeviceLogReport(String deviceid, DeviceLog loginfo)
        {
            DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();

            if (currNode != null)
            {
                currNode.DescribeDeviceLogReport(loginfo);
            }
        }

        void BeginInstallApks(String deviceid)
        {
            try
            {
                BeginInvoke(new Action(() =>
                {
                    DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();

                    if (currNode != null)
                    {
                        currNode.EnablePlay(false);
                        currNode.EnablePause(true);
                        currNode.EnableStop(true);
                    }
                }));
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DeviceListChange()", ex.ToString());
            }
        }

        delegate void DeviceChange(int flag, int no, string deviceid);

        void DeviceListChange(int flag, int no, string deviceid)
        {
            try
            {
                if (flag == 0) // Add
                {
                    DeviceNode newNode = new DeviceNode();

                    int nCount = Program.mDeviceList.GetCount();
                    newNode.DeviceNo = nCount.ToString();
                    newNode.appControl = this;
                    newNode.deviceid = deviceid;

                    m_arrDevicePanel.Add(newNode);

                    AndroidDevice curdev = Program.mDeviceList.GetAt(Program.mDeviceList.GetCount() - 1);

                    newNode.AndroidDeviceState = ApkDataModel.APKLan("LoadingDeviceInformation");
                    newNode.SetMarqueeProgressBar();
                    int yPos = (nCount > 1) ? (m_arrDevicePanel[nCount - 2].Location.Y + m_arrDevicePanel[nCount - 2].Height + 2) : 0;
                    newNode.Location = new Point(0, yPos);

                    curdev.onBeginInstallApk += new AndroidDevice.BeginInstallHandler(BeginInstallApks);
                    curdev.onLogReport += new AndroidDevice.DeviceLogReportHandler(DeviceLogReport);
                    curdev.onRefreshDevice += new AndroidDevice.DeviceRefreshHandler(RefreshDeviceInfo);

                    listDevices.Controls.Add(newNode);
                }
                else if (flag == 1) // Delete
                {
                    listDevices.Controls.Remove(m_arrDevicePanel[no]);

//                     if (no == 0 && m_arrDevicePanel.Count() > 1)
//                     {
//                         m_arrDevicePanel[1].Location = m_arrDevicePanel[0].Location;
//                     }
                    m_arrDevicePanel.RemoveAt(no);
                    listDevices.VerticalScroll.Value = 0;
                    for (int i = 0; i < m_arrDevicePanel.Count(); i++)
                    {
                        DeviceNode nodeitem = m_arrDevicePanel.ElementAt(i);
                        nodeitem.DeviceNo = (i + 1).ToString();
                        nodeitem.Location = new Point(0, (i > 0) ? (m_arrDevicePanel[i - 1].Location.Y + m_arrDevicePanel[i - 1].Height + 2) : 0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DeviceListChange()", ex.ToString());
            }
        }

        void onDeviceChange(int flag, int no, string deviceid)
        {
            try
            {
                if (InvokeRequired)
                {
                    DeviceChange del = new DeviceChange(DeviceListChange);
                    Invoke(del, flag, no, deviceid);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "onDeviceChange()", ex.ToString());
            }
        }

        delegate void DeviceReport(String deviceid, String apkName, String report, int totalApk, int currApk);

        void ProcRealDeviceReport(String deviceid, String apkName, String report, int totalApk, int currApk)
        {
            try
            {
                DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();
                AndroidDevice currDev = Program.mDeviceList.FindDevByID(deviceid);

                if (currNode == null || currDev == null)
                    return;

                if (report.StartsWith("PushApk"))
                {
                    currNode.AndroidDeviceState = String.Format(ApkDataModel.APKLan("InstallingIOnNApk"), apkName, currApk, totalApk);
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("LoadingApkOnDevice"), apkName, currApk, totalApk),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);
                }
                else if (report.StartsWith("Push"))
                {
                        
                }
                else if (report.StartsWith("PkgInstall"))
                {
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("InstallingApkOnDevice"), currDev.installingApkGroup, apkName),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);
                }
                else if (report.StartsWith("DeleteTemp"))
                {
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = ApkDataModel.APKLan("DeleteTempOnDevice"),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);

                }
                else if (report.StartsWith("Failure"))
                {
                    //Comment by CSC: Do not show report in status bar, only show on details report.
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.FAILURE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        seperator = false
                    };
                    if (report.Contains("INSTALL_FAILED_ALREADY_EXISTS"))   //程序已经存在
                    {
                        logitem.logcontent = ApkDataModel.APKLan("AlreadyExistApk");
                    }
                    else if (report.Contains("INSTALL_FAILED_INVALID_APK")) //无效的APK
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_INVALID_APK");
                    }
                    else if (report.Contains("INSTALL_FAILED_INVALID_URI")) //无效的链接
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_INVALID_URI");
                    }
                    else if (report.Contains("INSTALL_FAILED_DUPLICATE_PACKAGE")) //已存在同名程序
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_DUPLICATE_PACKAGE");
                    }
                    else if (report.Contains("INSTALL_FAILED_NO_SHARED_USER")) //要求的共享用户不存在
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_NO_SHARED_USER");
                    }
                    else if (report.Contains("INSTALL_FAILED_UPDATE_INCOMPATIBLE")) //版本不能共存
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_UPDATE_INCOMPATIBLE");
                    }
                    else if (report.Contains("INSTALL_FAILED_SHARED_USER_INCOMPATIBLE")) //需求的共享用户签名错误
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_SHARED_USER_INCOMPATIBLE");
                    }
                    else if (report.Contains("INSTALL_FAILED_MISSING_SHARED_LIBRARY")) //需求的共享库已丢失
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_MISSING_SHARED_LIBRARY");
                    }
                    else if (report.Contains("INSTALL_FAILED_REPLACE_COULDNT_DELETE")) //需求的共享库无效
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_REPLACE_COULDNT_DELETE");
                    }
                    else if (report.Contains("INSTALL_FAILED_OLDER_SDK")) //系统版本过旧
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_OLDER_SDK");
                    }
                    else if (report.Contains("INSTALL_FAILED_CONFLICTING_PROVIDER")) //存在同名的内容提供者
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_CONFLICTING_PROVIDER");
                    }
                    else if (report.Contains("INSTALL_FAILED_INVALID_INSTALL_LOCATION")) //无效的安装路径
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_INVALID_INSTALL_LOCATION");
                    }
                    else if (report.Contains("INSTALL_FAILED_NEWER_SDK")) //系统版本过新
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_NEWER_SDK");
                    }
                    else if (report.Contains("INSTALL_FAILED_TEST_ONLY")) //调用者不被允许测试的测试程序
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_TEST_ONLY");
                    }
                    else if (report.Contains("INSTALL_FAILED_CPU_ABI_INCOMPATIBLE")) //包含的本机代码不兼容
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_CPU_ABI_INCOMPATIBLE");
                    }
                    else if (report.Contains("CPU_ABIINSTALL_FAILED_MISSING_FEATURE")) //使用了一个无效的特性
                    {
                        logitem.logcontent = ApkDataModel.APKLan("CPU_ABIINSTALL_FAILED_MISSING_FEATURE");
                    }
                    else if (report.Contains("INSTALL_FAILED_CONTAINER_ERROR")) //SD卡访问失败
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_CONTAINER_ERROR");
                    }
                    else if (report.Contains("INSTALL_FAILED_INTERNAL_ERROR")) //系统问题导致安装失败
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_INTERNAL_ERROR");
                    }
                    else if (report.Contains("INSTALL_FAILED_MEDIA_UNAVAILABLE"))
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_MEDIA_UNAVAILABLE");
                    }
                    else if (report.Contains("INSTALL_FAILED_INSUFFICIENT_STORAGE"))
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_INSUFFICIENT_STORAGE");
                    }
                    else if (report.Contains("INSTALL_FAILED_ACWF_INCOMPATIBLE"))
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_ACWF_INCOMPATIBLE");
                    }
                    else if (report.Contains("INSTALL_FAILED_DEXOPT"))
                    {
                        logitem.logcontent = ApkDataModel.APKLan("INSTALL_FAILED_DEXOPT");
                    }
                    else if (report.Contains("UNINSTALL_FAILED"))
                    {
                        logitem.logcontent = String.Format(ApkDataModel.APKLan("UninstallApkFail"), apkName);
                    }
                    else if (report.Contains("UNABLE_TO_LOAD_APK"))
                    {
                        logitem.logcontent = ApkDataModel.APKLan("UnableToLoadApk");
                    }
                    else
                    {
                        logitem.logcontent = report;
                    }

                    DeviceLogReport(deviceid, logitem);

                    if (currDev.op_state == DeviceOperationStates.Installing)
                    {
                        Callback_ServiceData callback = new Callback_ServiceData(this.Callback_NoneOper);

                        m_service.CallApkWorker(
                            ApkServiceUri.InsertApkInstallLog,
                            HttpMethod.POST,
                            new
                            {
                                teltype = currDev.vendor,
                                vendor = currDev.model,
                                apkid = currDev.apklist.ElementAt(currDev.currInstallApkNo).uid,
                                serial = currDev.androidInfo.uid,
                                imei = currDev.androidInfo.imei,
                                status = (int)ApkInstallStatus.FAIL,
                                note = logitem.logcontent,
                                clientver = Program.SOFT_VERNAME,
                                token = Program.AUTH_TOKEN
                            },
                            callback
                        );
                    }

                }
                else if (report.StartsWith("UpgradeApk"))
                {
                    String[] reportstrs = apkName.Split('#');
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("UpgradeApkOnDevice"), reportstrs.Count() > 0 ? reportstrs[0] : "") + 
                                    Environment.NewLine + "   ",
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);

                    Callback_ServiceData callback = new Callback_ServiceData(this.Callback_NoneOper);
                    m_service.CallApkWorker(
                        ApkServiceUri.InsertApkUpdateLog,
                        HttpMethod.POST,
                        new
                        {
                            teltype = currDev.vendor,
                            vendor = currDev.model,
                            apkid = currDev.apklist.ElementAt(currDev.currInstallApkNo).uid,
                            serial = currDev.androidInfo.uid,
                            imei = currDev.androidInfo.imei,
                            status = (int)ApkInstallStatus.SUCCESS,
                            note = reportstrs.Count() > 1 ? "旧版本：" + reportstrs[1] + ", 更新版本：" + reportstrs[2] : "",
                            clientver = Program.SOFT_VERNAME,
                            token = Program.AUTH_TOKEN
                        },
                        callback
                    );
                }
                else if (report.StartsWith("Finished"))
                {
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("InstalledApkOnDevice"), apkName) + Environment.NewLine + "   ",
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);

                    /* Comment by CSC:
                     * Call web service to report the apk installation status
                     */
                    Callback_ServiceData callback = new Callback_ServiceData(this.Callback_NoneOper);

                    m_service.CallApkWorker(
                        ApkServiceUri.InsertApkInstallLog,
                        HttpMethod.POST,
                        new
                        {
                            teltype = currDev.vendor,
                            vendor = currDev.model,
                            apkid = currDev.apklist.ElementAt(currDev.currInstallApkNo).uid,
                            serial = currDev.androidInfo.uid,
                            imei = currDev.androidInfo.imei,
                            status = (int)ApkInstallStatus.SUCCESS,
                            note = "版本：" + currDev.apklist.ElementAt(currDev.currInstallApkNo).vname,
                            clientver = Program.SOFT_VERNAME,
                            token = Program.AUTH_TOKEN
                        },
                        callback
                    );
                }
                else if (report.StartsWith("InstallCompleted"))
                {
                    currNode.EnablePlay(false);
                    currNode.EnablePause(false);
                    currNode.EnableStop(false);
                    currNode.AndroidDeviceState = ApkDataModel.APKLan("GroupInstallFinished");
                }
                else if (report.StartsWith("UninstallApkStart"))
                {
                    currNode.AndroidDeviceState = String.Format(ApkDataModel.APKLan("UninstallingIOnNApk"), apkName, currApk, totalApk);

                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("UninstallingApkOnDevice"), apkName),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);
                }
                else if (report.StartsWith("UninstallApkFinish"))
                {
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = String.Format(ApkDataModel.APKLan("UninstalledApkOnDevice"), apkName),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);

                    Callback_ServiceData callback = new Callback_ServiceData(this.Callback_NoneOper);

                    m_service.CallApkWorker(
                        ApkServiceUri.InsertApkUninstallLog,
                        HttpMethod.POST,
                        new
                        {
                            teltype = currDev.vendor,
                            vendor = currDev.model,
                            serial = currDev.androidInfo.uid,
                            imei = currDev.androidInfo.imei,
                            apkname = apkName,
                            status = (int)ApkInstallStatus.SUCCESS,
                            note = "",
                            clientver = Program.SOFT_VERNAME,
                            token = Program.AUTH_TOKEN
                        },
                        callback
                    );
                }
                else if (report.StartsWith("UninstallCompleted"))
                {
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = ApkDataModel.APKLan("FinishUninstallApkOnDevice"),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);
                }
                else if (report.StartsWith("CancelInstall"))
                {
                    //currNode.AndroidDeviceState = ApkDataModel.APKLan("StopInstall");
                }
                else if (report.StartsWith("PreparingInstall"))
                {
                    currNode.AndroidDeviceState = ApkDataModel.APKLan("PreparingInstall");

                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = ApkDataModel.APKLan("PreparingInstall"),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);

                }
                else if (report.StartsWith("TimeoutConnAgent"))
                {
                    currNode.SetStatusIcon(DeviceStatusIcon.FAIL);
                    currNode.AndroidDeviceState = ApkDataModel.APKLan("CannotConnectAgent");
                    currNode.SetDefaultProgressBar();
                }

                //m_arrDevicePanel[nDevNo].AndroidDeviceState = report;
                currNode.PercentOperationProgress = currDev.operation_percent;
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "ProcRealDeviceReport()", ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        void onDeviceDetected(int devCnt)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (devCnt == 0)
                    {
                        panelManual.Visible = true;
                    }
                    else
                    {
                        panelManual.Visible = false;
                    }
                }));
            }
        }

        void onDeviceLoadFail(String deviceid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();

                    if (currNode == null)
                        return;

                    currNode.SetDefaultProgressBar();

                    currNode.AndroidDeviceState = ApkDataModel.APKLan("FailToConnect");
                    currNode.SetStatusIcon(DeviceStatusIcon.FAIL);
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "",
                        logcontent = ApkDataModel.APKLan("FailToConnectDetail"),
                        seperator = false
                    };

                    DeviceLogReport(deviceid, logitem);
                }));
            }
        }

        public void ShowDeviceSpace(String deviceid)
        {
            DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();

            if (currNode != null)
            {
                currNode.ShowDeviceSpace(deviceid);
            }
        }

        void RefreshDeviceInfo(String deviceid)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    ShowDeviceSpace(deviceid);
                }));
            }
            else
            {
                ShowDeviceSpace(deviceid);
            }
        }

        void onDeviceLoad(String deviceid, ApkResponseData res)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    AndroidDevice curdev = Program.mDeviceList.FindDevByID(deviceid);
                    DeviceNode currNode = m_arrDevicePanel.Where(m => m.deviceid == deviceid).FirstOrDefault();

                    if (curdev == null || currNode == null)
                        return;

                    currNode.DeviceVendor = curdev.vendor;
                    currNode.DeviceModel = curdev.model;
                    currNode.InstallLocation = curdev.install_location;
                    currNode.PercentOperationProgress = curdev.operation_percent;

                    if (curdev.totalSizeTel > 0)
                    {
                        currNode.FreeSpaceTel = curdev.freeSpaceTel;
                        currNode.PercentUsedTel = 1.0f - curdev.freeSpaceTel * 1.0f / curdev.totalSizeTel;
                    }
                    if (curdev.totalSizeSDCard > 0)
                    {
                        currNode.FreeSpaceSDCard = curdev.freeSpaceSDCard;
                        currNode.PercentUsedSDCard = 1.0f - curdev.freeSpaceSDCard * 1.0f / curdev.totalSizeSDCard;
                    }
                    currNode.SetDefaultProgressBar();

                    curdev.devStatus = DeviceState.Connected;
                    if (res.SVCC_RET == SERVICEERROR.ERR_SUCCESS)
                    {
                        currNode.AndroidDeviceState = ApkDataModel.APKLan("Connected");
                        currNode.SetStatusIcon(DeviceStatusIcon.SUCCESS);

                        Thread reportAppsThread = new Thread(new ThreadStart(curdev.ReportDeviceInstalledApp));
                        reportAppsThread.Start();
                    }
                    else if (res.SVCC_RET == SERVICEERROR.ERR_ALREADY_EXISTS_TELAPPS)
                    {
                        currNode.AndroidDeviceState = ApkDataModel.APKLan("AlreadyConnected");
                        currNode.SetStatusIcon(DeviceStatusIcon.WARNING);
                    }
                    DeviceLog logitem = new DeviceLog
                    {
                        logtype = LogType.NOTICE,
                        logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                        logtitle = "IMEI",
                        logcontent = curdev.androidInfo.imei,
                        seperator = false
                    };
                    DeviceLogReport(deviceid, logitem);

                    if (res.SVCC_RET == SERVICEERROR.ERR_ALREADY_EXISTS_TELAPPS)
                    {
                        logitem = new DeviceLog
                        {
                            logtype = LogType.WARNING,
                            logtime = String.Format("{0:HH:mm:ss}", DateTime.Now),
                            logtitle = "",
                            logcontent = ApkDataModel.APKLan("AlreadyConnectedNotAvailable"),
                            seperator = false
                        };

                        DeviceLogReport(deviceid, logitem);

                    }
                }));
            }
        }

        void onDeviceReport(String deviceid, String apkName, String report, int totalApk, int currApk)
        {
            try
            {
                if (InvokeRequired)
                {
                    DeviceReport del = new DeviceReport(ProcRealDeviceReport);
                    Invoke(del, deviceid, apkName, report, totalApk, currApk);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "onDeviceReport()", ex.ToString());
            }
        }

        private void Buttons_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            MessageBox.Show(btn.Name);
        }

        private void AppControl_Load(object sender, EventArgs e)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            this.panelManual.Left = this.listDevices.Left;
            this.panelManual.Top = this.listDevices.Top;
            this.panelManual.BringToFront();
            this.Shown += new EventHandler(InitializeApkGroup);
        }
        #endregion

        #region Apk Events
        private void InitializeApkGroup(object sender, EventArgs e)
        {
            Application.DoEvents();

            CallLoadApkGroup();
        }

        public void CallLoadApkGroup()
        {
            Callback_ServiceData callback = new Callback_ServiceData(this.LoadApkGroup);
            Dictionary<string, string> apkparam = new Dictionary<string, string>();
            apkparam.Add("token", Program.AUTH_TOKEN);

            m_service.CallApkWorker(
                ApkServiceUri.GetApkGroupList,
                HttpMethod.GET,
                apkparam,
                callback
            );
        }

        public void tabControl_App_ApkGroupTab(object sender, EventArgs e)
        {
            //Application.DoEvents();

            m_currGroupIndex = this.tabControl_App.SelectedIndex;

            try
            {
//                m_GroupLoading = false;
                for (int i = 0; i < m_arrImgDownClient.Count(); i++)
                {
                    if (m_arrImgDownClient[i] != null/* && m_arrImgDownClient[i].IsBusy*/)
                    {
                        m_arrImgDownClient[i].CancelAsync();
                        m_arrImgDownClient[i].Dispose();
                        //m_arrImgDownClient[i].Dispose();
                        //m_arrImgDownClient[i] = null; 
                    }
                }

                for (int i = 0; i < m_arrApkDownMan.Count(); i++)
                {
                    if (m_arrApkDownMan[i] != null/* && m_arrApkDownMan[i].m_downWorker.IsBusy*/)
                    {
                        m_arrApkDownMan[i].m_downWorker.CancelAsync();
                        m_arrApkDownMan[i].m_downWorker.Dispose();
                        m_arrApkDownMan[i] = null;
                    }
                }

                if (m_sqlMan.m_sqliteWorker != null/* && m_sqlMan.m_sqliteWorker.IsBusy*/)
                {
                    m_sqlMan.m_sqliteWorker.CancelAsync();
                    m_sqlMan.m_sqliteWorker.Dispose();
                }

                if (m_service.m_srvWorker != null && m_service.m_srvWorker.IsBusy)
                {
                    m_service.m_srvWorker.CancelAsync();
                    m_service.m_srvWorker.Dispose();
                }


                m_groupTabs[m_currGroupIndex].Controls.Clear();

                for (int i = 0; i < m_arrPicNew.Count(); i++)
                {
                    if (m_arrPicNew[i] != null)
                    {
                        m_arrPicNew[i].Dispose();
                    }
                }
                for (int i = 0; i < m_arrPercent.Count(); i++)
                {
                    if (m_arrPercent[i] != null)
                    {
                        m_arrPercent[i].Dispose();
                    }
                }
                for (int i = 0; i < m_arrLblData.Count(); i++)
                {
                    if (m_arrLblData[i] != null)
                    {
                        m_arrLblData[i].Dispose();
                    }
                }

                showLoadingPanel(m_currGroupIndex);

                if (m_waitLoadApkThread != null)
                {
                    m_bwaitThreadCancel = true;
                    m_waitLoadApkThread.Join();
                }

                m_bwaitThreadCancel = false;
                m_waitLoadApkThread = new Thread(WaitForLoadApk);
                //m_waitLoadApkThread.IsBackground = true;
                m_waitLoadApkThread.Start();
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "tabControl_App_ApkGroupTab()", ex.ToString());
            }
        }

        private void WaitForLoadApk()
        {
            try
            {
                //GC.Collect();
                while (!CheckApkWorkerStatus())
                {
                    if (m_bwaitThreadCancel)
                    {
                        return;
                    }
                    Thread.Sleep(200);
                }

                BeginInvoke(new Action(() =>
                {
                    InitApkList();
                }));
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "WaitForLoadApk()", ex.ToString());
            }
        }

        private void InitApkList()
        {
            Callback_ServiceData callback = new Callback_ServiceData(this.LoadApkList);
            Dictionary<string, string> apkparam = new Dictionary<string, string>();
            apkparam.Add("groupid", m_groupList[m_currGroupIndex].Id.ToString());

//            m_GroupLoading = true;

            m_service.CallApkWorker(
                ApkServiceUri.GetApkList,
                HttpMethod.GET,
                apkparam,
                callback
            );
        }

        public void showLoadingPanel(int tabIndex)
        {

            C1DockingTabPage currTab = m_groupTabs[tabIndex];
            m_loadingPanel = new Panel();
            m_loadingPanel.AutoScroll = true;
            m_loadingPanel.Location = new Point(0, 0);
            m_loadingPanel.Size = new Size(currTab.Width, currTab.Height);

            PictureBox lPic = new PictureBox();
            lPic.Image = Properties.Resources.ajax_loader;
            lPic.Location = new Point((currTab.Width) / 2 - 15, (currTab.Height - lPic.Height) / 2);

            m_loadingPanel.Controls.Add(lPic);

            currTab.Controls.Add(m_loadingPanel);
            //m_groupTabs[id].Invalidate();
        }

        public void hideLoadingPanel(int tabIndex)
        {
            C1DockingTabPage currTab = m_groupTabs[tabIndex];
            currTab.Controls.Remove(m_loadingPanel);
        }
        #endregion

        #region APK Load/Download Progress
        public void LoadApkGroup(ApkResponseData res)
        {
            if (res != null)
            {

                try
                {
                    m_groupList = JsonConvert.DeserializeObject<List<ApkGroupInfo>>(res.SVCC_DATA.ToString());
                    m_groupTabs = new C1DockingTabPage[m_groupList.Count()];

                    int i = 0;

                    foreach (ApkGroupInfo item in m_groupList)
                    {
                        C1DockingTabPage newtab = new C1DockingTabPage();
                        //newtab.CaptionText = "Test Tab";
                        //newtab.TextAlign = ContentAlignment.MiddleCenter;
                        newtab.Text = "  " + item.Name;
                        //newtab.Click += new EventHandler(OnClick_TabPage);

                        m_groupTabs[i] = newtab;
                        this.tabControl_App.Controls.Add(newtab);

                        Thread threadGroupImgLoad = new Thread(new ParameterizedThreadStart(DownloadGroupImage));
                        threadGroupImgLoad.Start(new ApkGroupParam
                        {
                            pitem = newtab,
                            item = item,
                            data = res
                        });

                        i++;
                    }

                    this.tabControl_App.SelectedIndexChanged += new EventHandler(tabControl_App_ApkGroupTab);
                    showLoadingPanel(0);
                    InitApkList();
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("AppControl", "LoadApkGroup()", ex.ToString());
                }
            }
        }

        private void LoadApkList(ApkResponseData res)
        {
            try
            {
                if (res != null)
                {
//                    if (m_GroupLoading)
                    {
                        Callback_UpdateApkList downCallback = new Callback_UpdateApkList(this.CompleteLoadApkList);
                        m_sqlMan.CallUpdateApkListWorker(m_groupList[m_currGroupIndex].Id, res, downCallback);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "LoadApkList()", ex.ToString());
            }
        }

        public void CompleteLoadApkList(bool res, ApkResponseData response)
        {
            int i = 0;
            C1DockingTabPage currTab = m_groupTabs[m_currGroupIndex];
            Panel mypanel = new Panel();

//            if (!m_GroupLoading) return;

            List<ApkFileInfo> apks = JsonConvert.DeserializeObject<List<ApkFileInfo>>(response.SVCC_DATA.ToString());
            m_apkCount = apks.Count();
            m_arrPicNew = new ApkPictureBox[m_apkCount];
            m_arrLblData = new Label[m_apkCount];
            m_arrPercent = new ProgressBar[m_apkCount];

            m_arrApkDownMan = new ApkDownManager[m_apkCount];

            m_arrImgDownClient = new WebClient[m_apkCount];

            mypanel.AutoScroll = true;
            mypanel.Location = new System.Drawing.Point(0, 5);
            mypanel.Size = new Size(currTab.Width, currTab.Height - 15);

            currTab.Controls.Add(mypanel);

            /* ----------------------- */

//            if (!m_GroupLoading) return;

            foreach (DBApkFileInfo item in m_sqlMan.m_apkList)
            {
//                if (!m_GroupLoading) return;
                Point posData = new Point(25 + (i % 6) * 80, 25 + (i / 6) * 90);

                m_arrPicNew[i] = new ApkPictureBox(item.name, item.version, item.filesize);
                m_arrPicNew[i].Location = posData;
                m_arrPicNew[i].BackColor = Color.Transparent;
                m_arrPicNew[i].Size = new Size(48, 48);

                m_arrLblData[i] = new Label();
                m_arrLblData[i].Location = new Point(posData.X - 15, posData.Y + 55);
                m_arrLblData[i].BackColor = Color.Transparent;
                m_arrLblData[i].TextAlign = ContentAlignment.MiddleCenter;
                m_arrLblData[i].Text = ApkDataModel.HanCut(item.name, 5);
                m_arrLblData[i].Size = new System.Drawing.Size(80, 16);

                mypanel.Controls.Add(m_arrPicNew[i]);
                mypanel.Controls.Add(m_arrLblData[i]);

                if (item.status.HasFlag(ApkDownStatus.COMPLETED_IMAGE))
                {
                    //FileStream imgstream = new FileStream(item.imgpath, FileMode.Open, FileAccess.Read);
                    //m_arrPicNew[i].Image = Image.FromStream(imgstream);
                    //imgstream.Close();
                    if (File.Exists(item.imgpath))
                    {
                        FileStream fs = new FileStream(item.imgpath, FileMode.Open, FileAccess.Read);
                        Image apkimg = Image.FromStream(fs);
                        m_arrPicNew[i].Image = apkimg;
                        fs.Dispose();
                    }
                }
                else
                {
//                    if (!m_GroupLoading) return;

                    item.status |= ApkDownStatus.LOADING_IMAGE;
                    DownloadApkImage(new ApkDownParam
                    {
                        pIndex = i,
                        data = response
                    });
                }

                if (!item.status.HasFlag(ApkDownStatus.COMPLETED_FILE))
                {
                    m_arrPercent[i] = new ProgressBar();
                    m_arrPercent[i].Location = new Point(posData.X, posData.Y + 70);
                    m_arrPercent[i].Refresh();
                    m_arrPercent[i].Value = 0;
                    m_arrPercent[i].Size = new System.Drawing.Size(50, 10);
                    mypanel.Controls.Add(m_arrPercent[i]);
                }
                i++;
            }

            if (!Directory.Exists(Program.LOCAL_APP_PATH + "\\" + Program.APP_CACHE_PATH))
            {
                Directory.CreateDirectory(Program.LOCAL_APP_PATH + "\\" + Program.APP_CACHE_PATH);
            }

            i = 0;
            foreach (DBApkFileInfo item in m_sqlMan.m_apkList)
            {
                if (!item.status.HasFlag(ApkDownStatus.COMPLETED_FILE))
                {
//                    if (!m_GroupLoading) return;

                    item.status |= ApkDownStatus.LOADING_FILE;
                    DownloadApkFile(new ApkDownParam
                    {
                        pIndex = i,
                        data = response
                    });
                }

                i++;
            }

            hideLoadingPanel(m_currGroupIndex);
        }

        public void DownloadGroupImage(object args)
        {

            ApkGroupParam aparam = (ApkGroupParam)args;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)HttpWebRequest.Create(aparam.data.SVCC_BASEURL + aparam.item.ImgPath).GetResponse();
                BeginInvoke(new Action(() =>
                {
                    using (Stream grpstream = resp.GetResponseStream())
                    {
                        try
                        {
                            ((C1DockingTabPage)(aparam.pitem)).Image = Image.FromStream(grpstream);
                        }
                        catch (System.Exception ex)
                        {
                        	
                        }
                    }
                }));
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DownloadGroupImage()", ex.ToString());
            }
        }

#if false
//         public void DownloadApkImage(object args)
//         {
// 
//             ApkParam aparam = (ApkParam)args;
// 
//             try
//             {
//                 HttpWebResponse resp = (HttpWebResponse)HttpWebRequest.Create(aparam.data.SVCC_BASEURL + aparam.item.ImgPath).GetResponse();
//                 ((ApkPictureBox)(aparam.pitem)).Image = Image.FromStream(resp.GetResponseStream());
//             }
//             catch (System.Exception ex)
//             {
//                 MessageBox.Show(ex.ToString(), "123");
//             }
//         }
#endif

        public void DownloadApkImage(object args)
        {
            string targetPath = Program.LOCAL_APP_PATH + "\\" + Program.IMG_CACHE_PATH;
            ApkDownParam aparam = (ApkDownParam)args;
            int currIndex = aparam.pIndex;

            List<ApkFileInfo> apks = JsonConvert.DeserializeObject<List<ApkFileInfo>>(aparam.data.SVCC_DATA.ToString());
            DBApkFileInfo lfl = m_sqlMan.m_apkList.Where(m => m.uid == apks.ElementAt(currIndex).Id).FirstOrDefault();

            try
            {
                string[] paths = apks.ElementAt(currIndex).ImgPath.Split('/');
                m_arrImgDownClient[currIndex] = new WebClient();

                m_arrImgDownClient[currIndex].DownloadProgressChanged += (s, e) =>
                {
                    //m_arrPercent[currIndex].Value = e.ProgressPercentage;
                };
                m_arrImgDownClient[currIndex].DownloadFileCompleted += (s, e) =>
                {
                    //FileStream imgstream = new FileStream(lfl.imgpath, FileMode.Open, FileAccess.Read);
                    //m_arrPicNew[currIndex].Image = Image.FromStream(imgstream);
                    if (File.Exists(lfl.imgpath))
                    {
                        try
                        {
                            FileStream fs = new FileStream(lfl.imgpath, FileMode.Open, FileAccess.Read);
                            Image apkimg = Image.FromStream(fs);
                            m_arrPicNew[currIndex].Image = apkimg;
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
                };

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                m_arrImgDownClient[currIndex].DownloadFileAsync(new Uri(aparam.data.SVCC_BASEURL + apks.ElementAt(currIndex).ImgPath),
                    targetPath + paths[paths.Count() - 1]);

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DownloadApkImage()", ex.ToString());
            }
        }

        public void DownloadApkFile(object args)
        {
            string targetPath = Program.LOCAL_APP_PATH + "\\" + Program.APP_CACHE_PATH;
            ApkDownParam aparam = (ApkDownParam)args;
            int currIndex = aparam.pIndex;

            try
            {
                List<ApkFileInfo> apks = JsonConvert.DeserializeObject<List<ApkFileInfo>>(aparam.data.SVCC_DATA.ToString());
                DBApkFileInfo lfl = m_sqlMan.m_apkList.Where(m => m.uid == apks.ElementAt(currIndex).Id).FirstOrDefault();
                string[] paths = apks.ElementAt(currIndex).FilePath.Split('/');

                m_arrApkDownMan[currIndex] = new ApkDownManager();
                DownloadApkFileParam downApkParam = new DownloadApkFileParam();
                downApkParam.currIndex = currIndex;
                downApkParam.startPoint = 0;
                downApkParam.srcFilePath = aparam.data.SVCC_BASEURL + apks.ElementAt(currIndex).FilePath;
                downApkParam.targetFilePath = targetPath + paths[paths.Count() - 1];
                downApkParam.appControl = this;

                if (File.Exists(downApkParam.targetFilePath))
                {
                    FileInfo finfo = new FileInfo(downApkParam.targetFilePath);
                    if (finfo.Length < apks.ElementAt(currIndex).FileSize)
                    {
                        downApkParam.startPoint = new FileInfo(downApkParam.targetFilePath).Length;
                    }
                    else
                    {
                        downApkParam.startPoint = 0;
                    }
                }
                else
                {
                    downApkParam.startPoint = 0;
                }

                m_arrApkDownMan[currIndex].CallApkDownWorker(downApkParam);

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DownloadApkFile()", ex.ToString());
            }
        }

        public bool CheckApkWorkerStatus()
        {
            bool rst = true;

            if (m_sqlMan.m_apkList != null)
            {
                int aaa = m_sqlMan.m_apkList.Where(m => m.status.HasFlag(ApkDownStatus.COMPLETED_FILE)).Count();
                if (m_sqlMan.m_apkList.Where(m => m.status.HasFlag(ApkDownStatus.COMPLETED_FILE)).Count() != m_sqlMan.m_apkList.Count())
                {
                    rst &= false;
                }

                if (m_sqlMan.m_apkList.Where(m => m.status.HasFlag(ApkDownStatus.COMPLETED_IMAGE)).Count() != m_sqlMan.m_apkList.Count())
                {
                    rst &= false;
                }
            }

            if (m_service.isDone == ApkWorkerStatus.BUSY)
            {
                rst &= false;
            }

            if (m_sqlMan.isDone == ApkWorkerStatus.BUSY)
            {
                rst &= false;
            }

            return rst;
        }

        public void SetApkInformation(String targetpath, ApkAdbParam apkinfo)
        {
            DBApkFileInfo apkitem = m_sqlMan.m_apkList.Where(m => m.filepath == targetpath).FirstOrDefault();

            if (apkitem != null)
            {
                apkitem.vcode = apkinfo.vcode;
                apkitem.vname = apkinfo.vname;
                apkitem.package = apkinfo.package;
            }
        }

        #endregion

        #region Methods
        public void SwitchLanguage()
        {
            btnAllCheck.Text = ApkDataModel.APKLan("OneClickInstall");
            chkAutoUninstall.Text = ApkDataModel.APKLan("AutoRemove");

            for (int i = 0; i < m_arrDevicePanel.Count(); i++)
            {
                DeviceNode nodeitem = m_arrDevicePanel.ElementAt(i);
                nodeitem.SwitchLanguage();
            }
        }

        public void SetApkDownStatus(int nIndex, ApkDownStatus status)
        {
            if (m_sqlMan.m_apkList != null && m_sqlMan.m_apkList.Count() > 0)
            {
                if (m_sqlMan.m_apkList.ElementAt(nIndex) != null)
                {
                    m_sqlMan.m_apkList.ElementAt(nIndex).status |= status;
                }
            }
        }

        public void Callback_NoneOper(ApkResponseData res)
        {

        }

        #endregion
        private void chkAutoUninstall_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void AppControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_currGroupIndex = this.tabControl_App.SelectedIndex;

            try
            {
                for (int i = 0; i < m_arrImgDownClient.Count(); i++)
                {
                    if (m_arrImgDownClient[i] != null && m_arrImgDownClient[i].IsBusy)
                    {
                        m_arrImgDownClient[i].CancelAsync();
                        m_arrImgDownClient[i].Dispose();
                    }
                }

                for (int i = 0; i < m_arrApkDownMan.Count(); i++)
                {
                    if (m_arrApkDownMan[i] != null && m_arrApkDownMan[i].m_downWorker.IsBusy)
                    {
                        m_arrApkDownMan[i].m_downWorker.CancelAsync();
                        m_arrApkDownMan[i].m_downWorker.Dispose();
                        m_arrApkDownMan[i] = null;
                    }
                }

                if (m_sqlMan.m_sqliteWorker != null && m_sqlMan.m_sqliteWorker.IsBusy)
                {
                    m_sqlMan.m_sqliteWorker.CancelAsync();
                    m_sqlMan.m_sqliteWorker.Dispose();
                }

                if (m_service.m_srvWorker != null && m_service.m_srvWorker.IsBusy)
                {
                    m_service.m_srvWorker.CancelAsync();
                    m_service.m_srvWorker.Dispose();
                }

//                m_GroupLoading = false;

                if (m_waitLoadApkThread != null)
                {
                    m_bwaitThreadCancel = true;
                    m_waitLoadApkThread.Join();
                }

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "tabControl_App_ApkGroupTab()", ex.ToString());
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
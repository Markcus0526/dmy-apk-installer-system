using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ApkInstaller.ServiceCorrespond;
using System.Threading;
using System.Globalization;

namespace ApkInstaller
{
    public delegate void Callback_DevInstallCompleted(String deviceid);

    public enum DevicePlayStatus
    {
        PLAY,
        PAUSE,
        STOP
    }

    public enum ApkInstallStatus
    {
        SUCCESS,
        FAIL
    }

    class AndroidDeviceWorker
    {
        public BackgroundWorker m_installWorker;
        private ApkServiceCall m_service = new ApkServiceCall();
        public DevicePlayStatus m_playStatus;
        public AndroidDevice m_device;

        public delegate void DeviceReportHandler(String sendername, String report);
        public event DeviceReportHandler onDeviceReport;

        public class AndroidDevParam
        {
            public bool autoUninstall { get; set; }
            public int installLocation { get; set; }
            public AndroidDevice device { get; set; }
            public Callback_DevInstallCompleted devCallback { get; set; }
        }

        public AndroidDeviceWorker()
        {
            m_playStatus = DevicePlayStatus.PLAY;
            onDeviceReport += new DeviceReportHandler(Program.mDevicePoll.DeviceReported);
        }

        public void CallInstallWorker(bool bAutoUninstall, int installLocation, AndroidDevice device, Callback_DevInstallCompleted callback)
        {
            m_installWorker = new BackgroundWorker();
            m_installWorker.WorkerReportsProgress = true;
            m_installWorker.WorkerSupportsCancellation = true;

            m_device = device;
            m_installWorker.DoWork += new DoWorkEventHandler(ApkInstallWorker_DoWork);
            m_installWorker.ProgressChanged += new ProgressChangedEventHandler(ApkInstallWorker_ProgressChanged);
            m_installWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ApkInstallWorker_RunWorkerCompleted);

            m_installWorker.RunWorkerAsync(new AndroidDevParam
            {
                autoUninstall = bAutoUninstall,
                installLocation = installLocation,
                device = device,
                devCallback = callback
            });
        }

        private void ApkInstallWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AndroidDevParam sparam = (AndroidDevParam)e.Argument;
            e.Result = Tuple.Create<String, Callback_DevInstallCompleted>(sparam.device.deviceid, sparam.devCallback);

            if (Program.LANGUAGEID == (int)ApkDataModel.ApkLanguage.CHINESE)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }

            try
            {
                onDeviceReport(sparam.device.deviceid, "PreparingInstall");

                sparam.device.CallBeginInstall(sparam.device.deviceid);
                if (sparam.autoUninstall)
                {
                    sparam.device.GetInstalledApkInfos();
                    for (int i = 0; i < sparam.device.installedApks.Count(); i++)
                    {

                        if (m_playStatus == DevicePlayStatus.PLAY)
                        {

                            sparam.device.currUnInstallApkNo = i;
                            sparam.device.uninstallingApkName = sparam.device.installedApks.ElementAt(i).appName;
                            sparam.device.UninstallApk(i, sparam.device.uninstallingApkName, true);

                            sparam.device.GetSpace();
                            sparam.device.RefreshDeviceInfo();
                        }

                        while (m_playStatus == DevicePlayStatus.PAUSE)
                        {
                            Thread.Sleep(200);
                        }

                        if (m_installWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (m_playStatus == DevicePlayStatus.STOP)
                        {
                            onDeviceReport(sparam.device.deviceid, "CancelInstall");
                            break;
                        }
                    }

                    if (m_playStatus.HasFlag(DevicePlayStatus.PLAY))
                    {
                        onDeviceReport(sparam.device.deviceid, "UninstallCompleted");
                    }
                }

                if (m_playStatus.HasFlag(DevicePlayStatus.PLAY))
                {
                    for (int i = 0; i < sparam.device.apklist.Count(); i++)
                    {
                        if (m_installWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (m_playStatus == DevicePlayStatus.PLAY)
                        {
                            sparam.device.operation_percent = 0;
                            sparam.device.op_state = DeviceOperationStates.Installing;
                            sparam.device.installingApkName = sparam.device.apklist.ElementAt(i).name;
                            sparam.device.currInstallApkNo = i;

                            ApkAdbParam apkinfo = sparam.device.apklist.ElementAt(i);

                            int ret = AdbWrapper.InstallApk(
                                sparam.device.deviceid,
                                Encoding.UTF8.GetBytes(sparam.device.installingApkName),
                                apkinfo.vcode,
                                apkinfo.vname,
                                apkinfo.package,
                                sparam.installLocation,
                                apkinfo.filepath);

                            if (m_installWorker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            //m_installWorker.ReportProgress(i);

                            sparam.device.GetSpace();
                            sparam.device.RefreshDeviceInfo();

                            while (m_playStatus == DevicePlayStatus.PAUSE)
                            {
                                Thread.Sleep(200);
                            }

                            if (m_installWorker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            if (m_playStatus == DevicePlayStatus.STOP)
                            {
                                onDeviceReport(sparam.device.deviceid, "CancelInstall");
                                break;
                            }
                        }
                    }

                    if (m_playStatus == DevicePlayStatus.PLAY)
                    {
                        onDeviceReport(sparam.device.deviceid, "InstallCompleted");
                        if (Program.chkProgramTask)
                            sparam.device.SendInstallCompletedEvent();

                        sparam.device.CallLogReport(
                            sparam.device.deviceid,
                            LogType.SUCCESS,
                            String.Format("{0:HH:mm:ss}", DateTime.Now),
                            "",
                            ApkDataModel.APKLan("FinishInstallApkOnDevice"),
                            true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                sparam.device.CallLogReport(
                    sparam.device.deviceid,
                    LogType.ERROR,
                    String.Format("{0:HH:mm:ss}", DateTime.Now),
                    "",
                    ApkDataModel.APKLan("ExceptionInstallApkOnDevice"),
                    false );

                ApkDataModel.WriteLogFile("AndroidDeviceWorker", "ApkInstallWorker_DoWork()", ex.ToString());
            }
        }

        private void ApkInstallWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //(int)Math.Round((100 / (decimal)sparam.apkpaths.Count) * i)
            //string.Format("Progress : {0} %", e.ProgressPercentage);
        }

        private void ApkInstallWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Program.mDeviceList.OnCompleted_DeviceApkInstall(m_device.deviceid);
            }
            else if (e.Error != null)
            {
//                throw e.Error;
                Program.mDeviceList.OnCompleted_DeviceApkInstall(m_device.deviceid);
            }
            else
            {
                Tuple<String, Callback_DevInstallCompleted> res = e.Result as Tuple<String, Callback_DevInstallCompleted>;
                res.Item2(res.Item1);
            }

        }

        public void PlayInstall()
        {
            m_playStatus = DevicePlayStatus.PLAY;
        }

        public void StopInstall()
        {
            m_playStatus = DevicePlayStatus.STOP;
        }

        public void PauseInstall()
        {
            m_playStatus = DevicePlayStatus.PAUSE;
        }

        public DevicePlayStatus GetPlayStatus()
        {
            return m_playStatus;
        }

        public void Callback_NoneOper(ApkResponseData res)
        {

        }
    }
}

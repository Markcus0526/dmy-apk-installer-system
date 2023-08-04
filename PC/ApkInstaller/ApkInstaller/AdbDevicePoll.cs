using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ApkInstaller.ServiceCorrespond;
using System.Windows.Forms;
using System.Diagnostics;

namespace ApkInstaller
{
    class AdbDevicePoll
    {
        public delegate void DeviceChangeHandler(int flag, int no, string deviceid);
        public event DeviceChangeHandler onDeviceChange;

        public delegate void DeviceReportHandler(String deviceid, String apkName, String report, int totalApk, int currApk);
        public event DeviceReportHandler onDeviceReport;

        public delegate void DeviceInitializeHandler(String deviceid, ApkResponseData res);
        public event DeviceInitializeHandler onDeviceLoad;

        public delegate void DeviceInitFailHandler(String deviceid);
        public event DeviceInitFailHandler onDeviceLoadFail;

        public delegate void DeviceDetectedHandler(int devCnt);
        public event DeviceDetectedHandler onDeviceDetected;

        private volatile bool _shouldStop = false;
        List<String> devicelists = new List<String>();
        public ADBEventPipeServer pipeserver = new ADBEventPipeServer();
        public ADBEventTcpServer wrapperserver = new ADBEventTcpServer();

        public void DevicePoll()
        {
            int devcountCur = 0;
            List<String> cacheAdbList = new List<String>();

            try
            {
                ApkAdbParam agentinfo = ApkParser.GetApkInformation(Program.APKAGENT_FILEPATH);

                if (!String.IsNullOrEmpty(agentinfo.package))
                {
                    Program.APKAGENT_PACKAGENAME = agentinfo.package;
                    Program.APKAGENT_VERSIONCODE = int.Parse(agentinfo.vcode);
                }
            }
            catch (System.Exception ex)
            {
            	
            }

            try
            {
                while (!_shouldStop)
                {
                    String devices = AdbWrapper.GetDeviceLists();
                    List<String> devlistcur = new List<String>();
                    int nNewLine;
                    bool changed = false;
                    if (devices == null)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    while ((nNewLine = devices.IndexOf("\n", 0)) > 0)
                    {
                        String deviceID = devices.Substring(0, nNewLine);
                        devcountCur++;

                        int nTabPos = deviceID.IndexOf('\t', 0);
                        string[] splitdevinfo = deviceID.Split('\t');
                        if (splitdevinfo.Count() > 1 && splitdevinfo[1] != "offline")
                        {
                            deviceID = splitdevinfo[0];
                            devlistcur.Add(deviceID);
                        }
                        devices = devices.Substring(nNewLine + 1);
                    }

                    onDeviceDetected(devlistcur.Count());

                    for (int i = devicelists.Count - 1; i >= 0; i--)
                    {
                        bool found = false;
                        for (int j = 0; j < devlistcur.Count; j++)
                        {
                            if (devicelists[i].Equals(devlistcur[j]))
                            {
                                int orgSameCount = cacheAdbList.Where(m => m == devicelists[i]).Count();
                                int currSameCount = devlistcur.Where(m => m == devicelists[i]).Count();

                                // || Program.mDeviceList.GetByDeviceID(devicelists[i]).devStatus == DeviceState.Connected
                                if (currSameCount >= orgSameCount)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (!found)
                        {
                            // Delete device
                            Program.mDeviceList.DeleteDevice(i);
                            onDeviceChange(1, i, devicelists[i]);
                            changed = true;
                        }
                    }

                    for (int j = 0; j < devlistcur.Count; j++)
                    {
                        bool found = false;
                        for (int i = 0; i < devicelists.Count; i++)
                        {
                            if (devicelists[i].Equals(devlistcur[j]))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            // Add device
                            if (!Program.mDeviceList.GetDeviceIDList().Contains(devlistcur[j]))
                            {
                                Program.mDeviceList.AddDevice(devlistcur[j]);

                                onDeviceChange(0, 0, devlistcur[j]);

                                AndroidDevice dev = Program.mDeviceList.FindDevByID(devlistcur[j]);

                                dev.initDeviceThread = new Thread(new ParameterizedThreadStart(dev.RetrieveDeviceInfo));
                                dev.initDeviceThread.Start(dev.deviceid);

                                changed = true;
                            }
                        }
                    }

                    if (changed)
                    {
                        devicelists.Clear();
                        for (int i = 0; i < Program.mDeviceList.GetCount(); i++)
                        {
                            AndroidDevice dev = Program.mDeviceList.GetAt(i);
                            devicelists.Add(dev.deviceid);
                        }
                    }

                    cacheAdbList.Clear();
                    foreach (String devitem in devlistcur)
                    {
                        cacheAdbList.Add(devitem);
                    }

                    Thread.Sleep(1000);
                }

                //Add by CSC: To close all actions of devices when close application.
                for (int i = devicelists.Count - 1; i >= 0; i--)
                {
                    Program.mDeviceList.DeleteDevice(i);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AdbDevicePoll", "DevicePoll()", ex.ToString());
            }

            wrapperserver.RequestStop();
        }

        public void InitPipeServer()
        {
            pipeserver.InitServer("APKPLInstaller");
            pipeserver.Listen();
        }

        public void InitADBTcpServer()
        {
            wrapperserver.InitServer();
            wrapperserver.Listen();
        }

        public void DeviceReported(String sender, String report)
        {
            AndroidDevice currdev = Program.mDeviceList.FindDevByID(sender);
            if (currdev != null)
            {
                String[] strResult = currdev.ReportState(report);

                if (report.Contains("APKAgent.apk"))
                {
                    return;
                }

                if (currdev.bShouldReport)
                {
                    if (currdev.op_state == DeviceOperationStates.Installing)
                    {
                        int totalApk = currdev.apklist.Count();
                        int currApk = currdev.currInstallApkNo + 1;

                        if (strResult.Count() > 1)
                        {
                            onDeviceReport(sender, strResult[1], strResult[0], totalApk, currApk);
                        }
                        else
                        {
                            onDeviceReport(sender, currdev.installingApkName, strResult[0], totalApk, currApk);
                        }
                    }
                    else if (currdev.op_state == DeviceOperationStates.Uninstalling)
                    {
                        int totalApk = currdev.installedApks.Count();
                        int currApk = currdev.currUnInstallApkNo + 1;

                        if (strResult.Count() > 1)
                        {
                            onDeviceReport(sender, strResult[1], strResult[0], totalApk, currApk);
                        }
                        else
                        {
                            onDeviceReport(sender, currdev.uninstallingApkName, strResult[0], totalApk, currApk);
                        }
                    }
                    else
                    {
                        if (strResult.Count() > 1)
                        {
                            onDeviceReport(sender, strResult[1], strResult[0], 0, 0);
                        }
                        else
                        {
                            onDeviceReport(sender, "", strResult[0], 0, 0);
                        }
                    }
                }
            }   
        }

        public void LoadDeviceInfo(String deviceid, ApkResponseData res)
        {
            onDeviceLoad(deviceid, res);
        }

        public void DeviceInfoFail(String deviceid)
        {
            onDeviceLoadFail(deviceid);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }
    }
}

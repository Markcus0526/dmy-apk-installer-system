using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using MiscUtil.IO;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using ApkInstaller.ServiceCorrespond;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;

namespace ApkInstaller
{
    class ApkInfo
    {
        public String appName;
        public String pName;
        public String versionName;
        public int versionCode = 0;
        public int flag_app;
        public Image icon;
        public Int64 apkSize; // in Bytes
        public Int64 firstInstallTime;
        public Int64 lastUpdateTime;
       
        public int installState;
    }

    class AndroidInfo
    {
        public String uid = "";
        public String brand = "";
        public String device = "";
        public String fingerprint = "";
        public String hardware = "";
        public String manufacture = "";
        public String model = "";
        public String id = "";
        public String serial = "";
        public String sdk_int = "";
        public String imei = "";
    }

    public enum LogType
    {
        NOTICE,
        WARNING,
        ERROR,
        SUCCESS,
        FAILURE
    }

    public class DeviceLog
    {
        public LogType logtype { get; set; }
        public String logtime { get; set; }
        public String logtitle { get; set; }
        public String logcontent { get; set; }
        public bool seperator { get; set; }
    }

    enum DeviceState
    {
        Idle = 0,
        Installing,
        Uninstalling,
        Connected,
        Disconnected,
        MAX_State,
        FailStartAgent
    };

    public enum DeviceOperationStates
    {
        IdleState,
        Installing,
        Uninstalling
    }

    public enum DeviceInstallStatus
    {
        INSTALLING,
        COMPLETED
    }

    class AndroidDevice
    {
        private static String PKT_CONNECT = "CONN";
        private static String PKT_DISCONNECT = "DCON";
	    private static String PKT_SDCARD_FREESPACE = "SDFS";
	    private static String PKT_SDCARD_TOTALSPACE = "SDTS";
	    private static String PKT_INTERNALMEMORY_FREESPACE = "IMFS";
	    private static String PKT_INTERNALMEMORY_TOTALSPACE = "IMTS";
        private static String PKT_INSTALLED_PROGRAM_LISTS = "IDPL";
        private static String PKT_SCREEN_CAPTURE = "CASC";
        private static String PKT_ANDROID_INFO = "INFO";
        private static String PKT_APK_INSTALL_COMPLETED = "AIED";

        public String deviceid;
        public String vendor;
        public String model;
        public Int64 freeSpaceTel; //Bytes
        public Int64 totalSizeTel; //Bytes
        public Int64 freeSpaceSDCard; //Bytes
        public Int64 totalSizeSDCard; //Bytes
        public Image background;
        public int install_location;

        public AndroidInfo androidInfo = new AndroidInfo();
        public List<ApkInfo> installedApks = new List<ApkInfo>();
        public Int32 currUnInstallApkNo;
        public Int32 currInstallApkNo;
        public Int32 agent_port;
        public Int64 totalApkSize;
        //public ADBEventPipeServer pipeserver = new ADBEventPipeServer();
        TcpClient clientSocket = new TcpClient();
        public DeviceOperationStates op_state;
        public bool agentRunning = false;
        public float operation_percent;
        public Thread workingthread;
        public Thread initDeviceThread;
        public bool bShouldReport;
        public List<ApkAdbParam> apklist = new List<ApkAdbParam>();
        private ApkServiceCall m_service = new ApkServiceCall();

        public string installingApkGroup;
        public string installingApkName;
        public string uninstallingApkName;
        public AndroidDeviceWorker apkInstallWorker = new AndroidDeviceWorker();

        public DeviceState devStatus = DeviceState.Disconnected;

        public delegate void BeginInstallHandler(String deviceid);
        public event BeginInstallHandler onBeginInstallApk;

        public delegate void DeviceLogReportHandler(string deviceid, DeviceLog loginfo);
        public event DeviceLogReportHandler onLogReport;

        public delegate void DeviceInitializeHandler(String deviceid, ApkResponseData res);
        public event DeviceInitializeHandler onDeviceLoad;

        public delegate void DeviceInitFailHandler(String deviceid);
        public event DeviceInitFailHandler onDeviceLoadFail;

        public delegate void DeviceRefreshHandler(string deviceid);
        public event DeviceRefreshHandler onRefreshDevice;

        public int InstallApk(String strApkFilePath)
        {
            try
            {
                operation_percent = 0;
                op_state = DeviceOperationStates.Installing;
                ApkAdbParam apkinfo = ApkParser.GetApkInformation(strApkFilePath);
                if (String.IsNullOrEmpty(apkinfo.package))
                {
                    return -1;
                }

                int ret = AdbWrapper.InstallApk(deviceid, Encoding.UTF8.GetBytes(""), apkinfo.vcode, apkinfo.vname, apkinfo.package, install_location, strApkFilePath);
                return ret;
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "InstallApk()", ex.ToString());
                return -1;
            }
        }

        public int UninstallApk(int apkno, string apkname, bool bReportStatus)
        {
            if (apkno < 0 || apkno >= installedApks.Count)
                return -1;
            operation_percent = 0;
            op_state = DeviceOperationStates.Uninstalling;

            int ret = AdbWrapper.UninstallApk(deviceid, Encoding.UTF8.GetBytes(apkname), installedApks[apkno].pName, bReportStatus);
            return ret;
        }

        public String SendCmdToAgent(String cmd)
        {
            try
            {
//                 if (!clientSocket.Connected)
//                 {
//                     clientSocket.Client.Disconnect(true);
//                     clientSocket.Close();
//                     clientSocket = null;
//                     ConnectToAgent();
//                 }
                NetworkStream serverStream = clientSocket.GetStream();
                Int32 bodylength = 0;
                String strPackettype = "";

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(cmd);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                byte[] packetlength = new byte[4];

                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packetlength, 0, 4);
                    bodylength = System.BitConverter.ToInt32(packetlength, 0) - 4;
                }

                byte[] packettype = new byte[4];

                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packettype, 0, 4);
                    strPackettype = System.Text.Encoding.ASCII.GetString(packettype);
                }
                if (cmd != strPackettype)
                    return null;
                Int32 datalen = bodylength - 4;

                if (datalen <= 0)
                {
                    return "OK";
                }

                byte[] packetdata = new byte[datalen];

                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packetdata, 0, datalen);
                }

                return System.Text.Encoding.ASCII.GetString(packetdata);
            }
            catch (System.Exception ex)
            {
                return null;
            }

            return null;
        }

        public int GetAndroidInfo()
        {
            NetworkStream serverStream;
            Int32 bodylength = 0;
            String strPackettype = "";
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(PKT_ANDROID_INFO);

            if (ConnectToAgent() < 0)
            {
                devStatus = DeviceState.FailStartAgent;
                return -1;
            }

            serverStream = clientSocket.GetStream();

            try
            {
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                byte[] packetlength = new byte[4];

                //if (serverStream.DataAvailable)
                {
                    serverStream.ReadTimeout = 60 * 1000;   //1 minute
                    serverStream.Read(packetlength, 0, 4);
                    bodylength = System.BitConverter.ToInt32(packetlength, 0) - 4;
                }

                byte[] packettype = new byte[4];
                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packettype, 0, 4);
                    strPackettype = System.Text.Encoding.ASCII.GetString(packettype);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "GetAndroidInfo()", ex.ToString());
            }
            if (PKT_ANDROID_INFO != strPackettype)
                return -1;
            Int32 datalen = bodylength - 4;

            if (datalen <= 0)
            {
                return 0;
            }

            byte[] packetdata = new byte[datalen];
            int totalread = 0;
            while (totalread < datalen)
            {
                //if (serverStream.DataAvailable)
                {
                    int nreaded = serverStream.Read(packetdata, totalread, datalen - totalread);
                    totalread += nreaded;
                }
            }

            try
            {
                int nOff = 0;
                int nCountAndroInfo = System.BitConverter.ToInt32(packetdata, nOff);
                nOff += 4;

                int nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.uid = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.brand = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.device = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.fingerprint = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.hardware = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.manufacture = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.model = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.id = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.serial = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.sdk_int = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                androidInfo.imei = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                if (String.IsNullOrEmpty(androidInfo.imei))
                {
                    androidInfo.imei = "NULL";
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "GetAndroidInfo()", ex.ToString());            	
            }


            return 0;
        }

        public int GetSpace()
        {
            if (ConnectToAgent() < 0)
            {
                devStatus = DeviceState.FailStartAgent;
                return -1;
            }

            String strResult = SendCmdToAgent(PKT_INTERNALMEMORY_FREESPACE);
            if (strResult == null)
                return -1;
            if (strResult != "OK")
            {
                freeSpaceTel = Int64.Parse(strResult);
            }

            strResult = SendCmdToAgent(PKT_INTERNALMEMORY_TOTALSPACE);
            if (strResult == null)
                return -1;
            if (strResult != "OK")
            {
                totalSizeTel = Int64.Parse(strResult);
            }

            strResult = SendCmdToAgent(PKT_SDCARD_FREESPACE);
            if (strResult == null)
                return -1;
            if (strResult != "OK")
            {
                freeSpaceSDCard = Int64.Parse(strResult);
            }

            strResult = SendCmdToAgent(PKT_SDCARD_TOTALSPACE);
            if (strResult == null)
                return -1;
            if (strResult != "OK")
            {
                totalSizeSDCard = Int64.Parse(strResult);
            }
            return 0;
        }

        public int GetInstalledApkInfos()
        {
            NetworkStream serverStream;
            Int32 bodylength = 0;
            String strPackettype = "";
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(PKT_INSTALLED_PROGRAM_LISTS);

            if (ConnectToAgent() < 0)
            {
                devStatus = DeviceState.FailStartAgent;
                return -1;
            }

            serverStream = clientSocket.GetStream();

            try
            {
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                byte[] packetlength = new byte[4];

                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packetlength, 0, 4);
                    bodylength = System.BitConverter.ToInt32(packetlength, 0) - 4;
                }

                byte[] packettype = new byte[4];
                //if (serverStream.DataAvailable)
                {
                    serverStream.Read(packettype, 0, 4);
                    strPackettype = System.Text.Encoding.ASCII.GetString(packettype);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "GetInstalledApkInfos()", ex.ToString());            	
            }
            if (PKT_INSTALLED_PROGRAM_LISTS != strPackettype)
                return -1;
            Int32 datalen = bodylength - 4;

            if (datalen <= 0)
            {
                return 0;
            }

            byte[] packetdata = new byte[datalen];
            int totalread = 0;
            while (totalread < datalen)
            {
                //if (serverStream.DataAvailable)
                {
                    int nreaded = serverStream.Read(packetdata, totalread, datalen - totalread);
                    totalread += nreaded;
                }
            }

            totalApkSize = 0;
            installedApks.Clear();
            int nOff = 0;
            int nCountApps = System.BitConverter.ToInt32(packetdata, nOff);
            nOff += 4;
            for (int i = 0; i < nCountApps; i++)
            {
                ApkInfo apk = new ApkInfo();
                int nLen = System.BitConverter.ToInt32(packetdata, nOff);  nOff+=4;
                apk.appName = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;
                
                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                apk.pName = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;
                apk.versionName = System.Text.Encoding.UTF8.GetString(packetdata, nOff, nLen); nOff += nLen;

                apk.versionCode = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;

                apk.flag_app = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;

                apk.apkSize = System.BitConverter.ToInt32(packetdata, nOff); nOff += 8;
                
                totalApkSize += apk.apkSize;

                apk.firstInstallTime = System.BitConverter.ToInt64(packetdata, nOff); nOff += 8;

                apk.lastUpdateTime = System.BitConverter.ToInt64(packetdata, nOff); nOff += 8;

                nLen = System.BitConverter.ToInt32(packetdata, nOff); nOff += 4;

                try
                {
                    using (var ms = new MemoryStream(packetdata, nOff, nLen))
                    {
                        apk.icon = Image.FromStream(ms);
                    }
                }
                catch (System.Exception ex)
                {
                	
                }
                nOff += nLen;

                if (apk.pName.ToLower() == "com.damy.apkagent")
                {
                    continue;
                }

                installedApks.Add(apk);
            }
            return 0;
        }

        public int ConnectToAgent()
        {
            int retryCount = 80, i = 0;
//             Stopwatch sw = new Stopwatch();
//             sw.Start();
            agentRunning = true;
            while (!CheckAgentAlive())
            {
                try
                {
//                     if (i > retryCount)
//                         break;

                    if (!agentRunning)
                        break;

                    if (clientSocket == null)
                    {
                        clientSocket = new TcpClient();
                        clientSocket.ReceiveTimeout = 10 * 1000;
                    }

                    if (AdbWrapper.StartAgent(deviceid, Program.APKAGENT_FILEPATH, Program.APKAGENT_VERSIONCODE, Program.APKAGENT_PACKAGENAME) > 0)
                    {
                        //MessageBox.Show("Init Device failed");
                        return -1;
                    }

                    //if (sw.ElapsedMilliseconds > 60 *1000) throw new TimeoutException();

                    agent_port = Program.mDeviceList.GetFreePort();
                    AdbWrapper.ForwardTCP(deviceid, agent_port);

                    if (!clientSocket.Connected)
                    {
                        clientSocket.Connect("127.0.0.1", agent_port);
                    }

                    String canread = SendCmdToAgent(PKT_CONNECT);

                    if (canread == null)
                    {
                        continue;
                    }

                    return 0;
                }
                catch (SocketException ex)
                {
                    clientSocket.Close();
                    clientSocket = new TcpClient();
                    clientSocket.ReceiveTimeout = 10 * 1000;   //2 minutes

                    Thread.Sleep(300);
                    i++;
                }
                catch (TimeoutException ex)
                {
                    //sw.Restart();
                    Program.mDevicePoll.DeviceReported(deviceid, "TimeoutConnAgent");
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("AndroidDevice", "ConnectToAgent() - Exception", ex.ToString());
                }
            }
            
            return 0;
        }

        public int DisconnectAgent()
        {
            try
            {
                agentRunning = false;
                SendCmdToAgent(PKT_DISCONNECT);

            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "DisconnectAgent()", ex.ToString());
                Console.Write(ex.Message);
            }

            return 0;
        }

        public bool CheckAgentAlive()
        {
            if (clientSocket == null)
                return false;

            if (!clientSocket.Connected)
            {
                return false;
            }

            try
            {
                if (clientSocket.Client.Poll(0, SelectMode.SelectRead))
                {
                    if (!clientSocket.Connected) return false;
                    else
                    {
                        byte[] b = new byte[1];
                        try
                        {
                            if (clientSocket.Client.Receive(b, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                        }
                        catch { return false; }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public int ScreenCapture()
        {
            NetworkStream serverStream;
            Int32 bodylength = 0;
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(PKT_SCREEN_CAPTURE);

            if (ConnectToAgent() < 0)
            {
                devStatus = DeviceState.FailStartAgent;
                return -1;
            }

            serverStream = clientSocket.GetStream();
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            byte[] packetlength = new byte[4];

            //if (serverStream.DataAvailable)
            {
                serverStream.Read(packetlength, 0, 4);
                bodylength = System.BitConverter.ToInt32(packetlength, 0) - 4;
            }

            byte[] packettype = new byte[4];

            //if (serverStream.DataAvailable)
            {
                serverStream.Read(packettype, 0, 4);
            }

            String strPackettype = System.Text.Encoding.ASCII.GetString(packettype);
            if (PKT_SCREEN_CAPTURE != strPackettype)
                return -1;
            Int32 datalen = bodylength - 4;

            if (datalen <= 0)
            {
                return 0;
            }

            byte[] packetdata = new byte[datalen];
            int totalread = 0;
            while (totalread < datalen)
            {
                //if (serverStream.DataAvailable)
                {
                    int nreaded = serverStream.Read(packetdata, totalread, datalen - totalread);
                    totalread += nreaded;
                }
            }

            using (var ms = new MemoryStream(packetdata, 0, datalen))
            {
               background = Image.FromStream(ms);
            }

            return 0;
        }

        public int ChangeInstallLocation(int location)
        {
            install_location = location;
            //return AdbWrapper.SetInstallLocatin(deviceid, install_location);

            return 0;
        }

        public int InitDevice(String devid)
        {
            try
            {
                deviceid = devid;
                bShouldReport = true;

                onDeviceLoad += new DeviceInitializeHandler(Program.mDevicePoll.LoadDeviceInfo);
                onDeviceLoadFail += new DeviceInitFailHandler(Program.mDevicePoll.DeviceInfoFail);
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDevice", "InitDevice()", ex.ToString());
                Console.Write(ex.Message);
            }
            return 0;
        }

        public void InitPipeServer(string devid)
        {
            //pipeserver.InitServer(devid);
            //pipeserver.Listen();
        }

        public void RetrieveDeviceInfo(object deviceid)
        {
            String devid = (String)deviceid;

            //install_location = AdbWrapper.GetInstallLocatin(devid);

            if (GetSpace() < 0)
            {
                onDeviceLoadFail(devid);
                return;
            }

            GetAndroidInfo();

            vendor = androidInfo.brand;
            model = androidInfo.model;

            Callback_ServiceData callback = new Callback_ServiceData(this.Callback_SetDeviceStatus);

            m_service.CallApkWorker(
                ApkServiceUri.CheckNewDevice,
                HttpMethod.POST,
                new
                {
                    teltype = vendor,
                    vendor = model,
                    serial = androidInfo.uid,
                    imei = androidInfo.imei,
                    token = Program.AUTH_TOKEN
                },
                callback
            );

            //ReportDeviceInstalledApp();
        }
        public void CloseDevice()
        {
            if (apkInstallWorker.m_installWorker != null) {
                if (apkInstallWorker.m_installWorker.IsBusy)
                {
                    apkInstallWorker.m_installWorker.CancelAsync();
                    Program.mDeviceList.OnCompleted_DeviceApkInstall(deviceid);
                }
                else
                {
                    Program.mDeviceList.OnCompleted_DeviceApkInstall(deviceid);
                }
            }

            DisconnectAgent();
            //pipeserver.RequestStop();

            Thread.Sleep(100);
        }

        public void ReportDeviceInstalledApp()
        {
            DateTime utcStarttime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Callback_ServiceData callback = new Callback_ServiceData(this.Callback_NoneOper);

            GetInstalledApkInfos();

            var applist = (from m in installedApks
                           select new
                           {
                               appname = m.appName,
                               appversion = m.versionName,
                               installtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", utcStarttime.AddMilliseconds(m.firstInstallTime).ToLocalTime())
                           }).ToList();

            m_service.CallApkWorker(
                ApkServiceUri.ReportInstalledApp,
                HttpMethod.POST,
                new
                {
                    teltype = vendor,
                    vendor = model,
                    serial = androidInfo.uid,
                    imei = androidInfo.imei,
                    token = Program.AUTH_TOKEN,
                    applist = applist
                },
                callback
            );
        }

        public void Callback_SetDeviceStatus(ApkResponseData res)
        {
            if (res != null)
            {
//                 if (res.SVCC_RET == SERVICEERROR.ERR_ALREADY_EXISTS_TELAPPS)
//                 {
//                     onDeviceSetExist(deviceid);
//                 }
//                 else if (res.SVCC_RET == SERVICEERROR.ERR_SUCCESS)
//                 {
                    onDeviceLoad(deviceid, res);
//                 }
            }
            else
            {
                onDeviceLoadFail(deviceid);
            }
        }

        public void CallBeginInstall(String deviceid)
        {
            this.onBeginInstallApk(deviceid);
        }

        public void CallLogReport(string deviceid, LogType ltype, String ltime, String ltitle, String lcontent, bool seperator)
        {
            DeviceLog loginfo = new DeviceLog
            {
                logtype = ltype,
                logtime = ltime,
                logtitle = ltitle,
                logcontent = lcontent,
                seperator = seperator
            };

            this.onLogReport(deviceid, loginfo);
        }

        public void RefreshDeviceInfo()
        {
            this.onRefreshDevice(deviceid);
        }

        public String[] ReportState(String report)
        {
            String[] strRes = report.Split('|');

            switch (op_state)
            {
                case (DeviceOperationStates.Installing):
                    if (strRes[0].StartsWith("PushApk"))
                    {
                        if (apklist.Count() > 0)
                        {
                            operation_percent = (float)Math.Round((decimal)currInstallApkNo / apklist.Count(), 1);
                        }
                        //operation_percent = 0.1f;
                    }
                    else if (strRes[0].StartsWith("Push"))
                    {
                        //operation_percent = (float)(0.5 * Convert.ToDouble(report.Substring(4)));
                    }
                    else if (strRes[0].StartsWith("PkgInstall"))
                    {
                        //operation_percent = 0.6f;
                    }
                    else if (strRes[0].StartsWith("DeleteTemp"))
                    {
                        //operation_percent = 0.9f;
                    }
                    else if (strRes[0].StartsWith("Failure"))
                    {
                        operation_percent = (float)Math.Round((decimal)currInstallApkNo / apklist.Count(), 1);
                    }
                    else if (strRes[0].StartsWith("Finished"))
                    {
                    }
                    else if (strRes[0].StartsWith("InstallCompleted"))
                    {
                        operation_percent = 1f;
                    }
                    else if (strRes[0].StartsWith("CancelInstall"))
                    {
                        operation_percent = 0f;
                    }
                    break;
                case (DeviceOperationStates.Uninstalling):
                    if (strRes[0].StartsWith("UninstallApkStart"))
                    {
                        operation_percent = (float)Math.Round((decimal)currUnInstallApkNo / installedApks.Count(), 1);
                    }
                    else if (strRes[0].StartsWith("UninstallCompleted"))
                    {
                        operation_percent = 1f;
                    }
                    else if (strRes[0].StartsWith("CancelInstall"))
                    {
                        operation_percent = 0f;
                    }
                    else if (strRes[0].StartsWith("Failure"))
                    {
                        operation_percent = (float)Math.Round((decimal)currUnInstallApkNo / installedApks.Count(), 1);
                        strRes[0] += "UNINSTALL_FAILED";
                    }
                    break;
            }
            
            return strRes;
        }

        public String ScreenshotCapture()
        {
            return AdbWrapper.adb_screen_image(deviceid);
        }

        public void PlayInstall()
        {
            apkInstallWorker.PlayInstall();
        }

        public void PauseInstall()
        {
            apkInstallWorker.PauseInstall();
        }

        public void StopInstall()
        {
            apkInstallWorker.StopInstall();
        }

        public void SendInstallCompletedEvent()
        {
            SendCmdToAgent(PKT_APK_INSTALL_COMPLETED);
        }
        public DevicePlayStatus PlayStatus
        {
            get
            {
                return this.apkInstallWorker.GetPlayStatus();
            }
            set
            {
                this.apkInstallWorker.m_playStatus = value;
            }
        }

        public void Callback_NoneOper(ApkResponseData res)
        {

        }
    }
    
    class APKInfo
    {
        String visiblename;
        String version;
        String installdate;
        String classname;
        int typeAPK;
        Image icon;
        Int64 size;
    }
}

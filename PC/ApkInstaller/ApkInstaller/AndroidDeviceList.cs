using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ApkInstaller.ServiceCorrespond;

namespace ApkInstaller
{
    class AndroidDeviceList
    {
        private List<AndroidDevice> deviceList = new List<AndroidDevice>();

        public int AddDevice(String deviceid)
        {
            AndroidDevice newDevice = new AndroidDevice();
            if ( newDevice.InitDevice(deviceid) < 0 )
                return deviceList.Count;
            deviceList.Add(newDevice);
            return deviceList.Count;
        }

        public int GetCount()
        {
            return deviceList.Count;
        }

        public List<String> GetDeviceIDList()
        {
            return (from m in deviceList
                    select m.deviceid).ToList();
        }

        public AndroidDevice GetAt(int no)
        {
            if (no < 0 || no >= deviceList.Count)
                return null;
            return deviceList[no];
        }

        public void RemoveAllDevice()
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                deviceList[i].CloseDevice();
            }
            deviceList.Clear();
        }

        public int DeleteDevice(String deviceid)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                if (deviceList[i].deviceid.Equals(deviceid))
                {
                    deviceList[i].CloseDevice();
                    //OnCompleted_DeviceApkInstall(deviceid);
                    deviceList.RemoveAt(i);
                    break;
                }
            }
            return deviceList.Count;
        }

        public int DeleteDevice(int devno)
        {
            if (devno < deviceList.Count)
            {
                deviceList[devno].CloseDevice();
                //OnCompleted_DeviceApkInstall(deviceList[devno].deviceid);
                deviceList.RemoveAt(devno);
            }
            return deviceList.Count;
        }

        public void RebootDevice(String deviceid)
        {
            AndroidDevice rebootDev = FindDevByID(deviceid);
            if (rebootDev != null && rebootDev.devStatus == DeviceState.Connected)
            {
                if (rebootDev.op_state != DeviceOperationStates.IdleState)
                {
                    rebootDev.StopInstall();
                }

                rebootDev.devStatus = DeviceState.Disconnected;
                Thread rebootDeviceThread = new Thread(() => AdbWrapper.RebootDevice(deviceid));
                rebootDeviceThread.Start();
            }
            //AdbWrapper.RebootDevice(deviceList[devno].deviceid);
        }

        public void InstallApk(ApkAdbParam apkinfo)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                AndroidDevice  device = deviceList[i];
                device.workingthread = new Thread(() => device.InstallApk(apkinfo.filepath));
                device.workingthread.Start();
                //deviceList[i].InstallApk(localpath_apk);
            }
        }

        public void OnCompleted_DeviceApkInstall(String deviceid)
        {
            AndroidDevice currdev = FindDevByID(deviceid);

            if (currdev != null)
            {
                currdev.op_state = DeviceOperationStates.IdleState;
            }

            int idleCount = deviceList.Where(m => m.op_state == DeviceOperationStates.IdleState).Count();
            if (idleCount == deviceList.Count())
            {
                Program.m_callbackInstComp();
            }
        }

        public void InstallApk(String currGroup, List<ApkAdbParam> apklist, bool bAutoUninstall)
        {
            try
            {
                for (int i = 0; i < deviceList.Count; i++)
                {
                    AndroidDevice device = deviceList[i];
                    if (device.devStatus != DeviceState.Connected)
                    {
                        continue;
                    }
                    device.apklist = apklist;
                    device.installingApkGroup = currGroup;
                    device.op_state = DeviceOperationStates.Installing;
                    device.apkInstallWorker.m_playStatus = DevicePlayStatus.PLAY;

                    Callback_DevInstallCompleted callback = new Callback_DevInstallCompleted(this.OnCompleted_DeviceApkInstall);

                    device.apkInstallWorker.CallInstallWorker(
                        bAutoUninstall,
                        device.install_location,
                        device,
                        callback
                    );
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AndroidDeviceList", "InstallApk()", ex.ToString());
            }
        }

        public AndroidDevice FindDevByID(String strID)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                if (deviceList[i].deviceid.Equals(strID))
                    return deviceList[i];
            }
            return null;
        }

        public int GetConnectedDeviceCount()
        {
            return deviceList.Where(m => m.devStatus == DeviceState.Connected).Count();
        }

        public Int32 GetFreePort()
        {
            Int32 ret = 51200;
           
            while (ret<52000)
            {
                Boolean conflict = false;
                for (int i = 0; i < deviceList.Count; i++)
                {
                    if (deviceList[i].agent_port.Equals(ret))
                    {
                        conflict = true;
                        break;
                    }

                }
                if (!conflict)
                    return ret;
                ret++;
            }
            return ret;
        }
    }

}

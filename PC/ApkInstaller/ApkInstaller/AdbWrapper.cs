using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace ApkInstaller
{
    class AdbWrapper
    {
        [DllImport("AdbWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.LPStr)]
        public static extern String adb_list_devices(  );

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern String adb_screen_image(String deviceid);

        [DllImport("AdbWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.LPStr)]
        public static extern String adb_get_device_vendor(String deviceid);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern String adb_get_device_model(String deviceid);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adb_reboot_device(String deviceid);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern String adb_get_freespace(String deviceid);


        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adb_test_server();


        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_install_apk(
            String deviceid, 
            byte[] apkname, 
            String vcode,
            String vname,
            String package,
            int location, 
            String localpath_apk);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_uninstall_apk(String deviceid, byte[] apkname, String package_name, bool bReportStatus);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_start_agent(String deviceid, String apkagent_path, int apkagent_versioncode, String agent_packagename);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_forward_tcp(String deviceid, int localport, int remoteport);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_get_install_location(String deviceid);

        [DllImport("AdbWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adb_set_install_location(String deviceid, int install_location);

        public static String GetDeviceLists()
        {
            return adb_list_devices();
        }

        public static String GetDeviceScreenImage(String deviceid)
        {
            return adb_screen_image(deviceid);
        }

        public static String GetDeviceVendor(String deviceid)
        {
            return adb_get_device_vendor(deviceid);
        }

        public static String GetDeviceModel(String deviceid)
        {
            return adb_get_device_model(deviceid);
        }

        public static void RebootDevice(String deviceid)
        {
            adb_reboot_device(deviceid);
        }

        public static String GetFreeSpace(String deviceid)
        {
            return adb_get_freespace(deviceid);
        }

        public static int InstallApk(
            String deviceid, 
            byte[] apkName, 
            String vcode, 
            String vname, 
            String package, 
            int instLocation, 
            String localpath_apk)
        {
            return adb_install_apk(
                deviceid,
                apkName,
                vcode,
                vname,
                package,
                instLocation, 
                localpath_apk);
        }

        public static int UninstallApk(String deviceid, byte[] apkname, String package_name, bool bReportStatus)
        {
            return adb_uninstall_apk(deviceid, apkname, package_name, bReportStatus);
        }


        public static int StartAgent(String deviceid, String apkagent_path, int apkagent_versioncode, String agent_packagename)
        {
            return adb_start_agent(deviceid, apkagent_path, apkagent_versioncode, agent_packagename);
        }

        public static int ForwardTCP(String deviceid, int localport)
        {
            return adb_forward_tcp(deviceid, localport, 25000);
        }

        public static int GetInstallLocatin(String deviceid)
        {
            return adb_get_install_location(deviceid);
        }

        public static int SetInstallLocatin(String deviceid, int install_location)
        {
            return adb_set_install_location(deviceid, install_location);
        }
       
    }

    
}

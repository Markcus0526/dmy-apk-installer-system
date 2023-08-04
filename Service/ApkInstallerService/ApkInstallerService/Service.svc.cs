using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ApkInstallerService.ServiceDB;
using ApkInstallerService.ServiceModel;
using Newtonsoft.Json;

namespace ApkInstallerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
    public class Service : IService
    {
        ApkServiceResponser _serviceResponser = new ApkServiceResponser();

        public ApkResponseData LoginUser(String username, String password)
        {
            return _serviceResponser.LoginUser(username, password);
        }

        public ApkResponseData GetApkGroupList(String token)
        {
            return _serviceResponser.GetApkGroupList(token);
        }

        public ApkResponseData GetApkList(String groupid)
        {
            return _serviceResponser.GetApkList(groupid);
        }

        public ApkResponseData GetApkInstallLog(String userid, String token)
        {
            return _serviceResponser.GetApkInstallLog(userid, token);
        }

        public ApkResponseData InsertApkInstallLog(            
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            String token)
        {
            return _serviceResponser.InsertApkInstallLog(teltype, vendor, apkid, serial, imei, status, note, clientver, token);
        }

        public ApkResponseData InsertApkUpdateLog(
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            String token)
        {
            return _serviceResponser.InsertApkUpdateLog(teltype, vendor, apkid, serial, imei, status, note, clientver, token);
        }

        public ApkResponseData InsertApkUninstallLog(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String apkname,
            String status,
            String note,
            String clientver,
            String token)
        {
            return _serviceResponser.InsertApkUninstallLog(teltype, vendor, serial, imei, apkname, status, note, clientver, token);
        }

        public ApkResponseData CheckNewDevice(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token)
        {
            return _serviceResponser.CheckNewDevice(teltype, vendor, serial, imei, token);
        }

        public ApkResponseData ReportInstalledApp(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token,
            List<TelAppInfo> applist)
        {
            return _serviceResponser.ReportInstalledApp(teltype, vendor, serial, imei, token, applist);
        }

        public ApkResponseData GetLevel2UserList(String token)
        {
            return _serviceResponser.GetLevel2UserList(token);
        }

        public ApkResponseData CheckUpdateVersion(String vcode)
        {
            return _serviceResponser.GetUpdateVersion(vcode);
        }
    }
}

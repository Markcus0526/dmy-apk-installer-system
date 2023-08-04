using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ApkInstallerService.ServiceModel;
using ApkInstallerService.Json;

namespace ApkInstallerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService
    {
        #region API_AUTH
        [OperationContract]
        [WebGet(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData LoginUser(String username, String password);
        #endregion

        #region API_APKSERVICE
        [WebGet, OperationContract]
        ApkResponseData GetApkGroupList(String token);

        [WebGet, OperationContract]
        ApkResponseData GetApkList(String groupid);

        [WebGet, OperationContract]
        ApkResponseData GetApkInstallLog(String userid, String token);

        [WebInvoke(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData CheckNewDevice(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token);

        [WebInvoke(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData InsertApkInstallLog(
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            String token);

        [WebInvoke(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData InsertApkUpdateLog(
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            String token);

        [WebInvoke(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData InsertApkUninstallLog(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String apkname,
            String status,
            String note,
            String clientver,
            String token);

        [WebInvoke(//UriTemplate = "CheckRegist/{carNo}",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        ApkResponseData ReportInstalledApp(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token,
            List<TelAppInfo> applist);

        [WebGet, OperationContract]
        ApkResponseData GetLevel2UserList(String token);

        [WebGet, OperationContract]
        ApkResponseData CheckUpdateVersion(String vcode);

        #endregion
    }
}

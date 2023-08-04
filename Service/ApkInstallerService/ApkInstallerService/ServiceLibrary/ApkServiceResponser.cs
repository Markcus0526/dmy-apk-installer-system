using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ApkInstallerService.ServiceModel;
using ApkInstallerService.ServiceLibrary;

namespace ApkInstallerService.ServiceDB
{
    public class ApkServiceResponser
    {
        #region Fields and properties
        public ServiceDBDataContext _dbcontext = new ServiceDBDataContext();

        private DBUser _dbUser = new DBUser();
        private DBApk _dbApk = new DBApk();
        private DBPatch _dbPatch = new DBPatch();
        #endregion

        public ApkResponseData LoginUser(String username, String password)
        {
            return _dbUser.LoginUser(_dbcontext, username, password);
        }

        public ApkResponseData GetApkGroupList(String token)
        {
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.RetrieveGroupList(_dbcontext, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
        }

        public ApkResponseData GetApkList(String groupid)
        {
            ApkResponseData ret = new ApkResponseData();

            /////// 30 days trial license (temporary) /////////
//             if (!ApkLicenseService.CheckTrialVersion())
//             {
//                 return null;
//             }

            try
            {
                ret = _dbApk.RetrieveApkList(_dbcontext, long.Parse(groupid));
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                ret.Result = APKERROR.ERR_PARSE_FAIL;
            }

            return ret;
        }

        public ApkResponseData GetApkInstallLog(String puserid, String token)
        {
            ApkResponseData ret = new ApkResponseData();

            tbl_user curruser = _dbUser.GetAuthorizeUser(_dbcontext, token);

            if (curruser != null)
            {
                try
                {
                    long puid = long.Parse(puserid);
                    ret = _dbApk.RetrieveApkInstallLog(_dbcontext, curruser, puid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
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
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.AddApkUninstallLog(_dbcontext, teltype, vendor, serial, imei, apkname, status, note, clientver, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
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
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.AddApkInstallLog(_dbcontext, teltype, vendor, apkid, serial, imei, status, note, clientver, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
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
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.AddApkUpdateLog(_dbcontext, teltype, vendor, apkid, serial, imei, status, note, clientver, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
        }

        public ApkResponseData CheckNewDevice(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token)
        {
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.CheckNewDevice(_dbcontext, teltype, vendor, serial, imei, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
        }

        public ApkResponseData ReportInstalledApp(
            String teltype,
            String vendor,
            String serial,
            String imei,
            String token,
            List<TelAppInfo> applist)
        {
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbApk.AddTelAppInfo(_dbcontext, teltype, vendor, serial, imei, applist, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
        }

        public ApkResponseData GetLevel2UserList(String token)
        {
            ApkResponseData ret = new ApkResponseData();

            long userid = _dbUser.AuthorizeUser(_dbcontext, token);

            if (userid >= 0)
            {
                try
                {
                    ret = _dbUser.RetrieveLevelUserList(_dbcontext, userid);
                }
                catch (System.Exception ex)
                {
                    ApkCommon.LogErrors(ex.ToString());
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
                return ret;
            }
            else
            {
                ret.Result = APKERROR.ERR_INVALID_USER;
            }

            return ret;
        }

        public ApkResponseData GetUpdateVersion(String vcode)
        {
            ApkResponseData ret = new ApkResponseData();

            if (String.IsNullOrEmpty(vcode))
            {
                ret.Result = APKERROR.ERR_PARSE_FAIL;
            }
            else
            {
                try
                {
                    int versionCode = int.Parse(vcode);
                    ret = _dbPatch.RetrievePatchList(_dbcontext, versionCode);
                }
                catch (System.Exception ex)
                {
                    ret.Result = APKERROR.ERR_PARSE_FAIL;
                }
            }

            return ret;
        }
    }
}
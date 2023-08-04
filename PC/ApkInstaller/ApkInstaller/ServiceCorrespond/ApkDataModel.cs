using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace ApkInstaller.ServiceCorrespond
{
    #region constants
    public enum SERVICEERROR
    {
        ERR_SUCCESS = 0, //Success
        ERR_LOGIN_INVALIDUSER = 100,    //incorrect user
        ERR_INCORRECT_PASSWORD,    //incorrect password
        ERR_APK_NOTEXIST = 120,
        ERR_INTERNAL_EXCEPTION = 500,
        ERR_PARSE_FAIL,
        ERR_INVALID_USER,
        ERR_ALREADY_EXISTS_TELAPPS
    }

    public static class APKEERROR
    {
        public static string APK_SUCCESS = "";

        public static string APK_INVALID_USER { get { return ApkDataModel.APKLan("APK_INVALID_USER"); } }
        public static string APK_INCORRECT_PWD { get { return ApkDataModel.APKLan("APK_INCORRECT_PWD"); } }
        public static string APK_NETWORK_FAIL { get { return ApkDataModel.APKLan("APK_NETWORK_FAIL"); } }
        public static string APK_UNKNOWN_ERR { get { return ApkDataModel.APKLan("APK_UNKNOWN_ERR"); } }
    }
    #endregion

    #region Service Definitions
    public static class ApkServiceUri
    {
        public const string LoginUser = "LoginUser";
        public const string GetApkGroupList = "GetApkGroupList";
        public const string GetApkList = "GetApkList";
        public const string GetApkInstallLog = "GetApkInstallLog";
        public const string InsertApkInstallLog = "InsertApkInstallLog";
        public const string InsertApkUpdateLog = "InsertApkUpdateLog";
        public const string InsertApkUninstallLog = "InsertApkUninstallLog";
        public const string CheckNewDevice = "CheckNewDevice";
        public const string ReportInstalledApp = "ReportInstalledApp";
        public const string GetLevel2UserList = "GetLevel2UserList";
        public const string CheckUpdateVersion = "CheckUpdateVersion";
    }
    #endregion

    #region Http Methods
    public static class HttpMethod
    {
        public const string POST = "POST";
        public const string GET = "GET";
    }
    #endregion

    #region Service Data Models
    public class ApkResponseData
    {
        public SERVICEERROR SVCC_RET { get; set; }

        object retdata = new object();
        public object SVCC_DATA
        {
            get { return retdata; }
            set { retdata = value; }
        }

        String token = "";
        public String SVCC_TOKEN
        {
            get { return token; }
            set { token = value; }
        }

        public String SVCC_BASEURL { get; set; }
    }

    public class ApkGroupInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImgPath { get; set; }
        public string CreateTime { get; set; }
    }

    public class ApkFileInfo
    {
        public long Id { get; set; }
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string ImgPath { get; set; }
        public long? FileSize { get; set; }
        public string Version { get; set; }
        public string CreateTime { get; set; }
    }

    public class ApkInstallLogInfo
    {
        public long Id { get; set; }
        public string ApkName { get; set; }
        public string TelVendor { get; set; }
        public string TelType { get; set; }
        public string TelInfo { get; set; }
        public string AndroidID { get; set; }
        public string ApkId { get; set; }
        public string TelId { get; set; }
        public string ImgPath { get; set; }
        public DateTime CreateTime { get; set; }        
    }


    public class ApkInstallDeviceStatictics
    {        
        public string deviceName;
        public int thisDayCount;
        public int thisMonthCount;
    }

    public class ApkInstallApkStatictics
    {
        public string apkID;
        public string apkName;
        public string imagePath;
        public int thisDayCount;
        public int thisMonthCount;
    }

    public class Level2ProxyName
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class PatchFileInfo
    {
        public long filesize { get; set; }
        public string filepath { get; set; }
    }

    public class PatchFiles
    {
        public String PatchFilePath { get; set; }
        public long PatchFileSize { get; set; }
        public List<PatchFileInfo> PatchList { get; set; }
    }
    #endregion

    #region Program Data Models
    public class ApkAdbParam
    {
        public long uid { get; set; }
        public string name { get; set; }
        public string filepath { get; set; }
        public string vcode { get; set; }
        public string vname { get; set; }
        public string package { get; set; }
    }
    #endregion

    class ApkDataModel
    {
        public static String strAuthSalt = "ApkInstaller1234567890987654321";
        public static string LOGFILENAME = "Log.txt";

        public static string GetMD5Hash(string value)
        {
            MD5 md5Hasher = MD5.Create();
            //byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value + strAuthSalt));
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string HanCut(string val, int cut_len)
        {
            string retStr = "";
            int byteLen = val.Length;

            if (byteLen <= cut_len)
            {
                return val;
            }
            else
            {
                retStr = val.Substring(0, cut_len);
                retStr = String.Concat(retStr, "...");
            }

            return retStr;
        }

        public static string APKLan(string keyword)
        {
            return Program.ResMan.GetString(keyword, Thread.CurrentThread.CurrentCulture);
        }

        public static void WriteLogFile(string fileName, string methodName, string message)
        {
            try
            {
                string filepath = Program.PROGRAM_PATH + "\\" + LOGFILENAME;
                if (!string.IsNullOrEmpty(message))
                {
                    using (FileStream file = new FileStream(filepath, File.Exists(filepath) ? FileMode.Append : FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        StreamWriter streamWriter = new StreamWriter(file);
                        streamWriter.WriteLine((((System.DateTime.Now + " - ") + fileName + " - ") + methodName + " - ") + message);
                        streamWriter.Close();
                    }
                }
            }
            catch
            {

            }
        }

        public static string GetApplicationDirectory()
        {
            string rootDir = Application.StartupPath;

            return rootDir;
        }

        public enum ApkLanguage
        {
            CHINESE = 0,
            ENGLISH
        }

        public enum USERLEVEL
        {
            LEVEL1 = 1,
            LEVEL2
        }

        public enum USERQUERYLEVEL
        {
            LEVELALL = -100,
            LEVEL2ALL,
            ONESELF
        }
    }
}

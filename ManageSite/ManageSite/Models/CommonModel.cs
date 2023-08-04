using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Web.Security;
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using System.Text;
using System.Xml;

namespace ManageSite.Models
{
    public class ComboBoxDataItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
    }

    public class Foreign_Operation
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Foreign_Allow
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Foreign_Priority
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Foreign_Sex
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class SearchDateType
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class SearchOption
    {
        public long key { get; set; }
        public string value { get; set; }
    }

    public class SmtpSecureOption
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public static class RESULTS
    {
        public const int ERROR = -1;
    }

    public static class DBRESULT
    {
        public const int CANNOT_DELETE = 520;
    }

    public class CommonModel
    {
        #region Constants


        #endregion

        public static SqlDBDataContext _db;
        public static string _logFilename = "Log.txt";

        public static string GetDisplayDateFormat()
        {
            return "yyyy年M月d日";
        }

        public static string GetDisplayDateTimeFormat()
        {
            return "yyyy年M月d日 HH:mm:ss";
        }

        public static string GetDisplayTimeFormat()
        {
            return "HH - mm";
        }

        public static string GetParamDateFormat()
        {
            return "yyyy-MM-dd";
        }

        public static string GetParamDateTimeFormat()
        {
            return "yyyy-MM-dd HH:mm:ss";
        }

        public static string ConnectionString
        {
            get 
            {
                return ConfigurationManager.ConnectionStrings["apkinstallerConnectionString"].ConnectionString;
            }
        }
        public static SqlDBDataContext GetDBContext()
        {
            _db = new SqlDBDataContext(ConfigurationManager.ConnectionStrings["apkinstallerConnectionString"].ConnectionString);
            return _db;
        }

        public static IQueryable<Foreign_Operation> GetOperationForeignList()
        {
            IList<Foreign_Operation> tmp = new List<Foreign_Operation>();

            tmp.Add(new Foreign_Operation { Id = 1, Name = "<span class='label label-success'>成功</span>" });
            tmp.Add(new Foreign_Operation { Id = 0, Name = "<span class='label label-danger'>- 失败 -</span>" });
            return tmp.AsQueryable();
        }

        public static int getOperationState(bool bSuccess)
        {
            if (bSuccess == true)
                return 1;
            return 0;
        }

        public static IEnumerable<SmtpSecureOption> GetSmtpSecureOption()
        {
            IList<SmtpSecureOption> tmp = new List<SmtpSecureOption>();

            tmp.Add(new SmtpSecureOption { value = "", name = "" });
            tmp.Add(new SmtpSecureOption { value = "ssl", name = "ssl" });
            return tmp.AsEnumerable();
        }

        public static IQueryable<Foreign_Allow> GetStatusForeignList()
        {
            IList<Foreign_Allow> tmp = new List<Foreign_Allow>();

            tmp.Add(new Foreign_Allow { Id = 1, Name = "显示" });
            tmp.Add(new Foreign_Allow { Id = 0, Name = "不显示" });
            return tmp.AsQueryable();
        }

        public static IQueryable<Foreign_Allow> GetAllowForeignList()
        {
            IList<Foreign_Allow> tmp = new List<Foreign_Allow>();

            tmp.Add(new Foreign_Allow { Id = (long)ADMINSTATUS.DISABLED, Name = "禁止用" });
            tmp.Add(new Foreign_Allow { Id = (long)ADMINSTATUS.APPROVED, Name = "正在用" });
            tmp.Add(new Foreign_Allow { Id = (long)ADMINSTATUS.PENDING, Name = "待审核" });
            return tmp.AsQueryable();
        }

        public static IQueryable<Foreign_Priority> GetRoleForeignList()
        {
            IList<Foreign_Priority> tmp = new List<Foreign_Priority>();

            tmp.Add(new Foreign_Priority { Id = "Admin", Name = "系统管理员" });
            tmp.Add(new Foreign_Priority { Id = "Leader", Name = "领导" });
            tmp.Add(new Foreign_Priority { Id = "Normal", Name = "普通管理员" });
            return tmp.AsQueryable();
        }

        public static IQueryable<Foreign_Sex> GetSexForeignList()
        {
            IList<Foreign_Sex> tmp = new List<Foreign_Sex>();

            tmp.Add(new Foreign_Sex { Id = 0, Name = "女" });
            tmp.Add(new Foreign_Sex { Id = 1, Name = "男" });
            return tmp.AsQueryable();
        }

        public string[] GetUserRoles()
        {
            string cookieName = FormsAuthentication.FormsCookieName;

            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

            string[] roles = authTicket.UserData.Split(new char[] { '|' });

            return roles;
        }

        public bool CheckUserRoles(string p_role)
        {
            string[] uroles = GetUserRoles();
            if (uroles.Contains(p_role) == true) {
                return true;
            }

            return false;
        }

        public string GetCurrentUserName()
        {
            FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;

            if (id != null)
            {
                return id.Name;
            }

            return null;
        }

        public static DateTime GetCurrentLoginTime()
        {
            return Convert.ToDateTime(HttpContext.Current.Session["loginTime"]);
        }

        public static bool make_jpg(string input, string output, string basePath)
        {
            if (!System.IO.File.Exists(basePath + input)) return false;
            
            Process ffmpeg = new Process();
            //ffmpeg.StartInfo.Arguments = string.Format("-y -i {0} -an -ss 00:00:14.35 -r 1 -vframes 1 -f mjpeg  {1}", input, Path.ChangeExtension(output, "jpg"));
            ffmpeg.StartInfo.Arguments = string.Format("-i {0} -an -ss 00:00:0 -r 1 -vframes 1 -f mjpeg -y {1}", basePath + input, basePath + output);
            ffmpeg.StartInfo.FileName = basePath + "plug-in/ffmpeg/ffmpeg.exe";
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpeg.Start();
            ffmpeg.WaitForExit(1000 * 10);

            return true;
        }

        public static void WriteLogFile(string fileName, string methodName, string message)
        {

            try
            {
                string filepath = HostingEnvironment.MapPath("~/") + "\\" + _logFilename;
                if (!string.IsNullOrEmpty(message))
                {
                    using (FileStream file = new FileStream(filepath, File.Exists(filepath)? FileMode.Append:FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        StreamWriter streamWriter = new StreamWriter(file);
                        streamWriter.WriteLine((((System.DateTime.Now + " - ") + fileName + " - ") + methodName + " - ") + message);
                        streamWriter.Close();
                    }
                }
            }
            catch {

            }

        }

        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;
            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }

        public static string GetContactPhone(string mobile_phone,
            string office_phone_region,
            string office_phone)
        {
            string ret = "";
            if (mobile_phone != null && mobile_phone.Trim().Length != 0)
                ret += mobile_phone.Trim();
            if (office_phone_region != null && office_phone_region.Trim().Length != 0)
            {
                if (office_phone != null && office_phone.Trim().Length != 0)
                {
                    if (ret != "")
                        ret = ret + ", ";
                    ret += office_phone_region.Trim() + "-";
                }
            }

            if (office_phone != null && office_phone.Trim().Length != 0)
            {
                if (ret != "")
                    if (office_phone_region == null || office_phone_region.Trim().Length == 0)
                        ret += ", ";
                ret += office_phone.Trim();
            }

            return ret;
        }

        public static bool CleanHistory()
        {
            string fileTempDir = HostingEnvironment.MapPath("~/FileTmp");
            if (Directory.Exists(fileTempDir))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fileTempDir);

                // Insert by AnH.(2013.03.18)
                DeleteDirectory(dirInfo);
                return true;
            }

            return false;
        }

        // Insert by AnH.(2013.03.20)
        public static bool DeleteDirectory(DirectoryInfo dirInfo)
        {
            foreach (System.IO.FileInfo file in dirInfo.GetFiles())
            {
                try
                {
                    if (file.IsReadOnly)
                        file.Attributes = FileAttributes.Normal;
                    file.Delete();
                }
                catch (System.Exception ex)
                {
                    CommonModel.WriteLogFile("CommonModel", "DeleteDirectory() -> Delete file", ex.ToString());
                }
            }

            foreach (System.IO.DirectoryInfo subDirectory in dirInfo.GetDirectories())
            {
                DeleteDirectory(subDirectory);

                try
                {
                    if (dirInfo.Attributes == FileAttributes.ReadOnly)
                        dirInfo.Attributes = FileAttributes.Normal;
                    dirInfo.Delete(true);
                }
                catch (System.Exception ex)
                {
                    CommonModel.WriteLogFile("CommonModel", "DeleteDirectory() -> Delete directory", ex.ToString());
                }
            }

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApkInstaller
{
    class Global
    {
        public static string SECTION_SECURITY = "SECURITY";
        public static string KEY_LOGON_REMEMBER = "LogonRemember";
        public static string KEY_LOGON_USERNAME = "LogonUsername";
        public static string KEY_LOGON_PASSWORD = "LogonPassword";

        private static string strRootPath;
        public static string RootPath
        {
            get { return strRootPath; }
            set { strRootPath = value; }
        }
    }
}

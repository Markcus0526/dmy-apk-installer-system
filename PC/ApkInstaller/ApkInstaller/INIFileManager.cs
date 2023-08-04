using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ApkInstaller
{
    class INIFileManager
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(String section, String key, String val,String filePath);
        
        public String GetINIValue(String section, String key, String iniPath)
        {
            StringBuilder strVal = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", strVal, 255, iniPath);
            return strVal.ToString();
        }

        public void SetIniValue(String section, String key, String value, String iniPath)
        {
            WritePrivateProfileString(section, key, value, iniPath);
        }
    }
}

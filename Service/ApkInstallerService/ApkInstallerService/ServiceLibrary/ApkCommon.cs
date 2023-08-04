using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ApkInstallerService.ServiceLibrary
{
    public class ApkCommon
    {
        #region Fields and properties
        private static String strErrorFileName = "ApkInstaller.log";
        public static String strAuthSalt = "ApkInstaller1234567890987654321";
        #endregion

        #region Public methods
        public static void LogErrors(String pMessage)
        {
            try
            {
                //StreamWriter w = File.AppendText(System.Environment.CurrentDirectory + "\\" + strErrorFileName);
                StreamWriter w = File.AppendText("C:\\" + strErrorFileName);
                w.WriteLine("{0}--{1}--{2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), pMessage);
                w.Flush();
                w.Close();
            }
            catch
            {
                //LogErrors(ex.ToString());
            }
        }

        public static string GetMD5Hash(string value)
        {
            MD5 md5Hasher = MD5.Create();
//            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value + strAuthSalt));
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        #endregion
    }
}
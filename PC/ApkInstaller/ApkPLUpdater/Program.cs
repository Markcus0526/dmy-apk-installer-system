using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace ApkPLUpdater
{
    class Program
    {
        public static bool bLiveApkInstaller = true;
        public static string PatchDir = "";
        public static string RootDir = "";

        static void Main(string[] args)
        {
            int waitCount = 50, i = 0;
            Process[] proc_adb = Process.GetProcessesByName("aiadb");
            RootDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            PatchDir = RootDir + "\\tmp";
            
            try
            {
                foreach (var item in proc_adb)
                {
                    item.Kill();
                }

                while (bLiveApkInstaller)
                {
                    if (i >= waitCount)
                    {
                        break;
                    }
                    Process[] apkinstallers = Process.GetProcessesByName("ApkInstaller");

                    if (apkinstallers.Count() == 0)
                    {
                        bLiveApkInstaller = false;
                    }
                    Thread.Sleep(200);
                    i++;
                }

                if (!bLiveApkInstaller)
                {
                    UpdateApkInstaller();
                }

            }
            catch (System.Exception ex)
            {
            	
            }
        }

        public static string GetXmlString(string fpath)
        {
            return File.ReadAllText(fpath);
        }

        public static bool UpdateApkInstaller()
        {
            bool rst = false;
            string patchxmlfile = PatchDir + "\\" + "PatchFiles.xml";

            if (File.Exists(patchxmlfile))
            {
                try
                {
                    XDocument xdoc = XDocument.Parse(RemoveXmlInvalidCharacters(GetXmlString(patchxmlfile)));

                    List<XElement> patchlist = xdoc.Element("PatchFiles").Element("mandatory").Elements("updateitem").ToList();

                    foreach (XElement item in patchlist)
                    {
                        string itempath = item.Value.ToString();
                        string filepath = PatchDir + "\\" + itempath;
                        string targetpath = RootDir + "\\" + itempath;

                        if (File.Exists(filepath))
                        {
                            if (itempath.Contains("ApkPLUpdater.exe"))
                            {
                                continue;
                            }
                            if (File.Exists(targetpath))
                            {
                                File.Delete(targetpath);
                            }
                            File.Copy(filepath, targetpath);
                        }
                    }

                    DirectoryInfo di = new DirectoryInfo(PatchDir);
                    CleanDirectory(di);

                    Process.Start(RootDir + "\\" + "ApkInstaller.exe");
                }
                catch (System.Exception ex)
                {

                }

            }

            return rst;
        }

        private static string RemoveXmlInvalidCharacters(string s)
        {
            return Regex.Replace(
                s,
                @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]",
                string.Empty);
        }

        private static void CleanDirectory(DirectoryInfo di)
        {
            if (di == null)
                return;
            try
            {
                foreach (FileSystemInfo fsEntry in di.GetFileSystemInfos())
                {
                    CleanDirectory(fsEntry as DirectoryInfo);
                    fsEntry.Delete();
                }
                WaitForDirectoryToBecomeEmpty(di);
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private static void WaitForDirectoryToBecomeEmpty(DirectoryInfo di)
        {
            for (int i = 0; i < 5; i++)
            {
                if (di.GetFileSystemInfos().Length == 0)
                    return;
                Console.WriteLine(di.FullName + i);
                Thread.Sleep(50 * i);
            }
        }
    }
}
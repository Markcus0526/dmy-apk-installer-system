using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using ApkInstaller.ServiceCorrespond;
using System.Text.RegularExpressions;

namespace ApkInstaller
{
    public class ApkParser
    {
        public static string GetManifestString(String apkpath)
        {
            string apkPath = apkpath;
            try
            {
                using (ZipInputStream zip = new ZipInputStream(File.OpenRead(apkPath)))
                {
                    using (FileStream filestream = new FileStream(apkPath, FileMode.Open, FileAccess.Read))
                    {
                        ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                        ZipEntry item;

                        while ((item = zip.GetNextEntry()) != null)
                        {
                            if (item.Name == "AndroidManifest.xml")
                            {
                                byte[] bytes = new byte[500 * 1024];

                                Stream strm = zipfile.GetInputStream(item);
                                int size = strm.Read(bytes, 0, bytes.Length);

                                using (BinaryReader s = new BinaryReader(strm))
                                {
                                    byte[] bytes2 = new byte[size];
                                    Array.Copy(bytes, bytes2, size);
                                    AndroidDecompress decompress = new AndroidDecompress();
                                    return decompress.decompressXML(bytes);
                                }
                            }
                        }
                    }

                }
            }
            catch (System.Exception ex)
            {
                string strexp = ex.ToString();
            }

            return "";
        }

        private static string RemoveXmlInvalidCharacters(string s)
        {
            return Regex.Replace(
                s,
                @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]",
                string.Empty);
        }

        public static ApkAdbParam GetApkInformation(string apkpath)
        {
            ApkAdbParam ret = new ApkAdbParam();

            string manifestXml = GetManifestString(apkpath);

            if (!String.IsNullOrEmpty(manifestXml))
            {
                try
                {
                    XDocument xdoc = XDocument.Parse(RemoveXmlInvalidCharacters(manifestXml));
                    ret.vcode = xdoc.Element("manifest").Attribute("versionCode").Value;
                    ret.vname = xdoc.Element("manifest").Attribute("versionName").Value;
                    ret.package = xdoc.Element("manifest").Attribute("package").Value;
                }
                catch (System.Exception ex)
                {
                	
                }
            }

            return ret;
        }
    }
}

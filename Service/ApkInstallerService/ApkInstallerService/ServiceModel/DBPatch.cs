using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ApkInstallerService.ServiceLibrary;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib;

namespace ApkInstallerService.ServiceModel
{
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

    public class DBPatch
    {
        public string patchFileName = "PatchFiles.xml";
        public ApkResponseData RetrievePatchList(ServiceDBDataContext db, int vcode)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                tbl_apppatch newpatch = (from m in db.tbl_apppatches
                                         where m.deleted == 0 && m.versionCode > vcode
                                         orderby m.versionCode descending
                                         select m).FirstOrDefault();

                result.Result = APKERROR.ERR_NOTEXIST_UPDATE;

                if (newpatch != null)
                {
                    String patchpath = DBCommon.physicalpath + newpatch.patchpath;
                    PatchFiles retData = new PatchFiles();
                    retData.PatchList = new List<PatchFileInfo>();

                    if (File.Exists(patchpath))
                    {
                        FileInfo finfo = new FileInfo(patchpath); 
                        retData.PatchFilePath = newpatch.patchpath;
                        retData.PatchFileSize = finfo.Length;

                        using (ZipInputStream zipIn = new ZipInputStream(File.OpenRead(patchpath)))
                        {
                            ZipEntry entry;
                            List<XElement> patchlist = new List<XElement>();

                            while ((entry = zipIn.GetNextEntry()) != null)
                            {
                                if (!entry.Name.EndsWith("/"))
                                {
                                    PatchFileInfo newitem = new PatchFileInfo();
                                    newitem.filepath = entry.Name;
                                    newitem.filesize = entry.Size;
                                    retData.PatchList.Add(newitem);
                                }
                                if (entry.Name == patchFileName)
                                {
                                    long size = entry.Size;
                                    byte[] data = new byte[size];
                                    zipIn.Read(data, 0, data.Length);

                                    string patchString = Encoding.ASCII.GetString(data);
                                    XDocument xdoc = XDocument.Parse(RemoveXmlInvalidCharacters(patchString));

                                    patchlist = xdoc.Element("PatchFiles").Element("mandatory").Elements("updateitem").ToList();
                                    //break;
                                }
                            }
                                
//                             while ((entry = zipIn.GetNextEntry()) != null)
//                             {
//                                 foreach (var item in patchlist)
//                                 {
//                                     if (item.Value.ToString() == entry.Name)
//                                     {
//                                         PatchFileInfo newitem = new PatchFileInfo();
//                                         newitem.filepath = item.Value.ToString();
//                                         newitem.filesize = entry.Size;
//                                         retData.PatchList.Add(newitem);
//                                     }
//                                 }
//                             }
// 
//                             if (patchlist.Count() == retData.PatchList.Count() + 1)
                            {
                                result.Data = retData;
                                result.Result = APKERROR.ERR_SUCCESS;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
            }

            return result;
        }

        private string RemoveXmlInvalidCharacters(string s)
        {
            return Regex.Replace(
                s,
                @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]",
                string.Empty);
        }
    }
}
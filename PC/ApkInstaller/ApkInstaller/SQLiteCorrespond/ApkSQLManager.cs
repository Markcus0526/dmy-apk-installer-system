using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApkInstaller.ServiceCorrespond;
using System.IO;
using System.Data.SQLite;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Threading;

namespace ApkInstaller.SQLiteCorrespond
{
    public enum ApkDownStatus
    {
        NONE = 0x00,    //Not exist apk file on local
        NOEXIST_FILE = 0x01,    //Not exist apk file on local
        NOTEXIST_IMAGE = 0x02,  //Not exist apk image on local
        UPDATE_FILE = 0x04,     //New or updated apk
        UPDATE_IMAGE = 0x08,    //New or updated image
        RETRY_FILE = 0x10,      //Continue to download file
        RETRY_IMAGE = 0x20,     //Continue to download image
        LOADING_FILE = 0x40,     //Downloading apk file
        LOADING_IMAGE = 0x80,     //Downloading apk image
        COMPLETED_FILE = 0x100,     //Completed download file
        COMPLETED_IMAGE = 0x200,     //Completed download image
    }

    public class DBApkFileInfo
    {
        public long uid { get; set; }
        public string groupids { get; set; }
        public string name { get; set; }
        public string filepath { get; set; }
        public string srvfilepath { get; set; }
        public string imgpath { get; set; }
        public string srvimgpath { get; set; }
        public long srvfilesize { get; set; }
        public long filesize { get; set; }
        public string version { get; set; }
        public DateTime createtime { get; set; }
        public int showorder { get; set; }
        public ApkDownStatus status { get; set; }

        public string vcode { get; set; }
        public string vname { get; set; }
        public string package { get; set; }
    }

    public delegate void Callback_UpdateApkList(bool res, ApkResponseData response);

    public class ApkSQLManager
    {
        ApkLocalDB db;

        public List<DBApkFileInfo> m_apkList = new List<DBApkFileInfo>();
        public BackgroundWorker m_sqliteWorker;
        public AppControl.ApkWorkerStatus isDone;
        public object lockObj = new object();

        private class UpdateApkParam
        {
            public long groupid { get; set; }
            public ApkResponseData response { get; set; }
            public Callback_UpdateApkList callback { get; set; }
        }

        public ApkSQLManager()
        {
            Properties.Settings.Default["apkinstallerConnectionString"] = "data source=\"" + Program.LOCAL_APP_PATH + "\\" + Program.LOCALDB_PATH + "\"";

            if (!File.Exists(Program.LOCAL_APP_PATH + "\\" + Program.LOCALDB_PATH))
            {
                if (File.Exists(Program.PROGRAM_PATH + "\\" + Program.LOCALDB_PATH)) {
                    try
                    {
                        if (!Directory.Exists(Program.LOCAL_APP_PATH + "\\DB"))
                        {
                            Directory.CreateDirectory(Program.LOCAL_APP_PATH + "\\DB");
                        }
                        File.Copy(Program.PROGRAM_PATH + "\\" + Program.LOCALDB_PATH, Program.LOCAL_APP_PATH + "\\" + Program.LOCALDB_PATH);
                    }
                    catch (System.Exception ex)
                    {
                        ApkDataModel.WriteLogFile("ApkSqlManager", "ApkSQLManager()", ex.ToString());                    	
                    }
                }
            }

            db = new ApkLocalDB();
            isDone = AppControl.ApkWorkerStatus.FINISHED;
        }

        public bool AddApkGroup(ApkGroupInfo groupinfo)
        {
            return true;
        }

        public void CallUpdateApkListWorker(long groupid, ApkResponseData respData, Callback_UpdateApkList callback)
        {
            m_sqliteWorker = new BackgroundWorker();
            m_sqliteWorker.WorkerReportsProgress = true;
            m_sqliteWorker.WorkerSupportsCancellation = true;

            m_sqliteWorker.DoWork += new DoWorkEventHandler(UpdateApkList_DoWork);

            //m_sqliteWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateApkList_ProgressChanged);
            m_sqliteWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateApkList_RunWorkerCompleted);

            m_sqliteWorker.RunWorkerAsync(new UpdateApkParam
            {
                groupid = groupid,
                response = respData,
                callback = callback
            });
        }

        private void UpdateApkList_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateApkParam sparam = (UpdateApkParam)e.Argument;
            List<ApkFileInfo> apks = new List<ApkFileInfo>();
            isDone = AppControl.ApkWorkerStatus.BUSY;
            try
            {
                apks = JsonConvert.DeserializeObject<List<ApkFileInfo>>(sparam.response.SVCC_DATA.ToString());
            }
            catch (System.Exception ex)
            {
                return;	
            }

            string appCachePath = Program.LOCAL_APP_PATH + "\\" + Program.APP_CACHE_PATH;
            string imgCachePath = Program.LOCAL_APP_PATH + "\\" + Program.IMG_CACHE_PATH;
            List<long> uids = apks.Select(m => m.Id).ToList();

            lock (lockObj)
            {
                using (ApkLocalDBTableAdapters.tbl_apkinfoTableAdapter t = new ApkLocalDBTableAdapters.tbl_apkinfoTableAdapter())
                {
                    try
                    {
                        //Monitor.Enter(m_apkList);
                        t.Fill(db.tbl_apkinfo);

                        t.Connection.Open();
                        t.Transaction = t.Connection.BeginTransaction();
                        /* Delete all of the deleted apks from server in the local cache db. */
                        var delRows = (from a in db.tbl_apkinfo
                                       where uids.Contains(a.uid) == false && a.groupids.Split(',').Contains(sparam.groupid.ToString())
                                       select a).ToList();

                        foreach (ApkLocalDB.tbl_apkinfoRow row in delRows)
                        {
                            row.Delete();
                        }
                        db.tbl_apkinfo.AcceptChanges();

                        m_apkList.Clear();
                        /* Check apk status and update local cache db */
                        foreach (ApkFileInfo srvApk in apks)
                        {
                            if (this.m_sqliteWorker.CancellationPending == true)
                            {
                                m_apkList.Clear();
                                e.Cancel = true;
                                break;
                            }
                            DBApkFileInfo newitem = new DBApkFileInfo();
                            ApkLocalDB.tbl_apkinfoRow row = db.tbl_apkinfo.Where(m => m.uid == srvApk.Id).FirstOrDefault();
                            string[] paths = srvApk.FilePath.Split('/');

                            if (row != null)    //Already exist on local
                            {
                                if (row.srvfilepath != sparam.response.SVCC_BASEURL + srvApk.FilePath || !File.Exists(row.filepath))
                                {
                                    newitem.status |= ApkDownStatus.UPDATE_FILE;
                                    newitem.filepath = row.filepath = appCachePath + paths[paths.Count() - 1];
                                }
                                else
                                {
                                    FileInfo finfo = new FileInfo(row.filepath);

                                    if (finfo.Length == srvApk.FileSize)
                                    {
                                        newitem.status |= ApkDownStatus.COMPLETED_FILE;

                                        ApkAdbParam apkinfo = ApkParser.GetApkInformation(row.filepath);
                                        newitem.vcode = apkinfo.vcode;
                                        newitem.vname = apkinfo.vname;
                                        newitem.package = apkinfo.package;
                                    }
                                    else
                                    {
                                        newitem.status |= ApkDownStatus.RETRY_FILE;
                                    }

                                    newitem.filesize = (long)srvApk.FileSize;
                                    newitem.filepath = row.filepath;
                                }

                                if (row.srvimgpath != sparam.response.SVCC_BASEURL + srvApk.ImgPath || !File.Exists(row.imgpath))
                                {
                                    paths = srvApk.ImgPath.Split('/');
                                    newitem.status |= ApkDownStatus.UPDATE_IMAGE;
                                    newitem.imgpath = row.imgpath = imgCachePath + paths[paths.Count() - 1];
                                }
                                else
                                {
                                    newitem.imgpath = row.imgpath;

                                    FileStream stream = new FileStream(row.imgpath, FileMode.Open, FileAccess.Read);

                                    try
                                    {
                                        Image imginfo = Image.FromStream(stream);
                                        newitem.status |= ApkDownStatus.COMPLETED_IMAGE;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        newitem.status |= ApkDownStatus.RETRY_IMAGE;
                                    }

                                    stream.Close();

                                    newitem.imgpath = row.imgpath;
                                }

                                newitem.uid = row.uid = srvApk.Id;
                                newitem.groupids = row.groupids = srvApk.GroupId;
                                newitem.name = row.name = srvApk.Name;
                                newitem.srvfilepath = row.srvfilepath = sparam.response.SVCC_BASEURL + srvApk.FilePath;
                                newitem.srvimgpath = row.srvimgpath = sparam.response.SVCC_BASEURL + srvApk.ImgPath;
                                newitem.srvfilesize = row.srvfilesize = srvApk.FileSize == null ? 0 : (long)srvApk.FileSize;
                                newitem.version = row.version = srvApk.Version;
                                newitem.filesize = newitem.srvfilesize;
                                //newitem.createtime = srvApk.CreateTime;
                            }
                            else
                            {
                                row = (ApkLocalDB.tbl_apkinfoRow)db.tbl_apkinfo.NewRow();
                                newitem.status |= ApkDownStatus.UPDATE_FILE | ApkDownStatus.UPDATE_FILE;
                                newitem.uid = row.uid = srvApk.Id;
                                newitem.groupids = row.groupids = srvApk.GroupId;
                                newitem.name = row.name = srvApk.Name;
                                newitem.srvfilepath = row.srvfilepath = sparam.response.SVCC_BASEURL + srvApk.FilePath;
                                newitem.srvimgpath = row.srvimgpath = sparam.response.SVCC_BASEURL + srvApk.ImgPath;
                                newitem.srvfilesize = row.srvfilesize = srvApk.FileSize == null ? 0 : (long)srvApk.FileSize;
                                newitem.version = row.version = srvApk.Version;
                                newitem.filepath = row.filepath = appCachePath + paths[paths.Count() - 1];
                                paths = srvApk.ImgPath.Split('/');
                                newitem.imgpath = row.imgpath = imgCachePath + paths[paths.Count() - 1];
                                row.showorder = 1;
                                newitem.showorder = (int)row.showorder;

                                if (this.m_sqliteWorker.CancellationPending == true)
                                {
                                    m_apkList.Clear();
                                    e.Cancel = true;
                                    break;
                                }
                                db.tbl_apkinfo.Addtbl_apkinfoRow(row);
                            }
                            m_apkList.Add(newitem);
                        }

                        t.Update(db.tbl_apkinfo);
                        t.Transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        m_apkList.Clear();
                        t.Transaction.Rollback();
                        ApkDataModel.WriteLogFile("ApkSqlManager", "UpdateApkList()", ex.ToString());
                    }
                    finally
                    {
                        t.Connection.Clone();
                        //Monitor.Exit(m_apkList);
                    }
                }
            }

            e.Result = Tuple.Create<bool, ApkResponseData, Callback_UpdateApkList>(e.Cancel, sparam.response, sparam.callback);
        }

        private void UpdateApkList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    m_apkList.Clear();
                }
                else if (e.Error != null)
                {
                    m_apkList.Clear();
                    throw e.Error;
                }
                else
                {
                    Tuple<bool, ApkResponseData, Callback_UpdateApkList> res = e.Result as Tuple<bool, ApkResponseData, Callback_UpdateApkList>;
                    res.Item3(res.Item1, res.Item2);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("ApkSqlManager", "UpdateApkList_RunWorkerCompleted()", ex.ToString());
            }

            isDone = AppControl.ApkWorkerStatus.FINISHED;
        }

        public List<DBApkFileInfo> GetApkList()
        {

            List<DBApkFileInfo> retlist = new List<DBApkFileInfo>();

            ApkLocalDBTableAdapters.tbl_apkinfoTableAdapter t = new ApkLocalDBTableAdapters.tbl_apkinfoTableAdapter();

            t.Fill(db.tbl_apkinfo);

            /*
            public long uid { get; set; }
        public string groupids { get; set; }
        public string name { get; set; }
        public string filepath { get; set; }
        public string srvfilepath { get; set; }
        public string imgpath { get; set; }
        public string srvimgpath { get; set; }
        public long srvfilesize { get; set; }
        public long filesize { get; set; }
        public string version { get; set; }
        public DateTime createtime { get; set; }
        public int showorder { get; set; }
        public ApkDownStatus status { get; set; }
            */

            retlist = (from m in db.tbl_apkinfo
                       select new DBApkFileInfo
                       {
                           uid = m.uid,
                           groupids = m.groupids,
                           name = m.name,
                           filepath = m.filepath,
                           srvfilepath = m.srvfilepath,
                           imgpath = m.imgpath,
                           srvimgpath = m.srvimgpath,
                           srvfilesize = m.srvfilesize,                           
                       }).ToList();
            
            return retlist;
        }
    }
}
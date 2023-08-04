using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;
using ManageSite.Models.Library;
using System.Collections.Specialized;

namespace ManageSite.Models
{
    #region apk model
    public class ApkInfo
    {
        public long uid { get; set; }
        public string groupids { get; set; }
        public string name { get; set; }
        public string filepath { get; set; }
        public string imgpath { get; set; }
        public long? filesize { get; set; }
        public string version { get; set; }
        public DateTime createtime { get; set; }

        public string groupname { get; set; }
    }

    public class ApkDownLog
    {
        public long uid { get; set; }
        public long userid { get; set; }
        public long apkid { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public byte result { get; set; }

        public string username { get; set; }
        public string apkname { get; set; }
    }

    public class ApkInstallLog
    {
        public long uid { get; set; }
        public long userid { get; set; }
        public long? apkid { get; set; }
        public long telid { get; set; }
        public InstallLogType logtype { get; set; }
        public InstallLogStatus logstatus { get; set; }
        public string imei { get; set; }
        public string note { get; set; }
        public string clientver { get; set; }
        public string remoteipaddr { get; set; }
        public DateTime createtime { get; set; }

        public string username { get; set; }
        public string apkname { get; set; }
        public string telname { get; set; }
    }

    public enum InstallLogType
    {
        NEWINSTALL,
        UNINSTALL,
        UPGRADE
    }

    public enum InstallLogStatus
    {
        SUCCESS,
        FAIL
    }

    public class APK_SUBMITSTATUS
    {
        public const string DUPLICATE_NAME = "操作失败： 安装包名称重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }
    #endregion

    public class ApkModel
    {
        SqlDBDataContext db = new SqlDBDataContext();

        #region Public Methods

        public string GetLogType(InstallLogType ltype)
        {
            string ret = "";
            switch (ltype) {
                case InstallLogType.NEWINSTALL:
                    ret = "安装";
                    break;
                case InstallLogType.UPGRADE:
                    ret = "更新";
                    break;
                case InstallLogType.UNINSTALL:
                    ret = "卸载";
                    break;
                default:
                    break;
            }

            return ret;
        }

        public string GetLogStatus(InstallLogStatus ltype)
        {
            string ret = "";
            switch (ltype)
            {
                case InstallLogStatus.SUCCESS:
                    ret = "成功";
                    break;
                case InstallLogStatus.FAIL:
                    ret = "失败";
                    break;
                default:
                    break;
            }

            return ret;
        }
        public IEnumerable<ApkInfo> GetApkList()
        {
            List<ApkInfo> retList = null;

            retList = db.tbl_apkinfos
                .Where(p => p.deleted == 0)
                //.Join(db.tbl_apkgroups, m => m.groupid, l => l.uid, (m, l) => new { apk = m, apkgroup = l})
                .OrderBy(p => p.name)
                .Select(row => new ApkInfo
                {
                    uid = row.uid,
                    groupids = row.groupids,
                    name = row.name,
                    filepath = row.filepath,
                    imgpath = row.imgpath,
                    filesize = row.filesize,
                    version = row.version,
                    createtime = row.createtime
                })
                .ToList();
            return retList;
        }

        public ApkInfo GetApkInfo(long uid)
        {
            ApkInfo userlist = (from row in db.tbl_apkinfos
                                    where (row.uid == uid)
                                select new ApkInfo
                                    {
                                        uid = row.uid,
                                        groupids = row.groupids,
                                        name = row.name,
                                        filepath = row.filepath,
                                        imgpath = row.imgpath,
                                        filesize = row.filesize,
                                        version = row.version,
                                        createtime = row.createtime
                                    }).SingleOrDefault();

            return userlist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_apkinfo SET deleted = 1 WHERE ";
            string whereSql = "";
            foreach (long uid in items)
            {
                if (whereSql != "") whereSql += " OR";
                whereSql += " uid = " + uid;
            }

            delSql += whereSql;

            db.ExecuteCommand(delSql);

            return true;
        }

        public bool DeleteItem(long uid)
        {
            tbl_apkinfo delitem = (from m in db.tbl_apkinfos
                                    where m.deleted == 0 && m.uid == uid
                                    select m).FirstOrDefault();

            if (delitem != null)
            {
                delitem.deleted = 1;
                db.SubmitChanges();
            }

            return true;
        }

        public string ValidateUserData(long uid, string name)
        {
//             if (uid == 0)   //user add case
//             {
//                 var existname = (from m in db.tbl_apkinfos
//                                  where (m.deleted == 0) && (m.name == name)
//                                  select m).FirstOrDefault();
//                 if (existname != null)
//                 {
//                     return APK_SUBMITSTATUS.DUPLICATE_NAME;
//                 }
//             }
//             else //user edit case
//             {
//                 var existname1 = (from m in db.tbl_apkinfos
//                                   where (m.deleted == 0) && (m.name == name) && (m.uid != uid)
//                                   select m).FirstOrDefault();
//                 if (existname1 != null)
//                 {
//                     return APK_SUBMITSTATUS.DUPLICATE_NAME;
//                 }
//             }

            return APK_SUBMITSTATUS.SUCCESS_SUBMIT;
        }

        public string InsertItem(string groupids, string name, string filepath, string imgpath, string version)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            /* Check if same name already exists. */
            string validateStr = ValidateUserData(0, name);

            if (validateStr != APK_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apkinfo newitem = new tbl_apkinfo();

                newitem.groupids = groupids;
                newitem.name = name;
                newitem.filepath = filepath;
                newitem.imgpath = imgpath;
                newitem.filepath = MoveApkFile(filepath);

                if (File.Exists(rootpath + newitem.filepath))
                {
                    FileInfo f = new FileInfo(rootpath + newitem.filepath);
                    newitem.filesize = f.Length;
                }

                newitem.version = version;
                newitem.createtime = DateTime.Today;
                newitem.showorder = 1;

                db.tbl_apkinfos.InsertOnSubmit(newitem);
                db.SubmitChanges();

                return APK_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("ApkModel", "InsertItem()", e.ToString());
                return APK_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, string groupids, string name, string filepath, string imgpath, string version)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string validateStr = ValidateUserData(uid, name);

            if (validateStr != APK_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apkinfo edititem = (from m in db.tbl_apkinfos
                                         where m.deleted == 0 && m.uid == uid
                                         select m).FirstOrDefault();
                if (edititem != null)
                {
                    edititem.name = name;
                    edititem.groupids = groupids;
                    edititem.imgpath = imgpath;

                    if (edititem.filepath != filepath)
                    {
                        edititem.filepath = MoveApkFile(filepath);
                    }
                    if (File.Exists(rootpath + edititem.filepath))
                    {
                        FileInfo f = new FileInfo(rootpath + edititem.filepath);
                        edititem.filesize = f.Length;
                    }
                    edititem.version = version;
                }

                db.SubmitChanges();

                return APK_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("ApkModel", "UpdateItem()", e.ToString());
                return APK_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string GetApkGroupName(List<tbl_apkgroup> groups, string gids)
        {
            string groupname = "";
            string[] groupids = gids.Split(',');

            try
            {
                foreach (string gitem in groupids)
                {
                    long gid = long.Parse(gitem);
                    tbl_apkgroup item = (from m in groups
                                         where m.uid == gid
                                         select m).FirstOrDefault();

                    if (item != null)
                    {
                        if (!String.IsNullOrEmpty(groupname))
                        {
                            groupname += "， ";
                        }
                        groupname += item.name;
                    }
                }
                return groupname;
            }
            catch
            {
                return "";
            }
        }

        public JqDataTableInfo GetApkDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<ApkInfo> filteredCompanies;

            try
            {
                var alllist = GetApkList();

                List<tbl_apkgroup> groups = (from m in db.tbl_apkgroups
                                             where m.deleted == 0
                                             select m).ToList();

                foreach (var item in alllist)
                {
                    item.groupname = GetApkGroupName(groups, item.groupids);
                }
                //Check whether the companies should be filtered by keyword
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    //Used if particulare columns are filtered 
                    var groupFilter = Convert.ToString(Request["sSearch_1"]);
                    var nameFilter = Convert.ToString(Request["sSearch_2"]);

                    //Optionally check whether the columns are searchable at all 
                    var isGroupSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                    var isNameSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                    filteredCompanies = alllist
                       .Where(c => isGroupSearchable && c.groupname.ToLower().Contains(param.sSearch.ToLower()) ||
                              isNameSearchable && c.name.ToLower().Contains(param.sSearch.ToLower()));
                }
                else
                {
                    filteredCompanies = alllist;
                }

                var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ApkInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.name :
                                                               "");
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == "asc")
                    filteredCompanies = filteredCompanies.OrderBy(orderingFunction);
                else
                    filteredCompanies = filteredCompanies.OrderByDescending(orderingFunction);

                var displayedCompanies = filteredCompanies.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayedCompanies
                             select new[] { 
                    Convert.ToString(c.uid), 
                    c.groupname,
                    "<a href='" + rootUri + "Apk/Add/" + c.uid + "'>" + c.name + "</a>", 
                    "<img src='" + rootUri + c.imgpath + "' alt='" + c.name + "'/>",
                    Math.Round((decimal)c.filesize / 1048576, 2).ToString() + " MB",
                    c.version

                };

                rst.sEcho = param.sEcho;
                rst.iTotalRecords = alllist.Count();
                rst.iTotalDisplayRecords = filteredCompanies.Count();
                rst.aaData = result;
            }
            catch (System.Exception ex)
            {
                CommonModel.WriteLogFile("ApkModel", "MoveApkFile()", ex.ToString());	
            }


            return rst;
        }

        public IEnumerable<ApkDownLog> GetApkDownLogList()
        {
            List<ApkDownLog> retList = null;

            retList = db.tbl_apkdownlogs
                .Where(p => p.deleted == 0)
                .Join(db.tbl_users, m => m.userid, l => l.uid, (m, l) => new { downlog = m, user = l })
                .Join(db.tbl_apkinfos, m => m.downlog.apkid, l => l.uid, (m, l) => new { downlog = m, apk = l })
                .OrderByDescending(p => p.downlog.downlog.downstartdate)
                .Select(row => new ApkDownLog
                {
                    uid = row.downlog.downlog.uid,
                    userid = row.downlog.user.uid,
                    apkid = row.apk.uid,
                    username = row.downlog.user.username,
                    apkname = row.apk.name,
                    starttime = row.downlog.downlog.downstartdate,
                    endtime = row.downlog.downlog.downenddate,
                    result = row.downlog.downlog.result
                })
                .ToList();
            return retList;
        }

        public JqDataTableInfo GetApkDownLogDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<ApkDownLog> filteredCompanies;

            var alllist = GetApkDownLogList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var usernameFilter = Convert.ToString(Request["sSearch_1"]);
                var apkFilter = Convert.ToString(Request["sSearch_2"]);

                //Optionally check whether the columns are searchable at all 
                var isUsernameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isApkSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                filteredCompanies = alllist
                   .Where(c => isUsernameSearchable && c.username.ToLower().Contains(param.sSearch.ToLower()) ||
                          isApkSearchable && c.apkname.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var isApkSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<ApkDownLog, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.username :
                                                           sortColumnIndex == 2 && isApkSortable ? c.apkname :
                                                           "");

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredCompanies = filteredCompanies.OrderBy(orderingFunction);
            else
                filteredCompanies = filteredCompanies.OrderByDescending(orderingFunction);

            var displayedCompanies = filteredCompanies.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCompanies
                         select new[] { 
                Convert.ToString(c.uid), 
                c.username,
                c.apkname,
                c.starttime == null ? "--" : String.Format("{0:yyyy-MM-dd HH:mm:ss}", c.starttime),
                c.endtime == null ? "--" : String.Format("{0:yyyy-MM-dd HH:mm:ss}", c.endtime),
                CommonModel.GetOperationForeignList().Where(m => m.Id == c.result).FirstOrDefault().Name

            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }

        public bool DeleteSelectedDownLogItems(long[] items)
        {
            string delSql = "UPDATE tbl_apkdownlog SET deleted = 1 WHERE ";
            string whereSql = "";
            foreach (long uid in items)
            {
                if (whereSql != "") whereSql += " OR";
                whereSql += " uid = " + uid;
            }

            delSql += whereSql;

            db.ExecuteCommand(delSql);

            return true;
        }

        public IEnumerable<ApkInstallLog> GetApkInstallLogList()
        {
            List<ApkInstallLog> retList = null;

            retList = db.tbl_apkinstalllogs
                .Where(p => p.deleted == 0)
                .Join(db.tbl_users, m => m.userid, l => l.uid, (m, l) => new { installlog = m, user = l })
                //.Join(db.tbl_apkinfos, m => m.installlog.apkid, l => l.uid, (m, l) => new { log = m, apk = l })
                .Join(db.tbl_telinfos, m => m.installlog.telid, l => l.uid, (m, l) => new { log = m, tel = l })
                .OrderByDescending(p => p.log.installlog.createtime)
                .Select(row => new ApkInstallLog
                {
                    uid = row.log.installlog.uid,
                    userid = row.log.user.uid,
                    apkid = row.log.installlog.apkid,
                    telid = row.tel.uid,
                    username = row.log.user.username,
                    apkname = (row.log.installlog.apkname != null) ? row.log.installlog.apkname : " ",
                    telname = row.tel.type + "-" + row.tel.vendor,
                    logtype = (InstallLogType)row.log.installlog.logtype,
                    logstatus = (InstallLogStatus)row.log.installlog.status,
                    imei = row.tel.imei,
                    note = row.log.installlog.note,
                    clientver = row.log.installlog.clientver,
                    remoteipaddr = row.log.installlog.remoteip,
                    createtime = row.log.installlog.createtime,
                }).ToList();

            List<tbl_apkinfo> apklist = (from m in db.tbl_apkinfos
                                         where m.deleted == 0
                                         select m).ToList();

            foreach (ApkInstallLog item in retList)
            {
                if (item.apkid != null)
                {
                    item.apkname = apklist.Where(m => m.uid == item.apkid).Select(m => m.name).FirstOrDefault();
                    if (item.apkname == null)
                    {
                        item.apkname = "";
                    }
                }
            }

            return retList;
        }

        public JqDataTableInfo GetApkInstallLogDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<ApkInstallLog> filteredCompanies = new List<ApkInstallLog>();

            var alllist = GetApkInstallLogList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var usernameFilter = Convert.ToString(Request["sSearch_1"]);
                var typeFilter = Convert.ToString(Request["sSearch_2"]);
                var apkFilter = Convert.ToString(Request["sSearch_3"]);
                var telFilter = Convert.ToString(Request["sSearch_4"]);
                var imeiFilter = Convert.ToString(Request["sSearch_5"]);
                var statusFilter = Convert.ToString(Request["sSearch_6"]);
                var noteFilter = Convert.ToString(Request["sSearch_7"]);

                //Optionally check whether the columns are searchable at all 
                var isUsernameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isTypeSearchable = Convert.ToBoolean(Request["bSearchable_2"]);
                var isApkSearchable = Convert.ToBoolean(Request["bSearchable_3"]);
                var isTelSearchable = Convert.ToBoolean(Request["bSearchable_4"]);
                var isImeiSearchable = Convert.ToBoolean(Request["bSearchable_5"]);
                var isStatusSearchable = Convert.ToBoolean(Request["bSearchable_6"]);
                var isNoteSearchable = Convert.ToBoolean(Request["bSearchable_7"]);

                try
                {
                    filteredCompanies = alllist
                       .Where(c => isUsernameSearchable && c.username.ToLower().Contains(param.sSearch.ToLower()) ||
                              //isTypeSearchable && GetLogType(c.logtype).Contains(param.sSearch.ToLower()) ||
                              isApkSearchable && c.apkname.ToLower().Contains(param.sSearch.ToLower()) ||
                              isTelSearchable && c.telname.ToLower().Contains(param.sSearch.ToLower()) ||
                              isImeiSearchable && (c.imei != null ?c.imei.ToLower().Contains(param.sSearch.ToLower()) : false ) ||
                              isStatusSearchable && GetLogStatus(c.logstatus).Contains(param.sSearch.ToLower()) ||
                              isNoteSearchable && (c.note != null ? c.note.ToLower().Contains(param.sSearch.ToLower()) : false)
                        )
                        .ToList();
                }
                catch (System.Exception ex)
                {
                }

                bool ww = filteredCompanies.Any();
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var isTypeSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var isApkSortable = Convert.ToBoolean(Request["bSortable_3"]);
            var isTelSortable = Convert.ToBoolean(Request["bSortable_4"]);
            var isStatusSortable = Convert.ToBoolean(Request["bSortable_5"]);
            var isNoteSortable = Convert.ToBoolean(Request["bSortable_6"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<ApkInstallLog, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.username :
                                                           sortColumnIndex == 2 && isTypeSortable ? GetLogType(c.logtype) :
                                                           sortColumnIndex == 3 && isApkSortable ? c.apkname :
                                                           sortColumnIndex == 4 && isTelSortable ? c.telname :
                                                           sortColumnIndex == 5 && isStatusSortable ? GetLogStatus(c.logstatus) :
                                                           sortColumnIndex == 6 && isNoteSortable ? c.note :
                                                           "");

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            if (sortDirection == "asc")
                filteredCompanies = filteredCompanies.OrderBy(orderingFunction);
            else
                filteredCompanies = filteredCompanies.OrderByDescending(orderingFunction);

            var displayedCompanies = filteredCompanies.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedCompanies
                         select new[] { 
                Convert.ToString(c.uid), 
                c.username,
                GetLogType(c.logtype),
                c.apkname,
                c.telname,
                c.imei,
                "<span class='label " + (c.logstatus == InstallLogStatus.SUCCESS ? "label-success" : "label-error") + "'>" + GetLogStatus(c.logstatus) + "</span>",
                "版本: " + c.clientver + "<br/>IP: " + c.remoteipaddr,
                c.note,
                String.Format("{0:yyyy-MM-dd HH:mm:ss}", c.createtime)
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();

            try
            {
                rst.iTotalDisplayRecords = filteredCompanies.Count();
            }
            catch (System.Exception ex)
            {
            	
            }
            rst.aaData = result;

            return rst;
        }

        public bool DeleteSelectedInstallLogItems(long[] items)
        {
            string delSql = "UPDATE tbl_apkinstalllog SET deleted = 1 WHERE ";
            string whereSql = "";
            foreach (long uid in items)
            {
                if (whereSql != "") whereSql += " OR";
                whereSql += " uid = " + uid;
            }

            delSql += whereSql;

            db.ExecuteCommand(delSql);

            return true;
        }

        #endregion

        #region Private Methods
        private string MoveApkFile(string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string targetpath = "";
            string[] fileArr = filepath.Split('.');

            if (fileArr.Count() > 1)
            {

                if (File.Exists(rootpath + filepath))
                {
                    targetpath = "Content/uploads/file/apk/" + String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                    targetpath += "." + fileArr[fileArr.Count() - 1];
                    try
                    {
                        File.Move(rootpath + filepath, rootpath + targetpath);
                    }
                    catch (System.Exception ex)
                    {
                        CommonModel.WriteLogFile("ApkModel", "MoveApkFile()", ex.ToString());
                    }
                }
            }
            return targetpath;
        }
        #endregion
    }
}
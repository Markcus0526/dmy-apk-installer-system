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
    #region Driver Model
    public class DriverInfo
    {
        public long uid { get; set; }
        public long telid { get; set; }
        public string name { get; set; }
        public string sysver { get; set; }
        public string filepath { get; set; }
        
        public string telname { get; set; }
    }

    public class DriverDownLog
    {
        public long uid { get; set; }
        public long userid { get; set; }
        public long driverid { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public byte result { get; set; }

        public string username { get; set; }
        public string drivername { get; set; }
    }

    public class DRIVER_SUBMITSTATUS
    {
        public const string DUPLICATE_NAME = "操作失败： 安装包名称重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }
    #endregion

    public class DriverModel
    {
        SqlDBDataContext db = new SqlDBDataContext();

        #region Public Methods
        public IEnumerable<DriverInfo> GetDriverList()
        {
            List<DriverInfo> retList = null;

            retList = db.tbl_driverinfos
                .Where(p => p.deleted == 0)
                .Join(db.tbl_telinfos, m => m.telid, l => l.uid, (m, l) => new { driver = m, tel = l })
                .OrderBy(p => p.driver.name)
                .Select(row => new DriverInfo
                {
                    uid = row.driver.uid,
                    telid = row.tel.uid,
                    name = row.driver.name,
                    sysver = row.driver.sysver,
                    filepath = row.driver.filepath,
                    telname = row.tel.vendor + row.tel.type
                })
                .ToList();
            return retList;
        }

        public DriverInfo GetDriverInfo(long uid)
        {
            DriverInfo userlist = (from row in db.tbl_driverinfos
                                where (row.uid == uid)
                                select new DriverInfo
                                {
                                    uid = row.uid,
                                    telid = row.telid,
                                    name = row.name,
                                    sysver = row.sysver,
                                    filepath = row.filepath
                                }).SingleOrDefault();

            return userlist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_driverinfo SET deleted = 1 WHERE ";
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

        public string ValidateUserData(long uid, string name)
        {
            if (uid == 0)   //user add case
            {
                var existname = (from m in db.tbl_driverinfos
                                 where (m.deleted == 0) && (m.name == name)
                                 select m).FirstOrDefault();
                if (existname != null)
                {
                    return DRIVER_SUBMITSTATUS.DUPLICATE_NAME;
                }
            }
            else //user edit case
            {
                var existname1 = (from m in db.tbl_driverinfos
                                  where (m.deleted == 0) && (m.name == name) && (m.uid != uid)
                                  select m).FirstOrDefault();
                if (existname1 != null)
                {
                    return DRIVER_SUBMITSTATUS.DUPLICATE_NAME;
                }
            }

            return DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT;
        }

        public string InsertItem(long telid, string name, string sysver, string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            /* Check if same name already exists. */
            string validateStr = ValidateUserData(0, name);

            if (validateStr != DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_driverinfo newitem = new tbl_driverinfo();

                newitem.telid = telid;
                newitem.name = name;
                newitem.sysver = sysver;
                newitem.filepath = MoveDriverFile(filepath);

                newitem.createtime = DateTime.Today;

                db.tbl_driverinfos.InsertOnSubmit(newitem);
                db.SubmitChanges();

                return DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("DriverModel", "InsertItem()", e.ToString());
                return DRIVER_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, long telid, string name, string sysver, string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string validateStr = ValidateUserData(uid, name);

            if (validateStr != DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_driverinfo edititem = (from m in db.tbl_driverinfos
                                        where m.deleted == 0 && m.uid == uid
                                        select m).FirstOrDefault();
                if (edititem != null)
                {
                    edititem.telid = telid;
                    edititem.name = name;
                    edititem.sysver = sysver;

                    if (edititem.filepath != filepath)
                    {
                        edititem.filepath = MoveDriverFile(filepath);
                    }
                }

                db.SubmitChanges();

                return DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("DriverModel", "UpdateItem()", e.ToString());
                return DRIVER_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public JqDataTableInfo GetDriverDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<DriverInfo> filteredCompanies;

            var alllist = GetDriverList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var telFilter = Convert.ToString(Request["sSearch_1"]);
                var nameFilter = Convert.ToString(Request["sSearch_2"]);

                //Optionally check whether the columns are searchable at all 
                var isTelSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                filteredCompanies = alllist
                   .Where(c => isTelSearchable && c.telname.ToLower().Contains(param.sSearch.ToLower()) ||
                          isNameSearchable && c.name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<DriverInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.name :
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
                c.telname,
                c.name,
                c.sysver,
                Convert.ToString(c.uid), 
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }

        public IEnumerable<DriverDownLog> GetDriverDownLogList()
        {
            List<DriverDownLog> retList = null;

            retList = db.tbl_driverdownlogs
                .Where(p => p.deleted == 0)
                .Join(db.tbl_users, m => m.userid, l => l.uid, (m, l) => new { downlog = m, user = l })
                .Join(db.tbl_driverinfos, m => m.downlog.driverid, l => l.uid, (m, l) => new { downlog = m, driver = l })
                .OrderByDescending(p => p.downlog.downlog.starttime)
                .Select(row => new DriverDownLog
                {
                    uid = row.downlog.downlog.uid,
                    userid = row.downlog.user.uid,
                    driverid = row.driver.uid,
                    username = row.downlog.user.username,
                    drivername = row.driver.name,
                    starttime = row.downlog.downlog.starttime,
                    endtime = row.downlog.downlog.endtime,
                    result = row.downlog.downlog.result
                })
                .ToList();
            return retList;
        }

        public JqDataTableInfo GetDriverDownLogDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<DriverDownLog> filteredCompanies;

            var alllist = GetDriverDownLogList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var usernameFilter = Convert.ToString(Request["sSearch_1"]);
                var driverFilter = Convert.ToString(Request["sSearch_2"]);

                //Optionally check whether the columns are searchable at all 
                var isUsernameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isDriverSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                filteredCompanies = alllist
                   .Where(c => isUsernameSearchable && c.username.ToLower().Contains(param.sSearch.ToLower()) ||
                          isDriverSearchable && c.drivername.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var isDriverSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<DriverDownLog, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.username :
                                                           sortColumnIndex == 2 && isDriverSortable ? c.drivername :
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
                c.drivername,
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
            string delSql = "UPDATE tbl_driverdownlog SET deleted = 1 WHERE ";
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
        private string MoveDriverFile(string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string targetpath = "";
            string[] fileArr = filepath.Split('.');

            if (fileArr.Count() > 1)
            {

                if (File.Exists(rootpath + filepath))
                {
                    targetpath = "Content/uploads/file/driver/" + String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                    targetpath += "." + fileArr[fileArr.Count() - 1];
                    try
                    {
                        File.Move(rootpath + filepath, rootpath + targetpath);
                    }
                    catch (System.Exception ex)
                    {
                        CommonModel.WriteLogFile("DriverModel", "MoveDriverFile()", ex.ToString());
                    }
                }
            }
            return targetpath;
        }
        #endregion
    }
}
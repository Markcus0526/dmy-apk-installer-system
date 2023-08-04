using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ManageSite.Models.Library;
using System.Collections.Specialized;

namespace ManageSite.Models
{
    #region PhoneModels
    public class PhoneInfo
    {
        public long uid { get; set; }
        public string type { get; set; }
        public string vendor { get; set; }
        public string sysver { get; set; }
        public string serial { get; set; }
        public string imei { get; set; }
    }

    public class PhoneAppInfo
    {
        public long uid { get; set; }
        public string appname { get; set; }
        public string appversion { get; set; }
        public string installtime { get; set; }
    }

    public class PHONE_SUBMITSTATUS
    {
        public const string DUPLICATE_LOGINID = "操作失败： 用户账号重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }
    #endregion

    public class PhoneModel
    {
        public IEnumerable<PhoneInfo> GetPhoneList()
        {
            SqlDBDataContext db = new SqlDBDataContext();

            List<PhoneInfo> retList = null;

            CommonModel commonModel = new CommonModel();

            retList = db.tbl_telinfos
                .Where(p => p.deleted == 0)
                .OrderBy(p => p.type)
                .Select(p => new PhoneInfo
                {
                    uid = p.uid,
                    type = p.type,
                    vendor = p.vendor,
                    sysver = p.sysver,
                    serial = p.serial,
                    imei = p.imei
                }).ToList();

            return retList;
        }

        public IEnumerable<PhoneInfo> GetPhoneFilterList(string filter)
        {
            SqlDBDataContext db = new SqlDBDataContext();

            List<tbl_telinfo> retList = null;

            CommonModel commonModel = new CommonModel();

            var results = db.ExecuteQuery<tbl_telinfo>("SELECT * FROM tbl_telinfo WHERE " + filter);

            return results
                .Where(p => p.deleted == 0)
                .OrderBy(p => p.type)
                .Select(p => new PhoneInfo
                {
                    uid = p.uid,
                    type = p.type,
                    vendor = p.vendor,
                    sysver = p.sysver,
                    serial = p.serial,
                    imei = p.imei
                }).ToList();
        }

        public PhoneInfo GetPhoneInfo(long uid)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();
            PhoneInfo phonelist = (from p in db.tbl_telinfos
                                 where (p.uid == uid)
                                 select new PhoneInfo
                                 {
                                     uid = p.uid,
                                     type = p.type,
                                     vendor = p.vendor,
                                     sysver = p.sysver,
                                     serial = p.serial,
                                     imei = p.imei
                                 }).SingleOrDefault();

            return phonelist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_telinfo SET deleted = 1 WHERE ";
            string whereSql = "";
            foreach (long uid in items)
            {
                if (whereSql != "") whereSql += " OR";
                whereSql += " uid = " + uid;
            }

            delSql += whereSql;

            SqlDBDataContext db = CommonModel.GetDBContext();
            db.ExecuteCommand(delSql);

            return true;
        }

        public string InsertItem(string type, string vendor, string sysver, string serial)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            try
            {
                tbl_telinfo newitem = new tbl_telinfo
                {
                    type = type,
                    vendor = vendor,
                    sysver = sysver,
                    serial = serial
                };
                db.tbl_telinfos.InsertOnSubmit(newitem);
                db.SubmitChanges();

                return PHONE_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PhoneModel", "InsertItem()", e.ToString());
                return PHONE_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, string type, string vendor, string sysver, string serial)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            try
            {
                tbl_telinfo edititem = (from m in db.tbl_telinfos
                                        where m.deleted == 0 && m.uid == uid
                                        select m).FirstOrDefault();

                if (edititem == null)
                {
                    return PHONE_SUBMITSTATUS.ERROR_SUBMIT;
                }

                edititem.type = type;
                edititem.vendor = vendor;
                edititem.sysver = sysver;
                edititem.serial = serial;

                db.SubmitChanges();

                return PHONE_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PhoneModel", "UpdateItem()", e.ToString());
                return PHONE_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public JqDataTableInfo GetPhoneDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<PhoneInfo> filteredCompanies;

            var alllist = GetPhoneList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var typeFilter = Convert.ToString(Request["sSearch_1"]);

                //Optionally check whether the columns are searchable at all 
                var isTypeSearchable = Convert.ToBoolean(Request["bSearchable_1"]);

                filteredCompanies = alllist
                   .Where(c => isTypeSearchable && c.type.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isTypeSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<PhoneInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isTypeSortable ? c.type :
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
                c.vendor,
                c.type,
                c.sysver,
                c.serial,
                c.uid.ToString()
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }

        public IEnumerable<PhoneAppInfo> GetPhoneAppList(long telid)
        {
            SqlDBDataContext db = new SqlDBDataContext();

            List<PhoneAppInfo> retList = null;

            CommonModel commonModel = new CommonModel();

            retList = db.tbl_telapps
                .Where(p => p.deleted == 0 && p.telid == telid)
                .OrderBy(p => p.appname)
                .Select(p => new PhoneAppInfo
                {
                    uid = p.uid,
                    appname = p.appname,
                    appversion = p.appversion,
                    installtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", p.installtime)
                }).ToList();

            return retList;
        }
    }
}

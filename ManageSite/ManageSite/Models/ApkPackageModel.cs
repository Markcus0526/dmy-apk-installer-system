using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ManageSite.Models.Library;
using System.Collections.Specialized;

namespace ManageSite.Models
{
    #region Package model
    public class PackageInfo
    {
        public long uid { get; set; }
        public string name { get; set; }
        public string imgpath { get; set; }
        public byte gtype { get; set; }
        public string userids { get; set; }
        public DateTime createtime { get; set; }
    }

    public class PACKAGE_SUBMITSTATUS
    {
        public const string DUPLICATE_NAME = "操作失败： 套餐名称重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }

    public enum ApkPackageType
    {
        PUBLIC,
        PRIVATE
    }
    #endregion

    public class ApkPackageModel
    {
        SqlDBDataContext db = new SqlDBDataContext();

        public IEnumerable<PackageInfo> GetPackageList()
        {
            List<PackageInfo> retList = null;

            retList = db.tbl_apkgroups
                .Where(p => p.deleted == 0)
                .OrderBy(p => p.name)
                .Select(row => new PackageInfo
                {
                    uid = row.uid,
                    name = row.name,
                    gtype = row.gtype,
                    userids = row.userids,
                    imgpath = row.imgpath,
                    createtime = row.createtime
                })
                .ToList();
            return retList;
        }

        public PackageInfo GetPackageInfo(long uid)
        {
            PackageInfo userlist = (from row in db.tbl_apkgroups
                                  where (row.uid == uid)
                                    select new PackageInfo
                                  {
                                      uid = row.uid,
                                      name = row.name,
                                      gtype = row.gtype,
                                      userids = row.userids,
                                      imgpath = row.imgpath,
                                      createtime = row.createtime
                                  }).SingleOrDefault();

            return userlist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_apkgroup SET deleted = 1 WHERE ";
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
            tbl_apkgroup delitem = (from m in db.tbl_apkgroups 
                                    where m.deleted == 0 && m.uid == uid
                                    select m).FirstOrDefault();

            if (delitem != null)
            {
                delitem.deleted = 1;
                db.SubmitChanges();
            }

            return true;
        }

        public string GetGType(ApkPackageType ltype)
        {
            string ret = "";
            switch (ltype)
            {
                case ApkPackageType.PUBLIC:
                    ret = "共有";
                    break;
                case ApkPackageType.PRIVATE:
                    ret = "私有";
                    break;
                default:
                    break;
            }

            return ret;
        }

        public string ValidateUserData(long uid, string name)
        {
            if (uid == 0)   //user add case
            {
                var existname = (from m in db.tbl_apkgroups
                                 where (m.deleted == 0) && (m.name == name)
                                 select m).FirstOrDefault();
                if (existname != null)
                {
                    return PACKAGE_SUBMITSTATUS.DUPLICATE_NAME;
                }
            }
            else //user edit case
            {
                var existname1 = (from m in db.tbl_apkgroups
                                  where (m.deleted == 0) && (m.name == name) && (m.uid != uid)
                                  select m).FirstOrDefault();
                if (existname1 != null)
                {
                    return PACKAGE_SUBMITSTATUS.DUPLICATE_NAME;
                }
            }

            return PACKAGE_SUBMITSTATUS.SUCCESS_SUBMIT;
        }

        public string InsertItem(string name, string gtype, string userids, string imgpath)
        {
            /* Check if same name already exists. */
            string validateStr = ValidateUserData(0, name);

            if (validateStr != PACKAGE_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apkgroup newitem = new tbl_apkgroup();

                newitem.name = name;
                newitem.imgpath = imgpath;
                newitem.createtime = DateTime.Today;
                newitem.userids = userids;
                newitem.gtype = byte.Parse(gtype);

                db.tbl_apkgroups.InsertOnSubmit(newitem);
                db.SubmitChanges();

                return PACKAGE_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PackageModel", "InsertItem()", e.ToString());
                return PACKAGE_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, string name, string gtype, string userids, string imgpath)
        {
            string validateStr = ValidateUserData(uid, name);

            if (validateStr != PACKAGE_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apkgroup edititem = (from m in db.tbl_apkgroups
                                         where m.deleted == 0 && m.uid == uid
                                         select m).FirstOrDefault();
                if (edititem != null)
                {
                    edititem.name = name;
                    edititem.imgpath = imgpath;
                    edititem.gtype = byte.Parse(gtype);
                    edititem.userids = userids;
                }

                db.SubmitChanges();

                return PACKAGE_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PackageModel", "UpdateItem()", e.ToString());
                return PACKAGE_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public JqDataTableInfo GetPackageDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<PackageInfo> filteredCompanies;

            var alllist = GetPackageList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var nameFilter = Convert.ToString(Request["sSearch_1"]);
                var typeFilter = Convert.ToString(Request["sSearch_2"]);

                //Optionally check whether the columns are searchable at all 
                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isTypeSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                filteredCompanies = alllist
                   .Where(c => isNameSearchable && c.name.ToLower().Contains(param.sSearch.ToLower()) ||
                                isTypeSearchable && GetGType((ApkPackageType)c.gtype).Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var isTypeSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<PackageInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.name :
                                                                sortColumnIndex == 2 && isTypeSortable ? GetGType((ApkPackageType)c.gtype) :
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
                "<a href='" + rootUri + "Apk/AddPackage/" + c.uid + "'>" + c.name + "</a>", 
                GetGType((ApkPackageType)c.gtype),
                "<img src='" + rootUri + c.imgpath + "' alt='" + c.name + "'/>"
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }
    }
}
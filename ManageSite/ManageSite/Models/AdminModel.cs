using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Web.Security;
using System.Web.Hosting;
using System.IO;
using ManageSite.Models.Library;
using System.Collections.Specialized;

namespace ManageSite.Models
{
    #region AdminModels
    public class AdminInfo
    {
        public long uid { get; set; }
        public string name { get; set; }
        public string realname { get; set; }
        public string password { get; set; }
        public string mailaddr { get; set; }
        public string role { get; set; }
        public string imgpath { get; set; }
        public byte status { get; set; }
        public DateTime regtime { get; set; }
        public string _status { get; set; }
    }

    public enum ADMINSTATUS
    {
        PENDING,
        APPROVED,
        DISABLED
    }

    public class ADMIN_SUBMITSTATUS
    {
        public const string DUPLICATE_LOGINID = "操作失败： 用户账号重复！";
        public const string DUPLICATE_MAILADDR = "操作失败： 邮件地址重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }
    #endregion

    public class AdminModel
    {

        public static IEnumerable<AdminInfo> GetAdminList()
        {
            SqlDBDataContext db = new SqlDBDataContext();

            List<AdminInfo> retList = null;

            CommonModel commonModel = new CommonModel();

            if (commonModel.CheckUserRoles("Administrator") == true)
            {
                retList = db.tbl_admins
                    .Where(p => p.deleted == 0 && p.name != commonModel.GetCurrentUserName() && p.name != "admin")
                    .OrderBy(p => p.name)
                    .Select(user => new AdminInfo
                    {
                        uid = user.uid,
                        name = user.name.Trim(),
                        realname = user.realname.Trim(),
                        password = user.password,
                        mailaddr = user.mailaddress,
                        role = user.role.Trim(),
                        imgpath = user.imgpath,
                        status = user.status,
                        regtime = user.regtime,
                    }).ToList();
            }
            else
            {
                retList = db.tbl_admins
                    .Where(p => p.deleted == 0 && p.name != "admin")
                    .OrderBy(p => p.name)
                    .Select(user => new AdminInfo
                    {
                        uid = user.uid,
                        name = user.name.Trim(),
                        realname = user.realname.Trim(),
                        password = user.password,
                        mailaddr = user.mailaddress,
                        role = user.role.Trim(),
                        imgpath = user.imgpath,
                        status = user.status,
                        regtime = user.regtime,
                    }).ToList();
            }
            foreach (var item in retList)
            {
                item.role = CommonModel.GetRoleForeignList().Where(r => r.Id == item.role.Trim()).FirstOrDefault().Name;

                string statusicon = "";
                switch (item.status)
                {
                    case (byte)ADMINSTATUS.APPROVED:
                        statusicon = "label-success";
                        break;
                    case (byte)ADMINSTATUS.PENDING:
                        statusicon = "label-warning";
                        break;
                    case (byte)ADMINSTATUS.DISABLED:
                        statusicon = "label-inverse";
                        break;
                }
                item._status = "<span class='label " + statusicon + "'>" + CommonModel.GetAllowForeignList().Where(r => r.Id == item.status).FirstOrDefault().Name + "</span>";
            }

            return retList;
        }

        public static AdminInfo GetAdminInfo(long uid)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();
            AdminInfo userlist = (from user in db.tbl_admins
                                  where (user.uid == uid)
                                  select new AdminInfo
                                  {
                                      uid = user.uid,
                                      name = user.name.Trim(),
                                      realname = user.realname.Trim(),
                                      password = user.password,
                                      mailaddr = user.mailaddress,
                                      role = user.role.Trim(),
                                      status = user.status,
                                      imgpath = user.imgpath,
                                      regtime = user.regtime,
                                  }).SingleOrDefault();

            return userlist;
        }

        public static bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_admin SET deleted = 1 WHERE ";
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

        public static bool DeleteItem(long uid)
        {
            string sql = "UPDATE tbl_admin SET deleted = 1 WHERE uid = " + uid;

            SqlDBDataContext db = CommonModel.GetDBContext();
            db.ExecuteCommand(sql);

            return true;
        }

        public static string ValidateUserData(long uid, string name)
        {
            using (SqlDBDataContext db = CommonModel.GetDBContext())
            {
                if (uid == 0)   //user add case
                {
                    var existname = (from m in db.tbl_admins
                                     where (m.deleted == 0) && (m.name == name)
                                     select m).FirstOrDefault();
                    if (existname != null)
                    {
                        return ADMIN_SUBMITSTATUS.DUPLICATE_LOGINID;
                    }
                }
                else //user edit case
                {
                    var existname1 = (from m in db.tbl_admins
                                      where (m.deleted == 0) && (m.name == name) && (m.uid != uid)
                                      select m).FirstOrDefault();
                    if (existname1 != null)
                    {
                        return ADMIN_SUBMITSTATUS.DUPLICATE_LOGINID;
                    }
                }

                return ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
        }

        public static string InsertItem(string name, string realname, string mailaddr, string role, string imgpath, int status, string userpass)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            string sha1Pswd = AccountModel.GetMD5Hash(userpass);

            /* Check if same username already exists in DB. */
            string validateStr = ValidateUserData(0, name);

            if (validateStr != ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();
                string sql = "INSERT INTO tbl_admin (name, realname, mailaddress, role, imgpath, status, password, regtime, deleted) VALUES (" +
                    "N'" + name + "', " + "N'" + realname + "', " + " N'" + mailaddr + "', N'" + role + "', N'" + imgpath + "', '" +
                   status + "', " + "N'" + sha1Pswd + "', '" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + "', 0)";

                int nRows = db.ExecuteCommand(sql);
                if (nRows <= 0)
                {
                    return ADMIN_SUBMITSTATUS.ERROR_SUBMIT;
                }
                db.Transaction.Commit();

                return ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                db.Transaction.Rollback();
                CommonModel.WriteLogFile("UserModel", "InsertItem()", e.ToString());
                return ADMIN_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public static string UpdateItem(long uid, string name, string realname, string mailaddr, string role, string imgpath, int status, string userpass)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            string validateStr = ValidateUserData(uid, name);

            if (validateStr != ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();
                string sql = "UPDATE tbl_admin SET name = N'" + name + "', realname = N'" + realname +
                    "', role = '" + role + "', imgpath = N'" + imgpath + "', mailaddress='" + mailaddr + "', status = " + status;

                if (userpass != null && userpass.Trim() != "")
                {
                    string sha1Pswd = AccountModel.GetMD5Hash(userpass);
                    sql += ", password = N'" + sha1Pswd + "'";
                }

                sql += " WHERE uid = " + uid;

                int nRows = db.ExecuteCommand(sql);
                if (nRows <= 0)
                {
                    return ADMIN_SUBMITSTATUS.ERROR_SUBMIT;
                }
                db.Transaction.Commit();
                return ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                db.Transaction.Rollback();
                CommonModel.WriteLogFile("UserModel", "UpdateItem()", e.ToString());
                return ADMIN_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public static JqDataTableInfo GetAdminDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<AdminInfo> filteredCompanies;

            var alllist = GetAdminList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var nameFilter = Convert.ToString(Request["sSearch_1"]);
                var idFilter = Convert.ToString(Request["sSearch_2"]);

                //Optionally check whether the columns are searchable at all 
                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);
                var isIDSearchable = Convert.ToBoolean(Request["bSearchable_2"]);

                filteredCompanies = alllist
                   .Where(c => isNameSearchable && c.realname.ToLower().Contains(param.sSearch.ToLower())
                               ||
                               isIDSearchable && c.name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var isIDSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<AdminInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.realname :
                                                           sortColumnIndex == 2 && isIDSortable ? c.name :
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
                "<img src='" + rootUri + (String.IsNullOrEmpty(c.imgpath) ? "content/img/defuser.png" : c.imgpath) + "' />",
                c.realname,
                "<a href='" + rootUri + "Admin/Add/" + c.uid + "'>" + c.name + "</a>", 
                c.role, 
                c._status
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }

    }
}
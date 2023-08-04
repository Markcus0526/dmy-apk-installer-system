using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ManageSite.Models.Library;
using System.Collections.Specialized;

namespace ManageSite.Models
{
    #region UserModels
    public class UserInfo
    {
        public long uid { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public int userlevel { get; set; }
        public long? parentid { get; set; }
    }

    public class USER_SUBMITSTATUS
    {
        public const string DUPLICATE_LOGINID = "操作失败： 用户账号重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
        public const string CANNOT_DOWNLEVEL = "无法下级角色, 还存在此用户的2级代理";
    }

    public enum USERLEVEL
    {
        LEVEL1 = 1,
        LEVEL2
    }
    #endregion

    public class UserModel
    {
        SqlDBDataContext db = new SqlDBDataContext();

        public IEnumerable<UserInfo> GetUserList()
        {
            List<UserInfo> retList = null;

            CommonModel commonModel = new CommonModel();

            retList = db.tbl_users
                .Where(p => p.deleted == 0)
                .OrderBy(p => p.username)
                .Select(user => new UserInfo
                {
                    uid = user.uid,
                    name = user.username.Trim(),
                    password = user.password,
                    userlevel = user.userlevel,
                    parentid = user.parentid
                }).ToList();

            return retList;
        }

        public IEnumerable<UserInfo> GetLevelUsers(USERLEVEL userlevel)
        {
            List<UserInfo> retList = null;

            CommonModel commonModel = new CommonModel();

            retList = db.tbl_users
                .Where(p => p.deleted == 0 && p.userlevel == (int)userlevel)
                .OrderBy(p => p.username)
                .Select(user => new UserInfo
                {
                    uid = user.uid,
                    name = user.username.Trim()
                }).ToList();

            return retList;
        }


        public UserInfo GetUserInfo(long uid)
        {
            UserInfo userlist = (from user in db.tbl_users
                                  where (user.uid == uid)
                                  select new UserInfo
                                  {
                                      uid = user.uid,
                                      name = user.username.Trim(),
                                      userlevel = user.userlevel,
                                      parentid = user.parentid
                                  }).SingleOrDefault();

            return userlist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_user SET deleted = 1 WHERE ";
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
            string sql = "UPDATE tbl_user SET deleted = 1 WHERE uid = " + uid;

            db.ExecuteCommand(sql);

            return true;
        }

        public string ValidateUserData(long uid, string name)
        {
                if (uid == 0)   //user add case
                {
                    var existname = (from m in db.tbl_users
                                     where (m.deleted == 0) && (m.username == name)
                                     select m).FirstOrDefault();
                    if (existname != null)
                    {
                        return ADMIN_SUBMITSTATUS.DUPLICATE_LOGINID;
                    }
                }
                else //user edit case
                {
                    var existname1 = (from m in db.tbl_users
                                      where (m.deleted == 0) && (m.username == name) && (m.uid != uid)
                                      select m).FirstOrDefault();
                    if (existname1 != null)
                    {
                        return USER_SUBMITSTATUS.DUPLICATE_LOGINID;
                    }
                }

                return USER_SUBMITSTATUS.SUCCESS_SUBMIT;
        }

        public string InsertItem(string name, string userpass, string userlevel, string parentid)
        {
            string sha1Pswd = AccountModel.GetMD5Hash(userpass);

            /* Check if same username already exists in DB. */
            string validateStr = ValidateUserData(0, name);

            if (validateStr != USER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                tbl_user newuser = new tbl_user
                {
                    username = name,
                    password = sha1Pswd
                };

                try
                {
                    newuser.userlevel = byte.Parse(userlevel);

                    if (newuser.userlevel == 2 && !String.IsNullOrEmpty(parentid))
                    {
                        newuser.parentid = long.Parse(parentid);
                    }
                }
                catch (System.Exception ex)
                {
                	
                }

                db.tbl_users.InsertOnSubmit(newuser);
                db.SubmitChanges();

                db.Transaction.Commit();

                return USER_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                db.Transaction.Rollback();
                CommonModel.WriteLogFile("UserModel", "InsertItem()", e.ToString());
                return USER_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, string name, string userpass, string userlevel, string parentid)
        {
            string validateStr = ValidateUserData(uid, name);

            if (validateStr != USER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }


            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                byte newlevel = byte.Parse(userlevel);
                string sha1Pswd = AccountModel.GetMD5Hash(userpass);
                tbl_user edituser = (from m in db.tbl_users
                                     where m.uid == uid
                                     select m).FirstOrDefault();

                if (edituser != null)
                {
                    edituser.username = name;
                    edituser.password = sha1Pswd;

                    if (edituser.userlevel == (byte)USERLEVEL.LEVEL1 && newlevel == (byte)USERLEVEL.LEVEL2)
                    {
                        tbl_user existUser = (from m in db.tbl_users
                                           where m.parentid == edituser.uid && m.deleted == 0
                                        select m).FirstOrDefault();

                        if (existUser != null)
                        {
                            return USER_SUBMITSTATUS.CANNOT_DOWNLEVEL + "(" + existUser.username + ")";
                        }
                    }

                    edituser.userlevel = newlevel;

                    if (newlevel == (byte)USERLEVEL.LEVEL2 && !String.IsNullOrEmpty(parentid))
                    {
                        edituser.parentid = long.Parse(parentid);
                    }
                    else if (newlevel == (byte)USERLEVEL.LEVEL1)
                    {
                        edituser.parentid = null;
                    }
                }

                db.SubmitChanges();

                db.Transaction.Commit();
                return USER_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                db.Transaction.Rollback();
                CommonModel.WriteLogFile("UserModel", "UpdateItem()", e.ToString());
                return USER_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public JqDataTableInfo GetUserDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<UserInfo> filteredCompanies;

            var alllist = GetUserList();

            var level1users = GetLevelUsers(USERLEVEL.LEVEL1);
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var nameFilter = Convert.ToString(Request["sSearch_1"]);

                //Optionally check whether the columns are searchable at all 
                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);

                filteredCompanies = alllist
                   .Where(c => isNameSearchable && c.name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<UserInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.name :
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
                (c.userlevel == (int)USERLEVEL.LEVEL1 ? "一级代理" : "二级代理"),
                level1users.Where(m => m.uid == c.parentid).Select(m => m.name).FirstOrDefault(),
                "<a href='" + rootUri + "User/Add/" + c.uid + "'>" + c.name + "</a>",
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using ManageSite.Models.Library;
using System.Collections.Specialized;
using System.IO;

namespace ManageSite.Models
{
    #region Patch Model
    public class PatchInfo
    {
        public long uid { get; set; }
        public int vcode { get; set; }
        public string vname { get; set; }
        public string patchpath { get; set; }
        public DateTime createtime { get; set; }
    }

    public class PATCH_SUBMITSTATUS
    {
        public const string DUPLICATE_VERSION = "操作失败： 安装包名称重复！";
        public const string SUCCESS_SUBMIT = "";
        public const string ERROR_SUBMIT = "操作失败";
    }
    #endregion

    public class AppPatchModel
    {
        SqlDBDataContext db = new SqlDBDataContext();

        #region Public Methods
        public IEnumerable<PatchInfo> GetPatchList()
        {
            List<PatchInfo> retList = null;

            retList = db.tbl_apppatches
                .Where(p => p.deleted == 0)
                .OrderBy(p => p.versionCode)
                .Select(row => new PatchInfo
                {
                    uid = row.uid,
                    vcode = row.versionCode,
                    vname = row.versionName,
                    patchpath = row.patchpath,
                    createtime = row.createtime
                })
                .ToList();
            return retList;
        }

        public PatchInfo GetPatchInfo(long uid)
        {
            PatchInfo userlist = (from row in db.tbl_apppatches
                                   where (row.uid == uid)
                                   select new PatchInfo
                                   {
                                       uid = row.uid,
                                       vcode = row.versionCode,
                                       vname = row.versionName,
                                       patchpath = row.patchpath,
                                       createtime = row.createtime
                                   }).SingleOrDefault();

            return userlist;
        }

        public bool DeleteSelectedItems(long[] items)
        {
            string delSql = "UPDATE tbl_apppatch SET deleted = 1 WHERE ";
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
                var existname = (from m in db.tbl_apppatches
                                 where (m.deleted == 0) && (m.versionName == name)
                                 select m).FirstOrDefault();
                if (existname != null)
                {
                    return PATCH_SUBMITSTATUS.DUPLICATE_VERSION;
                }
            }
            else //user edit case
            {
                var existname1 = (from m in db.tbl_apppatches
                                  where (m.deleted == 0) && (m.versionName == name) && (m.uid != uid)
                                  select m).FirstOrDefault();
                if (existname1 != null)
                {
                    return PATCH_SUBMITSTATUS.DUPLICATE_VERSION;
                }
            }

            return PATCH_SUBMITSTATUS.SUCCESS_SUBMIT;
        }

        public string InsertItem(int vcode, string vname, string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            /* Check if same name already exists. */
            string validateStr = ValidateUserData(0, vname);

            if (validateStr != PATCH_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apppatch newitem = new tbl_apppatch();

                newitem.versionCode = vcode;
                newitem.versionName = vname;
                newitem.patchpath = MovePatchFile(filepath);

                newitem.createtime = DateTime.Today;

                db.tbl_apppatches.InsertOnSubmit(newitem);
                db.SubmitChanges();

                return PATCH_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PatchModel", "InsertItem()", e.ToString());
                return PATCH_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public string UpdateItem(long uid, int vcode, string vname, string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string validateStr = ValidateUserData(uid, vname);

            if (validateStr != PATCH_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return validateStr;
            }

            try
            {
                tbl_apppatch edititem = (from m in db.tbl_apppatches
                                           where m.deleted == 0 && m.uid == uid
                                           select m).FirstOrDefault();
                if (edititem != null)
                {
                    edititem.versionCode = vcode;
                    edititem.versionName = vname;
                    if (edititem.patchpath != filepath)
                    {
                        edititem.patchpath = MovePatchFile(filepath);
                    }
                }

                db.SubmitChanges();

                return PATCH_SUBMITSTATUS.SUCCESS_SUBMIT;
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("PatchModel", "UpdateItem()", e.ToString());
                return PATCH_SUBMITSTATUS.ERROR_SUBMIT;
            }
        }

        public JqDataTableInfo GetPatchDataTable(JQueryDataTableParamModel param, NameValueCollection Request, String rootUri)
        {
            JqDataTableInfo rst = new JqDataTableInfo();
            IEnumerable<PatchInfo> filteredCompanies;

            var alllist = GetPatchList();
            //Check whether the companies should be filtered by keyword
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                //Used if particulare columns are filtered 
                var nameFilter = Convert.ToString(Request["sSearch_1"]);

                //Optionally check whether the columns are searchable at all 
                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_1"]);

                filteredCompanies = alllist
                   .Where(c =>  isNameSearchable && c.vname.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredCompanies = alllist;
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_1"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<PatchInfo, string> orderingFunction = (c => sortColumnIndex == 1 && isNameSortable ? c.vname :
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
                c.vcode.ToString(),
                c.vname,
                String.Format("{0:yyyy-MM-dd HH:mm:ss}", c.createtime), 
                Convert.ToString(c.uid)
            };

            rst.sEcho = param.sEcho;
            rst.iTotalRecords = alllist.Count();
            rst.iTotalDisplayRecords = filteredCompanies.Count();
            rst.aaData = result;

            return rst;
        }

        #endregion

        #region Private Methods
        private string MovePatchFile(string filepath)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string targetpath = "";
            string[] fileArr = filepath.Split('.');

            if (fileArr.Count() > 1)
            {

                if (File.Exists(rootpath + filepath))
                {
                    targetpath = "Content/uploads/file/patch/" + String.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                    targetpath += "." + fileArr[fileArr.Count() - 1];
                    try
                    {
                        File.Move(rootpath + filepath, rootpath + targetpath);
                    }
                    catch (System.Exception ex)
                    {
                        CommonModel.WriteLogFile("PatchModel", "MovePatchFile()", ex.ToString());
                    }
                }
            }
            return targetpath;
        }
        #endregion
    }
}
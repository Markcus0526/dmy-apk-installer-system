using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{
    public class ApkController : Controller
    {
        ApkPackageModel _packageModel = new ApkPackageModel();
        ApkModel _apkModel = new ApkModel();
        UserModel _userModel = new UserModel();

        #region Apk Files
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;
            ViewData["grouplist"] = _packageModel.GetPackageList();

            if (id > 0)
            {
                var info = _apkModel.GetApkInfo(id);
                if (info != null)
                {
                    ViewData["groupids"] = info.groupids;
                    ViewData["name"] = info.name;
                    ViewData["filepath"] = info.filepath;
                    ViewData["imgpath"] = info.imgpath;
                    ViewData["version"] = info.version;
                }
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Add()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string rst = "";

            string groupid = Request.Form["groupid"].ToString();
            long uid = long.Parse(Request.Form["uid"]);
            string name = Request.Form["name"].ToString();
            string imgpath = Request.Form["imgpath"].ToString();
            string filepath = Request.Form["filepath"].ToString();
            string version = Request.Form["version"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = _apkModel.UpdateItem(uid, groupid, name, filepath, imgpath, version);
            }
            else
            {
                rst = _apkModel.InsertItem(groupid, name, filepath, imgpath, version);
            }

            if (rst == APK_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["name"] = name;
                ViewData["imgpath"] = imgpath;
                ViewData["grouplist"] = _packageModel.GetPackageList();
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult List()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            if (Request.QueryString["succ"] != null)
            {
                if (Request.QueryString["succ"].ToString() == "add")
                {
                    ViewData["success"] = "操作成功: 添加/编辑APK成功！";
                }
                else if (Request.QueryString["succ"].ToString() == "del")
                {
                    ViewData["success"] = "操作成功: 您选择的记录都被删除！";
                }
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [AjaxOnly]
        public JsonResult RetrieveApkList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _apkModel.GetApkDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteApks(long[] selcheckbox)
        {
            bool rst = _apkModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }
        #endregion

        #region Apk Package List
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult AddPackage(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;
            ViewData["userlist"] = _userModel.GetUserList();

            if (id > 0)
            {
                var info = _packageModel.GetPackageInfo(id);
                if (info != null)
                {
                    ViewData["name"] = info.name;
                    ViewData["imgpath"] = info.imgpath;
                    ViewData["gtype"] = (int)info.gtype;
                    ViewData["userids"] = info.userids;
                }
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult AddPackage()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string rst = "";

            long uid = long.Parse(Request.Form["uid"]);
            string name = Request.Form["name"].ToString();
            string imgpath = Request.Form["imgpath"].ToString();
            string userids = Request.Form["userids"] == null ? "" : Request.Form["userids"].ToString();
            string gtype = Request.Form["gtype"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;
            ViewData["userlist"] = _userModel.GetUserList();

            if (uid > 0)
            {
                rst = _packageModel.UpdateItem(uid, name, gtype, userids, imgpath);
            }
            else
            {
                rst = _packageModel.InsertItem(name, gtype, userids, imgpath);
            }

            if (rst == APK_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("PackageList", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["name"] = name;
                ViewData["gtype"] = gtype;
                ViewData["imgpath"] = imgpath;
                ViewData["userids"] = userids;
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult PackageList()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            if (Request.QueryString["succ"] != null)
            {
                if (Request.QueryString["succ"].ToString() == "add")
                {
                    ViewData["success"] = "操作成功: 添加/编辑套餐成功！";
                }
                else if (Request.QueryString["succ"].ToString() == "del")
                {
                    ViewData["success"] = "操作成功: 您选择的记录都被删除！";
                }
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [AjaxOnly]
        public JsonResult RetrievePackageList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _packageModel.GetPackageDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeletePackages(long[] selcheckbox)
        {
            bool rst = _packageModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("PackageList", new { succ = "del" });
            }

            return View();
        }
        #endregion

        #region Other
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult DownLog()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            if (Request.QueryString["succ"] != null)
            {
                if (Request.QueryString["succ"].ToString() == "add")
                {
                    ViewData["success"] = "操作成功: 添加/编辑套餐成功！";
                }
                else if (Request.QueryString["succ"].ToString() == "del")
                {
                    ViewData["success"] = "操作成功: 您选择的记录都被删除！";
                }
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [AjaxOnly]
        public JsonResult RetrieveDownLogList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _apkModel.GetApkDownLogDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteDownLog(long[] selcheckbox)
        {
            bool rst = _apkModel.DeleteSelectedDownLogItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("DownLog", new { succ = "del" });
            }

            return View();
        }

       [Authorize(Roles = "Administrator,Leader")]
       [SessionExpireFilter]
       public ActionResult InstallLog()
       {
           string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

           ViewData["rootUri"] = rootUri;
           if (Request.QueryString["succ"] != null)
           {
               if (Request.QueryString["succ"].ToString() == "add")
               {
                   ViewData["success"] = "操作成功: 添加/编辑套餐成功！";
               }
               else if (Request.QueryString["succ"].ToString() == "del")
               {
                   ViewData["success"] = "操作成功: 您选择的记录都被删除！";
               }
           }

           return View();
       }

       [Authorize(Roles = "Administrator,Leader")]
       [SessionExpireFilter]
       [AjaxOnly]
       public JsonResult RetrieveInstallLogList(JQueryDataTableParamModel param)
       {
           string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

           JqDataTableInfo rst = _apkModel.GetApkInstallLogDataTable(param, Request.QueryString, rootUri);
           return Json(rst, JsonRequestBehavior.AllowGet);
       }

       [Authorize(Roles = "Administrator,Leader")]
       [SessionExpireFilter]
       [HttpPost]
       public ActionResult DeleteInstallLog(long[] selcheckbox)
       {
           bool rst = _apkModel.DeleteSelectedInstallLogItems(selcheckbox);
           if (rst)
           {
               return RedirectToAction("InstallLog", new { succ = "del" });
           }

           return View();
       }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{
    public class DriverController : Controller
    {
        CommonModel _commonModel = new CommonModel();
        DriverModel _driverModel = new DriverModel();
        PhoneModel _phoneModel = new PhoneModel();

        #region Driver List Actions
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Index()
        {
            return RedirectToAction("List");
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
                    ViewData["success"] = "操作成功: 添加/编辑驱动成功！";
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
        public JsonResult RetrieveDriverList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _driverModel.GetDriverDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteDrivers(long[] selcheckbox)
        {
            bool rst = _driverModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }
        #endregion

        #region Driver Add/Edit Actions
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;
            ViewData["tellist"] = _phoneModel.GetPhoneList();

            if (id > 0)
            {
                var info = _driverModel.GetDriverInfo(id);
                if (info != null)
                {
                    ViewData["telid"] = info.telid;
                    ViewData["name"] = info.name;
                    ViewData["sysver"] = info.sysver;
                    ViewData["filepath"] = info.filepath;
                }
            }

            return View("Add");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Add()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string rst = "";

            long uid = long.Parse(Request.Form["uid"]);
            long telid = long.Parse(Request.Form["telid"]);
            string name = Request.Form["name"].ToString();
            string sysver = Request.Form["sysver"].ToString();
            string filepath = Request.Form["filepath"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = _driverModel.UpdateItem(uid, telid, name, sysver, filepath);
            }
            else
            {
                rst = _driverModel.InsertItem(telid, name, sysver, filepath);
            }

            if (rst == DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["telid"] = telid;
                ViewData["name"] = name;
                ViewData["sysver"] = sysver;
                ViewData["filepath"] = filepath;
            }

            return View("Add");
        }
        #endregion

        #region Download Log Actions
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

            JqDataTableInfo rst = _driverModel.GetDriverDownLogDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteDownLog(long[] selcheckbox)
        {
            bool rst = _driverModel.DeleteSelectedDownLogItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("DownLog", new { succ = "del" });
            }

            return View();
        }
        #endregion
    }
}

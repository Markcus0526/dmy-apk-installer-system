using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{
    public class AppPatchController : Controller
    {
        CommonModel _commonModel = new CommonModel();
        AppPatchModel _patchModel = new AppPatchModel();

        #region Patch List Actions
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
                    ViewData["success"] = "操作成功: 添加/编辑Patch成功！";
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
        public JsonResult RetrievePatchList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _patchModel.GetPatchDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeletePatchs(long[] selcheckbox)
        {
            bool rst = _patchModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }
        #endregion

        #region Patch Add/Edit Actions
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;

            if (id > 0)
            {
                var info = _patchModel.GetPatchInfo(id);
                if (info != null)
                {
                    ViewData["vcode"] = info.vcode;
                    ViewData["vname"] = info.vname;
                    ViewData["patchpath"] = info.patchpath;
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
            int vcode = int.Parse(Request.Form["vcode"]);
            string vname = Request.Form["vname"].ToString();
            string patchpath = Request.Form["patchpath"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = _patchModel.UpdateItem(uid, vcode, vname, patchpath);
            }
            else
            {
                rst = _patchModel.InsertItem(vcode, vname, patchpath);
            }

            if (rst == DRIVER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["vcode"] = vcode;
                ViewData["vname"] = vname;
                ViewData["patchpath"] = patchpath;
            }

            return View("Add");
        }
        #endregion
    }
}

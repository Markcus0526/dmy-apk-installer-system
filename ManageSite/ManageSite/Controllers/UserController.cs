using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{
    public class UserController : Controller
    {
        CommonModel _commonModel = new CommonModel();
        UserModel _userModel = new UserModel();

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
                    ViewData["success"] = "操作成功: 添加/编辑管理员成功！";
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
        public JsonResult RetrieveUserList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _userModel.GetUserDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;
            ViewData["level1user"] = _userModel.GetLevelUsers(USERLEVEL.LEVEL1);

            if (id > 0)
            {
                var info = _userModel.GetUserInfo(id);
                if (info != null)
                {
                    ViewData["name"] = info.name;
                    ViewData["userlevel"] = info.userlevel;
                    ViewData["parentid"] = info.parentid;
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

            long uid = long.Parse(Request.Form["userid"]);
            string name = Request.Form["name"].ToString();
            string password = Request.Form["password"].ToString();
            string userlevel = Request.Form["userlevel"].ToString();
            string parentid = Request.Form["parentid"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = _userModel.UpdateItem(uid, name, password, userlevel, parentid);
            }
            else
            {
                rst = _userModel.InsertItem(name, password, userlevel, parentid);
            }

            if (rst == USER_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["name"] = name;
                ViewData["level1user"] = _userModel.GetLevelUsers(USERLEVEL.LEVEL1);
                ViewData["userlevel"] = userlevel;
                ViewData["parentid"] = parentid;
            }

            return View("Add");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteUsers(long[] selcheckbox)
        {
            bool rst = _userModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }
    }
}

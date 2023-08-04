using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ManageSite.Models;
using System.Web.Security;
using System.IO;
using ManageSite.Models.Library;
using MvcSiteMapProvider;

namespace ManageSite.Controllers
{
    public class AdminController : Controller
    {
        CommonModel _commonModel = new CommonModel();

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        //[MvcSiteMapNodeAttribute(Title = "管理员列表", ParentKey = "Admin")]
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
        public JsonResult RetrieveAdminList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = AdminModel.GetAdminDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        //[MvcSiteMapNodeAttribute(Title = "新增管理员", ParentKey = "Admin")]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;

            if (id > 0)
            {
                var info = AdminModel.GetAdminInfo(id);
                if (info != null)
                {
                    ViewData["name"] = info.name;
                    ViewData["realname"] = info.realname;
                    ViewData["mailaddr"] = info.mailaddr;
                    ViewData["role"] = info.role;
                    ViewData["imgpath"] = info.imgpath;
                    ViewData["status"] = info.status;
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

            long uid = long.Parse(Request.Form["adminid"]);
            string name = Request.Form["name"].ToString();
            string realname = Request.Form["realname"].ToString();
            string mailaddr = Request.Form["adminmailaddr"].ToString();
            string role = Request.Form["role"].ToString();
            string imgpath = Request.Form["imgpath"].ToString();
            int status = int.Parse(Request.Form["status"]);
            string password = Request.Form["password"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = AdminModel.UpdateItem(uid, name, realname, mailaddr, role, imgpath, status, password);
            }
            else
            {
                rst = AdminModel.InsertItem(name, realname, mailaddr, role, imgpath, status, password);
            }

            if (rst == ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["name"] = name;
                ViewData["realname"] = realname;
                ViewData["mailaddr"] = mailaddr;
                ViewData["role"] = role;
                ViewData["imgpath"] = imgpath;
                ViewData["status"] = status;
            }

            return View("Add");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeleteAdmins(long[] selcheckbox)
        {
            bool rst = AdminModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }
    }
}

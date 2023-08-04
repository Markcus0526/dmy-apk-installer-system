using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using ManageSite.Models.Library;
using System.Web.Script.Serialization;
using System.Text;
using System.Reflection;
using System.Data;

namespace ManageSite.Controllers
{
    public class PhoneController : Controller
    {
        CommonModel _commonModel = new CommonModel();
        PhoneModel _phoneModel = new PhoneModel();

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
        public JsonResult RetrievePhoneList(JQueryDataTableParamModel param)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            JqDataTableInfo rst = _phoneModel.GetPhoneDataTable(param, Request.QueryString, rootUri);
            return Json(rst, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [AjaxOnly]
        //public virtual ActionResult GetPhoneList(string sidx, string sord, int page, int rows)
        public virtual ActionResult GetPhoneList(string sidx, string sord, int page, int rows, bool _search, string filters)
        {
            var phones = _phoneModel.GetPhoneList() as IEnumerable<PhoneInfo>;

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = phones.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            if (_search)
            {
                var serializer = new JavaScriptSerializer();
                JqGridFilters f = serializer.Deserialize<JqGridFilters>(filters);

                if (f.rules.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (JqGridFilters.Rule rule in f.rules)
                    {
                        if (sb.Length != 0)
                            sb.Append(f.groupOp);

                        sb.AppendFormat(JqGridFilters.FormatMapping[(int)rule.op], rule.field, rule.data);
                    }

                    phones = _phoneModel.GetPhoneFilterList(sb.ToString());
                    //phones = phones.Where(x => x.serial.Contains("WW")).ToList();
                }
            }
            else
                phones = phones.Skip(pageIndex * pageSize).Take(pageSize);
            

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = (
                    from phone in phones
                    select new
                    {
                        uid = phone.uid.ToString(),
                        type = phone.type.ToString(), 
                        vendor = phone.vendor, 
                        sysver = phone.sysver,
                        serial = phone.serial,
                        imei = phone.imei
                    }).ToList()
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [AjaxOnly]
        public virtual ActionResult GetPhoneAppList(long telid, string sidx, string sord, int page, int rows)
        {
            var apps = _phoneModel.GetPhoneAppList(telid) as IEnumerable<PhoneAppInfo>;

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = apps.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            apps = apps.Skip(pageIndex * pageSize).Take(pageSize);

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = (
                    from app in apps
                    select new
                    {
                        //uid = app.uid.ToString(),
                        appname = app.appname,
                        appversion = app.appversion,
                        installtime = app.installtime
                    }).ToList()
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

//         public ActionResult Update(ContactViewModel viewModel, FormCollection formCollection)
//         {
//             var operation = formCollection["oper"];
//             if (operation.Equals("add") || operation.Equals("edit"))
//             {
//                 repository.SaveOrUpdate(new ContactViewModel
//                 {
//                     ContactId = viewModel.ContactId,
//                     DateOfBirth = viewModel.DateOfBirth,
//                     Email = viewModel.Email,
//                     IsMarried = viewModel.IsMarried,
//                     Name = viewModel.Name,
//                     PhoneNumber = viewModel.PhoneNumber
//                 });
//             }
//             else if (operation.Equals("del"))
//             {
//                 repository.Delete(new ContactViewModel
//                 {
//                     ContactId = viewModel.ContactId
//                     //ContactId = new Guid(formCollection["id"])
//                 });
//             }
// 
//             return Content(repository.HasErrors.ToString().ToLower());
//         }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Add(long id = 0)
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = id;

            if (id > 0)
            {
                var info = _phoneModel.GetPhoneInfo(id);
                if (info != null)
                {
                    ViewData["type"] = info.type;
                    ViewData["vendor"] = info.vendor;
                    ViewData["sysver"] = info.sysver;
                    ViewData["serial"] = info.serial;
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

            long uid = long.Parse(Request.Form["phoneid"]);
            string type = Request.Form["type"].ToString();
            string vendor = Request.Form["vendor"].ToString();
            string sysver = Request.Form["sysver"].ToString();
            string serial = Request.Form["serial"].ToString();

            ViewData["rootUri"] = rootUri;
            ViewData["id"] = uid;

            if (uid > 0)
            {
                rst = _phoneModel.UpdateItem(uid, type, vendor, sysver, serial);
            }
            else
            {
                rst = _phoneModel.InsertItem(type, vendor, sysver, serial);
            }

            if (rst == PHONE_SUBMITSTATUS.SUCCESS_SUBMIT)
            {
                return RedirectToAction("List", new { succ = "add" });
            }
            else
            {
                ViewData["error"] = rst;
                ViewData["type"] = type;
                ViewData["vendor"] = vendor;
                ViewData["sysver"] = sysver;
                ViewData["serial"] = serial;
            }

            return View("Add");
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult DeletePhones(long[] selcheckbox)
        {
            bool rst = _phoneModel.DeleteSelectedItems(selcheckbox);
            if (rst)
            {
                return RedirectToAction("List", new { succ = "del" });
            }

            return View();
        }

        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Apps()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            ViewData["rootUri"] = rootUri;

            return View();
        }
    }
}

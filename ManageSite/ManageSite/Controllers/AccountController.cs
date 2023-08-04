using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using ManageSite.Models;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{

    [HandleError]
    public class AccountController : Controller
    {
        private AccountModel accountModel = new AccountModel();

        public ActionResult LogOn(string returnUrl)
        {
            ViewData["rootUri"] = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            ViewData["base_url"] = ViewData["rootUri"] + "Account";
            ViewData["returnUrl"] = returnUrl;

            var ssw = HttpContext.Session["adminId"];

            if (ssw != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    /*
                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    } else {
                     */
                    return RedirectToAction("Index", "Home");
                    //}
                }
            }

            return View("LogOn");
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            string userRole = "";

            ViewData["rootUri"] = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            ViewData["base_url"] = ViewData["rootUri"] + "Account";

            ViewData["curdate"] = DateTime.Today.ToString("yyyy年MM月dd日");

//             if (!ApkLicenseService.CheckTrialVersion())
//             {
//                 ModelState.AddModelError("modelerror", "系统到期了，已使用了30天！");
//                 return View("LogOn", model);
//             }

            if (ModelState.IsValid)
            {
                var userInfo = accountModel.ValidateUser(model.UserName, model.Password);
                if (userInfo != null)
                {
                    accountModel.SignIn(model.UserName, model.RememberMe);

                    if (userInfo != null)
                    {
                        if (userInfo.role == "Admin")
                        {
                            userRole = "Administrator";
                        }
                        else if (userInfo.role == "Leader")
                        {
                            userRole = "Leader";
                        }
                        else if (userInfo.role == "Normal")
                        {
                            userRole = "Normal";
                        }
                        //////////////////////////////////
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                                userInfo.name,
                                DateTime.Now,
                                DateTime.Now.AddMinutes(1440),
                                model.RememberMe,
                                userRole,
                                FormsAuthentication.FormsCookiePath);

                        // Encrypt the ticket.
                        string encTicket = FormsAuthentication.Encrypt(ticket);

                        // Create the cookie.
                        Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));


                        HttpContext.Session.Add("loginTime", DateTime.Now);
                        HttpContext.Session.Add("adminrealname", /*userInfo.realname*/userInfo.realname);
                        HttpContext.Session.Add("adminName", userInfo.name);
                        HttpContext.Session.Add("adminRole", userInfo.password);
                        HttpContext.Session.Add("adminId", userInfo.uid);
                        HttpContext.Session.Add("adminphoto", String.IsNullOrEmpty(userInfo.imgpath) ? "content/img/defuser.png" : userInfo.imgpath);
                        //HttpContext.Session.Add("employee_uid", userInfo.employee_uid);
                        HttpContext.Session.Add("loginIP", Request.ServerVariables["REMOTE_ADDR"]);

                        //syslogModel.InsertSyslog(userInfo.name, Request.ServerVariables["REMOTE_ADDR"].ToString(), "成功登录后台系统！", 1, "");
                        //cookieModel.CreateCookiePerPage();
                        /////////////////////////////////
                    }

                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("modelerror", "帐号或密码错误，请重新输入");
                }
            }

            return View("LogOn", model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            accountModel.SignOut();

            //Response.Cache.SetExpires(DateTime.Now);
            return RedirectToAction("LogOn", "Account");
        }
    }
}

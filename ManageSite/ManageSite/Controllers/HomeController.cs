using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageSite.Models;
using MvcSiteMapProvider.Filters;

namespace ManageSite.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        [SiteMapTitle("My Account")]
        [Authorize(Roles = "Administrator,Leader")]
        [SessionExpireFilter]
        public ActionResult Index()
        {
            return RedirectToAction("List", "Admin");
        }

    }
}

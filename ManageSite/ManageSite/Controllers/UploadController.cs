using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using ManageSite.Models;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Drawing;
using ManageSite.Models.Library;

namespace ManageSite.Controllers
{
    public class UploadController : Controller
    {
        #region Image processing
        [Authorize(Roles = "Administrator,Leader,Normal")]
        [HttpPost]
        [SessionExpireFilter]
        public string UploadImage(HttpPostedFileBase userfile)
        {
            string basePath = Server.MapPath("~/Content/uploads/temp");
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string fileName = "";


            // Some browsers send file names with full path. This needs to be stripped.
            fileName = Path.GetFileName(userfile.FileName);
            var physicalPath = Path.Combine(basePath, fileName);

            if (System.IO.File.Exists(physicalPath))
            {
                string[] tmpFileName = fileName.Split('.');
                //string another = "";
                if (tmpFileName.Count() > 0)
                {
                    fileName = tmpFileName[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + tmpFileName[tmpFileName.Count() - 1];
                    physicalPath = Path.Combine(basePath, fileName);
                }
            }

            userfile.SaveAs(physicalPath);

            //return Json(new { ImgUrl = "Content/uploads/temp/" + fileName }, "text/plain");
            return "Content/uploads/temp/" + fileName;
        }

        [Authorize(Roles = "Administrator,Leader,Normal")]
        [SessionExpireFilter]
        public string RetrieveCropDialogHtml()
        {
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string basePath = Server.MapPath("~/");
            ViewData["rootUri"] = rootUri;

            if (Request.QueryString["cropfile"] != null) {
                string cropfile = Request.QueryString["cropfile"].ToString();
                ViewData["cropfile"] = cropfile;

                if (System.IO.File.Exists(basePath + cropfile))
                {
                    Bitmap img = new Bitmap(basePath + cropfile);
                    if (img.Height < 300)
                    {
                        ViewData["height"] = img.Height;
                        ViewData["width"] = img.Width;
                    }
                    else
                    {
                        ViewData["height"] = 300;
                        ViewData["width"] = Math.Round(img.Width * (300 / (decimal)img.Height));

                    }
                }
            }

            string ret = RenderPartialToString("~/Views/Shared/CropModal.ascx", ViewData);
            return ret;
        }

        [Authorize(Roles = "Administrator,Leader,Normal")]
        [SessionExpireFilter]
        public static string RenderPartialToString(string controlName, object viewData)
        {
            ViewPage viewPage = new ViewPage() { ViewContext = new ViewContext() };

            viewPage.ViewData = new ViewDataDictionary(viewData);
            viewPage.Controls.Add(viewPage.LoadControl(controlName));

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (HtmlTextWriter tw = new HtmlTextWriter(sw))
                {
                    viewPage.RenderControl(tw);
                }
            }

            return sb.ToString();
        }


        [Authorize(Roles = "Administrator,Leader,Normal")]
        [SessionExpireFilter]
        [HttpPost]
        [AjaxOnly]
        public string ResizeImage(int x, int y, int w, int h, string imgpath, string kind, string size)
        {
            ImageHelper helper = new ImageHelper();

            return helper.ResizeAndCrop(imgpath, x, y, w, h, kind, size);
        }

        [Authorize(Roles = "Administrator")]
        [SessionExpireFilter]
        public string RemoveImage(string filename)
        {
            string basePath = Server.MapPath("~/");

            var physicalPath = Path.Combine(basePath, filename);

            // TODO: Verify user permissions

            if (System.IO.File.Exists(physicalPath))
            {
                // The files are not actually removed in this demo
                try
                {
                    System.IO.File.Delete(physicalPath);
                }
                catch (System.Exception ex)
                {
                    CommonModel.WriteLogFile(this.GetType().Name, "RemoveTempImg()", ex.Message.ToString());
                }
            }

            // Return an empty string to signify success
            return "";
        }
        #endregion

        #region File processing
        [Authorize(Roles = "Administrator,Leader,Normal")]
        [HttpPost]
        [SessionExpireFilter]
        public string UploadFile(HttpPostedFileBase uploadfile)
        {
            string basePath = Server.MapPath("~/Content/uploads/temp");
            string rootUri = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            string fileName = "";


                // Some browsers send file names with full path. This needs to be stripped.
            fileName = Path.GetFileName(uploadfile.FileName);
            var physicalPath = Path.Combine(basePath, fileName);

            if (System.IO.File.Exists(physicalPath))
            {
                string[] tmpFileName = fileName.Split('.');
                //string another = "";
                if (tmpFileName.Count() > 0)
                {
                    fileName = tmpFileName[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + tmpFileName[tmpFileName.Count() - 1];
                    physicalPath = Path.Combine(basePath, fileName);
                }
            }

            uploadfile.SaveAs(physicalPath);

            //return Json(new { FilePath = "Content/uploads/temp/" + fileName }, "text/plain");
            return "Content/uploads/temp/" + fileName;

        }
        #endregion

    }
}

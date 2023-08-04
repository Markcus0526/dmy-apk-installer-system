using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Web.Hosting;

namespace ManageSite.Models.Library
{
    #region ImageCropModel
    public class GiftImageSize {
        public int p_width {get;set;}
        public int p_height {get;set;}
    }

    public static class CropImageSizes
    {
        public const string APKGROUP = "70,70";
        public const string ADMIN = "29,29";
        public const string APK = "48,48";
    };

    public static class UpImageCategory //Set the Content/uploads/ directory name
    {
        public const string APKGROUP = "ApkGroup";
        public const string ADMIN = "Admin";
        public const string APK = "Apk";
    };
    #endregion

    public class ImageHelper
    {
        private void saveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = this.getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        public string ResizeAndCrop(string filepath, int x, int y, int w, int h, string category, string imagesize)
        {
            string rootpath = HostingEnvironment.MapPath("~/");
            string file_prefix = "";
            int width = 0, height = 0;
            decimal zoomratio = 1;

            int orgWidth = 0, orgHeight = 0;
            int tgtX, tgtY, tgtW, tgtH;

            string[] sizelist = imagesize.Split(',');
            height = (int)decimal.Parse(sizelist[1]);
            width = (int)decimal.Parse(sizelist[0]);

            file_prefix = category + "_";

            //first get original image bitmap.
            Image orgimg = Image.FromFile(rootpath + filepath);

            if (orgimg == null) return "";

            orgWidth = orgimg.Width;
            orgHeight = orgimg.Height;

            if (orgimg.Height > 300)
            {
                zoomratio = (decimal)orgimg.Height / 300;
            }

            /* Fit target image size to orginal image size using computed ratio. */
            tgtX = (int)(Math.Round(x * zoomratio));
            tgtY = (int)(Math.Round(y * zoomratio));
            tgtW = (int)(Math.Round(w * zoomratio));
            tgtH = (int)(Math.Round(h * zoomratio));

            Bitmap srcBmp = orgimg as Bitmap;

            Rectangle cropSection = new Rectangle(tgtX, tgtY, tgtW, tgtH);

            string rstname = "Content/uploads/image/" + category + "/" + file_prefix + Path.GetFileName(filepath);
            try
            {
                /* Make fited and cropped image from original image. */
                using (Bitmap cropBmp = srcBmp.Clone(cropSection, srcBmp.PixelFormat))
                {
                    using (Bitmap targetBmp = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage((Image)targetBmp))
                        {
                            g.Clear(Color.Transparent);
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(cropBmp, 0, 0, width, height);

                            string savepath = rootpath + rstname;
                            try
                            {
                                targetBmp.Save(savepath, orgimg.RawFormat);
                            }
                            catch (Exception e)
                            {
                                CommonModel.WriteLogFile(this.GetType().Name, "ResizeAndCrop()", e.ToString());
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                CommonModel.WriteLogFile("ImageHelper", "ResizeAndCrop()", ex.ToString());            	
            }

            orgimg.Dispose();

            return rstname;
        }
    }
}
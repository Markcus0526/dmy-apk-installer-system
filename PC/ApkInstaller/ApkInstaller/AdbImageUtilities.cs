using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ApkInstaller.ServiceCorrespond
{
    class AdbImageUtilities
    {
        private AdbImageUtilities()
        {
        }

        public static Bitmap RotateImage(Image image, float angle)
        {
            return RotateImage(image, new PointF((float)image.Width / 2, (float)image.Height / 2), angle);
        }


        public static Bitmap RotateImage(Image image, PointF offset, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp;
            if (angle == 90 || angle == 270)
            {
                rotatedBmp = new Bitmap(image.Height, image.Width);
                rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            else
            {
                rotatedBmp = new Bitmap(image.Width, image.Height);
                rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image

            if (angle == 90 || angle == 270)
            {
                g.TranslateTransform(offset.Y, offset.X);
            }
            else
            {
                g.TranslateTransform(offset.X, offset.Y);
            }
            

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            
            g.TranslateTransform(-offset.X, -offset.Y);
            
            

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
    }
}

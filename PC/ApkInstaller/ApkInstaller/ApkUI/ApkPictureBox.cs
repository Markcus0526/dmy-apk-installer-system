using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ApkInstaller.ApkUI
{

    class ApkPictureBox : PictureBox
    {
        private const int Edge = 10;
        private Rectangle mHoverRectangle = Rectangle.Empty;
        private const int HOVER_RECTANGLE_SIZE = 20;

        private ApkToolTip m_tooltip;
        private string apkname = "";
        private string version = "";
        private long apksize = 0;

        public ApkPictureBox()
        {
            this.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            this.MouseLeave += new EventHandler(pictureBox_MouseLeave);
            this.MouseEnter += new EventHandler(pictureBox_MouseEnter);
            this.MouseHover += new EventHandler(pictureBox_MouseHover);
            this.Paint += new PaintEventHandler(pictureBox_Paint);
            this.Disposed += new EventHandler(ApkPictureBox_Disposed);

            m_tooltip = new ApkToolTip();
        }

        public ApkPictureBox(string apkname) : this()
        {
            this.apkname= apkname;
        }

        public ApkPictureBox(string apkname, string version) : this(apkname)
        {
            m_tooltip.m_ApkVersion = this.version = version;
        }

        public ApkPictureBox(string apkname, string version, long apksize) : this(apkname, version)
        {
            m_tooltip.m_ApkSize = this.apksize = apksize;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.BackColor = Color.LightGray;

            Rectangle r = new Rectangle(0, 0, this.Width, this.Height);
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddArc(r.X, r.Y, Edge, Edge, 180, 90);
            gp.AddArc(r.X + r.Width - Edge, r.Y, Edge, Edge, 270, 90);
            gp.AddArc(r.X + r.Width - Edge, r.Y + r.Height - Edge, Edge, Edge, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - Edge, Edge, Edge, 90, 90);
            this.Region = new Region(gp);
        }

        void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            mHoverRectangle = Rectangle.Empty;
            this.Invalidate();

            m_tooltip.Hide(this);
        }

        void ApkPictureBox_Disposed(object sender, EventArgs e)
        {
            if (m_tooltip != null)
            {
                m_tooltip.Dispose();
            }
        }

        void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (mHoverRectangle != Rectangle.Empty)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(150, Color.LightBlue)))
                {
                    e.Graphics.FillRectangle(b, mHoverRectangle);
                }
            }
        }

        void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            mHoverRectangle = new Rectangle(
                0,
                0,
                this.Width,
                this.Height);
            this.Invalidate();
        }

        void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            Rectangle rect = new Rectangle(
                0,
                0,
                this.Width,
                this.Height);

            m_tooltip.Show(this.apkname, this, rect.Right + 10, rect.Top);
        }

        void pictureBox_MouseHover(object sender, EventArgs e)
        {

        }
    }
}   

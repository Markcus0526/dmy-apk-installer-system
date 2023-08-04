using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using ApkInstaller.ServiceCorrespond;

namespace ApkInstaller
{
    public partial class MinimizeToTrayConfirmForm : Form
    {
        public int nExitAction;
        public bool confirmResult;
        private Boolean m_bDragging = false;
        private Point m_ptCur;
        private Point m_ptFormPoint;
        public bool bNotRetryPrompt;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
                                                            int nTopRect,
                                                            int nRightRect,
                                                            int nBottomRect,
                                                            int nWidthElipse,
                                                            int nHeightElipse);

        public MinimizeToTrayConfirmForm()
        {
            InitializeComponent();

            groupBoxAction.Text = ApkDataModel.APKLan("ConfirmExitGroupAction");
            radioDirectExit.Text = ApkDataModel.APKLan("ConfirmExitDirectExit");
            radioMinimizeToTray.Text = ApkDataModel.APKLan("ConfirmExitMinimizeToTray");
            checkNoRetry.Text = ApkDataModel.APKLan("ConfirmExitNoRetry");
            btnOK.Text = ApkDataModel.APKLan("ConfirmExitOk");
            btnCancel.Text = ApkDataModel.APKLan("ConfirmExitCancel");
            checkNoRetry.Checked = bNotRetryPrompt;

            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(2, 2, Width, Height, 10, 10));
                        
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            confirmResult = true;            
            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                Program.CHECK_PROGRAM_EXIT_KEY, radioDirectExit.Checked.ToString(), Program.INI_FILE_PATH);            

            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                Program.CHECK_NOT_RETRY_PROMPT, checkNoRetry.Checked.ToString(), Program.INI_FILE_PATH);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            confirmResult = false;
            this.Close();
        }

        private void MinimizeToTrayConfirmForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(ControlPaint.Light(SystemColors.InactiveBorder, 1.0f)), 5, 5, Width - 10, Height - 10, 5);
        }

        private void MinimizeToTrayConfirmForm_MouseDown(object sender, MouseEventArgs e)
        {
            m_bDragging = true;
            m_ptCur = Cursor.Position;
            m_ptFormPoint = this.Location;
        }

        private void MinimizeToTrayConfirmForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(m_ptCur));
                this.Location = Point.Add(m_ptFormPoint, new Size(diff));
            }
        }

        private void MinimizeToTrayConfirmForm_MouseUp(object sender, MouseEventArgs e)
        {
            m_bDragging = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using Newtonsoft;
using Newtonsoft.Json;
using ApkInstaller.ServiceCorrespond;
using System.Threading;
//using Global;

namespace ApkInstaller
{
    public partial class Login : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect,
                                                            int nTopRect,
                                                            int nRightRect,
                                                            int nBottomRect,
                                                            int nWidthElipse,
                                                            int nHeightElipse);

        string password;
        string username;
        string bRemeber;

        private ApkServiceCall m_service = new ApkServiceCall();
        private Boolean m_bDragging = false;
        private Point m_ptCur;
        private Point m_ptFormPoint;

        class UserServiceData
        {
            public ApkDataModel.USERLEVEL userlevel { get; set; }
        }

        public Login()
        {

           
//             try
//             {
//                 String strValue;
//                 strValue = Program.mINIFileManager.GetINIValue(Program.SECTION_NAME, "LanguageID", Program.INI_FILE_PATH);
//                 Program.LANGUAGEID = Convert.ToInt32(strValue);
//                 Program.SwitchLanguage();
// 
// 
//             }
//             catch(Exception ex)
//             {
//                 ApkDataModel.WriteLogFile("Login", "Login1()", ex.ToString());
//             }
            
            InitializeComponent();

            try
            {
                bRemeber = Program.mINIFileManager.GetINIValue(Global.SECTION_SECURITY, Global.KEY_LOGON_REMEMBER, Program.INI_FILE_PATH);
                username = Program.mINIFileManager.GetINIValue(Global.SECTION_SECURITY, Global.KEY_LOGON_USERNAME, Program.INI_FILE_PATH);
                password = Program.mINIFileManager.GetINIValue(Global.SECTION_SECURITY, Global.KEY_LOGON_PASSWORD, Program.INI_FILE_PATH);
            }
            catch (Exception ex)
            {
                ApkDataModel.WriteLogFile("Login", "Login2()", ex.ToString());
            }

            if (bRemeber.Equals("Yes"))
            {
                chkSavePassword.Checked = true;
                txtUserPassword.Text = password;
            }

            if (!username.Equals(""))
            {
                txtUserName.Text = username;
            }

            this.Text = ApkDataModel.APKLan("APKInstaller");
            this.lblUserName.Text = ApkDataModel.APKLan("UserName");
            this.lblUserPassword.Text = ApkDataModel.APKLan("Password");
            this.chkSavePassword.Text = ApkDataModel.APKLan("RememberMe");
            this.btnOk.Text = ApkDataModel.APKLan("Login");
            this.btnCancel.Text = ApkDataModel.APKLan("Cancel");
            this.lblLoading.Text = ApkDataModel.APKLan("LoggingNow");
            this.lblLoading.Visible = false;

            this.txtUserName.KeyDown += new KeyEventHandler(txtUserName_KeyDown);
            this.txtUserPassword.KeyDown += new KeyEventHandler(txtUserPassword_KeyDown);

            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(2, 2, Width, Height, 10, 10));
        }

        #region Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            LoginUser();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (!username.Equals(""))
            {
                txtUserPassword.Focus();
            }
        }

        private void Login_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(ControlPaint.Light(SystemColors.InactiveBorder, 1.0f)), 5, 5, Width - 10, Height - 10, 5);
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.btnOk.Enabled)
                {
                    LoginUser();
                }
            }
        }

        private void txtUserPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.btnOk.Enabled)
                {
                    LoginUser();
                }
            }
        }

        private void SplashScreen_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
        {
//             if (m_bLayoutCalled == false)
//             {
//                 m_bLayoutCalled = true;
//                 m_dt = DateTime.Now;
                this.Activate();
                SplashScreen.CloseForm();
//             }
        }

        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            m_bDragging = true;
            m_ptCur = Cursor.Position;
            m_ptFormPoint = this.Location;
        }

        private void Login_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(m_ptCur));
                this.Location = Point.Add(m_ptFormPoint, new Size(diff));
            }
        }

        private void Login_MouseUp(object sender, MouseEventArgs e)
        {
            m_bDragging = false;
        }
        #endregion

        #region User Methods
        private void LoginUser()
        {
            if (txtUserName.Text.Equals(""))
            {
                MessageBox.Show(ApkDataModel.APKLan("RequireUsername"), ApkDataModel.APKLan("Notice"));
                txtUserName.Focus();
                return;
            }

            CallUserLogin(txtUserName.Text, txtUserPassword.Text);
            this.lblLoading.Visible = true;
            this.btnOk.Enabled = false;
        }

        public void ValidateUser(ApkResponseData res)
        {
            string errorMsg = APKEERROR.APK_UNKNOWN_ERR;
            if (res != null)
            {
                if (res.SVCC_RET == SERVICEERROR.ERR_SUCCESS)
                {
                    errorMsg = APKEERROR.APK_SUCCESS;
                }
                else if (res.SVCC_RET == SERVICEERROR.ERR_LOGIN_INVALIDUSER)
                {
                    errorMsg = APKEERROR.APK_INVALID_USER;
                }
                else if (res.SVCC_RET == SERVICEERROR.ERR_INCORRECT_PASSWORD)
                {
                    errorMsg = APKEERROR.APK_INCORRECT_PWD;
                }
            }
            else
            {
                errorMsg = APKEERROR.APK_NETWORK_FAIL;
            }

            if (errorMsg == APKEERROR.APK_SUCCESS)
            {
                try
                {
                    UserServiceData levelinfo = JsonConvert.DeserializeObject<UserServiceData>(res.SVCC_DATA.ToString());

                    if (levelinfo != null)
                    {
                        Program.userLevel = levelinfo.userlevel;
                    }
                }
                catch (System.Exception ex)
                {

                }
                Program.SetValidUser(true, res.SVCC_TOKEN);
                Program.UserName = txtUserName.Text;

                Program.mINIFileManager.SetIniValue(Global.SECTION_SECURITY, Global.KEY_LOGON_USERNAME, txtUserName.Text, Program.INI_FILE_PATH);
                if (chkSavePassword.Checked)
                {
                    Program.mINIFileManager.SetIniValue(Global.SECTION_SECURITY, Global.KEY_LOGON_REMEMBER, "Yes", Program.INI_FILE_PATH);
                    Program.mINIFileManager.SetIniValue(Global.SECTION_SECURITY, Global.KEY_LOGON_PASSWORD, txtUserPassword.Text, Program.INI_FILE_PATH);
                }
                else
                {
                    Program.mINIFileManager.SetIniValue(Global.SECTION_SECURITY, Global.KEY_LOGON_REMEMBER, "No", Program.INI_FILE_PATH);
                    Program.mINIFileManager.SetIniValue(Global.SECTION_SECURITY, Global.KEY_LOGON_PASSWORD, "", Program.INI_FILE_PATH);
                }
                this.Close();
            }
            else
            {
                this.lblLoading.Visible = false;
                this.btnOk.Enabled = true;
                MessageBox.Show(errorMsg, ApkDataModel.APKLan("Notice"));
                txtUserName.Focus();
            }
        }

        private void CallUserLogin(string username, string password)
        {
            Dictionary<string, string> apkparam = new Dictionary<string, string>();
            apkparam.Add("username", username);
            apkparam.Add("password", ApkDataModel.GetMD5Hash(password));

            Callback_ServiceData callback = new Callback_ServiceData(this.ValidateUser);

            m_service.CallApkWorker(
                ApkServiceUri.LoginUser,
                HttpMethod.GET,
                apkparam,
                callback
            );
        }
        #endregion

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Login_MouseDown(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Login_MouseMove(sender, e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Login_MouseUp(sender, e);

        }

        private void Login_Shown(object sender, EventArgs e)
        {

        }       
    }
}



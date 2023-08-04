using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Resources;
using System.Reflection;
using ApkInstaller.ServiceCorrespond;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;
using System.Xml;

namespace ApkInstaller
{
    public class Port
    {
        public string name
        {
            get
            {
                return string.Format("{0} ({1} port {2})", this.process_name, this.protocol, this.port_number);
            }
            set { }
        }
        public string port_number { get; set; }
        public int process_id { get; set; }
        public string process_name { get; set; }
        public string protocol { get; set; }
        public string process_status { get; set; }
        public string process_path { get; set; }
    }

    static class Program
    {
        #region Fields and Properties
        public static Boolean m_bIsValidUser = false;
        public static Boolean m_ReadyToUpdate = false;
        public static INIFileManager mINIFileManager = new INIFileManager();
        public static AdbDevicePoll mDevicePoll = new AdbDevicePoll();
        public static AndroidDeviceList mDeviceList = new AndroidDeviceList();
        public static String SECTION_NAME = "Program";
        public static String LANGUAGE_KEY = "LanguageID";
        public static String CHECK_PROGRAM_EXIT_KEY = "CheckProgramExit";
        public static String CHECK_PROGRAM_TASK_KEY = "CheckProgramTask";
        public static String CHECK_PROGRAM_OTHER_KEY = "CheckProgramOther";
        public static String CHECK_NOT_RETRY_PROMPT = "CheckNotRetryPrompt";
        public static String INIFILE_NAME = "apkinstaller.ini";
        public static String PROGRAM_PATH = "";
        public static String INI_FILE_PATH = "";
        public static String otherHelperProcessPath = "";
        public static String APKAGENT_PACKAGENAME = "com.damy.apkagent";
        public static String APKAGENT_FILEPATH = "";
        public static int APKAGENT_VERSIONCODE = 1;
        public static int otherHelperProcessID;

        public static String LOCAL_APP_PATH = "";
        public static String LOCAL_APP_FOLDERNAME = @"apkinstaller";
        //public static String APKSERVICE_URL = @"http://localhost:8249/Service.svc/";
        //public static String APKSERVICE_URL = @"http://218.25.54.28:5051/Service.svc/";
        public static String APKSERVICE_URL = @"http://180.97.80.186:8052/Service.svc/";
        public static String LOCALDB_PATH = "DB\\apkinstaller.db";
        public static String APP_CACHE_PATH = "Cache\\Apps\\";
        public static String IMG_CACHE_PATH = "Cache\\Img\\";
        public static String SOFT_UPDATER = "ApkPLUpdater.exe";
        public static String AUTH_TOKEN = "";

        public static bool chkProgramExit = true;
        public static bool chkProgramTask = false;
        public static bool chkProgramOther = false;

        public static String UserName;
        public static ApkDataModel.USERLEVEL userLevel = ApkDataModel.USERLEVEL.LEVEL2;
        public static int SOFT_VERCODE = 32;
        public static String SOFT_VERNAME = "1.0.32";
        public static String NEW_VERNAME = "";

        public static ApkInstaller.AppControl.Callback_AllInstallCompleted m_callbackInstComp;

        public static ResourceManager ResMan;
        public static CultureInfo ApkCulture = null;

        public static int LANGUAGEID = (int)ApkDataModel.ApkLanguage.CHINESE;
        
        #endregion

        [STAThread]
        static void Main()
        {
            String strLang;
            
//             SplashScreen.ShowSplashScreen();
//             Application.DoEvents();

            PROGRAM_PATH = Application.StartupPath; //can use instead of this: System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
            

            INI_FILE_PATH = PROGRAM_PATH + "\\" + INIFILE_NAME;
            APKAGENT_FILEPATH = Program.PROGRAM_PATH + "\\" + "APKAgent.apk";

            ResMan = new ResourceManager("ApkInstaller.i18n.Language", Assembly.GetExecutingAssembly());

            LOCAL_APP_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + LOCAL_APP_FOLDERNAME;            

            //WriteAcl(Application.ExecutablePath);

            try
            {
                strLang = mINIFileManager.GetINIValue(SECTION_NAME, "LanguageID", INI_FILE_PATH);
                if (String.IsNullOrEmpty(strLang))
                {
                    strLang = "0";
                }
                LANGUAGEID = Convert.ToInt32(strLang);
                SwitchLanguage();

                strLang = mINIFileManager.GetINIValue(SECTION_NAME, CHECK_PROGRAM_EXIT_KEY, INI_FILE_PATH);
                if (String.IsNullOrEmpty(strLang))
                {
                    mINIFileManager.SetIniValue(SECTION_NAME, CHECK_PROGRAM_EXIT_KEY, "True", INI_FILE_PATH);
                    strLang = "0";
                }
                chkProgramExit = Convert.ToBoolean(strLang);

                strLang = mINIFileManager.GetINIValue(SECTION_NAME, 
                    CHECK_PROGRAM_TASK_KEY, INI_FILE_PATH);
                if (String.IsNullOrEmpty(strLang))
                {
                    Program.mINIFileManager.SetIniValue(Program.SECTION_NAME, 
                        CHECK_PROGRAM_TASK_KEY, "False", Program.INI_FILE_PATH);
                    strLang = "0";
                }
                chkProgramTask = Convert.ToBoolean(strLang);

                strLang = mINIFileManager.GetINIValue(SECTION_NAME, CHECK_PROGRAM_OTHER_KEY, INI_FILE_PATH);
                if (String.IsNullOrEmpty(strLang))
                {
                    mINIFileManager.SetIniValue(SECTION_NAME, CHECK_PROGRAM_OTHER_KEY, "False", INI_FILE_PATH);
                    strLang = "0";
                }
                chkProgramOther = Convert.ToBoolean(strLang);
            }
            catch (System.Exception ex)
            {
            }

            try
            {
                if (!Directory.Exists(LOCAL_APP_PATH))
                {
                    Directory.CreateDirectory(LOCAL_APP_PATH);
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("Program", "Main()", ex.ToString());
            }

            Application.EnableVisualStyles();

            CheckNewVersion();

            SingleApplication.Run(new Login());

            if (m_bIsValidUser)
            {
                MainForm frmMain = new MainForm();
                Application.Run(frmMain);
            }
        }

        #region Public Methods
        public static bool CheckTrialVersion()
        {
            string xmlstr = GetYahooCurrTime();

            try
            {
                if (String.IsNullOrEmpty(xmlstr))
                {
                    return false;
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlstr);

                XmlNodeList tnode = doc.DocumentElement.GetElementsByTagName("Timestamp");
                string currtime = tnode[0].InnerText;
                
                long currtimestamp = long.Parse(currtime);

                TimeSpan tspan = (new DateTime(2014, 1, 13, 0, 0, 0) - new DateTime(1970, 1, 1, 0, 0, 0, 0));

                if (currtimestamp < tspan.TotalSeconds)
                {
                    return true;
                }
                
            }
            catch (System.Exception ex)
            {
            	
            }

            return false;
        }
        public static string GetYahooCurrTime()
        {
            string reqUrl = "http://developer.yahooapis.com/TimeService/V1/getTime?appid=YahooDemo";
            string responseBody = null;
            string contentType = "application/text";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            req.Method = HttpMethod.GET;
            req.ContentType = contentType;

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                ApkDataModel.WriteLogFile("ApkServiceCall", "ApkSendRequest()", ex.ToString());
                return null;
            }

            Stream respStream = resp.GetResponseStream();
            if (respStream != null)
            {
                responseBody = new StreamReader(respStream).ReadToEnd();
                return responseBody;
            }

            return null;
        }

        public static void CheckNewVersion()
        {
            Dictionary<string, string> vparam = new Dictionary<string, string>();
            vparam.Add("vcode", SOFT_VERCODE.ToString());
            ApkResponseData newverData = ApkServiceCall.ApkSendRequest(ApkServiceUri.CheckUpdateVersion, HttpMethod.GET, vparam);
            if (newverData != null)
            {
                if (newverData.SVCC_RET == SERVICEERROR.ERR_SUCCESS)
                {
                    SingleApplication.Run(new ApkUpdater(newverData));

                    if (m_ReadyToUpdate)
                    {
                        ProcessStartInfo info = new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            FileName = PROGRAM_PATH + "\\" + SOFT_UPDATER,

                        };
                        Process.Start(info);
                        Application.Exit();
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            else
            {

            }
        }

        public static String GetSizeStrAuto(Int64 size)
        {
            String result = "";
            if (size < 1000 * 1024)
                result = ((float)size / 1024L).ToString("N2") + "KB";
            else if (size < 1000 * 1024 * 1024L)
                result = ((float)size / (1024.0 * 1024)).ToString("N2") + "MB";
            else
                result = ((float)size / (1024.0 * 1024 * 1024)).ToString("N2") + "GB";
            return result;
        }

        public static void SetValidUser(Boolean bIsValid, String token)
        {
            AUTH_TOKEN = token;
            m_bIsValidUser = bIsValid;
        }

        public static void SetValidUpdate(Boolean bIsValid)
        {
            m_ReadyToUpdate = bIsValid;
        }

        public static Boolean getIsValidUser()
        {
            return m_bIsValidUser;
        }

        public static void SwitchLanguage()
        {
            if (Program.LANGUAGEID == (int)ApkDataModel.ApkLanguage.CHINESE)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }            

            foreach (Form form in Application.OpenForms)
            {
                Type formtype = form.GetType();
                if (formtype == typeof(MainForm))
                {
//                     Button btnApp = (Button)(form.Controls.Find("btnApplication", true)[0]);
//                     btnApp.Text = ApkDataModel.APKLan("Application");

                    MainForm myMainForm = (MainForm)form;
                    myMainForm.SwitchLanguage();
                    Button btnDataAnal = (Button)(form.Controls.Find("btnDataAnalysis", true)[0]);
                    btnDataAnal.Text = ApkDataModel.APKLan("DataAnalysis");
                }
                else if (formtype == typeof(AppControl))
                {
                    AppControl myAppControl = (AppControl)form;
                    myAppControl.SwitchLanguage();
                }
                else if (formtype == typeof(DataAnalysis))
                {
                    DataAnalysis myDataAnalysis = (DataAnalysis)form;
                    myDataAnalysis.SwitchLanguage();
                }
            }
        }

        #endregion

        private static void WriteAcl(string filename)
        {
            //Set security for EveryOne Group
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            IdentityReference userIdentity = sid.Translate(typeof(NTAccount));

            var AccessRule_AllowEveryOne = new FileSystemAccessRule(userIdentity, FileSystemRights.FullControl, AccessControlType.Allow);
            var securityDescriptor = new FileSecurity();
            securityDescriptor.SetAccessRule(AccessRule_AllowEveryOne);
            File.SetAccessControl(filename, securityDescriptor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ApkInstaller.ServiceCorrespond;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace ApkInstaller
{
    public partial class ApkUpdater : Form
    {
        private ApkResponseData m_respData;
        private PatchFiles m_patchInfo;
        private BackgroundWorker m_downWorker;
        private String TempDir = Program.PROGRAM_PATH + "\\tmp";
        private String PatchFilePath = "";

        private DateTime lastUpdate;
        long lastBytes = 0, currBytes = 0;

        public ApkUpdater(ApkResponseData response)
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lbl_speed.Visible = false;
            lbl_speed.Text = "";
            m_respData = response;
            m_patchInfo = JsonConvert.DeserializeObject<PatchFiles>(response.SVCC_DATA.ToString());
            String[] tmps = m_patchInfo.PatchFilePath.Split('/');

            if (tmps.Count() > 1)
            {
                PatchFilePath = TempDir + "\\" + tmps[tmps.Count() - 1];
            }
        }

        private void ApkUpdater_Load(object sender, EventArgs e)
        {
        }

        private void ApkUpdater_Shown(object sender, EventArgs e)
        {
            lbl_speed.Visible = true;
            CallPatchDownWorker();
        }

        public void CallPatchDownWorker()
        {
            m_downWorker = new BackgroundWorker();
            m_downWorker.WorkerReportsProgress = true;
            m_downWorker.WorkerSupportsCancellation = true;

            lblTask.Text = "更新进度【1/2】 - 正在下载更新文件...";
            m_downWorker.DoWork += new DoWorkEventHandler(PatchDown_DoWork);

            m_downWorker.ProgressChanged += new ProgressChangedEventHandler(PatchDown_ProgressChanged);
            m_downWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PatchDown_Completed);

            m_downWorker.RunWorkerAsync();
        }

        public void PatchDown_DoWork(object sender, DoWorkEventArgs e)
        {
            DownloadApkFileParam sparam = (DownloadApkFileParam)e.Argument;
            HttpWebRequest webRequest;
            HttpWebResponse webResponse;
            Stream strResponse;
            Stream strLocal;

            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create(m_respData.SVCC_BASEURL + m_patchInfo.PatchFilePath);
                webRequest.Timeout = 180 * 1000;    //3 minutes
                webRequest.ReadWriteTimeout = 60 * 1000;
                webRequest.KeepAlive = true;

                lastUpdate = DateTime.Now;
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                // Retrieve the response from the server
                try
                {
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("ApkUpdater", "PatchDown_DoWork()", ex.ToString());
                    return;
                }
                // Ask the server for the file size and store it
                long fileSize = webResponse.ContentLength;

                // Open the URL for download 

                strResponse = webResponse.GetResponseStream();

                if (!Directory.Exists(TempDir))
                {
                    Directory.CreateDirectory(TempDir);
                }

//                 if (File.Exists(PatchFilePath))
//                 {
//                     File.Delete(PatchFilePath);
//                 }

                strLocal = new FileStream(PatchFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // It will store the current number of bytes we retrieved from the server
                int bytesSize = 0;
                // A buffer for storing and writing the data retrieved from the server
                byte[] downBuffer = new byte[2048];

                // Loop through the buffer until the buffer is empty
                while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    if (m_downWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    // Write the data from the buffer to the local hard drive
                    strLocal.Write(downBuffer, 0, bytesSize);
                    // Invoke the method that updates the form's label and progress bar

                    currBytes = strLocal.Length;
                    m_downWorker.ReportProgress((int)Math.Round((double)strLocal.Length / fileSize * 100));
                }

                // When the above code has ended, close the streams
                try
                {
                    strResponse.Close();
                    strLocal.Close();
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("AppControl", "DownloadApkThread()", ex.ToString());
                }
            }
            catch (System.Exception ex)
            {
                ApkDataModel.WriteLogFile("AppControl", "DownloadApkThread()", ex.ToString());
            }
            finally
            {

            }

            //DownloadApkThread();
        }

        public void PatchDown_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgsDetail.Value = e.ProgressPercentage;

            var now = DateTime.Now;
            var timeSpan = now - lastUpdate;
            var byteChange = currBytes - lastBytes;

            if (byteChange == 0 || timeSpan.Seconds == 0)
            {
//                 lastBytes = currBytes;
//                 lastUpdate = now;
                return;
            }
            double bytesPerSecond = byteChange / timeSpan.Seconds;

            lastBytes = currBytes;
            lastUpdate = now;

            lbl_speed.Text = (Math.Round(bytesPerSecond / 1024, 2)).ToString() + " KB/Sec";
        }

        public void PatchDown_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }
            else if (e.Error != null)
            {
                throw e.Error;
            }
            else
            {
                if (File.Exists(PatchFilePath))
                {
                    lbl_speed.Visible = false;
                    FileInfo patchFile = new FileInfo(PatchFilePath);

                    if (patchFile.Length == m_patchInfo.PatchFileSize)
                    {
                        lblTask.Text = "更新进度【2/2】 - 解压更新文件...";
                        bool bReadyToUpdate = CheckPatchFiles(PatchFilePath);

                        Program.SetValidUpdate(bReadyToUpdate);
                    }
                    else
                    {
                        File.Delete(PatchFilePath);
                    }
                }
            }
            this.Close();
        }

        private bool CheckPatchFiles(String filepath)
        {
            bool rst = true;
            if (UnzipPatch(filepath))
            {
                try
                {
                    foreach (PatchFileInfo item in m_patchInfo.PatchList)
                    {
                        if (File.Exists(TempDir + "\\" + item.filepath))
                        {
                            FileInfo finfo = new FileInfo(TempDir + "\\" + item.filepath);
                            if (finfo.Length != item.filesize)
                            {
                                rst = false;
                                break;
                            }
                            else if (item.filepath.Contains(Program.SOFT_UPDATER))
                            {
                                File.Delete(Program.PROGRAM_PATH + "\\" + Program.SOFT_UPDATER);
                                File.Copy(TempDir + "\\" + item.filepath, Program.PROGRAM_PATH + "\\" + Program.SOFT_UPDATER);
                            }

                        }
                        else
                        {
                            rst = false;
                            break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    rst = false;                	
                }
            }

            return rst;
        }

        private bool UnzipPatch(String filepath)
        {
            bool ret = false;
            try
            {
                using (ZipInputStream zipIn = new ZipInputStream(File.OpenRead(filepath)))
                {
                    ZipEntry entry;
                    while ((entry = zipIn.GetNextEntry()) != null)
                    {
                        if (entry.Name.EndsWith("/"))
                        {
                            Directory.CreateDirectory(String.Format("{0}\\{1}", TempDir, entry.Name.Replace(@"/", @"\")));
                        }
                        else
                        {
                            FileStream streamWriter = File.Create(String.Format("{0}\\{1}", TempDir, entry.Name.Replace(@"/", @"\")));
                            long size = entry.Size;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = zipIn.Read(data, 0, data.Length);
                                if (size > 0) streamWriter.Write(data, 0, (int)size);
                                else break;
                            }
                            streamWriter.Close();
                        }
                    }
                    ret = true;
                }
            }
            catch (System.Exception ex)
            {
                ret = false;
            }

            return ret;
        }
    }
}

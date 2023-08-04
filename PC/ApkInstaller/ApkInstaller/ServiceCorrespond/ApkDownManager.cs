using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Threading;
using ApkInstaller.SQLiteCorrespond;

namespace ApkInstaller.ServiceCorrespond
{
    public class DownloadApkFileParam
    {
        public long currIndex { get; set; }
        public long startPoint { get; set; }
        public string srcFilePath { get; set; }
        public string targetFilePath { get; set; }
        public AppControl appControl { get; set; }
    }

    class ApkDownManager
    {
        public bool isStop = false;
        public BackgroundWorker m_downWorker;
        public DownloadApkFileParam m_currApkInfo;

        public void CallApkDownWorker(DownloadApkFileParam downparam)
        {
            m_downWorker = new BackgroundWorker();
            m_downWorker.WorkerReportsProgress = true;
            m_downWorker.WorkerSupportsCancellation = true;

            m_currApkInfo = downparam;

            m_downWorker.DoWork += new DoWorkEventHandler(ApkDown_DoWork);

            m_downWorker.ProgressChanged += new ProgressChangedEventHandler(ApkDown_ProgressChanged);
            m_downWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ApkDown_Completed);

            m_downWorker.RunWorkerAsync(downparam);
        }

        public void ApkDown_DoWork(object sender, DoWorkEventArgs e)
        {
            DownloadApkFileParam sparam = (DownloadApkFileParam)e.Argument;
            HttpWebRequest webRequest;
            HttpWebResponse webResponse;
            Stream strResponse;
            Stream strLocal;

            try
            {
                // Put the object argument into an int variable
                int startPointInt = Convert.ToInt32(m_currApkInfo.startPoint);
                // Create a request to the file we are downloading
                webRequest = (HttpWebRequest)WebRequest.Create(m_currApkInfo.srcFilePath);
                // Set the starting point of the request
                webRequest.AddRange(startPointInt);
                webRequest.Timeout = 180 * 1000;    //3 minutes
                webRequest.ReadWriteTimeout = 60 * 1000;
                webRequest.KeepAlive = true;

                // Set default authentication for retrieving the file
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                // Retrieve the response from the server
                try
                {
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                }
                catch (System.Exception ex)
                {
                    ApkDataModel.WriteLogFile("ApkDownManager", "ApkDown_DoWork()", ex.ToString());
                    return;
                }
                // Ask the server for the file size and store it
                Int64 fileSize = webResponse.ContentLength;

                if (m_downWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Open the URL for download 

                using (strResponse = webResponse.GetResponseStream())
                {
                    if (startPointInt == 0)
                    {
                        strLocal = new FileStream(m_currApkInfo.targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    }
                    else
                    {
                        strLocal = new FileStream(m_currApkInfo.targetFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                    }

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
                        if (m_downWorker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                        m_downWorker.ReportProgress((int)Math.Round((double)strLocal.Length / (long)(fileSize + startPointInt) * 100));
                    }

                    strLocal.Close();
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

        public void DownloadApkThread()
        {
            //DownloadApkFileParam sparam = (DownloadApkFileParam)args;

        }

        public void ApkDown_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Calculate the download progress in percentages
            try
            {
                m_currApkInfo.appControl.m_arrPercent[m_currApkInfo.currIndex].Value = e.ProgressPercentage;
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        public void ApkDown_Completed(object sender, RunWorkerCompletedEventArgs e)
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
                ApkAdbParam apkinfo = ApkParser.GetApkInformation(m_currApkInfo.targetFilePath);

                m_currApkInfo.appControl.SetApkInformation(m_currApkInfo.targetFilePath, apkinfo);
                m_currApkInfo.appControl.m_arrPercent[m_currApkInfo.currIndex].Visible = false;
            }

            try
            {
                m_currApkInfo.appControl.m_sqlMan.m_apkList[(int)m_currApkInfo.currIndex].status |= ApkDownStatus.COMPLETED_FILE;
            }
            catch (System.Exception ex)
            {
                string exx = ex.ToString();
            }

        }
    }
}

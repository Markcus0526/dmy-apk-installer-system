using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading;

namespace ApkInstaller.ServiceCorrespond
{
    public delegate void Callback_ServiceData(ApkResponseData res);

    class ApkServiceCall
    {
        public BackgroundWorker m_srvWorker;
        public AppControl.ApkWorkerStatus isDone = AppControl.ApkWorkerStatus.FINISHED;
        private String m_strErr;
        private Callback_ServiceData retCallback;

        public class ApkServiceParam
        {
            public string uri { get; set; }
            public string method { get; set; }
            public object apkparam { get; set; }
            public Callback_ServiceData apkCallback { get; set; }
        }

        public ApkServiceCall()
        {

        }

        public static ApkResponseData ApkSendRequest(string uri, string method, object apkparam)
        {
            ApkResponseData result = null;
            string reqUrl = Program.APKSERVICE_URL + uri;
            string responseBody = null;
            string contentType = "application/json";
            string body = "";

            if (method == HttpMethod.GET)
            {
                var tmpparam = (Dictionary<string, string>)apkparam;
                foreach (var item in tmpparam)
                {
                    if (!String.IsNullOrEmpty(body))
                    {
                        body += "&";
                    }
                    body += item.Key + "=" + item.Value;
                }
                reqUrl += "?" + body;
            }
            else if (method == HttpMethod.POST)
            {
                body = JsonConvert.SerializeObject(apkparam);
            }

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            req.Method = method;
            //req.Headers.Add("Cache-Control", "no-cache\r\n");
            req.ContentType = contentType;

            if (method == HttpMethod.POST)
            {
                try
                {
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                    req.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
                    req.GetRequestStream().Close();
                }
                catch (System.Exception ex)
                {
                    //throw new InvalidOperationException(ex.ToString());
                    ApkDataModel.WriteLogFile("ApkServiceCall", "ApkSendRequest()", ex.ToString());
                    return null;
                }
            }

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                ApkDataModel.WriteLogFile("ApkServiceCall", "ApkSendRequest()", ex.ToString());
                //resp = (HttpWebResponse)e.Response;
                return null;
            }

            Stream respStream = resp.GetResponseStream();
            if (respStream != null)
            {
                responseBody = new StreamReader(respStream).ReadToEnd();
                result = JsonConvert.DeserializeObject<ApkResponseData>(responseBody);
            }
            else
            {
                //Console.WriteLine("HttpWebResponse.GetResponseStream returned null");
            }

            return result;
        }

        public void CallApkWorker(string uri, string method, object apkparam, Callback_ServiceData callback)
        {
            m_srvWorker = new BackgroundWorker();
            m_srvWorker.WorkerReportsProgress = true;
            m_srvWorker.WorkerSupportsCancellation = true;

            m_srvWorker.DoWork += new DoWorkEventHandler(ApkSrvWorker_DoWork);
            m_srvWorker.ProgressChanged += new ProgressChangedEventHandler(ApkSrvWorker_ProgressChanged);
            m_srvWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ApkSrvWorker_RunWorkerCompleted);

            // 작업쓰레드 시작
            retCallback = callback;
            m_srvWorker.RunWorkerAsync(new ApkServiceParam
            {
                uri = uri,
                method = method,
                apkparam = apkparam,
                apkCallback = callback
            });
        }

        private void ApkSrvWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ApkServiceParam sparam = (ApkServiceParam)e.Argument;
            //m_srvWorker.ReportProgress(i, filename);

            isDone = AppControl.ApkWorkerStatus.BUSY;
            m_strErr = "";
            ApkResponseData ret = ApkSendRequest(sparam.uri, sparam.method, sparam.apkparam);
            if (m_srvWorker.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
            e.Result = Tuple.Create<ApkResponseData>(ret);
        }

        private void ApkSrvWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //string.Format("Progress : {0} %", e.ProgressPercentage);
        }

        private void ApkSrvWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ApkResponseData result = null;
            if (e.Cancelled)
            {
                
            }
            else if (e.Error != null)
            {
                m_strErr = e.Error.ToString();
            }
            else
            {
                Tuple<ApkResponseData> res = e.Result as Tuple<ApkResponseData>;
                result = res.Item1;
            }

            isDone = AppControl.ApkWorkerStatus.FINISHED;
            retCallback(result);
        }

        public string GetErrorMesage()
        {
            return m_strErr;
        }
//         private void ApkSrvCallback_Completed(IAsyncResult iasRes)
//         {
//             this.isDone = true;
//         }

    }
}

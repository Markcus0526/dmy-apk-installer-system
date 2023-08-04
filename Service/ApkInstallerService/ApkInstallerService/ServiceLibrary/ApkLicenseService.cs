using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;

namespace ApkInstallerService.ServiceLibrary
{
    public class ApkLicenseService
    {
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
            req.Method = "GET";
            req.ContentType = contentType;

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
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
    }
}
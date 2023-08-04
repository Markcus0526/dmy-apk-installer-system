using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Web.Security;
using System.Security.Policy;
using System.Web.Mvc;

namespace ManageSite.Models
{
    public class SearchResult
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }
    public static class SearchModel
    {

        private static IList<XmlNode> ExamplesAsXmlNodes
        {
            get
            {
                IList<XmlNode> exampleList = (IList<XmlNode>)HttpRuntime.Cache["ExamplesXmlNodes"];
                if (exampleList == null)
                {
                    HttpRuntime.Cache["ExamplesXmlNodes"] = exampleList = new List<XmlNode>();
                    XmlDocument examples = new XmlDocument();
                    examples.Load(HttpContext.Current.Server.MapPath("~/Web.sitemap"));

                    foreach (XmlNode node in examples.SelectNodes("//siteMapNode"))
                    {
                        if (node.Attributes["controller"] == null)
                            continue;

                        if (node.Attributes["controller"].Value.ToLowerInvariant() == "home")
                            continue;

                        exampleList.Add(node);
                    }
                }

                return exampleList;
            }
        }

        private static IList<XmlNode> LeftNavAsXmlNodes()
        {
            IList<XmlNode> navList = new List<XmlNode>();

            XmlDocument navs = new XmlDocument();
            navs.Load(HttpContext.Current.Server.MapPath("~/Web.sitemap"));

            foreach (XmlNode node in navs.SelectNodes("//siteMapNode"))
            {
                if (node.Attributes["title"] != null)
                {
                    navList.Add(node);
                }
            }

            return navList;
        }

        private static bool ContainsIgnoreCase(this string source, string reference)
        {
            return source.ToUpperInvariant().Contains(reference.ToUpperInvariant());
        }

        public static bool IsStringInTwoArray(string[] arrayA, string[] arrayB)
        {

            //outer loop for all the elements in arrayA[i]
            for (int i = 0; i < arrayA.Length; i++)
            {
                //inner loop for all the elements in arrayB[j]
                for (int j = 0; j < arrayB.Length; j++)
                {
                    //compare arrayA to arrayB and output results
                    if (arrayA[i] == arrayB[j])
                    {
                        return true;
                    }
                }
                //set foundSwitch bool back to false
            }
            return false;
        }

        public static IEnumerable<SearchResult> Filter(string text)
        {
            if (string.IsNullOrEmpty(text.Trim()))
            {
                return new List<SearchResult>();
            }

            IList<XmlNode> tmpResult = ExamplesAsXmlNodes;

            IList<SearchResult> results = new List<SearchResult>();

            string[] currentUserRoles = Roles.GetRolesForUser();

            foreach (XmlNode node in tmpResult)
            {


                string[] siteMapRoles = null;
                if (node.Attributes["roles"].Value != null)
                {
                    siteMapRoles = node.Attributes["roles"].Value.Split(',');
                }

                if (IsStringInTwoArray(currentUserRoles, siteMapRoles))
                {
                    results.Add(new SearchResult
                    {
                        Text = string.Format("{0} >> {1}", node.ParentNode.Attributes["title"].Value, node.Attributes["title"].Value),
                        Url = string.Format("{0}/{1}", node.Attributes["controller"].Value.ToLowerInvariant(), node.Attributes["action"].Value.ToLowerInvariant())
                    });
                }
            }

            foreach (string token in text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                results = results.Where(result => result.Text.ContainsIgnoreCase(token.Trim())).ToList();
            }

            return results.AsEnumerable();
        }

        public static string GenerateLeftNavigation(string mainitem, string selected, string rootUri)
        {
            //bool isSelectedItem = false;
            string retHTML = "";

            IList<XmlNode> tmpResult = LeftNavAsXmlNodes();

            if (!String.IsNullOrEmpty(mainitem.Trim()))
            {
                foreach (XmlNode node in tmpResult)
                {
                    string liclass = "";
                    if (node.Attributes["status"].Value.ToLowerInvariant() == "main") {
                        if (String.IsNullOrEmpty(retHTML))
                        {
                            liclass = "start";
                        }
                        if (mainitem == node.Attributes["title"].Value)
                        {
                            liclass += " active";
                        }
                        retHTML += "<li class='" + liclass + "'>";
                    }
                    if (node.HasChildNodes)
                    {
                        
                    }
                    if (String.IsNullOrEmpty(retHTML)) {

                    }
#if false
                    if (node.Attributes["title"].Value.ToLowerInvariant() == "compinfo" || node.Attributes["controller"].Value.ToLowerInvariant() == "gift" || node.Attributes["controller"].Value.ToLowerInvariant() == "accumulate")
                    {
                        isSelectedItem = node.Attributes["action"].Value.ToLowerInvariant() == selected.ToLowerInvariant() ? true : false;
                    }
                    else
                    {
                        isSelectedItem = node.Attributes["controller"].Value.ToLowerInvariant() == selected.ToLowerInvariant() ? true : false;
                    }
                    retHTML += "<li class='t-item " + (isSelectedItem ? " active-page" : " t-state-default") + "'><a class='t-link ";
                    retHTML += (isSelectedItem ? " t-state-selected" : "");
                    retHTML += "' href='";

                    if (node.Attributes["controller"].Value.ToLowerInvariant() == "compinfo" || node.Attributes["controller"].Value.ToLowerInvariant() == "gift")
                    {
                        retHTML += rootUri + node.Attributes["controller"].Value.ToLowerInvariant() + "/" + node.Attributes["action"].Value.ToLowerInvariant();
                    }
                    else
                    {
                        retHTML += string.Format("{0}{1}/{2}", rootUri, node.Attributes["controller"].Value.ToLowerInvariant(), node.Attributes["action"].Value.ToLowerInvariant());
                    }
                    retHTML += "'>" + node.Attributes["title"].Value + "</a></li>";
#endif
                }
            }

            return retHTML;
        }
    }
}

#region Using directives

using System.Text;
using System.Web;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// MvcSiteMapHtmlHelper extension methods
    /// </summary>
    public static class LeftMenuHelper
    {

        public static bool IsActivateNode(SiteMapNode rootNode)
        {
            bool rst = false;
            SiteMapProvider selectedSiteMapProvider = System.Web.SiteMap.Providers[System.Web.SiteMap.Provider.Name];
            // Check for provider
            if (selectedSiteMapProvider == null)
            {
                throw new UnknownSiteMapProviderException(string.Format("Unknown SiteMap provider: {0}", System.Web.SiteMap.Provider.Name));
            }
            SiteMapNode currentNode = MenuHelper.GetCurrentNode(selectedSiteMapProvider);

            if (currentNode != null)
            {
                if (rootNode.HasChildNodes)
                {
                    foreach (SiteMapNode node in rootNode.ChildNodes)
                    {
                        if (currentNode.Url == node.Url)
                        {
                            rst = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (currentNode.Url == rootNode.Url)
                    {
                        rst = true;
                    }
                }
            }

            return rst;
        }

        /// <summary>
        /// Build a sitemap tree, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="rootNode">Root node</param>
        /// <param name="cssClass">SiteMap CSS class</param>
        /// <param name="renderListTag">Render a list tag?</param>
        /// <returns>Html markup</returns>
        public static string SiteMap(this MvcSiteMapHtmlHelper helper, SiteMapNode rootNode, string cssClass)
        {
            // String builder
            var sb = new StringBuilder();

            // Mvc node
            var mvcNode = rootNode as MvcSiteMapNode;
            var nodeLevel = mvcNode.GetNodeLevel();
            // Render root node
            if (nodeLevel == 0)
            {
                return "";
            }

            string cssActivated = (IsActivateNode(rootNode)) ? cssClass : "";
            sb.Append(string.Format("<li class='{0}'>", cssActivated));
            if (nodeLevel == 1)
            {
                if (mvcNode.HasChildNodes)
                {
                    sb.Append(string.Format("<a href=\"javascript:;\">"));
                }
                else
                {
                    sb.Append(string.Format("<a href=\"{0}\">", rootNode.Url));
                }
                sb.Append(string.Format("<i class=\"{0}\"></i> <span class=\"title\">{1}</span>", mvcNode.Icon, mvcNode.Title));

                if (mvcNode.HasChildNodes)
                {
                    sb.Append(string.Format("<span class=\"arrow {0}\"></span>", (IsActivateNode(rootNode)) ? "open" : ""));
                }
                sb.Append("</a>");
            }
            else
            {
                sb.Append(string.Format("<a href=\"{0}\">{1}</a>", rootNode.Url, rootNode.Title));
            }

            if (rootNode.HasChildNodes)
            {
                sb.Append("<ul class=\"sub-menu\">");
                sb.AppendLine();
                foreach (SiteMapNode node in rootNode.ChildNodes)
                {
                    if (node.IsAccessibleToUser(HttpContext.Current))
                    {
                        sb.Append(SiteMap(helper, node, cssClass));
                    }
                }
                sb.Append("</ul>");
            }
                
            sb.AppendLine("</li>");

            return sb.ToString();
        }

        public static string LeftNavigation(this MvcSiteMapHtmlHelper helper, SiteMapNode rootNode, string cssClass)
        {
            // String builder
            var sb = new StringBuilder();

            // Mvc node
            var mvcNode = rootNode as MvcSiteMapNode;

            if (rootNode.HasChildNodes)
            {
                sb.AppendLine();
                foreach (SiteMapNode node in rootNode.ChildNodes)
                {
                    if (node.IsAccessibleToUser(HttpContext.Current))
                    {
                        sb.Append(SiteMap(helper, node, cssClass));
                    }
                }
            }

            return sb.ToString();
        }
    }
}

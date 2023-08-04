#region Using directives

using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Routing;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// MvcSiteMapHtmlHelper extension methods
    /// </summary>
    public static class SiteMapPathHelper
    {
        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper)
        {
            return SiteMapPath(helper, " > ", "separator", "link", false);
        }

        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " > "</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper, string separator)
        {
            return SiteMapPath(helper, separator, "separator", "link", false);
        }

        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " &gt; "</param>
        /// <param name="currentNodeAsLink">Can the current SiteMap node be displayed as a link?</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper, string separator, bool currentNodeAsLink)
        {
            return SiteMapPath(helper, separator, "separator", "link", currentNodeAsLink);
        }

        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " &gt; "</param>
        /// <param name="separatorCssClass">CSS class for separator</param>
        /// <param name="linkCssClass">CSS class for links</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper, string separator, string separatorCssClass, string linkCssClass)
        {
            return SiteMapPath(helper, separator, separatorCssClass, linkCssClass, false);
        }

        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " &gt; "</param>
        /// <param name="separatorCssClass">CSS class for separator</param>
        /// <param name="linkCssClass">CSS class for links</param>
        /// <param name="currentNodeAsLink">Can the current SiteMap node be displayed as a link?</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper, string separator, string separatorCssClass, string linkCssClass, bool currentNodeAsLink)
        {
            return SiteMapPath(helper, separator, separatorCssClass, linkCssClass, currentNodeAsLink, null);
        }

        /// <summary>
        /// Gets SiteMap path for the current request
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " &gt; "</param>
        /// <param name="separatorCssClass">CSS class for separator</param>
        /// <param name="linkCssClass">CSS class for links</param>
        /// <param name="currentNodeAsLink">Can the current SiteMap node be displayed as a link?</param>
        /// <param name="htmlAttributes">The HTML attributes for the rendered links.</param>
        /// <returns>SiteMap path for the current request</returns>
        public static string SiteMapPath(this MvcSiteMapHtmlHelper helper, string separator, string separatorCssClass, string linkCssClass, bool currentNodeAsLink, object htmlAttributes)
        {
            IDictionary<string, object> htmlAttributesDictionary = new RouteValueDictionary(htmlAttributes);

            var pages = new List<string>();

            var node = SiteMap.CurrentNode;
            while (node != null)
            {
                var mvcNode = node as MvcSiteMapNode;
                if (node.IsAccessibleToUser(HttpContext.Current))
                {
                    if (mvcNode != null && mvcNode.Clickable == false)
                    {
                        pages.Add(SiteMapText(helper, node, linkCssClass, htmlAttributesDictionary));
                    }
                    else
                    {
                        pages.Add(SiteMapLink(helper, node, linkCssClass, currentNodeAsLink, htmlAttributesDictionary));
                    }

                }
                node = node.ParentNode;
            }
            pages.Reverse();

            var separatorHtml = "";
            /*
            if (!string.IsNullOrEmpty(separatorCssClass))
            {
                separatorHtml = string.Format("<span class=\"{0}\">{1}</span>", separatorCssClass, separator);
            }
            else
            {
                separatorHtml = separator;
            }
            */

            //return "<span class=\"siteMapPath\">" + string.Join(separatorHtml, pages.ToArray()) + "</span>";
            return string.Join(separatorHtml, pages.ToArray());
        }

        /// <summary>
        /// Gets a SiteMap text
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="node">The node to get a SiteMap link for</param>
        /// <param name="linkCssClass">The link CSS class.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>SiteMap text for the specified node</returns>
        private static string SiteMapText(this MvcSiteMapHtmlHelper helper, SiteMapNode node, string linkCssClass, IDictionary<string, object> htmlAttributes)
        {
            var extraAttributes = new StringBuilder();
            foreach (var attribute in htmlAttributes)
            {
                extraAttributes.Append(" " + attribute.Key + "=\"" + attribute.Value + "\"");
            }

            string spanHtml;
            if (!string.IsNullOrEmpty(linkCssClass))
            {
                spanHtml = string.Format("<span class=\"{0}\"{1}>{2}</span>", linkCssClass, extraAttributes, helper.HtmlHelper.Encode(node.Title));
            }
            else
            {
                spanHtml = string.Format("<span{1}>{0}</span>", helper.HtmlHelper.Encode(node.Title), extraAttributes);
            }

            return spanHtml;
        }

        /// <summary>
        /// Gets a SiteMap link
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="node">The node to get a SiteMap link for</param>
        /// <param name="linkCssClass">The link CSS class.</param>
        /// <param name="currentNodeAsLink">Can the current SiteMap node be displayed as a link?</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>SiteMap link for the specified node</returns>
        private static string SiteMapLink(this MvcSiteMapHtmlHelper helper, SiteMapNode node, string linkCssClass, bool currentNodeAsLink, IDictionary<string, object> htmlAttributes)
        {
            string spanHtml;
            /*
            if (!string.IsNullOrEmpty(linkCssClass))
            {
                spanHtml = string.Format("<span class=\"{0}\">{1}</span>", linkCssClass, helper.HtmlHelper.Encode(node.Title));
            }
            else
            {
                spanHtml = string.Format("<span>{0}</span>", helper.HtmlHelper.Encode(node.Title));
            }
             */
            spanHtml = node.Title;

            if (currentNodeAsLink || !node.Equals(SiteMap.CurrentNode))
            {
                // Determine extra attributes
                var extraAttributes = new StringBuilder();
                var mvcNode = node as MvcSiteMapNode;
                if (mvcNode != null)
                {
                    if (!string.IsNullOrEmpty(mvcNode.TargetFrame))
                    {
                        extraAttributes.Append(" target=\"" + mvcNode.TargetFrame + "\"");
                    }
                    if (!string.IsNullOrEmpty(mvcNode.Title))
                    {
                        extraAttributes.Append(" title=\"" + mvcNode.Title + "\"");
                    }
                }
                foreach (var attribute in htmlAttributes)
                {
                    extraAttributes.Append(" " + attribute.Key + "=\"" + attribute.Value + "\"");
                }

                if (node.Title == "首页")
                {
                    return string.Format("<li><i class='icon-home'></i><a href=\"{0}\"{1}>{2}</a><i class='icon-angle-right'></i></li>", node.Url, extraAttributes, spanHtml);
                }
                else
                {
                    return string.Format("<li><a href=\"{0}\"{1}>{2}</a><i class='icon-angle-right'></i></li>", node.Url, extraAttributes, spanHtml);
                }
            }

            return spanHtml;
        }
    }
}
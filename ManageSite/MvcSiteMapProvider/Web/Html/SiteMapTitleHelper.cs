#region Using directives

using System.Collections.Generic;
using System.Web;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// MvcSiteMapHtmlHelper extension methods
    /// </summary>
    public static class SiteMapTitleHelper
    {
        /// <summary>
        /// Gets the title of SiteMap.CurrentNode
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <returns>
        /// The title of the CurrentNode or the RootNode (if CurrentNode is null)
        /// </returns>
        public static string SiteMapTitle(this MvcSiteMapHtmlHelper helper)
        {
            var node = SiteMap.CurrentNode;
            return (node != null)
                ? node.Title
                : SiteMap.RootNode.Title;
        }

        /// <summary>
        /// Gets the title of SiteMap.CurrentNode and appends all ancestor titles
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <returns>
        /// Title of SiteMap.CurrentNode and all ancestor titles
        /// </returns>
        public static string SiteMapTitleAncestry(this MvcSiteMapHtmlHelper helper)
        {
            return SiteMapTitleAncestry(helper, " - ");
        }

        /// <summary>
        /// Gets the title of SiteMap.CurrentNode and appends all ancestor titles
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="separator">Separator, i.e. " - "</param>
        /// <returns>Title of SiteMap.CurrentNode and all ancestor titles</returns>
        public static string SiteMapTitleAncestry(this MvcSiteMapHtmlHelper helper, string separator)
        {
            var node = SiteMap.CurrentNode;
            if (node != null)
            {
                var titles = new List<string>();
                titles.Add(node.Title);
                while (node.ParentNode != null)
                {
                    node = node.ParentNode;
                    titles.Add(node.Title);
                }
                return string.Join(separator, titles.ToArray());
            }
            return "";
        }
    }
}

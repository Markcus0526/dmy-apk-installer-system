#region Using directives

using System.Web;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// SiteMapNode extensions
    /// </summary>
    public static class SiteMapNodeExtensions
    {
        /// <summary>
        /// Gets the level of the current SiteMapNode
        /// </summary>
        /// <param name="current">The current SiteMapNode</param>
        /// <returns>The level of the current SiteMapNode</returns>
        public static int GetNodeLevel(this SiteMapNode current)
        {
            var level = 0;
            var node = current;

            while (node.ParentNode != null)
            {
                level++;
                node = node.ParentNode;
            }

            return level;
        }
    }
}

#region Using directives

using System.Web.Mvc;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// HtmlHelperExtensions class
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Creates a new MvcSiteMapProvider HtmlHelper.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>
        /// A <see cref="MvcSiteMapHtmlHelper"/> instance 
        /// </returns>
        public static MvcSiteMapHtmlHelper MvcSiteMap(this HtmlHelper helper)
        {
            return new MvcSiteMapHtmlHelper(helper);
        }
    }
}

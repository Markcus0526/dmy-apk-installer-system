#region Using directives

using System.Web.Mvc;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// MvcSiteMapHtmlHelper class
    /// </summary>
    public class MvcSiteMapHtmlHelper
    {
        /// <summary>
        /// Gets or sets the HTML helper.
        /// </summary>
        /// <value>The HTML helper.</value>
        public HtmlHelper HtmlHelper { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcSiteMapHtmlHelper"/> class.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        public MvcSiteMapHtmlHelper(HtmlHelper htmlHelper)
        {
            HtmlHelper = htmlHelper;
        }
    }
}

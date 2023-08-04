#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using MvcSiteMapProvider.External;

#endregion

namespace MvcSiteMapProvider.Web
{
    /// <summary>
    /// XmlSiteMapResult class.
    /// </summary>
    public class XmlSiteMapResult
        : ActionResult
    {
        /// <summary>
        /// Gets or sets the XML namespace.
        /// </summary>
        /// <value>The XML namespace.</value>
        protected XNamespace Ns { get; private set; }

        /// <summary>
        /// Gets or sets the root node.
        /// </summary>
        /// <value>The root node.</value>
        protected SiteMapNode RootNode { get; private set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        protected string Url { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSiteMapResult"/> class.
        /// </summary>
        public XmlSiteMapResult()
            : this(SiteMap.RootNode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSiteMapResult"/> class.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        public XmlSiteMapResult(SiteMapNode rootNode)
            : this(rootNode, UrlUtilities.ResolveServerUrl("~/", true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSiteMapResult"/> class.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        /// <param name="url">The base URL.</param>
        public XmlSiteMapResult(SiteMapNode rootNode, string url)
        {
            Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            RootNode = rootNode;
            Url = url;
        }

        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the <see cref="T:System.Web.Mvc.ActionResult"/> class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml";

            var urlSet = new XElement(Ns + "urlset");
            urlSet.Add(GenerateUrlElements(RootNode, Url).ToArray());

            var xmlSiteMap = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                urlSet);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                xmlSiteMap.WriteTo(writer);
            }

        }

        /// <summary>
        /// Generates the URL elements.
        /// </summary>
        /// <param name="siteMapNode">The site map node.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        protected virtual IEnumerable<XElement> GenerateUrlElements(SiteMapNode siteMapNode, string url)
        {
            // Return value
            List<XElement> returnValue = new List<XElement>();

            // Mvc node
            var mvcNode = siteMapNode as MvcSiteMapNode;
            
            // Render current node
            if (mvcNode == null || mvcNode.Clickable)
            {
                var urlElement = new XElement(Ns + "url",
                    new XElement(Ns + "loc", url + siteMapNode.Url));

                if (mvcNode != null)
                {
                    if (mvcNode.LastModifiedDate > DateTime.MinValue)
                    {
                        urlElement.Add(new XElement(Ns + "lastmod", mvcNode.LastModifiedDate));
                    }
                    if (mvcNode.ChangeFrequency != ChangeFrequency.Undefined)
                    {
                        urlElement.Add(new XElement(Ns + "changefreq", mvcNode.ChangeFrequency.ToString().ToLowerInvariant()));
                    }
                    if (mvcNode.UpdatePriority != UpdatePriority.Undefined)
                    {
                        urlElement.Add(new XElement(Ns + "priority", (double)mvcNode.UpdatePriority / 100));
                    }
                }
                   
                returnValue.Add(urlElement);
            }
            if (siteMapNode.HasChildNodes)
            {
                // Make sure all child nodes are accessible prior to rendering them...
                var shouldRender = false;
                foreach (SiteMapNode node in siteMapNode.ChildNodes)
                {
                    if (node.IsAccessibleToUser(HttpContext.Current))
                    {
                        shouldRender = true;
                        break;
                    }
                }

                // Render child nodes?
                if (shouldRender)
                {
                    foreach (SiteMapNode node in siteMapNode.ChildNodes)
                    {
                        if (node.IsAccessibleToUser(HttpContext.Current))
                        {
                            returnValue.AddRange(GenerateUrlElements(node, url));
                        }
                    }
                }
            }

            // Return
            return returnValue;
        }
    }
}

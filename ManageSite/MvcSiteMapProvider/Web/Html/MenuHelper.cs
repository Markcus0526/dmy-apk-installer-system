#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace MvcSiteMapProvider.Web.Html
{
    /// <summary>
    /// MvcSiteMapHtmlHelper extension methods
    /// </summary>
    public static class MenuHelper
    {
        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string selectedMenuItemCssClass)
        {
            return Menu(helper, true, false, selectedMenuItemCssClass, null);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string selectedMenuItemCssClass, object menuHtmlAttributes)
        {
            return Menu(helper, true, false, selectedMenuItemCssClass, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="showStartingNode">Show starting node?</param>
        /// <param name="startFromCurrentNode">Start from current node?</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, bool showStartingNode, bool startFromCurrentNode, string selectedMenuItemCssClass, object menuHtmlAttributes)
        {
            return Menu(helper, SiteMap.Provider.Name, showStartingNode, startFromCurrentNode, selectedMenuItemCssClass, 1, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, string selectedMenuItemCssClass)
        {
            return Menu(helper, providerName, true, false, selectedMenuItemCssClass, 1, null);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="maxLevels">The max levels.</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, string selectedMenuItemCssClass, int maxLevels, object menuHtmlAttributes)
        {
            return Menu(helper, providerName, true, false, selectedMenuItemCssClass, maxLevels, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="showStartingNode">Show starting node?</param>
        /// <param name="startFromCurrentNode">Start from current node?</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="maxLevels">The max levels.</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, bool showStartingNode, bool startFromCurrentNode, string selectedMenuItemCssClass, int maxLevels, object menuHtmlAttributes)
        {
            return Menu(helper, providerName, showStartingNode, 0, startFromCurrentNode, selectedMenuItemCssClass, maxLevels, menuHtmlAttributes, -1);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="startingNodeLevel">Starting node level</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, int startingNodeLevel, string selectedMenuItemCssClass)
        {
            return Menu(helper, SiteMap.Provider.Name, false, startingNodeLevel, true, selectedMenuItemCssClass, 1, null, -1);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="startingNodeLevel">Starting node level</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, int startingNodeLevel, string selectedMenuItemCssClass, object menuHtmlAttributes)
        {
            return Menu(helper, SiteMap.Provider.Name, false, startingNodeLevel, true, selectedMenuItemCssClass, 1, menuHtmlAttributes, -1);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="startingNodeLevel">Starting node level</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, int startingNodeLevel, string selectedMenuItemCssClass)
        {
            return Menu(helper, providerName, false, startingNodeLevel, true, selectedMenuItemCssClass, 1, null, -1);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="startingNodeLevel">Starting node level</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, int startingNodeLevel, string selectedMenuItemCssClass, object menuHtmlAttributes)
        {
            return Menu(helper, providerName, false, startingNodeLevel, true, selectedMenuItemCssClass, 1, menuHtmlAttributes, -1);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="showStartingNode">if set to <c>true</c> show starting node.</param>
        /// <param name="startingNodeLevel">The starting node level.</param>
        /// <param name="startFromCurrentNode">if set to <c>true</c> start from current node.</param>
        /// <param name="selectedMenuItemCssClass">The selected menu item CSS class.</param>
        /// <param name="maxLevels">The max levels.</param>
        /// <param name="menuHtmlAttributes">The menu HTML attributes.</param>
        /// <param name="pinnedNodeLevel">The pinned node level.</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, bool showStartingNode, int startingNodeLevel, bool startFromCurrentNode, string selectedMenuItemCssClass, int maxLevels, object menuHtmlAttributes, int pinnedNodeLevel)
        {
            return Menu(helper, providerName, showStartingNode, startingNodeLevel, startFromCurrentNode, selectedMenuItemCssClass, selectedMenuItemCssClass, maxLevels, menuHtmlAttributes, pinnedNodeLevel);
        }

        /// <summary>
        /// Build a menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="showStartingNode">Show starting node?</param>
        /// <param name="startingNodeLevel">Starting node level</param>
        /// <param name="startFromCurrentNode">Start from current node?</param>
        /// <param name="selectedMenuItemCssClass">Selected menu item CSS class</param>
        /// <param name="openedMenuItemCssClass">The opened menu item CSS class.</param>
        /// <param name="maxLevels">The max levels.</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <param name="pinnedNodeLevel">The pinned node level.</param>
        /// <returns>Html markup</returns>
        public static string Menu(this MvcSiteMapHtmlHelper helper, string providerName, bool showStartingNode, int startingNodeLevel, bool startFromCurrentNode, string selectedMenuItemCssClass, string openedMenuItemCssClass, int maxLevels, object menuHtmlAttributes, int pinnedNodeLevel)
        {
            SiteMapProvider selectedSiteMapProvider = SiteMap.Providers[providerName];
            // Check for provider
            if (selectedSiteMapProvider == null)
            {
                throw new UnknownSiteMapProviderException(string.Format("Unknown SiteMap provider: {0}", providerName));
            }

            // Helpers
            UrlHelper urlHelper = new UrlHelper(helper.HtmlHelper.ViewContext.RequestContext);

            // Html attributes
            IDictionary<string, object> attributes = new RouteValueDictionary(menuHtmlAttributes);

            // Determine root node
            SiteMapNode relativeRootNode = null;
            if (startFromCurrentNode)
            {
                // get the deepest sitemap node relative to the current URL
                relativeRootNode = GetCurrentNode(selectedSiteMapProvider);

                // get the parent node at the desired pinned level
                if ((relativeRootNode != null) && (pinnedNodeLevel > 0))
                {
                    while (GetNodeLevel(relativeRootNode) > pinnedNodeLevel)
                    {
                        relativeRootNode = relativeRootNode.ParentNode;
                    }
                }
            }
            else
            {
                relativeRootNode = selectedSiteMapProvider.RootNode;
            }

            // Node not defined?
            if (relativeRootNode == null)
            {
                // String builder
                StringBuilder sb1 = new StringBuilder();
                CreateUnorderedListTag(helper, openedMenuItemCssClass, selectedMenuItemCssClass, selectedSiteMapProvider, urlHelper, attributes, new SiteMapNodeCollection(), sb1, maxLevels);
                return sb1.ToString();
            }
            else
            {
                // Fetch nodes to render
                SiteMapNodeCollection nodesToRender = new SiteMapNodeCollection();
                if (showStartingNode)
                {
                    nodesToRender.Add(relativeRootNode);
                }
                if (relativeRootNode.HasChildNodes)
                {
                    // Use startingNodeLevel?
                    if (startingNodeLevel == 0)
                    {
                        // no...
                        nodesToRender.AddRange(relativeRootNode.ChildNodes);
                    }
                    else
                    {
                        // yes: determine absolute level of relativeRootNode
                        int startingNodeAbsoluteLevel = GetNodeLevel(relativeRootNode);
                        if (startingNodeAbsoluteLevel + 1 == startingNodeLevel)
                        {
                            nodesToRender.AddRange(relativeRootNode.ChildNodes);
                        }
                    }
                }


                // String builder
                StringBuilder sb = new StringBuilder();
                CreateUnorderedListTag(helper, openedMenuItemCssClass, selectedMenuItemCssClass, selectedSiteMapProvider,
                                       urlHelper, attributes, nodesToRender, sb, maxLevels);
                return sb.ToString();
            }
        }

        /// <summary>
        /// This determines the deepest node matching the current HTTP context, so if the current URL describes a location
        /// deeper than the site map designates, it will determine the closest parent to the current URL and return that 
        /// as the current node. This allows menu relevence when navigating deeper than the sitemap structure designates, such
        /// as when navigating to MVC actions, which are not shown in the menus
        /// </summary>
        /// <param name="selectedSiteMapProvider">the current MVC Site Map Provider</param>
        /// <returns></returns>
        public static SiteMapNode GetCurrentNode(SiteMapProvider selectedSiteMapProvider)
        {
            // get the node matching the current URL location
            var currentNode = selectedSiteMapProvider.CurrentNode;

            // if there is no node matching the current URL path, 
            // remove parts until we get a hit
            if (currentNode == null)
            {
                var url = HttpContext.Current.Request.Url.LocalPath;

                while (url.Length > 0)
                {
                    // see if we can find a matching node
                    currentNode = selectedSiteMapProvider.FindSiteMapNode(url);

                    // if we get a hit, stop
                    if (currentNode != null) break;

                    // if not, remove the last path item  
                    var lastSlashlocation = url.LastIndexOf("/");
                    if (lastSlashlocation < 0) break; // protects us from malformed URLs
                    url = url.Remove(lastSlashlocation);
                }
            }

            return currentNode;
        }

        /// <summary>
        /// Build a sub menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="siteMapLevel">Pinned level</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Submenu(this MvcSiteMapHtmlHelper helper, int siteMapLevel, object menuHtmlAttributes)
        {
            return Submenu(helper, SiteMap.Provider.Name, siteMapLevel, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a sub menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="selectedMenuItemCssClass">The CSS class to apply to the selected menu item</param>
        /// <param name="siteMapLevel">Pinned level</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Submenu(this MvcSiteMapHtmlHelper helper, string selectedMenuItemCssClass, int siteMapLevel, object menuHtmlAttributes)
        {
            return Submenu(helper, SiteMap.Provider.Name, selectedMenuItemCssClass, siteMapLevel, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a sub menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="selectedMenuItemCssClass">The CSS class to apply to the selected menu item</param>
        /// <param name="siteMapLevel">Pinned level</param>
        /// <param name="maxLevel">The max level.</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Submenu(this MvcSiteMapHtmlHelper helper, string selectedMenuItemCssClass, int siteMapLevel, int maxLevel, object menuHtmlAttributes)
        {
            return Submenu(helper, SiteMap.Provider.Name, selectedMenuItemCssClass, siteMapLevel, maxLevel, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a sub menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="selectedMenuItemCssClass">The CSS class to apply to the selected menu item</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="siteMapLevel">Pinned level</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Submenu(this MvcSiteMapHtmlHelper helper, string providerName, string selectedMenuItemCssClass, int siteMapLevel, object menuHtmlAttributes)
        {
            return Submenu(helper, providerName, selectedMenuItemCssClass, siteMapLevel, -1, menuHtmlAttributes);
        }

        /// <summary>
        /// Build a sub menu, based on the MvcSiteMap
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="providerName">Provider name (can be used to specify SiteMap datasource)</param>
        /// <param name="selectedMenuItemCssClass">The CSS class to apply to the selected menu item</param>
        /// <param name="siteMapLevel">Pinned level</param>
        /// <param name="maxLevel">The max level.</param>
        /// <param name="menuHtmlAttributes">Html attributes for the outer unordered list</param>
        /// <returns>Html markup</returns>
        public static string Submenu(this MvcSiteMapHtmlHelper helper, string providerName, string selectedMenuItemCssClass, int siteMapLevel, int maxLevel, object menuHtmlAttributes)
        {
            return Menu(helper, providerName, false, 0, true, selectedMenuItemCssClass, maxLevel, menuHtmlAttributes, siteMapLevel);
        }

        /// <summary>
        /// Creates a UL tag
        /// </summary>
        /// <param name="helper">MvcSiteMapHtmlHelper instance</param>
        /// <param name="openedMenuItemCssClass">The opened menu item CSS class.</param>
        /// <param name="selectedMenuItemCssClass">Selected MenuItem Css Class</param>
        /// <param name="selectedSiteMapProvider">Selected SiteMapProvider</param>
        /// <param name="urlHelper">UrlHelper instance</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="nodesToRender">List of nodes to render</param>
        /// <param name="sb">StringBuilder to work with</param>
        /// <param name="maxLevels">Maximal number of levels</param>
        private static void CreateUnorderedListTag(MvcSiteMapHtmlHelper helper, string openedMenuItemCssClass, string selectedMenuItemCssClass, SiteMapProvider selectedSiteMapProvider, UrlHelper urlHelper, IDictionary<string, object> attributes, SiteMapNodeCollection nodesToRender, StringBuilder sb, int maxLevels)
        {
            // Create unordered list tag
            sb.Append("<ul");
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    sb.Append(string.Format(" {0}=\"{1}\"", attribute.Key, attribute.Value));
                }
            }
            sb.AppendLine(">");

            // get the deepest node matching the current context
            var currentNode = GetCurrentNode(selectedSiteMapProvider);

            // Render each node
            foreach (SiteMapNode node in nodesToRender)
            {
                MvcSiteMapNode mvcNode = node as MvcSiteMapNode;

                // Render node?
                if (node.IsAccessibleToUser(HttpContext.Current))
                {
                    // if there is a CurrentNode and it equals this node, render it with the selected item CSS
                    if (currentNode != null && (currentNode.Equals(node)))
                    {
                        sb.Append("<li");
                        if (!string.IsNullOrEmpty(selectedMenuItemCssClass))
                        {
                            sb.Append(string.Format(" class=\"{0}\"", selectedMenuItemCssClass));
                        }
                        sb.Append(">");
                    }
                    // if there is a CurrentNode, it is a descendant of this node, and it is not the rootNode, render it with the opened item CSS
                    else if (currentNode != null && currentNode.IsDescendantOf(node) && !node.Equals(selectedSiteMapProvider.RootNode))
                    {
                        sb.Append("<li");
                        if (!string.IsNullOrEmpty(openedMenuItemCssClass))
                        {
                            sb.Append(string.Format(" class=\"{0}\"", openedMenuItemCssClass));
                        }
                        sb.Append(">");
                    }
                    else
                    {
                        sb.Append("<li>");
                    }

                    string url = string.Empty;
                    if (!string.IsNullOrEmpty(node.Url))
                    {
                        if (node.Url.StartsWith("http%3A", StringComparison.InvariantCultureIgnoreCase))
                        {
                            url = node.Url.Replace("%3A", ":");
                        }
                        else
                        {
                            url = urlHelper.Content(node.Url);
                        }
                    }


                    string imageMarkup = string.Empty;
                    if (mvcNode != null && !string.IsNullOrEmpty(mvcNode.ImageUrl))
                    {
                        string imageUrl = urlHelper.Content(mvcNode.ImageUrl);
                        imageMarkup = string.Format("<img src=\"{0}\" class=\"icon\" alt=\"{1}\" />", imageUrl, helper.HtmlHelper.Encode(node.Title));
                    }

                    // Clickable?
                    if (mvcNode == null || (mvcNode != null && mvcNode.Clickable)) {
                    // Determine extra attributes
                    StringBuilder extraAttributes = new StringBuilder();
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

                    sb.AppendFormat("<a href=\"{0}\"{1}><span>{2}{3}</span></a>", url, extraAttributes, imageMarkup, helper.HtmlHelper.Encode(node.Title));
                    } else
                    {
                        sb.AppendFormat("<span>{0}{1}</span>", imageMarkup, helper.HtmlHelper.Encode(node.Title));
                    }

                    if (maxLevels != 1 && node.HasChildNodes)
                        CreateUnorderedListTag(helper, openedMenuItemCssClass, selectedMenuItemCssClass, selectedSiteMapProvider, urlHelper, null, node.ChildNodes, sb, maxLevels--);

                    sb.AppendLine("</li>");
                }
            }

            // Close unordered list tag
            sb.AppendLine("</ul>");
        }

        /// <summary>
        /// Get SiteMapNode level
        /// </summary>
        /// <param name="node">SiteMapNode to determine level for</param>
        /// <returns>SiteMapNode level</returns>
        public static int GetNodeLevel(SiteMapNode node)
        {
            int level = 0;
            while (node.ParentNode != null)
            {
                level++;
                node = node.ParentNode;
            }
            return level;
        }
    }
}

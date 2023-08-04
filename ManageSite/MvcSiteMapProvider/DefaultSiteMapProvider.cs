#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using MvcSiteMapProvider.Extensibility;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// DefaultSiteMapProvider class
    /// </summary>
    public class DefaultSiteMapProvider : StaticSiteMapProvider
    {
        #region Private

        private const string RootName = "mvcSiteMap";
        private const string NodeName = "mvcSiteMapNode";
        private readonly XNamespace ns = "http://mvcsitemap.codeplex.com/schemas/MvcSiteMap-File-2.0";
        private readonly object synclock = new object();
        private int cacheDuration = 5;
        private string cacheKey = "A33EF2B1-F0A4-4507-B011-94669840F79C";
        private bool isBuildingSiteMap;
        private SiteMapNode root;

        private bool scanAssembliesForSiteMapNodes;
        private string siteMapFile = string.Empty;
        private List<string> skipAssemblyScanOn = new List<string>();
        private List<string> attributesToIgnore = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the node key generator.
        /// </summary>
        /// <value>The node key generator.</value>
        public INodeKeyGenerator NodeKeyGenerator { get; set; }

        /// <summary>
        /// Gets or sets the controller type resolver.
        /// </summary>
        /// <value>The controller type resolver.</value>
        public IControllerTypeResolver ControllerTypeResolver { get; set; }

        /// <summary>
        /// Gets or sets the action method parameter resolver.
        /// </summary>
        /// <value>The action method parameter resolver.</value>
        public IActionMethodParameterResolver ActionMethodParameterResolver { get; set; }

        /// <summary>
        /// Gets or sets the acl module.
        /// </summary>
        /// <value>The acl module.</value>
        public IAclModule AclModule { get; set; }

        /// <summary>
        /// Gets the RootNode for the current SiteMapProvider.
        /// </summary>
        public override SiteMapNode RootNode
        {
            get
            {
                return GetRootNodeCore();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the current root, otherwise calls the BuildSiteMap method.
        /// </summary>
        /// <returns></returns>
        protected override SiteMapNode GetRootNodeCore()
        {
            return BuildSiteMap();
        }

        /// <summary>
        /// Retrieves a Boolean value indicating whether the specified <see cref="T:System.Web.SiteMapNode"/> object can be viewed by the user in the specified context.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> that contains user information.</param>
        /// <param name="node">The <see cref="T:System.Web.SiteMapNode"/> that is requested by the user.</param>
        /// <returns>
        /// true if security trimming is enabled and <paramref name="node"/> can be viewed by the user or security trimming is not enabled; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="context"/> is null.
        /// - or -
        /// <paramref name="node"/> is null.
        /// </exception>
        public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
        {
            return AclModule.IsAccessibleToUser(ControllerTypeResolver, this, context, node);
        }

        /// <summary>
        /// Initializes our custom provider, gets the attributes that are set in the config
        /// that enable us to customise the behaiviour of this provider.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        public override void Initialize(string name, NameValueCollection attributes)
        {
            base.Initialize(name, attributes);

            // Get the siteMapFile from the passed in attributes.
            siteMapFile = attributes["siteMapFile"];

            // If a cacheDuration was passed in set it, otherwise
            // it will default to 5 minutes.
            if (!string.IsNullOrEmpty(attributes["cacheDuration"]))
            {
                cacheDuration = int.Parse(attributes["cacheDuration"]);
            }

            // If a cache key was set in config, set it. 
            // Otherwise it will use the default which is a GUID.
            if (!string.IsNullOrEmpty(attributes["cacheKey"]))
            {
                cacheKey = attributes["cacheKey"];
            }

            // Enable Localization
            if (!string.IsNullOrEmpty(attributes["enableLocalization"]))
            {
                EnableLocalization = Boolean.Parse(attributes["enableLocalization"]);
            }

            // Resource key
            if (!string.IsNullOrEmpty(attributes["resourceKey"]))
            {
                ResourceKey = attributes["resourceKey"];
            }

            // Scan assemblies for IMvcSiteMapNodeAttribute?
            if (!string.IsNullOrEmpty(attributes["scanAssembliesForSiteMapNodes"]))
            {
                scanAssembliesForSiteMapNodes = Boolean.Parse(attributes["scanAssembliesForSiteMapNodes"]);

                // Which assemblies should be skipped?
                if (!string.IsNullOrEmpty(attributes["skipAssemblyScanOn"]))
                {
                    var tempSkipAssemblyScanOn = attributes["skipAssemblyScanOn"];
                    if (!string.IsNullOrEmpty(tempSkipAssemblyScanOn))
                    {
                        skipAssemblyScanOn = tempSkipAssemblyScanOn.Split(';', ',').ToList();
                    }
                }
            }

            // Which attributes in the sitemap XML should be ignored?
            if (!string.IsNullOrEmpty(attributes["attributesToIgnore"]))
            {
                var tempAttributesToIgnore = attributes["attributesToIgnore"];
                if (!string.IsNullOrEmpty(tempAttributesToIgnore))
                {
                    attributesToIgnore = tempAttributesToIgnore.Split(';', ',').ToList();
                }
            }

            // Is a node key generator given?
            if (!string.IsNullOrEmpty(attributes["nodeKeyGenerator"]))
            {
                NodeKeyGenerator = Activator.CreateInstance(
                    Type.GetType(attributes["nodeKeyGenerator"])) as INodeKeyGenerator;
            }
            if (NodeKeyGenerator == null)
            {
                NodeKeyGenerator = new DefaultNodeKeyGenerator();
            }

            // Is a controller type resolver given?
            if (!string.IsNullOrEmpty(attributes["controllerTypeResolver"]))
            {
                ControllerTypeResolver = Activator.CreateInstance(
                    Type.GetType(attributes["controllerTypeResolver"])) as IControllerTypeResolver;
            }
            if (ControllerTypeResolver == null)
            {
                ControllerTypeResolver = new DefaultControllerTypeResolver();
            }

            // Is an action method parameter resolver given?
            if (!string.IsNullOrEmpty(attributes["actionMethodParameterResolver"]))
            {
                ActionMethodParameterResolver = Activator.CreateInstance(
                    Type.GetType(attributes["actionMethodParameterResolver"])) as IActionMethodParameterResolver;
            }
            if (ActionMethodParameterResolver == null)
            {
                ActionMethodParameterResolver = new DefaultActionMethodParameterResolver();
            }

            // Is an acl module given?
            if (!string.IsNullOrEmpty(attributes["aclModule"]))
            {
                AclModule = Activator.CreateInstance(
                    Type.GetType(attributes["aclModule"])) as IAclModule;
            }
            if (AclModule == null)
            {
                AclModule = new DefaultAclModule();
            }
        }

        /// <summary>
        /// Adds a <see cref="T:System.Web.SiteMapNode"/> object to the node collection that is maintained by the site map provider.
        /// </summary>
        /// <param name="node">The <see cref="T:System.Web.SiteMapNode"/> to add to the node collection maintained by the provider.</param>
        protected override void AddNode(SiteMapNode node)
        {
            try
            {
                base.AddNode(node);
            }
            catch (InvalidOperationException)
            {
                if (!isBuildingSiteMap)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="T:System.Web.SiteMapNode"/> to the collections that are maintained by the site map provider and establishes a parent/child relationship between the <see cref="T:System.Web.SiteMapNode"/> objects.
        /// </summary>
        /// <param name="node">The <see cref="T:System.Web.SiteMapNode"/> to add to the site map provider.</param>
        /// <param name="parentNode">The <see cref="T:System.Web.SiteMapNode"/> under which to add <paramref name="node"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="node"/> is null.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="P:System.Web.SiteMapNode.Url"/> or <see cref="P:System.Web.SiteMapNode.Key"/> is already registered with the <see cref="T:System.Web.StaticSiteMapProvider"/>. A site map node must be made up of pages with unique URLs or keys.
        /// </exception>
        protected override void AddNode(SiteMapNode node, SiteMapNode parentNode)
        {
            try
            {
                base.AddNode(node, parentNode);
            }
            catch (InvalidOperationException)
            {
                if (!isBuildingSiteMap)
                {
                    throw;
                }
            }
        }

        #endregion

        #region Sitemap Building/XML Parsing

        private static AspNetHostingPermissionLevel appTrustLevel = AspNetHostingPermissionLevel.None;

        /// <summary>
        /// Builds the sitemap, firstly reads in the XML file, and grabs the outer root element and 
        /// maps this to become our main out root SiteMap node.
        /// </summary>
        /// <returns>The root SiteMapNode.</returns>
        public override SiteMapNode BuildSiteMap()
        {
            if (root != null && (HttpContext.Current.Cache[cacheKey] != null || isBuildingSiteMap))
            {
                // If sitemap already loaded and our cache key is still set,
                // checking a cache item enables us to invalidate the sitemap
                // after a given time period.
                return root;
            }

            lock (synclock)
            {
                XDocument siteMapXml = null;

                // Clear the current sitemap.
                Clear();

                try
                {
                    // Load the XML document.
                    siteMapXml = XDocument.Load(HttpContext.Current.Server.MapPath(siteMapFile));

                    // Enable Localization?
                    string enableLocalization = siteMapXml.Element(ns + RootName).GetAttributeValue("enableLocalization");
                    if (!string.IsNullOrEmpty(enableLocalization))
                    {
                        EnableLocalization = Boolean.Parse(enableLocalization);
                    }

                    // Get the root mvcSiteMapNode element, and map this to an MvcSiteMapNode,
                    // this becomes our root node.
                    var rootElement = siteMapXml.Element(ns + RootName).Element(ns + NodeName);
                    root = GetSiteMapNodeFromXmlElement(rootElement, null);

                    // Add our main root node.
                    AddNode(root);
                    isBuildingSiteMap = true;

                    // Process our XML file, passing in the main root sitemap node and xml element.
                    ProcessXmlNodes(root, rootElement);

                    // Look for assembly-defined nodes
                    // Process nodes in the assemblies of the current AppDomain?
                    if (scanAssembliesForSiteMapNodes)
                    {
                        if (GetCurrentTrustLevel() <= AspNetHostingPermissionLevel.Medium)
                        {
                            throw new MvcSiteMapException("Scanning assemblies for sitemap nodes is not supported in the current trust level. Consider a higher trust level or disable scanning assemblies for SiteMap nodes.");
                        }

                        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                            .Where(a => !a.FullName.StartsWith("mscorlib")
                                        && !a.FullName.StartsWith("System")
                                        && !a.FullName.StartsWith("Microsoft")
                                        && !a.FullName.StartsWith("WebDev")
                                        && !a.FullName.StartsWith("SMDiagnostics")
                                        && !a.FullName.StartsWith("Anonymously")
                                        && !a.FullName.StartsWith("App_")
                                        && !skipAssemblyScanOn.Contains(a.GetName().Name));

                        foreach (Assembly assembly in assemblies)
                        {
                            ProcessNodesInAssembly(assembly);
                        }
                    }


                    // Create a cache item, this is used for the sole purpose of being able to invalidate our sitemap
                    // after a given time period, it also adds a dependancy on the sitemap file,
                    // so that once changed it will refresh your sitemap, unfortunately at this stage
                    // there is no dependancy for dynamic data, this could be implemented by clearing the cache item,
                    // by setting a custom cacheKey, then use this in your administration console for example to
                    // clear the cache item when the structure requires refreshing.
                    HttpContext.Current.Cache.Insert(cacheKey,
                                                     "",
                                                     new CacheDependency(HttpContext.Current.Server.MapPath(siteMapFile)),
                                                     DateTime.Now.AddMinutes(cacheDuration),
                                                     Cache.NoSlidingExpiration
                        );

                    isBuildingSiteMap = false;
                }
                catch (Exception ex)
                {
                    // If there was ANY error loading or parsing the sitemap XML file, throw an exception.
                    throw new Exception("Error parsing SiteMap XML.", ex);
                }
                finally
                {
                    siteMapXml = null;
                    isBuildingSiteMap = false;
                }
            }

            // Finally return our root SiteMapNode.
            return root;
        }

        /// <summary>
        /// Get current trust level
        /// </summary>
        /// <returns>Current trust level</returns>
        private static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            if (appTrustLevel != AspNetHostingPermissionLevel.None)
                return appTrustLevel;

            foreach (AspNetHostingPermissionLevel trustLevel in
                new[] {
                          AspNetHostingPermissionLevel.Unrestricted,
                          AspNetHostingPermissionLevel.High,
                          AspNetHostingPermissionLevel.Medium,
                          AspNetHostingPermissionLevel.Low,
                          AspNetHostingPermissionLevel.Minimal 
                      })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (System.Security.SecurityException)
                {
                    continue;
                }

                appTrustLevel = trustLevel;
                return trustLevel;
            }

            appTrustLevel = AspNetHostingPermissionLevel.None;
            return AspNetHostingPermissionLevel.None;
        }

        /// <summary>
        /// Recursively processes our XML document, parsing our siteMapNodes and dynamicNode(s).
        /// </summary>
        /// <param name="rootNode">The main root sitemap node.</param>
        /// <param name="rootElement">The main root XML element.</param>
        protected void ProcessXmlNodes(SiteMapNode rootNode, XElement rootElement)
        {
            SiteMapNode childNode = rootNode;

            // Loop through each element below the current root element.
            foreach (XElement node in rootElement.Elements())
            {
                if (node.Name == ns + NodeName)
                {
                    // If this is a normal mvcSiteMapNode then map the xml element
                    // to an MvcSiteMapNode, and add the node to the current root.
                    childNode = GetSiteMapNodeFromXmlElement(node, rootNode);
                    AddNode(childNode, rootNode);
                    AddDynamicNodesFor(childNode, rootNode);
                }
                else
                {
                    // If the current node is not one of the known node types throw and exception
                    throw new Exception("An invalid element was found in the sitemap.");
                }

                // Continue recursively processing the XML file.
                ProcessXmlNodes(childNode, node);
            }
        }

        /// <summary>
        /// Process nodes in assembly
        /// </summary>
        /// <param name="assembly">Assembly</param>
        protected void ProcessNodesInAssembly(Assembly assembly)
        {
            var types = new List<KeyValuePair<Type, IMvcSiteMapNodeAttribute>>(); // type and its attribute

            // load all types and their attributes
            foreach (Type type in assembly.GetTypes())
            {
                try
                {
                    types.Add(new KeyValuePair<Type, IMvcSiteMapNodeAttribute>(type, type.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), true).FirstOrDefault() as IMvcSiteMapNodeAttribute));
                }
                catch { }
            }

            // sort them
            types.Sort((p1, p2) => (p1.Value == null ? 0 : p1.Value.Order) - (p2.Value == null ? 0 : p2.Value.Order));

            // use them
            foreach (var type in types)
            {
                SiteMapNode controllerNode = null;
                if (type.Value != null) // attribute on type-level specified
                {
                    controllerNode = GetSiteMapNodeFromMvcSiteMapNodeAttribute(type.Value, type.Key, null);
                    var parentNode = (string.IsNullOrEmpty(type.Value.ParentKey) ? null : FindSiteMapNodeFromKey(type.Value.ParentKey)) ?? root;
                    if (controllerNode != null && parentNode != null)
                    {
                        AddNode(controllerNode, parentNode);
                        AddDynamicNodesFor(controllerNode, parentNode);
                    }
                }

                foreach (MethodInfo method in type.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    foreach (MvcSiteMapNodeAttribute attribute in (MvcSiteMapNodeAttribute[])method.GetCustomAttributes(typeof(MvcSiteMapNodeAttribute), true))
                    {
                        var node = GetSiteMapNodeFromMvcSiteMapNodeAttribute(attribute, type.Key, method);
                        var parentNode = (controllerNode != null && string.IsNullOrEmpty(attribute.ParentKey)) ?
                                                                                                                   controllerNode :
                                                                                                                                      (string.IsNullOrEmpty(attribute.ParentKey) ? root : FindSiteMapNodeFromKey(attribute.ParentKey));
                        parentNode = parentNode ?? root;
                        if (node != null && parentNode != null)
                        {
                            AddNode(node, parentNode);
                            AddDynamicNodesFor(node, parentNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the dynamic nodes for node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parentNode">The parent node.</param>
        private void AddDynamicNodesFor(SiteMapNode node, SiteMapNode parentNode)
        {
            // Dynamic nodes available?
            var mvcNode = node as MvcSiteMapNode;
            if (mvcNode == null || mvcNode.DynamicNodeProvider == null)
            {
                return;
            }

            // Build dynamic nodes
            foreach (var dynamicNode in mvcNode.DynamicNodeProvider.GetDynamicNodeCollection().ToList())
            {
                string key = dynamicNode.Key;
                if (string.IsNullOrEmpty(key))
                {
                    key = NodeKeyGenerator.GenerateKey(Guid.NewGuid().ToString(), mvcNode.Url, mvcNode.Title, mvcNode.Area, mvcNode.Controller, mvcNode.Action, mvcNode.Clickable);
                }

                var clone = mvcNode.Clone(key) as MvcSiteMapNode;
                foreach (var kvp in dynamicNode.RouteValues)
                {
                    clone.RouteValues[kvp.Key] = kvp.Value;
                }
                foreach (var kvp in dynamicNode.Attributes)
                {
                    clone[kvp.Key] = kvp.Value;
                } 
                clone.DynamicNodeProvider = null;
                clone.IsDynamic = true;
                clone.Title = dynamicNode.Title;
                clone.Description = dynamicNode.Description;
                clone.TargetFrame = dynamicNode.TargetFrame;
                clone.ImageUrl = dynamicNode.ImageUrl;

                // If the dynamic node has a parent key set, use that as the parent. Otherwise use the parentNode.
                if (!string.IsNullOrEmpty(dynamicNode.ParentKey))
                {
                    var parent = FindSiteMapNodeFromKey(dynamicNode.ParentKey);
                    if (parent != null)
                    {
                        AddNode(clone, parent);
                    }
                }
                else
                {
                    AddNode(clone, parentNode);
                }
            }

            // Remove the original node
            RemoveNode(node);

            // Insert cache dependency
            CacheDescription cacheDescription = mvcNode.DynamicNodeProvider.GetCacheDescription();
            if (cacheDescription != null)
            {
                HttpContext.Current.Cache.Insert(
                    cacheDescription.Key, 
                    "",
                    cacheDescription.Dependencies,
                    cacheDescription.AbsoluteExpiration,
                    cacheDescription.SlidingExpiration,
                    cacheDescription.Priority,
                    (key, item, reason) => { if (!isBuildingSiteMap) root = null; });
            }
        }

        /// <summary>
        /// Clears the current sitemap.
        /// </summary>
        protected override void Clear()
        {
            root = null;
            base.Clear();
        }

        /// <summary>
        /// Retrieves a <see cref="T:System.Web.SiteMapNode"/> object that represents the currently requested page using the specified <see cref="T:System.Web.HttpContext"/> object.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> used to match node information with the URL of the requested page.</param>
        /// <returns>
        /// A <see cref="T:System.Web.SiteMapNode"/> that represents the currently requested page; otherwise, null, if no corresponding <see cref="T:System.Web.SiteMapNode"/> can be found in the <see cref="T:System.Web.SiteMapNode"/> or if the page context is null.
        /// </returns>
        public override SiteMapNode FindSiteMapNode(HttpContext context)
        {
            // Node
            SiteMapNode node = null;

            // Fetch route data
            var httpContext = new HttpContextWrapper(context);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            if (routeData != null)
            {
                VirtualPathData vpd = routeData.Route.GetVirtualPath(
                    new RequestContext(httpContext, routeData), routeData.Values);
                node = base.FindSiteMapNode(vpd.VirtualPath) as MvcSiteMapNode;

                if (routeData.DataTokens["area"] != null)
                {
                    routeData.Values.Add("area", routeData.DataTokens["area"]);
                }
                else
                {
                    routeData.Values.Add("area", "");
                }

                if (node == null)
                {
                    node = FindControllerActionNode(RootNode, routeData.Values);
                }
            }

            // Try base class
            if (node == null)
            {
                node = base.FindSiteMapNode(context);
            }

            // Check accessibility
            if (node != null)
            {
                if (node.IsAccessibleToUser(context))
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the controller action node.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A controller action node represented as a <see cref="SiteMapNode"/> instance 
        /// </returns>
        private SiteMapNode FindControllerActionNode(SiteMapNode rootNode, IDictionary<string, object> values)
        {
            if (rootNode != null)
            {
                // Search current level
                foreach (SiteMapNode node in GetChildNodes(rootNode))
                {
                    // Check if it is an MvcSiteMapNode
                    var mvcNode = node as MvcSiteMapNode;
                    if (mvcNode != null)
                    {
                        // Valid node?
                        if (NodeMatchesRoute(mvcNode, values))
                        {
                            return mvcNode;
                        }
                    }
                }

                // Search one deeper level
                foreach (SiteMapNode node in GetChildNodes(rootNode))
                {
                    var siteMapNode = FindControllerActionNode(node, values);
                    if (siteMapNode != null)
                    {
                        return siteMapNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Nodes the matches route.
        /// </summary>
        /// <param name="mvcNode">The MVC node.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A matches route represented as a <see cref="bool"/> instance 
        /// </returns>
        private bool NodeMatchesRoute(MvcSiteMapNode mvcNode, IDictionary<string, object> values)
        {
            var nodeValid = true;

            if (mvcNode != null)
            {
                // Find action method parameters?
                IEnumerable<string> actionParameters = new List<string>();
                if (mvcNode.DynamicNodeProvider == null && mvcNode.IsDynamic == false)
                {
                    actionParameters = ActionMethodParameterResolver.ResolveActionMethodParameters(
                        ControllerTypeResolver, mvcNode.Area, mvcNode.Controller, mvcNode.Action);
                }

                // Verify route values
                if (values.Count > 0)
                {
                    foreach (var pair in values)
                    {
                        if (mvcNode[pair.Key] != null)
                        {
                            if (mvcNode[pair.Key].ToLowerInvariant() == pair.Value.ToString().ToLowerInvariant())
                            {
                                continue;
                            }
                            else
                            {
                                // Is the current pair.Key a parameter on the action method?
                                if (!actionParameters.Contains(pair.Key, StringComparer.InvariantCultureIgnoreCase)) {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (pair.Value == null || string.IsNullOrEmpty(pair.Value.ToString()) || pair.Value == UrlParameter.Optional)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                nodeValid = false;
            }

            return nodeValid;
        }

        #endregion

        #region Mappers

        /// <summary>
        /// Maps an XMLElement from the XML file to an MvcSiteMapNode.
        /// </summary>
        /// <param name="node">The element to map.</param>
        /// <param name="parentNode">The parent SiteMapNode</param>
        /// <returns>An MvcSiteMapNode which represents the XMLElement.</returns>
        protected MvcSiteMapNode GetSiteMapNodeFromXmlElement(XElement node, SiteMapNode parentNode)
        {
            // Generate key for node
            string key = NodeKeyGenerator.GenerateKey(
                node.GetAttributeValue("key"),
                node.GetAttributeValue("url"),
                node.GetAttributeValue("title"),
                node.GetAttributeValue("area"),
                node.GetAttributeValue("controller"),
                node.GetAttributeValue("action"),
                !(node.GetAttributeValue("clickable") == "false"));

            // Handle title and description globalization
            var explicitResourceKeys = new NameValueCollection();
            var title = node.GetAttributeValue("title");
            var description = node.GetAttributeValue("description") ?? title;
            HandleResourceAttribute("title", ref title, ref explicitResourceKeys);
            HandleResourceAttribute("description", ref description, ref explicitResourceKeys);

            // Create a new SiteMap node, setting the key and url
            var siteMapNode = new MvcSiteMapNode(this, key, explicitResourceKeys);
            siteMapNode.Title = title;
            siteMapNode.Description = description;

            // Create a route data dictionary
            IDictionary<string, object> routeValues = new Dictionary<string, object>();

            // Add each attribute to our attributes collection on the siteMapNode
            // and to a route data dictionary.
            foreach (XAttribute attribute in node.Attributes())
            {
                var attributeName = attribute.Name.ToString();
                var attributeValue = attribute.Value;

                if (attributeName != "title"
                    && attributeName != "description")
                {
                    siteMapNode[attributeName] = attributeValue;
                }

                // Process route values
                if (
                    attributeName != "title"
                    && attributeName != "description"
                    && attributeName != "resourceKey"
                    && attributeName != "key"
                    && attributeName != "roles"
                    && attributeName != "url"
                    && attributeName != "clickable"
                    && attributeName != "dynamicNodeProvider"
                    && attributeName != "lastModifiedDate"
                    && attributeName != "changeFrequency"
                    && attributeName != "updatePriority"
                    && attributeName != "targetFrame"
                    && attributeName != "imageUrl"
                    && !attributesToIgnore.Contains(attributeName)
                    )
                {
                    routeValues.Add(attributeName, attributeValue);
                }

                if (attributeName == "roles")
                {
                    siteMapNode.Roles = attribute.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            // Inherit area and controller from parent
            var parentMvcNode = parentNode as MvcSiteMapNode;
            if (parentMvcNode != null)
            {
                if (siteMapNode["area"] == null)
                {
                    siteMapNode["area"] = parentMvcNode.Area;
                    routeValues.Add("area", parentMvcNode.Area);
                }
                if (node.GetAttributeValue("controller") == "")
                {
                    siteMapNode["controller"] = parentMvcNode.Controller;
                    routeValues.Add("controller", parentMvcNode.Controller);
                }
            }

            // Add defaults for area
            if (!routeValues.ContainsKey("area"))
            {
                siteMapNode["area"] = "";
                routeValues.Add("area", "");
            }

            // Clickable?
            if (!siteMapNode.Clickable)
            {
                siteMapNode.Url = "";
            }

            // Add route values to sitemap node
            siteMapNode.RouteValues = routeValues;
            routeValues = new Dictionary<string, object>();

            // Add node's route defaults
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            if (routeData != null)
            {
                // Specify defaults from route on siteMapNode
                var route = routeData.Route as Route;
                if (route != null && route.Defaults != null)
                {
                    foreach (var defaultValue in route.Defaults)
                    {
                        if (siteMapNode[defaultValue.Key] == null)
                        {
                            siteMapNode[defaultValue.Key] = defaultValue.Value.ToString();
                        }
                    }
                }
            }

            return siteMapNode;
        }

        /// <summary>
        /// Gets the site map node from MVC site map node attribute.
        /// </summary>
        /// <param name="attribute">IMvcSiteMapNodeAttribute to map</param>
        /// <param name="type">Type.</param>
        /// <param name="methodInfo">MethodInfo on which the IMvcSiteMapNodeAttribute is applied</param>
        /// <returns>
        /// A siteMapNode which represents the IMvcSiteMapNodeAttribute.
        /// </returns>
        protected SiteMapNode GetSiteMapNodeFromMvcSiteMapNodeAttribute(IMvcSiteMapNodeAttribute attribute, Type type, MethodInfo methodInfo)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (methodInfo == null) // try to find Index action
            {
                var ms = type.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public,
                                          (mi, o) => mi != null && string.Equals(mi.Name, "Index"), null);
                foreach (MethodInfo m in ms.OfType<MethodInfo>())
                {
                    var pars = m.GetParameters();
                    if (pars == null || pars.Length == 0)
                    {
                        methodInfo = m;
                        break;
                    }
                }
            }

            // Determine area (will only work if controller is defined as Assembly.<Area>.Controllers.HomeController)
            string area = "";
            if (string.IsNullOrEmpty(area))
            {
                var parts = type.Namespace.Split('.');
                area = parts[parts.Length - 2];

                var assemblyParts = type.Assembly.FullName.Split(',');

                if (type.Namespace == assemblyParts[0] + ".Controllers" || type.Namespace.StartsWith(area))
                {
                    // Is in default areaName...
                    area = "";
                }
            }

            // Determine controller and (index) action
            string controller = type.Name.Replace("Controller", "") ?? "Home";
            string action = (methodInfo != null ? methodInfo.Name : null) ?? "Index";
            if (methodInfo != null) // handle custom action name
            {
                var actionNameAttribute = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true).FirstOrDefault() as ActionNameAttribute;
                if (actionNameAttribute != null)
                {
                    action = actionNameAttribute.Name;
                }
            }

            // Generate key for node
            string key = NodeKeyGenerator.GenerateKey(
                attribute.Key,
                attribute.Url,
                attribute.Title,
                area,
                controller, action,
                attribute.Clickable);

            // Handle title and description globalization
            var explicitResourceKeys = new NameValueCollection();
            var title = attribute.Title;
            var description = attribute.Description;
            HandleResourceAttribute("title", ref title, ref explicitResourceKeys);
            HandleResourceAttribute("description", ref description, ref explicitResourceKeys);

            // Create a new SiteMap node, setting the key and url
            var siteMapNode = new MvcSiteMapNode(this, key, explicitResourceKeys);

            // Set the properties on siteMapNode.
            siteMapNode.Title = title;
            siteMapNode.Description = description;
            siteMapNode.Roles = attribute.Roles;
            siteMapNode["area"] = area;
            siteMapNode["controller"] = controller;
            siteMapNode["action"] = action;
            siteMapNode["dynamicNodeProvider"] = attribute.DynamicNodeProvider;
            siteMapNode.LastModifiedDate = attribute.LastModifiedDate;
            siteMapNode.ChangeFrequency = attribute.ChangeFrequency;
            siteMapNode.UpdatePriority = attribute.UpdatePriority;
            siteMapNode.TargetFrame = attribute.TargetFrame;
            siteMapNode.ImageUrl = attribute.ImageUrl;

            if (!string.IsNullOrEmpty(attribute.Url))
                siteMapNode.Url = attribute.Url;

            siteMapNode.ResourceKey = attribute.ResourceKey;

            // Create a route data dictionary
            IDictionary<string, object> routeValues = new Dictionary<string, object>();
            routeValues.Add("area", area);
            routeValues.Add("controller", controller);
            routeValues.Add("action", action);

            // Add route values to sitemap node
            siteMapNode.RouteValues = routeValues;

            // Clickable?
            siteMapNode.Clickable = attribute.Clickable;

            return siteMapNode;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Handle resource attribute
        /// </summary>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="text">Text</param>
        /// <param name="collection">NameValueCollection to be used for localization</param>
        private static void HandleResourceAttribute(string attributeName, ref string text, ref NameValueCollection collection)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string tempStr1 = null;
                var tempStr2 = text.TrimStart(new[] { ' ' });
                if (((tempStr2 != null) && (tempStr2.Length > 10)) && tempStr2.ToLower(CultureInfo.InvariantCulture).StartsWith("$resources:", StringComparison.Ordinal))
                {
                    tempStr1 = tempStr2.Substring(11);
                    string tempStr3 = null;
                    string tempStr4 = null;
                    var index = tempStr1.IndexOf(',');
                    tempStr3 = tempStr1.Substring(0, index);
                    tempStr4 = tempStr1.Substring(index + 1);
                    var length = tempStr4.IndexOf(',');
                    if (length != -1)
                    {
                        text = tempStr4.Substring(length + 1);
                        tempStr4 = tempStr4.Substring(0, length);
                    }
                    else
                    {
                        text = null;
                    }
                    if (collection == null)
                    {
                        collection = new NameValueCollection();
                    }
                    collection.Add(attributeName, tempStr3.Trim());
                    collection.Add(attributeName, tempStr4.Trim());
                }
            }
        }

        #endregion
    }
}

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcSiteMapProvider.Extensibility;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// DefaultControllerTypeResolver class
    /// </summary>
    public class DefaultControllerTypeResolver
        : IControllerTypeResolver
    {
        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>The cache.</value>
        protected Dictionary<string, Type> Cache { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControllerTypeResolver"/> class.
        /// </summary>
        public DefaultControllerTypeResolver()
        {
            Cache = new Dictionary<string, Type>();
        }

        #region IControllerTypeResolver Members

        /// <summary>
        /// Resolves the type of the controller.
        /// </summary>
        /// <param name="areaName">Name of the area.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>Controller type</returns>
        public Type ResolveControllerType(string areaName, string controllerName)
        {
            // Is the type cached?
            string cacheKey = areaName + "_" + controllerName;
            if (Cache.ContainsKey(cacheKey))
            {
                return Cache[cacheKey];
            }

            // Find controller details
            IEnumerable<string> areaNamespaces = FindNamespacesForArea(areaName, RouteTable.Routes);
            string area = areaName;
            string controller = controllerName;

            // Find controller type
            Type controllerType = null;
            if (!string.IsNullOrEmpty(area) && areaNamespaces.Count() > 0)
            {
                HashSet<string> namespaces = new HashSet<string>(areaNamespaces, StringComparer.OrdinalIgnoreCase);
                controllerType = GetControllerTypeWithinNamespaces(area, controller, namespaces);
            }
            else if (ControllerBuilder.Current.DefaultNamespaces.Count > 0)
            {
                controllerType = GetControllerTypeWithinNamespaces(area, controller, ControllerBuilder.Current.DefaultNamespaces);
            }
            else
            {
                controllerType = GetControllerTypeWithinNamespaces(area, controller, null);
            }

            // Cache the result
            if (!Cache.ContainsKey(cacheKey))
            {
                Cache.Add(cacheKey, controllerType);
            }

            // Return
            return controllerType;
        }

        #endregion

        /// <summary>
        /// Finds the namespaces for area.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="routes">The routes.</param>
        /// <returns>
        /// A namespaces for area represented as a <see cref="string"/> instance
        /// </returns>
        protected IEnumerable<string> FindNamespacesForArea(string area, RouteCollection routes)
        {
            foreach (var route in routes.OfType<Route>().Where(r => r.DataTokens != null))
            {
                if (route.DataTokens["area"] != null
                    && route.DataTokens["area"].ToString() == area
                    && route.DataTokens["Namespaces"] != null)
                {
                    return route.DataTokens["Namespaces"] as IEnumerable<string>;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the controller type within namespaces.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns>
        /// A controller type within namespaces represented as a <see cref="Type"/> instance 
        /// </returns>
        protected Type GetControllerTypeWithinNamespaces(string area, string controller, HashSet<string> namespaces)
        {
            Type foundType = null;

            if (namespaces != null && namespaces.Count > 0)
            {
                foreach (string ns in namespaces)
                {
                    // Cleanup namespace...
                    string controllerNamespace = ns.Replace("*", "");

                    // Find possible assemblies
                    var possibleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(controllerNamespace)).Count() > 0);

                    // Find types
                    var possibleTypes = from a in possibleAssemblies
                                        let types = a.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(controllerNamespace))
                                        from t in types
                                        select t;

                    // Verify candidates
                    foreach (var possibleType in possibleTypes)
                    {
                        if (possibleType.Name == controller + "Controller")
                        {
                            if (foundType == null)
                            {
                                foundType = possibleType;
                            }
                            else
                            {
                                throw new AmbiguousControllerException("Ambiguous controller. Found multiple controller types for " + controller + "Controller.");
                            }
                        }
                    }
                }
            }
            else if (namespaces == null)
            {
                // Find possible assemblies
                var possibleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetTypes().Where(t => t.Name != null && t.Name == controller + "Controller").Count() > 0);

                // Find types
                var possibleTypes = from a in possibleAssemblies
                                    let types = a.GetTypes().Where(t => t.Name != null && t.Name == controller + "Controller")
                                    from t in types
                                    select t;
                
                // Verify candidates
                foreach (var possibleType in possibleTypes)
                {
                    if (possibleType.Name == controller + "Controller")
                    {
                        if (foundType == null)
                        {
                            foundType = possibleType;
                        }
                        else
                        {
                            throw new AmbiguousControllerException("Ambiguous controller. Found multiple controller types for " + controller + "Controller.");
                        }
                    }
                }
            }

            return foundType;
        }
    }
}

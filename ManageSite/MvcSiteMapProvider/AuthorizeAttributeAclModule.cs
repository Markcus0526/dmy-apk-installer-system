#region Using directives

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcSiteMapProvider.Extensibility;
using MvcSiteMapProvider.External;
using Telerik.Web.Mvc.Infrastructure.Implementation;
using System.Web.Mvc.Async;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// AuthorizeAttributeAclModule class
    /// </summary>
    public class AuthorizeAttributeAclModule
        : IAclModule
    {
        #region IAclModule Members

        /// <summary>
        /// Determines whether node is accessible to user.
        /// </summary>
        /// <param name="controllerTypeResolver">The controller type resolver.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="context">The context.</param>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if accessible to user; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAccessibleToUser(IControllerTypeResolver controllerTypeResolver, DefaultSiteMapProvider provider, HttpContext context, SiteMapNode node)
        {
            // Is security trimming enabled?
            if (!provider.SecurityTrimmingEnabled)
            {
                return true;
            }

            // Is it a regular node?
            var mvcNode = node as MvcSiteMapNode;
            if (mvcNode == null)
            {
                throw new AclModuleNotSupportedException("The current ACL module does not provide functionality for regular SiteMapNode objects.");
            }

            // Clickable? Always accessible.
            if (mvcNode.Clickable == false)
            {
                return true;
            }
            
            // Time to delve into the AuthorizeAttribute defined on the node.
            // Let's start by getting all metadata for the controller...
            var controllerType = controllerTypeResolver.ResolveControllerType(mvcNode.Area, mvcNode.Controller);
            if (controllerType == null)
            {
                return false;
            }

            // Find routes for the sitemap node's url
            HttpContextBase httpContext = new HttpContextWrapper(context);
            string originalPath = httpContext.Request.Path;
            httpContext.RewritePath(node.Url,true);
            var routes = RouteTable.Routes.GetRouteData(httpContext);
            routes.DataTokens.Remove("area");
            routes.DataTokens.Remove("Namespaces");
            foreach (var routeValue in mvcNode.RouteValues)
            {
                routes.Values[routeValue.Key] = routeValue.Value;
            }
            var requestContext = new RequestContext(httpContext, routes);
            
            // Find controller context
            var controller = ControllerBuilder.Current.GetControllerFactory().CreateController(requestContext, mvcNode.Controller) as ControllerBase;
            httpContext.RewritePath(originalPath, true);
            if (controller == null)
            {
                return false;
            }

            var controllerContext = new ControllerContext(requestContext, controller);
            ControllerDescriptor controllerDescriptor = new ReflectedAsyncControllerDescriptor(controllerType);
            ActionDescriptor actionDescriptor = controllerDescriptor.FindAction(controllerContext, mvcNode.Action);
            var authorizationContext = new AuthorizationContext(controllerContext, actionDescriptor);

            // Verify accessibility
            IEnumerable<AuthorizeAttribute> authorizeAttributesToCheck = 
                controllerDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToList()
                .Union(actionDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToList());

            foreach (var authorizeAttribute in authorizeAttributesToCheck) {
                try
                {
                    var currentAuthorizationAttributeType = authorizeAttribute.GetType();

                    var builder = new AuthorizeAttributeBuilder();
                    var subclassedAttribute = (currentAuthorizationAttributeType == typeof(AuthorizeAttribute)) ?
                                                               new InternalAuthorize(authorizeAttribute) : // No need to use Reflection.Emit when ASP.NET MVC built-in attribute is used
                                                               (IAuthorizeAttribute)builder.Build(currentAuthorizationAttributeType).Invoke(null);

                    subclassedAttribute.Order = authorizeAttribute.Order;
                    subclassedAttribute.Roles = authorizeAttribute.Roles;
                    subclassedAttribute.Users = authorizeAttribute.Users;

                    return subclassedAttribute.IsAuthorized(authorizationContext.HttpContext);
                }
                catch
                {
                    // do not allow on exception
                    return false;
                }
            }

            // No objection.
            return true;
        }

        #endregion
    }
}

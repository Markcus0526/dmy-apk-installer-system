#region Using directives

using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;

#endregion

namespace MvcSiteMapProvider.Filters
{
    /// <summary>
    /// SiteMapTitle attribute
    /// Can be used for setting sitemap title on an action method.
    /// Credits go to Kenny Eliasson - http://mvcsitemap.codeplex.com/Thread/View.aspx?ThreadId=67056
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SiteMapTitleAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// Property name of ViewData to look in
        /// </summary>
        protected readonly string PropertyName;

        /// <summary>
        /// Creates a new SiteMapTitleAttribute instance.
        /// </summary>
        /// <param name="propertyName">Property in ViewData used as the SiteMap.CurrentNode.Title</param>
        public SiteMapTitleAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Fires when action executed
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is ViewResult)
            {
                var result = (ViewResult)filterContext.Result;

                var target = ResolveTarget(result.ViewData.Model, PropertyName);
                if (target == null)
                {
                    target = ResolveTarget(result.ViewData, PropertyName);
                }

                if (target != null)
                {
                    if (SiteMap.CurrentNode != null)
                    {
                        SiteMap.CurrentNode.Title = target.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Resolve target
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="expression">Target expression</param>
        /// <returns>
        /// A target represented as a <see cref="object"/> instance 
        /// </returns>
        internal static object ResolveTarget(object target, string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return target;

            var left = expression;
            var right = "";

            if (expression.IndexOf(".") > -1)
            {
                left = expression.Substring(0, expression.IndexOf("."));
                right = expression.Substring(expression.IndexOf(".") + 1);
            }
            object result;
            try
            {
                if (target is ViewDataDictionary)
                {
                    result = ((ViewDataDictionary)target).Eval(expression);
                }
                else if (target is IDictionary)
                {
                    result = result = ResolveTarget(
                        ((IDictionary)target)[left],
                        right);
                }
                else if (target is IList)
                {
                    result = result = ResolveTarget(
                        ((IList)target)[int.Parse(left)],
                        right);
                }
                else
                {
                    var propertyInfo = target.GetType().GetProperty(left);
                    result = ResolveTarget(
                        propertyInfo.GetValue(target, null),
                        right);
                }
            }
            catch (NullReferenceException)
            {
                result = null;
            }

            return result;
        }
    }

}

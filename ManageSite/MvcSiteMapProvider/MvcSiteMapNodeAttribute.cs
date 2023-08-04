#region Using directives

using System;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// SiteMap node attribute, used to decorate action methods with SiteMap node metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class MvcSiteMapNodeAttribute : Attribute, IMvcSiteMapNodeAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MvcSiteMapNodeAttribute()
        {
            Clickable = true;
        }

        /// <summary>
        /// SiteMap node key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// SiteMap node title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// SiteMap node description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// SiteMap node URL (optional)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// SiteMap node parent key
        /// </summary>
        public string ParentKey { get; set; }

        /// <summary>
        /// Resource key, used when working with localization
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the roles that may access the SiteMap node.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Is it a clickable node?
        /// </summary>
        public bool Clickable { get; set; }

        /// <summary>
        /// Dynamic node provider
        /// </summary>
        public string DynamicNodeProvider { get; set; }

        /// <summary>
        /// Used for ordering nodes
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        /// <value>The last modified date.</value>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the change frequency.
        /// </summary>
        /// <value>The change frequency.</value>
        public ChangeFrequency ChangeFrequency { get; set; }

        /// <summary>
        /// Gets or sets the update priority.
        /// </summary>
        /// <value>The update priority.</value>
        public UpdatePriority UpdatePriority { get; set; }

        /// <summary>
        /// Gets or sets the target frame.
        /// </summary>
        /// <value>The target frame.</value>
        public string TargetFrame { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>The image URL.</value>
        public string ImageUrl { get; set; }
    }
}

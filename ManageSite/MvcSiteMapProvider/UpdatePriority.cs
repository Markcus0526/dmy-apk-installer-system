#region Using directives

using System;

#endregion

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Sitemap update priority
    /// </summary>
    [Serializable]
    public enum UpdatePriority
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 9999,

        /// <summary>
        /// Automatic
        /// </summary>
        Automatic = 50,

        /// <summary>
        /// Low
        /// </summary>
        Low = 0,

        /// <summary>
        /// Normal
        /// </summary>
        Normal = 50,

        /// <summary>
        /// High
        /// </summary>
        High = 75,

        /// <summary>
        /// Critical
        /// </summary>
        Critical = 100
    }
}

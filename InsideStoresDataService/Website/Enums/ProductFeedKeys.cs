using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// List of keys for product feeds supported by this data service.
    /// </summary>
    public enum ProductFeedKeys
    {
        /// <summary>
        /// Google product search. (Active)
        /// </summary>
        Google,

        /// <summary>
        /// Bing product search - via adCenter. (Active)
        /// </summary>
        Bing,

        /// <summary>
        /// Amazon (Active)
        /// </summary>
        Amazon,

        /// <summary>
        /// TheFind.com  (no longer used)
        /// </summary>
        TheFind,

        /// <summary>
        /// Nextag.com  (no longer used)
        /// </summary>
        Nextag,

        /// <summary>
        /// Shopzilla.com  (no longer used)
        /// </summary>
        Shopzilla,

        /// <summary>
        /// Pronto.com  (no longer used)
        /// </summary>
        Pronto,

        /// <summary>
        /// PriceGrabber.com  (no longer used)
        /// </summary>
        PriceGrabber,

        /// <summary>
        ///  Shopping.com  (no longer used)
        /// </summary>
        Shopping,

        /// <summary>
        ///  Custum feed by InsideStores created for Shopify import. (Active)
        /// </summary>
        Shopify,

        /// <summary>
        /// Google Canada product search. (Active)
        /// </summary>
        /// <remarks>
        /// Prices 1.4x for Canadian dollars, links to shopify CA website.
        /// </remarks>
        GoogleCanada,

        /// <summary>
        /// 1stDibs.com (Active)
        /// </summary>
        FirstDibs
    }
}
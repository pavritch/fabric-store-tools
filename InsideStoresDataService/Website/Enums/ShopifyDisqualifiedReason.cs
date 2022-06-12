using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// The reason why a specific product is diqualified from being on shopify.
    /// </summary>
    /// <remarks>
    /// Value stored as int column in SQL.
    /// </remarks>
    public enum ShopifyDisqualifiedReason
    {
        /// <summary>
        /// Disqualified based on some sort of manual or curation criteria.
        /// </summary>
        /// <remarks>
        /// Unlike a missing image, cannot be cured via automation. Requires manual intervention to change state.
        /// </remarks>
        Manual,

        /// <summary>
        /// Product has no images. We only post products with images to Shopify.
        /// </summary>
        /// <remarks>
        /// Can be cured via automation when an image is detected.
        /// </remarks>
        MissingImage,

    }
}
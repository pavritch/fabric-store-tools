using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// What is the disposition of a locally known product to the live shopify store.
    /// </summary>
    /// <remarks>
    /// Values are int field in SQL table.
    /// </remarks>
    public enum ShopifyLiveStatus
    {
        /// <summary>
        /// No specific status - as in has not yet been determined because not yet pushed to Shopify.
        /// </summary>
        /// <remarks>
        /// Not on shopify! But could be a new product or something whereas not disqualified either.
        /// </remarks>
        None,

        /// <summary>
        /// The product is not on Shopify in any form and is disqualified for a specific reason.
        /// </summary>
        Disqualified,

        // NOTE: if on shopify in any form, must be either published or unpublished status

        /// <summary>
        /// Exists on Shopify, but is presently marked as unpublished. 
        /// </summary>
        /// <remarks>
        /// Most often because discontinued, but could be other reasons.
        /// </remarks>
        Unpublished,

        /// <summary>
        /// Fully live, published and well on Shopify.
        /// </summary>
        Published
    }
}
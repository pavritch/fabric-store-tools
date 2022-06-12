using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public enum ShopifyProductOperation
    {
        /// <summary>
        /// Used only to initiate Shopify support features. Perform the very first full query from live store to populate SQL.
        /// </summary>
        VirginReadProduct,

        /// <summary>
        /// A full update of live shopify product is needed (from local SQL).
        /// </summary>
        FullUpdate,

        /// <summary>
        /// A partial update of shopify product is needed. Just price and availability.
        /// </summary>
        PriceAndAvailability,

        /// <summary>
        /// Notify local system of a new product to become aware of and start tracking.
        /// </summary>
        /// <remarks>
        /// It's possible we don't yet have images. That might lead to a temporary disqualification from going live.
        /// </remarks>
        NewProduct,

        /// <summary>
        /// Delete this product globally - both live shopify and local shopify SQL.
        /// </summary>
        Delete,

        /// <summary>
        /// This product should no longer be live on shopify due to some constraint.
        /// </summary>
        /// <remarks>
        /// Should be deleted from shopify, not just unpublished.
        /// </remarks>
        Disqualify,

        /// <summary>
        /// See if a disqualified product might now satisfy whatever condition caused it to otherwise be disqualified.
        /// </summary>
        /// <remarks>
        /// When a product passes being requalified, a full update is needed (that's our rule to keep things simple).
        /// </remarks>
        Requalify,

        /// <summary>
        /// Notify that images relating to this product have changed - invoke reevaluation.
        /// </summary>
        /// <remarks>
        /// Might trigger disqualified product to become qualified.
        /// </remarks>
        ImageUpdate,
    }
}
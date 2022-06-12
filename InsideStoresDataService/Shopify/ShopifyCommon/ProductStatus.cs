using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopifyCommon
{
    /// <summary>
    /// List of stock status states.
    /// </summary>
    /// <remarks>
    /// When downloading, a product is published or not, but not deleted (obviously).
    /// Unpublished from download get turned to Deleted on the local side upon running an update.
    /// </remarks>
    public enum ProductStatus
    {
        InStock,
        OutOfStock,
        Unpublished, // on shopify, which means will get deleted on next run
        Deleted // means deleted from shopify, don't attempt to update
    }
}
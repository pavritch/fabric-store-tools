using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Result from processing a single Shopify product event in ShopifySyncProduct.
    /// </summary>
    public enum ShopifyProductEventResult
    {
        Success,
        Failed,
        Cancelled,
    }
}
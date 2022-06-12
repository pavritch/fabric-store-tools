using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCommon
{
    /// <summary>
    /// This is all the information we track for shopify products.
    /// </summary>
    public class ProductInformation
    {
        // shopify references - never changed 
        public long ShopifyProductId { get; set; }
        public long ShopifyVariantId { get; set; }
        public string ShopifyHandle { get; set; }

        // our store and productID - never changed
        public StoreKey Store { get; set; }
        public int ProductID { get; set; }

        /// <summary>
        /// This is the status already sync'd to shopify.
        /// </summary>
        /// <remarks>
        /// Update operations will update this status from local SQL and then push to shopify.
        /// </remarks>
        public ProductStatus Status { get; set; }

        /// <summary>
        /// Price last set on shopify
        /// </summary>
        public double Price { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Shopify product data serialized and persisted to SQL.
    /// </summary>
    public class ShopifyLocalProduct
    {
        #region Persisted Properties
        /// <summary>
        /// Exact copy of full data set returned by Shopify API.
        /// </summary>
        /// <remarks>
        /// Includes variants. Does not include meta data.
        /// </remarks>
        public ShopifySharp.ShopifyProduct Product { get; set; }

        /// <summary>
        /// Meta fields for this product. Exactly reflects live state.
        /// </summary>
        /// <remarks>
        /// Obtained via separate API call. Not merged into main product to 
        /// mirror how the API behaves.
        /// </remarks>
        public List<ShopifySharp.ShopifyMetaField> ProductMetaFields { get; set; }

        /// <summary>
        /// Meta fields for each variant. Dic key is Shopify variantID. Exactly reflects live state.
        /// </summary>
        /// <remarks>
        /// If a variant is missing from the dic, then should assume it does not have any meta fields.
        /// </remarks>
        public Dictionary<long, List<ShopifySharp.ShopifyMetaField>> VariantMetaFields { get; set; }

        /// <summary>
        /// True to indicate this product is globally discontinued.
        /// </summary>
        public bool IsDiscontinued { get; set; }

        /// <summary>
        /// State of this known product on the live store.
        /// </summary>
        public ShopifyLiveStatus Status { get; set; }

        /// <summary>
        /// When a product is disqualified, this is the reason.
        /// </summary>
        public ShopifyDisqualifiedReason? DisqualifiedReason { get; set; } 
        #endregion

        #region Serialization
        public string Serialize()
        {
            return this.ToJSON();
        }

        public static ShopifyLocalProduct Deserialize<ShopifyLocalProduct>(string json)
        {
            return json.FromJSON<ShopifyLocalProduct>();
        } 
        #endregion
    }
}
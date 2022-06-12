using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifySharp;
using RestSharp;
using ShopifySharp.Filters;
using Newtonsoft.Json;

namespace ShopifyUpdateProducts
{
    public class ShopifySmallProductVariant : ShopifyObject
    {
        /// <summary>
        /// The number of items in stock for this product variant.
        /// </summary>
        [JsonProperty("inventory_quantity")]
        public int InventoryQuantity { get; set; }

        /// <summary>
        /// The price of the product variant.
        /// </summary>
        [JsonProperty("price")]
        public double Price { get; set; }
    }

    public class UpdateProductVariantService : ShopifyProductVariantService
    {
        public UpdateProductVariantService(string myShopifyUrl, string shopAccessToken) : base(myShopifyUrl, shopAccessToken) { }

        public async Task<ShopifySmallProductVariant> UpdateInventoryAndPriceAsync(ShopifySmallProductVariant variant)
        {
            var req = RequestEngine.CreateRequest(string.Format("variants/{0}.json", variant.Id.Value), Method.PUT, "variant");

            req.AddJsonBody(new { variant });

            return await RequestEngine.ExecuteRequestAsync<ShopifySmallProductVariant>(_RestClient, req);
        }
    }
}

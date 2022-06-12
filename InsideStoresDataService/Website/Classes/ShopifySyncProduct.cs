using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Website.Shopify.Entities;

namespace Website
{

    /// <summary>
    /// Contains logic/knowledge on how to sync between local SQL and live shopify store.
    /// </summary>
    public class ShopifySyncProduct
    {
        private ShopifyStore Store {get; set;}
        private Dictionary<StoreKeys, IWebStore> WebStores { get; set; }

        /// <summary>
        /// Data persisted for product in SQL.
        /// </summary>
        private ShopifyLocalProduct Data {get; set;}

        public ShopifySyncProduct(ShopifyStore store, Dictionary<StoreKeys, IWebStore> webStores)
        {
            this.Store = store;
            this.WebStores = webStores;
        }

        public IWebStore GetWebStore(StoreKeys storeKey)
        {
            IWebStore store = null;
            WebStores.TryGetValue(storeKey, out store);
            return store;
        }


        /// <summary>
        /// Process a single event from the queue (table ProductEvents).
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public ShopifyProductEventResult ProcessEvent(ProductEvent evt)
        {
            ShopifyProductOperation op = (ShopifyProductOperation)evt.Operation;

            var storeKey = (StoreKeys)evt.StoreID;
            var webStore = GetWebStore(storeKey);
            var productID = evt.ProductID;

            var result = ShopifyProductEventResult.Failed;

            try
            {
                switch (op)
                {
                    case ShopifyProductOperation.VirginReadProduct:
                        result = ExecuteVirginReadProduct(webStore, productID, evt.Data.FromJSON<LiveShopifyProduct>());
                        break;

                    case ShopifyProductOperation.FullUpdate:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.PriceAndAvailability:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.NewProduct:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.Delete:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.Disqualify:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.Requalify:
                        throw new NotImplementedException();

                    case ShopifyProductOperation.ImageUpdate:
                        throw new NotImplementedException();

                    default:
                        throw new NotImplementedException();

                }

            }
            catch(Exception Ex)
            {
                Store.WriteLog(ShopifyLogLocus.Products, ShopifyLogType.Error, string.Format("ProcessProductEvent({3}, {0}, {1}) Exception: {2}", storeKey.ToString(), productID, Ex.ToString(), op.ToString()));
                Debug.WriteLine(Ex.ToString());
                result = ShopifyProductEventResult.Failed;
            }

            return result;
        }

        /// <summary>
        /// For initial population only! Given ID of known shopify product, get all information and put into SQL.
        /// </summary>
        /// <remarks>
        /// This forms the starting point for truth data on our local side.
        /// </remarks>
        /// <param name="webStore"></param>
        /// <param name="productID"></param>
        /// <param name="liveProduct"></param>
        /// <returns></returns>
        private ShopifyProductEventResult ExecuteVirginReadProduct(IWebStore webStore, int productID, LiveShopifyProduct liveProduct)
        {
            var now = DateTime.Now;

            var shopifyProduct = new Shopify.Entities.Product()
            {
                StoreID = (int)webStore.StoreKey,
                ProductID = productID,
                ShopifyProductID = liveProduct.ShopifyProductID,
                Created = now,
                LastModified = now,
                Data = null, // json data
                LastFullUpdate = null,
                LastAvailabilityUpdate = null,
                LastOperation = (int)ShopifyProductOperation.VirginReadProduct,
                LastOperationResult = (int)ShopifyProductEventResult.Success,
                LastOperationErrorData = null,
                FullUpdateRequired = 0,
                CreatedOnShopify = DateTime.Parse("11/1/2016") // indicates date for original load via CSV files
            };

            // persist
            using (var dc = new ShopifyDataContext(Store.ConnectionString))
            {
                dc.Products.InsertOnSubmit(shopifyProduct);
                dc.SubmitChanges();
            }

            return ShopifyProductEventResult.Success;
        }

    }
}
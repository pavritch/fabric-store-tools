using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App
{
    /// <summary>
    /// Implements IStoreDatabaseConnector to virtualize access to SQL.
    /// </summary>
    public class FakeStoreDatabaseConnector : IStoreDatabaseConnector
    {
        // SHANE - this concrete class must be fleshed out by you

        private IAppModel AppModel
        {
            get
            {
                // purposely not held internally since that would create a circular reference.
                // Cannot inject in ctor since AppModel concrete class already depends on this being injected.
                return App.GetInstance<IAppModel>();
            }
        }

        /// <summary>
        /// Determine if a SQL database connection is available for this store.
        /// </summary>
        /// <remarks>
        /// Can be as simple as seeing if can read just a single row from a stock table, just
        /// to see if succeeds or throws exception.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        public Task<bool> IsDatabaseAvailableAsync(StoreType storeKey)
        {
            // FAKE
            return Task.FromResult(true);
        }

        /// <summary>
        /// Determine if this vendor has a record in the SQL ASPDNSF Manufacturer table.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        public Task<bool> IsVendorInDatabaseAsync(StoreType storeKey, int manufacturerID)
        {
            // FAKE
            return Task.FromResult(true);
        }


        /// <summary>
        /// Returns a set of extended product data for the specified set of productIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 products per request.
        /// Should return list in same order as original IDs.
        /// </remarks>
        public Task<List<ProductSupplementalData>> GetProductSupplementalDataAsync(StoreType storeKey, List<int> products)
        {
            // All faked - returns nearly the same fake record for each item.

            var list = new List<ProductSupplementalData>();

            foreach(var id in products)
            {
                var record = new ProductSupplementalData
                {
                    ProductID = id,
                    SKU = "BR-2623-001276",
                    Name = "2623-001276 Lucido Pink Satin Stripe by Brewster",
                    ProductGroup = "Fabric",
                    StoreUrl = "http://www.insidefabric.com/p-1076148-2623-001276-lucido-pink-satin-stripe-by-brewster.aspx",
                };

                list.Add(record);
            }

            // on the URL, not SEName column will have something like this: 1076148-2623-001276-lucido-pink-satin-stripe-by-brewster
            // so the code logic will need to string fmt adding the domain, /p-XXXXX and the .aspx suffix. 

            // you can find out the domain name by using the storeKey to lookup via IAppModel to get the collection of Stores.

            return Task.FromResult(list);
        }

        /// <summary>
        /// Returns a set of extended product variant data for the specified set of variantIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 variants per request. 
        /// Should return list in same order as original IDs.
        /// </remarks>
        public Task<List<VariantSupplementalData>> GetVariantSupplementalDataAsync(StoreType storeKey, List<int> variants)
        {
            // All faked - returns nearly the same fake record for each item.

            var list = new List<VariantSupplementalData>();

            foreach (var id in variants)
            {
                var record = new VariantSupplementalData
                {
                    VariantID = id,
                    ProductID = 102033,
                    SKU = "BR-2623-001276",
                    Name = "2623-001276 Lucido Pink Satin Stripe by Brewster",
                    ProductGroup = "Fabric",
                    StoreUrl = "http://www.insidefabric.com/p-1076148-2623-001276-lucido-pink-satin-stripe-by-brewster.aspx",
                    UnitOfMeasure = "Yard",
                };

                list.Add(record);
            }

            // on the URL, not SEName column will have something like this: 1076148-2623-001276-lucido-pink-satin-stripe-by-brewster
            // so the code logic will need to string fmt adding the domain, /p-XXXXX and the .aspx suffix. 

            return Task.FromResult(list);
        }


        /// <summary>
        /// Fetch the full set of product counts displayed on dashboards.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        public async Task<ProductCountMetrics> GetProductCountMetricsAsync(StoreType storeKey, int manufacturerID)
        {
            // for live code, will need to make a variety of calls into SQL

            var rnd = new Random(manufacturerID);
            var productCount = rnd.Next(1000, 20000);

            await Task.Delay(100); // fake latency

            var metrics = new ProductCountMetrics()
            {
                ProductCount = productCount,
                ProductVariantCount = rnd.Next(3000, 40000),
                DiscontinuedProductCount = rnd.Next(1000, 8000),
                ClearanceProductCount = rnd.Next(100, 2000),

                InStockProductCount = productCount / 2,
                OutOfStockProductCount = productCount / 4,
                InStockProductVariantCount = rnd.Next(1000, 8000),
                OutOfStockProductVariantCount = rnd.Next(1000, 8000),
            };

            return metrics;
        }
    }
}

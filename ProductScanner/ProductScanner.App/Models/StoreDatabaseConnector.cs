using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App
{
    public class StoreDatabaseConnector : IStoreDatabaseConnector
    {
        // several of these are just passthroughs - probably makes more sense to just use the interface from the core
        // and get rid of this class

        /// <summary>
        /// Determine if a SQL database connection is available for this store.
        /// </summary>
        /// <remarks>
        /// Can be as simple as seeing if can read just a single row from a stock table, just
        /// to see if succeeds or throws exception.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        public async Task<bool> IsDatabaseAvailableAsync(StoreType storeKey)
        {
            try
            {
                var database = GetStoreDatabase(storeKey);
                return await database.DoesStoreExistAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if this vendor has a record in the SQL ASPDNSF Manufacturer table.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        public Task<bool> IsVendorInDatabaseAsync(StoreType storeKey, int manufacturerID)
        {
            var database = GetStoreDatabase(storeKey);
            return database.DoesVendorExistAsync(manufacturerID);
        }

        /// <summary>
        /// Returns a set of extended product data for the specified set of productIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 products per request.
        /// Should return list ordered by ProductID.
        /// </remarks>
        public async Task<List<ProductSupplementalData>> GetProductSupplementalDataAsync(StoreType storeKey, List<int> products)
        {
            var database = GetStoreDatabase(storeKey);
            return await database.GetProductSupplementalDataAsync(products);
        }

        /// <summary>
        /// Returns a set of extended product variant data for the specified set of variantIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 variants per request. 
        /// Should return list ordered by variantID.
        /// </remarks>
        public async Task<List<VariantSupplementalData>> GetVariantSupplementalDataAsync(StoreType storeKey, List<int> variants)
        {
            var database = GetStoreDatabase(storeKey);
            return await database.GetVariantSupplementalDataAsync(variants);
        }

        /// <summary>
        /// Fetch the full set of product counts displayed on dashboards.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        public async Task<ProductCountMetrics> GetProductCountMetricsAsync(StoreType storeKey, int manufacturerID)
        {
            var storeDatabase = GetStoreDatabase(storeKey);
            var metrics = await storeDatabase.GetProductCountMetricsAsync(manufacturerID);
            return metrics;
        }

        // if we make StoreDatabaseConnector generic we can get rid of this ugliness
        // or just get rid of this class altogether
        public IStoreDatabase GetStoreDatabase(StoreType storeType)
        {
            var storeDatabaseType = typeof (IStoreDatabase<>).MakeGenericType(storeType.GetStore().GetType());
            return App.GetInstance(storeDatabaseType) as IStoreDatabase;
        }
    }
}

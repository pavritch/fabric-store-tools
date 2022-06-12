using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App
{

    /// <summary>
    /// Abstraction for database access to ASPDNSF.
    /// </summary>
    
    // note: since every method here takes in the store key, it makes sense to have this be generic on store type
    public interface IStoreDatabaseConnector
    {
        /// <summary>
        /// Determine if a SQL database connection is available for this store.
        /// </summary>
        /// <remarks>
        /// Can be as simple as seeing if can read just a single row from a stock table, just
        /// to see if succeeds or throws exception.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <returns></returns>
        Task<bool> IsDatabaseAvailableAsync(StoreType storeKey);

        /// <summary>
        /// Determine if this vendor has a record in the SQL ASPDNSF Manufacturer table.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        Task<bool> IsVendorInDatabaseAsync(StoreType storeKey, int manufacturerID);

        /// <summary>
        /// Returns a set of extended product data for the specified set of productIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 products per request. 
        /// </remarks>
        Task<List<ProductSupplementalData>> GetProductSupplementalDataAsync(StoreType storeKey, List<int> products);

        /// <summary>
        /// Returns a set of extended product variant data for the specified set of variantIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 variants per request. 
        /// </remarks>
        Task<List<VariantSupplementalData>> GetVariantSupplementalDataAsync(StoreType storeKey, List<int> variants);

        /// <summary>
        /// Fetch the full set of product counts displayed on dashboards.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        Task<ProductCountMetrics> GetProductCountMetricsAsync(StoreType storeKey, int manufacturerID);
    }
}

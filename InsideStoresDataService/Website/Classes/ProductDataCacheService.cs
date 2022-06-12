//------------------------------------------------------------------------------
// 
// Class: ProductDataCacheService 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Website.Entities;
using System.Diagnostics;
using System.Web.Caching;
using System.Threading;

namespace Website
{
    public class ProductDataService<T> : SqlDataCacheBase where T : class, IProductDataCache, new()
    {
#if DEBUG
        private const int RefreshIntervalSeconds = 60 * 1000;
#else
        private const int RefreshIntervalSeconds = 60 * 30;
#endif
        private readonly string connectionString;
        private readonly StoreKeys storeKey;

        public ProductDataService(StoreKeys storeKey, string connectionString)
            : base(RefreshIntervalSeconds + RandomSeconds)
        {
            this.connectionString = connectionString;
            this.storeKey = storeKey;

            Refresh();
        }

        /// <summary>
        /// Returns the fully populated data cache object else null if does not exist yet.
        /// </summary>
        public T Data
        {
            get
            {
                lock (lockObject)
                {
                    return AspNetCache[CacheKey] as T;
                }
            }
        }


        #region Overrides of SqlDataCacheBase

        /// <summary>
        /// Must return the actual data object to be stored in ASP.NET cache.
        /// </summary>
        /// <returns></returns>
        protected override object GetData()
        {
            Debug.WriteLine("Populating cache for {0}: {1}", storeKey, DateTime.Now);
            var cache = new T();
            var bSuccess = cache.Populate(storeKey, connectionString);

            return bSuccess ? cache : null;
        }

        #endregion


        /// <summary>
        /// Callback from ASP.NET cache manager. Called when time has expired and we should 
        /// get new data.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="reason"></param>
        /// <param name="expensiveObject"></param>
        /// <param name="dependency"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="slidingExpiration"></param>
        protected override void CacheExpirationCallback(
                string key,
                CacheItemUpdateReason reason,
                out Object expensiveObject,
                out CacheDependency dependency,
                out DateTime absoluteExpiration,
                out TimeSpan slidingExpiration)
        {
            bool bHasLock = false;
            IWebStore store = null;

            try
            {
                store = MvcApplication.Current.GetWebStore(storeKey);
                Monitor.TryEnter(store.ExclusiveDatabaseLockObject, ref bHasLock);

                // if we did not get the lock, then some kind of database operation
                // is in motion so we keep the existing data

                if (bHasLock)
                    expensiveObject = GetData();
                else
                    expensiveObject = AspNetCache[key];

                dependency = null;
                absoluteExpiration = AbsoluteExpiration;
                slidingExpiration = SlidingExpiration;
            }
            catch
            {
                expensiveObject = null;
                dependency = null;
                absoluteExpiration = Cache.NoAbsoluteExpiration;
                slidingExpiration = Cache.NoSlidingExpiration;
            }
            finally
            {
                if(bHasLock)
                    Monitor.Exit(store.ExclusiveDatabaseLockObject);
            }
        }        
        
    }

}
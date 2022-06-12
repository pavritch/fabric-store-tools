using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Web.Caching;
using System.Diagnostics;

namespace Website
{
    /// <summary>
    /// Used to temporarily cache the results for a list of products in a product collection
    /// </summary>
    public class CachedProductListByProductCollectionResult
    {
        const int CacheTimeSeconds = 60 * 10; // 10 minutes
        #region Locals

        private static readonly object lockObj = new object();

        public string Key { get; set; }
        public List<int> Products { get; set; }

        #endregion


        public CachedProductListByProductCollectionResult(int collectionID, List<int> products)
        {
            Key = MakeCacheKey(collectionID);
            this.Products = products;
        }

        public static string MakeCacheKey(int collectionID)
        {
            return string.Format("CachedProductListByProductCollectionResult:{0}", collectionID);
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }


        public static CachedProductListByProductCollectionResult Lookup(int collectionID)
        {
            var key = MakeCacheKey(collectionID);
            var data = HttpRuntime.Cache[key] as CachedProductListByProductCollectionResult;
            return data;
        }

        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            //Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }
    }
}
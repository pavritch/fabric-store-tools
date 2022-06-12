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
    /// Used to temporarily cache lists of new products by manufacturer
    /// </summary>
    public class CachedNewProductsByManufacturerResult
    {
        const int CacheTimeSeconds = 60 * 20; // 20 minutes
        #region Locals

        private static readonly object lockObj = new object();

        public string Key { get; set; }
        public List<int> Products { get; set; }

        #endregion


        public CachedNewProductsByManufacturerResult(StoreKeys storeKey, int? manufactureID, int days, List<int> products)
        {
            Key = MakeCacheKey(storeKey, manufactureID, days);
            this.Products = products;
        }

        public static string MakeCacheKey(StoreKeys storeKey, int? manufacturerID, int days)
        {
            return string.Format("CachedNewProductsByManufacturerResult:{0}:{1}:{2}", storeKey, manufacturerID.GetValueOrDefault(), days);
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }


        public static CachedNewProductsByManufacturerResult Lookup(StoreKeys storeKey, int? manufacturerID, int days)
        {
            var key = MakeCacheKey(storeKey, manufacturerID, days);
            var data = HttpRuntime.Cache[key] as CachedNewProductsByManufacturerResult;
            return data;
        }

        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            //Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }
    }
}
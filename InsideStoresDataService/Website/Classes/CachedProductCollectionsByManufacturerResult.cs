using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Web.Caching;
using System.Diagnostics;
using Website.Entities;

namespace Website
{
    /// <summary>
    /// Used to temporarily cache the results of all books for a given manufacturer
    /// </summary>
    public class CachedProductCollectionsByManufacturerResult
    {
        const int CacheTimeSeconds = 60 * 20; // 20 minutes
        #region Locals

        private static readonly object lockObj = new object();

        public string Key { get; set; }
        public List<ProductCollection> Collections { get; set; }

        #endregion


        public CachedProductCollectionsByManufacturerResult(StoreKeys storeKey, int manufacturerID, List<ProductCollection> collections, string keyPrefix)
        {
            Key = MakeCacheKey(storeKey, manufacturerID, keyPrefix);
            this.Collections = collections;
        }

        public static string MakeCacheKey(StoreKeys storeKey, int manufacturerID, string keyPrefix)
        {
            return string.Format("{0}:{1}:{2}", keyPrefix, storeKey, manufacturerID);
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }

        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            //Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }

    }
}
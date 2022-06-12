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
    /// Used to temporarily cache the results for a list of products in a pattern (by correlator)
    /// </summary>
    public class CachedListByPatternCorrelatorResult
    {
        const int CacheTimeSeconds = 60 * 10; // 10 minutes
        #region Locals

        private static readonly object lockObj = new object();

        public string Key { get; set; }
        public List<int> Products { get; set; }

        #endregion


        public CachedListByPatternCorrelatorResult(string pattern, bool skipMissingImages, List<int> products)
        {
            Key = MakeCacheKey(pattern, skipMissingImages);
            this.Products = products;
        }

        public static string MakeCacheKey(string pattern, bool skipMissingImages)
        {
            return string.Format("CachedListByPatternCorrelatorResult:{0}:{1}", pattern, skipMissingImages.ToString());
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }


        public static CachedListByPatternCorrelatorResult Lookup(string pattern, bool skipMissingImages)
        {
            var key = MakeCacheKey(pattern, skipMissingImages);
            var data = HttpRuntime.Cache[key] as CachedListByPatternCorrelatorResult;
            return data;
        }

        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            //Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }


    }
}
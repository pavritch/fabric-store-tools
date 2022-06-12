using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Web.Caching;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Website
{
    /// <summary>
    /// Used to temporarily cache the results for an auto suggest query in ASP.NET memory.
    /// </summary>
    public class CachedAutoSuggestResult
    {
        const Formatting fmt = Formatting.None;
        const int CacheTimeSeconds = (60 * 60) * 1; // 1hr
        #region Locals

        private static readonly object lockObj = new object();

        public string Key { get; set; }
        public List<int> Suggestions { get; set; }

        #endregion


        public CachedAutoSuggestResult(AutoSuggestQuery searchCriteria, List<int> Suggestions)
        {
            Key = MakeCacheKey(searchCriteria);
            this.Suggestions = Suggestions;
        }

        public static string MakeCacheKey(AutoSuggestQuery searchCriteria)
        {
            // use a clone to tweak query to lower case - we want all cached results to be case insenstive
            var clone = new AutoSuggestQuery()
            {
                Query = searchCriteria.Query.ToLower(),
                Take = searchCriteria.Take,
                ListID = searchCriteria.ListID,
                Mode = searchCriteria.Mode,
            };

            // the digest of the query parameters is used as the cache key
            var json = JsonConvert.SerializeObject(clone, fmt);
            return string.Format("AutoSuggest:{0}", json.GetMD5HashCode());
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }


        public static CachedAutoSuggestResult Lookup(AutoSuggestQuery searchCriteria)
        {
            var key = MakeCacheKey(searchCriteria);
            var data = HttpRuntime.Cache[key] as CachedAutoSuggestResult;
            return data;
        }

        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }

    }
}
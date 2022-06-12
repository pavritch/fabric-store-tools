using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Web.Caching;
using System.Diagnostics;
using Newtonsoft.Json;
using InsideFabric.Data;

namespace Website
{
    /// <summary>
    /// Used to temporarily cache the results of a search in ASP.NET memory.
    /// </summary>
    public class CachedSearchResult
    {
        const Formatting fmt = Formatting.None;
        const int CacheTimeSeconds = (60 * 15); // 15 minutes
        #region Locals

        private static object lockObj = new object();

        public string Key { get; set; }
        public string ProductDataIdentity { get; set; }

        private Dictionary<ProductSortOrder, List<int>> dicProducts = new Dictionary<ProductSortOrder, List<int>>();

        #endregion

        public List<int> Products
        {
            get
            {
                return GetSortedList(ProductSortOrder.Default);
            }
            set
            {
                AddSortedList(ProductSortOrder.Default, value);
            }
        }

        public void AddSortedList(ProductSortOrder sortOrder, List<int> list)
        {
            lock (this)
            {
                dicProducts[sortOrder] = list;
            }
        }

        public List<int> GetSortedList(ProductSortOrder sortOrder)
        {
            lock (this)
            {
                List<int> value = null;

                dicProducts.TryGetValue(sortOrder, out value);
                return value;
            }
        }

        public CachedSearchResult(IProductDataCache productContext, string Query, List<int> Products)
        {
            Key = MakeSearchCacheKey(productContext, Query);
            ProductDataIdentity = productContext.Identity;
            this.Products = Products;
        }

        public CachedSearchResult(IProductDataCache productContext, SearchCriteria searchCriteria, List<int> Products)
        {
            var json = JsonConvert.SerializeObject(searchCriteria, fmt);

            Key = MakeSearchCacheKey(productContext, json);
            ProductDataIdentity = productContext.Identity;
            this.Products = Products;
        }

        public CachedSearchResult(IProductDataCache productContext, FacetSearchCriteria searchCriteria, List<int> Products)
        {
            var json = JsonConvert.SerializeObject(searchCriteria, fmt);

            Key = MakeSearchCacheKey(productContext, json);
            ProductDataIdentity = productContext.Identity;
            this.Products = Products;
        }


        public static string MakeSearchCacheKey(IProductDataCache productContext, string Query)
        {
            return string.Format("Search:{0}:{1}", productContext.Identity, Query.GetMD5HashCode());
        }

        public void Insert()
        {
            lock (lockObj)
            {
                HttpRuntime.Cache.Insert(Key, this, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(CacheTimeSeconds), CacheItemPriority.BelowNormal, /* ItemRemovedCallback */ null);
            }
        }

        public static CachedSearchResult Lookup(IProductDataCache productContext, string Query)
        {
            var key = MakeSearchCacheKey(productContext, Query);
            var data = HttpRuntime.Cache[key] as CachedSearchResult;

            if (data == null || data.ProductDataIdentity != productContext.Identity)
                return null;

            return data;
        }


        public static CachedSearchResult Lookup(IProductDataCache productContext, SearchCriteria searchCriteria)
        {
            var json = JsonConvert.SerializeObject(searchCriteria, fmt);

            var key = MakeSearchCacheKey(productContext, json);
            var data = HttpRuntime.Cache[key] as CachedSearchResult;

            if (data == null || data.ProductDataIdentity != productContext.Identity)
                return null;

            return data;
        }

        public static CachedSearchResult Lookup(IProductDataCache productContext, FacetSearchCriteria searchCriteria)
        {
            var json = JsonConvert.SerializeObject(searchCriteria, fmt);

            var key = MakeSearchCacheKey(productContext, json);
            var data = HttpRuntime.Cache[key] as CachedSearchResult;

            if (data == null || data.ProductDataIdentity != productContext.Identity)
                return null;

            return data;
        }


        protected void ItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            Debug.WriteLine(string.Format("Releasing ASP.NET cache object: {0}", key));
        }


    }
}
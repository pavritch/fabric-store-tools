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
    public class CachedBooksByManufacturerResult : CachedProductCollectionsByManufacturerResult
    {
        private const string KeyPrefix = "CachedBooksByManufacturerResult";

        public CachedBooksByManufacturerResult(StoreKeys storeKey, int manufacturerID, List<ProductCollection> collections)
            : base(storeKey, manufacturerID, collections, KeyPrefix)
        {
        }

        public static CachedBooksByManufacturerResult Lookup(StoreKeys storeKey, int manufacturerID)
        {
            var key = MakeCacheKey(storeKey, manufacturerID, KeyPrefix);
            var data = HttpRuntime.Cache[key] as CachedBooksByManufacturerResult;
            return data;
        }
    }
}
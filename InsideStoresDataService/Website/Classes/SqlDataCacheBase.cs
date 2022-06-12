//------------------------------------------------------------------------------
// 
// Class: SqlDataCacheBase 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using System.Web.Caching;

namespace Website
{
    /// <summary>
    /// General base class used for the ASP.NET cache.
    /// </summary>
    public abstract class SqlDataCacheBase
    {
        protected object lockObject = new object();
        protected Cache AspNetCache;
        
        private static readonly Random _randomGenerator = new Random();
        
        /// <summary>
        /// A random number of seconds less than 60.
        /// </summary>
        /// <remarks>
        /// Intended to facilitate adding some wiggle room in the refresh time,
        /// so all the items which may be started at the same time will not 
        /// refresh at exactly the same time.
        /// </remarks>
        public static int RandomSeconds
        {
            get { return _randomGenerator.Next(59);}            
        }
        
        /// <summary>
        /// The key to use for the ASP.NET cache.
        /// </summary>
        /// <remarks>
        /// Set to a GUID by ctor. Callers really never need to know what the value is since
        /// they just reference this property.
        /// </remarks>
        public string CacheKey {get;set;}
        
        /// <summary>
        /// The amount of seconds until cache expires, or int.MaxValue if not meaningful.
        /// </summary>
        /// <remarks>
        /// If int.MaxValue, converted to Cache.NoAbsoluteExpiration when configuring the cache object.
        /// </remarks>
        public int RefreshSeconds {get;set;} 
        
        /// <summary>
        /// The timespan for the sliding expiration, else Cache.NoSlidingExpiration if not meaningful.
        /// </summary>
        public TimeSpan SlidingExpiration {get;set;}

        protected SqlDataCacheBase(int RefreshSeconds)
        : this(RefreshSeconds, Cache.NoSlidingExpiration)
        {
        }

        protected SqlDataCacheBase(TimeSpan SlidingExpiration)
        : this(int.MaxValue, SlidingExpiration)
        {
        }

        protected SqlDataCacheBase(int RefreshSeconds, TimeSpan SlidingExpiration)
        {
            this.CacheKey = Guid.NewGuid().ToString();
            this.RefreshSeconds = RefreshSeconds;
            this.SlidingExpiration = SlidingExpiration;
            AspNetCache = HttpRuntime.Cache;

            if (AspNetCache == null)
                throw new Exception("Class must be instantiated when there is a valid HttpContext.Current.");
        }

        protected DateTime AbsoluteExpiration
        {
            get
            {
                if (RefreshSeconds == int.MaxValue)
                    return Cache.NoAbsoluteExpiration;

                return DateTime.Now.AddSeconds(RefreshSeconds);
            }
        }
        
        protected abstract object GetData();
        
        public virtual void Refresh()
        {
            var data = GetData();

            lock (lockObject)
            {
                AspNetCache.Insert(CacheKey, data, null, AbsoluteExpiration, SlidingExpiration, CacheExpirationCallback);
            }
        }
        
        public void Remove()
        {
            lock (lockObject)
            {
                AspNetCache.Remove(CacheKey);
            }
        }
        
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
        protected virtual void CacheExpirationCallback(
                string key,
                CacheItemUpdateReason reason,
                out Object expensiveObject,
                out CacheDependency dependency,
                out DateTime absoluteExpiration,
                out TimeSpan slidingExpiration)

        {
            try
            {
                expensiveObject = GetData();
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
        }        
        
    }
}
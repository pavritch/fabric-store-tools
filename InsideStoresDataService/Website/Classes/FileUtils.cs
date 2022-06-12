using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Website
{
    public static class FileUtils
    {
        /// <summary>
        /// Reads and caches binary file from filepath 
        /// </summary>
        /// <param name="sFilepath"></param>
        /// <returns></returns>
        public static Byte[] ReadInBinaryFileWithCache(string sFilepath)
        {
            Byte[] aryContents;

            // cache the contents to make future reads faster, set file dependency on cache

            try
            {
                var cache = HttpRuntime.Cache;

                string CacheKey = sFilepath.ToUpper(); // form consistent cache key

                if ((aryContents = (Byte[])cache[CacheKey]) == null)
                {
                    using (FileStream oFile = new FileStream(sFilepath, FileMode.Open))
                    {
                        // read the entire file
                        aryContents = new byte[oFile.Length];
                        oFile.Read(aryContents, 0, aryContents.Length);
                    }

                    // cache it	
                    if (aryContents.Length > 0)
                        cache.Insert(CacheKey, aryContents, new CacheDependency(sFilepath));
                }
            }
            catch
            {
                aryContents = new byte[0];
            }

            return aryContents;
        }


        public static string ReadTextFileWithCache(string sFilepath)
        {
            // cache contents on file read to reduce latency, then add a cache file dependency

            // this cache key is same as one on alternate GFC page			
            string CacheKey = sFilepath.ToUpper(); // salt added to ensure unique
            string sResult;
            var cache = HttpRuntime.Cache;

            if ((sResult = (string)cache[CacheKey]) == null)
            {
                using (StreamReader objStreamReader = File.OpenText(sFilepath))
                    sResult = objStreamReader.ReadToEnd();

                if (sResult.Length > 0)
                    cache.Insert(CacheKey, sResult, new CacheDependency(sFilepath));
            }
            return sResult;
        }    
    }
}

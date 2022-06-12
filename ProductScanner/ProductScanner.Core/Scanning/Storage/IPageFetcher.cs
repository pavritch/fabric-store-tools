using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ProductScanner.Core.Scanning.Storage
{
    public interface IPageFetcher<T> where T : Vendor
    {
        Task<HtmlNode> FetchAsync(string url, CacheFolder cacheFolder, string filename, NameValueCollection values = null, NameValueCollection headers = null, Func<HtmlNode, bool> pageValidator = null);
        Task<HtmlNode> FetchAsync(string url, CacheFolder cacheFolder, string filenameNoExtension, string postData, NameValueCollection headers = null);
        void DisableCaching();
        Task<List<string>> FetchFtpAsync(string url, CacheFolder cacheFolder, string filenameNoExtension, NetworkCredential credentials);
        void RemoveCachedFile(CacheFolder cacheFolder, string filename);
        Task<byte[]> FetchBinaryAsync(string url);
        void EnableCaching();
    }

    public enum CacheFolder
    {
        Search,
        Details,
        Price,
        Files,
        Stock,
        Images,
        Colorways,
    }

    public enum CacheFileType
    {
        Html,
        Txt,
        Xls
    }
}
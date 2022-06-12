using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

namespace ProductScanner.Core.WebClient
{
    public interface IWebClientEx
    {
        WebHeaderCollection Headers { get; }
        WebHeaderCollection ResponseHeaders { get; }
        CookieContainer Cookies { get; }
        Uri ResponseUri { get; }
        bool Allow404s { get; set; }

        Task<string> DownloadStringTaskAsync(string url);
        Task<string> PostValuesTaskAsync(string url, NameValueCollection postValues);
        Task<string> PostDataAsync(string signInPageUrl, byte[] payload);
        Task<byte[]> DownloadDataTaskAsync(string url);
        void DisableAutoRedirect();
    }
}
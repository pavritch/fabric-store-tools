using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.WebClient;
using Utilities;

namespace ProductScanner.Core.Scanning.Storage
{
    public class PageFetcher<T> : IPageFetcher<T> where T : Vendor, new()
    {
        private readonly IWebClientFactory _webClientFactory;
        private readonly IFtpClient _ftpClient;
        private readonly IVendorScanSessionManager<T> _sessionManager;
        private readonly IStorageProvider<T> _storageProvider;
        private bool _cachingEnabled = true;

        public PageFetcher(IWebClientFactory webClientFactory, IVendorScanSessionManager<T> sessionManager, IStorageProvider<T> storageProvider, IFtpClient ftpClient)
        {
            _webClientFactory = webClientFactory;
            _storageProvider = storageProvider;
            _sessionManager = sessionManager;
            _ftpClient = ftpClient;
        }

        public async Task<HtmlNode> FetchAsync(string url, CacheFolder cacheFolder, string filenameNoExtension, 
            NameValueCollection values = null, NameValueCollection headers = null, Func<HtmlNode, bool> pageValidator = null)
        {
            filenameNoExtension = filenameNoExtension.Replace(".html", "");
            if (_cachingEnabled && _storageProvider.HasFile(cacheFolder, filenameNoExtension, CacheFileType.Html))
                return _storageProvider.GetHtmlFile(cacheFolder, filenameNoExtension);

            var cookies = _sessionManager.GetCookies();
            var webClient = _webClientFactory.Create(cookies, headers);

            var numTries = 0;
            HtmlNode downloadedPage = null;
            do
            {
                numTries++;
                try
                {
                    _sessionManager.BumpVendorRequest();
                    downloadedPage = await webClient.DownloadPageAsync(url, values);

                    var throttle = await _sessionManager.GetThrottleAsync();
                    await Task.Delay(throttle);
                }
                catch (PageDownloadException e)
                {
                    downloadedPage = e.ReceivedPage;
                }
                if (numTries > 3) throw new Exception();
            } while (pageValidator != null && !pageValidator(downloadedPage));

            if (_cachingEnabled) _storageProvider.SaveFile(cacheFolder, filenameNoExtension, downloadedPage);
            return downloadedPage;
        }

        public async Task<HtmlNode> FetchAsync(string url, CacheFolder cacheFolder, string filenameNoExtension, string postData,
            NameValueCollection headers = null)
        {
            if (_cachingEnabled && _storageProvider.HasFile(cacheFolder, filenameNoExtension, CacheFileType.Html))
                return _storageProvider.GetHtmlFile(cacheFolder, filenameNoExtension);

            var cookies = _sessionManager.GetCookies();
            var webClient = _webClientFactory.Create(cookies, headers);

            HtmlNode downloadedPage = null;
            try
            {
                _sessionManager.BumpVendorRequest();
                downloadedPage = await webClient.PostDataAsync(url, postData);

                var throttle = await _sessionManager.GetThrottleAsync();
                await Task.Delay(throttle);
            }
            catch (PageDownloadException e)
            {
                downloadedPage = e.ReceivedPage;
            }

            if (_cachingEnabled) _storageProvider.SaveFile(cacheFolder, filenameNoExtension, downloadedPage);
            return downloadedPage;
        }

        public async Task<List<string>> FetchFtpAsync(string url, CacheFolder cacheFolder, string filenameNoExtension, NetworkCredential credentials)
        {
            if (_cachingEnabled && _storageProvider.HasFile(cacheFolder, filenameNoExtension, CacheFileType.Txt))
                return _storageProvider.GetStringFile(cacheFolder, filenameNoExtension).Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var fileList = await _ftpClient.DirectoryListingAsync(credentials, url);
            _sessionManager.BumpVendorRequest();

            if (_cachingEnabled)
            {
                if (fileList.Any()) _storageProvider.SaveFile(cacheFolder, filenameNoExtension, fileList.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return fileList;
        }

        public async Task<byte[]> FetchBinaryAsync(string url)
        {
            try
            {
                var cookies = _sessionManager.GetCookies();
                var webClient = _webClientFactory.Create(cookies);
                return await webClient.DownloadDataTaskAsync(url);
            }
            catch (Exception)
            {
                throw new Exception("URL Not found:" + url);
            }
        }

        public void RemoveCachedFile(CacheFolder cacheFolder, string filename)
        {
            _storageProvider.RemoveFile(cacheFolder, filename);
        }

        public void DisableCaching()
        {
            _cachingEnabled = false;
        }

        public void EnableCaching()
        {
            _cachingEnabled = true;
        }
    }
}
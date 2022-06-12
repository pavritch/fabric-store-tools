using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core.Config;

namespace ProductScanner.Core.WebClient
{
    public class WebClientLoggingDecorator : IWebClientEx
    {
        private readonly IWebClientEx _webClientEx;
        private readonly IAppSettings _appSettings;
        public WebClientLoggingDecorator(IAppSettings appSettings, IWebClientEx webClientEx)
        {
            _webClientEx = webClientEx;
            _appSettings = appSettings;
        }

        private void WriteFile(string id, string result)
        {
            if (!_appSettings.LogExternalResponsesToDisk) return;

            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "responses\\");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(directory + id + ".html", result);
        }

        public async Task<string> DownloadStringTaskAsync(string url)
        {
            var id = GetId();

            var result = await _webClientEx.DownloadStringTaskAsync(url);
            WriteFile(id, result);
            return result;
        }

        public async Task<string> PostValuesTaskAsync(string url, NameValueCollection postValues)
        {
            var id = GetId();

            var result = await _webClientEx.PostValuesTaskAsync(url, postValues);
            WriteFile(id, result);
            return result;
        }

        public async Task<string> PostDataAsync(string url, byte[] payload)
        {
            var id = GetId();
            var result = await _webClientEx.PostDataAsync(url, payload);
            WriteFile(id, result);
            return result;
        }

        private string GetId()
        {
            return string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
        }

        public WebHeaderCollection Headers { get { return _webClientEx.Headers; } }
        public WebHeaderCollection ResponseHeaders { get { return _webClientEx.ResponseHeaders; } }
        public CookieContainer Cookies { get { return _webClientEx.Cookies; } }
        public Uri ResponseUri { get { return _webClientEx.ResponseUri; } }
        public void DisableAutoRedirect() { _webClientEx.DisableAutoRedirect(); }

        public Task<byte[]> DownloadDataTaskAsync(string url)
        {
            throw new NotImplementedException();
        }

        public bool Allow404s
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
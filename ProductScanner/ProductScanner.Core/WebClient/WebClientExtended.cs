using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace ProductScanner.Core.WebClient
{
    public static class GZipCompressor
    {
        public static void Compress(Stream destinationStream, byte[] bytesToWrite)
        {
            if (destinationStream == null)
            {
                throw new ArgumentException();
            }

            if (bytesToWrite == null)
            {
                throw new ArgumentException();
            }

            if (!destinationStream.CanWrite)
            {
                throw new ArgumentException();
            }

            using (GZipStream gzipStream = new GZipStream(destinationStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytesToWrite, 0, bytesToWrite.Length);
            }
        }

        public static byte[] Decompress(Stream sourceStream)
        {
            if (sourceStream == null)
            {
                throw new ArgumentException();
            }

            if (!sourceStream.CanRead)
            {
                throw new ArgumentException();
            }

            MemoryStream memoryStream = new MemoryStream();
            const int bufferSize = 65536;

            using (GZipStream gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[bufferSize];

                int bytesRead = 0;

                do
                {
                    bytesRead = gzipStream.Read(buffer, 0, bufferSize);
                }
                while (bytesRead == bufferSize);

                memoryStream.Write(buffer, 0, bytesRead);
            }

            return memoryStream.ToArray();
        }
    }

    public class WebClientExtended : System.Net.WebClient, IWebClientEx
    {
        /// <summary>
        /// Optional handler to allow caller to adjust request just before being sent.
        /// </summary>
        private Action<HttpWebRequest> webRequestCallback = null;

        // "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private const string DefaultAcceptHeader = "*/*";

        private readonly CookieContainer m_cookieContainer = new CookieContainer();

        public string CustomAcceptHeader { get; set; }
        public bool IsCompressionEnabled { get; set; }
        public int? TimeoutSeconds { get; set; }
        public Uri ResponseUri { get; set; }
        public bool AutoRedirect { get; set; }
        public bool Allow404s { get; set; }
        public Guid Id { get; set; }

        /// <summary>
        /// Optionally set a handler to allow caller to adjust request just before being sent.
        /// </summary>
        /// <param name="callback"></param>
        public void SetWebRequestCallback(Action<HttpWebRequest> callback)
        {
            webRequestCallback = callback;
        }

        public static IWebClientEx WithCookies(CookieCollection cookies)
        {
            var webClient = new WebClientExtended();
            webClient.Cookies.Add(cookies);
            return webClient;
        }

        public WebClientExtended()
        {
            Id = Guid.NewGuid();
            AutoRedirect = true;
            Allow404s = false;
        }

        public void DisableAutoRedirect()
        {
            AutoRedirect = false;
        }

        public CookieContainer Cookies
        {
            get { return m_cookieContainer; }
        }

        public async Task<string> PostDataAsync(string signInPageUrl, byte[] payload)
        {
            byte[] aryResponse = await UploadDataTaskAsync(signInPageUrl, payload);
            string strResponse;

            // Content-Encoding: gzip
            if (ResponseHeaders.AllKeys.Contains("Content-Encoding"))
            {
                if (string.Equals(ResponseHeaders["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
                    aryResponse = aryResponse.Decompress();
            }
            strResponse = Encoding.UTF8.GetString(aryResponse);
            return strResponse;
        }

        public string PostData(string signInPageUrl, byte[] payload)
        {
            byte[] aryResponse = UploadData(signInPageUrl, payload);
            string strResponse;

            // Content-Encoding: gzip
            if (ResponseHeaders.AllKeys.Contains("Content-Encoding"))
            {
                if (string.Equals(ResponseHeaders["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
                    aryResponse = aryResponse.Decompress();
            }
            strResponse = Encoding.UTF8.GetString(aryResponse);
            return strResponse;
        }

        public async Task<string> PostValuesTaskAsync(string url, NameValueCollection nvCol)
        {
            string strResponse;
            byte[] aryResponse;
            try
            {
                aryResponse = await UploadValuesTaskAsync(new Uri(url), nvCol);
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;
                if (response != null && Allow404s && response.StatusCode == HttpStatusCode.NotFound)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                throw;
            }

            // Content-Encoding: gzip
            if (ResponseHeaders.AllKeys.Contains("Content-Encoding"))
            {
                if (string.Equals(ResponseHeaders["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
                {
                    aryResponse = aryResponse.Decompress();
                }
            }

            strResponse = Encoding.UTF8.GetString(aryResponse);
            return strResponse;
        }

        public string PostValues(string signInPageUrl, NameValueCollection nvCol)
        {
            byte[] aryResponse = base.UploadValues(signInPageUrl, nvCol);
            string strResponse = null;

            // Content-Encoding: gzip
            if (ResponseHeaders.AllKeys.Contains("Content-Encoding"))
            {
                if (string.Equals(ResponseHeaders["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
                    aryResponse = aryResponse.Decompress();
            }

            strResponse = Encoding.UTF8.GetString(aryResponse);

            return strResponse;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                var r = request as HttpWebRequest;
                r.CookieContainer = m_cookieContainer;
                r.MaximumResponseHeadersLength = -1;

                r.AllowAutoRedirect = AutoRedirect;

                // 10/8/2013 Changed to latest chrome user agent
                // Previous user agent: Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15
                r.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";

                //r.Accept = string.IsNullOrEmpty(CustomAcceptHeader) ? DefaultAcceptHeader : CustomAcceptHeader;

                // only kind of compression supported is GZIP.
                if (IsCompressionEnabled && !r.Headers.AllKeys.Contains("Accept-Encoding"))
                    r.Headers.Add("Accept-Encoding", "gzip;q=1.0, identity; q=0.5, *;q=0");

                if (webRequestCallback != null)
                    webRequestCallback(r);

                if (TimeoutSeconds.HasValue)
                    request.Timeout = TimeoutSeconds.Value * 1000;
            }

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                var resp = base.GetWebResponse(request);
                if (resp != null)
                {
                    ResponseUri = resp.ResponseUri;
                    var cookie = resp.Headers[HttpResponseHeader.SetCookie];
                    if (cookie != null) m_cookieContainer.SetCookies(ResponseUri, cookie);
                }
                return resp;
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null && Allow404s && response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ex.Response;
                }
                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Website
{

    public class WebClientEx : WebClient
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

        public WebClientEx(CookieContainer cc)
        {
            m_cookieContainer = cc;
        }

        /// <summary>
        /// Optionally set a handler to allow caller to adjust request just before being sent.
        /// </summary>
        /// <param name="callback"></param>
        public void SetWebRequestCallback(Action<HttpWebRequest> callback)
        {
            webRequestCallback = callback;
        }

        public WebClientEx()
        {
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

        public string PostData(string signInPageUrl, byte[] payload)
        {
            byte[] aryResponse = base.UploadData(signInPageUrl, payload);
            string strResponse = null;

            // Content-Encoding: gzip
            if (ResponseHeaders.AllKeys.Contains("Content-Encoding"))
            {
                if (string.Equals(ResponseHeaders["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
                    aryResponse = aryResponse.Decompress();
            }
            
            strResponse = System.Text.Encoding.UTF8.GetString(aryResponse);

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
            
            strResponse = System.Text.Encoding.UTF8.GetString(aryResponse);

            return strResponse;
        }

        protected override WebRequest GetWebRequest(Uri address)
        { 
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                var r = request as HttpWebRequest;
                r.CookieContainer = m_cookieContainer;

                r.AllowAutoRedirect = AutoRedirect;

                // 10/8/2013 Changed to latest chrome user agent
                // Previous user agent: Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15
                r.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"; 

                r.Accept = string.IsNullOrEmpty(CustomAcceptHeader) ? DefaultAcceptHeader : CustomAcceptHeader;

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

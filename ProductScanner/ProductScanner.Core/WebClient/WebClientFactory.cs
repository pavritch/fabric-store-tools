using System.Collections.Specialized;
using System.Net;

namespace ProductScanner.Core.WebClient
{
    public class WebClientFactory : IWebClientFactory
    {
        public IWebClientEx Create(CookieCollection cookies, NameValueCollection headers = null)
        {
            if (cookies == null) return Create();

            var webClient = new WebClientExtended();
            webClient.Cookies.Add(cookies);
            if (headers != null) webClient.Headers.Add(headers);
            return webClient;
        }

        public IWebClientEx Create()
        {
            return new WebClientExtended();
        }
    }
}
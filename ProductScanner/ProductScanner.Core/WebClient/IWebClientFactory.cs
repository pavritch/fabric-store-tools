using System.Collections.Specialized;
using System.Net;

namespace ProductScanner.Core.WebClient
{
    public interface IWebClientFactory
    {
        IWebClientEx Create(CookieCollection cookies, NameValueCollection headers = null);
    }
}
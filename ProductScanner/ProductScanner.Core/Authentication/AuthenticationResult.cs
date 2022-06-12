using System.Net;

namespace ProductScanner.Core.Authentication
{
    // class used to return result of authentication attempt against a vendor
    public class AuthenticationResult
    {
        public bool IsSuccessful { get; set; }
        public CookieCollection Cookies { get; set; }
        public string LoginUrl { get; set; }

        // sometimes the login url contains generated session info, so we need to save it
        public AuthenticationResult(bool isSuccessful, CookieCollection cookies = null, string loginUrl = null)
        {
            IsSuccessful = isSuccessful;
            if (isSuccessful) Cookies = cookies;
            LoginUrl = loginUrl;
        }
    }
}
using System.Net;
using System.Threading.Tasks;

namespace ProductScanner.Core.Authentication
{
    // default authenticator implementation for when a vendor doesn't need to authenticate
    public class NullVendorAuthenticator<T> : IVendorAuthenticator<T> where T : Vendor
    {
        public Task<AuthenticationResult> LoginAsync()
        {
            return Task.FromResult(new AuthenticationResult(true, new CookieCollection()));
        }
    }
}
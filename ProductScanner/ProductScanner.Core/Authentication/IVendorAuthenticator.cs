using System.Threading.Tasks;

namespace ProductScanner.Core.Authentication
{
    public interface IVendorAuthenticator
    {
        Task<AuthenticationResult> LoginAsync();
    }

    public interface IVendorAuthenticator<in T> : IVendorAuthenticator where T : Vendor
    {
    }
}
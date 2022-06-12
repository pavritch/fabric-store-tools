using System.Net;
using System.Threading.Tasks;

namespace ProductScanner.Core.VendorTesting.TestTypes
{
    public interface IVendorTest
    {
        Task<TestResult> RunAsync(CookieCollection cookies);
        string GetDescription();
    }

    public interface IVendorTest<T> : IVendorTest where T : Vendor
    {
    }
}
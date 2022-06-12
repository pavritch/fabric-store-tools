using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace Chella.Tests
{
    // Check 404s
    //public class ChellaSearchUrlTest : ValidUrlTest<ChellaVendor>
    //{
    //    public ChellaSearchUrlTest()
    //        : base("http://www.chellatextiles.com/searchFrame.php") { }
    //}

    public class ChellaProductScrapeTest : ProductScrapeTest<ChellaVendor>
    {
        public ChellaProductScrapeTest(IProductScraper<ChellaVendor> productScraper) : base(productScraper) { }
        public override async Task<TestResult> RunAsync(CookieCollection cookies)
        {
            var discoveredProduct = new DiscoveredProduct("303-66");
            var result = await ProductScraper.ScrapeAsync(discoveredProduct);

            if (result.Count == 1) return new TestResult(TestResultCode.Successful);
            return new TestResult(TestResultCode.Failed);
        }
    }
}

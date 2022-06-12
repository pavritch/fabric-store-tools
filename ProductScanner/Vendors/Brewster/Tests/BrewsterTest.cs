using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace Brewster.Tests
{
    //public class BrewsterFtpTest : ValidUrlTest<BrewsterVendor>
    //{
        //public BrewsterFtpTest() : base("ftp://dealers:Brewster%231@ftpimages.brewsterhomefashions.com/") { }
    //}

    /*
    public class BrewsterProductScrapeTest : ProductScrapeTest<BrewsterVendor>
    {
        private readonly IVendorScanSessionManager<BrewsterVendor> _sessionManager;

        public BrewsterProductScrapeTest(IProductScraper<BrewsterVendor> productScraper,
            IVendorScanSessionManager<BrewsterVendor> sessionManager)
            : base(productScraper)
        {
            _sessionManager = sessionManager;
        }

        public async override Task<TestResult> RunAsync(CookieCollection cookies)
        {
            await _sessionManager.AuthAsync();

            var product = new DiscoveredProduct("299-6339");
            var result = await ProductScraper.ScrapeAsync(product);

            if (result.Count == 1) return new TestResult(TestResultCode.Successful, "Product Scraped Succesfully");
            return new TestResult(TestResultCode.Failed);
        }
    }
    */
}

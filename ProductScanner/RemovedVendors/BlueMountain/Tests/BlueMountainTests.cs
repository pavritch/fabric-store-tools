using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace BlueMountain.Tests
{
    public class BlueMountainSitemapUrlTest : ValidUrlTest<BlueMountainVendor>
    {
        public BlueMountainSitemapUrlTest() : base("http://www.designbycolor.net/sitemap.xml") { }
    }

    public class BlueMountainProductScrapeTest : ProductScrapeTest<BlueMountainVendor>
    {
        private readonly IVendorScanSessionManager<BlueMountainVendor> _sessionManager;

        public BlueMountainProductScrapeTest(IProductScraper<BlueMountainVendor> productScraper,
            IVendorScanSessionManager<BlueMountainVendor> sessionManager)
            : base(productScraper)
        {
            _sessionManager = sessionManager;
        }

        public async override Task<TestResult> RunAsync(CookieCollection cookies)
        {
            await _sessionManager.StartAsync();

            var product = new DiscoveredProduct("BC1580701");
            var result = await ProductScraper.ScrapeAsync(product);

            if (result.Count == 1) return new TestResult(TestResultCode.Successful, "Product Scraped Succesfully");
            return new TestResult(TestResultCode.Failed);
        }
    }
}

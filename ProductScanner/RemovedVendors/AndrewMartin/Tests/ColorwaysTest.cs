using System;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace AndrewMartin.Tests
{
    public class ColorwaysTest : ProductScrapeTest<AndrewMartinVendor>
    {
        public ColorwaysTest(IProductScraper<AndrewMartinVendor> productScraper) : base(productScraper) { }
        public override async Task<TestResult> RunAsync(CookieCollection cookies)
        {
            var discoveredProduct = new DiscoveredProduct(new Uri("http://www.andrewmartin.co.uk/pelham-fabric.php"));
            var products = await ProductScraper.ScrapeAsync(discoveredProduct);

            if (products.Count == 29) return TestResult.Success("Found 29 colorways");
            return TestResult.Failed("Expected 29 colorways but found {0}", products.Count);
        }
    }
}

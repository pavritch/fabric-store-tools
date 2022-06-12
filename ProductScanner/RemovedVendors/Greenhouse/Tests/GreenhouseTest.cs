using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using Greenhouse.Discovery;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Greenhouse.Tests
{

    public class GreenhouseDiscoveryTest : VendorTest<GreenhouseVendor>
    {
        public GreenhouseDiscoveryTest(IWebClientFactory webClientFactory) : base(webClientFactory)
        {
        }

        public async override Task<TestResult> RunAsync(CookieCollection cookies)
        {
            var url = "https://www.greenhousefabrics.com/fabrics/ajax?start=0";
            var webClient = _webClientFactory.Create(cookies);
            var doc = await webClient.DownloadPageAsync(url);

            var deserialized = JsonConvert.DeserializeObject<GreenhouseJSONObject>(doc.InnerText);
            if (deserialized.Fabrics == string.Empty)
                return new TestResult(TestResultCode.Failed, "Could not parse discovered JSON");

            var page = new HtmlDocument();
            page.LoadHtml(deserialized.Fabrics);

            var root = page.DocumentNode;
            var fabrics = root.QuerySelectorAll("a");
            var ct = fabrics.Select(x => x.Attributes["href"].Value.CaptureWithinMatchedPattern("/fabric/(?<capture>(.*))$")).Distinct().Count();
            if (ct >= 90) return new TestResult(TestResultCode.Successful);
            return new TestResult(TestResultCode.Failed, "Discovered JSON Count Incorrect");
        }

        public override string GetDescription()
        {
            return "Checking JSON Endpoint Product Count";
        }
    }


    /*public class GreenhouseScrapeTest : ProductScrapeTest<GreenhouseVendor>
    {
        public GreenhouseScrapeTest(IProductScraper<GreenhouseVendor> productScraper) : base(productScraper) { }

        public async override Task<TestResult> RunAsync(CookieCollection cookies)
        {
            var discoveredProduct = new DiscoveredProduct("a7893-sage");
            var result = await ProductScraper.ScrapeAsync(discoveredProduct);

            if (result.Count == 1) return new TestResult(TestResultCode.Successful, "Scraped Product Successfully");
            return new TestResult(TestResultCode.Failed);
        }
    }*/
}

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Details;

namespace ProductScanner.Core.VendorTesting.TestTypes
{
    public abstract class ProductScrapeTest<T> : IVendorTest<T> where T : Vendor
    {
        protected readonly IProductScraper<T> ProductScraper;
        protected ProductScrapeTest(IProductScraper<T> productScraper)
        {
            ProductScraper = productScraper;

            // never want to use the cache for a test
            ProductScraper.DisableCaching();
        }
        public abstract Task<TestResult> RunAsync(CookieCollection cookies);
        public string GetDescription()
        {
            return "Product Scrape Test";
        }
    }

    public class ValidUrlTest<T> : IVendorTest<T> where T : Vendor
    {
        private readonly string _url;
        public ValidUrlTest(string url)
        {
            _url = url;
        }

        public async Task<TestResult> RunAsync(CookieCollection cookies)
        {
            var container = new CookieContainer();
            container.Add(cookies);
            try
            {
                using (var handler = new HttpClientHandler { CookieContainer = container })
                using (var client = new HttpClient(handler))
                {
                    var result = await client.GetAsync(_url);
                    if (result.StatusCode == HttpStatusCode.OK)
                        return new TestResult(TestResultCode.Successful, "URL Found ");
                    return new TestResult(TestResultCode.Failed, string.Format("Invalid URL ({0})", result.StatusCode));
                }
            }
            catch (Exception)
            {
                return new TestResult(TestResultCode.Failed);
            }
        }

        public string GetDescription()
        {
            return "Checking for Valid URL";
        }
    }
}
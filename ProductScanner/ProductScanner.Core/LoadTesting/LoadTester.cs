using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;

namespace ProductScanner.Core.LoadTesting
{
    // used to exercise stock check web service by firing up threads and submitting stock checks
    public class LoadTester : ILoadTester
    {
        private readonly IRandomRequestGenerator _requestGenerator;
        public LoadTester(IRandomRequestGenerator requestGenerator)
        {
            _requestGenerator = requestGenerator;
        }

        public void RunAgainstAPI(int numThreads, int maxVendorsPerRequest, int maxProductsPerVendor)
        {
            for (int i = 0; i < numThreads; i++)
                Task.Factory.StartNew(() => TestAgainstAPIAsync(maxVendorsPerRequest, maxProductsPerVendor), TaskCreationOptions.LongRunning);
        }

        private async void TestAgainstAPIAsync(int maxVendorsPerRequest, int maxProductsPerVendor)
        {
            var endpoint = ConfigurationManager.AppSettings["StockCheckEndpoint"];
            while (true)
            {
                var stockChecks = await _requestGenerator.GenerateAsync(maxVendorsPerRequest, maxProductsPerVendor);
                var httpClient = new HttpClient();
                try
                {
                    var res = await httpClient.PostAsJsonAsync(endpoint, stockChecks);
                    var resp = await res.Content.ReadAsStringAsync();
                    var results = resp.JSONtoList<List<StockCheckResult>>();
                    if (results.Any())
                    {
                        Console.WriteLine("{0} => {1}",
                            stockChecks.Select(x => x.VariantId).Select(x => x.ToString()).Aggregate((a, b) => a + "," + b),
                            results.Select(x => x.StockCheckStatus.ToString()).Aggregate((a, b) => a + "," + b));
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
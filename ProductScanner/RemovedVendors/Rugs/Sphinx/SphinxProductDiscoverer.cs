using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Sphinx
{
    public class SphinxProductDiscoverer : IProductDiscoverer<SphinxVendor>
    {
        private readonly IPageFetcher<SphinxVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<SphinxVendor> _sessionManager;

        public SphinxProductDiscoverer(IPageFetcher<SphinxVendor> pageFetcher, IVendorScanSessionManager<SphinxVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var sessionUrl = _sessionManager.GetLoginUrl();
            var listUrl = sessionUrl.Replace("/main.mbr/start", "/upclist.mbr/start");

            var listPage = await _pageFetcher.FetchAsync(listUrl, CacheFolder.Search, "list");
            var rows = listPage.QuerySelectorAll("table tr").Skip(1);
            var products = new List<ScanData>();
            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td").ToList();
                var data = new ScanData();
                data[ScanField.Code] = cells[0].InnerText;
                data[ScanField.Collection] = cells[1].InnerText;
                data[ScanField.Pattern] = cells[2].InnerText;
                data[ScanField.Backing] = cells[3].InnerText;
                data[ScanField.Size] = cells[4].InnerText;
                data[ScanField.Description] = cells[5].InnerText;
                data[ScanField.UPC] = cells[6].InnerText;

                // this is a dupe size (different backing but nothing I can do with that)
                if (data[ScanField.UPC] == "748679418701") continue;
                if (data[ScanField.Size] == "ASSTP") continue;

                if (data[ScanField.Description].Contains("SPECIAL") ||
                    data[ScanField.Description].ContainsIgnoreCase("3 PC") ||
                    data[ScanField.Description].Contains("ROLL") ||
                    data[ScanField.Description].ContainsIgnoreCase("swatch"))
                    continue;
                products.Add(data);
            }
            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}
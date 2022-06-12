using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace RalphLauren.Discovery
{
    public class RalphLaurenProductDiscoverer : IProductDiscoverer<RalphLaurenVendor>
    {
        private const string ProductUrl = "http://customers.folia-fabrics.com/readitem.asp?acct=01022149&action=read&ltype=1";
        private const string OptionalUrl = "http://customers.folia-fabrics.com/";
        private readonly IPageFetcher<RalphLaurenVendor> _pageFetcher;
        public RalphLaurenProductDiscoverer(IPageFetcher<RalphLaurenVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = new List<ScanData>();

            var postValues = new NameValueCollection();
            postValues.Add("acct", "01022149");
            postValues.Add("ltype", "2");
            postValues.Add("ID", "");
            var page = await _pageFetcher.FetchAsync(ProductUrl, CacheFolder.Search, "all-products", postValues);

            // detect that we are not in an authenticated session and abort

            if (page.OuterHtml.ContainsIgnoreCase("Please enter your customer account number and password, then click Login") || 
                page.QuerySelector("input[value='Login']") != null)
                throw new Exception("Authentication problem. Website is prompting for customer account and password.");

            if (page.InnerText.ContainsIgnoreCase("Internal server error"))
                throw new Exception("Server Error.");

            var collectionNodes = page.QuerySelectorAll("table tr").ToList();
            foreach (var collectionNode in collectionNodes)
            {
                var cells = collectionNode.QuerySelectorAll("td").ToArray();
                var mpn = cells[2].InnerText.ToUpper();

                if (mpn == "ITEM NO") // skip first row with headings
                    continue;

                var url = cells[2].QuerySelector("a").Attributes["href"].Value;

                var newProduct = new ScanData();
                newProduct[ScanField.ManufacturerPartNumber] = mpn;
                newProduct.AddImage(new ScannedImage(ImageVariantType.Primary, "http://s7ondemand1.scene7.com/is/image/Polo/" + mpn + "?$enlarge$"));

                var urlFrags = url.Substring(url.IndexOf("?")).ToLower().Split('&');
                for (int f = 0; f < urlFrags.Length; f++)
                {
                    if (urlFrags[f].IndexOf("pattno") > -1)
                        newProduct[ScanField.PatternNumber] = urlFrags[f].Replace("pattno=", "");
                    else if (urlFrags[f].IndexOf("pattco") > -1)
                        newProduct[ScanField.ColorNumber] = urlFrags[f].Replace("pattco=", "");
                }

                newProduct.DetailUrl = new Uri(OptionalUrl + url);
                products.Add(newProduct);
            }
            return products.Select(x => new DiscoveredProduct(x) { DetailUrl = x.DetailUrl}).ToList();
        }
    }
}
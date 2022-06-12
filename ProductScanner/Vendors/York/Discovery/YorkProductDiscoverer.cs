using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace York.Discovery
{
    public class YorkProductDiscoverer : IProductDiscoverer<YorkVendor>
    {
        // used to cycle through the pages of a collection
        private const string PostUrl = "http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WBRAND3+webrtn=BRANDD3+ml=LANSA:XHTML+partition=YWP+language=ENG+sid=";
        private const string HostName = "www.yorkwall.com";

        // These collections cannot be sold here online.
        private static readonly Dictionary<string, string> ForbiddenCollections = new Dictionary<string, string>
        {
            // code logic uses only the KEY here, value is simply for reference
            {"RRD536RRC", "Avenue"},
            {"RRD470RRD", "Brocades & Damasks"},
            {"RRD747RRC", "Designer Portfolio"},
            {"GTD714GTD", "Ginger Tree II"},
            {"AVD839AVD", "Glitterati II"},
            {"CLD583CLD", "Kaleidoscope"},
            {"AVD590AVD", "La Stanza"},
            {"RRD578RRD", "Masters Anniversary Edition"}, 
            {"RRD717RRD", "Middlebury"},
            {"YWD070AMP", "Operetta"},
            {"RRD514RRC", "Park Place"},
            {"TCC731TCC", "Passage East"},
            {"CLD546CLD", "Pure"},
            {"CLD780CLD", "Rhythm & Hues"},
            {"ASH557YWD", "Veranda"},
            {"TCC587TCC", "Vintage Jewel"},
        };

        private readonly IPageFetcher<YorkVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<YorkVendor> _sessionManager;

        public YorkProductDiscoverer(IPageFetcher<YorkVendor> pageFetcher, IVendorScanSessionManager<YorkVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var collectionUrls = await DiscoverCollectionUrls();
            var webProducts = await DiscoverAllProducts(collectionUrls);
            return webProducts.Select(x => new DiscoveredProduct(x) { DetailUrl = x.DetailUrl }).ToList();
        }

        private async Task<List<string>> DiscoverCollectionUrls()
        {
            var pageNode = await _pageFetcher.FetchAsync("http://www.yorkwall.com/static/collections", CacheFolder.Search, "collections-menu");
            var rootNavNode = pageNode.QuerySelector("ul[id='secondaryNav']");
            if (rootNavNode == null)
                throw new Exception("Unable to locate navigation starting point on root product page: collections");

            var primaryNav = rootNavNode.QuerySelector("a[title='Collections']").ParentNode;
            var collectionUrls = primaryNav.SelectNodes("ul/li/a").Select(x => x.Attributes["href"].Value).ToList();
            // the 'token' from the Forbidden list is the last split in the url
            return collectionUrls.Where(x => !ForbiddenCollections.ContainsKey(x.Split('/').Last())).ToList();
        }

        private async Task<List<ScanData>> DiscoverAllProducts(List<string> collectionUrls)
        {
            var products = new List<ScanData>();
            await _sessionManager.ForEachNotifyAsync("Discovering products by collection", collectionUrls, async collectionUrl => 
                products.AddRange(await DiscoverProductsForCollection(collectionUrl)));
            return products;
        }

        private async Task<List<ScanData>> DiscoverProductsForCollection(string collectionUrl)
        {
            var products = new List<ScanData>();
            var collectionToken = collectionUrl.Split('/').Last();
            var firstPage = await _pageFetcher.FetchAsync(collectionUrl, CacheFolder.Search, collectionToken + "-" + 1);
            var pagination = firstPage.QuerySelector(".paginationCurrentPage");
            if (pagination == null) return products;

            var numPages = pagination.QuerySelectorAll("a").Last().InnerText.ToIntegerSafe();
            if (numPages == 0) numPages = 1;
            var currentPage = firstPage;
            await _sessionManager.ForEachNotifyAsync("Finding products for collection " + collectionToken, Enumerable.Range(1, numPages), async pageNum =>
            {
                var nvCol = GetRequiredPostValues(currentPage);
                nvCol.Add("CURRPAGE", pageNum.ToString());
                currentPage = await _pageFetcher.FetchAsync(PostUrl, CacheFolder.Search, collectionToken + "-" + pageNum, nvCol);

                var table = currentPage.QuerySelector("table[class='prdList']");
                var productCells = table.QuerySelectorAll("td[width='230']").ToList();
                products.AddRange(productCells.Select(CreateProduct));
            });

            return products;
        }

        private ScanData CreateProduct(HtmlNode tableCell)
        {
            var vendorProduct = new ScanData();
            var iconImageUrlNode = tableCell.QuerySelector("img[class='prddtlimg']");
            if (iconImageUrlNode != null)
            {
                var onclick = iconImageUrlNode.ParentNode.GetAttributeValue("onclick", string.Empty);
                var detailUrl = onclick.CaptureWithinMatchedPattern(@"window.location.href\s=\s'(?<capture>(.*))';");
                if (!string.IsNullOrWhiteSpace(detailUrl))
                    vendorProduct.DetailUrl = new Uri("http://" + HostName + detailUrl);
            }

            var largeImageUrlNode = tableCell.QuerySelector("a[id='o1_LANSA_12994']");
            if (largeImageUrlNode != null)
            {
                var largeImageUrl = largeImageUrlNode.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrWhiteSpace(largeImageUrl))
                    vendorProduct[ScanField.ImageUrl] = largeImageUrl;
            }

            var firstCellTable = tableCell.SelectSingleNode("table");
            vendorProduct[ScanField.ManufacturerPartNumber] = firstCellTable.SelectSingleNode("tbody/tr/td[2]").InnerText.Trim();
            vendorProduct[ScanField.PatternName] = firstCellTable.QuerySelector("td[class='prdName']").InnerText.Trim();
            vendorProduct[ScanField.Category] = firstCellTable.SelectSingleNode("tbody/tr[2]").InnerText.Trim();
            return vendorProduct;
        }

        private NameValueCollection GetRequiredPostValues(HtmlNode page)
        {
            var nvCol = new NameValueCollection();
            var namedElements = new[]
            {
                "STDRENTRY",
                "STDSESSID",
                "STDWEBUSR",
                "STDWEBC01",
                "STDTABFLR",
                "STDROWNUM",
                "STDUSERST",
                "STDUSRTYP",
                "LW3VARFLD",
                "STDNXTFUN",
                "STDPRVFUN",
                "LW3SITTOT",
                "LW3SITCNT",
                "LW3EASTAT",
                "LW3CUSIND",
                "STDCUSIND",
                "LW3PROCID",
                "LW3VNDNME",
                "STDLISTID",
                "STD_ADLIN",
                "INBRDCOL",
                "IMGSTRPOS",
                "WW3SUBSIT",
                "NEWNBRPAG",
                "CLKNBRPAG",
                "INBRAND",
                "NBRPAG",
                "_SERVICENAME",
                "_WEBAPP",
                "_WEBROUTINE",
                "_PARTITION",
                "_LANGUAGE",
                "_LW3TRCID"
            };

            foreach (var item in page.OwnerDocument.GetFormPostValuesByName(namedElements))
                nvCol.Add(item.Key, item.Value);
            return nvCol;
        }
    }

    /*
    public class YorkProductDiscoverer : IProductDiscoverer<YorkVendor>
    {
        private readonly YorkMasterFileLoader _fileLoader;

        public YorkProductDiscoverer(YorkMasterFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _fileLoader.LoadProducts();
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
    */
}
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;

namespace Momeni
{
    public class MomeniProductScraper : ProductScraper<MomeniVendor>
    {
        // I don't see an easy way to find the direct page from the data that we already have, so we need to search
        private const string SearchUrl = "http://mom-web.momeni.com/b2b_asp/api/ItemSearch/GetSearchByKeyWord";
        private const string StockUrl = "http://mom-web.momeni.com/B2B_ASP/api/ItemSearch/GetItemsDetail";

        public MomeniProductScraper(IPageFetcher<MomeniVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var upc = product.ScanData[ScanField.UPC];
            var values = new Dictionary<string, string>();
            values.Add("Discontinued", "false");
            values.Add("Keywords", product.MPN);
            values.Add("MainCollectionId", "");
            values.Add("isLoadAll", "true");
            values.Add("itemCollectionIds", "");
            values.Add("itemcolorListIds", "");
            values.Add("itemlifeStyleListIds", "");
            values.Add("itemsizeListIds", "");
            values.Add("pageIndex", "1");
            values.Add("pageSize", "30");

            var headers = new NameValueCollection();
            headers.Add("Content-Type", "application/json");

            var searchResults = await PageFetcher.FetchAsync(SearchUrl, CacheFolder.Stock, "search-" + upc, values.ToJSON(), headers);
            var results = JObject.Parse(searchResults.InnerText);
            var matches = results["Data"]["result"]["itemsInfo"];

            var numMatches = matches.ToObject<List<object>>().Count;
            if (numMatches == 0) return new List<ScanData>();

            var match = matches[0];
            var designNo = match["DesignNo"].ToString();
            var collectionId = match["CollectionId"].ToString();

            var itemValues = new Dictionary<string, string>();
            itemValues.Add("designNo", designNo);
            itemValues.Add("itemCollectionIds", collectionId);
            itemValues.Add("itemDesignIds", "");
            itemValues.Add("itemId", product.MPN);
            itemValues.Add("itemcolorListIds", "");
            itemValues.Add("itemlifeStyleListIds", "");
            itemValues.Add("itemsizeListIds", "");
            itemValues.Add("userId", "");
            itemValues.Add("userNo", "0");

            var page = await PageFetcher.FetchAsync(StockUrl, CacheFolder.Stock, upc, itemValues.ToJSON(), headers);
            var stockResult = JObject.Parse(page.InnerText);
            if (stockResult["Data"]["result"].ToObject<object>() == null)
            {
                return new List<ScanData>();
            }
            var stock = stockResult["Data"]["result"]["itemInfo"]["quantity"].ToObject<int>();

            product.ScanData[ScanField.StockCount] = stock.ToString();

            return new List<ScanData> { product.ScanData };
        }
    }
}
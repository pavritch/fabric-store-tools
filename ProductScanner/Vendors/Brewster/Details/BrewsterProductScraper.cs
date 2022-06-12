using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Brewster.Details
{
    public class BrewsterProductScraper : ProductScraper<BrewsterVendor>
    {
        private const string SearchUrl = "https://dealer.brewsterwallcovering.com/Product/Search/?sku={0}";
        private const string QuickAddUrl = "https://dealer.brewsterwallcovering.com/Order/QuickAdd";

        public BrewsterProductScraper(IPageFetcher<BrewsterVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var values = new NameValueCollection();
            values.Add("SearchText", product.MPN);
            values.Add("Search", "Search");

            var stockPage = await PageFetcher.FetchAsync(string.Format(SearchUrl, product.MPN), CacheFolder.Stock, product.MPN, values);
            if (!stockPage.InnerText.ContainsIgnoreCase("6005540"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Stock, product.MPN);
                throw new AuthenticationException();
            }

            if (stockPage.InnerText.ContainsIgnoreCase("Invalid Material Number")) return new List<ScanData>();
            var costElement = stockPage.GetFieldValue(".roomPricing tr:nth-child(3) td:nth-child(2)");
            var stockElement = stockPage.GetFieldValue(".productInventory tr:nth-child(3) td:nth-child(2)");

            var scanData = new ScanData(product.ScanData);
            if (stockPage.InnerText.ContainsIgnoreCase("Discontinued Pattern"))
            {
                scanData.IsDiscontinued = true;
                return new List<ScanData> {scanData};
            }

            scanData[ScanField.OrderInfo] = string.Join(", ", stockPage.QuerySelectorAll("#Order_SampleType option").Select(x => x.NextSibling.InnerText).ToList());

            scanData.Cost = costElement.Replace(" ER", "").Replace(" BDR", "").Replace(" EA", "").ToDecimalSafe();
            scanData[ScanField.StockCount] = stockElement == null ? "0" : stockElement.Replace("+", "");

            //await QueryQuickAdd(product.MPN);
            return new List<ScanData> {scanData};
        }

        // this seems to kind of work, but I eventually get "Your account has an error" or something similar
        private async Task<string> QueryQuickAdd(string mpn)
        {
            var values = new NameValueCollection();
            values.Add("CaseDiscount", "50-15-10");
            values.Add("CasePrice", "26.77");
            values.Add("Collection", "UTOPIA A-ST");
            values.Add("Coverage", "0.00");
            values.Add("Description", "");
            values.Add("Image", mpn);
            values.Add("Length", "0.00");
            values.Add("Match", "Straight");
            values.Add("Material", "Non Woven");
            values.Add("MSRP", "69.99");
            values.Add("Name", "Sakura Turquoise Floral Wallpaper");
            values.Add("Paste", "Unpasted");
            values.Add("PriceCode", "A");
            values.Add("Repeat", "20.9 in  53 cm");
            values.Add("RoomDiscount", "50-15");
            values.Add("RoomPrice", "29.75");
            values.Add("SKU", mpn);

            values.Add("StockItems[0].BatchNumber", "1-0");
            values.Add("StockItems[0].AvailableQuantity", "102.000");
            values.Add("StockItems[0].RandolphAvailable", "102.000");
            values.Add("StockItems[0].LargoAvailable", "0.000");

            values.Add("Width", "0.00");
            values.Add("Order.SampleType", "");
            values.Add("Order.Quantity", "3");
            values.Add("Order.Batch", "");
            values.Add("Order.Comments", "");
            values.Add("QuickAdd", "ADD TO CART");

            var quickAddPage = await PageFetcher.FetchAsync(QuickAddUrl, CacheFolder.Details, mpn, values);

            return string.Empty;
        }
    }
}
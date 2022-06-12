using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace RobertAllen.Details
{
    public class RobertAllenProductScraper : RobertAllenBaseProductScraper<RobertAllenVendor>
    {
        public RobertAllenProductScraper(IPageFetcher<RobertAllenVendor> pageFetcher) : base(pageFetcher) { }
    }

    public class BeaconHillProductScraper : RobertAllenBaseProductScraper<BeaconHillVendor>
    {
        public BeaconHillProductScraper(IPageFetcher<BeaconHillVendor> pageFetcher) : base(pageFetcher) { }
    }

    public class RobertAllenBaseProductScraper<T> : ProductScraper<T> where T : Vendor, new()
    {
        private const string StockUrl = "http://www.robertallendesign.com/catalog/product/getAvailablePieces/";

        private readonly Dictionary<string, ScanField> _attributeKeys = new Dictionary<string, ScanField>
        {
            { "PATTERN", ScanField.PatternName},
            { "SKU", ScanField.Ignore},
            { "WHOLESALE PRICE", ScanField.Ignore},
            { "CONTENTS", ScanField.Content},
            { "WIDTH", ScanField.Width},
            { "REPEAT DIRECTION", ScanField.Direction},
            { "HORIZONTAL REPEAT", ScanField.HorizontalRepeat},
            { "VERTICAL REPEAT", ScanField.VerticalRepeat},
            { "ORIGIN", ScanField.Country},
            { "STATUS", ScanField.Status},
        };

        private readonly Dictionary<string, ScanField> _detailsKeys = new Dictionary<string, ScanField>
        {
            { "PERFORMANCE", ScanField.FabricPerformance},
            { "CLEANING CODE", ScanField.Cleaning},
            { "END USE", ScanField.Use},
            { "BARRIER FINISH", ScanField.Finish},
            { "BRAND", ScanField.Brand},
            { "BOOK", ScanField.Book},
            { "BOOK NUMBER", ScanField.BookNumber},
            { "COLLECTION", ScanField.Collection},
            { "FABRIC GRADE", ScanField.Ignore},
            { "ABRASION", ScanField.Durability},
            { "FINISH", ScanField.Finish},
            { "COLLECTION NUMBER", ScanField.Ignore},
        }; 

        public RobertAllenBaseProductScraper(IPageFetcher<T> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var detailsUrl = context.DetailUrl.AbsoluteUri;
            var mpn = context.MPN;
            var data = new ScanData();
            HtmlNode page;
            try
            {
                page = await PageFetcher.FetchAsync(detailsUrl, CacheFolder.Details, mpn);
            }
            catch (Exception)
            {
                return new List<ScanData>();
            }
            if (page.InnerText.ContainsIgnoreCase("Please call customer service at 800-333-3777 for price, stock availability and lead times"))
                return new List<ScanData>();
            //if (page.InnerText.ContainsIgnoreCase("Please place your order and a customer service representative will contact you"))
                //return new List<ScanData>();
            if (page.InnerText.ContainsIgnoreCase("SIGN IN TO ORDER FABRICS OR CHECK STOCK"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, mpn);
                throw new AuthenticationException();
            }

            if (IsPageValid(page))
            {
                var attributes = page.QuerySelectorAll(".attr-details");
                foreach (var attr in attributes)
                {
                    var label = attr.QuerySelector("span").InnerText.Trim();
                    var relevantNodes = attr.Children().Skip(1).Where(x => x.NodeType == HtmlNodeType.Text).ToList();
                    relevantNodes.AddRange(attr.QuerySelectorAll("span").Skip(1));
                    var value = string.Join(", ", relevantNodes.Select(x => x.InnerText));
                    var property = _attributeKeys[label];
                    data[property] = value;
                }

                var otherDataItems = page.QuerySelectorAll(".details-list .mobile-accordion-item");
                foreach (var otherDataItem in otherDataItems)
                {
                    var title = otherDataItem.GetFieldValue(".mobile-accordion-title");
                    var value = otherDataItem.GetFieldValue(".mobile-accordion-content");
                    var property = _detailsKeys[title];
                    data[property] = value;
                }

                var memoBtn = page.QuerySelector(".order-button-link a:contains('Order Memo')");
                if (memoBtn == null)
                {
                    data.RemoveSwatch = true;
                }
                var priceInfo = page.QuerySelector(".price");
                if (priceInfo == null) return new List<ScanData>();

                data.Cost = page.QuerySelector(".price").InnerText.Replace("$", "").ToDecimalSafe();

                data.DetailUrl = new Uri(detailsUrl);
                var imageNode = page.QuerySelector("#zoom1");
                if (imageNode != null)
                {
                    data.AddImage(new ScannedImage(ImageVariantType.Primary, imageNode.Attributes["href"].Value));
                }

                // this stock call only works for the valid pages
                var values = new NameValueCollection();
                values.Add("sku", mpn);
                var stockData = await PageFetcher.FetchAsync(StockUrl, CacheFolder.Stock, mpn, values);
                if (stockData.InnerHtml == string.Empty)
                {
                    PageFetcher.RemoveCachedFile(CacheFolder.Stock, mpn);
                    // sleep for 5 minutes - this usually happens during their maintenance period
                    Thread.Sleep(1000 * 60 * 5);
                    throw new AuthenticationException();
                }
                var availableStock = (float)stockData.QuerySelectorAll("td").Where((x, i) => i % 4 == 3).Sum(x => x.InnerText.ToDoubleSafe());
                data[ScanField.StockCount] = availableStock.ToString();
            }


            var outletUrl = string.Format("https://www.robertallenoutlet.com/trade/fabric_detail.aspx?product={0}", mpn);
            var outletPage = await PageFetcher.FetchAsync(outletUrl, CacheFolder.Stock, mpn + "-outlet");
            data = ScrapeOutletPage(outletPage, data);
            data[ScanField.ManufacturerPartNumber] = mpn;
            data[ScanField.ProductGroup] = context.ProductGroup;

            return new List<ScanData> {data};
        }

        private bool IsPageValid(HtmlNode page)
        {
            // they recently added redirects on some of the robertallendesign.com pages
            // these pages don't actually have any info
            if (page.InnerHtml.ContainsIgnoreCase("window.location.replace")) return false;

            // some of them redirect to the main page
            // valid pages contain the link back to the home page
            if (page.InnerHtml.ContainsIgnoreCase("Go to Home Page")) return true;

            return false;
        }

        private ScanData ScrapeOutletPage(HtmlNode page, ScanData data)
        {
            var lots = page.QuerySelectorAll("#ctl00_content1_grvAvailableLots td").ToList();
            var outletStock = lots.Where((x, i) => i%2 == 1).Sum(x => x.InnerText.ToDoubleSafe());
            if (data[ScanField.StockCount] == string.Empty)
            {
                data[ScanField.StockCount] = outletStock.ToString();
            }

            var retailPrice = page.GetFieldValue("#ctl00_content1_lblRetailPriceVal");
            if (retailPrice == null) return data;

            var retail = retailPrice.Replace("$", "").Replace("(USD)", "").Trim().ToDoubleSafe();
            data[ScanField.RetailPrice] = retail.ToString();

            data.IsLimitedAvailability = page.InnerText.ContainsIgnoreCase("however limited yardage is available");
            data[ScanField.Railroaded] = page.GetFieldValue("#ctl00_content1_lblRailrd");
            data[ScanField.Ignore] = page.GetFieldValue("#ctl00_content1_divDiscWithStock");
            data[ScanField.Ignore] = page.GetFieldValue("#ctl00_content1_lblDiscountPriceVal");

            if (data[ScanField.PatternName] == string.Empty) data[ScanField.PatternName] = page.GetFieldValue("#ctl00_content1_lblPattern");
            data[ScanField.Width] = page.GetFieldValue("#ctl00_content1_lblWidth");
            data[ScanField.HorizontalRepeat] = page.GetFieldValue("#ctl00_content1_lblHrepeat");
            data[ScanField.VerticalRepeat] = page.GetFieldValue("#ctl00_content1_lblVRepeat");
            data[ScanField.Country] = page.GetFieldValue("#ctl00_content1_lblOrigin");

            if (retail != 0)
                data.Cost = (decimal) (retail/2);

            var selectedColor = page.QuerySelector("#ctl00_content1_ddlAvailableColor").QuerySelector("option[selected='selected']");
            if (selectedColor != null)
                data[ScanField.ColorName] = selectedColor.NextSibling.InnerText.Trim();
            return data;
        }
    }
}
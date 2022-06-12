using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace ClarenceHouse.Details
{
    public class ClarenceHouseProductScraper : ProductScraper<ClarenceHouseVendor>
    {
        private const string PublicSearchUrl = "http://clarencehouse.com/search_results/";

        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            {"Name", ScanField.PatternName},
            {"Color", ScanField.ColorName},
            {"Unit Price", ScanField.Cost},
            {"Stock Available now", ScanField.StockCount},
            {"Fiber Content", ScanField.Content},
            {"Country of Origin", ScanField.Country},
            {"Width", ScanField.Width},
            {"Horizontal Repeat", ScanField.HorizontalRepeat},
            {"Vertical Repeat", ScanField.VerticalRepeat},
            {"World wide Exclusivity", ScanField.IsExclusive},
            {"Comments", ScanField.Note},
            {"Incoming Stock", ScanField.IncomingStock},
        };

        public ClarenceHouseProductScraper(IPageFetcher<ClarenceHouseVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var url = discoveredProduct.DetailUrl.AbsoluteUri;
            var key = discoveredProduct.MPN;

            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, key);
            if (page.InnerHtml.ContainsIgnoreCase("Please enter your customer account number"))
                throw new AuthenticationException();

            var mpn = FormatMpn(key);

            var product = new ScanData(discoveredProduct.ScanData);
            var rows = page.QuerySelectorAll(".productinfoboxheadderraw").Skip(1).ToList();
            foreach (var row in rows)
            {
                var values = row.InnerText.Trim().Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var fieldKey = values.First();
                var fieldValue = values.Last();
                var ppt = _fields[fieldKey];
                product[ppt] = fieldValue.Trim();
            }

            product[ScanField.ManufacturerPartNumber] = mpn;
            product[ScanField.ColorNumber] = GetColorNumber(mpn);
            product[ScanField.PatternNumber] = GetPatternNumber(mpn);

            var otherColorsUrl = new Uri("http://clarencehouse.com/" + page.QuerySelector(".othercolors a").Attributes["href"].Value);
            product[ScanField.WebItemNumber] = otherColorsUrl.GetQueryParameter("pattno");
            product[ScanField.AlternateItemNumber] = otherColorsUrl.GetQueryParameter("pattcd");

            if (page.InnerText.ContainsIgnoreCase("will not be re-ordered") &&
                product[ScanField.StockCount] != "0")
            {
                product[ScanField.IsLimitedAvailability] = "Yes";
            }

            // see if have enough information to determine unit of measure

            // the problem is when stock is none, it just says None and no hint as to UoM

            var stock = product[ScanField.StockCount];
            if (stock != null && !stock.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                string value = null;

                if (stock.ContainsIgnoreCase("yards"))
                    value = "Yard";
                else if (stock.ContainsIgnoreCase("rolls"))
                    value = "Roll";
                else if (stock.ContainsIgnoreCase("each"))
                    value = "Each";

                if (value != null)
                    product[ScanField.UnitOfMeasure] = value;
            }

            var images = await ScrapePublicPageAsync(mpn);
            images.ForEach(product.AddImage);

            return new List<ScanData> {product};
        }

        private async Task<List<ScannedImage>> ScrapePublicPageAsync(string mpn)
        {
            var values = new NameValueCollection();
            values.Add("search_text", mpn);

            var page = await PageFetcher.FetchAsync(PublicSearchUrl, CacheFolder.Images, mpn, values);
            if (page.InnerText.ContainsIgnoreCase("Not Found")) return new List<ScannedImage>();

            var smallUrl = "http://clarencehouse.com" + page.QuerySelector(".search_results img").Attributes["src"].Value;
            var mediumUrl = smallUrl.Replace("/s/", "/m/");
            var largeUrl = smallUrl.Replace("/s/", "/l/");

            if (smallUrl.ContainsIgnoreCase("no_image")) return new List<ScannedImage>();

            // mark them all as primary and prospective, so the head checks are done
            // and the first valid one is selected
            return new List<ScannedImage>
            {
                new ScannedImage(ImageVariantType.Primary, largeUrl),
                new ScannedImage(ImageVariantType.Primary, mediumUrl),
                new ScannedImage(ImageVariantType.Primary, smallUrl),
            };
        }

        private string GetColorNumber(string mpn) { return mpn.IndexOf('-') > 0 ? mpn.Split('-')[1] : string.Empty; }
        private string GetPatternNumber(string mpn) { return mpn.IndexOf('-') > 0 ? mpn.Split('-')[0] : mpn; }

        private string FormatMpn(string mpn)
        {
            mpn = mpn.ToUpper().Trim();
            if (mpn.IndexOf(' ') > 0)
                mpn = mpn.Substring(0, mpn.IndexOf(" "));

            mpn = mpn.Replace("?", "-");
            return mpn.Replace("/", "-");
        }
    }
}
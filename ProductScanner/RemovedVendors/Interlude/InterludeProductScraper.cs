using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Interlude
{
    public class InterludeProductScraper : ProductScraper<InterludeVendor>
    {
        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Item #:", ScanField.SKU},
            { "Dimensions:", ScanField.Dimensions},
            { "Unit:", ScanField.UnitOfMeasure},
            { "Material:", ScanField.Material},
            { "Finish:", ScanField.Finish},
            { "Socket Details:", ScanField.Socket},
            { "Availability:", ScanField.StockCount},
        }; 

        public InterludeProductScraper(IPageFetcher<InterludeVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("http://www.interludehome.com/", ""));

            var data = new ScanData(product.ScanData);

            var specsRows = detailsPage.QuerySelectorAll(".product-specs tr").ToList();
            foreach (var row in specsRows)
            {
                var cells = row.QuerySelectorAll("td").ToList();
                if (cells.Count < 2) continue;

                var label = cells[0].InnerText.Trim();
                var value = cells[1].InnerText.Trim();
                var prop = _fields[label];
                data[prop] = value;
            }

            var wholesalePrice = detailsPage.QuerySelector(".variantprice");
            if (wholesalePrice != null)
                data.Cost = wholesalePrice.InnerText.Replace("Wholesale Price:&nbsp;", "").Replace("Trade Price:&nbsp;", "").Replace("$", "").ToDecimalSafe();
            else
            {
                var salePrice = detailsPage.QuerySelector(".SalePrice");
                if (salePrice != null)
                    data.Cost = salePrice.InnerText.Replace("&nbsp;On Sale For:&nbsp;", "").Replace("$", "").ToDecimalSafe();
            }

            data[ScanField.ManufacturerPartNumber] = data[ScanField.SKU];
            data[ScanField.ProductName] = detailsPage.GetFieldValue(".ProductNameText");
            data.DetailUrl = product.DetailUrl;

            var descriptionItem = detailsPage.QuerySelector(".product-description--label:contains('Item Notes')");
            if (descriptionItem != null)
            {
                data[ScanField.Description] = descriptionItem.NextSibling.NextSibling.InnerText;
            }

            var image = detailsPage.QuerySelector(".ProductDiv img").Attributes["src"].Value;
            data.AddImage(new ScannedImage(ImageVariantType.Primary, "http://www.interludehome.com/" + image));

            return new List<ScanData> { data };
        }
    }
}
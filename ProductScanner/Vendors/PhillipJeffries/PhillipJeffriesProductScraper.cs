using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace PhillipJeffries
{
    public class PhillipJeffriesProductScraper : ProductScraper<PhillipJeffriesVendor>
    {
        private readonly IVendorScanSessionManager<PhillipJeffriesVendor> _sessionManager;

        public PhillipJeffriesProductScraper(IPageFetcher<PhillipJeffriesVendor> pageFetcher, IVendorScanSessionManager<PhillipJeffriesVendor> sessionManager) : base(pageFetcher)
        {
            _sessionManager = sessionManager;
        }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            //var headers = new NameValueCollection();
            //headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            //headers["Accept-Encoding"] = "gzip, deflate, br";
            //headers["Accept-Language"] = "en-US,en;q=0.8,ms;q=0.6";

            var url = product.DetailUrl.AbsoluteUri;
            var id = url.Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries).Last();
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, id);

            if (detailsPage.QuerySelector("base") == null) return new List<ScanData>();

            var baseLink = detailsPage.QuerySelector("base").Attributes["href"].Value;
            var collectionId = baseLink.Replace("/", "").Replace("shop", "");

            var collectionUrl = string.Format("https://www.phillipjeffries.com/api/products/collections/{0}/skews.json", collectionId);
            var skewData = await PageFetcher.FetchAsync(collectionUrl, CacheFolder.Details, collectionId);

            if (skewData.OuterHtml.ContainsIgnoreCase("Internal Server Error"))
            {
                _sessionManager.Log(EventLogRecord.Error("Internal Server Error: " + collectionUrl));
                return new List<ScanData>();
            }

            dynamic data = JObject.Parse(skewData.OuterHtml);
            dynamic match = null;
            foreach (var item in data.data.items)
            {
                if (item.id == id)
                {
                    match = item;
                }
            }

            var scanData = new ScanData();
            scanData[ScanField.ProductName] = match.name;
            scanData[ScanField.ManufacturerPartNumber] = match.id;
            scanData[ScanField.ESellable] = match.is_esellable;
            scanData[ScanField.CollectionId] = collectionId;

            // Specs
            scanData[ScanField.AverageBolt] = match.specs.bolt_size;
            scanData[ScanField.Width] = match.specs.width;
            scanData[ScanField.FireCode] = match.specs.fire_rating;
            scanData[ScanField.Cleaning] = match.specs.maintenance;
            scanData[ScanField.ShippingMethod] = match.specs.shipping_class;
            scanData[ScanField.HorizontalRepeat] = match.specs.horizontal_repeat;
            scanData[ScanField.VerticalRepeat] = match.specs.vertical_repeat;
            scanData[ScanField.LeadTime] = match.specs.leed_credit;
            scanData[ScanField.Color] = match.specs.color;

            scanData[ScanField.UnitOfMeasure] = match.stock.unit_of_measure;

            scanData[ScanField.StockCount] = match.order.wallcovering.availability;
            scanData[ScanField.OrderInfo] = match.order.wallcovering.stocked;
            scanData[ScanField.MinimumQuantity] = match.order.wallcovering.minimum_order;
            scanData[ScanField.OrderIncrement] = match.order.wallcovering.order_increment;

            scanData.Cost = match.order.wallcovering.price.amount;

            scanData[ScanField.Collection] = data.name;
            scanData[ScanField.Description] = data.description;

            string imageUrl = match.assets.original.ToString();
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            // https://www.phillipjeffries.com/api/products/collections/CARD-4-METWEAVE/skews.json
            return new List<ScanData> {scanData};
        }
    }
}
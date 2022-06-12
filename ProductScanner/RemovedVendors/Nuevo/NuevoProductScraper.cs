using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nuevo
{
    public class NuevoProductScraper : ProductScraper<NuevoVendor>
    {
        public NuevoProductScraper(IPageFetcher<NuevoVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var values = new NameValueCollection();
            values.Add("Page", "search");
            values.Add("keyword", mpn);

            var scanData = new ScanData(product.ScanData);
            var searchResults = await PageFetcher.FetchAsync("http://www.nuevoliving.com/NuevoCore.cfm", CacheFolder.Details, mpn, values);
            var images = searchResults.QuerySelectorAll("#imgmain").ToList();
            images.ForEach(x => scanData.AddImage(new ScannedImage(ImageVariantType.Primary, "http://www.nuevoliving.com/" + x.Attributes["src"].Value)));

            scanData[ScanField.ManufacturerPartNumber] = mpn;
            return new List<ScanData> { scanData };
        }
    }
}
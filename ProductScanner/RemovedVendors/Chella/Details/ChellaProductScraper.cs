using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Chella.Details
{
    public class ChellaProductScraper : ProductScraper<ChellaVendor>
    {
        public ChellaProductScraper(IPageFetcher<ChellaVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var mpn = context.ScanData[ProductPropertyType.ManufacturerPartNumber];
            var page = await PageFetcher.FetchAsync(context.ScanData[ProductPropertyType.ProductDetailUrl], CacheFolder.Details, mpn);
            var detailsParagraph = page.QuerySelector("#fabrics_singleview_container p").InnerText;
            var fields = detailsParagraph.Split(new[] {'\n'}).Select(x => x.Trim()).ToList();
            var name = page.QuerySelector("h2").InnerText;

            var splitSku = mpn.Split(new[] {'-'});

            var product = new ScanData(context.ScanData);
            product[ProductPropertyType.PatternNumber] = splitSku.First();
            product[ProductPropertyType.ColorNumber] = splitSku.Last();
            product[ProductPropertyType.ManufacturerPartNumber] = mpn;
            product[ProductPropertyType.ProductName] = name;

            // copy discovery properties over
            product[ProductPropertyType.ProductType] = context.ScanData[ProductPropertyType.ProductType];
            product[ProductPropertyType.Category] = context.ScanData[ProductPropertyType.Category];
            product[ProductPropertyType.Color] = context.ScanData[ProductPropertyType.Color];

            var hiresImage = page.QuerySelector("a[rel='non']");
            if (hiresImage != null)
            {
                product.AddImage(new ScannedImage(ImageVariantType.Primary, hiresImage.Attributes["href"].Value));
            }
            else
            {
                var lowerRes = page.QuerySelector(".MagicZoomPlus").Attributes["href"].Value;
                product.AddImage(new ScannedImage(ImageVariantType.Primary, lowerRes));
            }


            foreach (var field in fields)
            {
                if (field.Contains("Country")) product[ProductPropertyType.CountryOfOrigin] = field;
                if (field.Contains("Notes")) product[ProductPropertyType.Note] = field;
                if (field.Contains("FR")) product[ProductPropertyType.FireCode] = field;
                if (!field.Contains("Durability")) continue;
                product[ProductPropertyType.Durability] = field;
            }
            return new List<ScanData>{ product };
        }
    }
}
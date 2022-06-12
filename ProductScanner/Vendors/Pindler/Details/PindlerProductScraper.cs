using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Pindler.Details
{
    public class PindlerProductScraper : ProductScraper<PindlerVendor>
    {
        public PindlerProductScraper(IPageFetcher<PindlerVendor> pageFetcher) : base(pageFetcher) { }

        public override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var imageUrl = product.ScanData[ScanField.ImageUrl];
            if (imageUrl != null)
            {
                // the 450px image is what is in the CSV file, and we can typically find a hi-res version
                // in the web_hr folder - but note that it is typically a different shot like closer up - and Pindler has
                // advised to use the hi-res photo only for the enlargement view.

                // http://www.pindler.com/images/products/memo_images/web_pictures/1331_LINEN.jpg  (450px)
                // http://www.pindler.com/images/products/memo_images/web_pictures/web_hr/1331_LINEN.jpg  (1200px)

                var filename = new Uri(imageUrl, UriKind.Absolute).Segments.Last();
                var altImageUrl = string.Format("http://www.pindler.com/images/products/memo_images/web_pictures/web_hr/{0}", filename);
                product.ScanData.AddImage(new ScannedImage(ImageVariantType.Primary, altImageUrl));
                product.ScanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));
            }
            return Task.FromResult(new List<ScanData> {product.ScanData});
        }
    }
}
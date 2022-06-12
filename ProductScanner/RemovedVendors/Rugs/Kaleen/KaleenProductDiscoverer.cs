using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Kaleen
{
    public class KaleenProductDiscoverer : IProductDiscoverer<KaleenVendor>
    {
        private readonly IProductFileLoader<KaleenVendor> _productFileLoader;
        private readonly KaleenNewlyDiscontinuedFileLoader _discontinuedFileLoader;
        private readonly KaleenInventoryFileLoader _inventoryFileLoader;
        private readonly IPageFetcher<KaleenVendor> _pageFetcher;
        private const string ImageFolder = "ftp://kaleenimages:2016kaleendownload@205.144.214.4/Kaleen%20High%20Res%20JPEG%20Images%20December%202017/";

        public KaleenProductDiscoverer(IProductFileLoader<KaleenVendor> productFileLoader, KaleenNewlyDiscontinuedFileLoader discontinuedFileLoader, 
            KaleenInventoryFileLoader inventoryFileLoader, IPageFetcher<KaleenVendor> pageFetcher)
        {
            _productFileLoader = productFileLoader;
            _discontinuedFileLoader = discontinuedFileLoader;
            _inventoryFileLoader = inventoryFileLoader;
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var variants = await _productFileLoader.LoadProductsAsync();
            var discontinued = _discontinuedFileLoader.LoadData();
            var stock = _inventoryFileLoader.LoadData();

            var imageUrls = await _pageFetcher.FetchFtpAsync(ImageFolder, CacheFolder.Images, "imagesFtp", new NetworkCredential("kaleenimages", "2016kaleendownload"));
            foreach (var variant in variants)
            {
                var matchedImages = imageUrls.Where(x => x.Contains(variant[ScanField.Image1])).ToList();
                matchedImages.ForEach(x => variant.AddImage(new ScannedImage(GetType(x), ImageFolder + x)));

                var discMatch = discontinued.SingleOrDefault(x => x[ScanField.UPC] == variant[ScanField.UPC]);
                if (discMatch != null)
                {
                    variant[ScanField.Cost] = discMatch[ScanField.Cost];
                    variant[ScanField.StockCount] = discMatch[ScanField.StockCount];
                    if (discMatch[ScanField.StockCount].ToIntegerSafe() == 0) variant.IsDiscontinued = true;
                }

                var stockMatch = stock.SingleOrDefault(x => x[ScanField.UPC] == variant[ScanField.UPC]);
                if (stockMatch != null)
                {
                    variant[ScanField.StockCount] = stockMatch[ScanField.StockCount];
                }
            }
            return variants.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private ImageVariantType GetType(string url)
        {
            if (url.Contains("_1")) return ImageVariantType.Scene;
            if (url.Contains("_2")) return ImageVariantType.Scene;
            if (url.Contains("_3")) return ImageVariantType.Round;
            if (url.Contains("_4")) return ImageVariantType.Square;
            if (url.Contains("_5")) return ImageVariantType.Runner;
            return ImageVariantType.Rectangular;
        }
    }
}
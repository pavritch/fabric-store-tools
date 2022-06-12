using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using MoreLinq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;

namespace Jaipur
{
    // inventory file url - ftp://jrinventory:inventory*21@fingerprinti.com/Jaipur%20inventory%20feed.xls
    // images url - ftp://jrimages:JRImg2775@fingerprinti.com/Jaipur%20Rugs%20Item%20Images/
    public class JaipurMetadataCollector : IMetadataCollector<JaipurVendor>
    {
        private const string AllRugsUrl = "https://www.jaipurliving.com/rugs.aspx";
        private readonly IPageFetcher<JaipurVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<JaipurVendor> _sessionManager;
        private readonly IProductFileLoader<JaipurVendor> _fileLoader; 
        private readonly JaipurSearcher _searcher;

        public JaipurMetadataCollector(IPageFetcher<JaipurVendor> pageFetcher, IVendorScanSessionManager<JaipurVendor> sessionManager, IProductFileLoader<JaipurVendor> fileLoader, JaipurSearcher searcher)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _fileLoader = fileLoader;
            _searcher = searcher;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                foreach (var variant in product.Variants)
                {
                    var match = fileProducts.SingleOrDefault(x => x[ScanField.ItemNumber] == variant[ScanField.ManufacturerPartNumber]);
                    if (match != null)
                        variant[ScanField.MAP] = match[ScanField.MAP];
                }
            }

            await PopulateSearchMetadataAsync(products);
            await DiscoverImagesAsync();

            var images = await DiscoverImagesAsync();
            foreach (var product in products)
            {
                var sku = product[ScanField.SKU];
                var productImageUrls = images.Where(x => x.GetSku() == sku).Select(img => img.GetFullUrl()).ToList();
                var primary = productImageUrls.Where(x => x.Contains("Headshot"));
                var scene = productImageUrls.Where(x => x.Contains("Floorshot") || x.Contains("Corner") || x.Contains("Closeup") || x.Contains("Roomscene"));

                MoreEnumerable.ForEach(primary, x => product.AddImage(new ScannedImage(GetImageVariant(x), x, new NetworkCredential("jrimages", "JRImg2775"))));
                MoreEnumerable.ForEach(scene, x => product.AddImage(new ScannedImage(ImageVariantType.Scene, x, new NetworkCredential("jrimages", "JRImg2775"))));
            }
            return products;
        }

        private ImageVariantType GetImageVariant(string imageUrl)
        {
            if (imageUrl.ContainsIgnoreCase("Oval")) return ImageVariantType.Oval;
            if (imageUrl.ContainsIgnoreCase("Rectangle")) return ImageVariantType.Rectangular;
            if (imageUrl.ContainsIgnoreCase("Round")) return ImageVariantType.Round;
            if (imageUrl.ContainsIgnoreCase("Runner")) return ImageVariantType.Runner;
            if (imageUrl.ContainsIgnoreCase("Square")) return ImageVariantType.Square;
            return ImageVariantType.Scene;
        }

        private async Task PopulateSearchMetadataAsync(List<ScanData> products)
        {
            // color - different from details, more of a color grouping
            // pattern - not on details
            // designer - I don't think this is clear on details - it's probably there but as part of collection

            var searchPage = await _pageFetcher.FetchAsync(AllRugsUrl, CacheFolder.Search, "allRugs");
            var colors = searchPage.QuerySelectorAll("#Subhdr152 span.chkbox").Select(x => x.Attributes["title"].Value);
            foreach (var color in colors)
            {
                var productIds = await _searcher.RunColorSearch(color);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                foreach (var match in matches) match[ScanField.ColorGroup] = color;
            }

            var patterns = searchPage.QuerySelectorAll("#Subhdr182 span.chkbox").Select(x => x.Attributes["title"].Value);
            foreach (var pattern in patterns)
            {
                var productIds = await _searcher.RunPatternSearch(pattern);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                foreach (var match in matches) match[ScanField.Pattern] = pattern;
            }

            var designers = searchPage.QuerySelectorAll("#Subhdr1102 span.chkbox").Select(x => x.Attributes["title"].Value);
            foreach (var designer in designers)
            {
                var productIds = await _searcher.RunDesignerSearch(designer);
                var matches = products.Where(x => productIds.Contains(x[ScanField.ManufacturerPartNumber]));
                foreach (var match in matches) match[ScanField.Designer] = designer;
            }
        }

        private async Task<List<ImageInfo>> DiscoverImagesAsync()
        {
            var imageUrls = new List<ImageInfo>();
            await _sessionManager.ForEachNotifyAsync("Discovering lo res images", _loResImageLocations, async imageLocation =>
            {
                var filename = imageLocation.Key.Replace("ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/", "")
                    .Replace("/", "-");
                var loRes = await _pageFetcher.FetchFtpAsync(imageLocation.Key, CacheFolder.Images, filename + "lo", new NetworkCredential("jrimages", "JRImg2775"));
                loRes.ForEach(x => imageUrls.Add(new ImageInfo(x, imageLocation.Key, imageLocation.Value)));
            });

            await _sessionManager.ForEachNotifyAsync("Discovering hi res images", _imageLocations, async imageLocation =>
            {
                var filename = imageLocation.Key.Replace("ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/", "")
                    .Replace("/", "-");
                var hiRes = await _pageFetcher.FetchFtpAsync(imageLocation.Key, CacheFolder.Images, filename + "hi", new NetworkCredential("jrimages", "JRImg2775"));
                hiRes.ForEach(x => imageUrls.Add(new ImageInfo(x, imageLocation.Key, imageLocation.Value)));
            });
            return imageUrls;
        }

        private readonly Dictionary<string, ImageVariantType> _imageLocations = new Dictionary<string, ImageVariantType>
        {
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Closeup/Rectangle/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Corner/Rectangle/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Corner/Round/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Corner/Runner/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot/Oval/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot/Rectangle/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot/Round/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot/Runner/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot/Square/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Floorshot2/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Headshot/Oval/", ImageVariantType.Oval },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Headshot/Rectangle/", ImageVariantType.Rectangular },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Headshot/Round/", ImageVariantType.Round },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Headshot/Runner/", ImageVariantType.Runner },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Headshot/Square/", ImageVariantType.Square },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20HRes/Rugs/Roomshot/Rectangle/", ImageVariantType.Scene },
        }; 

        private readonly Dictionary<string, ImageVariantType> _loResImageLocations = new Dictionary<string, ImageVariantType>
        {
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Closeup/Rectangle/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Corner/Rectangle/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Corner/Round/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Corner/Runner/", ImageVariantType.Alternate },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot/Oval/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot/Rectangle/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot/Round/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot/Runner/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot/Square/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Floorshot2/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Headshot/Oval/", ImageVariantType.Oval },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Headshot/Rectangle/", ImageVariantType.Rectangular },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Headshot/Round/", ImageVariantType.Round },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Headshot/Runner/", ImageVariantType.Runner },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Headshot/Square/", ImageVariantType.Square },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Roomscene/Rectangle/", ImageVariantType.Scene },
            { "ftp://jrimages:JRImg2775@fingerprinti.com/Images%20-%20LRes/Rugs/Roomscene/Round/", ImageVariantType.Scene },
        }; 
    }
}
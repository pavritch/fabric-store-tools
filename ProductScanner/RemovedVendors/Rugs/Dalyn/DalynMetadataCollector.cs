using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Dalyn
{
    public class DalynMetadataCollector : IMetadataCollector<DalynVendor>
    {
        private readonly IProductFileLoader<DalynVendor> _fileLoader;
        private readonly IPageFetcher<DalynVendor> _pageFetcher;
        private readonly DalynInventoryFileLoader _inventoryFileLoader;

        private const string RugCollectionsUrl = "http://scanner.insidefabric.com/vendors/Dalyn/Stock%20Rug%20Collections%20Images,%20Expanded%20content/";

        public DalynMetadataCollector(IProductFileLoader<DalynVendor> fileLoader, IPageFetcher<DalynVendor> pageFetcher, DalynInventoryFileLoader inventoryFileLoader)
        {
            _fileLoader = fileLoader;
            _pageFetcher = pageFetcher;
            _inventoryFileLoader = inventoryFileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileVariants = await _fileLoader.LoadProductsAsync();
            var imageUrls = await GetImageUrls(RugCollectionsUrl, "collections");
            imageUrls.AddRange(await GetImageUrls("http://scanner.insidefabric.com/vendors/Dalyn/Clearance%20Rug%20Collections%20Images/", "clearance"));

            var stockData = _inventoryFileLoader.LoadStockData();

            foreach (var variant in fileVariants)
            {
                var patternNumber = variant[ScanField.PatternNumber];
                var color = variant[ScanField.Color];

                var match1 = FindMatchedImage(imageUrls, patternNumber, color, variant[ScanField.Image1], ImageVariantType.Rectangular);
                var match2 = FindMatchedImage(imageUrls, patternNumber, color, variant[ScanField.Image2], ImageVariantType.Scene);
                var match3 = FindMatchedImage(imageUrls, patternNumber, color, variant[ScanField.Image3], ImageVariantType.Scene);
                var match4 = FindMatchedImage(imageUrls, patternNumber, color, variant[ScanField.Image4], ImageVariantType.Scene);
                var match5 = FindMatchedImage(imageUrls, patternNumber, color, variant[ScanField.Image5], ImageVariantType.Scene);

                if (match1 != null) variant.AddImage(match1);
                if (match2 != null) variant.AddImage(match2);
                if (match3 != null) variant.AddImage(match3);
                if (match4 != null) variant.AddImage(match4);
                if (match5 != null) variant.AddImage(match5);

                var stockMatch = stockData.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == variant[ScanField.ManufacturerPartNumber]);
                if (stockMatch != null)
                {
                    variant[ScanField.StockCount] = stockMatch[ScanField.StockCount];
                }
                else
                {
                    variant[ScanField.StockCount] = "0";
                }
            }

            return fileVariants;
        }

        private ScannedImage FindMatchedImage(List<string> imageUrls, string patternNumber, string color, string key, ImageVariantType type)
        {
            if (key == string.Empty) return null;

            var match = imageUrls.FirstOrDefault(x => IsMatch(x, key));
            if (match != null)
            {
                return new ScannedImage(type, match);
            }
            var possibleImages = imageUrls.Where(x => x.Contains(patternNumber.ToLower()))
                .Where(x => x.Contains(color.ToLower())).ToList();
            if (possibleImages.Any())
                return new ScannedImage(type, possibleImages.First());
            return null;
        }

        private bool IsMatch(string imageUrl, string key)
        {
            var formattedKey = key.ToLower().Replace(" ", "").Replace("_", "");
            return imageUrl.Replace("%20", "").Replace("_", "").Contains("/" + formattedKey);
        }

        private async Task<List<string>> GetImageUrls(string startUrl, string key)
        {
            var baseUrl = "http://scanner.insidefabric.com";
            // images:
            // looks like a couple in Clearance Rug Collections Images
            // mostly everything is in Stock Rug Collections Images
            var imageUrls = new List<string>();
            var results = await _pageFetcher.FetchAsync(startUrl, CacheFolder.Images, key);
            var collections = results.QuerySelectorAll("a").Skip(1).ToList();
            foreach (var collection in collections)
            {
                var collectionItemsPage = await _pageFetcher.FetchAsync(baseUrl + collection.Attributes["href"].Value, CacheFolder.Images, collection.InnerText);
                var collectionItems = collectionItemsPage.QuerySelectorAll("a").Skip(1).ToList();
                foreach (var item in collectionItems)
                {
                    if (item.InnerText.ContainsIgnoreCase(".jpg"))
                    {
                        imageUrls.Add(baseUrl + item.Attributes["href"].Value);
                        continue;
                    }
                    var imageListPage = await _pageFetcher.FetchAsync(baseUrl + item.Attributes["href"].Value, CacheFolder.Images, item.InnerText);
                    var images = imageListPage.QuerySelectorAll("a").Skip(1).ToList();
                    images.ForEach(x => imageUrls.Add(baseUrl + x.Attributes["href"].Value));
                }
            }
            return imageUrls.Select(x => x.ToLower()).Distinct().ToList();
        }
    }
}
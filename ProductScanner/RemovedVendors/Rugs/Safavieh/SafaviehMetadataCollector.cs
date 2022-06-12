using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Safavieh
{
    public class SafaviehMetadataCollector : IMetadataCollector<SafaviehVendor>
    {
        private readonly SafaviehInventoryFileLoader _inventoryFileLoader;
        private readonly IProductFileLoader<SafaviehVendor> _fileLoader;
        private readonly IPageFetcher<SafaviehVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<SafaviehVendor> _sessionManager; 
        private readonly IStorageProvider<SafaviehVendor> _storageProvider; 

        private readonly NetworkCredential _ftpCredential = new NetworkCredential("onlinevendors", "soho712");

        //private const string FebUpdateImages = "ftp://onlinevendors:soho712@ftp.safavieh.com/DROP%20SHIP%20VENDORS/2015%20Feb%20Rug%20Update%20Images/";
        private const string FloorShots = "ftp://onlinevendors:soho712@ftp.safavieh.com/DROP%20SHIP%20VENDORS/Room%20and%20Lifestyle%20Images%205-2-16/";
        private const string AllImages = "ftp://onlinevendors:soho712@ftp.safavieh.com/DROP%20SHIP%20VENDORS/RUG%20IMAGES%20-%20COMPLETE%20COLLECTION/PROMOTIONAL%20RUG%20images%20-%20NO%20MAP/";

        private const string InventoryFolder = "ftp://ftp.safavieh.com/DROP%20SHIP%20VENDORS/Info/";

        private const string ColorSearchUrl = "http://safavieh.com/rugs/colors";

        public SafaviehMetadataCollector(SafaviehInventoryFileLoader inventoryFileLoader, IProductFileLoader<SafaviehVendor> fileLoader, IPageFetcher<SafaviehVendor> pageFetcher, IVendorScanSessionManager<SafaviehVendor> sessionManager, IStorageProvider<SafaviehVendor> storageProvider)
        {
            _inventoryFileLoader = inventoryFileLoader;
            _fileLoader = fileLoader;
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _storageProvider = storageProvider;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            _pageFetcher.DisableCaching();
            var listing = await _pageFetcher.FetchFtpAsync(InventoryFolder, CacheFolder.Search, "inventoryfolder", _ftpCredential);
            _pageFetcher.EnableCaching();

            if (!listing.Any()) throw new Exception("Inventory Folder Contains no Items");

            var filename = listing.Last();

            await DownloadInventoryFiles(InventoryFolder + filename);
            var stockData = _inventoryFileLoader.LoadStockData();

            var joinedData = products.Join(stockData, 
                variant => variant[ScanField.ManufacturerPartNumber], 
                stock => stock[ScanField.ManufacturerPartNumber],
                (variant, stock) =>
                {
                    variant[ScanField.StockCount] = stock[ScanField.StockCount];
                    return variant;
                }).ToList();

            // scan for images
            await PopulateImageURLsAsync(joinedData);

            // also some metadata from the website that we can gather
            // come back to this later - still having some issues with pulling the search results out
            //var colorSearchPage = await _pageFetcher.FetchAsync(ColorSearchUrl, CacheFolder.Search, "color-search");
            //var colors = colorSearchPage.QuerySelectorAll(".item");
            //foreach (var color in colors)
            //{
                // http://safavieh.com/rugs?colors=brown&pg=1
                //var matches = await FindMatches(color);
            //}
            return joinedData;
        }

        private async Task<List<string>> FindMatches(HtmlNode node)
        {
            var name = node.QuerySelector(".title").InnerText;
            var urlName = node.QuerySelector("a").Attributes["href"].Value.Split('=').Last();
            var pageNum = 1;
            while (true)
            {
                var values = CreateSearchValues(urlName, pageNum);
                var pageData = await _pageFetcher.FetchAsync("http://safavieh.com/wp-admin/admin-ajax.php", CacheFolder.Search, name + "-" + pageNum, values);
                var html = pageData.OuterHtml.CaptureWithinMatchedPattern(@"html\('(?<capture>(.*))'\)");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var products = htmlDoc.DocumentNode.QuerySelectorAll(".item").ToList();

                pageNum++;
            }
        }

        private NameValueCollection CreateSearchValues(string name, int pageNum)
        {
            var values = new NameValueCollection();
            values.Add("data[0][itemTerm_id]", name);
            values.Add("data[0][itemId]", name);
            values.Add("data[0][itemCategory]", "colors");
            values.Add("action", "saf_ajax");
            values.Add("func", "filter_items");
            values.Add("type", "rugs");
            values.Add("search_type", "rugs");
            values.Add("itemCount", "50");
            values.Add("page", pageNum.ToString());
            return values;
        }

        private async Task DownloadInventoryFiles(string url)
        {
            var bytes = await new Uri(url).DownloadBytesFromWebAsync(_ftpCredential);
            _storageProvider.SaveStockFile(ProductFileType.Xlsx, bytes);
        }

        private async Task PopulateImageURLsAsync(List<ScanData> variants)
        {
            //var newImages = await _pageFetcher.FetchFtpAsync(FebUpdateImages, CacheFolder.Images, "newImages", _ftpCredential);
            var floorShots = await _pageFetcher.FetchFtpAsync(FloorShots, CacheFolder.Images, "floorShots", _ftpCredential);

            var folders = await _pageFetcher.FetchFtpAsync(AllImages, CacheFolder.Images, "allImages", _ftpCredential);
            var imageDict = new Dictionary<string, List<string>>();
            await _sessionManager.ForEachNotifyAsync("Loading image subfolders", folders, async folder =>
            {
                var subfolderUrl = AllImages + folder;
                var images = await _pageFetcher.FetchFtpAsync(subfolderUrl, CacheFolder.Images, folder, _ftpCredential);
                imageDict.Add(folder, images);
            });

            _sessionManager.ForEachNotify("Populating images", variants, variant =>
            {
                //AddImages(newImages, FebUpdateImages, variant);
                AddImages(floorShots, FloorShots, variant);

                foreach (var images in imageDict)
                {
                    var folder = images.Key;
                    AddImages(images.Value, AllImages + folder + "/", variant);
                }
                //var webUrl = string.Format("http://8075feee030fbee1dd88-722e1f8caa27908fefb21615e56d7a68.r96.cf2.rackcdn.com/antique/zoom/{0}.jpg", 
                //variant[ScanField.ProductName].ToLower());
                //variant.AddImage(new ScannedImage(ImageVariantType.Rectangular, webUrl));
            });
        }

        private void AddImages(List<string> imageUrls, string baseUrl, ScanData variant)
        {
            var matches = imageUrls.Where(x => x.ToLower().StartsWith(variant[ScanField.SKU].ToLower()));
            matches.ForEach(x => variant.AddImage(new ScannedImage(FindShape(x), baseUrl + x, _ftpCredential)));
            //else
            //{
            //    match = imageUrls.SingleOrDefault(x => x == variant[ScanField.Image1]);
            //    if (match != null)
            //        variant.AddImage(new ScannedImage(FindShape(match), baseUrl + variant[ScanField.Image1], _ftpCredential));
            //}
        }

        private ImageVariantType FindShape(string url)
        {
            if (url.ContainsIgnoreCase("-CRNR")) return ImageVariantType.Scene;
            if (url.ContainsIgnoreCase("-FL")) return ImageVariantType.Scene;
            if (url.ContainsIgnoreCase("ROOM")) return ImageVariantType.Scene;
            if (url.ContainsIgnoreCase("FLOOR")) return ImageVariantType.Scene;

            if (url.ContainsIgnoreCase("DETAIL")) return ImageVariantType.Other;

            var parts = url.Split('-');
            if (parts.Length < 2) return ImageVariantType.Rectangular;

            var size = parts[1];
            if (size.ContainsIgnoreCase("SQ")) return ImageVariantType.Square;
            if (size.ContainsIgnoreCase("OV")) return ImageVariantType.Oval;
            if (size.ContainsIgnoreCase("R")) return ImageVariantType.Round;
            return ImageVariantType.Rectangular;
        }
    }
}
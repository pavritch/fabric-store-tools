using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Surya
{
    public class SuryaMetadataCollector : IMetadataCollector<SuryaVendor>
    {
        private const string StyleListUrl = "http://www.surya.com/rugs/?shopby=2";
        private const string ColorListUrl = "http://www.surya.com/rugs/?shopby=3";
        private readonly List<string> _imageLocations = new List<string>
        {
            "ftp://suryacustomer:homedecor@media1.surya.com/Rugs/Corners",
            "ftp://suryacustomer:homedecor@media1.surya.com/Rugs/RoomScenes",
            "ftp://suryacustomer:homedecor@media1.surya.com/Rugs/Shag_Profiles",
            "ftp://suryacustomer:homedecor@media1.surya.com/Rugs/Style%20Shots",
            "ftp://suryacustomer:homedecor@media1.surya.com/Rugs/Textures",
        }; 
        private readonly IPageFetcher<SuryaVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<SuryaVendor> _sessionManager;

        public SuryaMetadataCollector(IPageFetcher<SuryaVendor> pageFetcher, IVendorScanSessionManager<SuryaVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            // commented out for now, because of authentication issues with their server
            //await PopulateImagesAsync(products);

            await PopulateStylesAsync(products);
            await PopulateColorsAsync(products);
            //await PopulateTrendsAsync(products);

            return products;
        }

        private async Task PopulateImagesAsync(List<ScanData> products)
        {
            await _sessionManager.ForEachNotifyAsync("Populating images", _imageLocations, async imageLocation =>
            {
                await SetImagesAsync(imageLocation, products);
            });
        }

        private async Task SetImagesAsync(string imageLocation, List<ScanData> products)
        {
            var fileList = await _pageFetcher.FetchFtpAsync(imageLocation, CacheFolder.Images, 
                imageLocation.Replace("ftp://suryacustomer:homedecor@media1.surya.com/Rugs/", ""), new NetworkCredential("suryacustomer", "homedecor"));
            foreach (var file in fileList)
            {
                var match = products.FirstOrDefault(x => file.StartsWith(x[ScanField.ManufacturerPartNumber].Replace("-", "").ToLower()));
                if (match != null) match.AddImage(new ScannedImage(ImageVariantType.Scene, imageLocation + "/" + file, new NetworkCredential("suryacustomer", "homedecor")));
            }
        }

        private async Task PopulateStylesAsync(List<ScanData> products)
        {
            var styleList = await _pageFetcher.FetchAsync(StyleListUrl, CacheFolder.Search, "styles");
            var styleIds = styleList.QuerySelectorAll(".product-name a")
                .Select(x => StringExtensions.CaptureWithinMatchedPattern(x.Attributes["href"].Value, @"stylegroup_id=(?<capture>(\d+))")).ToList();
            var styleNames = styleList.QuerySelectorAll(".product-name a").Select(x => x.InnerText.Trim()).ToList();
            await _sessionManager.ForEachNotifyAsync("Collecting metadata by style", styleIds, async (index, styleId) =>
            {
                var styleUrl = string.Format("http://www.surya.com/rugs/?IsFiltered=1&stylegroup_id={0}&n=0", styleId);
                var stylePage = await _pageFetcher.FetchAsync(styleUrl, CacheFolder.Search, "style-" + styleId);
                var matchedMpns = stylePage.QuerySelectorAll(".product-name a").Select(x => x.InnerText);
                var matchedProducts = products.Where(x => matchedMpns.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
                foreach (var product in matchedProducts) product[ScanField.Style] = styleNames[index];
            });
        }

        private async Task PopulateColorsAsync(List<ScanData> products)
        {
            var colorList = await _pageFetcher.FetchAsync(ColorListUrl, CacheFolder.Search, "colors");
            var colorIds = colorList.QuerySelectorAll(".product-name a")
                .Select(x => x.Attributes["href"].Value.CaptureWithinMatchedPattern(@"color_id=(?<capture>(\d+))")).ToList();
            var colorNames = colorList.QuerySelectorAll(".product-name a").Select(x => x.InnerText.Trim()).ToList();
            await _sessionManager.ForEachNotifyAsync("Collecting metadata by color", colorIds, async (index, colorId) =>
            {
                var url = string.Format("http://www.surya.com/rugs/?IsFiltered=1&color_id={0}&n=0", colorId);
                var colorPage = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "color-" + colorId);
                var matchedMpns = colorPage.QuerySelectorAll(".product-name a").Select(x => x.InnerText);
                var matchedProducts = products.Where(x => matchedMpns.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
                foreach (var product in matchedProducts) AddColor(product, colorNames[index]);
            });
        }

        private void AddColor(ScanData data, string color)
        {
            if (data[ScanField.ColorGroup] == string.Empty) data[ScanField.ColorGroup] = color;
            else data[ScanField.ColorGroup] += ", " + color;
        }

        private async Task PopulateTrendsAsync(List<ScanData> products)
        {
            var trendList = await _pageFetcher.FetchAsync(ColorListUrl, CacheFolder.Search, "colors");
            var trendIds = trendList.QuerySelectorAll("#p_lt_ctl06_pageplaceholder_p_lt_ctl01_Filter_filterControl_ddlTrend option")
                .Select(x => x.Attributes["value"].Value).Where(x => !string.IsNullOrWhiteSpace(x));
            var trendNames = trendList.QuerySelector("#p_lt_ctl06_pageplaceholder_p_lt_ctl01_Filter_filterControl_ddlTrend").InnerText
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(x => x.Trim()).ToList();
            await _sessionManager.ForEachNotifyAsync("Collecting metadata by trend", trendIds, async (index, trendId) =>
            {
                var url = string.Format("http://www.surya.com/rugs/?isfiltered=1&trend={0}&n=0", trendId);
                var trendPage = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "trend-" + trendId);
                var matchedMpns = trendPage.QuerySelectorAll(".product-name a").Select(x => x.InnerText);
                var matchedProducts = products.Where(x => matchedMpns.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
                foreach (var product in matchedProducts) product[ScanField.Design] = trendNames[index];
            });
        }
    }
}
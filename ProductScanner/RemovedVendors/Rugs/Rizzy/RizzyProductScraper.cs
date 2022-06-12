using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;

namespace Rizzy
{
    public class RizzyProductScraper : ProductScraper<RizzyVendor>
    {
        private const string ImageUrl = "ftp://remote.rizzyhome.com:2121/Rugs/2014%20Rug%20Images%20-%20Catalog/Flat%20Shots/Flats___Resized/hires/{0}/";
        private const string SearchUrl = "http://www.rizzyhome.com/api/APIItemSearch/GetAllItemsBySearchKeyWord?1=1";
        private const string MainPage = "http://www.rizzyhome.com/Home";
        private const string DetailUrl = "http://www.rizzyhome.com/api/APIItemSearch/GetItemsDetail";
        private NetworkCredential _ftpCredential = new NetworkCredential("images", "ftpimg");
        // user=images, pass=ftpimg
        public RizzyProductScraper(IPageFetcher<RizzyVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var scanData = new ScanData(product.ScanData);
            var id = scanData[ScanField.ItemNumber].Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries).First();

            var mainPage = await PageFetcher.FetchAsync(MainPage, CacheFolder.Details, "main");
            var token = mainPage.QuerySelector("#hdnaccesstoken").Attributes["value"].Value;

            var values = new Dictionary<string, string>();
            values.Add("itemCategoryIds", "");
            values.Add("itemDesignIds", "");
            values.Add("itemId", "");
            values.Add("userId", "");
            values.Add("userNo", "0");
            values.Add("itemlifeStyleListIds", "");
            values.Add("itemcolorListIds", "");
            values.Add("itemsizeListIds", "");
            values.Add("designNo", "0");
            values.Add("colorNo", "-1");
            values.Add("generalDesignNo", "");
            values.Add("WebMainCollectionNo", "0");
            values.Add("pageSize", "20");
            values.Add("pageIndex", "1");
            values.Add("generalSizeNo", "-1");
            values.Add("generalColorNo", "0");
            values.Add("isLoadAll", "true");
            values.Add("priceRange", "0");
            values.Add("SearchKeywords", id);
            values.Add("isSpecialBuys", "false");

            var headers = new NameValueCollection();
            headers.Add("Content-Type", "application/json");
            headers.Add("accesstoken", token);

            var searchPage = await PageFetcher.FetchAsync(SearchUrl, CacheFolder.Details, "search-" + id, values.ToJSON(), headers);
            var results = JObject.Parse(searchPage.InnerText);
            var matches = results["Data"]["result"]["itemsInfoList"];
            if (!matches.ToObject<List<object>>().Any())
                return new List<ScanData>();

            var match = matches[0];
            var itemId = match["itemId"].ToString();
            var designId = match["DesignId"].ToString();
            var catId = match["CategoryID"].ToString();

            var detailValues = new Dictionary<string, string>();
            detailValues.Add("Country", "");
            detailValues.Add("FiberDescription", "");
            detailValues.Add("Thickness", "");
            detailValues.Add("designId", designId);
            detailValues.Add("isLoadAll", "true");
            detailValues.Add("itemCategoryIds", catId);
            detailValues.Add("itemDesignIds", "");
            detailValues.Add("itemId", itemId);
            detailValues.Add("itemcolorListIds", "");
            detailValues.Add("itemlifeStyleListIds", "");
            detailValues.Add("itemsizeListIds", "");
            detailValues.Add("userId", "");
            detailValues.Add("userNo", "0");

            var detailPage = await PageFetcher.FetchAsync(DetailUrl, CacheFolder.Details, itemId, detailValues.ToJSON(), headers);
            var jObj = JObject.Parse(detailPage.InnerText);
            var quantity = jObj["Data"]["result"]["itemInfo"]["quantity"].ToObject<string>();
            scanData[ScanField.StockCount] = quantity;
            scanData[ScanField.ManufacturerPartNumber] = scanData[ScanField.SKU];

            var imageUrl = "http://www.rizzyhome.com" + jObj["Data"]["result"]["itemInfo"]["ImageUrl"].ToObject<string>();
            scanData.AddImage(new ScannedImage(ImageVariantType.Rectangular, imageUrl));

            //var url = "http://rizzyhome.com/" + results.Attributes["href"].Value;
            //var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, id);

            //var shippingInfo = detailsPage.QuerySelector("#tab2 p").InnerText;
            //var imageUrl = "http://rizzyhome.com/" + detailsPage.QuerySelector("#zoom1").Attributes["href"].Value;

            //scanData[ProductPropertyType.TempContent2] = shippingInfo;
            //scanData[ProductPropertyType.ProductDetailUrl] = url;

            //var pattern = product.ScanData[ProductPropertyType.PatternName];
            //var imageListUrl = string.Format(ImageUrl, pattern);
            //var imageList = await PageFetcher.FetchFtpAsync(imageListUrl, CacheFolder.Images, pattern, _ftpCredential);

            return new List<ScanData> { scanData };
        }
    }
}
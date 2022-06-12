namespace Loloi.Previous
{
    //public class LoloiProductScraper : ProductScraper<LoloiVendor>
    //{
    //    private const string BaseImageUrl = "http://images.repzio.com/productimages/175/";
    //    public LoloiProductScraper(IPageFetcher<LoloiVendor> pageFetcher) : base(pageFetcher) { }

    //    public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
    //    {
    //        var headers = new NameValueCollection();
    //        headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

    //        var url = product.DetailUrl.AbsoluteUri;
    //        var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("http://www.loloirugs.com/details/", ""), headers: headers);

    //        var collection = detailsPage.GetFieldValue("#collectionSubTotal");
    //        var name = detailsPage.GetFieldValue(".product-detail h1");
    //        var details = detailsPage.QuerySelectorAll(".lg-detail li").Select(x => x.InnerText).ToList();

    //        var images = detailsPage.QuerySelector(".slideset").QuerySelectorAll(".image")
    //            .Select(x => x.InnerText.CaptureWithinMatchedPattern("/175/(?<capture>(.*)).jpg")).ToList();
    //        var mpn = images.First().Replace("_lg", "");
    //        var description = detailsPage.GetFieldValue("#description");
    //        var care = detailsPage.GetFieldValue("#care");
    //        var detailsBottom = detailsPage.QuerySelectorAll("#details p").Select(x => x.InnerText).ToList();

    //        var scanData = new ScanData();
    //        scanData.DetailUrl = new Uri(url);
    //        scanData[ScanField.Collection] = collection;
    //        scanData[ScanField.PatternName] = name;
    //        scanData[ScanField.ManufacturerPartNumber] = mpn;
    //        scanData[ScanField.Description] = description;
    //        scanData[ScanField.Cleaning] = care;

    //        scanData[ScanField.Construction] = details[0];
    //        scanData[ScanField.Material] = details[1];
    //        scanData[ScanField.Country] = details[2];

    //        scanData[ScanField.PileHeight] = detailsBottom[0].Replace("Pile Height", "");
    //        scanData[ScanField.Backing] = detailsBottom[1].Replace("Backing", "");
    //        scanData[ScanField.Weight] = detailsBottom[2].Replace("Weight", "");

    //        scanData.AddImage(new ScannedImage(ImageVariantType.Rectangular, BaseImageUrl + images.First() + ".jpg"));
    //        if (images.Count > 1)
    //            scanData.AddImage(new ScannedImage(ImageVariantType.Scene, BaseImageUrl + images.Last() + ".jpg"));

    //        var sizes = detailsPage.QuerySelectorAll(".size").ToList();
    //        var infos = detailsPage.QuerySelectorAll(".info").ToList();
    //        var variants = new List<ScanData>();
    //        for (int i = 0; i < sizes.Count; i++)
    //        {
    //            var size = sizes[i];
    //            var info = infos[i];

    //            var variant = new ScanData();

    //            variant[ScanField.Size] = size.InnerText.Trim().Split('$').First().Replace("&#39;", "'").Replace("&quot;", "\"")
    //                .Replace("Square", "").Replace("X", "x").Trim();
    //            variant[ScanField.StockCount] = info.GetFieldValue(".availability");
    //            variant[ScanField.LeadTime] = info.GetFieldValue(".eta");
    //            variant[ScanField.SKU] = size.QuerySelector("input").Attributes["value"].Value;

    //            if (variants.Any(x => x[ScanField.Size] == variant[ScanField.Size]))
    //                continue;
    //            variants.Add(variant);
    //        }
    //        scanData.Variants = variants;
    //        return new List<ScanData> { scanData };
    //    }
    //}
}
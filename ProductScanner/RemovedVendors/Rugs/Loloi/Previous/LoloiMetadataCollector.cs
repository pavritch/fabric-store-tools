namespace Loloi.Previous
{
    //public class LoloiMetadataCollector : IMetadataCollector<LoloiVendor>
    //{
    //    private const string SearchUrl = "http://www.loloirugs.com/product/rugs";
    //    private const string ColorFilterUrl = "http://www.loloirugs.com/product/FilterResults?styles=&collections=&sizes=&colors={0}&constructions=&materials=&SortOrder=&ShippingNow=undefined&_=1444699396547";
    //    private const string StyleFilterUrl = "http://www.loloirugs.com/product/FilterResults?styles={0}&collections=&sizes=&colors=&constructions=&materials=&SortOrder=&ShippingNow=undefined&_=1444700527655";
    //    private readonly IProductFileLoader<LoloiVendor> _fileLoader; 
    //    private readonly IPageFetcher<LoloiVendor> _pageFetcher;
    //    private readonly NameValueCollection _headers = new NameValueCollection();

    //    public LoloiMetadataCollector(IProductFileLoader<LoloiVendor> fileLoader, IPageFetcher<LoloiVendor> pageFetcher)
    //    {
    //        _fileLoader = fileLoader;
    //        _pageFetcher = pageFetcher;
    //        _headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
    //    }

    //    public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
    //    {
    //        var priceData = await _fileLoader.LoadProductsAsync();
    //        foreach (var product in products)
    //        {
    //            var collection = product[ScanField.Collection].Replace(" Collection", "").ToLower();
    //            var matches = priceData.Where(x => x[ScanField.Collection].ToLower() == collection).ToList();
    //            foreach (var variant in product.Variants)
    //            {
    //                var sizeMatch = matches.SingleOrDefault(x => x[ScanField.Size] == 
    //                    variant[ScanField.Size].Replace("'-", ".").Replace("\"", ""));
    //                if (sizeMatch != null)
    //                    variant[ScanField.RetailPrice] = sizeMatch[ScanField.RetailPrice];
    //            }
    //        }

    //        var metadata = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all", headers: _headers);
    //        var colorList = metadata.QuerySelectorAll("#UDF5 a").Select(x => x.InnerText).ToList();
    //        await AddMetadata(colorList, ColorFilterUrl, products, ScanField.ColorGroup);

    //        var styleList = metadata.QuerySelectorAll("#UDF19 a").Select(x => x.InnerText).ToList();
    //        await AddMetadata(styleList, StyleFilterUrl, products, ScanField.Style);
    //        return products;
    //    }

    //    private async Task AddMetadata(List<string> items, string url, List<ScanData> products, ScanField field)
    //    {
    //        foreach (var item in items)
    //        {
    //            var filteredListUrl = string.Format(url, item);
    //            var filteredListPage = await _pageFetcher.FetchAsync(filteredListUrl, CacheFolder.Search, item, headers: _headers);
    //            var urls = filteredListPage.QuerySelectorAll("article a.coll").Select(x => x.Attributes["href"].Value).ToList();
    //            var mpns = urls.Select(x => x.Replace("/product/details/", "")).Select(x => x.Remove(x.Length - 4)).ToList();
    //            var matches = products.Where(x => mpns.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
    //            matches.ForEach(x => x[field] = item);
    //        }
    //    }
    //}
}
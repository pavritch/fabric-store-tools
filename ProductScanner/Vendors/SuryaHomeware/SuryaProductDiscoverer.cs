using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SuryaHomeware
{
    public class SuryaHomewareMetadataCollector : IMetadataCollector<SuryaHomewareVendor>
    {
        private const string SearchUrl = "http://surya.com/pillows-throws/";
        private readonly IPageFetcher<SuryaHomewareVendor> _pageFetcher;

        public SuryaHomewareMetadataCollector(IPageFetcher<SuryaHomewareVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");

            await SearchMetadata(products, searchPage, "#p_lt_ctl06_pageplaceholder_p_lt_ctl01_Filter_filterControl_ddlStyleGroup option", "stylegroup_id", ScanField.Style);
            await SearchMetadata(products, searchPage, "#p_lt_ctl06_pageplaceholder_p_lt_ctl01_Filter_filterControl_ddlTrend option", "trend", ScanField.Style2);
            // I'm not sure how to figure out the color ids for the URL - I don't see them in the html
            //SearchMetadata(products, searchPage, "#p_lt_ctl06_pageplaceholder_p_lt_ctl01_Filter_filterControl_chkListColors option", "color_id", ScanField.ColorGroup);

            return products;
        }

        private async Task SearchMetadata(List<ScanData> products, HtmlNode searchPage, string searchKey, string urlKey, ScanField field)
        {
            var options = searchPage.QuerySelectorAll(searchKey).ToList();
            foreach (var option in options.Skip(1))
            {
                var id = option.Attributes[0].Value;
                var name = option.NextSibling.InnerText.Trim();
                var searchUrl = string.Format("http://surya.com/pillows-throws/?isfiltered=1&{0}={1}&n=0", urlKey, id);
                var results = await _pageFetcher.FetchAsync(searchUrl, CacheFolder.Search, name);
                var resultProducts = results.QuerySelectorAll(".product-name a").Select(x => x.InnerText).ToList();
                var matches = products.Where(x => resultProducts.Contains(x[ScanField.PatternNumber])).ToList();
                matches.ForEach(x => x[field] = name);
            }
        }
    }


    public class SuryaHomewareProductDiscoverer : IProductDiscoverer<SuryaHomewareVendor>
    {
        private readonly SuryaPillowFileLoader _pillowFileLoader;
        private readonly SuryaPoufFileLoader _poufFileLoader;
        private readonly SuryaThrowFileLoader _throwFileLoader;

        public SuryaHomewareProductDiscoverer(SuryaPillowFileLoader pillowFileLoader, SuryaPoufFileLoader poufFileLoader, SuryaThrowFileLoader throwFileLoader)
        {
            _pillowFileLoader = pillowFileLoader;
            _poufFileLoader = poufFileLoader;
            _throwFileLoader = throwFileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var pillows = _pillowFileLoader.LoadProducts();
            var poufs = _poufFileLoader.LoadProducts();
            var throws = _throwFileLoader.LoadProducts();
            return Task.FromResult(pillows.Concat(poufs).Concat(throws).Select(x => new DiscoveredProduct(x)).Distinct().ToList());

            //return Task.FromResult(poufs.Select(x => new DiscoveredProduct(x)).Distinct().ToList());
        }
    }
}
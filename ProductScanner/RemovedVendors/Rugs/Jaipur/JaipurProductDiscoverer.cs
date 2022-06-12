using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Jaipur
{
    // NOTE: I could use URLs like this to get everything, where 20 is the number of pages - but the server times out with higher numbers
    // http://www.jaipurrugs.com/back_scroll.aspx?qstr=3295@20@1614@first&productId=0
    public class JaipurProductDiscoverer : IProductDiscoverer<JaipurVendor>
    {
        private const string AllRugsUrl = "https://www.jaipurliving.com/rugs.aspx";
        private const string RugBatchUrl = "https://www.jaipurliving.com/product-back-scroll.aspx?qstr=3295@5@1614@first&productId={0}&producttype=%27Rugs%27,%27Rug%20Swatch%27,%27Ring%20Sets%27";
        private readonly IPageFetcher<JaipurVendor> _pageFetcher;
        private readonly JaipurSearcher _searcher;

        public JaipurProductDiscoverer(IPageFetcher<JaipurVendor> pageFetcher, JaipurSearcher searcher)
        {
            _pageFetcher = pageFetcher;
            _searcher = searcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var firstPage = await _pageFetcher.FetchAsync(AllRugsUrl, CacheFolder.Search, "allRugs");
            var productIds = await _searcher.GetProductIdsAsync(RugBatchUrl, firstPage, "Discovering Product Ids", "allrugs-");
            return productIds.Select(x => new DiscoveredProduct(x.ToString())).ToList();
        }
    }
}

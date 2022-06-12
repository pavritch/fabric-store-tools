using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
    public class ArtAndFrameSourceMetadataCollector : IMetadataCollector<ArtAndFrameSourceVendor>
    {
        private readonly IPageFetcher<ArtAndFrameSourceVendor> _pageFetcher;
        private readonly ArtAndFrameSearcher _searcher;

        public ArtAndFrameSourceMetadataCollector(IPageFetcher<ArtAndFrameSourceVendor> pageFetcher, ArtAndFrameSearcher searcher)
        {
            _pageFetcher = pageFetcher;
            _searcher = searcher;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var subjectsPage = await _pageFetcher.FetchAsync("http://www.artandframesourceinc.com/art-work/subjects", CacheFolder.Search, "subjects");
            var subjectCategories = subjectsPage.QuerySelectorAll("#narrow-by-list2 a").ToList();

            var stylePage = await _pageFetcher.FetchAsync("http://www.artandframesourceinc.com/art-work/style", CacheFolder.Search, "styles");
            var styleCategories = stylePage.QuerySelectorAll("#narrow-by-list2 a").ToList();

            foreach (var category in subjectCategories.Concat(styleCategories))
            {
                var url = category.Attributes["href"].Value;
                var name = category.InnerHtml.Trim().Split('<').First().Trim();
                var results = await _searcher.SearchSectionAsync(url + "?limit=60", name);

                var matches = products.Where(x => results.Contains(x.GetDetailUrl()));
                matches.ForEach(x => x[ScanField.Category] = name);
            }
            return products;
        }
    }
}
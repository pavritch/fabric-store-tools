using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
    public class ArtAndFrameSourceProductDiscoverer : IProductDiscoverer<ArtAndFrameSourceVendor>
    {
        private readonly ArtAndFrameSearcher _searcher;

        public ArtAndFrameSourceProductDiscoverer(ArtAndFrameSearcher searcher)
        {
            _searcher = searcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var artWork = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/art-work?limit=60", "Art Work");
            var marketIntro = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/art-work/market-introductions?limit=60", "Market Introductions");
            var subjects = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/art-work/subjects?limit=60", "Subjects");
            var style = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/art-work/style?limit=60", "Style");
            var canvas = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/art-work/canvas?limit=60", "Canvas");

            //var frames = await _searcher.SearchSectionAsync("http://www.artandframesourceinc.com/frames?limit=60", "Frames");
            //return frames.Select(x => new DiscoveredProduct(new Uri(x))).ToList();

            var all = artWork.Concat(marketIntro).Concat(subjects).Concat(style).Concat(canvas);
            all = all.DistinctBy(x => new Uri(x).GetDocumentName());
            return all.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }

    }
}
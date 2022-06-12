using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Jaipur
{
    // handles the logic for searching and paging
    public class JaipurSearcher
    {
        private readonly IVendorScanSessionManager<JaipurVendor> _sessionManager;
        private readonly IPageFetcher<JaipurVendor> _pageFetcher;

        public JaipurSearcher(IVendorScanSessionManager<JaipurVendor> sessionManager, IPageFetcher<JaipurVendor> pageFetcher)
        {
            _sessionManager = sessionManager;
            _pageFetcher = pageFetcher;
        }

        private int GetNumPages(HtmlNode page)
        {
            // showing 32 per page
            var totalResults = Convert.ToInt32(page.InnerText.CaptureWithinMatchedPattern(@"Search Results:\s+(?<capture>(\d+))"));
            return totalResults/32 + 1;
        }

        public async Task<List<string>> GetProductIdsAsync(string searchUrl, HtmlNode firstPage, string taskName, string prefix)
        {
            var numPages = GetNumPages(firstPage);
            var numIterations = numPages/5 + 1;
            var ids = new List<string>();
            var lastId = 0;
            await _sessionManager.ForEachNotifyAsync(taskName, Enumerable.Range(1, numIterations), async i =>
            {
                var url = string.Format(searchUrl, lastId);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, prefix + i);
                var products = page.QuerySelectorAll(".wrdLatest").ToList();
                if (!products.Any()) return;

                ids.AddRange(products.Select(x => x.InnerHtml.CaptureWithinMatchedPattern(@"Code=(?<capture>([a-zA-Z0-9]*))")));
                lastId = Convert.ToInt32(products.Last().Attributes["id"].Value);
            });
            return ids;
        }

        private string CreateColorSearchUrl(string color)
        {
            return string.Format("http://www.jaipurliving.com/scroll.aspx?qstr=where GroundColorFamily IN ('{0}') AND ProductType in ('Rugs') @3295@0@78&productId={{0}}", color);
        }

        private string CreatePatternSearchUrl(string pattern)
        {
            return string.Format("http://www.jaipurliving.com/scroll.aspx?qstr=where Pattern IN ( '{0}' ) AND ProductType in ('Rugs') @3295@0@78&productId={{0}}", pattern);
        }

        private string CreateDesignerSearchUrl(string designer)
        {
            return string.Format("http://www.jaipurliving.com/scroll.aspx?qstr=where Designer IN ( '{0}' ) AND ProductType in ('Rugs') @3295@0@78&productId={{0}}", designer);
        }

        public async Task<List<string>> RunColorSearch(string color)
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format("http://www.jaipurliving.com/rugs.aspx?p5={0}", color), CacheFolder.Search, color);
            var url = CreateColorSearchUrl(color);
            return await GetProductIdsAsync(url, firstPage, "Scanning color: " + color, color + "-");
        }

        public async Task<List<string>> RunPatternSearch(string pattern)
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format("http://www.jaipurliving.com/rugs.aspx?p8={0}", pattern.Replace(" ", "_")), CacheFolder.Search, pattern);
            var url = CreatePatternSearchUrl(pattern);
            return await GetProductIdsAsync(url, firstPage, "Scanning pattern: " + pattern, pattern + "-");
        }

        public async Task<List<string>> RunDesignerSearch(string designer)
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format("http://www.jaipurliving.com/rugs.aspx?p10={0}", designer.Replace(" ", "_")), CacheFolder.Search, designer);
            var url = CreateDesignerSearchUrl(designer);
            return await GetProductIdsAsync(url, firstPage, "Scanning designer: " + designer, designer + "-");
        }
    }
}
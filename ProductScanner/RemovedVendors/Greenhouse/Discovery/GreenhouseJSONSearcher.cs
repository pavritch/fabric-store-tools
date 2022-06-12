using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Greenhouse.Discovery
{
    public class GreenhouseJSONSearcher
    {
        private const string SearchUrl = "https://www.greenhousefabrics.com/fabrics/ajax?start={0}{1}";
        private readonly IPageFetcher<GreenhouseVendor> _pageFetcher;

        public GreenhouseJSONSearcher(IPageFetcher<GreenhouseVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<string>> Search(string searchKey = "")
        {
            var mpns = new List<string>();
            var start = 0;
            while (true)
            {
                var doc = await _pageFetcher.FetchAsync(string.Format(SearchUrl, start, searchKey), CacheFolder.Search, searchKey + start);
                var deserialized = JsonConvert.DeserializeObject<GreenhouseJSONObject>(doc.InnerText);
                // add a failsafe error out in case we loop forever
                if (deserialized.Fabrics == string.Empty || deserialized.Fabrics.Contains("No results") || start > 20000)
                    break;

                var page = new HtmlDocument();
                page.LoadHtml(deserialized.Fabrics);

                var root = page.DocumentNode;
                var fabrics = root.QuerySelectorAll("a");
                mpns.AddRange(fabrics.Select(x => StringExtensions.CaptureWithinMatchedPattern(x.Attributes["href"].Value.Replace("%C3%A9", "e"), "/fabric/(?<capture>(.*))$")).Distinct());

                start += 100;
            }
            return mpns.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
    }
}
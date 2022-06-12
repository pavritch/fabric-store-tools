using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Capel.Discovery
{
    public class CapelProductDiscoverer : IProductDiscoverer<CapelVendor>
    {
        private readonly IPageFetcher<CapelVendor> _pageFetcher;
        private const string SearchUrl = "https://www.capelrugs.com/SearchResult.aspx";

        public CapelProductDiscoverer(IPageFetcher<CapelVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            // not getting anywhere with this - not sure what else to try
            var initialPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-1");

            var testVals = new NameValueCollection();
            testVals.Add("__EVENTTARGET", "SearchCategoryResult4$PageSizectl$dlPageSize");
            testVals.Add("__EVENTARGUMENT", "9999");
            testVals.Add("__LASTFOCUS", "");
            testVals.Add("__VIEWSTATE", "");
            var testPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-test", testVals);

            var allProductValues = new NameValueCollection();
            allProductValues.Add("__EVENTTARGET", "");
            allProductValues.Add("__EVENTARGUMENT", "");
            allProductValues.Add("__LASTFOCUS", "");
            allProductValues.Add("__VIEWSTATE", "");
            allProductValues.Add("__EVENTVALIDATION", initialPage.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value);

            allProductValues.Add("enterkey", "");
            allProductValues.Add("TopSection1$dealerlocator", "zip code");
            allProductValues.Add("leftbannerCTL$keywordTXT", "Keyword / Style No. Search...");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl00$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl01$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl02$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl03$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl04$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl05$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl06$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$rptSearchInit$ctl07$searchDropDown", "");
            allProductValues.Add("leftbannerCTL$DealerRedirect$zipcode", "enter your zip code...");
            allProductValues.Add("SearchCategoryResult4$PageSizectl$dlPageSize", "9999");
            allProductValues.Add("SearchCategoryResult4$Pagingctrl1$imgNext.x", "15");
            allProductValues.Add("SearchCategoryResult4$Pagingctrl1$imgNext.y", "-1");

            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl00$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl00$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl00$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl00$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl01$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl01$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl01$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl01$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl02$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl02$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl02$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl02$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl03$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl03$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl03$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl03$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl04$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl04$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl04$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl04$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl05$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl05$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl05$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl05$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl06$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl06$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl06$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl06$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl07$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl07$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl07$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl07$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl08$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl08$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl08$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl08$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl09$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl09$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl09$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl09$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl10$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl10$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl10$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl10$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl11$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl11$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl11$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl11$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl12$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl12$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl12$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl12$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl13$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl13$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl13$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl13$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl14$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl14$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl14$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl14$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl15$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl15$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl15$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl15$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl16$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl16$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl16$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl16$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl17$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl17$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl17$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl17$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl18$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl18$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl18$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl18$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl19$subRepeaterctl$subRepeater$ctl00$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl19$subRepeaterctl$subRepeater$ctl01$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl19$subRepeaterctl$subRepeater$ctl02$altimagetxt", "");
            allProductValues.Add("SearchCategoryResult4$ResultRepeater$ctl19$subRepeaterctl$subRepeater$ctl03$altimagetxt", "");

            var allProductsPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all", allProductValues);
            var allProductsPage2 = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all2", allProductValues);

            return new List<DiscoveredProduct>();
        }
    }
}
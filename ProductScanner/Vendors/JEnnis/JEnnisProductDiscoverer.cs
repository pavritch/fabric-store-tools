using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace JEnnis
{
    public class JEnnisProductDiscoverer : IProductDiscoverer<JEnnisVendor>
    {
        private readonly IPageFetcher<JEnnisVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<JEnnisVendor> _sessionManager; 
        private string SearchUrl = "https://www.jennisfabrics.com/jennis-web-core/loadProduct.jef?marketGroup=HO&productType=FABR";
        private string UrlPrefix = "https://www.jennisfabrics.com/jennis-web-core/";

        private readonly IProductFileLoader<JEnnisVendor> _productFileLoader;

        public JEnnisProductDiscoverer(IPageFetcher<JEnnisVendor> pageFetcher, IVendorScanSessionManager<JEnnisVendor> sessionManager, IProductFileLoader<JEnnisVendor> productFileLoader)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _productFileLoader = productFileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = await _productFileLoader.LoadProductsAsync();
            return products.Select(x => new DiscoveredProduct(x)).ToList();

            var searchOne = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            var searchTwo = await _pageFetcher.FetchAsync("https://www.jennisfabrics.com/jennis-web-core/searchProduct.jef?MarketGroup=HO&ABRASION=select&dropselect1=select&BACKING+CONTENT=select&dropselect2=select&COATING=select&dropselect3=select&COLD+CRACK=select&dropselect4=select&CONSTRUCTION=select&dropselect5=select&CONTENT=select&dropselect6=select&ECOFRIENDLY=select&dropselect7=select&FIRE+RETARDANCY=select&dropselect8=select&RAILROADED=select&dropselect9=select&STYLE=select&dropselect10=select&ULTRAVIOLET=select&dropselect11=select&USES=select&dropselect12=select&BRAND=select&dropselect13=select&totalDrop=13&productName=&collectionList=--+Select+Collections+--&colorHide1=null&colorHide2=null&colorHide3=null&colorHide4=null&colorHide5=null&index=&colorSearchEvent=colorSrDone", CacheFolder.Search, "search2");

            var values = new NameValueCollection();
            values["pageNumber"] = "1";
            values["pageStart"] = "0";
            values["itemsPerPage"] = "999";

            var searchThree = await _pageFetcher.FetchAsync("https://www.jennisfabrics.com/jennis-web-core/iterateProductListpage.jef", CacheFolder.Search, "search-3", values);
            var items = searchThree.QuerySelectorAll("a") .Select(x => new Uri(UrlPrefix + x.Attributes["href"].Value).GetQueryParameter("prdCode")).ToList();

            var codes = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", items, async code =>
            {
                var url = string.Format("https://www.jennisfabrics.com/jennis-web-core/productProfile.jef?prdCode={0}&invType=REG", code);
                var details = await _pageFetcher.FetchAsync(url, CacheFolder.Search, code);
                var colorCodes = details.QuerySelectorAll(".img_bg a").Select(x => new Uri(UrlPrefix + x.Attributes["href"].Value).GetQueryParameter("prdCode")).ToList();

                codes.AddRange(colorCodes);
                codes.Add(code);
            });

            return codes.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}
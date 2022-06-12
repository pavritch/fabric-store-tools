using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;

namespace BlueMountain.Discovery
{
    public class BlueMountainProductDiscoverer : IProductDiscoverer<BlueMountainVendor>
    {
        private const string SitemapUrl = "http://www.designbycolor.net/sitemap.xml";
        private const string BookUrl = "http://www.designbycolor.net/en/Living/book/{0}/{1}/default.aspx";
        private readonly IPageFetcher<BlueMountainVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<BlueMountainVendor> _sessionManager;

        public BlueMountainProductDiscoverer(IPageFetcher<BlueMountainVendor> pageFetcher, IVendorScanSessionManager<BlueMountainVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            return await DiscoverProducts(await FindBookIds());
        }

        private async Task<List<BlueMountainBookInfo>> FindBookIds()
        {
            // find book ids from the sitemap
            var page = await _pageFetcher.FetchAsync(SitemapUrl, CacheFolder.Search, "sitemap");

            var urls = page.QuerySelectorAll("loc").Select(x => x.InnerText).ToList();
            var idAndColor = urls.Where(x => x.Contains("book")).Select(x => x.CaptureWithinMatchedPattern("book/(?<capture>(.*))/default.aspx"));
            return idAndColor.Select(x => new BlueMountainBookInfo(x.Split(new[] {'/'}).Last(), Convert.ToInt32(x.Split(new[] {'/'}).First()))).ToList();
        }

        private async Task<List<DiscoveredProduct>> DiscoverProducts(IEnumerable<BlueMountainBookInfo> bookIds)
        {
            var discoveredProducts = new List<DiscoveredProduct>();
            await _sessionManager.ForEachNotifyAsync("Scanning Books", bookIds, async book =>
            {
                var url = string.Format(BookUrl, book.ColorId, book.Color);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "book-" + book.ColorId);

                // find out how many groups there are and execute the postback for each of them
                var linkSections = page.QuerySelectorAll(".linksection").Count();
                for (var i = 0; i < linkSections; i++)
                {
                    var linkId = string.Format("ctl00$ContentPlaceHolder1$rptBook$ctl0{0}$lnkOpenBook", i);

                    var postValues = CreatePostValuesForBookSection(page, linkId);
                    page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, string.Format("book-{0}-{1}", book.ColorId, i), postValues);
                }

                // the last one we downloaded for each book should have all products shown
                for (var i = 0; i < linkSections; i++)
                {
                    var linkId = string.Format("#ctl00_ContentPlaceHolder1_rptBook_ctl{0}_lnkOpenBook", i.ToString().PadLeft(2, '0'));
                    var sectionId = string.Format("#ctl00_ContentPlaceHolder1_rptBook_ctl{0}_panelBook", i.ToString().PadLeft(2, '0'));

                    var headingText = page.QuerySelector(linkId).InnerText;
                    var patterns = page.QuerySelectorAll(string.Format("{0} .PatternBoxDefault > img", sectionId))
                        .Select(x => x.Attributes["onclick"].Value).ToList();

                    // each of these looks like: clickInfo('BC1581860','PT018694');
                    var mpns = patterns.Select(x => x.CaptureWithinMatchedPattern(@"clickInfo\('(?<capture>(\w+))'")).ToList();
                    var imageIds = patterns.Select(x => x.CaptureWithinMatchedPattern(@"clickInfo\('\w+','(?<capture>(\w+))'\);")).ToList();
                    for (var productCtr = 0; productCtr < mpns.Count(); productCtr++)
                    {
                        var mpn = mpns[productCtr];
                        if (!discoveredProducts.Any(x => x.GetMPN() == mpn))
                        {
                            discoveredProducts.Add(CreateDiscoveredProduct(mpn, imageIds[productCtr], book.Color, headingText));
                        }
                    }
                }
            });
            return discoveredProducts.ToList();
        }

        private NameValueCollection CreatePostValuesForBookSection(HtmlNode page, string linkId)
        {
            var viewstate = page.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var values = new NameValueCollection();
            values.Add("__EVENTTARGET", linkId);
            values.Add("__EVENTARGUMENT", string.Empty);
            values.Add("__LASTFOCUS", string.Empty);
            values.Add("__VIEWSTATE", viewstate);
            return values;
        }

        private DiscoveredProduct CreateDiscoveredProduct(string productId, string imageId, string color, string heading)
        {
            color = color.Replace("%20", " ");
            var webMetadata = new ScanData();
            webMetadata[ProductPropertyType.ManufacturerPartNumber] = productId;
            webMetadata[ProductPropertyType.ColorGroup] = color.Trim();
            webMetadata[ProductPropertyType.Category] = heading.Replace(color, "").Trim();
            webMetadata[ProductPropertyType.ItemNumber] = productId;
            webMetadata[ProductPropertyType.ImageUrl] = string.Format("http://www.designbycolor.net/showimage.aspx?img={0}.jpg", imageId);
            return new DiscoveredProduct(webMetadata);
        }
    }
}
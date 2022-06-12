using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace York.Details
{
    public class YorkProductScraper : ProductScraper<YorkVendor>
    {
        private const string PriceCheckUrl = "http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WINPRCK1+webrtn=INPRCK1+ml=LANSA:XHTML+partition=YWP+language=ENG+sid=";
        private readonly Dictionary<string, ScanField> _detailFields = new Dictionary<string, ScanField>
        {
            { "Pattern #", ScanField.Ignore },
            { "Pattern Name", ScanField.PatternName },
            { "Collection", ScanField.Collection },
            { "Features", ScanField.Bullet5 },
            { "Match Type", ScanField.Match },
            { "Pattern Repeat", ScanField.Repeat },
            { "Total Number Available", ScanField.StockCount },
            { "Special Effects", ScanField.Bullet6 },
            { "Back Order Date", ScanField.Ignore },
            { "Border Height", ScanField.BorderHeight },
            { "Roll Dimensions (Spool)", ScanField.Ignore },
            { "Roll Dimensions (Each)", ScanField.Ignore },
            { "Roll Dimensions (Unknown)", ScanField.Ignore },
            { "Roll Dimensions (Yard)", ScanField.Ignore },
            { "Roll Dimensions (Double Roll)", ScanField.Ignore },
            { "MSRP (Each)", ScanField.Ignore },
            { "MSRP (Unknown)", ScanField.Ignore },
            { "MSRP (Yard)", ScanField.Ignore },
            { "MSRP (Single Roll)", ScanField.Ignore },
            { "MSRP (Double Roll)", ScanField.Ignore },
            { "MSRP (Spool)", ScanField.Ignore },
            { "Also available wide width - Details", ScanField.Ignore },
            { "Also available narrow width - Details", ScanField.Ignore },
            { "This design is exclusive to our international partner. They are not available in the US.", ScanField.Ignore },
        };

        public YorkProductScraper(IPageFetcher<YorkVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var mpn = context.MPN;
            var url = context.DetailUrl.AbsoluteUri;
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
            if (!page.InnerHtml.ContainsIgnoreCase("Welcome, TESSA"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, mpn);
                throw new AuthenticationException();
            }

            var cost = await GetCost(page, mpn);

            var product = CreateProduct(context.ScanData, mpn, page);
            if (product == null) return new List<ScanData>();

            product.Cost = cost;
            return new List<ScanData> {product};
        }

        protected ScanData CreateProduct(ScanData discoveredData, string mpn, HtmlNode docNode)
        {
            var product = new ScanData(discoveredData);

            var detailNodes = docNode.QuerySelectorAll("ul[id='prdAdvancedDetails'] li strong").Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).ToList();
            foreach (var detailNode in detailNodes)
            {
                var name = detailNode.InnerText.Replace(":", "").Trim();
                var value = GetDetailValue(detailNode);
                var field = _detailFields[name];
                product[field] = value;

                if (name.ContainsIgnoreCase("Dimensions")) product[ScanField.Dimensions] = value;
                if (name.ContainsIgnoreCase("MSRP")) product[ScanField.RetailPrice] = value;
            }

            var allKeys = detailNodes.Select(x => x.InnerText.Replace(":", "").Trim()).ToList();
            var isSpool = allKeys.Any(e => e.ContainsIgnoreCase("(Spool)"));
            var isEach = allKeys.Any(e => e.ContainsIgnoreCase("(Each)"));
            var isYard = allKeys.Any(e => e.ContainsIgnoreCase("(Yard)"));
            var isRoll = allKeys.Any(e => e.ContainsIgnoreCase(" Roll)"));
            if (isSpool || isEach)
                product[ScanField.UnitOfMeasure] = "Each";
            else if (isYard)
                product[ScanField.UnitOfMeasure] = "Yard";
            else if (isRoll)
                product[ScanField.UnitOfMeasure] = "Roll";
            // seems like 'unknown' ones are all 'Roll'
            else
                product[ScanField.UnitOfMeasure] = "Roll";
            return product;
        }

        private string GetDetailValue(HtmlNode detailNode)
        {
            var spans = detailNode.ParentNode.QuerySelectorAll("span").ToList();

            if (spans.Count > 0)
            {
                // take inner text of first span (seems to be the one with US measurements
                return spans.First().InnerText.TrimToNull();
            }
            // simple plain text value after the strong tag
            return detailNode.ParentNode.InnerHtml.CaptureWithinMatchedPattern(@"</strong>(?<capture>((.*)))").TrimToNull();
        }

        private async Task<decimal> GetCost(HtmlNode detailsPage, string mpn)
        {
            var namedElements = new[]
            {
                "STDWEBUSR",
                "STDSESSID",
                "STDWEBC01",
                "STDWEBC02",
            };

            var values = GetCostPageValues();
            values.Add("LW3LISTV2.0001.WIITM#", mpn);
            foreach (var item in detailsPage.OwnerDocument.GetFormPostValuesByName(namedElements))
                values.Add(item.Key, item.Value);

            var pricePage = await PageFetcher.FetchAsync(PriceCheckUrl, CacheFolder.Price, mpn, values);
            if (pricePage.InnerText.ContainsIgnoreCase("You are not authorized to this item"))
                return 0;

            var priceElement = pricePage.QuerySelector("input[name='LW3LISTV2.0001.IIPRCE']");
            if (priceElement == null) return 0;

            var priceValue = priceElement.Attributes["value"].Value;
            return priceValue.ToDecimalSafe();
        }

        private NameValueCollection GetCostPageValues()
        {
            var values = new NameValueCollection();
            values.Add("STDTABFLR", "");
            values.Add("STDRENTRY", "V");
            values.Add("LW3SITTOT", ".00");
            values.Add("LW3SITCNT", "0");
            values.Add("LW3EASTAT", "");
            values.Add("LW3CUSIND", "");
            values.Add("STDLISTID", "");
            values.Add("STDUSERST", "");
            values.Add("STDCUSIND", "B");
            values.Add("STD_ADLIN", "");
            values.Add("STDUSRTYP", "");
            values.Add("LW3GENLN", "0000000");
            values.Add("LW3GENPT", "");
            values.Add("LW3PAGPOS", "000");
            values.Add("LW3LSTPOS", "");
            values.Add("LW3SELVEW", "");
            values.Add("LW3TOPRRN", "000000000000000");
            values.Add("LW3BTMRRN", "000000000000000");
            values.Add("LW3PROCID", "");
            values.Add("LW3VARFLD", "");
            values.Add("LW3VERSEQ", "");
            values.Add("LW3PAGE", "10");
            values.Add("LW3TCOUNT", "1");
            values.Add("LW3PREV", "");
            values.Add("LW3MORE", "");
            values.Add("STDROWNUM", "1");
            values.Add("LW3MSGCNT", "01");
            values.Add("LW3ERRCNT", "00");
            values.Add("LW3PRDREF", "");
            values.Add("LW3SHOW", "N");
            values.Add("LWBOD53", "WINPRCK");
            values.Add("LW3VNDNME", "");
            values.Add("LW3SHOWME", "Y");
            values.Add("LW3RBTN01", "V");
            values.Add("WW3SUBSIT", "YORKWALL");
            values.Add("WW3WEBPRC", "Y");
            values.Add("_SERVICENAME", "WINPRCK1_INPRCK1");
            values.Add("_WEBAPP", "WINPRCK1");
            values.Add("_WEBROUTINE", "INPRCK1");
            values.Add("_PARTITION", "YWP");
            values.Add("_LANGUAGE", "ENG");
            values.Add("_LW3TRCID", "false");
            values.Add("LW3LISTV2..", "1");
            values.Add("LW3LISTV2.0001.LW3LINES", "01");
            values.Add("LW3LISTV2.0001.IIDESC", "");
            values.Add("LW3LISTV2.0001.LW3QTYORD", "");
            values.Add("LW3LISTV2.0001.IIPRCE", ".000");
            values.Add("LW3LISTV2.0001.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0001.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0001.LW3ERROR", "");
            values.Add("LW3LISTV2.0001.IMLITM", "");
            values.Add("LW3LISTV2.0002.LW3LINES", "02");
            values.Add("LW3LISTV2.0002.WIITM#", "");
            values.Add("LW3LISTV2.0002.IIDESC", "");
            values.Add("LW3LISTV2.0002.LW3QTYORD", "");
            values.Add("LW3LISTV2.0002.IIPRCE", ".000");
            values.Add("LW3LISTV2.0002.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0002.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0002.LW3ERROR", "");
            values.Add("LW3LISTV2.0002.IMLITM", "");
            values.Add("LW3LISTV2.0003.LW3LINES", "03");
            values.Add("LW3LISTV2.0003.WIITM#", "");
            values.Add("LW3LISTV2.0003.IIDESC", "");
            values.Add("LW3LISTV2.0003.LW3QTYORD", "");
            values.Add("LW3LISTV2.0003.IIPRCE", ".000");
            values.Add("LW3LISTV2.0003.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0003.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0003.LW3ERROR", "");
            values.Add("LW3LISTV2.0003.IMLITM", "");
            values.Add("LW3LISTV2.0004.LW3LINES", "04");
            values.Add("LW3LISTV2.0004.WIITM#", "");
            values.Add("LW3LISTV2.0004.IIDESC", "");
            values.Add("LW3LISTV2.0004.LW3QTYORD", "");
            values.Add("LW3LISTV2.0004.IIPRCE", ".000");
            values.Add("LW3LISTV2.0004.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0004.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0004.LW3ERROR", "");
            values.Add("LW3LISTV2.0004.IMLITM", "");
            values.Add("LW3LISTV2.0005.LW3LINES", "05");
            values.Add("LW3LISTV2.0005.WIITM#", "");
            values.Add("LW3LISTV2.0005.IIDESC", "");
            values.Add("LW3LISTV2.0005.LW3QTYORD", "");
            values.Add("LW3LISTV2.0005.IIPRCE", ".000");
            values.Add("LW3LISTV2.0005.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0005.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0005.LW3ERROR", "");
            values.Add("LW3LISTV2.0005.IMLITM", "");
            values.Add("LW3LISTV2.0006.LW3LINES", "06");
            values.Add("LW3LISTV2.0006.WIITM#", "");
            values.Add("LW3LISTV2.0006.IIDESC", "");
            values.Add("LW3LISTV2.0006.LW3QTYORD", "");
            values.Add("LW3LISTV2.0006.IIPRCE", ".000");
            values.Add("LW3LISTV2.0006.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0006.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0006.LW3ERROR", "");
            values.Add("LW3LISTV2.0006.IMLITM", "");
            values.Add("LW3LISTV2.0007.LW3LINES", "07");
            values.Add("LW3LISTV2.0007.WIITM#", "");
            values.Add("LW3LISTV2.0007.IIDESC", "");
            values.Add("LW3LISTV2.0007.LW3QTYORD", "");
            values.Add("LW3LISTV2.0007.IIPRCE", ".000");
            values.Add("LW3LISTV2.0007.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0007.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0007.LW3ERROR", "");
            values.Add("LW3LISTV2.0007.IMLITM", "");
            values.Add("LW3LISTV2.0008.LW3LINES", "08");
            values.Add("LW3LISTV2.0008.WIITM#", "");
            values.Add("LW3LISTV2.0008.IIDESC", "");
            values.Add("LW3LISTV2.0008.LW3QTYORD", "");
            values.Add("LW3LISTV2.0008.IIPRCE", ".000");
            values.Add("LW3LISTV2.0008.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0008.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0008.LW3ERROR", "");
            values.Add("LW3LISTV2.0008.IMLITM", "");
            values.Add("LW3LISTV2.0009.LW3LINES", "09");
            values.Add("LW3LISTV2.0009.WIITM#", "");
            values.Add("LW3LISTV2.0009.IIDESC", "");
            values.Add("LW3LISTV2.0009.LW3QTYORD", "");
            values.Add("LW3LISTV2.0009.IIPRCE", ".000");
            values.Add("LW3LISTV2.0009.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0009.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0009.LW3ERROR", "");
            values.Add("LW3LISTV2.0009.IMLITM", "");
            values.Add("LW3LISTV2.0010.LW3LINES", "10");
            values.Add("LW3LISTV2.0010.WIITM#", "");
            values.Add("LW3LISTV2.0010.IIDESC", "");
            values.Add("LW3LISTV2.0010.LW3QTYORD", "");
            values.Add("LW3LISTV2.0010.IIPRCE", ".000");
            values.Add("LW3LISTV2.0010.WW3WPRICE", ".000");
            values.Add("LW3LISTV2.0010.LW3LINTOT", ".00");
            values.Add("LW3LISTV2.0010.LW3ERROR", "");
            values.Add("LW3LISTV2.0010.IMLITM", "");
            values.Add("LW3LISTV..", "1");
            values.Add("LW3LISTV.0001.LW3LINES", "01");
            values.Add("LW3LISTV.0001.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0001.LW3ERROR", "");
            values.Add("LW3LISTV.0001.WIITM#", "");
            values.Add("LW3LISTV.0001.IIDESC", "");
            values.Add("LW3LISTV.0001.IIPRCE", ".000");
            values.Add("LW3LISTV.0001.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0001.IMLITM", "");
            values.Add("LW3LISTV.0002.LW3LINES", "02");
            values.Add("LW3LISTV.0002.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0002.LW3ERROR", "");
            values.Add("LW3LISTV.0002.WIITM#", "");
            values.Add("LW3LISTV.0002.IIDESC", "");
            values.Add("LW3LISTV.0002.IIPRCE", ".000");
            values.Add("LW3LISTV.0002.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0002.IMLITM", "");
            values.Add("LW3LISTV.0003.LW3LINES", "03");
            values.Add("LW3LISTV.0003.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0003.LW3ERROR", "");
            values.Add("LW3LISTV.0003.WIITM#", "");
            values.Add("LW3LISTV.0003.IIDESC", "");
            values.Add("LW3LISTV.0003.IIPRCE", ".000");
            values.Add("LW3LISTV.0003.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0003.IMLITM", "");
            values.Add("LW3LISTV.0004.LW3LINES", "04");
            values.Add("LW3LISTV.0004.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0004.LW3ERROR", "");
            values.Add("LW3LISTV.0004.WIITM#", "");
            values.Add("LW3LISTV.0004.IIDESC", "");
            values.Add("LW3LISTV.0004.IIPRCE", ".000");
            values.Add("LW3LISTV.0004.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0004.IMLITM", "");
            values.Add("LW3LISTV.0005.LW3LINES", "05");
            values.Add("LW3LISTV.0005.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0005.LW3ERROR", "");
            values.Add("LW3LISTV.0005.WIITM#", "");
            values.Add("LW3LISTV.0005.IIDESC", "");
            values.Add("LW3LISTV.0005.IIPRCE", ".000");
            values.Add("LW3LISTV.0005.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0005.IMLITM", "");
            values.Add("LW3LISTV.0006.LW3LINES", "06");
            values.Add("LW3LISTV.0006.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0006.LW3ERROR", "");
            values.Add("LW3LISTV.0006.WIITM#", "");
            values.Add("LW3LISTV.0006.IIDESC", "");
            values.Add("LW3LISTV.0006.IIPRCE", ".000");
            values.Add("LW3LISTV.0006.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0006.IMLITM", "");
            values.Add("LW3LISTV.0007.LW3LINES", "07");
            values.Add("LW3LISTV.0007.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0007.LW3ERROR", "");
            values.Add("LW3LISTV.0007.WIITM#", "");
            values.Add("LW3LISTV.0007.IIDESC", "");
            values.Add("LW3LISTV.0007.IIPRCE", ".000");
            values.Add("LW3LISTV.0007.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0007.IMLITM", "");
            values.Add("LW3LISTV.0008.LW3LINES", "08");
            values.Add("LW3LISTV.0008.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0008.LW3ERROR", "");
            values.Add("LW3LISTV.0008.WIITM#", "");
            values.Add("LW3LISTV.0008.IIDESC", "");
            values.Add("LW3LISTV.0008.IIPRCE", ".000");
            values.Add("LW3LISTV.0008.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0008.IMLITM", "");
            values.Add("LW3LISTV.0009.LW3LINES", "09");
            values.Add("LW3LISTV.0009.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0009.LW3ERROR", "");
            values.Add("LW3LISTV.0009.WIITM#", "");
            values.Add("LW3LISTV.0009.IIDESC", "");
            values.Add("LW3LISTV.0009.IIPRCE", ".000");
            values.Add("LW3LISTV.0009.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0009.IMLITM", "");
            values.Add("LW3LISTV.0010.LW3LINES", "10");
            values.Add("LW3LISTV.0010.WW3TOTAVL", "0");
            values.Add("LW3LISTV.0010.LW3ERROR", "");
            values.Add("LW3LISTV.0010.WIITM#", "");
            values.Add("LW3LISTV.0010.IIDESC", "");
            values.Add("LW3LISTV.0010.IIPRCE", ".000");
            values.Add("LW3LISTV.0010.LW3LINTOT", ".00");
            values.Add("LW3LISTV.0010.IMLITM", "");
            return values;
        }
    }
}
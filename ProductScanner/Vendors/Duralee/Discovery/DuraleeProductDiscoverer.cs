using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace Duralee.Discovery
{
    // the discovery phase of duralee has the majority of the logic because of how the data is retrieved
    public class BBergerProductDiscoverer : DuraleeBaseProductDiscoverer<BBergerVendor>
    {
        public BBergerProductDiscoverer(IPageFetcher<BBergerVendor> pageFetcher, 
            IVendorScanSessionManager<BBergerVendor> sessionManager)
            : base(pageFetcher, sessionManager) { }
    }

    public class ClarkeAndClarkeProductDiscoverer : DuraleeBaseProductDiscoverer<ClarkeAndClarkeVendor>
    {
        public ClarkeAndClarkeProductDiscoverer(IPageFetcher<ClarkeAndClarkeVendor> pageFetcher, 
            IVendorScanSessionManager<ClarkeAndClarkeVendor> sessionManager)
            : base(pageFetcher, sessionManager) { }
    }

    public class DuraleeProductDiscoverer : DuraleeBaseProductDiscoverer<DuraleeVendor>
    {
        public DuraleeProductDiscoverer(IPageFetcher<DuraleeVendor> pageFetcher,
            IVendorScanSessionManager<DuraleeVendor> sessionManager)
            : base(pageFetcher, sessionManager) { }
    }

    public class HighlandCourtProductDiscoverer : DuraleeBaseProductDiscoverer<HighlandCourtVendor>
    {
        public HighlandCourtProductDiscoverer(IPageFetcher<HighlandCourtVendor> pageFetcher,
            IVendorScanSessionManager<HighlandCourtVendor> sessionManager)
            : base(pageFetcher, sessionManager) { }
    }

    public class DuraleeBaseProductDiscoverer<T> : IProductDiscoverer<T> where T : Vendor, new()
    {
        private const string JsonFabricSearchUrl = "https://api.convermax.com/v3/duralee/search?page={0}&pagesize=63&catalog=fabric&facet.0.Field=BrandName&facet.0.selection={1}&facet.1.Field=Discontinued&facet.1.selection=False&extra.user=VncgFCIfwrZ1ibQYNzYSG%2BTjlqwIrKybIUAu11TumGolhnB3t5WCCdsKYmMyC%2BQ5B1EEM48xZQePp3RLm7EOSA%3D%3D&analytics.userid=9q4NQmlLB9bISRls&analytics.sessionid=802BYi0pLth4QPxs";
        private const string JsonFabricSearchDiscUrl = "https://api.convermax.com/v3/duralee/search?page={0}&pagesize=63&catalog=fabric&facet.0.Field=BrandName&facet.0.selection={1}&facet.1.Field=Discontinued&facet.1.selection=True&extra.user=VncgFCIfwrZ1ibQYNzYSG%2BTjlqwIrKybIUAu11TumGolhnB3t5WCCdsKYmMyC%2BQ5B1EEM48xZQePp3RLm7EOSA%3D%3D&analytics.userid=9q4NQmlLB9bISRls&analytics.sessionid=802BYi0pLth4QPxs";
        private const string JsonTrimSearchUrl = "https://api.convermax.com/v3/duralee/search?page={0}&pagesize=63&catalog=trim&facet.0.Field=BrandName&facet.0.selection={1}&facet.1.Field=Discontinued&facet.1.selection=False&extra.user=VncgFCIfwrZ1ibQYNzYSG%2BTjlqwIrKybIUAu11TumGolhnB3t5WCCdsKYmMyC%2BQ5B1EEM48xZQePp3RLm7EOSA%3D%3D&analytics.userid=9q4NQmlLB9bISRls&analytics.sessionid=802BYi0pLth4QPxs";
        private const string JsonTrimSearchDiscUrl = "https://api.convermax.com/v3/duralee/search?page={0}&pagesize=63&catalog=trim&facet.0.Field=BrandName&facet.0.selection={1}&facet.1.Field=Discontinued&facet.1.selection=True&extra.user=VncgFCIfwrZ1ibQYNzYSG%2BTjlqwIrKybIUAu11TumGolhnB3t5WCCdsKYmMyC%2BQ5B1EEM48xZQePp3RLm7EOSA%3D%3D&analytics.userid=9q4NQmlLB9bISRls&analytics.sessionid=802BYi0pLth4QPxs";
        private readonly IPageFetcher<T> _pageFetcher;
        private readonly string _vendorName;

        public DuraleeBaseProductDiscoverer(IPageFetcher<T> pageFetcher, IVendorScanSessionManager<T> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _vendorName = new T().GetName().ToLower();
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = new List<ScanData>();
            if (_vendorName == "duralee")
            {
                products.AddRange(await SearchProducts("Duralee"));
                products.AddRange(await SearchProducts("Duralee-Contract"));

                products.AddRange(await SearchProducts("Duralee", isDisc:true));
                products.AddRange(await SearchProducts("Duralee-Contract", isDisc:true));

                products.AddRange(await SearchProducts("Duralee", true, isDisc:false));
                products.AddRange(await SearchProducts("Duralee", true, isDisc:true));

                products.AddRange(await SearchProducts("Bailey-Griffin", isDisc:false));
                products.AddRange(await SearchProducts("Bailey-Griffin", isDisc:true));
            }
            if (_vendorName == "bberger")
            {
                products.AddRange(await SearchProducts("B.-Berger", isDisc:false));

                products.AddRange(await SearchProducts("B.-Berger", isDisc:true));
            }
            if (_vendorName == "highlandcourt")
            {
                products.AddRange(await SearchProducts("Highland-Court", isDisc:false));

                products.AddRange(await SearchProducts("Highland-Court", isDisc:true));
            }
            if (_vendorName == "clarkeandclarke")
            {
                products.AddRange(await SearchProducts("Clarke-Clarke", isDisc:false));

                products.AddRange(await SearchProducts("Clarke-Clarke", isDisc:true));
            }
            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<ScanData>> SearchProducts(string selection, bool trim = false, bool isDisc = false)
        {
            var products = new List<ScanData>();
            var pageNum = 0;
            while (true)
            {
                var url = string.Format(JsonFabricSearchUrl, pageNum, selection);
                if (isDisc) url = string.Format(JsonFabricSearchDiscUrl, pageNum, selection);

                if (trim) url = string.Format(JsonTrimSearchUrl, pageNum, selection);
                if (trim && isDisc) url = string.Format(JsonTrimSearchDiscUrl, pageNum, selection);

                var search = (await _pageFetcher.FetchAsync(url, CacheFolder.Search, 
                    selection + "-" + pageNum + (isDisc ? "-disc" : "") + (trim ? "-trim" : ""))).InnerText;

                dynamic productData = JObject.Parse(search);
                int count = productData.TotalHits;
                foreach (var item in productData.Items)
                {
                    if (((JArray)productData.Items).Count < 63) return products;

                    var scanData = new ScanData();

                    scanData.DetailUrl = new Uri("https://www.duralee.com" + item.ItemLink.Value);

                    scanData[ScanField.ManufacturerPartNumber] = item.ItemNumber.Value;
                    scanData[ScanField.Cleaning] = GetValue(item.CareCode);
                    scanData[ScanField.Country] = GetValue(item.Origin);
                    scanData[ScanField.Durability] = GetValue(item.TestCode1);
                    scanData[ScanField.HorizontalRepeat] = item.HorizontalRepeat.Value.ToString();
                    scanData[ScanField.VerticalRepeat] = item.VerticalRepeat.Value.ToString();
                    scanData[ScanField.Weight] = item.Weight.Value.ToString();
                    scanData[ScanField.Width] = item.Width.Value.ToString();
                    scanData[ScanField.ProductGroup] = item.ProductTypeCode.Value.ToString();

                    scanData[ScanField.PatternName] = GetValue(item.MarketingPatternName);
                    scanData[ScanField.PatternNumber] = GetValue(item.PatternCode);
                    scanData[ScanField.ColorName] = GetValue(item.MarketingColorName);
                    scanData[ScanField.ColorNumber] = GetValue(item.ColorCode);
                    scanData[ScanField.ImageUrl] = GetValue(item.ResultImage);

                    products.Add(scanData);
                }
                if (products.Count >= count) return products;
                pageNum++;
            }
        }

        private string GetValue(dynamic value)
        {
            if (value == null) return String.Empty;
            return value.Value.ToString();
        }
    }
}
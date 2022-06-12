using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace TrendLighting
{
    public class TrendSearch
    {
        private const string SearchUrl = "http://www.tlighting.com/retail/filter.html?limit=all&{0}={1}";

        public string Category { get; set; }
        public string SearchValue { get; set; }
        public string UrlKey { get; set; }
        public int UrlValue { get; set; }

        public TrendSearch(string category, string searchValue, string key, int value)
        {
            Category = category;
            SearchValue = searchValue;
            UrlKey = key;
            UrlValue = value;
        }

        public string GetSearchUrl()
        {
            return string.Format(SearchUrl, UrlKey, UrlValue);
        }

        public ScanField GetScanField()
        {
            var fields = new Dictionary<string, ScanField>
            {
                { "Type", ScanField.ProductType },
                { "Category", ScanField.Category },
                { "Application", ScanField.ProductUse },
                { "Finishes/Materials", ScanField.Finish },
                { "Features", ScanField.AdditionalInfo },
            };
            return fields[Category.TitleCase()];
        }
    }
}
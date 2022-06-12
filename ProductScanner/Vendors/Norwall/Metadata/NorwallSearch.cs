using ProductScanner.Core.Scanning.Products.Vendor;

namespace Norwall.Metadata
{
    public class NorwallSearch
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public ScanField AssociatedProperty { get; set; }

        public NorwallSearch(string key, string name, int value, ScanField associatedProperty)
        {
            Key = key;
            Name = name;
            Value = value;
            AssociatedProperty = associatedProperty;
        }

        public string GetUrl(int pageNum)
        {
            var url = "http://www.norwall.net/product_search_result.php?{0}={1}&page={2}";
            return string.Format(url, Key, Value, pageNum);
        }
    }
}
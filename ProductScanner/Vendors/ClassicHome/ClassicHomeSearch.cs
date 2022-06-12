using System;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ClassicHome
{
    public class ClassicHomeSearch
    {
        public Uri SearchUrl { get; set; }
        public ScanField ScanField { get; set; }
        public string Value { get; set; }

        public ClassicHomeSearch(Uri searchUrl, ScanField scanField, string value)
        {
            SearchUrl = searchUrl;
            ScanField = scanField;
            Value = value;
        }
    }
}
using System;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Brewster.Discovery
{
    public class BrewsterExcelField
    {
        public string Header { get; set; }
        public ScanField AssociatedProperty { get; set; }
        public Func<string, string> PostProcessor { get; set; }

        public BrewsterExcelField(string header, ScanField associatedProperty, Func<string, string> postProcessor = null)
        {
            Header = header;
            AssociatedProperty = associatedProperty;
            PostProcessor = postProcessor;
        }
    }
}
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public class FileProperty
    {
        public ScanField Property { get; set; }
        public string Header { get; set; }

        public FileProperty(string header, ScanField property)
        {
            Property = property;
            Header = header;
        }
    }
}
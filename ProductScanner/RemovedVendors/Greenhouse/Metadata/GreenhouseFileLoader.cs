using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Greenhouse.Metadata
{
    public class GreenhouseFileLoader : ProductFileLoader<GreenhouseVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("MPN", ScanField.ManufacturerPartNumber),
            new FileProperty("PATTERN", ScanField.Pattern),
            new FileProperty("COLOR", ScanField.Color),
            new FileProperty("WHOLESALE PRICE", ScanField.Cost),
            new FileProperty("RETAIL PRICE", ScanField.RetailPrice),
            new FileProperty("CONTENTS & FINISH", ScanField.Ignore),
        };

        public GreenhouseFileLoader(IStorageProvider<GreenhouseVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx) { }
    }
}
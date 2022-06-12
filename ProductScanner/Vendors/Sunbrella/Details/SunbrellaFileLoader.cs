using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Sunbrella.Details
{
    public class SunbrellaFileLoader : ProductFileLoader<SunbrellaVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("SKU", ScanField.ManufacturerPartNumber),
            new FileProperty("PATTERN", ScanField.PatternName),
            new FileProperty("COLOR", ScanField.ColorName),
            new FileProperty("INSIDE", ScanField.Cost),
            new FileProperty("MIAP", ScanField.MAP),
        };

        public SunbrellaFileLoader(IStorageProvider<SunbrellaVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }
    }
}
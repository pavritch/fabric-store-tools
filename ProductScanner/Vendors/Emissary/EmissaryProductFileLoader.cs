using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Emissary
{
    public class EmissaryProductFileLoader : ProductFileLoader<EmissaryVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("QTY", ScanField.StockCount),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("", ScanField.Ignore),
            new FileProperty("Item", ScanField.ManufacturerPartNumber),
            new FileProperty("Description ", ScanField.Description),
            new FileProperty("INV16-09", ScanField.Ignore),
            new FileProperty("EMH-131", ScanField.Ignore),
            new FileProperty("INV16-10", ScanField.Ignore),
            new FileProperty("Ship by", ScanField.ShippingMethod),
            new FileProperty("Catalog", ScanField.Ignore),
            new FileProperty("Packed", ScanField.Weight),
        };

        public EmissaryProductFileLoader(IStorageProvider<EmissaryVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx, 4, 7) { }
    }
}
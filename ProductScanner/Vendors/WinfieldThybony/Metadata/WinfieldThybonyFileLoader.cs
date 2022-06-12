using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace WinfieldThybony.Metadata
{
    public class WinfieldThybonyFileLoader : ProductFileLoader<WinfieldThybonyVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("MPN", ScanField.ManufacturerPartNumber),
            new FileProperty("RetailPrice", ScanField.RetailPrice),
            new FileProperty("Unit", ScanField.UnitOfMeasure),
            new FileProperty("WholesalePrice", ScanField.Cost),
        };

        public WinfieldThybonyFileLoader(IStorageProvider<WinfieldThybonyVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xls) { }
    }
}
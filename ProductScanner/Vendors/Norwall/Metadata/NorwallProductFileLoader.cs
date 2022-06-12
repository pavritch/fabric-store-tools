using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Norwall.Metadata
{
    public class NorwallProductFileLoader : ProductFileLoader<NorwallVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("SKU's", ScanField.ManufacturerPartNumber),
        };

        public NorwallProductFileLoader(IStorageProvider<NorwallVendor> storageProvider) 
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            return await base.LoadProductsAsync();
        }
    }
}
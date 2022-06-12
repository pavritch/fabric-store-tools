using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Brewster.Metadata
{
    public class BrewsterFileLoader : ProductFileLoader<BrewsterVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Pattern", ScanField.ManufacturerPartNumber),
            new FileProperty("Book #", ScanField.BookNumber),
            new FileProperty("Book Name", ScanField.Book),
            new FileProperty("Brand", ScanField.Brand),
            new FileProperty("Code", ScanField.Code),
            new FileProperty("Unit of Measure", ScanField.UnitOfMeasure),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("Partner Cost", ScanField.Cost),
            // these are either exactly double the previous set, or exactly the same
            new FileProperty("Unit of Measure", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("MAP", ScanField.Ignore),
            new FileProperty("Partner Cost", ScanField.Ignore),
        };

        public BrewsterFileLoader(IStorageProvider<BrewsterVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties)
        {
        }
    }
}
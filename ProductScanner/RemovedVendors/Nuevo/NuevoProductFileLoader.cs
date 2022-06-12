using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nuevo
{
    public class NuevoProductFileLoader : ProductFileLoader<NuevoVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item No.", ScanField.ManufacturerPartNumber),
            new FileProperty("Name", ScanField.ProductName),
            new FileProperty("Product Group", ScanField.ProductGroup),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Price", ScanField.RetailPrice),
            new FileProperty("Unit of Measure", ScanField.UnitOfMeasure),
            new FileProperty("Description", ScanField.Description),
            new FileProperty("Size", ScanField.Dimensions),
        };

        public NuevoProductFileLoader(IStorageProvider<NuevoVendor> storageProvider) 
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x[ScanField.Dimensions] = x[ScanField.Dimensions].Replace("¼", ".25").Replace("½", ".5"));
            products.ForEach(x => x.Cost = x[ScanField.RetailPrice].ToDecimalSafe()/2.5m);
            return products.Where(x => x[ScanField.ProductGroup] != string.Empty).ToList();
        }
    }
}
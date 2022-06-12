using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Fabricut.Discovery
{
    public class TrendFileLoader : FabricutProductFileLoader<TrendVendor>
    {
        public TrendFileLoader(IStorageProvider<TrendVendor> storageProvider) : base(storageProvider) { }
    }

    public class FabricutFileLoader : FabricutProductFileLoader<FabricutVendor>
    {
        public FabricutFileLoader(IStorageProvider<FabricutVendor> storageProvider) : base(storageProvider) { }
    }

    public class SHarrisFileLoader : FabricutProductFileLoader<SHarrisVendor>
    {
        public SHarrisFileLoader(IStorageProvider<SHarrisVendor> storageProvider) : base(storageProvider) { }
    }

    public class StroheimFileLoader : FabricutProductFileLoader<StroheimVendor>
    {
        public StroheimFileLoader(IStorageProvider<StroheimVendor> storageProvider) : base(storageProvider) { }
    }

    public class VervainFileLoader : FabricutProductFileLoader<VervainVendor>
    {
        public VervainFileLoader(IStorageProvider<VervainVendor> storageProvider) : base(storageProvider) { }
    }

    public class FabricutProductFileLoader<T> : ProductFileLoader<T> where T : FabricutBaseVendor, new()
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("BRAND NAME", ScanField.Brand),
            new FileProperty("FABRIC NUMBER", ScanField.ManufacturerPartNumber),
            new FileProperty("PATTERN NAME", ScanField.PatternName),
            new FileProperty("COLOR", ScanField.ColorName),
            new FileProperty("STATUS SPELLED OUT", ScanField.Ignore),
            new FileProperty("IMAGE NAME", ScanField.ImageUrl),
            new FileProperty("BASE COLOR", ScanField.Color),
            new FileProperty("UNIT OF MEASURE", ScanField.UnitOfMeasure),
            new FileProperty("MATERIAL TYPE", ScanField.ProductGroup),
            new FileProperty("PATTERN WIDTH 2", ScanField.Width),
            new FileProperty("VERTL REPEAT LEN2", ScanField.VerticalRepeat),
            new FileProperty("HORIZ REPEAT LEN2", ScanField.HorizontalRepeat),
            new FileProperty("PRICE - WHOLESALE", ScanField.Ignore), // comes from inventory file
            new FileProperty("CONTENTS", ScanField.Content),
            new FileProperty("COUNTRY OF ORIGIN", ScanField.Country),
            new FileProperty("SAMPLE BOOK NO 1", ScanField.BookNumber),
            new FileProperty("SAMPLE BOOK DESC 1", ScanField.Book),
            new FileProperty("DURABILITY", ScanField.Durability),
            new FileProperty("FINISH", ScanField.Finish),
            new FileProperty("CLEANING CODE", ScanField.Cleaning),
            new FileProperty("FLAME RETARDANT", ScanField.FlameRetardant),
            new FileProperty("AVERAGE BOLT", ScanField.AverageBolt),
            new FileProperty("WEB APPLICATION", ScanField.Ignore),
            new FileProperty("WEB USE", ScanField.Use),
            new FileProperty("WEB SCALE", ScanField.Scale),
            new FileProperty("WEB CATEGORY", ScanField.Ignore),
            new FileProperty("WEB CATEGORY DESC 1", ScanField.Category1),
            new FileProperty("WEB CATEGORY DESC 2", ScanField.Category2),
            new FileProperty("WEB CATEGORY DESC 3", ScanField.Category3),
            new FileProperty("WEB CATEGORY DESC 4", ScanField.Category4),
            new FileProperty("WEB DESIGN", ScanField.Ignore),
            new FileProperty("WEB DESIGN DESC 1", ScanField.Style1),
            new FileProperty("WEB DESIGN DESC 2", ScanField.Style2),
            new FileProperty("WEB DESIGN DESC 3", ScanField.Style3),
            new FileProperty("WEB DESIGN DESC 4", ScanField.Style4),
            new FileProperty("FILE CREATE DATE", ScanField.Ignore),
            new FileProperty("RAILROADED", ScanField.Railroaded),
            new FileProperty("MINIMUM QUANTITY", ScanField.Ignore),
            new FileProperty("ORDER INCREMENT", ScanField.Ignore),
        };

        public FabricutProductFileLoader(IStorageProvider<T> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var p in products)
            {
                p[ScanField.ManufacturerPartNumber] = p[ScanField.ManufacturerPartNumber].PadLeft(7, '0');
                p[ScanField.ItemNumber] = p[ScanField.ImageUrl].Replace(".jpg", "").Replace(".JPG", "");
            }
            return products;
        }
    }
}
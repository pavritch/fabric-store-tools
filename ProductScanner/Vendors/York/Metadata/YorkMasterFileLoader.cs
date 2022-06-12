using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace York.Metadata
{
    public class YorkMasterFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("BRAND NAME", ScanField.Brand),
            new FileProperty("VENDOR", ScanField.ManufacturerPartNumber),
            new FileProperty("COLLECTION", ScanField.Collection),
            new FileProperty("COLLECTION", ScanField.CollectionExpiration),
            new FileProperty("PRODUCT NAME", ScanField.ProductName),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("MINIMUM", ScanField.MinimumQuantity),
        };

        private readonly IStorageProvider<YorkVendor> _storageProvider;
        public YorkMasterFileLoader(IStorageProvider<YorkVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadProducts()
        {
            var staticFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var data = fileLoader.Load(Path.Combine(staticFilesFolder, "Master.xlsx"), Properties, ScanField.ManufacturerPartNumber, 2, 4);

            data.ForEach(x =>
            {
                x.Cost = (.45m*x[ScanField.RetailPrice].ToDecimalSafe());
                x[ScanField.CollectionExpiration] = x[ScanField.CollectionExpiration].Replace("9/31/2018", "9/30/2018");
            });
            return data.Where(x => DateTime.Parse(x[ScanField.CollectionExpiration]) > DateTime.Today).ToList();
        }
    }
}
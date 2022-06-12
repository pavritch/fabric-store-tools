using System.Collections.Generic;
using System.IO;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace York.Metadata
{
    public class YorkPricingFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Pattern", ScanField.ManufacturerPartNumber),
            new FileProperty("Pattern", ScanField.Ignore),
            new FileProperty("Suggested", ScanField.RetailPrice),
            new FileProperty("MAP Pricing", ScanField.MAP),
        };

        private readonly IStorageProvider<YorkVendor> _storageProvider;
        public YorkPricingFileLoader(IStorageProvider<YorkVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var staticFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var data = fileLoader.Load(Path.Combine(staticFilesFolder, "YorkDesignerSeriesMapMaster.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 3);
            foreach (var product in data)
            {
                product[ScanField.ManufacturerPartNumber] = product[ScanField.ManufacturerPartNumber].Replace("*", "").Trim();
                product[ScanField.MAP] = product[ScanField.MAP].Replace("$", "");
                product[ScanField.RetailPrice] = product[ScanField.RetailPrice].Replace("$", "");
            }
            return data;
        }

        //private static readonly List<FileProperty> Properties = new List<FileProperty>
        //{
        //    new FileProperty("Item", ScanField.ManufacturerPartNumber),
        //    new FileProperty("Desc 1", ScanField.Ignore),
        //    new FileProperty("Div", ScanField.Ignore),
        //    new FileProperty("Coll", ScanField.Ignore),
        //    new FileProperty("Collection", ScanField.Ignore),
        //    new FileProperty("MSRP", ScanField.RetailPrice),
        //    new FileProperty("MAP", ScanField.MAP),
        //    new FileProperty("Ct", ScanField.Ignore),
        //};

        //private readonly IStorageProvider<YorkVendor> _storageProvider;
        //public YorkPricingFileLoader(IStorageProvider<YorkVendor> storageProvider)
        //{
        //    _storageProvider = storageProvider;
        //}

        //public List<ScanData> LoadInventoryData()
        //{
        //    var staticFilesFolder = _storageProvider.GetStaticFolder();
        //    var fileLoader = new ExcelFileLoader();
        //    var data = fileLoader.Load(Path.Combine(staticFilesFolder, "MAP_Pricing.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        //    foreach (var product in data)
        //    {
        //        product[ScanField.ManufacturerPartNumber] = product[ScanField.ManufacturerPartNumber].Replace("*", "").Trim();
        //        product[ScanField.MAP] = product[ScanField.MAP].Replace("$", "");
        //        if (product[ScanField.RetailPrice].Replace("$", "").ToDoubleSafe() >= 0)
        //            product[ScanField.RetailPrice] = product[ScanField.RetailPrice].Replace("$", "");
        //    }
        //    return data;
        //}
    }
}
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ExcelLibrary.SpreadSheet;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Chella.Metadata
{
    public class ChellaFileLoader : IProductFileLoader<ChellaVendor>
    {
        private readonly IStorageProvider<ChellaVendor> _storageProvider;

        public ChellaFileLoader(IStorageProvider<ChellaVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public Task<List<ScanData>> LoadProductsAsync()
        {
            var products = new List<ScanData>();
            var workbook = Workbook.Load(new MemoryStream(File.ReadAllBytes(_storageProvider.GetProductsFileStaticPath(ProductFileType.Xls))));
            var sheet = workbook.Worksheets[0];

            // for now just assuming that there are max of 300 rows
            for (var row = 0; row < 300; row++)
            {
                var itemNumber = sheet.Cells[row, 0].StringValue;

                // skip the header and blank rows
                if (!itemNumber.ContainsDigit()) continue;

                var vendorProduct = new ScanData();
                vendorProduct[ProductPropertyType.ItemNumber] = itemNumber.Trim();
                vendorProduct[ProductPropertyType.PatternName] = sheet.Cells[row, 1].StringValue.Trim();
                vendorProduct[ProductPropertyType.Width] = sheet.Cells[row, 2].StringValue.Trim();
                vendorProduct[ProductPropertyType.Durability] = sheet.Cells[row, 3].StringValue.Trim();
                vendorProduct[ProductPropertyType.Repeat] = sheet.Cells[row, 4].StringValue.Trim();
                vendorProduct[ProductPropertyType.WholesalePrice] = sheet.Cells[row, 5].StringValue.Trim();
                vendorProduct[ProductPropertyType.Content] = sheet.Cells[row, 6].StringValue.Trim();
                products.Add(vendorProduct);
            }
            return Task.FromResult(products);
        }
    }
}
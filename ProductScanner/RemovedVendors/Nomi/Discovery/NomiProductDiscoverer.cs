using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nomi.Discovery
{
    public class NomiProductDiscoverer : IProductDiscoverer<NomiVendor>
    {
        private readonly IStorageProvider<NomiVendor> _storageProvider;

        public NomiProductDiscoverer(IStorageProvider<NomiVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var filePath = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx);
            return Task.FromResult(LoadProductsFromXLSX(File.ReadAllBytes(filePath)).Select(x => new DiscoveredProduct(x)).ToList());
        }

        private IEnumerable<ScanData> LoadProductsFromXLSX(byte[] binaryData)
        {
            var products = new List<ScanData>();
            using (var xlsx = new ExcelPackage(new MemoryStream(binaryData)))
            {
                var sheet = xlsx.Workbook.Worksheets[1];
                var rowNum = 1;
                var patternName = "";
                var contentOne = "";
                var contentTwo = "";
                while (true)
                {
                    var col1 = sheet.GetValue<string>(rowNum, 1);
                    if (string.IsNullOrWhiteSpace(col1)) break;

                    // if col1 has no dash, it's one of the header rows
                    if (!col1.Contains("-"))
                    {
                        patternName = sheet.GetValue<string>(rowNum, 1);
                        contentOne = sheet.GetValue<string>(rowNum + 1, 1);
                        contentTwo = sheet.GetValue<string>(rowNum + 2, 1);
                        rowNum += 2;
                    }
                    else
                    {
                        var row = GetFullRow(sheet, rowNum);
                        row = row.Replace("B & W", "BlackAndWhite");
                        var splitRow = row.Split(new [] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        var patternNumber = splitRow.First();
                        var color = splitRow[1];
                        var price = splitRow.Last().Replace("$", "");
                        products.Add(CreateNomiProduct(patternName, contentOne, contentTwo, patternNumber, color, price));
                    }
                    rowNum++;
                }
            }
            return products;
        }

        private string GetFullRow(ExcelWorksheet sheet, int rowNum)
        {
            return Enumerable.Range(1, 25).Select(x => sheet.GetValue<string>(rowNum, x)).Aggregate((a, b) => a + " " + b);
        }

        private ScanData CreateNomiProduct(string patternName, string contentOne, string contentTwo, string mpn, string color, string price)
        {
            var contentOneParts = contentOne.Split(new[] { "•" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            var contentTwoParts = contentTwo.Split(new[] { "•" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            var product = new ScanData();
            product[ProductPropertyType.PatternName] = patternName;
            product[ProductPropertyType.ManufacturerPartNumber] = mpn;
            product[ProductPropertyType.ColorName] = color;
            product[ProductPropertyType.WholesalePrice] = price;

            if (contentOneParts.Count >= 1) product[ProductPropertyType.Material] = contentOneParts[0];
            if (contentOneParts.Count >= 2) product[ProductPropertyType.Content] = contentOneParts[1];
            if (contentOneParts.Count >= 3) product[ProductPropertyType.Width] = contentOneParts[2];
            if (contentOneParts.Count >= 4) product[ProductPropertyType.Repeat] = contentOneParts[3];

            product[ProductPropertyType.FireCode] = contentTwoParts[0];
            product[ProductPropertyType.FlameRetardant] = contentTwoParts[1];
            product[ProductPropertyType.Durability] = contentTwoParts[2];
            return product;
        }
    }
}
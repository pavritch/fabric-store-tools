using System.Collections.Generic;
using System.IO;
using CsvHelper;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public static class CsvFileLoader
    {
        public static List<ScanData> ReadProductsCsvFile(string filePath, List<FileProperty> fileProperties)
        {
            var products = new List<ScanData>();
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var csv = new CsvReader(reader);
                while (csv.Read())
                {
                    var product = new ScanData();
                    foreach (var property in fileProperties)
                    {
                        product[property.Property] = csv.GetField(property.Header);
                    }
                    products.Add(product);
                }
            }
            return products;
        }
    }
}
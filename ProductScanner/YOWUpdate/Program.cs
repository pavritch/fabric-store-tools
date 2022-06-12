using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ProductScanner.Core;
using ProductScanner.Core.Config;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Data;
using Utilities.Extensions;

namespace YOWUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            AddStockColumn();
        }

        public static void AddStockColumn()
        {
            // ftp://econtent%40yowexchange.com:f4gz2sp@yowexchange.com/InventoryFeed/YowInventory_12.11.15.csv

            var stockFileLoader = new YourOtherWarehouseStockFileLoader();
            var fileLoader = new YourOtherWarehousePriceFileLoader();

            FileExtensions.ConvertCSVToXLSX(@"C:\Dropbox\YOW\YowInventory_12.11.15.csv", @"C:\Dropbox\YOW\YowInventory_12.11.15.xlsx");

            var products = fileLoader.LoadData(@"C:\Dropbox\YOW\YourOtherWarehouse_Price.xlsx");
            var stockInfo = stockFileLoader.LoadStockData(@"C:\Dropbox\YOW\YowInventory_12.11.15.xlsx");

            foreach (var product in products)
            {
                var stockRecord = stockInfo.FirstOrDefault(x => x[ProductPropertyType.ManufacturerPartNumber] == product[ProductPropertyType.ManufacturerPartNumber]);
                if (stockRecord != null)
                {
                    product[ProductPropertyType.StockCount] = stockRecord[ProductPropertyType.StockCount];
                }
            }

            var withStock = products.Count(x => x[ProductPropertyType.StockCount].ToIntegerSafe() > 0);

            var textWriter = File.CreateText(@"C:\Dropbox\YOW\YourOtherWarehouse_PriceWithStock.csv");
            var writer = new CsvWriter(textWriter);
            foreach (var product in products)
            {
                foreach (var item in product.Values)
                {
                    writer.WriteField(item);
                }
                writer.NextRecord();
            }
            textWriter.Close();

            FileExtensions.ConvertCSVToXLSX(@"C:\Dropbox\YOW\YourOtherWarehouse_PriceWithStock.csv", @"C:\Dropbox\YOW\YourOtherWarehouse_PriceWithStock.xlsx");
        }
    }

    public class YourOtherWarehouseStockFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("ItemNumber", ProductPropertyType.ManufacturerPartNumber),
            new FileProperty("ProductNumber", ProductPropertyType.Ignore),
            new FileProperty("ItemDescription", ProductPropertyType.Ignore),
            new FileProperty("QuantityAvailable", ProductPropertyType.StockCount),
        };

        public List<ScanData> LoadStockData(string filePath)
        {
            var stockFileLoader = new ExcelFileLoader();
            return stockFileLoader.Load(filePath, Properties, ProductPropertyType.ManufacturerPartNumber, 1, 2);
        }
    }

    public class YourOtherWarehousePriceFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("OMSID", ProductPropertyType.ItemNumber),
            new FileProperty("YOW ITEM NUMBER", ProductPropertyType.ManufacturerPartNumber),
            new FileProperty("NEW ITEM?", ProductPropertyType.IsClearance),
            new FileProperty("UPC", ProductPropertyType.UPC),
            new FileProperty("BRAND", ProductPropertyType.Brand),
            new FileProperty("PRODUCT NAME", ProductPropertyType.ProductName),
            new FileProperty("MODEL NUMBER", ProductPropertyType.ModelNumber),
            new FileProperty("FINISH / COLOR", ProductPropertyType.Color),
            new FileProperty("CLASS", ProductPropertyType.Classification),
            new FileProperty("SUBCLASS", ProductPropertyType.Subclass),
            new FileProperty("CATEGORY", ProductPropertyType.Category),
            new FileProperty("LIST PRICE", ProductPropertyType.RetailPrice),
            new FileProperty("PRODUCT IMAGE", ProductPropertyType.Image1),
            new FileProperty("MARKETING COPY", ProductPropertyType.Description),
            new FileProperty("BULLET 1", ProductPropertyType.TempContent1),
            new FileProperty("BULLET 2", ProductPropertyType.TempContent2),
            new FileProperty("BULLET 3", ProductPropertyType.TempContent3),
            new FileProperty("BULLET 4", ProductPropertyType.TempContent4),
            new FileProperty("BULLET 5", ProductPropertyType.TempContent5),
            new FileProperty("BULLET 6", ProductPropertyType.TempContent6),
            new FileProperty("BULLET 7", ProductPropertyType.TempContent7),
            new FileProperty("BULLET 8", ProductPropertyType.TempContent8),
            new FileProperty("BULLET 9", ProductPropertyType.TempContent9),
            new FileProperty("BULLET 10", ProductPropertyType.TempContent10),
            new FileProperty("PRODUCT WEIGHT (LBS.)", ProductPropertyType.Weight),
            new FileProperty("PRODUCT LENGTH (IN)", ProductPropertyType.Length),
            new FileProperty("PRODUCT HEIGHT (IN)", ProductPropertyType.Height),
            new FileProperty("PRODUCT WIDTH (IN)", ProductPropertyType.Width),
            new FileProperty("ITEM BOXED WEIGHT, EACH (LBS.)", ProductPropertyType.Packaging),
            new FileProperty("ITEM BOXED LENGTH, EACH (IN)", ProductPropertyType.PackageLength),
            new FileProperty("ITEM BOXED HEIGHT, EACH (IN)", ProductPropertyType.PackageHeight),
            new FileProperty("ITEM BOXED WIDTH, EACH (IN)", ProductPropertyType.PackageWidth),
            new FileProperty("SHIPPING METHOD", ProductPropertyType.ShippingMethod),
            new FileProperty("COUNTRY OF ORIGIN CODE", ProductPropertyType.CountryOfOrigin),
            new FileProperty("WATERSENSE QUALIFIED?", ProductPropertyType.TempContent11),
        };

        public List<ScanData> LoadData(string filePath)
        {
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(filePath, Properties, ProductPropertyType.ManufacturerPartNumber, 1, 2);
        }
    }
            
}

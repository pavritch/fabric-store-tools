using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Capel.Discovery
{
    public class CapelFileLoader : ProductFileLoader<CapelVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Status", ProductPropertyType.Status),
            new FileProperty("Discontinued", ProductPropertyType.TempContent1),
            new FileProperty("UPC", ProductPropertyType.UPC),
            new FileProperty("Quantity", ProductPropertyType.StockCount),
            new FileProperty("DateAvailable", ProductPropertyType.TempContent2),
            new FileProperty("ItemNum", ProductPropertyType.ManufacturerPartNumber),
            new FileProperty("ItemName", ProductPropertyType.Ignore),
            new FileProperty("InternetName", ProductPropertyType.ProductName),
            new FileProperty("StyleNum", ProductPropertyType.Code),
            new FileProperty("ColorDesc", ProductPropertyType.Ignore),
            new FileProperty("InternetColor", ProductPropertyType.Color),
            new FileProperty("Size", ProductPropertyType.Size),
            new FileProperty("Shape", ProductPropertyType.Shape),
            new FileProperty("Tariff", ProductPropertyType.Tariff),
            new FileProperty("UnitPrice", ProductPropertyType.WholesalePrice),
            new FileProperty("DiscountPercent", ProductPropertyType.TempContent4),
            new FileProperty("DiscountPrice", ProductPropertyType.TempContent5),
            new FileProperty("MAP", ProductPropertyType.MAP),
            new FileProperty("MSRP", ProductPropertyType.RetailPrice),
            new FileProperty("PerSqFoot", ProductPropertyType.TempContent7),
            new FileProperty("NetWeight", ProductPropertyType.Weight),
            new FileProperty("Country", ProductPropertyType.CountryOfOrigin),
            new FileProperty("Construction", ProductPropertyType.Construction),
            new FileProperty("Content", ProductPropertyType.Content),
            new FileProperty("Licensee", ProductPropertyType.TempContent8),
            new FileProperty("PackLength", ProductPropertyType.PackageLength),
            new FileProperty("PackWidth", ProductPropertyType.PackageWidth),
            new FileProperty("PackHeight", ProductPropertyType.PackageHeight),
            new FileProperty("Girth", ProductPropertyType.TempContent9),
            new FileProperty("Image1", ProductPropertyType.Image1),
            new FileProperty("Image2", ProductPropertyType.Image2),
            new FileProperty("Image3", ProductPropertyType.Image3),
            new FileProperty("Image4", ProductPropertyType.Image4),
            new FileProperty("Image5", ProductPropertyType.Image5),
        };

        public CapelFileLoader(IStorageProvider<CapelVendor> storageProvider) : base(storageProvider)
        {
            _headerRow = 1;
            _startRow = 2;
            _keyProperty = ProductPropertyType.ManufacturerPartNumber;
            _properties = Properties;
        }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                var mpn = product[ProductPropertyType.ManufacturerPartNumber];
                if (mpn.Length < 6) continue;

                var colorNumber = mpn.Substring(mpn.Length - 3);

                // looks like first 6 are pattern number, and last 3 are color number
                // other digits in the middle are the size
                product[ProductPropertyType.ColorNumber] = colorNumber;
                product[ProductPropertyType.PatternNumber] = mpn.Substring(0, 6);
                product[ProductPropertyType.SKU] = mpn.Substring(0, 6) + colorNumber;

                var replace = "ftp://portal1.capel.net";
                var replaceWith = "ftp://CapelFTP:rugpics@portal1.capel.net";

                product[ProductPropertyType.Image1] = product[ProductPropertyType.Image1].Replace(replace, replaceWith);
                product[ProductPropertyType.Image2] = product[ProductPropertyType.Image2].Replace(replace, replaceWith);
                product[ProductPropertyType.Image3] = product[ProductPropertyType.Image3].Replace(replace, replaceWith);
                product[ProductPropertyType.Image4] = product[ProductPropertyType.Image4].Replace(replace, replaceWith);
                product[ProductPropertyType.Image5] = product[ProductPropertyType.Image5].Replace(replace, replaceWith);
            }
            return products;
        }
    }
}
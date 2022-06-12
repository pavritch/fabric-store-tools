using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Interlude
{
    public class InterludeProductBuilder : ProductBuilder<InterludeVendor>
    {
        public InterludeProductBuilder(IPriceCalculator<InterludeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount];

            var vendor = new InterludeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(stock.Contains("In Stock")), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductName]);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Cabinets, name => name.Contains("Cabinet")},
            {HomewareCategory.Accent_Chairs, name => name.Contains("Chair")},
            {HomewareCategory.Accent_Tables , name => name.Contains("Accent Table")},
            {HomewareCategory.Accessories , name => name.Contains("Trio") || name.Contains("Pedestal")},
            {HomewareCategory.Andirons, name => name.Contains("Andiron")},
            {HomewareCategory.Bar_Carts, name => name.Contains("Bar Cart")},
            {HomewareCategory.Bar_Stools, name => name.Contains("Stool") || name.Contains("Barstool")},
            {HomewareCategory.Baskets, name => name.Contains("Basket")},
            {HomewareCategory.Benches, name => name.Contains("Bench")},
            {HomewareCategory.Bookcases, name => name.Contains("Bookshelf") || name.Contains("Bookshelves")},
            {HomewareCategory.Bottles, name => name.Contains("Bottle")},
            {HomewareCategory.Bowls, name => name.Contains("Bowl") || name.Contains("Candy Dish")},
            {HomewareCategory.Candleholders, name => name.Contains("Candlestand") || name.Contains("Hurricane")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Cocktail")},
            {HomewareCategory.Console_Tables, name => name.Contains("Console") || name.Contains("Media Unit")},
            {HomewareCategory.Decorative_Boxes, name => name.Contains("Box")},
            {HomewareCategory.Decorative_Trays, name => name.Contains("Tray")},
            {HomewareCategory.Desks, name => name.Contains("Desk")},
            {HomewareCategory.Dressers, name => name.Contains("Dressers") || name.Contains("Dresser")},
            {HomewareCategory.Fire_Screens, name => name.Contains("Firescreen") || name.Contains("Fireplace Screen") || name.Contains("Fire Screen")},
            //{HomewareCategory.Floor_Lamps, name => name.Contains("Floor Lamp")},
            //{HomewareCategory.Gathering_Table, name => name.Contains("Game Table") || name.Contains("Center Table")},
            {HomewareCategory.Jars, name => name.Contains("Jar")},
            {HomewareCategory.Magazine_Racks, name => name.Contains("Magazine")},
            {HomewareCategory.Mirrors, name => name.Contains("Mirror")},
            {HomewareCategory.Ottomans, name => name.Contains("Ottoman")},
            {HomewareCategory.Poufs, name => name.Contains("Pouf")},
            //{HomewareCategory.Rugs, name => name.Contains("Rug")},
            {HomewareCategory.Sconces, name => name.Contains("Sconce")},
            {HomewareCategory.Sofas, name => name.Contains("Sofa") || name.Contains("Chaise")},
            {HomewareCategory.Statues_and_Sculptures, name => name.Contains("Sculpture")},
            {HomewareCategory.Shelves, name => name.Contains("Shelf") || name.Contains("Shelves")},
            {HomewareCategory.Side_Tables, name => name.Contains("Side Table") || name.Contains("Chest") || name.Contains("Trunk")},
            {HomewareCategory.Table_Lamps, name => name.Contains("Lamp")},
            {HomewareCategory.Wall_Art, name => name.Contains("Wall Decor") || name.Contains("Wall Panel") || name.Contains("Wall ")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
        };

        private HomewareCategory AssignCategory(string productName)
        {
            foreach (var category in _categories)
                if (category.Value(productName))
                    return category.Key;
            if (productName.ContainsIgnoreCase("Table")) return HomewareCategory.Accent_Tables;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var mainDimensions = data[ScanField.Dimensions].Split(new[] {';'}).First();
            var features = new HomewareProductFeatures();
            features.Depth = GetLength(mainDimensions).ToIntegerSafe();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = GetHeight(mainDimensions).ToIntegerSafe();
            features.Width = GetWidth(mainDimensions).ToIntegerSafe();
            //features.ShippingWeight = data[ScanField.Weight];
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var mainDimensions = data[ScanField.Dimensions].Split(new[] {';'}).First();
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Diameter, GetDiam(mainDimensions)},
                {ScanField.Finish, FormatFinish(data[ScanField.Finish])},
                {ScanField.Material, FormatMaterial(data[ScanField.Material])},
            });
        }

        private string GetHeight(string dimensions)
        {
            var dims = dimensions.Split(new[] {'x'}).ToList();
            if (dims.First().Contains("h")) return dims.First().Replace("\"", "").Replace("h", "");
            return string.Empty;
        }

        private string GetLength(string dimensions)
        {
            var dims = dimensions.Split(new[] {'x'}).ToList();
            if (dims.First().Contains("h") && dims.Count == 3) return dims[1].Replace("\"", "");
            return string.Empty;
        }

        private string GetWidth(string dimensions)
        {
            var dims = dimensions.Split(new[] {'x'}).ToList();
            if (dims.First().Contains("h") && dims.Count == 3) return dims[2].Replace("\"", "");
            return string.Empty;
        }

        private string GetDiam(string dimensions)
        {
            var dims = dimensions.Split(new[] {'x'}).ToList();
            var diam = dims.SingleOrDefault(x => x.Contains("diam"));
            if (diam != null) return diam.Replace("diam", "").Replace("\"", "").ToInchesMeasurement();
            return string.Empty;
        }

        private string FormatFinish(string finish)
        {
            var finishes = finish.Split(new[] {'/', ';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.TitleCase().Trim()).ToList();
            return string.Join(", ", finishes);
        }

        private string FormatMaterial(string material)
        {
            var materials = material.Split(new[] {'/', ';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new RugMaterial(x.Trim()).FormatAsString()).ToList();
            return string.Join(", ", materials);
        }
    }
}
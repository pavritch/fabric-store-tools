using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace GoHomeLTD
{
    public class GoHomeDimensions
    {
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Diameter { get; set; }
    }

    public class GoHomeProductBuilder : ProductBuilder<GoHomeVendor>
    {
        public GoHomeProductBuilder(IPriceCalculator<GoHomeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new GoHomeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(GetStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));

            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductName], data[ScanField.Category2]);
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data[ScanField.DetailUrlTEMP];

            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private GoHomeDimensions GetDimensions(string dimensions)
        {
            dimensions = dimensions.ToLower();

            var currDim = new GoHomeDimensions();
            var parts = dimensions.Split(new[] {'x'}).Select(x => x.Trim()).ToList();
            foreach (var part in parts)
            {
                if (part.ContainsIgnoreCase("dia")) currDim.Diameter = part.Replace("dia", "").Trim('"', ' ').ToDoubleSafe();
                if (part.ContainsIgnoreCase("h")) currDim.Height = part.Replace("h", "").Trim('"', ' ').ToDoubleSafe();
                if (part.ContainsIgnoreCase("w")) currDim.Width = part.Replace("w", "").Trim('"', ' ').ToDoubleSafe();
                if (part.ContainsIgnoreCase("l")) currDim.Length = part.Replace("l", "").Trim('"', ' ').ToDoubleSafe();
            }
            return currDim;
        }

        private bool GetStock(string stock)
        {
            if (stock == "ContentPlaceHolder1_StylePage_Icon_Inventory_Low") return true;
            if (stock == "ContentPlaceHolder1_StylePage_Icon_Inventory_Medium") return true;
            if (stock == "ContentPlaceHolder1_StylePage_Icon_Inventory_High") return true;
            if (stock == "ContentPlaceHolder1_StylePage_Icon_Green") return true;
            return false;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            var dimensions = GetDimensions(data[ScanField.Dimensions]);
            features.Depth = dimensions.Length;
            features.Height = dimensions.Height;
            features.Width = dimensions.Width;
            features.Features = BuildSpecs(data, dimensions.Diameter);
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data, double diameter)
        {
            var specs = ToSpecs(new Dictionary<ScanField, string>
            {
                { ScanField.Collection, data[ScanField.Collection]},
                { ScanField.Finish, data[ScanField.Finish].Replace("Finish: ", "")},
                { ScanField.Material, data[ScanField.Material].Replace("Material: ", "")},
            });
            if (diameter != 0) specs.Add(ScanField.Diameter.ToString(), diameter.ToString());
            return specs;
        }

        private HomewareCategory AssignCategory(string name, string subcategory)
        {
            foreach (var category in _categories)
                if (category.Value(name, subcategory))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accent_Chairs, (name, cat) => cat.Contains("Chairs")},
            {HomewareCategory.Accent_Tables, (name, cat) => name.Contains("Dressing Table")},
            {HomewareCategory.Accessories, (name, cat) => name.ContainsAnyOf("Wine Cooler", "Wine Rack", "Wine Bottle Rack", "Pinecone", "Crystal Ball", "Globe") ||
                cat.ContainsAnyOf("Office Accessories", "Trophies")},
            {HomewareCategory.Bar_Carts, (name, cat) => cat.Contains("Carts")},
            {HomewareCategory.Bar_Stools, (name, cat) => cat.Contains("Stools")},
            {HomewareCategory.Baskets, (name, cat) => cat.Contains("Baskets")},
            {HomewareCategory.Benches, (name, cat) => name.Contains("Bench")},
            {HomewareCategory.Bookcases, (name, cat) => name.Contains("Bookshelf")},
            {HomewareCategory.Bowls, (name, cat) => name.Contains("Bowl")},
            {HomewareCategory.Cabinets, (name, cat) => name.Contains("Cabinet")},
            {HomewareCategory.Candles, (name, cat) => name.Contains("Votive")},
            {HomewareCategory.Candleholders, (name, cat) => cat.ContainsAnyOf("Candlesticks", "Lanterns") || 
                name.ContainsAnyOf("Candlestick", "Candleholder", "Pillar Holders", "Candle Sticks", "Candelabra", "Pillarholder", "Candle Stand",
                "Candle Holders", "T-Light Holder")},
            {HomewareCategory.Chandeliers, (name, cat) => name.Contains("Chandelier")},
            {HomewareCategory.Coasters, (name, cat) => name.Contains("Coasters")},
            {HomewareCategory.Coffee_Tables, (name, cat) => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, (name, cat) => cat.Contains("Console Tables")},
            {HomewareCategory.Decanters, (name, cat) => name.Contains("Decanter")},
            {HomewareCategory.Decor, (name, cat) => cat.Contains("Decorative Accessories")},
            {HomewareCategory.Decorative_Trays, (name, cat) => cat.Contains("Trays") || name.Contains("Tray")},
            {HomewareCategory.Desks, (name, cat) => cat.Contains("Desks")},
            {HomewareCategory.Dining_Chairs, (name, cat) => name.Contains("Dining Chair")},
            {HomewareCategory.Dining_Sets, (name, cat) => cat.ContainsAnyOf("Serving", "Glassware", "Bar Decor")},
            {HomewareCategory.Dining_Tables, (name, cat) => name.Contains("Dining Table")},
            {HomewareCategory.Dressers, (name, cat) => name.ContainsAnyOf("Chest", "Dresser")},
            {HomewareCategory.Floor_Lamps, (name, cat) => name.Contains("Floor Lamp") || (name.Contains("Lamp") && cat.Contains("Floor"))},
            {HomewareCategory.Home_Accents, (name, cat) => cat.Contains("Books")},
            {HomewareCategory.Ice_Buckets, (name, cat) => name.Contains("Ice Bucket")},
            {HomewareCategory.Jars, (name, cat) => cat.Contains("Jars")},
            {HomewareCategory.Kitchen_and_Dining, (name, cat) => name.ContainsAnyOf("Cheese", "Stand", "Platter", "Bottle Chiller", "Bucket", 
                "Pitcher", "Cocktail", "Picks", "Nut Cracker", "Bottle Stoppers")},
            {HomewareCategory.Mirrors, (name, cat) => cat.Contains("Mirrors")},
            {HomewareCategory.Ottomans, (name, cat) => cat.Contains("Ottoman") && !name.Contains("Pouf")},
            {HomewareCategory.Pendants, (name, cat) => cat.Contains("Ceiling Lighting") || name.Contains("Pendant")},
            {HomewareCategory.Pillows_Throws_and_Poufs, (name, cat) => cat.ContainsAnyOf("Pillows", "Throws") || name.Contains("Pouf")},
            {HomewareCategory.Sconces, (name, cat) => name.Contains("Sconce")},
            {HomewareCategory.Side_Tables, (name, cat) => cat.Contains("Side Tables") || name.Contains("Sidetable")},
            {HomewareCategory.Sofas, (name, cat) => cat.Contains("Sofas")},
            {HomewareCategory.Shelves, (name, cat) => cat.Contains("Shelving")},
            {HomewareCategory.Table_Lamps, (name, cat) => name.Contains("Table Lamp") || (name.Contains("Lamp") && cat.Contains("Table"))},
            {HomewareCategory.Vases, (name, cat) => cat.Contains("Vases")},
            {HomewareCategory.Wall_Art, (name, cat) => cat.Contains("Wall Art") || name.ContainsAnyOf("Clock", "Plaque", "Print")},
            {HomewareCategory.Wall_Lamps, (name, cat) => name.ContainsAnyOf("Hanging Light", "Ceiling Light", "Wall Light", 
                "Wall Lamp", "Ceiling Lamp", "Hurricane") || name.EndsWith("Light")},
            {HomewareCategory.Wine_Racks, (name, cat) => name.ContainsAnyOf("Rack", "Wine")},
        };
    }
}

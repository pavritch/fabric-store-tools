using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace CurreyCo
{
    public class CurreyDimensions
    {
        public double Depth { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Diameter { get; set; }
    }

    public class CurreyProductBuilder : ProductBuilder<CurreyVendor>
    {
        public CurreyProductBuilder(IPriceCalculator<CurreyVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var name = FormatName(data[ScanField.ProductName]);

            var vendor = new CurreyVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(IsInStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));
            homewareProduct.IsDiscontinued = data.IsDiscontinued;
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category], name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accessories, (cat, name) => name.ContainsAnyOf("Anjou", "Lion", "Extension Rod", "Bottle", "Jewelry Box")},
            {HomewareCategory.Accent_Chairs, (cat, name) => name.ContainsAnyOf("Chair", "Armchair")},
            {HomewareCategory.Accent_Tables, (cat, name) => name.ContainsAnyOf("Dumbwaiter", "Accent Table", "Occasional Table", "Card Table")},
            {HomewareCategory.Bar_Stools, (cat, name) => name.Contains("Stool")},
            {HomewareCategory.Bar_Carts, (cat, name) => name.Contains("Bar Cart")},
            {HomewareCategory.Birdbaths, (cat, name) => name.Contains("Birdbath")},
            {HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            {HomewareCategory.Cabinets, (cat, name) => name.Contains("Cabinet")},
            {HomewareCategory.Candleholders, (cat, name) => name.Contains("Candelabra")},
            {HomewareCategory.Chains, (cat, name) => name.Contains("Chain")},
            {HomewareCategory.Chandeliers, (cat, name) => name.Contains("Chandelier")},
            {HomewareCategory.Coat_Racks, (cat, name) => name.Contains("Coat")},
            {HomewareCategory.Coffee_Tables, (cat, name) => name.ContainsAnyOf("Trunk", "Coffee Table", "Cocktail Table", "Drinks Table")},
            {HomewareCategory.Console_Tables, (cat, name) => name.ContainsAnyOf("Console Table", "Console") && !name.Contains("Lamp")},
            {HomewareCategory.Desks, (cat, name) => name.Contains("Desk")},
            {HomewareCategory.Dining_Tables, (cat, name) => name.Contains("Dining Table")},
            {HomewareCategory.Dressers, (cat, name) => name.ContainsAnyOf("Chest of Drawers", "Etagere", "Chest", "Highboy")},
            {HomewareCategory.End_Tables, (cat, name) => name.Contains("Corner Table")},
            {HomewareCategory.Floor_Lamps, (cat, name) => name.ContainsAnyOf("Floor Lamp", "Torchiere")},
            {HomewareCategory.Garden_Decorations, (cat, name) => name.Contains("Obelisk")},
            {HomewareCategory.Garden_Planters, (cat, name) => name.Contains("Terrarium")},
            {HomewareCategory.Love_Seats, (cat, name) => name.ContainsAnyOf("Settee", "Daybed", "Day Bed", "Banquette")},
            {HomewareCategory.Mirrors, (cat, name) => name.Contains("Mirror")},
            {HomewareCategory.Nightstands, (cat, name) => name.ContainsAnyOf("Night Stand", "Nightstand")},
            {HomewareCategory.Ottomans, (cat, name) => name.Contains("Ottoman")},
            {HomewareCategory.Pendants, (cat, name) => name.ContainsAnyOf("Pendant", "Lantern")},
            {HomewareCategory.Room_Screens, (cat, name) => name.Contains("Screen")},
            //{HomewareCategory.Rugs, (cat, name) => name.Contains("Rug")},
            {HomewareCategory.Sconces, (cat, name) => name.ContainsAnyOf("Wall Sconce", "Wall sconce", "Sconce")},
            {HomewareCategory.Shades, (cat, name) => name.Contains("Shade")},
            {HomewareCategory.Sideboards, (cat, name) => name.ContainsAnyOf("Credenza", "Sideboard", "Buffet")},
            {HomewareCategory.Side_Tables, (cat, name) => name.ContainsAnyOf("Cellarette", "Side Table")},
            {HomewareCategory.Sofas, (cat, name) => name.ContainsAnyOf("Chaise", "Sofa")},
            {HomewareCategory.Table_Lamps, (cat, name) => name.ContainsAnyOf("Table Lamp", "Console Lamp", "Bookcase Lamp")},
            {HomewareCategory.Vanities, (cat, name) => name.Contains("Vanity")},
            {HomewareCategory.Wall_Art, (cat, name) => name.ContainsAnyOf("Wall Ornament", "Wall Bracket", "Valentine", "Ceiling Mount", "Flush Mount", "Semi-Flush")},
        };

        private HomewareCategory AssignCategory(string categoryName, string productName)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName, productName))
                    return category.Key;
            if (productName.Contains("Table")) return HomewareCategory.Accent_Tables;
            return HomewareCategory.Unknown;
        }

        private string FormatName(string productName)
        {
            productName = productName.Replace("?", "'");
            productName = productName.Replace("\"", "");
            productName = productName.Replace("Ã‰tagÃ¨re", "Etagere");
            return productName;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dimensions = GetDimensions(data[ScanField.Dimensions]);

            var features = new HomewareProductFeatures();
            features.Depth = dimensions.Depth;
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = dimensions.Height;
            features.ShippingWeight = data[ScanField.ShippingWeight].ToDoubleSafe();
            features.Width = dimensions.Width;
            return features;
        }

        private CurreyDimensions GetDimensions(string dimensions)
        {
            dimensions = dimensions.RemovePattern(" Adjustable.*");

            var currDim = new CurreyDimensions();
            var parts = dimensions.Split(new[] {'x'}).Select(x => x.Trim()).ToList();

            // some of the dimensions are just "32 x 17"
            if (parts.All(x => !x.ContainsAnyOf("d", "h", "w")))
            {
                if (parts.Count >= 1) currDim.Height = parts[0].ToDoubleSafe();
                if (parts.Count >= 2) currDim.Width = parts[1].ToDoubleSafe();
                return currDim;
            }

            foreach (var part in parts)
            {
                if (part.ContainsIgnoreCase("d")) currDim.Depth = part.Replace("d", "").MeasurementFromFraction().ToDoubleSafe();
                if (part.ContainsIgnoreCase("h")) currDim.Height = part.Replace("h", "").MeasurementFromFraction().ToDoubleSafe();
                if (part.ContainsIgnoreCase("w")) currDim.Width = part.Replace("w", "").MeasurementFromFraction().ToDoubleSafe();
                if (part.ContainsIgnoreCase("rd")) currDim.Diameter = part.Replace("rd", "").MeasurementFromFraction().ToDoubleSafe();
            }
            return currDim;
        }

        private string FormatFinish(string finish)
        {
            finish = finish.Replace("Annato", "Annatto");
            finish = finish.Replace("Black w", "Black");
            finish = finish.Replace("Glass w", "Glass");
            finish = finish.Replace("Light  Anique", "Light Antique");
            finish = finish.Replace("MolÂ©", "Mol");
            finish = finish.Replace("Off-White", "Off White");
            return finish.TitleCase();
        }

        private string FormatMaterial(string material)
        {
            material = material.Replace("Mirrror", "Mirror");
            material = material.Replace("Terra Cotta", "Terracotta");
            material = material.Replace("Terracota", "Terracotta");
            return material.TitleCase();
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.ChainLength, data[ScanField.ChainLength]},
                {ScanField.Finish, FormatFinish(data[ScanField.Finish])},
                {ScanField.Material, FormatMaterial(data[ScanField.Material])},
                {ScanField.NumberOfLights, data[ScanField.NumberOfLights]},
            };
            if (data[ScanField.Bulbs] != "0") specs.Add(ScanField.Bulbs, data[ScanField.Bulbs]);
            if (data[ScanField.Wattage] != string.Empty) specs.Add(ScanField.Wattage, data[ScanField.Wattage] + "W");
            if (data[ScanField.WattagePerLight] != string.Empty) specs.Add(ScanField.WattagePerLight, data[ScanField.WattagePerLight] + "W");
            if (!data[ScanField.Shade].Contains("page")) specs.Add(ScanField.Shade, data[ScanField.Shade]);
            return ToSpecs(specs);
        }

        private bool IsInStock(string stock)
        {
            if (stock.ContainsIgnoreCase("Call for Availability")) return false;
            return true;
        }
    }
}
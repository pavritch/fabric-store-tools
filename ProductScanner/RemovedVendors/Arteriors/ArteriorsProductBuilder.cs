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

namespace Arteriors
{
    public class ArteriorsProductBuilder : ProductBuilder<ArteriorsVendor>
    {
        public ArteriorsProductBuilder(IPriceCalculator<ArteriorsVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new ArteriorsVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount] == "In Stock"), mpn, BuildFeatures(data));

            var name = data[ScanField.ProductName].Replace("Chandeleir", "Chandelier");
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category], name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();

            // TODO: Packaging
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            { HomewareCategory.Accessories, (cat, name) => name.ContainsAnyOf("Globe", "Coffer", "Armillary", "Magnifying", "Authentic Horns", 
                "Chest", "Slipcover", "Sphere")},
            {HomewareCategory.Accent_Cabinets, (cat, name) => name.Contains("Cabinet")},
            {HomewareCategory.Accent_Chairs, (cat, name) => name.Contains("Chair")},
            {HomewareCategory.Accent_Shelves, (cat, name) => name.Contains("Shelves")},
            {HomewareCategory.Accent_Stools, (cat, name) => name.Contains("Stool") && !name.Contains("Bar Stool") && !name.Contains("Counter Stool") && !name.Contains("Barstool")},
            {HomewareCategory.Accent_Tables, (cat, name) => name.ContainsAnyOf("Accent Table", "Entry Table", "Occasional Table")},
            {HomewareCategory.Andirons, (cat, name) => name.Contains("Andirons")},
            {HomewareCategory.Bar_Carts, (cat, name) => name.ContainsAnyOf("Bar Cart", "Tiered Stand")},
            {HomewareCategory.Bar_Stools, (cat, name) => name.ContainsAnyOf("Bar Stool", "Counter Stool", "Barstool")},
            {HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            {HomewareCategory.Bookcases, (cat, name) => name.Contains("Bookshelf")},
            {HomewareCategory.Bookends, (cat, name) => name.Contains("Bookends")},
            {HomewareCategory.Bowls, (cat, name) => name.Contains("Bowl")},
            {HomewareCategory.Cabinets, (cat, name) => name.Contains("Cabinets")},
            {HomewareCategory.Candleholders, (cat, name) => name.ContainsAnyOf("Pillar Holder", "Candlesticks", "Candelabra", "Hurricane", "Candle Holder")},
            {HomewareCategory.Centerpieces, (cat, name) => name.Contains("Centerpiece")},
            {HomewareCategory.Chains, (cat, name) => cat.Contains("Chain") || name.Contains(" Chain")},
            {HomewareCategory.Chandeliers, (cat, name) => cat.Contains("Chandeliers") || name.Contains("Chandelier")},
            {HomewareCategory.Cocktail_Shakers, (cat, name) => name.Contains("Cocktail Shaker")},
            {HomewareCategory.Coffee_Tables, (cat, name) => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, (cat, name) => name.Contains("Console")},
            {HomewareCategory.Decanters, (cat, name) => name.Contains("Decanter")},
            {HomewareCategory.Decorative_Boxes, (cat, name) => name.Contains("Box")},
            {HomewareCategory.Decorative_Containers, (cat, name) => name.Contains("Container")},
            {HomewareCategory.Decorative_Trays, (cat, name) => cat.Contains("Trays") || name.Contains("Tray")},
            {HomewareCategory.Desks, (cat, name) => name.Contains("Desk")},
            {HomewareCategory.Dining_Tables, (cat, name) => name.Contains("Dining Table")},
            {HomewareCategory.End_Tables, (cat, name) => name.ContainsAnyOf("End Table", "Cocktail Table")},
            {HomewareCategory.Fire_Screens, (cat, name) => cat.Contains("Fireplace") && name.Contains("Screen") || name.Contains("Lancelot Screen")},
            {HomewareCategory.Floor_Lamps, (cat, name) => cat.Contains("Floor Lamps") || name.Contains("Floor Lamp")},
            {HomewareCategory.Garden_Planters, (cat, name) => name.Contains("Cachepot")},
            {HomewareCategory.Ice_Buckets, (cat, name) => name.Contains("Ice Bucket")},
            {HomewareCategory.Jars, (cat, name) => name.Contains("Small Jar")},
            {HomewareCategory.Log_Holders, (cat, name) => name.Contains("Log Holder")},
            {HomewareCategory.Mirrors, (cat, name) => cat.Contains("Mirrors") || name.Contains("Mirror") && !name.Contains("Sconce")},
            {HomewareCategory.Ottomans, (cat, name) => name.Contains("Ottoman")},
            {HomewareCategory.Pendants, (cat, name) => cat.Contains("Pendants") || name.Contains("Pendant")},
            {HomewareCategory.Room_Screens, (cat, name) => name.ContainsAnyOf("Room Screen", "Screen")},
            {HomewareCategory.Sconces, (cat, name) => cat.Contains("Sconces") || name.ContainsAnyOf("Sconce", "Lantern")},
            {HomewareCategory.Shades, (cat, name) => cat.Contains("Shades")},
            {HomewareCategory.Shelves, (cat, name) => name.Contains("Etagere")},
            {HomewareCategory.Side_Tables, (cat, name) => name.ContainsAnyOf("Side Table", "Table")},
            {HomewareCategory.Sofas, (cat, name) => name.Contains("Sofa") || name.Contains("Settee")},
            {HomewareCategory.Statues_and_Sculptures, (cat, name) => cat.Contains("Sculptures") || 
                name.ContainsAnyOf("Sculpture", "Urn", "Armillary")},
            {HomewareCategory.Table_Lamps, (cat, name) => cat.Contains("Table Lamps") || name.ContainsAnyOf("Cloche", "Desk Lamp", "Table Lamp") 
                || (name.Contains("Lamp") && !name.Contains("Floor"))},
            {HomewareCategory.Tool_Sets, (cat, name) => name.Contains("Fireplace Tool Set")},
            {HomewareCategory.Towel_Holders, (cat, name) => name.Contains("Towel Holder")},
            {HomewareCategory.Vases, (cat, name) => cat.Contains("Vases") || name.Contains("Vase")},
            {HomewareCategory.Vanity_Lights, (cat, name) => name.Contains("Vanity Light")},
            {HomewareCategory.Wall_Art, (cat, name) => name.Contains("Plaque")},
            {HomewareCategory.Wall_Lamps, (cat, name) => name.ContainsAnyOf("Torchiere", "Mount", "Ceiling Light", "Flush")},
        };

        private HomewareCategory AssignCategory(string vendorCategory, string name)
        {
            foreach (var category in _categories)
                if (category.Value(vendorCategory, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Color = FormatColor(data[ScanField.Color]);
            features.Depth = GetDimension(data[ScanField.Depth], "D");
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = GetDimension(data[ScanField.Height], "H");
            //features.Length = GetDimension(data[ScanField.Dimensions], "L");
            features.ShippingWeight = FormatShippingWeight(data[ScanField.Packaging]).ToDoubleSafe();
            features.Width = GetDimension(data[ScanField.Width], "W");
            return features;
        }

        private string FormatShippingWeight(string packaging)
        {
            return packaging.CaptureWithinMatchedPattern(@"Net Weight: (?<capture>(.*)) lbs");
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Designer, data[ScanField.Designer].Replace(" for Arteriors", "")},
                {ScanField.Finish, FormatFinish(data[ScanField.Finish])},
                {ScanField.Material, FormatMaterial(data[ScanField.Material])},
                {ScanField.ShadeBottomDiameter, data[ScanField.ShadeBottomDiameter]},
                {ScanField.ShadeTopDiameter, data[ScanField.ShadeTopDiameter]},
                {ScanField.ShadeSide, data[ScanField.ShadeSide]},
                {ScanField.Shape, data[ScanField.Shape].Trim(',').TitleCase()},

                {ScanField.Bulb, data[ScanField.Bulb]},
                {ScanField.CordColor, data[ScanField.CordColor]},
                {ScanField.CordType, data[ScanField.CordType]},
                {ScanField.CordLength, data[ScanField.CordLength]},
                {ScanField.DampRatedCovered, data[ScanField.DampRatedCovered]},
                {ScanField.HarpSize, data[ScanField.HarpSize]},
                {ScanField.LampBottom, data[ScanField.LampBottom]},
                {ScanField.Plug, data[ScanField.Plug]},
                {ScanField.SocketQuantity, data[ScanField.SocketQuantity]},
                {ScanField.SocketType, data[ScanField.SocketType]},
                {ScanField.SocketWattage, data[ScanField.SocketWattage]},
                {ScanField.SwitchColor, data[ScanField.SwitchColor]},
                {ScanField.SwitchLocation, data[ScanField.SwitchLocation]},
                {ScanField.SwitchType, data[ScanField.SwitchType]},
                {ScanField.Voltage, data[ScanField.Voltage]},
                {ScanField.WirewayPosition, data[ScanField.WirewayPosition]},
            };
            if (data[ScanField.Diameter] != "0") specs.Add(ScanField.Diameter, GetDimension(data[ScanField.Diameter], "Dia").ToString());
            return ToSpecs(specs);
        }

        private double GetDimension(string dimensions, string key)
        {
            var dim = dimensions.Replace(key + ": ", "").Replace("in", "");
            if (!dim.Contains("-")) return dim.ToDoubleSafe();

            dim = dim.Substring(0, dim.IndexOf("-")).Trim();
            return dim.ToDoubleSafe();
        }

        private string FormatColor(string color)
        {
            color = color.Trim(',');
            var colors = color.Split(new[] {','}).Select(x => new ItemColor(x).GetFormattedColor()).Distinct();
            return string.Join(", ", colors);
        }

        private string FormatFinish(string finish)
        {
            var finishes = finish.Trim(',').Split(new[] {','}).Select(x => x.Trim().TitleCase()).Distinct();
            return string.Join(", ", finishes);
        }

        private string FormatMaterial(string material)
        {
            var materials = material.Trim(',').Split(new[] {','}).Distinct();
            materials = materials.Where(x => !x.ContainsDigit()).Select(x => x.Trim().TitleCase()).ToList();
            return string.Join(", ", materials);
        }
    }
}
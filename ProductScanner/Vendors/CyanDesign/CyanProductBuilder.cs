using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace CyanDesign
{
    public class CyanProductBuilder : ProductBuilder<CyanDesignVendor>
    {
        public CyanProductBuilder(IPriceCalculator<CyanDesignVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new CyanDesignVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToIntegerSafe()), mpn, BuildFeatures(data));

            var name = FormatName(data[ScanField.ProductName]);
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category].Replace("Décor", "Decor"), name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private string FormatName(string name)
        {
            name = name.Replace("-Ch", "");
            name = name.Replace("-Chs", "");
            name = name.Replace("-Gg", "");

            name = name.Replace("-CH", "");
            name = name.Replace("-CHS", "");
            name = name.Replace("-CHR", "");
            name = name.Replace("-CLR", "");
            name = name.Replace("-GG", "");
            name = name.Replace("-PWH", "");
            name = name.Replace("-SRB", "");
            name = name.Replace("-VI", "");

            name = name.Replace("Café", "Cafe");
            name = name.Replace("Décor", "Decor");

            name = name.ReplaceWholeWord("Lg.", "Large");
            name = name.ReplaceWholeWord("Lg", "Large");
            name = name.ReplaceWholeWord("Md.", "Medium");
            name = name.ReplaceWholeWord("Md", "Medium");
            name = name.ReplaceWholeWord("Med", "Medium");
            name = name.ReplaceWholeWord("Med.", "Medium");
            name = name.ReplaceWholeWord("Sm.", "Small");
            name = name.ReplaceWholeWord("Sm", "Small");

            name = name.Replace("CHANDLR", "Chandelier");
            name = name.Replace("CHAND", "Chandelier");

            name = name.ReplaceWholeWord("AMBR", "Amber");
            name = name.ReplaceWholeWord("Blk", "Black");
            name = name.ReplaceWholeWord("Blu", "Blue");
            name = name.ReplaceWholeWord("Blu/Grn", "Blue/Green");
            name = name.ReplaceWholeWord("CHAND", "Chandelier");
            name = name.ReplaceWholeWord("Chdlr", "Chandelier");
            name = name.ReplaceWholeWord("Chandlr", "Chandelier");
            name = name.ReplaceWholeWord("Chndlr", "Chandelier");
            name = name.ReplaceWholeWord("Chndlir", "Chandelier");
            name = name.ReplaceWholeWord("Chandlier", "Chandelier");
            name = name.ReplaceWholeWord("Cndlhldr", "Candleholder");
            name = name.ReplaceWholeWord("Cndlhr", "Candleholder");
            name = name.ReplaceWholeWord("Cndlhdr", "Candleholder");
            name = name.ReplaceWholeWord("Cnsle", "Console");
            name = name.ReplaceWholeWord("Contempo", "Contemporary");
            name = name.ReplaceWholeWord("Contnr", "Container");
            name = name.ReplaceWholeWord("Plntr", "Planter");
            name = name.ReplaceWholeWord("Pndt", "Pendant");
            name = name.ReplaceWholeWord("Holdr", "Holder");
            name = name.ReplaceWholeWord("3pc", "3 Piece");
            name = name.ReplaceWholeWord("Stnd", "Stand");
            name = name.ReplaceWholeWord("Lt.", "Light");
            name = name.ReplaceWholeWord("Sclptre", "Sculpture");
            name = name.ReplaceWholeWord("1LT", "1 Light");
            name = name.ReplaceWholeWord("2LT", "2 Light");
            name = name.ReplaceWholeWord("3LT", "3 Light");
            name = name.ReplaceWholeWord("4LT", "4 Light");
            name = name.ReplaceWholeWord("5LT", "5 Light");
            name = name.ReplaceWholeWord("6LT", "6 Light");
            name = name.ReplaceWholeWord("Brkt", "Bracket");
            name = name.ReplaceWholeWord("Mnt", "Mount");
            name = name.ReplaceWholeWord("Vse", "Vase");
            name = name.ReplaceWholeWord("Tbl", "Table");
            return name.TitleCase();
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accent_Chairs, (cat, name) => name.Contains("Chair")},
            {HomewareCategory.Accessories, (cat, name) => cat.Contains("Filler")},
            {HomewareCategory.Bar_Carts, (cat, name) => name.Contains("Bar Cart")},
            {HomewareCategory.Bar_Stools, (cat, name) => name.Contains("Stool")},
            {HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            {HomewareCategory.Bookends, (cat, name) => name.Contains("Bookends")},
            {HomewareCategory.Bookcases, (cat, name) => name.Contains("Book Shelf")},
            {HomewareCategory.Bowls, (cat, name) => name.Contains("Bowl")},
            {HomewareCategory.Cabinets, (cat, name) => name.Contains("Cabinet")},
            {HomewareCategory.Candleholders, (cat, name) => cat.Contains("Candle Holders")},
            {HomewareCategory.Chandeliers, (cat, name) => name.Contains("Chandelier")},
            {HomewareCategory.Coffee_Tables, (cat, name) => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, (cat, name) => name.Contains("Console")},
            {HomewareCategory.Decorative_Pillows, (cat, name) => cat.Contains("Pillows")},
            {HomewareCategory.Decorative_Trays, (cat, name) => cat.Contains("Trays")},
            {HomewareCategory.Fire_Screens, (cat, name) => name.ContainsAnyOf("Firescreen", "Fire Screen")},
            {HomewareCategory.Mirrors, (cat, name) => cat.Contains("Mirrors")},
            {HomewareCategory.Ottomans, (cat, name) => name.Contains("Ottoman")},
            {HomewareCategory.Pendants, (cat, name) => cat.Contains("Pendants") || name.Contains("Pendant")},
            {HomewareCategory.Garden_Planters, (cat, name) => name.Contains("Planter")},
            {HomewareCategory.Plates, (cat, name) => name.Contains("Plate")},
            {HomewareCategory.Sconces, (cat, name) => cat.Contains("Sconces")},
            {HomewareCategory.Shelves, (cat, name) => cat.Contains("Etageres")},
            {HomewareCategory.Side_Tables, (cat, name) => name.Contains("Side Table") || (name.Contains("Table") && !name.Contains("Lamp"))},
            {HomewareCategory.Statues_and_Sculptures, (cat, name) => cat.Contains("Sculptur")},
            {HomewareCategory.Table_Lamps, (cat, name) => cat.Contains("Lamps") || name.Contains("Table Lamp")},
            {HomewareCategory.Vanities, (cat, name) => name.Contains("Vanity")},
            {HomewareCategory.Vases, (cat, name) => name.Contains("Vase")},
            {HomewareCategory.Wall_Art, (cat, name) => cat.Contains("Wall Decor")},
            {HomewareCategory.Wall_Lamps, (cat, name) => cat.Contains("Ceiling Mounts")},

            {HomewareCategory.Ignored, (cat, name) => name.Contains("Rug")},
        };

        private HomewareCategory AssignCategory(string itemCategory, string name)
        {
            foreach (var category in _categories)
                if (category.Value(itemCategory, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dimensions = new CyanDimensions(data[ScanField.Dimensions]);
            var features = new HomewareProductFeatures();
            features.Color = data[ScanField.Color].TitleCase();
            features.Depth = dimensions.Length;
            features.Features = BuildSpecs(data);
            features.Height = dimensions.Height;
            features.ShippingWeight = data[ScanField.ShippingWeight].ToDoubleSafe();
            features.Width = dimensions.Width;
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var dimensions = new CyanDimensions(data[ScanField.Dimensions]);
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Country, data[ScanField.Country]},
                {ScanField.Finish, data[ScanField.Finish].TitleCase()},
                {ScanField.Material, data[ScanField.Material].Replace(" // ", " and ")},
                {ScanField.NumberOfLights, data[ScanField.NumberOfLights]},
                {ScanField.ShadeColor, data[ScanField.ShadeColor].TitleCase()},
                {ScanField.Socket, data[ScanField.Socket]},
            };

            if (data[ScanField.Bulbs] != "0") specs.Add(ScanField.Bulbs, data[ScanField.Bulbs]);
            if (data[ScanField.Diameter] != "0") specs.Add(ScanField.Diameter, dimensions.Diameter.ToString().ToInchesMeasurement());
            if (data[ScanField.ItemExtension] != "0") specs.Add(ScanField.ItemExtension, data[ScanField.ItemExtension].ToInchesMeasurement());
            if (data[ScanField.WattagePerLight] != string.Empty) specs.Add(ScanField.WattagePerLight, data[ScanField.WattagePerLight] + "W");
            return ToSpecs(specs);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Nuevo
{
    public class NuevoProductBuilder : ProductBuilder<NuevoVendor>
    {
        public NuevoProductBuilder(IPriceCalculator<NuevoVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var features = BuildFeatures(data);
            var vendor = new NuevoVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, features);
            var name = data[ScanField.ProductName].TitleCase() + " " + features.Color;

            homewareProduct.ManufacturerDescription = data[ScanField.Description].TitleCase();
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductGroup].TitleCase());

            if (name.Contains("Palma")) homewareProduct.HomewareCategory = HomewareCategory.Bar_Stools;
            homewareProduct.Name = name;
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();

            var dimensionsStr = data[ScanField.Dimensions].Replace("size : ", "").Replace("¾", ".75");
            if (dimensionsStr.Contains("-"))
            {
                dimensionsStr = dimensionsStr.Substring(0, dimensionsStr.IndexOf("-") - 1);
            }
            var dims = dimensionsStr.Split(new[] {'x'}).Select(x => x.Trim(new []{' ', '"', ':'})).ToList();
            if (dims.Count == 3)
            {
                features.Depth = dims[0].ToDoubleSafe();
                features.Width = dims[1].ToDoubleSafe();
                features.Height = dims[2].ToDoubleSafe();
            }
            if (dims.Count == 2)
            {
                features.Depth = dims[0].ToDoubleSafe();
                features.Width = dims[1].ToDoubleSafe();
            }
            if (dims.Count == 1)
            {
                features.Width = dims[0].ToDoubleSafe();
            }
            features.Color = FormatColor(data[ScanField.Color]);
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Dimensions, data[ScanField.Dimensions]},
            };
            return ToSpecs(specs);
        }

        private string FormatColor(string color)
        {
            return string.Join(", ", color.Replace("|", ",").Split(',').Select(x => x.Trim().TitleCase()));
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accessories, group => group.ContainsAnyOf("Games", "Pedestal")},
            {HomewareCategory.Accent_Cabinets, group => group.Contains("Cabinet")},
            {HomewareCategory.Accent_Chairs, group => group.ContainsAnyOf("Lounge Chair", "Lounger")},
            {HomewareCategory.Bar_Stools, group => group.Contains("Stool")},
            {HomewareCategory.Bar_Tables, group => group.Contains("Bar")},
            {HomewareCategory.Beds, group => group.Contains("Bed")},
            {HomewareCategory.Benches, group => group.Contains("Bench")},
            {HomewareCategory.Bookcases, group => group.Contains("Bookcase")},
            {HomewareCategory.Bulbs, group => group.Contains("Bulbs")},
            {HomewareCategory.Coat_Racks, group => group.ContainsAnyOf("Rack", "Capping")},
            {HomewareCategory.Coffee_Tables, group => group.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, group => group.Contains("Console")},
            {HomewareCategory.Decorative_Pillows, group => group.Contains("Cushion")},
            {HomewareCategory.Desks, group => group.Contains("Desk")},
            {HomewareCategory.Desk_Chairs, group => group.Contains("Office Chair")},
            {HomewareCategory.Dining_Chairs, group => group.Contains("Dining Chair")},
            {HomewareCategory.Dining_Tables, group => group.ContainsAnyOf("Dining Table", "Furniture")},
            {HomewareCategory.Dressers, group => group.Contains("Dresser")},
            {HomewareCategory.Floor_Lamps, group => group.Contains("Floor Lamp")},
            {HomewareCategory.Gathering_Table, group => group.ContainsAnyOf("Buffet", "Bistro Table")},
            {HomewareCategory.Lamp_Parts, group => group.ContainsAnyOf("Lamp Parts", "Light Panel")},
            {HomewareCategory.Mirrors, group => group.Contains("Mirror")},
            {HomewareCategory.Pendants, group => group.Contains("Pendant")},
            {HomewareCategory.Sconces, group => group.Contains("Sconce")},
            {HomewareCategory.Shelves, group => group.Contains("Shelving")},
            {HomewareCategory.Side_Tables, group => group.Contains("Side Table")},
            {HomewareCategory.Sofas, group => group.Contains("Sofa")},
            {HomewareCategory.Table_Lamps, group => group.ContainsAnyOf("Table Lamp", "Wall Lamp", "Ceiling Lamp")},
            {HomewareCategory.TV_Stands, group => group.Contains("Media Unit")},
        };

        private HomewareCategory AssignCategory(string name)
        {
            foreach (var category in _categories)
                if (category.Value(name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }
    }
}
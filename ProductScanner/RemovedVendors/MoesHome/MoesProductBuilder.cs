using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace MoesHome
{
    public class MoesProductBuilder : ProductBuilder<MoesHomeVendor>
    {
        public MoesProductBuilder(IPriceCalculator<MoesHomeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            data.Cost = data[ScanField.Cost].ToDecimalSafe();

            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new MoesHomeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToDoubleSafe()), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Breadcrumbs], data[ScanField.ProductName].TitleCase());
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.MinimumQuantity = data[ScanField.MinimumQuantity].ToIntegerSafe();
            homewareProduct.Name = data[ScanField.ProductName].TitleCase();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, 
                string.Format("http://scanner.insidefabric.com/vendors/Moes/{0}.jpg", mpn)) };
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Chairs, name => name.Contains("Chair") && !name.Contains("Dining")},
            {HomewareCategory.Accent_Tables, name => name.Contains("Accent Table")},
            {HomewareCategory.Bar_Stools, name => name.Contains("Stool")},
            {HomewareCategory.Bar_Tables, name => name.Contains("Bar Table") || name.Contains("Bartable") || name.Contains("Countertable")},
            {HomewareCategory.Beds, name => name.Contains("Bed") || name.Contains("Headboard")},
            {HomewareCategory.Benches, name => name.Contains("Bench")},
            {HomewareCategory.Bookcases, name => name.Contains("Bookshelf") || name.Contains("Bookcase")},
            {HomewareCategory.Cabinets, name => name.Contains("Cabinet")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, name => name.Contains("Console Table")},
            {HomewareCategory.Decorative_Pillows, name => name.Contains("Pillow") || name.Contains("Cushion") || name.Contains("Goat Fur")},
            {HomewareCategory.Desks, name => name.Contains("Desk")},
            {HomewareCategory.Dining_Chairs, name => name.Contains("Dining Chair")},
            {HomewareCategory.Dining_Tables, name => name.Contains("Dining Table")},
            {HomewareCategory.Dressers, name => name.Contains("Dresser") || name.Contains("Tallboy")},
            {HomewareCategory.End_Tables, name => name.Contains("End Table")},
            {HomewareCategory.Floor_Lamps, name => name.Contains("Floor Lamp")},
            {HomewareCategory.Mirrors, name => name.Contains("Mirror")},
            {HomewareCategory.Nightstands, name => name.Contains("Nightstand") || name.Contains("Night Stand")},
            {HomewareCategory.Pendants, name => name.Contains("Pendant")},
            {HomewareCategory.Poufs, name => name.Contains("Pouf")},
            {HomewareCategory.Room_Screens, name => name.Contains("Screen") || name.Contains("Divider")},
            {HomewareCategory.Shelves, name => name.Contains("Shelf")},
            {HomewareCategory.Sideboards, name => name.Contains("Sideboard")},
            {HomewareCategory.Side_Tables, name => name.Contains("Side Table") || name.Contains("Sidetable")},
            {HomewareCategory.Sofas, name => name.Contains("Sectional") || name.Contains("Sofa") || name.Contains("Chaise") || name.Contains("Seater")},
            {HomewareCategory.Statues_and_Sculptures, name => name.Contains("Sculpture") || name.Contains("Statue")},
            {HomewareCategory.Table_Lamps, name => name.Contains("Table Lamp")},
            {HomewareCategory.TV_Stands, name => name.Contains("TV Stand") || name.Contains("TV Table") || name.Contains("Entertainment Unit")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
            {HomewareCategory.Wall_Art, name => name.Contains("Décor") || name.Contains("Wall Decor")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPassCategories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Wall_Art, cat => cat.Contains("Decorative") || cat.Contains("Wall Décor")},
        }; 

        private HomewareCategory AssignCategory(string breadcrumbs, string name)
        {
            foreach (var category in _categories)
                if (category.Value(name))
                    return category.Key;

            foreach (var category in _secondPassCategories)
                if (category.Value(breadcrumbs))
                    return category.Key;
            return HomewareCategory.Accent_Furniture;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Features = BuildSpecs(data);
            features.Color = data[ScanField.Color].TitleCase();
            features.Depth = data[ScanField.Depth].ToDoubleSafe();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Height = data[ScanField.Height].ToDoubleSafe();
            features.ShippingWeight = data[ScanField.Weight].ToDoubleSafe();
            features.UPC = data[ScanField.UPC];
            features.Width = data[ScanField.Width].ToDoubleSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>()
            {
                {ScanField.Bulbs, data[ScanField.Bulbs].Replace(", Type B", "").Replace(" Bulb", "").Replace(" Bulbs", "")},
                {ScanField.Country, data[ScanField.Country]},
                {ScanField.Material, data[ScanField.Material]},
                {ScanField.Wattage, data[ScanField.Wattage]},
            };
            if (data[ScanField.CordLength] != "N/A") specs.Add(ScanField.CordLength, data[ScanField.CordLength].Replace(" Mtr", " Meters").Replace(" m", " Meters").Replace(" METERS", " Meters").TitleCase());
            return ToSpecs(specs);
        }
    }
}
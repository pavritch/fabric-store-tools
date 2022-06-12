using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Astek.Details
{
    public class AstekProductBuilder : IProductBuilder<AstekVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            var mpn = data[ProductPropertyType.ManufacturerPartNumber];
            var retail = data[ProductPropertyType.RetailPrice].ToDecimalSafe();

            var vendor = new AstekVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, retail * .45m, true, vendor);

            vendorProduct.SetRetailPrice(retail);
            vendorProduct.SetOurPrice(retail * .7m);

            var tc1 = data[ProductPropertyType.TempContent1];
            var tc3 = data[ProductPropertyType.TempContent3];
            var category = FormatCategory(data[ProductPropertyType.Category]);
            var productName = FormatName(data[ProductPropertyType.ProductName], mpn);

            vendorProduct.PublicProperties[ProductPropertyType.AdditionalInfo] = FindMatches(_additionalInfoWords, tc1);
            vendorProduct.PublicProperties[ProductPropertyType.Backing] = FindMatches(_backingWords, tc1);
            vendorProduct.PublicProperties[ProductPropertyType.Category] = category;
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FindMatches(_cleaningWords, tc1);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = FindCollection(data[ProductPropertyType.ProductName]);
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = FindColorGroup(data);
            vendorProduct.PublicProperties[ProductPropertyType.Construction] = FindMatches(_materialWords, tc1);
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = FindMatches(_designers, data[ProductPropertyType.ProductName]);
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FindMatches(_durabilityWords, tc1);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatHorizontalRepeat(tc3);
            vendorProduct.PublicProperties[ProductPropertyType.Match] = FindMatches(_matchWords, tc3);
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ProductPropertyType.Material];
            vendorProduct.PublicProperties[ProductPropertyType.ProductGroup] = FindProductGroup(data[ProductPropertyType.ProductName]);
            vendorProduct.PublicProperties[ProductPropertyType.ProductName] = productName;
            vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = data[ProductPropertyType.TempContent1].Contains("Pre-Pasted") ? "Yes" : "No";
            vendorProduct.PublicProperties[ProductPropertyType.Strippable] = data[ProductPropertyType.TempContent1].Contains("Strippable") ? "Yes" : "No";
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(tc3);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data[ProductPropertyType.ProductDetailUrl];

            var parts = data[ProductPropertyType.Dimensions].Replace("Dimensions:", "").Split(new[] {'X'});
            var width = parts.First().Replace(" inches", "").Trim().ToDoubleSafe();
            if (parts.Length > 1)
            {
                var lengthStr = parts.Last();
                var lengthInches = 0d;
                if (lengthStr.Contains("yds"))
                {
                    var lengthInYards = lengthStr.Replace(" yds", "").ToDoubleSafe();
                    lengthInches = lengthInYards*12*3;
                    vendorProduct.PublicProperties[ProductPropertyType.Length] = lengthInYards + " yards";
                }
                if (lengthStr.Contains("ft"))
                {
                    var lengthInFeet = lengthStr.Replace(" ft", "").ToDoubleSafe();
                    lengthInches = lengthInFeet*12;
                    vendorProduct.PublicProperties[ProductPropertyType.Length] = lengthInFeet + " feet";
                }

                var dimensions = new Dimensions(width, lengthInches);
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();

                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            }

            if (width > 0)
                vendorProduct.PublicProperties[ProductPropertyType.Width] = width + " inches";
            vendorProduct.PublicProperties[ProductPropertyType.ManufacturerPartNumber] = RemoveFromMPN(mpn);

            vendorProduct.Correlator = mpn;
            vendorProduct.ManufacturerDescription = data[ProductPropertyType.Description];
            vendorProduct.Name = BuildName(mpn, productName, vendor.DisplayName);
            vendorProduct.ProductGroup = ProductGroup.Wallcovering;
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = GetUnitOfMeasure(data[ProductPropertyType.TempContent4], category);
            return vendorProduct;
        }

        private string BuildName(string mpn, string productName, string vendorName)
        {
            if (ExcludeMPN(mpn))
                return new[] {productName, "by", vendorName}.BuildName();
            return new[] {mpn, productName, "by", vendorName}.BuildName();
        }

        // exclude mpn from name?
        private bool ExcludeMPN(string mpn)
        {
            if (mpn.Contains("AD_Tile")) return true;
            if (mpn.StartsWith("KH_")) return true;
            if (mpn.StartsWith("WD_")) return true;
            if (mpn.StartsWith("XM_")) return true;
            if (mpn.StartsWith("JZ_")) return true;
            if (mpn.StartsWith("JF_")) return true;
            if (mpn.StartsWith("GB_")) return true;
            if (mpn.StartsWith("GG_")) return true;
            if (mpn.StartsWith("MU_")) return true;
            if (mpn.StartsWith("WD_")) return true;
            if (mpn.StartsWith("H14_")) return true;
            if (mpn.StartsWith("CV_")) return true;
            if (mpn.StartsWith("SM_")) return true;

            if (mpn.StartsWith("Houndstooth_")) return true;
            if (mpn.Contains("_original")) return true;

            // Looks like most of them >15 are ones where we don't want it in the name
            if (mpn.Length > 15) return true;

            return false;
        }

        private UnitOfMeasure GetUnitOfMeasure(string tc4, string category)
        {
            if (category.Contains("Decal")) return UnitOfMeasure.Each;
            if (category.Contains("Tile")) return UnitOfMeasure.Each;
            if (category.Contains("Mural")) return UnitOfMeasure.Each;

            if (tc4.Contains("Decal")) return UnitOfMeasure.Each;
            if (tc4.Contains("Tile")) return UnitOfMeasure.Each;
            return UnitOfMeasure.Roll;
        }

        private string RemoveFromMPN(string mpn)
        {
            foreach (var designer in _designers) mpn = mpn.Replace(designer, string.Empty);
            foreach (var designer in _designers) mpn = mpn.Replace(designer.Replace(" ", ""), string.Empty);
            foreach (var color in _colorsInNames) mpn = mpn.Replace(color, string.Empty);
            return mpn.Trim();
        }

        private string FindCollection(string name)
        {
            foreach (var collection in _collections)
            {
                if (collection.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return collection;
            }
            return string.Empty;
        }

        private string FindProductGroup(string name)
        {
            foreach (var group in _productGroups)
            {
                if (group.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return group;
            }
            return string.Empty;
        }

        private string FindColorGroup(ScanData data)
        {
            foreach (var prop in _properties)
            {
                var val = _colorGroups.FirstOrDefault(x => data[prop].ContainsIgnoreCase(x));
                if (val != null) return val;
            }
            return string.Empty;
        }

        private string FormatName(string productName, string mpn)
        {
            productName = productName.ReplaceWholeWord("wallpaper", "").Trim();
            productName = productName.ReplaceWholeWord("wallcovering", "").Trim();
            productName = productName.RemovePattern(" - WND.*");
            productName = productName.Replace(" - ", " ");
            productName = productName.Replace(":", "");
            productName = productName.Replace("&#039;", "'");
            productName = productName.Replace("&amp;", "&");
            productName = productName.Replace("&quot;", "'");

            productName = RemoveFields(productName);
            productName = productName.Replace(mpn, "").Trim();
            return productName.Trim(new[] {'-', ' '});
        }

        private string RemoveFields(string productName)
        {
            foreach (var collection in _collections) productName = productName.Replace(collection, string.Empty);
            foreach (var group in _productGroups) productName = productName.Replace(group, string.Empty);
            foreach (var designer in _designers) productName = productName.Replace(designer, string.Empty);

            return productName;
        }

        private string FormatDimensions(string dimensions)
        {
            dimensions = dimensions.Replace("Dimensions: ", "");
            var parts = dimensions.Split(new[] {'X'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count() == 1) return string.Empty;
            return dimensions;
        }

        private string FormatHorizontalRepeat(string tempContent3)
        {
            var val = tempContent3.CaptureWithinMatchedPattern(@"Repeat Width: (?<capture>(\d*\.?\d*))");
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            return val + " inches";
        }

        private string FormatVerticalRepeat(string tempContent3)
        {
            var val = tempContent3.CaptureWithinMatchedPattern(@"Repeat Height: (?<capture>(\d*\.?\d*))");
            if (string.IsNullOrWhiteSpace(val)) return string.Empty;
            return val + " inches";
        }

        private string FindMatches(List<string> options, string field)
        {
            var matches = options.Where(field.ContainsIgnoreCase).ToList();
            if (!matches.Any()) return string.Empty;
            return matches.Aggregate((a, b) => a + ", " + b);
        }

        private string FormatCategory(string category)
        {
            var toRemove = new List<string>
            {
                "Wallpaper"
            };
            var categories = category.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            categories = categories.Select(x => x.TitleCase().RomanNumeralCase().Replace("&#039;", "'")).ToList();
            categories = categories.Where(x => !toRemove.Contains(x)).ToList();
            if (!categories.Any()) return string.Empty;
            return categories.Aggregate((a, b) => a + ", " + b);
        }

        private readonly List<string> _colorGroups = new List<string>
        {
            "beige", "brown", "yellow", "aqua", "green", "olive",
            "teal", "purple", "blue", "violet", "white", "black",
            "orange", "red", "pink", "gray", "silver", "gold",
        }; 
        private readonly List<string> _additionalInfoWords = new List<string> { "Untrimmed", "Pre-trimmed"}; 
        private readonly List<string> _backingWords = new List<string> { "Self-Adhesive"}; 
        private readonly List<string> _cleaningWords = new List<string> { "Spongeable", "Scrubbable", "Washable"}; 
        private readonly List<string> _durabilityWords = new List<string> { "Good Lightfastness"};
        private readonly List<string> _matchWords = new List<string> {"Straight match", "Random match", "Half-drop match", "Offset match"}; 
        private readonly List<string> _materialWords = new List<string> { "Non Woven" }; 
        private readonly List<string> _colorsInNames = new List<string>
        {
            "Ahaus",
            "Alder Medium",
            "Ash Natural",
            "Ash White",
            "Auburn Leather",
            "Aqua Blue",
            "Becky",
            "Beige",
            "Blackwood",
            "Black and White",
            "Black Glossy",
            "Black Leather",
            "Black Matte",
            "Black Velour",
            "Blue",
            "Blue Sky",
            "Blue/Green Lisboa",
            "Brown",
            "Brushed Silver",
            "Calvados Dark Red",
            "Carbon",
            "Carrara Beige",
            "Carrara Grey",
            "Charcoal",
            "Chartres",
            "Chestnut",
            "Chocolate Levante",
            "Cinnaber",
            "Clear Smoke",
            "Cortes Blue",
            "Cortes Light Blue",
            "Cortes Rust",
            "Damast",
            "Dark Grey",
            "Dark Mahogany",
            "Elm Red",
            "Gloria",
            "Golden Walnut",
            "Green",
            "Grey",
            "High Gloss Silver",
            "High Gloss Gold",
            "Hunter Green",
            "Ice",
            "Japan Elm",
            "Japan Oak",
            "Japanese Cherry",
            "Light Alder",
            "Light Beech",
            "Light Brown",
            "Light Cherry",
            "Light Grey",
            "Light Pine",
            "Magenta",
            "Mahogany Sapele",
            "Maple",
            "Marino Green",
            "Marmi Black",
            "Marmi Green",
            "Marmi Grey",
            "Marmi White",
            "Milky",
            "Medium Cherry",
            "Medium Walnut",
            "Multicolor",
            "Natural",
            "Natural Walnut",
            "Navy Blue",
            "Night",
            "Oak",
            "Oak Light",
            "Oak natural",
            "Opal",
            "Orange",
            "Orange Glossy",
            "Pale Blue and Green",
            "Pearl",
            "Pear Tree",
            "Pine Country",
            "Porrinho",
            "Red",
            "Red Blue",
            "Red Bordeaux",
            "Red Maple",
            "Rice Paper",
            "Segovia",
            "Signal Red",
            "Snow",
            "Splinter",
            "Teal baroque",
            "Terrazzo",
            "Vario",
            "Vigo",
            "Walnut",
            "White Matte",
            "Whitewood",
            "Wild Oak",
        }; 
        private readonly List<string> _materialsInNames = new List<string>
        {
            "Topeka",
            "Bermuda",
            "Artist Canvas",
            "Silver Mylar",
            "Silver Canvas",
            "Pearl Canvas",
            "Palisades",
            "Linen",
        }; 
        private readonly List<string> _collections = new List<string>
        {
            "St. Regis Collection",
            "Anderson Design Group",
            "Gary Baseman Wallpaper",
            "Little Pond Designs",
            "Jim Flora",
            "Beverly Factor",
            "Diamonds Are Forever"
        }; 
        private readonly List<string> _productGroups = new List<string>
        {
            "Wallpaper",
            "Wall Decal",
            "Wall Tile"
        };
        private readonly List<string> _designers = new List<string>
        {
            "Karim Rashid",
            "Gary Baseman",
            "Michael Uhlenkott",
            "Jessica Swift",
            "Genevieve Carter",
            "Quincy Sutton",
            "Judy Reed Silver",
            "Talia Donag",
            "Aaron Barker",
            "Cynthia Charette"
        }; 

        // other
        // High Contrast
        // Grey Flannel with Blue
        // Day
        // Night
        // In Blue
        // Ultramarine
        // Sand Stone
        // Mid Summer Night

        private readonly List<ProductPropertyType> _properties = new List<ProductPropertyType>
        {
            ProductPropertyType.Description,
            ProductPropertyType.ProductName
        }; 
    }
}
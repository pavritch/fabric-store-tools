using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace JamesHare.Details
{
    /*
    public class JamesHareProduct : ProductBase
    {
        public override string Name
        {
            get
            {
                var nameParts = new List<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;
                    nameParts.Add(s);
                };

                add(ManufacturerPartNumber);
                add(PatternName);
                add(ColorName);
                add("by");
                add(AssociatedVendor.DisplayName);

                return string.Join(" ", nameParts);
            }
        }

        public override string SKU
        {
            get
            {
                var value = string.Format("{0}-{1}", AssociatedVendor.SkuPrefix, ManufacturerPartNumber.Replace("/", "-"));
                if (value.IsValidSKU())
                    return value;

                throw new Exception(string.Format("Invalid SKU: {0}", value));
            }
        }

        public override int StockCount
        {
            get
            {
                return 999999;
            }
        }

        [ProductProperty]
        public virtual string Collection
        {
            get
            {
                var collection = GetVendorProperty(ProductPropertyType.Collection);
                return collection.UnEscape().Trim();
            }
        }

        [ProductProperty]
        public virtual string Cleaning
        {
            get
            {
                var cleaning = GetVendorProperty(ProductPropertyType.Cleaning, true);
                if (cleaning == null) return null;

                if (cleaning.ContainsIgnoreCase("Washable")) return cleaning;
                return null;
            }
        }

        [ProductProperty]
        public virtual string PatternNumber
        {
            get
            {
                var mpn = GetVendorProperty(ProductPropertyType.ManufacturerPartNumber, true);
                return mpn.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).First();
            }
        }

        [ProductProperty]
        public virtual string ColorNumber
        {
            get
            {
                var mpn = GetVendorProperty(ProductPropertyType.ManufacturerPartNumber, true);
                return mpn.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last();
            }
        }

        [ProductProperty]
        public virtual string Durability
        {
            get
            {
                var cleaning = GetVendorProperty(ProductPropertyType.Cleaning, true);
                if (cleaning == null) return null;

                if (cleaning.Contains("Rub")) return cleaning.Replace("Rub", "");
                return null;
            }
        }

        [ProductProperty]
        public virtual string ColorName
        {
            get
            {
                var color = GetVendorProperty(ProductPropertyType.Color, true);
                if (color == null) return null;
                return color.Replace("-", " ").TitleCase();
            }
        }

        [ProductProperty]
        public virtual string Content
        {
            get
            {
                var content = GetVendorProperty(ProductPropertyType.Content, true);
                if (content == null) return null;
                return content.ToFormattedFabricContent();
            }
        }

        [ProductProperty]
        public virtual string PatternName
        {
            get
            {
                var pattern = GetVendorProperty(ProductPropertyType.PatternName, true);
                if (pattern == null) return null;
                return pattern.Replace(" - ", " ").Replace("&eacute", "e").TitleCase();
            }
        }

        [ProductProperty]
        public virtual string Category
        {
            get
            {
                var type = GetVendorProperty(ProductPropertyType.ProductType, true);
                if (type == null) return null;
                return type.UnEscape().TitleCase();
            }
        }

        [ProductProperty]
        public virtual string ProductType
        {
            get
            {
                var type = GetVendorProperty(ProductPropertyType.Category, true);
                if (type == null) return null;
                return type.UnEscape().TitleCase();
            }
        }

        private string GetInchesValue(string text)
        {
            int start = text.IndexOf("(") + 1;
            int end = text.IndexOf(")", start);
            return text.Substring(start, end - start);
        }

        [ProductProperty]
        public virtual string VerticalRepeat
        {
            get
            {
                var repeat = GetVendorProperty(ProductPropertyType.Repeat, true);
                if (repeat == null) return null;
                return GetInchesValue(repeat).ToInchesMeasurement();
            }
        }

        [ProductProperty]
        public virtual string Width
        {
            get
            {
                var width = GetVendorProperty(ProductPropertyType.Width, true);
                if (width == null) return null;
                return GetInchesValue(width).ToInchesMeasurement();
            }
        }

        [ProductProperty]
        public virtual string Weight
        {
            get
            {
                var weight = GetVendorProperty(ProductPropertyType.Weight, true);
                if (weight == null) return null;
                return weight.Replace("gms", "grams");
            }
        }

        public override string PatternCorrelator
        {
            get { return string.Format("{0}-{1}", AssociatedVendor.SkuPrefix, PatternName).ToUpper(); }
        }

        public override string ManufacturerDescription
        {
            get
            {
                return GetVendorProperty(ProductPropertyType.Description, true);
            }
        }

        [ProductProperty]
        public virtual string AdditionalInfo
        {
            get
            {
                var cleaning = GetVendorProperty(ProductPropertyType.Cleaning, true);
                if (cleaning == null) return null;

                if (cleaning.Contains("Scalloped")) return cleaning;
                return null;
            }
        }

        [ProductProperty]
        public virtual string ColorGroup
        {
            get
            {
                var color = GetVendorProperty(ProductPropertyType.ColorGroup, true);
                if (color == null) return null;

                if (color.Contains("(Light)"))
                {
                    color = "Light " + color;
                }
                if (color.Contains("(Mid)"))
                {
                    color = "Mid " + color;
                }
                if (color.Contains("(Dark)"))
                {
                    color = "Dark " + color;
                }
                color = color.Replace(" (Light)", "");
                color = color.Replace(" (Mid)", "");
                color = color.Replace(" (Dark)", "");
                if (color == "Ivories") return "Ivory";

                if (color.EndsWith("s"))
                    color = color.Remove(color.Length - 1);
                return color;
            }
        }

        [ProductProperty]
        public virtual string ItemNumber
        {
            get
            {
                return ManufacturerPartNumber;
            }
        }
    }
    */
}

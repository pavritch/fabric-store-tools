using System;
using System.Collections.Generic;
using System.Linq;
using FabricUpdater.Core;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.ProductProperties;
using FabricUpdater.Core.Scanning.Products;
using Utilities;

namespace FabricUpdater.Vendors.Seabrook
{
    public class SeabrookProduct : ProductBase, IVendorProduct
    {
        public SeabrookProduct(ProductConfigValues configValues) : base(configValues) { }

        public override bool IsExcludedProduct
        {
            get
            {
                if (Collection == null) return false;

                // need to exclude anything in the Carl Robinson collections
                return Collection.StartsWith("Carl Robinson");
            }
        }

        public override bool SetAndValidateKey()
        {
            if (FormatKeyProperties())
            {
                var mpn = VendorProperties[ProductPropertyType.ManufacturerPartNumber];

                // ensure have good MPN in case file has extra or blank rows

                if (string.IsNullOrWhiteSpace(mpn))
                    return false;

                // vendor properties have not been run through formatters yet, so must ensure
                // upper case here - noticed some lower in the file.

                Key = mpn.ToUpper();
                return true;
            }
            return false;
        }

        public override bool FormatKeyProperties()
        {
            return true;
        }

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

                add(ItemNumber);
                add(Collection);

                add("by");
                add("Seabrook");

                var value = string.Join(" ", nameParts);

                return value;
            }

        }

        public override string SKU
        {
            get
            {
                var value = string.Format("{0}-{1}", SkuPrefix, ManufacturerPartNumber);
                if (value.IsValidSKU())
                    return value;

                throw new Exception(string.Format("Invalid SKU: {0}", value));
            }
        }

        public override int StockCount
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.StockCount, Optional: true);
                if (string.IsNullOrWhiteSpace(value))
                    return 0;

                double count = Convert.ToDouble(value);
                return count > 0 ? 999999 : 0;
            }
        }

        private static readonly string[] noSwatchesPhrases = new string[] { "NO SAMPLE AVAIL", "SAMPLE NOT AVAILABLE", "NO SAMP AVAIL", "NO SAMPLE AVAILABLE" };

        public override bool IsSwatchAvailable
        {
            get
            {
                if (UnitOfMeasure == "Each") // murals
                    return false;

                var value = GetVendorProperty(ProductPropertyType.Note, Optional: true);

                if (value == null)
                    return base.IsSwatchAvailable;

                foreach (var phrase in noSwatchesPhrases)
                {
                    if (value.ContainsIgnoreCase(phrase))
                        return false;
                }

                return base.IsSwatchAvailable;
            }
        }


        [ProductProperty]
        public virtual string Backing
        {
            get
            {
                return GetVendorProperty(ProductPropertyType.Backing, Optional: true);
            }
        }


        [ProductProperty]
        public virtual string Dimensions
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.Dimensions, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                return value.Replace("in.Width", "in. by Width");
            }
        }


        [ProductProperty]
        public virtual string BorderHeight
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.BorderHeight, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                value = value.ToUpper().Replace("IN.", " ").Trim();
                return value.ToInchesMeasurement();
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

        [ProductProperty]
        public string Brand
        {
            get
            {
                var brand = GetVendorProperty(ProductPropertyType.Brand, true);
                return brand != null ? brand.TitleCase() : null;
            }
        }

        private static readonly Dictionary<string, string> notePhrases = new Dictionary<string, string>()
        {
            { "DECALS -- PEEL & STICK", "Peel & Stick Decals" },
            { "PATTERN IS TEXTURED", "Pattern is textured" },
            { "PKGD IN 15 YD BOLTS", "Bolt length 15 yards" },
            { "REVERSE HANG", "Reverse Hang" },
            { "PRICED/SOLD BY 5 YD SPOOL", "5 yard spool" }
        };

        [ProductProperty]
        public string Note
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.Note, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                foreach(var item in notePhrases)
                {
                    if (value.ContainsIgnoreCase(item.Key))
                        return item.Value;
                }

                return null;
            }
        }

        [ProductProperty]
        public virtual string AdditionalInfo
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.AdditionalInfo, true);

                if (string.IsNullOrWhiteSpace(value) || value.ContainsIgnoreCase("0 sq. ft"))
                    return null;

                return value;
            }
        }


        [ProductProperty]
        public string Color
        {
            get
            {
                Func<string, string> cleanColor = (s) =>
                    {
                        if (s.EndsWith("s"))
                            s = s.Remove(s.Length-1);

                        return s;
                    };

                var list = new List<string>();

                foreach (var item in new ProductPropertyType[] { ProductPropertyType.Color, ProductPropertyType.Color1, ProductPropertyType.Color2, ProductPropertyType.BackgroundColor })
                {
                    var color = GetVendorProperty(item, true);
                    if (color == null) continue;

                    color = cleanColor(color);
                    if (!list.Contains(color))
                        list.Add(color);
                }

                if (list.Count == 0)
                    return null;

                return list.ToCommaDelimitedList();
            }
        }


        public override string PatternCorrelator
        {
            get { return GetVendorProperty(ProductPropertyType.PatternCorrelator, true); }
        }

        [ProductProperty]
        public virtual string Finish
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.Finishes, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                return value;
            }
        }

        [ProductProperty]
        public virtual string Designer
        {
            get
            {
                return GetVendorProperty(ProductPropertyType.Designer, Optional: true);
            }
        }


        [ProductProperty]
        public string Collection
        {
            get
            {
                var collection = GetVendorProperty(ProductPropertyType.Collection, true);
                if (collection == null) return null;

                if (Char.IsDigit(collection.First()))
                {
                    collection = collection.Substring(4);
                }

                return collection.TitleCase().RomanNumeralCase().Replace("-Export-Platinum", " ").Replace("-Export", string.Empty).Trim();
            }
        }

        [ProductProperty]
        public string Length
        {
            get
            {
                var length = GetVendorProperty(ProductPropertyType.Length, true);
                return length != "0 ft." ? length : null;
            }
        }

        [ProductProperty]
        public string Material
        {
            get
            {
                var material = GetVendorProperty(ProductPropertyType.Material, true);
                return material == null ? null : material.TitleCase();
            }
        }

        [ProductProperty]
        public string Prepasted
        {
            get
            {
                var prepasted = GetVendorProperty(ProductPropertyType.Prepasted, true);
                return prepasted == null ? null : prepasted.TitleCase();
            }
        }

        [ProductProperty]
        public string ProductType
        {
            get
            {
                var productType = GetVendorProperty(ProductPropertyType.ProductType, true);
                return productType == null ? null : productType.TitleCase();
            }
        }

        [ProductProperty]
        public string Match
        {
            get
            {
                var match = GetVendorProperty(ProductPropertyType.Match, true);
                return match == null ? null : match.TitleCase();
            }
        }

        [ProductProperty]
        public string Style
        {
            get
            {
                var category = GetVendorProperty(ProductPropertyType.Style, true);
                if (category == null) return null;
                 
                category = category.Replace("/", ", ");
                category = category.Replace("Moir�", "Moira");
                return category.TitleCase();
            }
        }



        [ProductProperty]
        public virtual string Width
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.Width, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                value = value.ToUpper().Replace("IN.", " ").Trim();
                return value.ToInchesMeasurement();
            }
        }

        [ProductProperty]
        public virtual string VerticalRepeat
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.VerticalRepeat, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                value = value.ToUpper().Replace("IN.", " ").Trim();
                return value.ToInchesMeasurement();
            }
        }

        [ProductProperty]
        public virtual string HorizontalRepeat
        {
            get
            {
                var value = GetVendorProperty(ProductPropertyType.HorizontalRepeat, Optional: true);

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                value = value.ToUpper().Replace("IN.", " ").Trim();
                return value.ToInchesMeasurement();
            }
        }

        [ProductProperty]
        public string WebItemNumber
        {
            get
            {
                return GetVendorProperty(ProductPropertyType.WebItemNumber);
            }
        }
    }
}

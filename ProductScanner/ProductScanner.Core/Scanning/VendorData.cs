using System;
using System.Collections.Generic;
using FabricUpdater.Core.Scanning.ProductProperties;

namespace FabricUpdater.Core.Scanning
{
    public enum ImageVariantType
    {
        Primary,
        Alternate,
        Scene,
        Rectangular,
        Square,
        Round,
        Oval,
        Octagon,
        Runner,
        Star,
        Hearth,
        Kidney
    }

    public class ProductImage
    {
        public bool IsDefault { get; set; }
        public ImageVariantType ImageVariantType { get; set; }
        public string SourceUrl { get; set; }

        public ProductImage(ImageVariantType imageVariantType, string sourceUrl)
        {
            ImageVariantType = imageVariantType;
            if (sourceUrl != null)
                SourceUrl = sourceUrl.Replace(" ", "%20");
        }
    }

    public class VariantInfo
    {
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public string MPN { get; set; }
        public string Description { get; set; }
    }

    public class VendorData : Dictionary<ProductPropertyType, string>
    {
        public VendorData()
        {
            RelatedProducts = new List<string>();
            ProductImages = new List<ProductImage>();
        }
        public VendorData(VendorData properties) : this()
        {
            if (properties == null) return;
            foreach (var kvp in properties)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public new string this[ProductPropertyType prop]
        {
            get
            {
                if (!ContainsKey(prop)) return string.Empty;
                if (base[prop] == null) return string.Empty;
                return base[prop];
            }
            set
            {
                if (value == null)
                {
                    base[prop] = null;
                    return;
                }

                var str = value.Trim();
                base[prop] = str;
            }
        }

        // other colorways
        public List<string> RelatedProducts { get; set; }
        public List<ProductImage> ProductImages { get; set; }

        public bool HasCorrelator()
        {
            return !string.IsNullOrWhiteSpace(this[ProductPropertyType.PatternCorrelator]);
        }

        public bool HasWholesalePrice()
        {
            if (!ContainsKey(ProductPropertyType.WholesalePrice)) return false;
            if (this[ProductPropertyType.WholesalePrice] == "0") return false;

            return true;
        }
    }

    public enum ProductGroup
    {
        None,
        Fabric,
        Wallpaper,
        Trim,
        Hardware,
        Rug
    }

    public enum UnitOfMeasure
    {
        None,
        Each,
        Pair,
        Bag10,
        Yard,
        Roll,
        SquareFoot,
        SquareMeter,
        Meter,
        Panel,
        Swatch
    }
}
using System.Collections.Generic;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products
{
    public class StoreProduct
    {
        public StoreProduct()
        {
            ProductVariants = new List<StoreProductVariant>();
            PublicProperties = new Dictionary<string, string>();
            PrivateProperties = new Dictionary<string, string>();
            ProductImages = new List<ProductImage>();
            AvailableImages = new List<string>();
            Name = string.Empty;
        }

        private string _seTitle;

        public int? ProductID { get; set; }
        public int VendorID { get; set; }
        public string Description { get; set; }
        public string SEName { get; set; }
        public bool IsPublished { get; set; }
        
        public string SameAsSKU { get; set; }
        public string Correlator { get; set; }
        public string ManufacturerDescription { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public bool IsDiscontinued { get; set; }
        public string ImageFilename { get; set; }
        public bool IsClearance { get; set; }
        public string SEDescription { get; set; }
        public string SEKeywords { get; set; }

        public string SETitle
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_seTitle)) return _seTitle;
                return Name;
            }
            set { _seTitle = value; }
        }

        public ProductClass? ProductClass { get; set; }
        public ProductGroup ProductGroup { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }


        public List<ProductImage> ProductImages { get; set; }
        public List<string> AvailableImages { get; set; }

        public Dictionary<string, string> PublicProperties { get; set; }

        // Should we use enum as key?
        public Dictionary<string, string> PrivateProperties { get; set; }

        // for attaching a typesafe data structure of features/attributes to this class
        public RugProductFeatures RugFeatures { get; set; }
        public HomewareProductFeatures HomewareFeatures { get; set; }

        public List<StoreProductVariant> ProductVariants { get; set; }

        public bool HasBeenManuallyEdited()
        {
            var prop = GetPrivateProperty(ProductPropertyType.HasBeenEdited);
            return prop == "true";
        }

        public string GetPrivateProperty(ProductPropertyType property)
        {
            if (!PrivateProperties.ContainsKey(property.DescriptionAttr())) return string.Empty;
            return PrivateProperties[property.DescriptionAttr()];
        }

        public string GetDetailUrl()
        {
            var key = LibEnum.GetDescription(ProductPropertyType.ProductDetailUrl);
            if (PrivateProperties.ContainsKey(key))
                return PrivateProperties[key];
            return string.Empty;
        }
    }
}
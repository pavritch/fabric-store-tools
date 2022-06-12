using System;
using System.Collections.Generic;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Products
{
    // DTO used to pull variants from database
    public class StoreProductVariant
    {
        // used to deserialize from JSON
        public StoreProductVariant()
        {
            PrivateProperties = new Dictionary<string, string>();
        }

        public int? VariantID { get; set; }
        public int? ProductID { get; set; }
        public bool IsSwatch { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }
        public string SKUSuffix { get; set; }
        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal OurPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public bool InStock { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public int MinimumQuantity { get; set; }
        public int OrderIncrementQuantity { get; set; }
        public bool IsFreeShipping { get; set; }
        public ProductShapeType Shape { get; set; }
        public string OrderRequirementsNotice { get; set; }
        public bool IsPublished { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }

        // store product associated with this variant
        // every variant should be associated to a product
        public StoreProduct StoreProduct { get; set; }
        
        public Dictionary<string, string> PublicProperties { get; set; }

        public Dictionary<string, string> PrivateProperties { get; set; }

        // for attaching a typesafe data structure of features/attributes to this class
        public RugProductVariantFeatures RugFeatures { get; set; }

        /// <summary>
        /// Typesafe accessor for features.
        /// </summary>
        //public RugProductVariantFeatures RugFeatures
        //{
            //get
            //{
                //return Features as RugProductVariantFeatures;
            //}
        //}


        public string GetUniqueKey()
        {
            // if it's a swatch, return MPN + "-Swatch"
            // otherwise just the MPN
            if (IsSwatch) return ManufacturerPartNumber + "-Swatch";
            return ManufacturerPartNumber;
        }
    }
}
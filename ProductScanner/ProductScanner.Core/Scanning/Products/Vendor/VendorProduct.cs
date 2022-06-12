using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using Newtonsoft.Json;
using ProductScanner.Core.Scanning.ProductProperties;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    // this class represents the final data that we've scanned from the vendor
    // we can use as much detail and strongly typed data here as makes sense, as it will
    // be transformed into StoreProduct in the core code
    public abstract class VendorProduct
    {
        private List<VendorVariant> _variants;
        public Dictionary<ProductPropertyType, string> PublicProperties { get; set; }

        // Should we use enum as key?
        public Dictionary<ProductPropertyType, string> PrivateProperties { get; set; }

        protected VendorProduct()
        {
            _variants = new List<VendorVariant>();
            PublicProperties = new Dictionary<ProductPropertyType, string>();
            PrivateProperties = new Dictionary<ProductPropertyType, string>();
            ScannedImages = new List<ScannedImage>();
            Name = string.Empty;
        }

        public void AddPublicProp(ProductPropertyType property, string value)
        {
            if (PublicProperties.ContainsKey(property)) PublicProperties[property] = value;
            else PublicProperties.Add(property, value);
        }

        public string GetPublicProperty(ProductPropertyType property)
        {
            if (!PublicProperties.ContainsKey(property)) return string.Empty;
            return PublicProperties[property];
        }

        public string GetPrivateProperty(ProductPropertyType property)
        {
            if (!PrivateProperties.ContainsKey(property)) return string.Empty;
            return PrivateProperties[property];
        }

        public void RemoveWhen(ProductPropertyType property, Func<string, bool> filter)
        {
            if (!PublicProperties.ContainsKey(property) || PublicProperties[property] == null) return;

            if (filter(PublicProperties[property])) PublicProperties.Remove(property);
        }

        public virtual Dictionary<string, string> GetPublicProperties()
        {
            return PublicProperties.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Where(x => !x.Key.HasAttribute<ExcludedProductPropertyAttribute>())
                .ToDictionary(k => k.Key.DescriptionAttr(), v => v.Value);
        }

        public VendorVariant GetPrimaryVariant()
        {
            return GetPopulatedVariants().First();
        }

        public virtual List<VendorVariant> GetPopulatedVariants()
        {
            // use the product properties to fill in what we need for the variant
            if (!_variants.Any(x => x.IsDefault) && _variants.Any()) _variants.First().IsDefault = true;
            return _variants;
        }


        public List<ExcludedReason> GetExcludedReasons()
        {
            return _variants.SelectMany(x => x.GetExcludedReasons()).ToList();
        }

        public string GetPrimaryImageUrl()
        {
            var primaryImage = ScannedImages.FirstOrDefault(x => x.ImageVariantType == ImageVariantType.Primary);
            return primaryImage != null ? primaryImage.Url : null;
        }

        public void AddVariants(List<VendorVariant> variants)
        {
            _variants.AddRange(variants);
        }

        public void RemoveVariants(List<VendorVariant> variants)
        {
            variants.ForEach(x => _variants.Remove(x));
        }

        public abstract List<ProductImage> GetProductImages(string skuOverride);
        public abstract void SetProductGroup(ProductGroup productGroup);

        private string _seTitle;
        private string _seName;

        public string SameAsSKU { get; set; }
        public string Correlator { get; set; }
        public string ManufacturerDescription { get; set; }
        public string OrderRequirementsNotice { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                var s = value.Replace("&amp;", "&");
                _name = s;
            }
        }

        public bool IsDiscontinued { get; set; }
        public bool IsClearance { get; set; }
        public bool IsSkipped { get; set; }

        public int MinimumQuantity { get; set; }
        public int OrderIncrement { get; set; }

        // when this is set, we'll use this value instead of the SEName
        public string ImageFilename { get; set; }

        public string SEName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_seName)) return _seName;
                return Name.MakeSafeSEName();
            }
            set { _seName = value; }
        }

        public string SETitle
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_seTitle)) return _seTitle;
                return Name;
            }
            set { _seTitle = value; }
        }

        public Core.Vendor Vendor { get; set; }
        public ProductClass ProductClass { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }

        public ProductGroup ProductGroup { get; protected set; }

        [JsonProperty]
        public List<ScannedImage> ScannedImages { get; set; }

        // we need to keep the same IDs that we used before for any existing images (matched by URLs)
        public virtual void SetImageIds(List<ProductImage> existingImages)
        {
            foreach (var image in existingImages)
            {
                var match = ScannedImages.SingleOrDefault(x => x.Url == image.SourceUrl);
                if (match != null)
                    match.Id = Guid.Parse(image.Filename.Replace(".jpg", ""));
            }
        }

        public virtual bool HasOnlySample()
        {
            return false;
        }

        public virtual StoreProduct MakeNewStoreProduct(int skuOverride)
        {
            var storeProduct = new StoreProduct
            {
                AvailableImages = new List<string>(),
                Correlator = Correlator,
                ImageFilename = null,
                IsClearance = IsClearance,
                IsDiscontinued = false,
                IsPublished = true,
                ManufacturerDescription = ManufacturerDescription,
                Name = Name,
                ProductClass = ProductClass,
                ProductGroup = ProductGroup,
                ProductImages = GetProductImages(skuOverride.ToString()),
                ProductVariants = GetPopulatedVariants().Select(x => x.BuildNewStoreVariant()).ToList(),
                PublicProperties = GetPublicProperties(),
                SEName = SEName,
                SETitle = SETitle,
                SameAsSKU = SameAsSKU,
                UnitOfMeasure = UnitOfMeasure,
                VendorID = Vendor.Id,
            };

            var privateProperties = PrivateProperties.ToDictionary(k => k.Key.DescriptionAttr(), v => v.Value);
            privateProperties[ProductPropertyType.LastFullUpdate.DescriptionAttr()] = DateTime.Now.ToShortDateString();
            storeProduct.PrivateProperties = privateProperties;
            return storeProduct;
        }

        public virtual StoreProduct MakeUpdateStoreProduct(StoreProduct sqlProduct, bool skuChangeFlag, bool skipImages)
        {
            /*
            You can also now ponder and/or implement logic to not update most fields for records which have this flag set
            but inventory, discontinued, prices should be updated. And technically, images too.
            Imagefilenameoverride – doubtful we should allow this to be edited since we have a lot of automated processing for images. Don’t need the grief.
            I would figure this flag would be evaluated only on “full update” operations.

            Kinds of properties which get locked down: Names, titles, descriptions, bullets, features, seNames, category.
            */

            // not sure yet if this can be done here, or needs to be done in StoreDatabase

            var storeProduct = new StoreProduct
            {
                AvailableImages = sqlProduct.AvailableImages,
                Correlator = Correlator,
                Description = sqlProduct.Description,
                ImageFilename = sqlProduct.ImageFilename,
                IsClearance = IsClearance,
                IsPublished = true,
                IsDiscontinued = false,
                ManufacturerDescription = ManufacturerDescription,
                Name = Name,
                ProductClass = ProductClass,
                ProductGroup = ProductGroup,
                ProductImages = skipImages ? new List<ProductImage>() : GetProductImages(sqlProduct.SKU),
                PublicProperties = GetPublicProperties(),
                SEDescription = sqlProduct.SEDescription,
                SEKeywords = sqlProduct.SEKeywords,
                SEName = SEName,
                SETitle = SETitle,
                SameAsSKU = SameAsSKU,
                UnitOfMeasure = UnitOfMeasure,
                VendorID = Vendor.Id,

                // fields set on an update
                ProductID = sqlProduct.ProductID,
                ProductVariants = GetPopulatedVariants().Select(x => x.BuildUpdateStoreVariant(sqlProduct)).ToList(),
            };

            var privateProperties = storeProduct.PrivateProperties.ToDictionary(k => k.Key.DescriptionAttr(), v => v.Value);
            privateProperties[ProductPropertyType.LastFullUpdate.DescriptionAttr()] = DateTime.Now.ToShortDateString();
            if (PrivateProperties.ContainsKey(ProductPropertyType.IsLimitedAvailability))
                privateProperties[ProductPropertyType.IsLimitedAvailability.DescriptionAttr()] = PrivateProperties[ProductPropertyType.IsLimitedAvailability];
            storeProduct.PrivateProperties = privateProperties;
            return storeProduct;
        }

        public abstract List<VendorVariant> GetFilteredVariants();
    }
}
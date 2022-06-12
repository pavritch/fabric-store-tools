using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public class RugProduct : VendorProduct
    {
        // set default: For Default, the preference is for Rectangular….but in the absence of that, then Oval, Octagon, etc.
        private readonly List<ImageVariantType> _defaultPriority = new List<ImageVariantType>
        {
            ImageVariantType.Rectangular,
            ImageVariantType.Runner,
            ImageVariantType.Round,
            ImageVariantType.Oval,
            ImageVariantType.Octagon,
            ImageVariantType.Square,
            ImageVariantType.Star,
        };

        public RugProduct(Core.Vendor vendor)
        {
            SetProductGroup(ProductGroup.Rug);
            UnitOfMeasure = UnitOfMeasure.Each;

            MinimumQuantity = 1;
            OrderIncrement = 1;

            Vendor = vendor;
        }

        public RugProductFeatures RugProductFeatures { get; set; }

        public override List<ProductImage> GetProductImages(string skuOverride)
        {
            var productImages = new List<ProductImage>();
            foreach (var image in ScannedImages)
            {
                string filename = string.Format("{0}.jpg", image.Id);
                var productImage = new ProductImage(image.ImageVariantType, image.Url, filename);
                productImages.Add(productImage);
            }

            // set default: For Default, the preference is for Rectangular….but in the absence of that, then Oval, Octagon, etc.
            foreach (var priority in _defaultPriority)
            {
                var match = productImages.FirstOrDefault(x => x.ImageVariant == priority.ToString());
                if (match != null)
                {
                    match.IsDefault = true;
                    break;
                }
            }

            if (productImages.Any() && !productImages.Any(x => x.IsDefault))
            {
                productImages.First().IsDefault = true;
            }

            return productImages;
        }

        public override bool HasOnlySample()
        {
            return GetPopulatedVariants().All(x => x.GetShape() == ProductShapeType.Sample);
        }

        public override List<VendorVariant> GetFilteredVariants()
        {
            return GetPopulatedVariants();
        }

        public override StoreProduct MakeNewStoreProduct(int skuOverride)
        {
            var storeProduct = base.MakeNewStoreProduct(skuOverride);
            storeProduct.SKU = Vendor.SkuPrefix + "-" + skuOverride;
            storeProduct.RugFeatures = RugProductFeatures;
            return storeProduct;
        }

        public override StoreProduct MakeUpdateStoreProduct(StoreProduct sqlProduct, bool skuChangeFlag, bool skipImages)
        {
            SetImageIds(sqlProduct.ProductImages);

            var storeProduct = base.MakeUpdateStoreProduct(sqlProduct, skuChangeFlag, skipImages);
            storeProduct.SKU = sqlProduct.SKU;
            storeProduct.RugFeatures = RugProductFeatures;
            return storeProduct;
        }

        public override sealed void SetProductGroup(ProductGroup productGroup)
        {
            if (productGroup != ProductGroup.Rug) throw new Exception("Invalid product group");
            ProductGroup = productGroup;
        }
    }
}
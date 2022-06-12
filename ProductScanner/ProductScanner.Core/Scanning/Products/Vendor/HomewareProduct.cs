using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.ProductProperties;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public class HomewareProduct : VendorProduct
    {
        public StockData StockData { get; set; }
        public string MPN { get; set; }
        private decimal _cost;
        public ProductPriceData ProductPriceData { get; set; }
        public HomewareProductFeatures ProductFeatures { get; set; }

        private HomewareCategory _category;

        public HomewareCategory HomewareCategory
        {
            get { return _category; }
            set
            {
                _category = value;
                ProductFeatures.Category = value.CategoryAttr();
            }
        }

        public HomewareProduct(decimal cost, Core.Vendor vendor, StockData stockData, string mpn, HomewareProductFeatures features)
        {
            _cost = cost;
            Vendor = vendor;
            StockData = stockData;
            MPN = mpn;

            UnitOfMeasure = UnitOfMeasure.Each;
            SetProductGroup(ProductGroup.Homeware);

            ProductFeatures = features;

            OrderIncrement = 1;
            MinimumQuantity = 1;
        }

        public override List<VendorVariant> GetPopulatedVariants()
        {
            var populatedVariants = base.GetPopulatedVariants();
            if (populatedVariants.Any()) return new List<VendorVariant>(populatedVariants);

            var variants = new List<VendorVariant>();
            variants.Add(GetMainProduct());
            return variants;
        }

        public override List<VendorVariant> GetFilteredVariants()
        {
            var mainProduct = GetMainProduct();
            if (mainProduct.IsExcluded()) return new List<VendorVariant>();
            return GetPopulatedVariants();
        }

        private FabricVendorVariant GetMainProduct()
        {
            return new FabricVendorVariant(MPN, _cost, ProductPriceData, StockData, Vendor, this, true)
            {
                IsDefault = true,
                IsClearance = IsClearance,
                IsSkipped = IsSkipped,
                MinimumQuantity = MinimumQuantity,
                OrderIncrementQuantity = OrderIncrement
            };
        }

        public override StoreProduct MakeNewStoreProduct(int skuOverride)
        {
            var storeProduct = base.MakeNewStoreProduct(skuOverride);
            storeProduct.SKU = Vendor.SkuPrefix + "-" + skuOverride;
            storeProduct.HomewareFeatures = ProductFeatures;
            storeProduct.PrivateProperties[ProductPropertyType.StockCount.DescriptionAttr()] = StockData.ActualCount.ToString();
            if (!storeProduct.ProductImages.Any()) return null;
            return storeProduct;
        }

        public override StoreProduct MakeUpdateStoreProduct(StoreProduct sqlProduct, bool skuChangeFlag, bool skipImages)
        {
            var storeProduct = base.MakeUpdateStoreProduct(sqlProduct, skuChangeFlag, skipImages);
            storeProduct.SKU = sqlProduct.SKU;
            storeProduct.HomewareFeatures = ProductFeatures;
            return storeProduct;
        }

        public override List<ProductImage> GetProductImages(string skuOverride)
        {
            var productImages = new List<ProductImage>();
            foreach (var image in ScannedImages)
            {
                string filename;
                if (image.ImageVariantType == ImageVariantType.Primary)
                    filename = string.Format("{0}-{1}-{2}.jpg", Vendor.SkuPrefix, skuOverride, !string.IsNullOrWhiteSpace(ImageFilename) ? ImageFilename : SEName);
                else
                    filename = string.Format("{0}.jpg", image.Id);

                productImages.Add(new ProductImage(image.ImageVariantType, image.Url, filename)
                {
                    IsDefault = image.ImageVariantType == ImageVariantType.Primary
                });
            }

            if (productImages.Any() && !productImages.Any(x => x.IsDefault))
            {
                productImages.First().IsDefault = true;
            }
            return productImages;
        }

        public override sealed void SetProductGroup(ProductGroup productGroup)
        {
            if (productGroup != ProductGroup.Homeware) throw new Exception("Invalid product group");
            ProductGroup = productGroup;
        }
    }
}
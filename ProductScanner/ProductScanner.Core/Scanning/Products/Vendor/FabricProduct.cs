using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public enum Packaging
    {
        None,
        SingleRoll,
        DoubleRoll,
        TripeRoll
    }

    public sealed class FabricProduct : VendorProduct
    {
        public StockData StockData { get; set; }
        public string MPN { get; set; }
        public bool HasSwatch { get; set; }

        // TODO: Create a class to enforce invariants for SKUs?
        // use logic from SkuTweaks extension method
        private string _sku;

        public string SKU
        {
            get { return _sku; }
            set
            {
                if (!Regex.IsMatch(value, @"^[A-Z][A-Z]\-([A-Z]|[0-9]|[\-]){1,64}$"))
                    throw new Exception("Invalid SKU");
                _sku = value;
            }
        }

        [Description("Book")]
        public string Book { get; set; }

        public Packaging Packaging { get; set; }

        private decimal _cost;
        public ProductPriceData ProductPriceData { get; set; }

        public override Dictionary<string, string> GetPublicProperties()
        {
            var props = base.GetPublicProperties();
            // unit of measure is in both a field in the database AND in public properties...
            // but...not for rugs... - added an override in rug product
            props.Add(ProductPropertyType.UnitOfMeasure.DescriptionAttr(), UnitOfMeasure.ToString());
            //props.Add(ProductPropertyType.Book, "Book");
            return props;
        }

        public FabricProduct(string mpn, decimal cost, StockData stockData, Core.Vendor vendor)
        {
            StockData = stockData;
            MPN = mpn;
            Vendor = vendor;
            _cost = Math.Round(cost, 2);

            // default values - overriden at the vendor level
            OrderIncrement = 1;
            MinimumQuantity = 1;

            // by default
            HasSwatch = true;
        }

        private FabricVendorVariant GetMainProduct()
        {
            return new FabricVendorVariant(MPN, _cost, ProductPriceData, StockData, Vendor, this, isSwatch: false)
            {
                MinimumQuantity = MinimumQuantity,
                OrderIncrementQuantity = OrderIncrement,
                IsDefault = true,
                IsClearance = IsClearance,
                IsSkipped = IsSkipped
            };
        }

        public override List<VendorVariant> GetPopulatedVariants()
        {
            var mainProduct = GetMainProduct();

            var swatchVariant = new FabricVendorVariant(MPN, Vendor.SwatchCost, new ProductPriceData(Vendor.SwatchPrice, Vendor.SwatchPrice), StockData, Vendor, this, isSwatch: true)
            {
                SKUSuffix = "-Swatch",
                IsSkipped = IsSkipped,
            };
            // if the main product has no price, don't create the swatch
            // the main product will still be reported on, but will be filtered out before the commit
            if (!Vendor.SwatchesEnabled ||
                !HasSwatch ||
                UnitOfMeasure == UnitOfMeasure.Each ||
                _cost == 0)
            {
                return new List<VendorVariant> { mainProduct };
            }
            return new List<VendorVariant> { mainProduct, swatchVariant };
        }

        public override List<VendorVariant> GetFilteredVariants()
        {
            var mainProduct = GetMainProduct();
            if (mainProduct.IsExcluded()) return new List<VendorVariant>();
            return GetPopulatedVariants();
        }

        public override List<ProductImage> GetProductImages(string skuOverride)
        {
            var productImages = new List<ProductImage>();
            foreach (var image in ScannedImages)
            {
                string filename;
                if (image.ImageVariantType == ImageVariantType.Primary)
                {
                    filename = string.Format("{0}.jpg", !string.IsNullOrWhiteSpace(ImageFilename) ? ImageFilename : SEName);
                }
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
                productImages.First().ImageVariant = ImageVariantType.Primary.ToString();
            }
            return productImages;
        }

        public override StoreProduct MakeNewStoreProduct(int skuOverride)
        {
            var storeProduct = base.MakeNewStoreProduct(skuOverride);
            storeProduct.SKU = SKU;
            storeProduct.PrivateProperties[ProductPropertyType.StockCount.DescriptionAttr()] = StockData.ActualCount.ToString();

            if (ProductGroup != ProductGroup.Wallcovering) return storeProduct;
            return storeProduct;
        }

        public override StoreProduct MakeUpdateStoreProduct(StoreProduct sqlProduct, bool skuChangeFlag, bool skipImages)
        {
            var storeProduct = base.MakeUpdateStoreProduct(sqlProduct, skuChangeFlag, skipImages);
            storeProduct.SKU = skuChangeFlag ? SKU : sqlProduct.SKU;

            if (ProductGroup != ProductGroup.Wallcovering) return storeProduct;
            return storeProduct;
        }

        public override void SetProductGroup(ProductGroup productGroup)
        {
            if (productGroup != ProductGroup.Wallcovering && productGroup != ProductGroup.Fabric && productGroup != ProductGroup.Trim) 
                throw new Exception("Invalid product group");
            ProductGroup = productGroup;
        }

        /*public void StandardizeWallpaperIncrement(ProductGroup productGroup, int orderIncrement, RollDimensions dimensions)
        {
            if (productGroup != ProductGroup.Wallcovering || (orderIncrement != 2 && orderIncrement != 3)) return;
            if (dimensions == null) return;

            OrderIncrement = 1;
            MinimumQuantity = 1;
            ProductPriceData.OurPrice *= orderIncrement;
            ProductPriceData.RetailPrice *= orderIncrement;
            ProductPriceData.SalePrice *= orderIncrement;
            _cost *= orderIncrement;

            //PublicProperties[ProductPropertyType.Coverage] = dimensions.GetTotalCoverageFormatted(orderIncrement);
            //PublicProperties[ProductPropertyType.Dimensions] = dimensions.FormatTotal(orderIncrement);
            //PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthTotalInYardsFormatted(orderIncrement);
            //PublicProperties[ProductPropertyType.Packaging] = GetRollType(orderIncrement);
        }*/

        public void NormalizeWallpaperPricing(int orderIncrement)
        {
            ProductPriceData.OurPrice *= orderIncrement;
            ProductPriceData.RetailPrice *= orderIncrement;
            ProductPriceData.SalePrice *= orderIncrement;
            _cost *= orderIncrement;
        }
    }
}
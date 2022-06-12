using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public class HomewareVendorVariant : VendorVariant
    {
        public string Name { get; set; }

        public HomewareVendorVariant(ProductPriceData priceData, string skuSuffix, Core.Vendor vendor)
        {
            RetailPrice = priceData.RetailPrice;
            OurPrice = priceData.OurPrice;
            SalePrice = priceData.SalePrice;

            SKUSuffix = skuSuffix;

            MinimumCost = Math.Round(vendor.MinimumCost, 2);

            OrderIncrementQuantity = 1;
            MinimumQuantity = 1;
        }

        public override string GetDescription() { return Name; }
        public override ProductShapeType GetShape() { return ProductShapeType.None; }

        public override List<ExcludedReason> GetExcludedReasons()
        {
            var baseReasons = base.GetExcludedReasons();
            if (MissingImage()) baseReasons.Add(ExcludedReason.MissingImage);
            return baseReasons;
        }

        private bool MissingImage()
        {
            return !VendorProduct.ScannedImages.Any();
        }
    }

    public class RugVendorVariant : VendorVariant
    {
        public RugVendorVariant(string mpn, string skuSuffix, decimal cost, ProductPriceData priceData, bool inStock, Core.Vendor vendor, VendorProduct vendorProduct, int orderIndex, RugProductVariantFeatures features)
        {
            if (string.IsNullOrWhiteSpace(skuSuffix) || !skuSuffix.StartsWith("-")) throw new Exception("Invalid Sku Suffix");
            if (features == null) throw new ArgumentException();

            RugProductVariantFeatures = features;
            VendorProduct = vendorProduct;
            ManufacturerPartNumber = mpn;
            SKUSuffix = skuSuffix;
            StockData = new StockData(inStock);

            Cost = Math.Round(cost, 2);
            RetailPrice = priceData.RetailPrice;
            OurPrice = priceData.OurPrice;
            SalePrice = priceData.SalePrice;

            OrderIndex = orderIndex;

            MinimumCost = Math.Round(vendor.MinimumCost, 2);
            OrderIncrementQuantity = 1;
            MinimumQuantity = 1;
        }

        public RugProductVariantFeatures RugProductVariantFeatures { get; private set; }
        public int OrderIndex { get; set; }

        public override int GetDisplayOrder()
        {
            return OrderIndex + 1;
        }

        public override List<ExcludedReason> GetExcludedReasons()
        {
            var baseReasons = base.GetExcludedReasons();
            if (MissingImage()) baseReasons.Add(ExcludedReason.MissingImage);
            return baseReasons;
        }

        public override bool IsExcluded()
        {
            if (MissingImage()) return true;
            return base.IsExcluded();
        }

        private bool MissingImage()
        {
            return !VendorProduct.ScannedImages.Any();
        }

        public override string GetDescription()
        {
            return RugProductVariantFeatures.Description;
        }

        public override ProductShapeType GetShape()
        {
            return (ProductShapeType)Enum.Parse(typeof(ProductShapeType), RugProductVariantFeatures.Shape);
        }
    }

    public class PricingConstants
    {
        public static decimal MinimumOurPrice = 10;
        public static decimal MinimumMarginDollars = 5;
    }

    // Fabric + Wallpaper
    public class FabricVendorVariant : VendorVariant
    {
        // this constructor just used for fabric/wallpaper?
        public FabricVendorVariant(string mpn, decimal cost, ProductPriceData priceData, StockData stockData, Core.Vendor vendor, VendorProduct vendorProduct, bool isSwatch)
        {
            VendorProduct = vendorProduct;
            Cost = Math.Round(cost, 2);
            RetailPrice = priceData.RetailPrice;
            OurPrice = priceData.OurPrice;

            if (!isSwatch)
            {
                OurPrice = Math.Max(priceData.OurPrice, PricingConstants.MinimumOurPrice);

                var margin = OurPrice - cost;
                if (margin < 5)
                    OurPrice = cost + 5;
            }

            SalePrice = priceData.SalePrice;
            StockData = stockData;
            ManufacturerPartNumber = mpn;
            MinimumQuantity = 1;
            OrderIncrementQuantity = 1;
            SKUSuffix = string.Empty;

            MinimumCost = vendor.MinimumCost;
            MinimumPrice = Math.Round(vendor.MinimumPrice, 2);
        }

        public override string GetDescription() { return null; }
        public override ProductShapeType GetShape() { return ProductShapeType.None; }
    }


    public abstract class VendorVariant
    {
        private decimal _ourPrice;

        public VendorProduct VendorProduct { get; set; }
        public decimal MinimumCost { get; set; }
        public decimal MinimumPrice { get; set; }

        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? SalePrice { get; set; }

        public decimal OurPrice
        {
            get { return _ourPrice; }
            set
            {
                // make sure the value is always rounded to the nearest cent
                _ourPrice = Math.Round(value, 2);
            }
        }

        public StockData StockData { get; set; }
        public bool IsClearance { get; set; }
        // don't put this product in any batches - don't make any changes to it because we don't have reliable info
        public bool IsSkipped { get; set; }
        public bool IsDefault { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string UPC { get; set; }
        public string SKUSuffix { get; set; }
        public int MinimumQuantity { get; set; }
        public int OrderIncrementQuantity { get; set; }
        public string Area { get; set; }
        public string AlternateItemNumber { get; set; }

        public virtual int GetDisplayOrder()
        {
            return IsSwatch() ? 2 : 1;
        }

        public abstract string GetDescription();
        public abstract ProductShapeType GetShape();

        public string GetUniqueKey()
        {
            // if it's a swatch, return MPN + "-Swatch"
            // otherwise just the MPN
            if (IsSwatch()) return ManufacturerPartNumber + "-Swatch";
            return ManufacturerPartNumber;
        }

        public bool IsSwatch()
        {
            if (SKUSuffix == null) return false;
            return SKUSuffix.Contains("Swatch");
        }

        public bool IsFreeShipping()
        {
            return IsSwatch();
        }

        public virtual bool IsExcluded()
        {
            // we still want to process skipped products - we don't want to treat them as discontinued
            if (IsSkipped) return false;

            if (HasLowPrice()) 
                return true;
            if (HasLowCost()) 
                return true;
            if (HasRetailLowerThanOurPrice()) 
                return true;
            if (HasOurPriceLowerThanCost()) 
                return true;
            if (!HasCost()) 
                return true;
            if (!HasOurPrice()) 
                return true;
            if (!HasUnitOfMeasure()) 
                return true;
            if (!HasProductGroup()) 
                return true;
            if (HasHighMinQuantity())
                return true;
            return false;
        }

        private bool HasHighMinQuantity()
        {
            return MinimumQuantity > 4;
        }

        public virtual List<ExcludedReason> GetExcludedReasons()
        {
            var reasons = new List<ExcludedReason>();
            if (HasLowCost())
                reasons.Add(ExcludedReason.HasLowCost);
            if (HasLowPrice())
                reasons.Add(ExcludedReason.HasLowPrice);
            if (HasRetailLowerThanOurPrice()) reasons.Add(ExcludedReason.HasRetailLowerThanOurPrice);
            if (HasOurPriceLowerThanCost()) reasons.Add(ExcludedReason.HasOurPriceLowerThanCost);
            if (!HasCost()) reasons.Add(ExcludedReason.MissingCost);
            if (!HasOurPrice())
                reasons.Add(ExcludedReason.MissingOurPrice);
            if (!HasUnitOfMeasure()) reasons.Add(ExcludedReason.MissingUnitOfMeasure);
            if (!HasProductGroup())
                reasons.Add(ExcludedReason.MissingProductGroup);
            if (string.IsNullOrWhiteSpace(ManufacturerPartNumber)) reasons.Add(ExcludedReason.MissingMPN);
            if (OrderIncrementQuantity <= 0) reasons.Add(ExcludedReason.OrderIncrementZero);
            if (MinimumQuantity <= 0) reasons.Add(ExcludedReason.MinimumQuantityZero);
            if (VendorProduct.ProductGroup == ProductGroup.Rug)
            {
                if (GetShape() == ProductShapeType.None) reasons.Add(ExcludedReason.RugShapeMissing);
                if (VendorProduct.UnitOfMeasure != UnitOfMeasure.Each) reasons.Add(ExcludedReason.InvalidUnitOfMeasure);
            }

            return reasons;
        }

        private bool HasOurPrice()
        {
            // if it's a swatch (only fabric/wallpaper) then look at the non-swatch cost for this check
            // reason is that we want to filter out swatches and main products if the main product does not have a price
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasOurPrice();
            return OurPrice > 0;
        }

        public bool HasCost()
        {
            // if it's a swatch (only fabric/wallpaper) then look at the non-swatch cost for this check
            // reason is that we want to filter out swatches and main products if the main product does not have a price
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasCost();
            return Cost > 0;
        }

        private bool HasRetailLowerThanOurPrice()
        {
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasRetailLowerThanOurPrice();
            return HasCost() && RetailPrice <= OurPrice;
        }

        private bool HasOurPriceLowerThanCost()
        {
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasOurPriceLowerThanCost();
            return HasCost() && OurPrice <= Cost;
        }

        private bool HasLowCost()
        {
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasLowCost();
            return HasCost() && Cost <= MinimumCost;
        }

        private bool HasLowPrice()
        {
            if (IsSwatch()) return VendorProduct.GetPrimaryVariant().HasLowPrice();
            return OurPrice <= MinimumPrice;
        }

        private bool HasProductGroup()
        {
            return VendorProduct.ProductGroup != ProductGroup.None;
        }

        private bool HasUnitOfMeasure()
        {
            return VendorProduct.UnitOfMeasure != UnitOfMeasure.None;
        }

        private int GetMinimumQuantity()
        {
            if (IsSwatch()) return 1;

            if (OurPrice < 50 && (VendorProduct.ProductGroup == ProductGroup.Fabric || VendorProduct.ProductGroup == ProductGroup.Trim)
                && VendorProduct.UnitOfMeasure == UnitOfMeasure.Yard)
            {
                return 2;
            }

            //if (VendorProduct.ProductGroup == ProductGroup.Wallcovering && VendorProduct.UnitOfMeasure == UnitOfMeasure.Roll && MinimumQuantity < 2)
                //return 2;

            // in the case when MinQty is higher, need to make sure it's bumped to next allowable qty (based on OrderInc)
            return ExtensionMethods.RoundUp(MinimumQuantity, OrderIncrementQuantity);
        }

        public StoreProductVariant BuildNewStoreVariant()
        {
            var variant = new StoreProductVariant
            {
                Cost = Cost,
                Description = GetDescription(),
                DisplayOrder = GetDisplayOrder(),
                InStock = StockData.InStock,
                IsDefault = IsDefault,
                IsFreeShipping = IsFreeShipping(),
                IsPublished = true,
                IsSwatch = IsSwatch(),
                ManufacturerPartNumber = ManufacturerPartNumber,
                MinimumQuantity = GetMinimumQuantity(),
                OrderIncrementQuantity = OrderIncrementQuantity,
                OrderRequirementsNotice = VendorProduct.OrderRequirementsNotice,
                OurPrice = OurPrice,
                PublicProperties = new Dictionary<string, string>(),
                RetailPrice = RetailPrice,
                SalePrice = SalePrice,
                Shape = GetShape(),
                SKUSuffix = SKUSuffix,
                UnitOfMeasure = IsSwatch() ? UnitOfMeasure.Swatch : VendorProduct.UnitOfMeasure
            };

            if (this is RugVendorVariant)
                variant.RugFeatures = (this as RugVendorVariant).RugProductVariantFeatures;
            return variant;
        }

        public StoreProductVariant BuildUpdateStoreVariant(StoreProduct sqlProduct)
        {
            var matchingVariant = FindMatchingSqlVariant(sqlProduct.ProductVariants, this);
            // we didn't find the variant, so it's actually a new variant for this product
            if (matchingVariant == null)
                return BuildNewStoreVariant();

            var variant = new StoreProductVariant
            {
                Cost = Cost,
                Description = GetDescription(),
                DisplayOrder = GetDisplayOrder(),
                InStock = StockData.InStock,
                IsDefault = IsDefault,
                IsFreeShipping = IsFreeShipping(),
                IsPublished = true,
                IsSwatch = IsSwatch(),
                ManufacturerPartNumber = ManufacturerPartNumber,
                MinimumQuantity = GetMinimumQuantity(),
                OrderIncrementQuantity = OrderIncrementQuantity,
                //OrderRequirementsNotice = x.OrderRequirementsNotice,
                OurPrice = OurPrice,
                PublicProperties = new Dictionary<string, string>(),
                RetailPrice = RetailPrice,
                SalePrice = SalePrice,
                Shape = GetShape(),
                SKUSuffix = SKUSuffix,
                UnitOfMeasure = IsSwatch() ? UnitOfMeasure.Swatch : VendorProduct.UnitOfMeasure,

                // update fields
                VariantID = matchingVariant.VariantID,
                ProductID = sqlProduct.ProductID
            };
            if (this is RugVendorVariant)
                variant.RugFeatures = (this as RugVendorVariant).RugProductVariantFeatures;
            return variant;
        }

        private StoreProductVariant FindMatchingSqlVariant(List<StoreProductVariant> sqlVariants, VendorVariant variant)
        {
            StoreProductVariant matchingSqlVariant;
            // checking for existing variant to do comparison
            if (variant.IsSwatch())
            {
                // we want to find the swatch variant
                matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber && x.IsSwatch);
            }
            else
            {
                // we want to find the non-swatch variant
                matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber && x.IsDefault);
                if (matchingSqlVariant == null)
                    matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber);
            }
            return matchingSqlVariant;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Pipeline.Metadata
{
    public class DefaultProductValidator<T> : IProductValidator<T> where T : Vendor, new()
    {
        public virtual ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var vendor = new T();
            var excludedReasons = new List<ExcludedReason>();
            if (product.Name.Contains("\n")) excludedReasons.Add(ExcludedReason.NameContainsNewlines);
            if (string.IsNullOrWhiteSpace(product.Name)) excludedReasons.Add(ExcludedReason.MissingName);

            if (ViolatesTrademarks(product.Name)) excludedReasons.Add(ExcludedReason.TrademarkViolation);

            var images = product.GetProductImages(string.Empty);
            // TODO: need to check this somewhere else, since validation has not ocurred yet - meaning there can be multiple primary at this point
            //if (images.Count(x => x.IsDefault) != 1) excludedReasons.Add(ExcludedReason.MultipleDefaultImages);
            if (images.Any(x => string.IsNullOrWhiteSpace(x.Filename))) excludedReasons.Add(ExcludedReason.ImageMissingFilename);
            if (images.Any(x => string.IsNullOrWhiteSpace(x.SourceUrl))) excludedReasons.Add(ExcludedReason.ImageMissingSource);
            if (images.Count() != images.Select(x => x.SourceUrl).Distinct().Count()) excludedReasons.Add(ExcludedReason.DuplicateImages);

            var variants = product.GetPopulatedVariants();
            var nonSwatches = variants.Where(x => !x.IsSwatch()).ToList();
            if (!variants.Any()) excludedReasons.Add(ExcludedReason.MissingVariants);
            if (variants.Count(x => x.IsDefault) != 1) excludedReasons.Add(ExcludedReason.NotExactlyOneDefaultVariant);
            if (variants.Select(x => x.SKUSuffix ?? "").Distinct().Count() != variants.Count()) excludedReasons.Add(ExcludedReason.VariantSKUsNotDistinct);
            if (nonSwatches.Select(x => x.ManufacturerPartNumber).Distinct().Count() != nonSwatches.Count()) excludedReasons.Add(ExcludedReason.VariantSKUsNotDistinct);

            if (vendor.Store == StoreType.InsideFabric || vendor.Store == StoreType.InsideWallpaper) 
                excludedReasons.AddRange(ValidateFabricWallpaper(product as FabricProduct));
            if (vendor.Store == StoreType.InsideAvenue) excludedReasons.AddRange(ValidateHomeware(product as HomewareProduct));
            if (vendor.Store == StoreType.InsideRugs) excludedReasons.AddRange(ValidateRug(product as RugProduct));

            return new ProductValidationResult(product, excludedReasons);
        }

        private bool ViolatesTrademarks(string productName)
        {
            var trademarkPhrases = new List<string> {"Hershey", "Twizzler", "Kit Kat", "Twix"};
            return productName.ContainsAnyOfIgnoreCase(trademarkPhrases.ToArray());
        }

        private List<ExcludedReason> ValidateFabricWallpaper(FabricProduct product)
        {
            var excludedReasons = new List<ExcludedReason>();
            var images = product.GetProductImages(string.Empty);
            // TODO: need to check this somewhere else, since validation has not ocurred yet - meaning there can be multiple primary at this point
            //if (images.Count(x => x.ImageVariant == "Primary") != 1) excludedReasons.Add(ExcludedReason.FabricRequiresOnePrimaryImage);

            if (!product.PublicProperties.Any()) excludedReasons.Add(ExcludedReason.MissingPublicProperties);
            if (string.IsNullOrWhiteSpace(product.Correlator)) excludedReasons.Add(ExcludedReason.MissingRequiredCorrelator);
            return excludedReasons;
        }

        private List<ExcludedReason> ValidateHomeware(HomewareProduct product)
        {
            var excludedReasons = new List<ExcludedReason>();
            if (product.HomewareCategory == HomewareCategory.Root) excludedReasons.Add(ExcludedReason.HomewareAssignedToRoot);
            if (product.HomewareCategory == HomewareCategory.Unknown) excludedReasons.Add(ExcludedReason.HomewareCategoryUnknown);
            else if (!product.HomewareCategory.CategoryIncluded()) excludedReasons.Add(ExcludedReason.HomewareCategoryExcluded);

            if (product.MinimumQuantity > 4) excludedReasons.Add(ExcludedReason.HighMinimumQuantity);
            if (product.ProductFeatures == null) excludedReasons.Add(ExcludedReason.HomewarePropertiesNull);

            var images = product.GetProductImages(string.Empty);
            if (images.Count(x => x.ImageVariant == "Primary") != 1) excludedReasons.Add(ExcludedReason.HomewareRequiresOnePrimaryImage);
            if (images.Any(x => x.ImageVariant != "Primary" && x.ImageVariant != "Scene")) excludedReasons.Add(ExcludedReason.HomewareImagesMustBePrimaryOrScene);
            return excludedReasons;
        }

        private List<ExcludedReason> ValidateRug(RugProduct product)
        {
            var excludedReasons = new List<ExcludedReason>();
            var variants = product.GetPopulatedVariants();
            if (variants.Count == 1 &&
                variants.First().GetShape() == ProductShapeType.Sample)
            {
                excludedReasons.Add(ExcludedReason.RugOnlySample);
            }

            if (product.RugProductFeatures == null) excludedReasons.Add(ExcludedReason.RugPropertiesNull);
            if (string.IsNullOrWhiteSpace(product.Correlator)) excludedReasons.Add(ExcludedReason.MissingRequiredCorrelator);

            var images = product.GetProductImages(string.Empty);
            if (images.Any(x => x.ImageVariant == "Primary") ||
                images.Any(x => x.ImageVariant == "None") ||
                images.Any(x => x.ImageVariant == "Alternate")) excludedReasons.Add(ExcludedReason.InvalidImageVariant);

            return excludedReasons;
        }
    }
}
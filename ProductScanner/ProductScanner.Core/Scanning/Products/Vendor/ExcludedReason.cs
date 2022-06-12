namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public enum ExcludedReason
    {
        HomewareAssignedToRoot,
        HomewareCategoryUnknown,
        HomewareCategoryExcluded,
        RugOnlySample,
        FreightOnly,
        HandKnotted,
        // some are not allowed to be sold online
        UnapprovedProduct,
        ObsoleteProduct,
        HighMinimumQuantity,
        HasLowCost,
        HasLowPrice,
        HasRetailLowerThanOurPrice,
        HasOurPriceLowerThanCost,
        MissingCost,
        MissingOurPrice,
        MissingUnitOfMeasure,
        MissingProductGroup,
        MissingImage,

        TrademarkViolation,

        // Bad Data Reasons
        MissingVariants,
        NotExactlyOneDefaultVariant,
        VariantSKUsNotDistinct,
        MissingName,
        NameContainsNewlines,
        RugPropertiesNull,
        HomewarePropertiesNull,
        InvalidImageVariant,
        MultipleDefaultImages,
        ImageMissingSource,
        ImageMissingFilename,
        DuplicateImages,
        HomewareRequiresOnePrimaryImage,
        HomewareImagesMustBePrimaryOrScene,
        FabricRequiresOnePrimaryImage,
        MissingPublicProperties,
        MissingRequiredCorrelator,
        MissingMPN,
        OrderIncrementZero,
        MinimumQuantityZero,
        RugShapeMissing,
        InvalidUnitOfMeasure,
        WallcoveringExcluded,
        HighShippingCost
    }
}
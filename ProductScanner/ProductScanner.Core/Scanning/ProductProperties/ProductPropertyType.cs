using System.ComponentModel;

namespace ProductScanner.Core.Scanning.ProductProperties
{
    /// <summary>
    /// A list of all possible property dictionary keys for VendorProperties and ProductProperties.
    /// </summary>
    /// <remarks>
    /// This list of names is used for both VendorProperties (in ProductBase.cs) and in ProductProperties.
    /// 
    /// Any items decorated with the ExcludedProductPropertyAttribute are not allowed to be inserted
    /// into the PropductProperties dictionary in NewProduct. These are used only for raw data collection
    /// from the vendor.
    /// 
    /// This is an ordered list based on analysis of the existing product database.
    /// It would be rare at this point to come across the need to add new properties
    /// to this list. When deciding what to name a property when multiple names
    /// might seem to fit, generally prefers the names which are listed first.
    /// </remarks>
    public enum ProductPropertyType
    {
        /// <summary>
        /// The vendor's part number in their own format which matches up with their website/spreadsheets.
        /// </summary>
        /// <remarks>
        /// Must be unique for each product for the vendor. This the field we use to sync our products
        /// to theirs. We want as concrete a match as possible. This is also what our staff sees to
        /// indicate what product they should order from the manufacturer.
        /// Becomes pv.ManufacturerPartNumber in SQL
        /// </remarks>
        [ExcludedProductProperty]
        [Description("Manufacturer Part Number")]
        ManufacturerPartNumber,

        /// <summary>
        /// What we pay - our cost.
        /// </summary>
        /// <remarks>
        /// It's important to be able to detect changes in cost early on so we can update our website.
        /// Nearly all cost changes are increases. If we don't catch early, we're selling at below
        /// our cost. Becomes pv.Cost in SQL.
        /// </remarks>
        [ExcludedProductProperty]
        [Description("Wholesale Price")]
        WholesalePrice,

        [ExcludedProductProperty]
        [Description("Multiplier")]
        Multiplier,

        [ExcludedProductProperty]
        [Description("MAP")]
        MAP,

        [ExcludedProductProperty]
        [Description("Discontinued")]
        IsDiscontinued,

        /// <summary>
        /// This is the MSRP. It is not what we sell for.
        /// </summary>
        /// <remarks>
        /// Shows on the web as the comparison price to let people see how much they save when
        /// buying from us. Becomes pv.Price in SQL.
        /// </remarks>
        [ExcludedProductProperty]
        [Description("Retail Price")]
        RetailPrice,

        [ExcludedProductProperty]
        [Description("Last Full Update")]
        LastFullUpdate,

        /// <summary>
        /// Actual inventory count from the vendor.
        /// </summary>
        /// <remarks>
        /// Sometimes, all you know is "in stock" or "out of stock". Then use 999999 and 0.
        /// Our goal is to get real-time stock indication on the website.
        /// </remarks>
        [ExcludedProductProperty]
        [Description("StockCount")]
        StockCount,

        [ExcludedProductProperty]
        [Description("Limited Availability")]
        IsLimitedAvailability,

        [ExcludedProductProperty]
        [Description("Stocked at Mill")]
        StockedAtMill,

        [ExcludedProductProperty]
        [Description("Largest Piece")]
        LargestPiece,

        /// <summary>
        /// Url to a nice big product image. The web service will resize into large, medium and small.
        /// </summary>
        [ExcludedProductProperty]
        [Description("Image URL")]
        ImageUrl,

        [ExcludedProductProperty]
        [Description("Alternate Image URL")]
        AlternateImageUrl,


        /// <summary>
        /// Sometimes, the product detail page URL is highly SEO optimized with text and does not follow
        /// an unambiguous template. In such scenarios, it's useful to capture the product detail directly
        /// from the website and use it in subsequent processing
        /// </summary>
        [ExcludedProductProperty]
        [Description("Product Detail URL")]
        ProductDetailUrl,

        [ExcludedProductProperty]
        [Description("HasBeenEdited")]
        HasBeenEdited,

        /// <summary>
        /// True when is an outlet/clearance product.
        /// </summary>
        /// <remarks>
        /// Do not set unless have specific instructions to do so.
        /// </remarks>
        [ExcludedProductProperty]
        [Description("Is Clearance")]
        IsClearance,

        /// <summary>
        /// One of Fabric|Wallcovering|Trim|Rug
        /// </summary>
        [ExcludedProductProperty]
        [Description("Product Group")]
        ProductGroup,

        [ExcludedProductProperty]
        [Description("Pattern Correlator")]
        PatternCorrelator,

        [ExcludedProductProperty]
        [Description("Classification")]
        Classification,

        [ExcludedProductProperty]
        [Description("Subclass")]
        Subclass,

        [ExcludedProductProperty]
        [Description("Order Increment")]
        OrderIncrement,

        // this is an integer value used internally
        [ExcludedProductProperty]
        [Description("Minimum Quantity")]
        MinimumQuantity,

        // this is for when the vendor presents costs one way (like for double rolls) and we want
        // to show it another way (like single rolls) -- so this is a multiplier.
        [ExcludedProductProperty]
        [Description("Price Adjustment Factor")]
        PriceAdjustmentFactor,

        /// <summary>
        /// The filename we we use for saving the product image on the server.
        /// </summary>
        [ExcludedProductProperty]
        [Description("Image Filename Override")]
        ImageFilenameOverride,

        /// <summary>
        /// Our globally unique product identifer in the form of XX-YYYYYYY
        /// </summary>
        [ExcludedProductProperty]
        [Description("SKU")]
        SKU,

        /// <summary>
        /// Used internally.
        /// </summary>
        [ExcludedProductProperty]
        [Description("Ignore")]
        Ignore,

        [ExcludedProductProperty]
        [Description("Spec")]
        Spec,

        [ExcludedProductProperty]
        [Description("Status")]
        Status,


        [ExcludedProductProperty]
        [Description("Description")]
        Description,

        // These are a set of general-purpose temp properties. Their intended use
        // is for storing properties that are later concatenated and used to 
        // hydrate other properties
        [ExcludedProductProperty]
        [Description("TempContent1")]
        TempContent1,
        [ExcludedProductProperty]
        [Description("TempContent2")]
        TempContent2,
        [ExcludedProductProperty]
        [Description("TempContent3")]
        TempContent3,
        [ExcludedProductProperty]
        [Description("TempContent4")]
        TempContent4,
        [ExcludedProductProperty]
        [Description("TempContent5")]
        TempContent5,
        [ExcludedProductProperty]
        [Description("TempContent6")]
        TempContent6,
        [ExcludedProductProperty]
        [Description("TempContent7")]
        TempContent7,
        [ExcludedProductProperty]
        [Description("TempContent8")]
        TempContent8,
        [ExcludedProductProperty]
        [Description("TempContent9")]
        TempContent9,

        [ExcludedProductProperty]
        [Description("TempContent10")]
        TempContent10,
        [ExcludedProductProperty]
        [Description("TempContent11")]
        TempContent11,
        [ExcludedProductProperty]
        [Description("TempContent12")]
        TempContent12,

        [ExcludedProductProperty]
        [Description("Price1")]
        Price1,
        [ExcludedProductProperty]
        [Description("Price2")]
        Price2,
        [ExcludedProductProperty]
        [Description("Price3")]
        Price3,
        [ExcludedProductProperty]
        [Description("Price4")]
        Price4,
        [ExcludedProductProperty]
        [Description("Price5")]
        Price5,
        [ExcludedProductProperty]
        [Description("Price6")]
        Price6,
        [ExcludedProductProperty]
        [Description("Price7")]
        Price7,
        [ExcludedProductProperty]
        [Description("Price8")]
        Price8,
        [ExcludedProductProperty]
        [Description("Price9")]
        Price9,

        [ExcludedProductProperty]
        [Description("Image1")]
        Image1,
        [ExcludedProductProperty]
        [Description("Image2")]
        Image2,
        [ExcludedProductProperty]
        [Description("Image3")]
        Image3,
        [ExcludedProductProperty]
        [Description("Image4")]
        Image4,
        [ExcludedProductProperty]
        [Description("Image5")]
        Image5,
        [ExcludedProductProperty]
        [Description("Image6")]
        Image6,
        [ExcludedProductProperty]
        [Description("Image7")]
        Image7,
        [ExcludedProductProperty]
        [Description("Image8")]
        Image8,

        [ExcludedProductProperty]
        [Description("Color1")]
        Color1,
        [ExcludedProductProperty]
        [Description("Color2")]
        Color2,
        [ExcludedProductProperty]
        [Description("Color3")]
        Color3,
        [ExcludedProductProperty]
        [Description("Color4")]
        Color4,
        [ExcludedProductProperty]
        [Description("Color5")]
        Color5,
        [ExcludedProductProperty]
        [Description("Background Color")]
        BackgroundColor,

        [ExcludedProductProperty]
        [Description("Style1")]
        Style1,
        [ExcludedProductProperty]
        [Description("Style2")]
        Style2,
        [ExcludedProductProperty]
        [Description("Style3")]
        Style3,
        [ExcludedProductProperty]
        [Description("Style4")]
        Style4,
        [ExcludedProductProperty]
        [Description("Style5")]
        Style5,

        [ExcludedProductProperty]
        [Description("Category1")]
        Category1,
        [ExcludedProductProperty]
        [Description("Category2")]
        Category2,
        [ExcludedProductProperty]
        [Description("Category3")]
        Category3,
        [ExcludedProductProperty]
        [Description("Category4")]
        Category4,
        [ExcludedProductProperty]
        [Description("Category5")]
        Category5,

        [ExcludedProductProperty]
        [Description("Type1")]
        Type1,
        [ExcludedProductProperty]
        [Description("Type2")]
        Type2,


        [ExcludedProductProperty]
        [Description("Pile")]
        Pile,

        [ExcludedProductProperty]
        [Description("Embroidery")]
        Embroidery,


        [ExcludedProductProperty]
        [Description("Scale")]
        Scale,

        [ExcludedProductProperty]
        PileHeight,

        [ExcludedProductProperty]
        PantoneTPX,

        [ExcludedProductProperty]
        SampleType,


        [ExcludedProductProperty]
        [Description("PatternAndColor")]
        PatternAndColor,

        [ExcludedProductProperty]
        [Description("WidthInches")]
        WidthInches,
        [ExcludedProductProperty]
        [Description("LengthInches")]
        LengthInches,
        [ExcludedProductProperty]
        [Description("Wattage")]
        Wattage,

        // properties below are for the property dictionary used to populate
        // the right side of the fabric product page.

        // the web service will order these on the fabric website pretty much
        // in the order shown here. Most values are optional. We'll display
        // what we find. 

        // these properties also contribute to the full text search and
        // autocomplete drop list.

        [Description("Brand")]
        Brand,
        [Description("Manufacturer")]
        Manufacturer,
        [Description("Web Item Number")]
        WebItemNumber,
        [Description("Item Number")]
        ItemNumber,
        [Description("Alternate Item Number")]
        AlternateItemNumber,
        [Description("Model Number")]
        ModelNumber,
        [Description("Product")]
        Product,
        [Description("Large Cord")]
        LargeCord,
        [Description("Cord")]
        Cord,
        [Description("Cordette")]
        Cordette,
        [Description("Tassel")]
        Tassel,
        [Description("Product Name")]
        ProductName,
        [Description("Short Name")]
        ShortName,
        [Description("Minimum Order")]
        MinimumOrder,
        [Description("Order Prerequisite")]
        OrderPrerequisite,
        [Description("CFA")]
        CFA,
        [Description("Pattern")]
        Pattern,
        [Description("Pattern Name")]
        PatternName,
        [Description("Pattern Number")]
        PatternNumber,
        [Description("Tariff")]
        Tariff,
        [Description("Color")]
        Color,
        [Description("Color Name")]
        ColorName,
        [Description("Color Number")]
        ColorNumber,
        [Description("Color Group")]
        ColorGroup,
        [Description("Book")]
        Book,
        [Description("Book Number")]
        BookNumber,
        [Description("Designer")]
        Designer,
        [Description("Collection")]
        Collection,
        [Description("Collection Id")]
        CollectionId,
        [Description("Category")]
        Category,
        [Description("Group")]
        Group,
        [Description("Feature")]
        Feature,
        [Description("Width")]
        Width,
        [Description("Length")]
        Length,
        [Description("Height")]
        Height,
        [Description("Depth")]
        Depth,
        [Description("Border Height")]
        BorderHeight,
        [Description("Product Use")]
        ProductUse,
        [Description("Product Type")]
        ProductType,
        [Description("Type")]
        Type,
        [Description("Material")]
        Material,
        [Description("Style")]
        Style,
        [Description("Upholstery Use")]
        UpholsteryUse,
        [Description("Use")]
        Use,
        [Description("Coordinates")]
        Coordinates,
        [Description("Unit Of Measure")]
        UnitOfMeasure,
        [Description("Unit")]
        Unit,
        [Description("Design")]
        Design,
        [Description("Backing")]
        Backing,
        [Description("Construction")]
        Construction,
        [Description("Finish")]
        HardwareFinish,
        [Description("Finish")]
        Finish,
        [Description("Finish Treatment")]
        FinishTreatment,
        [Description("Finishes")]
        Finishes,
        [Description("Edge Feature")]
        EdgeFeature,
        [Description("Furniture Grade")]
        FurnitureGrade,
        [Description("Grade")]
        Grade,
        [Description("Repeat")]
        Repeat,
        [Description("Horizontal Repeat")]
        HorizontalRepeat,
        [Description("Vertical Repeat")]
        VerticalRepeat,
        [Description("Horizontal Half Drop")]
        HorizontalHalfDrop,
        [Description("Direction")]
        Direction,
        [Description("Hide Size")]
        HideSize,
        [Description("Match")]
        Match,
        [Description("Prepasted")]
        Prepasted,
        [Description("Paste")]
        Paste,
        [Description("Removal")]
        Removal,
        [Description("Coverage")]
        Coverage,
        [Description("Railroaded")]
        Railroaded,
        [Description("Soft Home Grade")]
        SoftHomeGrade,
        [Description("Order Requirements Notice")]
        OrderRequirementsNotice,
        [Description("Strippable")]
        Strippable,
        [Description("Thickness")]
        Thickness,
        [Description("Treatment")]
        Treatment,
        [Description("Weight")]
        Weight,
        [Description("Country of Finish")]
        CountryOfFinish,
        [Description("Country of Origin")]
        CountryOfOrigin,
        [Description("Area")]
        Area,
        [Description("Shape")]
        Shape,
        [Description("Content")]
        Content,
        [Description("Fabric Contents")]
        FabricContents,
        [Description("Base")]
        Base,
        [Description("Fabric Performance")]
        FabricPerformance,
        [Description("Durability")]
        Durability,
        [Description("Fire Code")]
        FireCode,
        [Description("Flammability")]
        Flammability,
        [Description("Flame Retardant")]
        FlameRetardant,
        [Description("WYZ/MAR")]
        WYZMAR,
        [Description("Cleaning")]
        Cleaning,
        [Description("Cleaning Code")]
        CleaningCode,
        [Description("UV Protection Factor")]
        UVProtectionFactor,
        [Description("UV Resistance")]
        UVResistance,
        [Description("Code")]
        Code,
        [Description("Other")]
        Other,
        [Description("Comment")]
        Comment,
        [Description("Note")]
        Note,
        [Description("Additional Info")]
        AdditionalInfo,
        [Description("Average Bolt")]
        AverageBolt,
        [Description("Lead Time")]
        LeadTime,
        [Description("Order Info")]
        OrderInfo,
        [Description("Same As SKU")]
        SameAsSKU,
        [Description("Layout")]
        Layout,
        [Description("Screens")]
        Screens,
        [Description("Cord Spread")]
        CordSpread,
        [Description("UPC")]
        UPC,
        [Description("Bullets")]
        Bullets,
        [Description("Dimensions")]
        Dimensions,
        [Description("Shipping Weight")]
        ShippingWeight,
        [Description("Shipping Method")]
        ShippingMethod,
        [Description("Packaging")]
        Packaging,
        [Description("Package Length")]
        PackageLength,
        [Description("Package Width")]
        PackageWidth,
        [Description("Package Height")]
        PackageHeight,
        [Description("Number of Panels")]
        NumberOfPanels,
        [Description("Number of Sheets")]
        NumberOfSheets,
        [Description("Number of Pieces")]
        NumberOfPieces,
        [Description("Outer Diameter")]
        OuterDiameter,
        [Description("Inner Diameter")]
        InnerDiameter,
        [Description("Diameter")]
        Diameter,
        [Description("Size")]
        Size,
        [Description("Drop")]
        Drop,

        [Description("League")]
        League,
        [Description("Theme")]
        Theme,
        [Description("Made to Order")]
        MadeToOrder
    }

}

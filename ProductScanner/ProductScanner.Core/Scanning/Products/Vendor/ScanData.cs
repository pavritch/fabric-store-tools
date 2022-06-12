using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using InsideFabric.Data;
using Newtonsoft.Json;
using Utilities;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public enum ProductShapeType
    {
        // this value should not show up in any committed data.
        [Description("None")]
        None,

        [Description("Rectangular")]
        Rectangular,

        [Description("Square")]
        Square,

        [Description("Oval")]
        Oval,

        [Description("Round")]
        Round,

        [Description("Octagon")]
        Octagon,

        [Description("Runner")]
        Runner,

        [Description("Star")]
        Star,

        [Description("Heart")]
        Heart,

        [Description("Kidney")]
        Kidney,

        // not sure what a basket shape is?
        [Description("Basket")]
        Basket,

        // like an animal skin (bear rug, etc.)
        [Description("Animal")]
        Animal,

        [Description("Novelty")]
        Novelty,

        // the item is designated as a sample piece (and if is a sample, never use rectangle or square, etc.)
        [Description("Sample")]
        Sample,

        // try real hard not to use other.
        [Description("Other")]
        Other,
    }


    public static class ShapeTypeExtensions
    {
        public static ImageVariantType ToImageVariantType(this ProductShapeType shapeType)
        {
            ImageVariantType imageType;
            Enum.TryParse(shapeType.ToString(), out imageType);
            return imageType;
        }

        public static ImageVariantType ToImageVariantType(this string shape)
        {
            ImageVariantType imageType;
            Enum.TryParse(shape, out imageType);
            return imageType;
        }
    }

    public class ScannedImage
    {
        public Guid Id { get; set; }
        public ImageVariantType ImageVariantType { get; set; }
        public string Url { get; set; }
        public NetworkCredential NetworkCredential { get; set; }

        public ScannedImage(ImageVariantType imageVariantType, string url, NetworkCredential networkCredential = null)
        {
            Id = Guid.NewGuid();
            ImageVariantType = imageVariantType;
            Url = url.Replace(" ", "%20");
            NetworkCredential = networkCredential;
        }

        public string UrlWithCredentials()
        {
            if (NetworkCredential == null) return Url;
            var withCredentials = Url.Replace("ftp://", string.Format("ftp://{0}:{1}@", NetworkCredential.UserName, NetworkCredential.Password));
            return withCredentials;
        }
    }

    public class ProductPriceData
    {
        public decimal OurPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? SalePrice { get; set; }

        public ProductPriceData(decimal ourPrice, decimal retailPrice, decimal? salePrice = null)
        {
            OurPrice = Math.Round(ourPrice, 2);
            RetailPrice = Math.Round(retailPrice, 2);
            if (salePrice.HasValue) SalePrice = Math.Round(salePrice.Value, 2);
        }
    }

    [JsonConverter(typeof(ScanDataConverter))]
    public class ScanData : Dictionary<ScanField, string>
    {
        public ScanData()
        {
            RelatedProducts = new List<string>();
            _scannedImages = new List<ScannedImage>();
            Variants = new List<ScanData>();
        }

        public ScanData(ScanData properties) : this()
        {
            if (properties == null) return;
            foreach (var kvp in properties)
            {
                this[kvp.Key] = kvp.Value;
            }

            DetailUrl = properties.DetailUrl;
            Cost = properties.Cost;
            _scannedImages = new List<ScannedImage>(properties.GetScannedImages());
        }

        public void AddFields(ScanData properties)
        {
            foreach (var kvp in properties)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public new string this[ScanField prop]
        {
            get
            {
                if (!ContainsKey(prop)) return string.Empty;
                if (base[prop] == null) return string.Empty;
                return base[prop];
            }
            set
            {
                if (value == null)
                {
                    base[prop] = null;
                    return;
                }

                var str = value.Trim();
                base[prop] = str;
            }
        }

        public void SetFieldIfNotSet(ScanField field, string value)
        {
            if (!ContainsKey(field) || string.IsNullOrWhiteSpace(base[field])) this[field] = value;
        }

        // other colorways
        public List<string> RelatedProducts { get; set; }

        private List<ScannedImage> _scannedImages; 
        public List<ScanData> Variants { get; set; }
        public bool IsDiscontinued { get; set; }
        public bool IsLimitedAvailability { get; set; }
        public bool IsClearance { get; set; }
        public bool IsSkipped { get; set; }
        public bool MadeToOrder { get; set; }
        public bool RemoveSwatch { get; set; }

        public decimal Cost { get; set; }
        public Uri DetailUrl { get; set; }
        public bool IsBolt { get; set; }

        public string GetDetailUrl()
        {
            if (DetailUrl == null) return string.Empty;
            return DetailUrl.AbsoluteUri;
        }

        public void AddImage(ScannedImage image)
        {
            // don't add duplicate image urls
            if (_scannedImages.Any(x => x.Url == image.Url)) return;
            _scannedImages.Add(image);
        }

        public List<ScannedImage> GetScannedImages()
        {
            return _scannedImages;
        }

        public bool HasCorrelator()
        {
            return !string.IsNullOrWhiteSpace(this[ScanField.PatternCorrelator]);
        }

        public bool HasWholesalePrice()
        {
            return Cost > 0;
        }
    }

    // We want to move away from using ProductPropertyType for the scan, since it is also 
    // used for the  website display. These two things are separate responsibilities, so 
    // they should be separate objects
    public enum ScanField
    {
        Ignore,
        Wattage,
        Collection,
        Color,
        Finish,
        Depth,
        Height,
        Weight,
        PackageWidth,
        PackageHeight,
        ShippingWeight,
        SKU,
        Width,
        Category,
        ProductName,
        RetailPrice,
        Description,
        LongDescription,
        StockCount,
        ManufacturerPartNumber,
        PatternCorrelator,
        ProductGroup,
        PatternName,
        ImageUrl,
        Dimensions,
        AssemblyInstruction,
        Bulbs,
        CordLength,
        BaseWidth,
        ColorGroup,
        Material,
        Material2,
        Material3,
        Material4,
        Note,
        UnitOfMeasure,
        AlternateItemNumber,
        Classification,
        UPC,
        MinimumQuantity,
        ColorName,
        Length,
        Packaging,
        Code,
        Group,
        ProductType,
        HardwareFinish,
        Shape,
        Designer,
        Bullets,
        Diameter,
        Country,
        Insert,
        Content,
        NumberOfLights,
        Shade,
        WattagePerLight,
        ChainLength,
        Status,
        Cost,
        DiscountedPrice,
        ItemExtension,
        PackageLength,
        FreightOnly,
        Socket,
        ShadeColor,
        MAP,
        ItemNumber,
        Design,
        Construction,
        Backing,
        Style,
        Cleaning,
        Size,
        Pile,
        Railroaded,
        FrameStyle,
        FireCode,
        VerticalRepeat,
        ProductUse,
        FlameRetardant,
        Durability,
        PatternNumber,
        Brand,
        NumberOfPanels,
        NumberOfPieces,
        NumberOfSheets,
        Book,
        BookNumber,
        Match,
        League,
        EdgeFeature,
        Prepasted,
        Strippable,
        Repeat,
        Coverage,
        Theme,
        Other,
        Style1,
        Style2,
        Bullet1,
        Bullet2,
        Bullet3,
        Bullet4,
        Bullet5,
        Bullet6,
        Bullet7,
        Bullet8,
        Bullet9,
        Bullet10,
        HorizontalRepeat,
        ColorNumber,
        IsLimitedAvailability,
        Pattern,
        IsExclusive,
        IncomingStock,
        Drop,
        Attribute,
        CFA,
        OrderInfo,
        IsDiscontinued,
        AverageBolt,
        IsBolt,
        InnerDiameter,
        OuterDiameter,
        Scale,
        Use,
        Category1,
        Category2,
        Category3,
        Category4,
        Style3,
        Style4,
        DrawerDimensions,
        SeatDimensions,
        Image1,
        Image2,
        Image3,
        Image4,
        Image5,
        Image6,
        Image7,
        IsClearance,
        Direction,
        AdditionalInfo,
        OrderIncrement,
        CutFee,
        MinimumOrder,
        Flammability,
        EmbroideredWidth,
        HorizontalHalfDrop,
        Color1,
        Color2,
        Color3,
        Type1,
        Type2,
        AlternateImageUrl,
        CA_TB117,
        Coordinates,
        HasSwatch,
        FabricPerformance,
        Screens,
        Layout,
        CordSpread,
        PriceAdjustmentFactor,
        WebItemNumber,
        BorderHeight,
        Type,
        PileHeight,
        LeadTime,
        Warranty,
        DetailUrlTEMP,
        SecondaryMaterials,
        ShippingInfo,
        ShippingMethod,
        Category5,
        Breadcrumbs,
        BulbReplacement,
        ShadeSize,
        Tags,
        FoodSafe,
        RoomType,
        WoodType,
        SeatHeight,
        AdjustableHeight,
        ArmHeight,
        Drawers,
        Season,
        Multiplier,
        Voltage,
        Lumens,
        SwitchType,

        // used for ScanDataConverter
        Prop_Cost,
        ShadeBottomDiameter,
        ShadeSide,
        ShadeTopDiameter,
        WirewayPosition,
        SwitchLocation,
        SwitchColor,
        SocketWattage,
        SocketType,
        SocketQuantity,
        SocketColor,
        Plug,
        LampBottom,
        HarpSize,
        DampRatedCovered,
        CordType,
        CordColor,
        Bulb,
        Substrate,
        Coordinates2,
        Coordinates3,
        Coordinates4,
        Coordinates5,
        OurPrice,
        ESellable,
        FinishCode,
        ShadeMaterial,
        Keywords,
        CategoryCode,
        AssembledWidth,
        AssembledHeight,
        AssembledDepth,
        AssembledLength,
        CollectionId,
        Filename,
        CollectionExpiration,
        Lightfastness,
        Closure,
        StockInventory,
        Prop65
    }

    public enum ProductClass
    {
        None,
        Wallpaper,
        WallMurals,
        WallDecals,
        Border,
        Applique,
        AdhesiveFilm
    }

    public enum ProductGroup
    {

        [Description("None")]
        None,

        [Description("Fabric")]
        Fabric,

        [Description("Wallcovering")]
        Wallcovering,

        [Description("Trim")]
        Trim,

        [Description("Rug")]
        Rug,

        [Description("Homeware")]
        Homeware
    }


    /// <summary>
    /// Allowed units of measure. Description MUST exactly match what's in SQL.
    /// </summary>
    /// <remarks>
    /// Description attribute is used to persist value to SQL, and also used to resolve back.
    /// </remarks>
    public enum UnitOfMeasure
    {
        [Description("None")]
        None,

        [Description("Each")]
        Each,

        [Description("Pair")]
        Pair,

        [Description("Bag of 10")]
        Bag10,

        [Description("Yard")]
        Yard,

        [Description("Roll")]
        Roll,

        [Description("Square Foot")]
        SquareFoot,

        [Description("Square Meter")]
        SquareMeter,

        [Description("Meter")]
        Meter,

        [Description("Panel")]
        Panel,

        [Description("Tile")]
        Tile,

        [Description("Swatch")]
        Swatch
    }

    public static class StoreProductExtensions
    {
        public static UnitOfMeasure ToUnitOfMeasure(this string input)
        {
            // uses description to match

            if (string.IsNullOrWhiteSpace(input))
                return UnitOfMeasure.None;

            foreach(var v in LibEnum.GetValues<UnitOfMeasure>())
            {
                if (LibEnum.GetDescription(v) == input)
                    return v;
            }

            return UnitOfMeasure.None;
        }

        public static string ToDescriptionString(this UnitOfMeasure input)
        {
            return LibEnum.GetDescription(input);
        }

        public static ProductGroup ToProductGroup(this string input)
        {
            // note that we're matching on description

            if (string.IsNullOrWhiteSpace(input))
                return ProductGroup.None;

            foreach (var v in LibEnum.GetValues<ProductGroup>())
            {
                if (LibEnum.GetDescription(v).Equals(input, StringComparison.OrdinalIgnoreCase))
                    return v;
            }

            return ProductGroup.None;
        }

        public static ProductShapeType ToProductShape(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ProductShapeType.None;

            foreach (var v in LibEnum.GetValues<ProductShapeType>())
            {
                if (LibEnum.GetDescription(v) == input)
                    return v;
            }

            return ProductShapeType.None;
        }

    }
}


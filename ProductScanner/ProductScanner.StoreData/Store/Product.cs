using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.DataEntities.Store
{
    [Table("Product")]
    public partial class Product
    {
        public virtual List<ProductManufacturer> ProductManufacturer { get; set; }
        public virtual List<ProductVariant> ProductVariants { get; set; }
        public virtual List<ProductCategory> ProductCategories { get; set; } 

        public int ProductID { get; set; }

        public Guid ProductGUID { get; set; }

        [Required]
        [StringLength(400)]
        public string Name { get; set; }

        [Column(TypeName = "ntext")]
        public string Summary { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        public string SEKeywords { get; set; }

        [Column(TypeName = "ntext")]
        public string SEDescription { get; set; }

        [Column(TypeName = "ntext")]
        public string SpecTitle { get; set; }

        [Column(TypeName = "ntext")]
        public string MiscText { get; set; }

        [Column(TypeName = "ntext")]
        public string SwatchImageMap { get; set; }

        [Column(TypeName = "ntext")]
        public string IsFeaturedTeaser { get; set; }

        [Column(TypeName = "ntext")]
        public string FroogleDescription { get; set; }

        [Column(TypeName = "ntext")]
        public string SETitle { get; set; }

        [Column(TypeName = "ntext")]
        public string SENoScript { get; set; }

        [Column(TypeName = "ntext")]
        public string SEAltText { get; set; }

        [Column(TypeName = "ntext")]
        public string SizeOptionPrompt { get; set; }

        [Column(TypeName = "ntext")]
        public string ColorOptionPrompt { get; set; }

        [Column(TypeName = "ntext")]
        public string TextOptionPrompt { get; set; }

        public int ProductTypeID { get; set; }

        public int TaxClassID { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        // This is actually the correlator. Real MPN is ProductVariant.ManufacturerPartNumber
        [StringLength(50)]
        public string ManufacturerPartNumber { get; set; }

        // added convenience method to make callers more clear
        public string PatternCorrelator
        {
            get { return ManufacturerPartNumber; }
        }

        public int SalesPromptID { get; set; }

        [Column(TypeName = "ntext")]
        public string SpecCall { get; set; }

        public byte SpecsInline { get; set; }

        public byte IsFeatured { get; set; }

        [StringLength(100)]
        public string XmlPackage { get; set; }

        public int ColWidth { get; set; }

        public byte Published { get; set; }

        public byte Wholesale { get; set; }

        public byte RequiresRegistration { get; set; }

        public int Looks { get; set; }

        [Column(TypeName = "ntext")]
        public string Notes { get; set; }

        public int? QuantityDiscountID { get; set; }

        [Column(TypeName = "ntext")]
        public string RelatedProducts { get; set; }

        [Column(TypeName = "ntext")]
        public string UpsellProducts { get; set; }

        [Column(TypeName = "money")]
        public decimal UpsellProductDiscountPercentage { get; set; }

        [Column(TypeName = "ntext")]
        public string RelatedDocuments { get; set; }

        public byte TrackInventoryBySizeAndColor { get; set; }

        public byte TrackInventoryBySize { get; set; }

        public byte TrackInventoryByColor { get; set; }

        public byte IsAKit { get; set; }

        public int ShowInProductBrowser { get; set; }

        public int IsAPack { get; set; }

        public int PackSize { get; set; }

        public bool IsDiscontinued
        {
            get { return ShowBuyButton == 0; }
        }
        public int ShowBuyButton { get; set; }

        [Column(TypeName = "ntext")]
        public string RequiresProducts { get; set; }

        public byte HidePriceUntilCart { get; set; }

        public byte IsCalltoOrder { get; set; }

        public byte ExcludeFromPriceFeeds { get; set; }

        public byte RequiresTextOption { get; set; }

        public int? TextOptionMaxLength { get; set; }

        [StringLength(150)]
        public string SEName { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData2 { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData3 { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData4 { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData5 { get; set; }

        [StringLength(10)]
        public string ContentsBGColor { get; set; }

        [StringLength(10)]
        public string PageBGColor { get; set; }

        [StringLength(20)]
        public string GraphicsColor { get; set; }

        [StringLength(512)]
        public string ImageFilenameOverride { get; set; }

        public byte IsImport { get; set; }

        public byte IsSystem { get; set; }

        public byte Deleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public int PageSize { get; set; }

        [StringLength(100)]
        public string WarehouseLocation { get; set; }

        public DateTime AvailableStartDate { get; set; }

        public DateTime? AvailableStopDate { get; set; }

        public byte GoogleCheckoutAllowed { get; set; }

        public int SkinID { get; set; }

        //[Required]
        [StringLength(50)]
        public string TemplateName { get; set; }

        [StringLength(32)]
        public string ProductGroup { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}

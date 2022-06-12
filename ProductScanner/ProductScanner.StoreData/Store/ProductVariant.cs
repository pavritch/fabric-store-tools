using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.DataEntities.Store
{
    [Table("ProductVariant")]
    public partial class ProductVariant
    {
        public virtual Product Product { get; set; }

        [Key]
        public int VariantID { get; set; }

        public Guid VariantGUID { get; set; }

        public int IsDefault { get; set; }

        [StringLength(400)]
        public string Name { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        public string SEKeywords { get; set; }

        [Column(TypeName = "ntext")]
        public string SEDescription { get; set; }

        [Column(TypeName = "ntext")]
        public string Colors { get; set; }

        [Column(TypeName = "ntext")]
        public string ColorSKUModifiers { get; set; }

        [Column(TypeName = "ntext")]
        public string Sizes { get; set; }

        [Column(TypeName = "ntext")]
        public string SizeSKUModifiers { get; set; }

        [Column(TypeName = "ntext")]
        public string FroogleDescription { get; set; }

        public int ProductID { get; set; }

        [StringLength(50)]
        public string SKUSuffix { get; set; }

        [StringLength(50)]
        public string ManufacturerPartNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Column(TypeName = "money")]
        public decimal? SalePrice { get; set; }

        [Column(TypeName = "money")]
        public decimal? Weight { get; set; }

        [Column(TypeName = "money")]
        public decimal? MSRP { get; set; }

        [Column(TypeName = "money")]
        public decimal? Cost { get; set; }

        public int? Points { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }

        public int Inventory { get; set; }

        public int DisplayOrder { get; set; }

        [Column(TypeName = "ntext")]
        public string Notes { get; set; }

        public byte IsTaxable { get; set; }

        public byte IsShipSeparately { get; set; }

        public byte IsDownload { get; set; }

        [Column(TypeName = "ntext")]
        public string DownloadLocation { get; set; }

        public byte FreeShipping { get; set; }

        public byte Published { get; set; }

        public byte Wholesale { get; set; }

        public byte IsSecureAttachment { get; set; }

        public byte IsRecurring { get; set; }

        public int RecurringInterval { get; set; }

        public int RecurringIntervalType { get; set; }

        public int? SubscriptionInterval { get; set; }

        public int? RewardPoints { get; set; }

        [StringLength(100)]
        public string SEName { get; set; }

        [StringLength(250)]
        public string RestrictedQuantities { get; set; }

        public int? MinimumQuantity { get; set; }

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

        public byte Deleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public int SubscriptionIntervalType { get; set; }

        public byte CustomerEntersPrice { get; set; }

        [Column(TypeName = "ntext")]
        public string CustomerEntersPricePrompt { get; set; }

        [Column(TypeName = "ntext")]
        public string SEAltText { get; set; }

        public byte Condition { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}

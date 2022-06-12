using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.DataEntities.Store
{
    [Table("Category")]
    public partial class Category
    {
        public int CategoryID { get; set; }

        public Guid CategoryGUID { get; set; }

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
        public string DisplayPrefix { get; set; }

        [Column(TypeName = "ntext")]
        public string SETitle { get; set; }

        [Column(TypeName = "ntext")]
        public string SENoScript { get; set; }

        [Column(TypeName = "ntext")]
        public string SEAltText { get; set; }

        public int ParentCategoryID { get; set; }

        public int ColWidth { get; set; }

        public byte SortByLooks { get; set; }

        public int DisplayOrder { get; set; }

        [Column(TypeName = "ntext")]
        public string RelatedDocuments { get; set; }

        [StringLength(100)]
        public string XmlPackage { get; set; }

        public byte Published { get; set; }

        public byte Wholesale { get; set; }

        public byte AllowSectionFiltering { get; set; }

        public byte AllowManufacturerFiltering { get; set; }

        public byte AllowProductTypeFiltering { get; set; }

        public int? QuantityDiscountID { get; set; }

        public int ShowInProductBrowser { get; set; }

        [StringLength(100)]
        public string SEName { get; set; }

        [Column(TypeName = "ntext")]
        public string ExtensionData { get; set; }

        [StringLength(10)]
        public string ContentsBGColor { get; set; }

        [StringLength(10)]
        public string PageBGColor { get; set; }

        [StringLength(20)]
        public string GraphicsColor { get; set; }

        [Column(TypeName = "ntext")]
        public string ImageFilenameOverride { get; set; }

        public byte IsImport { get; set; }

        public byte Deleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public int PageSize { get; set; }

        public int TaxClassID { get; set; }

        public int SkinID { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateName { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}

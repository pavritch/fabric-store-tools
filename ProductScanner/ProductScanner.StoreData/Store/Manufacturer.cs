using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.DataEntities.Store
{
    [Table("Manufacturer")]
    public partial class Manufacturer
    {
        public int ManufacturerID { get; set; }

        public Guid ManufacturerGUID { get; set; }

        [Required]
        [StringLength(400)]
        public string Name { get; set; }

        [StringLength(100)]
        public string SEName { get; set; }

        [Column(TypeName = "ntext")]
        public string SEKeywords { get; set; }

        [Column(TypeName = "ntext")]
        public string SEDescription { get; set; }

        [Column(TypeName = "ntext")]
        public string SETitle { get; set; }

        [Column(TypeName = "ntext")]
        public string SENoScript { get; set; }

        [Column(TypeName = "ntext")]
        public string SEAltText { get; set; }

        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(25)]
        public string Suite { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(10)]
        public string ZipCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(25)]
        public string Phone { get; set; }

        [StringLength(25)]
        public string FAX { get; set; }

        [StringLength(255)]
        public string URL { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        public int? QuantityDiscountID { get; set; }

        public byte SortByLooks { get; set; }

        [Column(TypeName = "ntext")]
        public string Summary { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        public string Notes { get; set; }

        [Column(TypeName = "ntext")]
        public string RelatedDocuments { get; set; }

        [StringLength(100)]
        public string XmlPackage { get; set; }

        public int ColWidth { get; set; }

        public int DisplayOrder { get; set; }

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

        public byte Published { get; set; }

        public byte Wholesale { get; set; }

        public int ParentManufacturerID { get; set; }

        public byte IsImport { get; set; }

        public byte Deleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public int PageSize { get; set; }

        public int SkinID { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateName { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}

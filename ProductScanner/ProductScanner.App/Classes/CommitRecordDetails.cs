using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products;

namespace ProductScanner.App
{
    /// <summary>
    ///  Used by right click logic on commit batches to show details about a single record.
    /// </summary>
    public class CommitRecordDetails : ICommitRecordDetails
    {
        /// <summary>
        /// Some kind of visual identifier for this record. Used in title bar of dialog.
        /// </summary>
        /// <remarks>
        /// SKU, ProductID, VariantID - whichever we have at the moment based on the batch type.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// Long name for the product - typically p.Name field.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Image url. Can come from either p.ImageFilenameOverride or StoreProduct, etc.
        /// </summary>
        public string ImageUrl { get; set; }


        /// <summary>
        /// Url to our cart website detail page, else null.
        /// </summary>
        public string StoreUrl { get; set; }

        /// <summary>
        /// Url to vendor product detail page when known, otherwise null.
        /// </summary>
        public string VendorUrl { get; set; }

        /// <summary>
        /// JSON (indented) representation of this record.
        /// </summary>
        public string JSON { get; set; }

        /// <summary>
        /// List of images when the record type includes images, else null.
        /// </summary>
        public List<ProductImage> ProductImages { get; set; }

        /// <summary>
        /// List of variants when the record type includes variants, else null.
        /// </summary>
        public List<StoreProductVariant> ProductVariants { get; set; }
    }
}
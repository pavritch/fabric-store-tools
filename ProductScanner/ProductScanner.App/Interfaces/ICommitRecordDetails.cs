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
    public interface ICommitRecordDetails
    {
        /// <summary>
        /// Some kind of visual identifier for this record. Used in title bar of dialog.
        /// </summary>
        /// <remarks>
        /// SKU, ProductID, VariantID - whichever we have at the moment based on the batch type.
        /// </remarks>
        string Title { get; }

        /// <summary>
        /// Long name for the product - typically p.Name field.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Image url. Can come from either p.ImageFilenameOverride or StoreProduct, etc.
        /// </summary>
        string ImageUrl { get; }

        /// <summary>
        /// Url to store detail page for this product when known, otherwise null.
        /// </summary>
        string StoreUrl { get; }

        /// <summary>
        /// Url to vendor product detail page when known, otherwise null.
        /// </summary>
        string VendorUrl { get; }

        /// <summary>
        /// JSON (indented) representation of this record.
        /// </summary>
        string JSON { get; }

        /// <summary>
        /// List of images when the record type includes images, else null.
        /// </summary>
        List<ProductImage> ProductImages { get; }

        /// <summary>
        /// List of variants when the record type includes variants, else null.
        /// </summary>
        List<StoreProductVariant> ProductVariants { get; }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace InsideFabric.Data
{

    /// <summary>
    /// An image which has been downloaded, processed, and exists on disk in the set of resized folders.
    /// </summary>
    public class LiveProductImage
    {
        /// <summary>
        /// The filename assigned to this image on disk for resized folders.
        /// </summary>
        /// <remarks>
        /// The legacy Ext4 AvailableImages would be a list of these filenames.
        /// </remarks>
        public string Filename { get; set; }

        /// <summary>
        /// When true, indicates that we generated this image from another on the fly.
        /// </summary>
        /// <remarks>
        /// Did not come directly from the vendor.
        /// </remarks>
        public bool IsGenerated { get; set; }

        /// <summary>
        /// True if this image is to be used for imagefilenameoverride and listing pages.
        /// </summary>
        /// <remarks>
        /// Only one in the set should have this flag.
        /// </remarks>
        public bool IsDefault { get; set; }

        /// <summary>
        /// True to indicate that this image should be included/shown on product detail pages.
        /// </summary>
        public bool IsDisplayedOnDetailPage { get; set; }

        /// <summary>
        /// One of the designated shapes.
        /// </summary>
        public string ImageVariant { get; set; }

        /// <summary>
        /// The date this record was created on.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// The width of the original image after cropping.
        /// </summary>
        /// <remarks>
        /// Not necessarily the same as what's on disk in original folder.
        /// </remarks>
        public int CroppedWidth { get; set; }

        /// <summary>
        /// The height of the original image after cropping.
        /// </summary>
        /// <remarks>
        /// Not necessarily the same as what's on disk in original folder.
        /// </remarks>
        public int CroppedHeight { get; set; }

        /// <summary>
        /// Where the image originally was downloaded from. Null for generated images.
        /// </summary>
        public string SourceUrl { get; set; }

        [JsonIgnore]
        public double CroppedAspectRatio
        {
            get
            {
                return (double)CroppedWidth / (double)CroppedHeight;
            }
        }
    }
}
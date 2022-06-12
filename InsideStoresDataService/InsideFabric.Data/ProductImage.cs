using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace InsideFabric.Data
{

    /// <summary>
    /// The order listed here generally determines how to choose the IsDefault image to be used for category/search listings.
    /// </summary>
    public enum ImageVariantType
    {
        // for fabric and wallpaper (face shots), but NEVER for rugs. Rugs should use their shape.
        [Description("Primary")]
        Primary,

        [Description("Rectangular")]
        Rectangular,

        [Description("Square")]
        Square,

        [Description("Oval")]
        Oval,

        [Description("Round")]
        Round,

        [Description("ScallopedRound")]
        ScallopedRound,

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

        // this would rarely be used - if ever. Unless there is a good reason to use Alternate,
        // Other would likely be a better choice if nothing else fits. Alternate requires special logic
        // to understand what to do with it.
        [Description("Alternate")]
        Alternate,

        // for images which are clearly some kind of room scene; irrespective of shape.
        [Description("Scene")]
        Scene,

        // the item is designated as a sample piece
        [Description("Sample")]
        Sample,

        // try real hard not to use other.
        [Description("Other")]
        Other,
    }

    /// <summary>
    /// The main product record will have a collection of these ProductImage objects (0-N).
    /// </summary>
    /// <remarks>
    /// Will be persisted alongside the product in the p.Extension4 JSON data using the dictionary key named “ProductImages”.
    /// </remarks>
    [Serializable]
    public class ProductImage
    {
        /// <summary>
        /// Indicates that this is the default image to be used for thumbs and the first one displayed when viewing product detail page. 
        /// Any time only one image is needed, this is the choice.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// One of the predefined (well-known) shape keys. Used for matching. Technically, multiple images for a given product can use the same key, 
        /// but in processing, when a single image is needed when matching on the key, the first will be taken with a secondary ordering on DisplayOrder. 
        /// For example, alternate/scene keys could easily be more than one when we have rich information on a product.
        /// </summary>
        /// <remarks>
        /// from enum ImageVariantType
        /// </remarks>
        public string ImageVariant { get; set; }

        /// <summary>
        /// 0-rel indicates the display ordering for images; typically when shown on the product detail page as a set of available images. Duplicates okay. 
        /// As needed, the cart software will decide how to do its ordering, for example, it might put all the Scene images at the end, ordered as indicated. 
        /// It’s technically okay to leave all as zero.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// the globally (for the store, but preferable across stores since we’ll ultimately consolidate them) unique filename for this image (xxxxxx.jpg).
        /// Must be JPG! We’ll use this same name for all resized instances of this image (sizes go in different disk folders). Follow prescribed formulas
        /// for image names for fabric/wallpaper products (same rules as always). Only the default image is ultimately used for SEO; so the names of alternate/scene 
        /// images do not have to conform to a strict formula – might even be a GUID to keep things simple. Keep in mind that for the most part this name must be
        /// figured out WITHOUT the benefit of testing for duplicates – so a formula must be used that naturally guarantees uniqueness. Image names 
        /// found not to be unique should be changed by server logic to be a GUID as the fallback.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// the Url (technically, more of a Uri) of where to get the image when getting the image from a vendor’s website (or could be a folder on our site if needed, etc.).
        /// Include account/passwords as needed by some vendors. Can be HTTP, HTTPS or FTP. FILE might be supported if we find it necessary. 
        /// Should be to the highest resolution image. Null if unknown - which really is only for legacy transition.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Optional processing instructions, JSON. As needed, we’ll evolve a simple library of post-processing instructions to manipulate newly-ingested images. 
        /// Intended for super simple like Cropping surrounding white space, etc. Null if not needed.
        /// </summary>
        public string ProcessingInstructions { get; set; }

        public ProductImage()
        {
        }

        public ProductImage(string imageVariant, string url, string filename = null)
        {
            this.ImageVariant = imageVariant;
            this.SourceUrl = url;
            this.Filename = filename;
        }

        public ProductImage(ImageVariantType imageVariantType, string sourceUrl, string filename = null)
            : this(imageVariantType.ToString(), sourceUrl, filename)
        {

        }

    }
}
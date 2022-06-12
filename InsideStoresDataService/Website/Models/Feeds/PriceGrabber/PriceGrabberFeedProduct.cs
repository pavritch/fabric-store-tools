using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a PriceGrabber product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.PriceGrabber, TrackingCode: "pg",
        AnalyticsOrganicTrackingCode = "utm_source=pricegrabberproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=pricegrabberproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class PriceGrabberFeedProduct : FeedProduct
    {
        #region Feed Columns
        /// <summary>
        /// Retsku
        /// </summary>
        /// <remarks>
        /// The Retsku is your internal product ID number. It must be unique, and it must remain the same in every feed. 
        /// Retskus must not be reused even if you stop listing the product in your feed. Sequential 
        /// Retskus (1, 2, 3... ) are not recommended.
        /// </remarks>
        [CsvField("Retsku", IsRequired: true)]
        public string Retsku { get; set; }

        /// <summary>
        /// Product Title
        /// </summary>
        /// <remarks>
        /// Name of the product being sold. Titles should not be in ALL CAPITALS or contain any promotional or merchant-specific
        /// text (for example, "Free Shipping"). Please do not include the Manufacturer Name in the Product Titles (exceptions are
        /// Golf Clubs and Fragrances/Colognes). Product Titles should be descriptive in addition to stating what the product actually
        /// is, so users can easily find your product. Character limit for Product Titles is 100 characters.
        /// </remarks>
        [CsvField("Product Title", IsRequired: true)]
        public string ProductTitle { get; set; }

        /// <summary>
        /// Detailed Description
        /// </summary>
        /// <remarks>
        /// Thorough but concise description of the product. Detailed Descriptions should contain information relating only to the item. 
        /// They should not contain any promotional or merchant-specific text. No html, bulletpoints, extra line breaks, 
        /// or special characters permitted. Character limit for Detailed Descriptions is 1500 characters.
        /// </remarks>
        [CsvField("Detailed Description", IsRequired: true)]
        public string DetailedDescription { get; set; }

        /// <summary>
        /// Categorization
        /// </summary>
        /// <remarks>
        /// Category text indicating where the product belongs on site. It is best to use PriceGrabber's taxonomy format, but we are able
        /// to accept any category text, as long as it clearly states the category of the product. Category text should have sufficient detail;
        /// for example, "Indoor Living > Cat Supplies > Litter Boxes" as opposed to "Indoor Living". 
        /// See https://partner.pricegrabber.com/mss_main.php?sec=1&ccode=us for categories.
        /// </remarks>
        [CsvField("Categorization", IsRequired: true)]
        public string Categorization { get; set; }

        /// <summary>
        /// Product URL
        /// </summary>
        /// <remarks>
        /// Link leading directly to the product on your site. Must begin with "http://...". If you are selling a product that comes
        /// in multiple colors or sizes, it's recommended to have the link go to the product with the color and size options preselected.
        /// </remarks>
        [CsvField("Product URL", IsRequired: true)]
        public string ProductURL { get; set; }

        /// <summary>
        /// Primary Image URL
        /// </summary>
        /// <remarks>
        /// Link leading directly to the product's image. Must begin with "http://...". If you are selling a product that comes in multiple 
        /// colors or sizes, the image should show the specific color and size of the product being listed. Watermarked, popup images, and 
        /// inappropriate images are not accepted. Larger images are preferable to smaller or thumbnail images. If you are submitting multiple 
        /// images for a single product, the Primary Image URL should lead to the main image of the product.
        /// </remarks>
        [CsvField("Primary Image URL", IsRequired: true)]
        public string PrimaryImageURL { get; set; }

        /// <summary>
        /// Alternate Image URL 1
        /// </summary>
        /// <remarks>
        /// If you are submitting multiple images for a single product, the Alternate Image URLs should lead directly to images displaying
        /// different angles or views of the product. You can submit up to 8 Alternate Image URLs.
        /// </remarks>
        [CsvField("Alternate Image URL 1", IsRequired: false)]
        public string AlternateImageURL1 { get; set; }

        /// <summary>
        /// Alternate Image URL 2
        /// </summary>
        /// <remarks>
        /// If you are submitting multiple images for a single product, the Alternate Image URLs should lead directly to images displaying
        /// different angles or views of the product. You can submit up to 8 Alternate Image URLs.
        /// </remarks>
        [CsvField("Alternate Image URL 2", IsRequired: false)]
        public string AlternateImageURL2 { get; set; }

        /// <summary>
        /// Selling Price
        /// </summary>
        /// <remarks>
        /// Current selling price of the item. Please omit currency symbols. Prices should be provided in the appropriate currency
        /// (US Dollars for PriceGrabber.com, Pound Sterling for PriceGrabber.co.uk, Canadian Dollars for PriceGrabber.ca).
        /// </remarks>
        [CsvField("Selling Price", IsRequired: false)]
        public string SellingPrice { get; set; }

        /// <summary>
        /// Regular Price
        /// </summary>
        /// <remarks>
        /// Regular selling price of the item.
        /// </remarks>
        [CsvField("Regular Price", IsRequired: false)]
        public string RegularPrice { get; set; }

        /// <summary>
        /// Condition
        /// </summary>
        /// <remarks>
        /// Condition of the item being sold. 
        /// Accepted values are: New, Used, Like New, Refurbished, 3rd Party, Open Box, OEM, Downloadable 
        /// </remarks>
        [CsvField("Condition", IsRequired: false)]
        public string Condition { get; set; }

        /// <summary>
        /// Manufacturer Name
        /// </summary>
        /// <remarks>
        /// Manufacturer or brand name of the product.
        /// </remarks>
        [CsvField("Manufacturer Name", IsRequired: false)]
        public string ManufacturerName { get; set; }

        /// <summary>
        /// Manufacturer Part Number
        /// </summary>
        /// <remarks>
        /// Manufacturer-issued part number for the product. The MPN is case-insensitive. Duplicate MPNs should not be 
        /// submitted, as only the first product will be recognized. All other duplicates will be dropped.
        /// </remarks>
        [CsvField("Manufacturer Part Number", IsRequired: false)]
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// UPC/EAN	ISBN
        /// </summary>
        /// <remarks>
        /// UPC (US, Canada): Unique code of 12 numbers. EAN (Europe): Unique code of 13 numbers. Usually found adjacent
        /// to the bar code on the product packaging. Submission is highly recommended for all products.
        /// </remarks>
        [CsvField("UPC/EAN ISBN", IsRequired: false)]
        public string UPC_EAN_ISBN { get; set; }

        /// <summary>
        /// Availability
        /// </summary>
        /// <remarks>
        /// Indicates availability of the product.
        /// Accepted values:  Yes, No, Preorder
        /// </remarks>
        [CsvField("Availability", IsRequired: false)]
        public string Availability { get; set; }

        /// <summary>
        /// On Sale
        /// </summary>
        /// <remarks>
        /// Indicates whether the product is on sale.
        /// Accepted values:  Yes, No
        /// </remarks>
        [CsvField("On Sale", IsRequired: false)]
        public string OnSale { get; set; }

        /// <summary>
        /// Video URL
        /// </summary>
        /// <remarks>
        /// Link leading to a video review for the product.
        /// </remarks>
        [CsvField("VideoURL", IsRequired: false)]
        public string VideoURL { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        /// <remarks>
        /// Indicates the color of the item. If multiple colors, please separate the colors with commas.
        /// </remarks>
        [CsvField("Color", IsRequired: false)]
        public string Color { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        /// <remarks>
        /// Indicates the size of the item. If multiple sizes, please separate the sizes with commas.
        /// </remarks>
        [CsvField("Size", IsRequired: false)]
        public string Size { get; set; }

        /// <summary>
        /// Material
        /// </summary>
        /// <remarks>
        /// Indicates the material which is used to make the item. For example: Cotton, Leather, Suede, etc.
        /// </remarks>
        [CsvField("Material", IsRequired: false)]
        public string Material { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        /// <remarks>
        /// Indicates the gender for whom the item is intended.
        /// Accepted values: Men, Women, Boys, Girls, Baby Boys, Baby Girls, Unisex
        /// </remarks>
        [CsvField("Gender", IsRequired: false)]
        public string Gender { get; set; }

        /// <summary>
        /// Shipping Cost
        /// </summary>
        /// <remarks>
        /// Lowest shipping fee available for the product. For additional information about shipping.
        /// </remarks>
        [CsvField("Shipping Cost", IsRequired: false)]
        public string ShippingCost { get; set; }

        /// <summary>
        /// Weight
        /// </summary>
        /// <remarks>
        /// The product's shipping weight. For additional information about using weight when calculating shipping costs,
        /// see below. The unit for weight is pounds in the US, and kilograms for UK.
        /// </remarks>
        [CsvField("Weight", IsRequired: false)]
        public string Weight { get; set; }

        #endregion

        #region Local Methods

        public PriceGrabberFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.PriceGrabber, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            Retsku = StoreFeedProduct.SKU;
            ProductTitle = StoreFeedProduct.Title.Left(100);
            DetailedDescription = StoreFeedProduct.Description.Left(1500);

            var trackingInfo = FeedTrackingInfo;
            ProductURL = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            PrimaryImageURL = StoreFeedProduct.ImageUrl;
            AlternateImageURL1 = null;
            AlternateImageURL2 = null;
            SellingPrice = StoreFeedProduct.OurPrice.ToString("N2");
            RegularPrice = StoreFeedProduct.RetailPrice.ToString("N2");
            Condition = "New";
            ManufacturerName = StoreFeedProduct.Brand;
            ManufacturerPartNumber = StoreFeedProduct.ManufacturerPartNumber;
            UPC_EAN_ISBN = StoreFeedProduct.UPC;
            Availability = StoreFeedProduct.IsInStock ? "Yes" : "No";
            OnSale = null;
            VideoURL = null;
            Color = null;
            Size = null;
            Material = null;
            Gender = null;
            ShippingCost = null;
            Weight = null;

            Categorization = MakeCategorization();

        }

        protected abstract string MakeCategorization();

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a bing product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Bing, TrackingCode: "b",
        AnalyticsOrganicTrackingCode = "utm_source=bingproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode= "utm_source=bingproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class BingFeedProduct : FeedProduct
    {
        #region Feed Columns
        /// <summary>
        /// Unique ID - use InsideFabric SKU.
        /// </summary>
        /// <remarks>
        /// Text 1-199 characaters.
        /// Merchant Product ID. The ID you assign to a product covers the course of that product’s lifetime and should be consistent between feed updates.
        /// Any ID is acceptable as long as it is unique per item for the merchant. MPID may contain numerical and alphanumerical characters.
        /// </remarks>
        [CsvField("MerchantProductID", IsRequired: true)]
        public string MerchantProductID { get; set; }


        /// <summary>
        /// Product title.
        /// </summary>
        /// <remarks>
        /// Text 1-255 characters.
        /// The product name or in the case of a book, magazine, DVD, CD, game, etc., the title.
        /// Ensure titles are unique and descriptive enough to maximize your chance of display. No text fields should be wrapped in quotes.
        /// </remarks>
        [CsvField("Title", IsRequired: true)]
        public string Title { get; set; }


        /// <summary>
        /// Name of brand.
        /// </summary>
        /// Text 0-255 characters.
        /// <remarks>
        /// </remarks>
        [CsvField("Brand", IsRequired: false)]
        public string Brand { get; set; }


        /// <summary>
        /// Manufacturer assigned part number for this product.
        /// </summary>
        /// <remarks>
        /// Text 0-255 characters.
        /// </remarks>
        [CsvField("MPN", IsRequired: false)]
        public string MPN { get; set; }

        /// <summary>
        /// Manufacturer assigned UPC.
        /// </summary>
        /// <remarks>
        /// Text 0-12 digits.
        /// </remarks>
        [CsvField("UPC", IsRequired: false)]
        public string UPC { get; set; }

        /// <summary>
        /// Manufacturer assigned ISBN.
        /// </summary>
        /// <remarks>
        /// Text 0-13 digits.
        /// </remarks>
        [CsvField("ISBN", IsRequired: false)]
        public string ISBN { get; set; }


        /// <summary>
        /// Merchant SKU.
        /// </summary>
        /// <remarks>
        /// Text 0-255 characters.
        /// Used to refer to different versions of the same product, often to denote different sizes or colors.
        /// </remarks>
        [CsvField("MerchantSKU", IsRequired: false)]
        public string MerchantSKU { get; set; }



        /// <summary>
        /// Product website URL.
        /// </summary>
        /// <remarks>
        /// Text 0-2000 characters.
        /// Link to your website where a potential buyer can complete the purchase of your product.
        /// Please use fully qualified URL. URL should go directly to the product page on your site 
        /// where the customer may purchase the item listed. The URL should be HTTP or HTTPS only.
        /// No IP addresses.
        /// </remarks>
        [CsvField("ProductURL", IsRequired: true)]
        public string ProductUrl { get; set; }



        /// <summary>
        /// Price in US Dollars.
        /// </summary>
        /// <remarks>
        /// Decimal.
        /// The base price, excluding tax and shipping of the product.
        /// Please use two decimal places only, Bing may round up. Commas are acceptable. IE: 1,200.00,.
        /// USD is expected and assumed. Do not put dollar sign ($) or other symbols in field.
        /// Example: 15.00
        /// </remarks>
        [CsvField("price", IsRequired: true)]
        public string Price { get; set; }



        /// <summary>
        /// Stock Availability.
        /// </summary>
        /// <remarks>
        /// Examples:  In Stock, Out of Stock, Pre-Order, Back-Order
        /// </remarks>
        [CsvField("Availability", IsRequired: false)]
        public string Availability { get; set; }


        /// <summary>
        /// Product Description.
        /// </summary>
        /// <remarks>
        /// Please include a detailed description of your offer.
        /// No HTML coding. Ensure no descriptions are wrapped in quotes. Please do not include promotional text.
        /// </remarks>
        [CsvField("description", IsRequired: true)]
        public string Description { get; set; }


        /// <summary>
        /// Image URL.
        /// </summary>
        /// <remarks>
        /// Text 0-1000 characters.
        /// The image your customers see on the Bing Shopping site if your offer does not match to another item in our catalog.
        /// 220X220 pixels or larger. We may alter the size for optimal display on our website. No IP addresses, “No Image”, 
        /// watermarks, or free shipping text. If the underlying image changes, the URL must also change otherwise, old image 
        /// may continue to be used. The Bingbot must be allowed to crawl your site or your offers will not display.
        /// Supported types: BMP, GIF, EXIF, JPG, PNG and TIFF.
        /// </remarks>
        [CsvField("ImageURL", IsRequired: true)]
        public string ImageUrl { get; set; }


        /// <summary>
        /// Shipping.
        /// </summary>
        /// <remarks>
        /// Text 0-255 characters
        /// The lowest amount a customer would pay to have the product shipped to them. This attribute will override any 
        /// info set in the Shipping tab of your Bing shopping account.
        /// Each shipping attribute group must be separated with a comma and the four sub-attributes by colons such as: 
        /// Country:Region:Service:Price. The rate and three colons are required even for blank values. Do not enclose values in quotations.
        /// Example: US:::7.95
        /// </remarks>
        [CsvField("shipping", IsRequired: false)]
        public string Shipping { get; set; }


        /// <summary>
        /// Merchant Category.
        /// </summary>
        /// <remarks>
        /// Text 0-255 characters.
        /// Your category hierarchy for this product. The category hierarchy usually does not change over the lifetime of a product.
        /// Note: Please send only the primary category (most relevant, most descriptive) to which this offer should be assigned.
        /// Acceptable delimiters are pipe, comma, greater than. Examples are | , >
        /// </remarks>
        [CsvField("MerchantCategory", IsRequired: true)]
        public string MerchantCategory { get; set; }


        /// <summary>
        /// Shipping Weight.
        /// </summary>
        /// <remarks>
        /// Decimal. 0-N
        /// The weight of the product in pounds.
        /// </remarks>
        [CsvField("ShippingWeight", IsRequired: false)]
        public string ShippingWeight { get; set; }


        /// <summary>
        /// Condition.
        /// </summary>
        /// <remarks>
        /// New, Used, Collectable, Open Box, Refurbished, Remanufactured
        /// </remarks>
        [CsvField("Condition", IsRequired: false)]
        public string Condition { get; set; }


        /// <summary>
        /// Bing Category.
        /// </summary>
        /// <remarks>
        /// Bing Shopping Category.Bing’s list of categories to match your product to Bing’s standardized structure.
        /// Ideally, all your products in one Merchant Category align to one B_Category. Structure may change over time. 
        /// Acceptable delimiters are pipe, comma, greater than. IE: | , >
        /// Inside fabric should use: Home Furnishings
        /// </remarks>
        [CsvField("BingCategory", IsRequired: false)]
        public string BingCategory { get; set; }


        /// <summary>
        /// Sales tax. Specified in Merchant center.
        /// </summary>
        /// <remarks>
        /// Text 0-255 characters.
        /// The tax amount you will charge per each item. This attribute will override any info set in the Tax tab of your Bing shopping account.
        /// Example: US::8.25:y
        /// </remarks>
        [CsvField("Tax", IsRequired: false)]
        public string Tax { get; set; }

        #endregion

        #region Local Methods

        public BingFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Bing, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            MerchantProductID = StoreFeedProduct.ID;  // Fxxxxxx with ProductID (F for fabric)
            Title = StoreFeedProduct.Title;
            Brand = StoreFeedProduct.Brand;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            UPC = StoreFeedProduct.UPC;
            ISBN = null;
            MerchantSKU = StoreFeedProduct.SKU;

            var trackingInfo = FeedTrackingInfo;
            ProductUrl = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsOrganicTrackingCode);
            
            Price = StoreFeedProduct.OurPrice.ToString("N2");
            Availability = StoreFeedProduct.IsInStock ? "In Stock" : "Out of Stock";
            Description = StoreFeedProduct.Description;
            ImageUrl = StoreFeedProduct.ImageUrl;
            Shipping = null;
            MerchantCategory = StoreFeedProduct.CustomProductCategory;
            ShippingWeight = null;
            Condition = "New";
            Tax = null;

            // must be filled in by caller
            BingCategory = MakeBingCategory();
        }

        protected abstract string MakeBingCategory();

        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Nextag product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Nextag, TrackingCode: "n",
        AnalyticsOrganicTrackingCode = "utm_source=nextagproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=nextagproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class NextagFeedProduct : FeedProduct
    {
        /// <summary>
        /// Unique ID - not submitted to Nextag.
        /// </summary>
        /// <remarks>
        /// Only used internally.
        /// </remarks>
        public string ID { get; set; }

        #region Feed Columns

        /// <summary>
        /// Manufacturer
        /// </summary>
        /// <remarks>
        /// The Manufacturer is the brand of the product. Our systems will automatically append this
        /// directly to the front of the product title.
        /// </remarks>
        [CsvField("Manufacturer", IsRequired: true)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Manufacturer Part #
        /// </summary>
        /// <remarks>
        /// Combined with the manufacturer name, the Manufacturer Part Number is essential for adding your products
        /// to the Nextag catalog and ensuring quick upload time. Do not include the manufacturer name in this column.
        /// Acceptable: AN01-B
        /// Unacceptable: ACME AN01-B
        /// </remarks>
        [CsvField("Manufacturer Part #", IsRequired: true)]
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        /// <remarks>
        /// The name of your product. We will display product names up to 80 characters long.
        /// Do not include the manufacturer name in the product name. This will be appended automatically to the front of the name.
        /// Product names that contain the following will not be imported: the characters “$”, “!”, “@”,”%”,”^”,”&”,”~”,”*”,”|”
        /// </remarks>
        [CsvField("Product Name", IsRequired: true)]
        public string ProductName { get; set; }

        /// <summary>
        /// Product Description
        /// </summary>
        /// <remarks>
        /// A detailed description of your product. We can display descriptions up to 500 characters long, 
        /// however, only the first 150 characters are displayed on the initial search page.
        /// </remarks>
        [CsvField("Product Description", IsRequired: true)]
        public string ProductDescription { get; set; }

        /// <summary>
        /// Click-Out URL
        /// </summary>
        /// <remarks>
        /// The URL of the page on your site where this product is sold. When a user clicks on your 
        /// product listing at Nextag, they will be taken to this page.
        /// </remarks>
        [CsvField("Click-Out URL", IsRequired: true)]
        public string ClickOutURL { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        /// <remarks>
        /// The price of your product, without additional text.
        /// 49.99 or $49.99
        /// </remarks>
        [CsvField("Price", IsRequired: true)]
        public string Price { get; set; }

        /// <summary>
        /// Category: Other Format
        /// </summary>
        /// <remarks>
        /// Example: Large Appliances/Vacuum Cleaners
        /// </remarks>
        [CsvField("Category: Other Format", IsRequired: true)]
        public string CategoryOtherFormat { get; set; }

        /// <summary>
        /// Category: Nextag Numeric ID
        /// </summary>
        /// <remarks>
        /// The category your product belongs to. Nextag accepts merchant categories as long as they are 
        /// clearly understandable; however, we prefer it when merchants use our category names or numbers, 
        /// which can be found here: http://merchants.Nextag.com/serv/main/buyer/BulkCategoryCodes.jsp
        /// Example:  2700464 : More Categories / Home & Garden / Small Appliances / Vacuum Cleaners
        /// </remarks>
        [CsvField("Category: Nextag Numeric ID", IsRequired: true)]
        public string CategoryNextagNumericID { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        /// <remarks>
        /// The URL of a photo of your product. Link directly to the image, not the page where it is located.
        /// We accept images in JPEG or GIF format. Do not include company logos or names or promotional or other 
        /// identifying text. Images should be at least 100 x 100 pixels in size. Do not include any “image not found” 
        /// images. We do not accept image URLs that begin with https://.
        /// </remarks>
        [CsvField("Image URL", IsRequired: true)]
        public string ImageURL { get; set; }

        /// <summary>
        /// Ground Shipping
        /// </summary>
        /// <remarks>
        /// Flat-rate ground shipping price. Enter a value in dollars here to display the cost of ground shipping 
        /// alongside your product listings on our site. Entering “0” as a value indicates that the product has 
        /// Free Shipping. Any rows that are left blank will display shipping as “See Site” on the site.
        /// Merchants must select the “Fixed Rate, varies by product” option within the Manage Shipping Rules 
        /// section of the Seller Dashboard for this to take effect.
        /// </remarks>
        [CsvField("Ground Shipping", IsRequired: false)]
        public string GroundShipping { get; set; }

        /// <summary>
        /// Stock Status
        /// </summary>
        /// <remarks>
        /// States whether your product is in stock or not. This column has two possible values – In Stock and Out of Stock. 
        /// Note that marking your products as “Out Of Stock” does not remove them from our listings; it simply labels them “Out of Stock.”
        /// </remarks>
        [CsvField("Stock Status", IsRequired: true)]
        public string StockStatus { get; set; }

        /// <summary>
        /// Product Condition
        /// </summary>
        /// <remarks>
        /// The condition of each product you are selling.
        /// values: New, Open Box, OEM, Refurbished, Pre-Owned, Like New, Good, Very Good, Acceptable
        /// </remarks>
        [CsvField("Product Condition", IsRequired: true)]
        public string ProductCondition { get; set; }

        /// <summary>
        /// Promo Text - formerly Marketing Message
        /// </summary>
        /// <remarks>
        /// Promotional text message displayed on our site as part of your product listing. Offers an additional opportunity to promote 
        /// your products to Nextag users. Maximum length of 80 characters with the first 40 displayed in your listing and the
        /// remaining when moused over.
        /// 
        /// Note: there is an additional fee for promotional messaging. Be sure to enter your marketing message bid in the Seller Dashboard.
        /// 
        /// Acceptable: 30% off all strollers
        /// Acceptable: Free Shipping on orders over $50.00
        /// Acceptable: Free Shipping Plus Free Return Shipping
        /// </remarks>
        [CsvField("Promo Text", IsRequired: false)]
        public string PromoText { get; set; }

        /// <summary>
        /// Weight
        /// </summary>
        /// <remarks>
        /// Nextag can also calculate your shipping based on weight. If you use UPS, FedEx, DHL, or USPS to deliver your products,
        /// by entering your product weight here we can automatically display the correct cost of shipping to our users. Merchants
        /// must select within the Manage Shipping Rules section of the Seller Dashboard for this to take effect.
        /// </remarks>
        [CsvField("Weight", IsRequired: false)]
        public string Weight { get; set; }

        /// <summary>
        /// Cost-per-Click
        /// </summary>
        /// <remarks>
        /// The Cost-per-Clicks or CPCs that you want to pay Nextag when a buyer clicks from our site to yours. This is optional
        /// and should only be used if you want to do a Fixed Bid on individual products using your Product File. By using this 
        /// column, you will be unable to bid by Product on the Seller Dashboard. Products with CPCs below our minimum 
        /// rates will not be imported.
        /// </remarks>
        [CsvField("Cost-per-Click", IsRequired: false)]
        public string CostPerClick { get; set; }

        /// <summary>
        /// UPC
        /// </summary>
        /// <remarks>
        /// The UPC of the product you are selling. This is a 12 digit number.
        /// </remarks>
        [CsvField("UPC", IsRequired: false)]
        public string UPC { get; set; }

        /// <summary>
        /// Distributor ID
        /// </summary>
        /// <remarks>
        /// The distributor IDs (i.e., TechData or Ingram IDs) of your products.
        /// </remarks>
        [CsvField("Distributor ID", IsRequired: false)]
        public string DistributorID { get; set; }

        /// <summary>
        /// MUZE ID
        /// </summary>
        /// <remarks>
        /// The MUZE IDs of any music, video or video game products you are selling. This field is required if you
        /// are selling music, video or video game products.
        /// </remarks>
        [CsvField("MUZE ID", IsRequired: false)]
        public string MUZEID { get; set; }

        /// <summary>
        /// ISBN
        /// </summary>
        /// <remarks>
        /// The ISBNs of any book products you are selling. This field is required for book products.
        /// </remarks>
        [CsvField("ISBN", IsRequired: false)]
        public string ISBN { get; set; }

        #endregion

        #region Local Methods

        public NextagFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Nextag, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            // Nextag does not itself support a unique ID - so we use this one internally
            ID = StoreFeedProduct.ID;

            Manufacturer = StoreFeedProduct.Brand;
            ManufacturerPartNumber = StoreFeedProduct.ManufacturerPartNumber;
            ProductName = MakeProductName(StoreFeedProduct);
            ProductDescription = StoreFeedProduct.Description;

            var trackingInfo = FeedTrackingInfo;
            ClickOutURL = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            Price = StoreFeedProduct.OurPrice.ToString("N2"); ;
            CategoryOtherFormat = StoreFeedProduct.CustomProductCategory.Replace(">", "/");
            ImageURL = StoreFeedProduct.ImageUrl;
            GroundShipping = null;
            StockStatus = StoreFeedProduct.IsInStock ? "In Stock" : "Out of Stock";
            ProductCondition = "New";
            PromoText = null;
            Weight = null;
            CostPerClick = null;
            UPC = StoreFeedProduct.UPC;
            DistributorID = null;
            MUZEID = null;
            ISBN = null;

            CategoryNextagNumericID = MakeCategoryNextagNumericID();
        }

        protected abstract string MakeCategoryNextagNumericID();

        /// <summary>
        /// Nextag does not like manufacturer names to be included.
        /// </summary>
        /// <remarks>
        /// Remove the manufacturer name which is usually included in the form
        /// "by Kravet Fabrics" and so forth.
        /// </remarks>
        /// <returns></returns>
        protected virtual string MakeProductName(IStoreFeedProduct FeedProduct)
        {
            return FeedProduct.Title;
        }

        #endregion
    }
}
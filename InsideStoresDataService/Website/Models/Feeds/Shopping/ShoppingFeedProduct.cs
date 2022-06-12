using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Shopping product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Shopping, TrackingCode: "s",
        AnalyticsOrganicTrackingCode = "utm_source=shoppingproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode= "utm_source=shoppingproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class ShoppingFeedProduct : FeedProduct
    {
        /// <summary>
        /// Used internally. Not part of output file.
        /// </summary>
        public string ID { get; set; }

        #region Feed Columns

        /// <summary>
        /// MPN
        /// </summary>
        /// <remarks>
        /// Shopping.com maintains a database of Manufacturer Part Numbers and associated brands and will match the
        /// MPNs in your data feed against it. MPNs can be obtained from the manufacturer.
        /// </remarks>
        [CsvField("MPN", IsRequired: true)]
        public string MPN { get; set; }

        /// <summary>
        /// Manufacturer Name
        /// </summary>
        /// <remarks>
        /// Brand, manufacturer, or publisher for the product.
        /// </remarks>
        [CsvField("Manufacturer Name", IsRequired: true)]
        public string ManufacturerName { get; set; }

        /// <summary>
        /// UPC
        /// </summary>
        /// <remarks>
        /// A Universal Product Code is necessary when you do not have the MPN for categories that require MPNs. 
        /// It is a 12 digit number used to identify a product. UPC numbers are found with bar codes on product packaging.
        /// </remarks>
        [CsvField("UPC", IsRequired: false)]
        public string UPC { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        /// <remarks>
        /// Product Names must be clear and descriptive of the product being sold. Make sure shoppers can identify what type of
        /// product that is being sold by reading the product name.
        /// </remarks>
        [CsvField("Product Name", IsRequired: true)]
        public string ProductName { get; set; }

        /// <summary>
        /// Product Description
        /// </summary>
        /// <remarks>
        /// Product descriptions should contain details elaborating on the Product Names. Unnecessary information within product
        /// names may lower the relevancy ranking of your products. Maximum length: 1024 characters. Plain text only.
        /// </remarks>
        [CsvField("Product Description", IsRequired: true)]
        public string ProductDescription { get; set; }

        /// <summary>
        /// Product Price  
        /// </summary>
        /// <remarks>
        /// Current available price.  4.23
        /// </remarks>
        [CsvField("Product Price", IsRequired: true)]
        public string ProductPrice { get; set; }

        /// <summary>
        /// Product URL
        /// </summary>
        /// <remarks>
        /// URL on your website containing the product detail and buy button for the applicable product.
        /// </remarks>
        [CsvField("Product URL", IsRequired: true)]
        public string ProductURL { get; set; }

        /// <summary>
        /// Image URL 
        /// </summary>
        /// <remarks>
        /// URL to the item’s largest image. We will download this image once and host it on our servers.
        /// A high-quality image will help shoppers browse and will attract them to your products. To find the
        /// image url, right click on the image and select “Properties,” copy the selection next to Address (URL).
        /// </remarks>
        [CsvField("Image URL", IsRequired: true)]
        public string ImageURL { get; set; }

        /// <summary>
        /// Shopping.com Categorization
        /// </summary>
        /// <remarks>
        /// The category & subcategory where the product should appear within the Shopping.com website. You may use the 
        /// categories used on your own website, and our technicians will do their best to remap them to the correct
        /// product area on Shopping.com. The best matching results are when you use Shopping.com categories found here:
        /// https://merchants.shopping.com/Taxonomy.html
        /// </remarks>
        [CsvField("Shopping.com Categorization", IsRequired: true)]
        public string ShoppingCategorization { get; set; }

        /// <summary>
        /// Stock Availability
        /// </summary>
        /// <remarks>
        /// Please denote Y or N to show if the product is in stock. If you do not put any information in this 
        /// column, your items will appear on site as “stock unknown.”
        /// </remarks>
        [CsvField("Stock Availability", IsRequired: false)]
        public string StockAvailability { get; set; }

        /// <summary>
        /// Stock Description
        /// </summary>
        /// <remarks>
        /// (21 characters max.) This will be displayed beneath the “In Stock” or “Out of Stock” 
        /// indicator on Shopping.com. Examples: “Backordered 2-3 Weeks,” “Ships 2-3 Days.”
        /// </remarks>
        [CsvField("Stock Description", IsRequired: false)]
        public string StockDescription { get; set; }

        /// <summary>
        ///  Ground Shipping 
        /// </summary>
        /// <remarks>
        /// Shipping cost for this item. Use 0 to denote free shipping.
        /// </remarks>
        [CsvField("Ground Shipping ", IsRequired: false)]
        public string GroundShipping { get; set; }

        /// <summary>
        ///  Weight 
        /// </summary>
        /// <remarks>
        /// Weight of the item in pounds. Include this field if you wish us to calculate your shipping cost
        /// based on standard UPS or FedEx rates by zip code.
        /// </remarks>
        [CsvField("Weight", IsRequired: false)]
        public string Weight { get; set; }

        /// <summary>
        /// Zip Code
        /// </summary>
        /// <remarks>
        /// Include the zip code here for UPS shipping, if you ship from multiple locations. If you ship from one
        /// location, you can put the zip code information into the Merchant Account Center, 
        /// under the Products tab->Shipping Info link.
        /// </remarks>
        [CsvField("Zip Code", IsRequired: false)]
        public string ZipCode { get; set; }

        /// <summary>
        /// Condition
        /// </summary>
        /// <remarks>
        /// New
        /// </remarks>
        [CsvField("Condition", IsRequired: false)]
        public string Condition { get; set; }

        #endregion

        #region Local Methods

        public ShoppingFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Shopping, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            ID = StoreFeedProduct.ID;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            ManufacturerName = StoreFeedProduct.Brand;
            UPC = StoreFeedProduct.UPC;
            ProductName = StoreFeedProduct.Title;
            ProductDescription = StoreFeedProduct.Description;
            ProductPrice = StoreFeedProduct.OurPrice.ToString("N2");

            var trackingInfo = FeedTrackingInfo;
            ProductURL = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            ImageURL = StoreFeedProduct.ImageUrl;
            StockAvailability = StoreFeedProduct.IsInStock ? "Y" : "N";
            StockDescription = "Usually Ships 1-2 Days";
            GroundShipping = 0.ToString("N2"); ;
            Weight = null;
            ZipCode = null;
            Condition = "New";

            ShoppingCategorization = MakeShoppingCategorization();
        }

        protected abstract string MakeShoppingCategorization();

        #endregion
    }
}
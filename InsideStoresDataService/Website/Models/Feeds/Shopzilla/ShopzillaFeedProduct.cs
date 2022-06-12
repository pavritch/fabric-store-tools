using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Shopzilla product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Shopzilla, TrackingCode: "sz",
        AnalyticsOrganicTrackingCode = "utm_source=shopzillaproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=shopzillaproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class ShopzillaFeedProduct : FeedProduct
    {
        /// <summary>
        /// Unique ID - not submitted to Shopzilla.
        /// </summary>
        /// <remarks>
        /// Only used internally.
        /// </remarks>
        public string ID { get; set; }

        #region Feed Columns

        /// <summary>
        /// Category ID
        /// </summary>
        /// <remarks>
        /// Please enter the appropriate Shopzilla Category ID number of the category in which you 
        /// would like your product to be listed. Shopzilla’s category IDs can be found
        /// at: http://merchant.shopzilla.com/oa/general/taxonomy.xpml
        /// Miscellaneous Home Decor is: 13,020,210
        /// </remarks>
        [CsvField("Category ID", IsRequired: true)]
        public string CategoryID { get; set; }

        /// <summary>
        /// Manufacturer
        /// </summary>
        /// <remarks>
        /// Use the Manufacturer field to designate the maker of the product.
        /// </remarks>
        [CsvField("Manufacturer", IsRequired: true)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        /// <remarks>
        /// This is the name of the product, and should be true to the actual product. Titles should be 
        /// as clear and concise as possible. Please use all available information when stating what the 
        /// product is. This would include gender, material, color, model number, and brand name.
        /// The maximum length for this field is 100 characters.
        /// Do not use all capital letters.
        /// Do not use HTML code or control characters.
        /// </remarks>
        [CsvField("Title", IsRequired: true)]
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        /// <remarks>
        /// The product description should accurately represent the product listed, provide additional product
        /// information and enhance relevancy.
        /// The maximum length for this field is 1,000 characters.
        /// Only the first 255 characters are displayed on the site and factor into relevancy.
        /// Do not use HTML code or control characters.
        /// Do not use promotional language such as “free shipping” or “sale item”.
        /// </remarks>
        [CsvField("Description", IsRequired: true)]
        public string Description { get; set; }

        /// <summary>
        /// Product URL
        /// </summary>
        /// <remarks>
        /// Enter the URL (link) to the page you would like consumers to go to when they click on your product
        /// listing. For best results, please enter the product URLs to your specific product pages.
        /// URLs must begin with http://.
        /// </remarks>
        [CsvField("Product URL", IsRequired: true)]
        public string ProductURL { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        /// <remarks>
        /// Please provide us with the URL of the product image on your site. The URL must link to the actual image
        /// and not to an html page. We will copy this image to our servers for quicker serving of product images.
        /// Image URLs must begin with http://
        /// The URL must end in .jpg or .gif.
        /// Image size should be at least 200x200 pixels and no greater than 1000x1000.
        /// We are unable to download images from secure locations (https://)
        /// </remarks>
        [CsvField("Image URL", IsRequired: true)]
        public string ImageURL { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        /// <remarks>
        /// SKU stands for Stock Keeping Unit and is a unique designator for each listing in your feed. Each
        /// product in your feed must have a different SKU listed. For instance, if you have 100 offers in your
        /// feed, you should have 100 different SKUs.
        /// Since we use the SKUs provided to uniquely identify your products within our database, please keep the
        /// same SKUs for any updates to your feed.
        /// </remarks>
        [CsvField("SKU", IsRequired: true)]
        public string SKU { get; set; }

        /// <summary>
        /// Availability
        /// </summary>
        /// <remarks>
        /// This field lets your consumers know if the product is currently available on your website. Since our site
        /// prefers to show “In Stock” items over “Out of Stock” or “See Site” items, please be sure to note this in the
        /// Availability column. If no information is provided in your Availability column, our system will default to
        /// “See Site for Availability”.
        /// In Stock | Out of Stock | Back-Order | Limited Qty
        /// </remarks>
        [CsvField("Availability", IsRequired: true)]
        public string Availability { get; set; }

        /// <summary>
        /// Condition
        /// </summary>
        /// <remarks>
        /// Use this field to designate the state of the product. Providing this information will allow Shopzilla to
        /// display your product’s condition before consumers click through to your site. If no information is provided
        /// in your Condition column, our system will default to “new”.
        /// New | Used | Open Box
        /// </remarks>
        [CsvField("Condition", IsRequired: true)]
        public string Condition { get; set; }

        /// <summary>
        /// Ship Weight
        /// </summary>
        /// <remarks>
        /// This field is used to designate the weight of your product (in pounds). This field is required if your store’s
        /// shipping costs are determined based on an item’s weight. Shopzilla will use this information in
        /// conjunction with the information you provide in our Shipping Tool, located in the Account Management
        /// section of the Business Services website.
        /// </remarks>
        [CsvField("Ship Weight", IsRequired: false)]
        public string ShipWeight { get; set; }

        /// <summary>
        /// Ship Cost
        /// </summary>
        /// <remarks>
        /// This field is used to designate a flat shipping cost for the product, if desired. This field is not necessary if
        /// you plan to use the Shopzilla Shipping Tool to specify ship costs. This field should contain the lowest amount a
        /// buyer would be required to pay to ship that product, and that product only, within the United States.
        /// To designate free shipping, please use 0.00.
        /// Shipping cost included in the feed file will override any rules set using the Shipping Tool on the Business Services website.
        /// </remarks>
        [CsvField("Ship Cost", IsRequired: false)]
        public string ShipCost { get; set; }

        /// <summary>
        /// Bid
        /// </summary>
        /// <remarks>
        /// This is an optional field and recommended only for those merchants programmatically setting their product bids. Shopzilla 
        /// offers product-level bidding through our online Bidding Tool. Bids placed in the feed are not shown in the online bidding 
        /// tool. To reduce confusion, we strongly recommend using the online bidding tool for setting bids (Note: You will have access 
        /// to the Bidding Tool once your products are live on Shopzilla.)
        /// </remarks>
        [CsvField("Bid", IsRequired: false)]
        public string Bid { get; set; }

        /// <summary>
        /// Promotional Code
        /// </summary>
        /// <remarks>
        /// This field is used to add promotional text next to your product. You may specify up to two code numbers.
        /// If you are using two codes, please be sure to have a space between each number. For instance, 1 23 or 
        /// 24 5 are acceptable entries.
        /// Note: Depending on the categorization of your products and current display rules, promo text may not
        /// appear next to your listings.
        /// </remarks>
        [CsvField("Promotional Code", IsRequired: false)]
        public string PromotionalCode { get; set; }

        /// <summary>
        /// UPC
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [CsvField("UPC", IsRequired: false)]
        public string UPC { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        /// <remarks>
        /// The product price should be the cost of the product before tax or shipping. Do not include any rebates,
        /// coupons, or bulk discounts. Prices should be whole numbers with a maximum of two decimal places (for 
        /// example: 99.99). Price fields are numeric; no text is permitted. Do not include “$”.
        /// </remarks>
        [CsvField("Price", IsRequired: true)]
        public string Price { get; set; }

        #endregion

        #region Local Methods

        public ShopzillaFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Shopzilla, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            // Shopzilla does not itself support a unique ID - so we use this one internally
            ID = StoreFeedProduct.ID;

            Manufacturer = StoreFeedProduct.Brand;
            Title = StoreFeedProduct.Title.Left(100);
            Description = StoreFeedProduct.Description.Left(1000);

            var trackingInfo = FeedTrackingInfo;
            ProductURL = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            ImageURL = StoreFeedProduct.ImageUrl;
            SKU = StoreFeedProduct.SKU;
            Availability = StoreFeedProduct.IsInStock ? "In Stock" : "Out of Stock";
            Condition = "New";
            ShipWeight = null;
            ShipCost = null;
            Bid = null;
            PromotionalCode = null;
            UPC = StoreFeedProduct.UPC;
            Price = StoreFeedProduct.OurPrice.ToString("N2");

            CategoryID = MakeCategoryID();

        }

        protected abstract string MakeCategoryID();

        #endregion
    }
}
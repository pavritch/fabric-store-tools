using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Reflection;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Amazon product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Amazon, TrackingCode:"a",
        AnalyticsOrganicTrackingCode = "utm_source=amazonproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=amazonproduct&utm_medium=paid&utm_campaign=products")]

   public abstract class AmazonFeedProduct : FeedProduct
    {
        #region Feed Columns
        // optional fields not included below:  tax, pattern, material

        // Two of these three items are needed:   Brand, UPC, MPN

        /// <summary>
        /// This is the name of your item which is required. We recommend you include characteristics 
        /// such as color or brand in the title which differentiates the item from other products.
        /// </summary>
        /// <remarks>
        /// Amazon displays 64 characters on results page, but about 85 on shopping list page.
        /// Roma Cotton Rich Bootcut Jeans with Belt - Size 8 Tall
        /// Roma Cotton Rich Bootcut Jeans with Belt - Size 8 Standard
        /// Merlin: Series 3 - Volume 2 - 3 DVD Box set
        /// </remarks>
        [CsvField("title", IsRequired: true)]
        public string Title { get; set; }

        /// <summary>
        /// The user is sent to this URL when your item is clicked on Product Search. We also refer to this 
        /// as the landing page. It must point to a page showing the exact item the user was looking at. You can 
        /// use tracking URLs to distinguish users coming from Product Search.
        /// </summary>
        /// <remarks>
        /// Read our policies carefully. All your URLs must link directly to webpages about your products without pop ups.
        /// We don't allow landing pages requiring sign ups, passwords, or direct links to files/email addresses.
        /// Any symbols used must be replaced by URL encoded entities (e.g. comma = %2C).
        /// </remarks>
        [CsvField("link", IsRequired: true)]
        public string Link { get; set; }

        /// <summary>
        /// Include only information relevant to the item, but be comprehensive since we use this text to find your item. 
        /// We recommend you submit around 500 to 1000 characters, but you can submit up to 10000 characters.
        /// Make sure to follow our Editorial guidelines closely. For example, do not include any promotional text such 
        /// as "Free shipping", do not use BLOCK CAPITALS, and do not include a description of your store.
        /// </summary>
        /// <remarks>
        /// Amazon displays about 145 characters on results page, but about 185 on shopping list page.
        /// Attractively styled and boasting stunning picture quality, the LG Flatron M2262D 22" Full HD LCD TV is an excellent television/monitor. The LG Flatron M2262D 22" Full HD LCD TV 
        /// Comes with the belt. A smart pair of bootcut jeans in stretch cotton. The flower print buckle belt makes it extra stylish.
        /// </remarks>
        [CsvField("description", IsRequired: true)]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The identifier for each item has to be unique within your account, and cannot be re-used between feeds. 
        /// If you have multiple feeds, ids of items within different feeds must still be unique. You can use any 
        /// sequence of letters and digits for the item id.
        /// Once an item is submitted, the id must not change when you update your data feed or be used for a different product at a later point in time.
        /// </remarks>
        [CsvField("id", IsRequired: true)]
        public string ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// used, new, refurbished
        /// </remarks>
        [CsvField("condition", IsRequired: true)]
        public string Condition { get; set; }

        /// <summary>
        /// The price must include a currency.
        /// </summary>
        /// <remarks>
        /// The price of the item has to be the most prominent price on the landing page.
        /// For the US, don't include tax in the price.
        /// 159 USD
        /// </remarks>
        [CsvField("price", IsRequired: true)]
        public string Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// in stock --- will be in transit within 3 business days
        /// out of stock
        /// available for order
        /// preorder
        /// </remarks>
        [CsvField("availability", IsRequired: true)]
        public string Availability { get; set; }

        /// <summary>
        /// This is the URL of an associated image for a product. Submit full-size images for your 
        /// products and do not submit thumbnail versions of the images. For all apparel products,
        /// we require images of at least 250 x 250 pixels and recommend images of at least 400 x 400 pixels.
        /// </summary>
        /// <remarks>
        /// Prefer at least 400x400.
        /// If you have no image for your item, you cannot submit the item.
        /// Do not submit a placeholder such as "No image", logo of the brand or logo of your store.
        /// Images of products must not contain logos or other promotions within the image.
        /// </remarks>
        [CsvField("image link", IsRequired: true)]
        public string ImageLink { get; set; }

        /// <summary>
        /// Specified in Amazon merchant center.
        /// </summary>
        /// <remarks>
        /// US::Standard Free Shipping:0 USD
        /// US::Standard Rate:4.95 USD,US::Next Day:8.50 USD
        /// US::Standard:14.95 USD
        /// </remarks>
        [CsvField("shipping")]
        public string Shipping { get; set; }

        [CsvField("shipping weight")]
        public string ShippingWeight { get; set; }

        /// <summary>
        /// Specified in Merchant center.
        /// </summary>
        /// <remarks>
        /// US:CA:8.25:n   - for CA at 8.25% and do not charge tax on shipping.
        /// </remarks>
        [CsvField("tax")]
        public string Tax { get; set; }

        /// <summary>
        /// Universal Product Code (UPC).
        /// </summary>
        /// <remarks>
        /// A unique numerical identifier for commercial products that's usually associated with a barcode printed on retail merchandise.
        /// In the US - this would be the UPC code if one is known. Not required.
        /// </remarks>
        [CsvField("gtin")]
        public string GTIN { get; set; }

        /// <summary>
        /// Name of brand.
        /// </summary>
        [CsvField("brand", IsRequired: true)]
        public string Brand { get; set; }

        /// <summary>
        /// The number which uniquely identifies the product to its manufacturer.
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("mpn", IsRequired: true)]
        public string MPN { get; set; }

        /// <summary>
        /// Amazon product category.
        /// </summary>
        /// <remarks>
        /// Must be an exact phrase from Amazon's published taxonomy.
        /// Apparel & Accessories > Clothing > Jeans
        /// Media > DVDs & Movies > Television Shows
        /// http://support.Amazon.com/merchants/bin/answer.py?hl=en&answer=160081&topic=30064&ctx=topic
        /// </remarks>
        [CsvField("Amazon product category", IsRequired: true)]
        public string AmazonProductCategory { get; set; }

        /// <summary>
        /// The product_type attribute: For this attribute, you can use either one of the categories 
        /// defined in the Amazon product taxonomy, or your own category names.
        /// </summary>
        ///<remarks>
        ///Can be any private word, phrase, taxonomy. Max 10. Can include own breadcrumbs or from Amazon's taxonomy.
        ///</remarks>
        [CsvField("product type", IsRequired: true)]
        public string ProductType { get; set; }

        [CsvField("additional image link")]
        public string AdditionalImageLink { get; set; }

        /// <summary>
        /// Primary colors for product.
        /// </summary>
        /// <remarks>
        /// When multiple colors, use slashes (Green / Black).
        /// </remarks>
        [CsvField("color")]
        public string Color { get; set; }

        [CsvField("size")]
        public string Size { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Female, Male
        /// </remarks>
        [CsvField("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Adult
        /// </remarks>
        [CsvField("age group")]
        public string AgeGroup { get; set; }

        /// <summary>
        /// All items that are color/material/pattern/size variants of the same product must have the same item group id. 
        /// If you have a “Parent SKU” that is shared by all variants of a product, you can provide that as the value for 'item group id'.
        /// </summary>
        /// <remarks>
        /// The Item group id attribute is different from the ID attribute. An Item group ID attribute will have common values for a
        /// group of variants whereas the ‘ID’ attribute should have unique values across a group of variants and for all other items, as well.
        /// If you send us an item group id attribute, we will automatically look for variant attributes. Conversely, if you did send us
        /// Item group id, you should ensure you send us at least one variant attribute.
        /// </remarks>
        [CsvField("item group id")]
        public string ItemGroupID { get; set; }

        [CsvField("sale price")]
        public string SalePrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 2011-03-01T16:00-08:00/2011-03-03T16:00-08:00
        /// </remarks>
        [CsvField("sale price effective date")]
        public string SalePriceEffectiveDate { get; set; }

        #endregion

        #region Local Methods

        public AmazonFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Amazon, feedProduct)
        {

        }

        protected override void Populate()
        {
            Title = StoreFeedProduct.Title;

            var trackingInfo = FeedTrackingInfo;
            Link = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            Description = StoreFeedProduct.Description;
            ID = StoreFeedProduct.SKU;
            Condition = "new";
            Price = string.Format("{0:N2} USD", StoreFeedProduct.OurPrice); // 11.99 USD
            Availability = StoreFeedProduct.IsInStock ? "in stock" : "out of stock";
            ImageLink = StoreFeedProduct.ImageUrl;
            ShippingWeight = null;
            Tax = "US:CA:8.00:n";
            GTIN = StoreFeedProduct.UPC;
            Brand = StoreFeedProduct.Brand;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            ProductType = StoreFeedProduct.CustomProductCategory;
            AdditionalImageLink = null;
            Color = null;
            Size = StoreFeedProduct.Size;
            Gender = null;
            AgeGroup = null;
            ItemGroupID = MakeItemGroupID();
            SalePrice = null;
            SalePriceEffectiveDate = null;

            // must be filled in by caller
            AmazonProductCategory = MakeAmazonProductCategory(); 
            Shipping = MakeShipping();

        }

        protected abstract string MakeShipping();
        protected abstract string MakeAmazonProductCategory();

        protected virtual string MakeItemGroupID()
        {
            return null;
        }


        #endregion
    }
}
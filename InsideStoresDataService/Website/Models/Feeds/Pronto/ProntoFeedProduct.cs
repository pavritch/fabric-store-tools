using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Pronto product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Pronto, TrackingCode: "p",
        AnalyticsOrganicTrackingCode = "utm_source=prontoproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=prontoproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class ProntoFeedProduct : FeedProduct
    {
        #region Feed Columns

        // all 42 fields must be present and in this exact order

        /// <summary>
        /// Title
        /// </summary>
        /// <remarks>
        /// This is the title of the product for sale. Titles shall be concise, but feature-rich and well-formed. When
        /// available, they should include both the product name and model. Product titles should not include a
        /// description of the product (this should be entered in the Description field). The optimal product title for
        /// categorization and search performance in the Pronto system follows the below format and order as much
        /// as possible (see the accompanying Pronto Data Feed Optimization Strategies document for additional
        /// information). The character limit for this field is 350 characters. After 350 characters, all product 
        /// titles will be trimmed.
        /// </remarks>
        [CsvField("Title", IsRequired: true)]
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        /// <remarks>
        /// This should be a complete, detailed, and accurate description of the product from your website. The text
        /// should be basic facts about the product, and should not contain any extraneous promotional information.
        /// The maximum number of characters allowed is 2048.
        /// </remarks>
        [CsvField("Description", IsRequired: true)]
        public string Description { get; set; }

        /// <summary>
        /// Sales Price
        /// </summary>
        /// <remarks>
        /// This should represent the current sales price for which a consumer may purchase the product at that
        /// time. This entry should be numerical only (no text), should not include any symbols (including $), and
        /// should be quoted in US dollars only. It should not include any rebates, coupons, or other promotional
        /// discounts that may be available. Allowed data is any numerical value between .01 – 100,000.00.
        /// Example:  6.00
        /// </remarks>
        [CsvField("SalePrice", IsRequired: true)]
        public string SalePrice { get; set; }

        /// <summary>
        /// Condition
        /// </summary>
        /// <remarks>
        /// This should reflect the condition of the product. New, Used or Refurb are the acceptable values.
        /// </remarks>
        [CsvField("Condition", IsRequired: true)]
        public string Condition { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        /// <remarks>
        /// This should be the most granular category information available on the product, such as the category and
        /// sub-category that the merchant may already use themselves (like site breadcrumbs). The optimal
        /// category information for most accurate classification in the Pronto system follows the below format as
        /// much as possible, and contains the specific head noun for the product (in the below example, digital
        /// cameras is the head noun that is most critical to appear; see the accompanying Pronto Data Feed
        /// Optimization Strategies document for additional information): Photo/Video > Cameras & Accessories > Digital Cameras
        /// Pronto will use this information to assist in the categorization of each product listing within Pronto’s
        /// categorization schema. The maximum number of characters allowed is 2048.
        /// </remarks>
        [CsvField("Category", IsRequired: true)]
        public string Category { get; set; }


        /// <summary>
        /// URL
        /// </summary>
        /// <remarks>
        /// This should be the URL to a product detail page for which a user can affect a purchase. All URLs should
        /// be fully qualified URLs (including the http://www.). These URLs can include any built-in tracking tags as
        /// long as they do not interfere with the URL resolving directly to the product page to which it is associated.
        /// The maximum number of characters allowed is 2048.
        /// Example: http://www.mystore.com/product1234.html
        /// </remarks>
        [CsvField("URL", IsRequired: true)]
        public string URL { get; set; }

        /// <summary>
        /// Short Title
        /// </summary>
        /// <remarks>
        /// A user friendly short title for display purposes up to 128 characters.
        /// Example: Black Cocktail Dress
        /// </remarks>
        [CsvField("ShortTitle", IsRequired: false)]
        public string ShortTitle { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        /// <remarks>
        /// This text may be up to 128 characters and should represent the item’s color. If the product is multiple
        /// colors, then please provide a hyphen separated list of the colors.
        /// </remarks>
        [CsvField("Color", IsRequired: false)]
        public string Color { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        /// <remarks>
        /// This text may be up to 128 characters long and should represent the size of the item, where applicable
        /// (shoes, clothing etc).
        /// Example: XL or 4
        /// </remarks>
        [CsvField("Size", IsRequired: false)]
        public string Size { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        /// <remarks>
        /// This text should represent additional attributes about the product, for instance if a shoe is leather or
        /// suede, or if the fabric of a shirt is linen or cotton. Please provide in name value pairs (material=leather;
        /// sleevelength=short). This field may be up to 2048 characters long.
        /// Examples:
        /// Material=leather
        /// length=long
        /// 
        /// </remarks>
        [CsvField("Attributes", IsRequired: false)]
        public string Attributes { get; set; }

        /// <summary>
        /// Keywords
        /// </summary>
        /// <remarks>
        /// These should be search engine type keywords that Pronto will use to help classify your product listings.
        /// They should be entered in all lowercase letters, with commas separating the different keyword phrases.
        /// The maximum number of characters allowed is 2048.
        /// Examples:  digital camera, men's waterproof boots
        /// </remarks>
        [CsvField("Keywords", IsRequired: false)]
        public string Keywords { get; set; }

        /// <summary>
        /// Brand:
        /// </summary>
        /// <remarks>
        /// This should be the brand name of the product. The character limit for this field is 128 characters. The
        /// maximum number of characters allowed is 128. Example: Apple
        /// </remarks>
        [CsvField("Brand", IsRequired: true)]
        public string Brand { get; set; }

        /// <summary>
        /// Manufacturer
        /// </summary>
        /// <remarks>
        /// This should represent the name of the product manufacturer to distinguish the product from the retailer.
        /// This field may be up to 128 characters long. Example: Sony
        /// </remarks>
        [CsvField("Manufacturer", IsRequired: false)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Artist or Author
        /// </summary>
        /// <remarks>
        /// This should be the Artist or Author’s full name, provided for books or music only.
        /// </remarks>
        [CsvField("ArtistAuthor", IsRequired: false)]
        public string ArtistAuthor { get; set; }

        /// <summary>
        /// Retail Price
        /// </summary>
        /// <remarks>
        /// This should represent the normal retail list price (if different than the sales price) for the product. This
        /// entry should be numerical only (no text), should not include any symbols (including $), and should be
        /// quoted in US dollars only between the values of .01-10,000.
        /// Example: 8.00
        /// </remarks>
        [CsvField("RetailPrice", IsRequired: false)]
        public string RetailPrice { get; set; }

        /// <summary>
        /// Special Offer
        /// </summary>
        /// <remarks>
        /// This text may be up to 2048 characters and should represent a specific offer provided for the product to
        /// appear on comparison grid and product grid pages. Offers should be specific and relevant to the product,
        /// such as “Free 1 GB SD Card” or “$150 rebate”. Offers may not be generic, such as “Free Shipping” or “Always 
        /// Lowest Prices”. The character limit for this field is 2048 characters. After 2048 characters, all special 
        /// offers will be trimmed. Example: Free 1 GB SD Card
        /// 
        /// </remarks>
        [CsvField("SpecialOffer", IsRequired: false)]
        public string SpecialOffer { get; set; }

        /// <summary>
        /// CouponText
        /// </summary>
        /// <remarks>
        /// This text may be up to 2048 characters long. This should represent whether there is a coupon applicable
        /// to this product. If the retailer has a coupon available for this product, please describe the discount in this
        /// section. Example: 20% Discount Coupon Available for this Product!
        /// </remarks>
        [CsvField("CouponText", IsRequired: false)]
        public string CouponText { get; set; }

        /// <summary>
        /// Coupon Code
        /// </summary>
        /// <remarks>
        /// This should represent if there is a coupon code required to take advantage of the coupon for this product,
        /// for instance if a user needs to input a coupon. This field must be no more than 128 characters long and
        /// should only contain the coupon code. Example: 445JJF4
        /// </remarks>
        [CsvField("CouponCode", IsRequired: false)]
        public string CouponCode { get; set; }

        /// <summary>
        /// InStock
        /// </summary>
        /// <remarks>
        /// This should represent if the item is in stock or not. Available options: In Stock, Limited Quantities,
        /// Preorder, Backorder or Special
        /// Example: In Stock
        /// </remarks>
        [CsvField("InStock", IsRequired: false)]
        public string InStock { get; set; }

        /// <summary>
        /// Inventory Count
        /// </summary>
        /// <remarks>
        /// This should represent the number of SKUs of this item that the merchant has remaining in stock. This
        /// field can be any integer between 0-1,000,000. Example:  34
        /// </remarks>
        [CsvField("InventoryCount", IsRequired: false)]
        public string InventoryCount { get; set; }

        /// <summary>
        /// Bundle
        /// </summary>
        /// <remarks>
        /// This should represent whether the item comes in a bundle or not, for instance if it’s in a package of 4 or
        /// just a single product. Only two valid values, “yes” or “no”. Example: Yes
        /// </remarks>
        [CsvField("Bundle", IsRequired: false)]
        public string Bundle { get; set; }

        /// <summary>
        /// Release Date
        /// </summary>
        /// <remarks>
        /// Date that this product will be available for sale. This data must be entered in yyyymmdd format and this
        /// date can be up to two years in the future. Example: 19800211
        /// </remarks>
        [CsvField("ReleaseDate", IsRequired: false)]
        public string ReleaseDate { get; set; }

        /// <summary>
        /// ProntoCategoryID
        /// </summary>
        /// <remarks>
        /// The 1 to 3 digit number that represents the specific sub-category where the product fits within Pronto’s
        /// categorization schema. Use the Pronto Category Mapping document available in the Merchant Account
        /// Center to obtain these numbers. Example: 357
        /// </remarks>
        [CsvField("ProntoCategoryID", IsRequired: false)]
        public string ProntoCategoryID { get; set; }

        /// <summary>
        /// Mobile URL
        /// </summary>
        /// <remarks>
        /// Mobile landing page. Must include http:// or https://. This may be up to 2048 characters long.
        /// Example:  http://www.m.merchant.com
        /// </remarks>
        [CsvField("MobileURL", IsRequired: false)]
        public string MobileURL { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        /// <remarks>
        /// This should be a single URL to an image of the product that is being listed. All URLs should be fully
        /// qualified URLs (including the http://www.). The image should be of good size and quality (100 pixels x
        /// 100 pixels, minimum), as Pronto will use that image next to the associated product information. All
        /// submitted image URLs should be for .gifs, .jpegs, or .pngs and cannot be animated.
        /// </remarks>
        [CsvField("ImageURL", IsRequired: true)]
        public string ImageURL { get; set; }

        /// <summary>
        /// Shipping Cost
        /// </summary>
        /// <remarks>
        /// This should be the value for the flat shipping cost for the item. This should represent the lowest cost a
        /// buyer would have to pay for continental US shipping for that product only (represented in US dollars). The
        /// price shall be a real number, consisting of only digits and a decimal point. Free shipping should be
        /// notated with a zero. If the product is unavailable for flat shipping, leave the field empty. Data allowed is
        /// any numerical value between .01-1,000.00.  Example:  6.50
        /// </remarks>
        [CsvField("ShippingCost", IsRequired: false)]
        public string ShippingCost { get; set; }

        /// <summary>
        /// Shipping Weight
        /// </summary>
        /// <remarks>
        /// Weight of the item, in pounds. Data allowed is any numerical value between .01-1,000.00.
        /// Example: 2.5
        /// </remarks>
        [CsvField("ShippingWeight", IsRequired: false)]
        public string ShippingWeight { get; set; }

        /// <summary>
        /// Zip Code
        /// </summary>
        /// <remarks>
        /// Zip code from which the item is shipped, in the five digit US zip code format.
        /// Example: 10019
        /// </remarks>
        [CsvField("ZipCode", IsRequired: false)]
        public string ZipCode { get; set; }

        /// <summary>
        /// Estimated Ship Date
        /// </summary>
        /// <remarks>
        /// The estimated date that the product can be shipped, in text. This field may be up to 128 characters long.
        /// Example: Usually in 1-2 days
        /// </remarks>
        [CsvField("EstimatedShipDate", IsRequired: false)]
        public string EstimatedShipDate { get; set; }

        /// <summary>
        /// ProductBid
        /// </summary>
        /// <remarks>
        /// This is the cost-per-click bid price to be used for the product (in lieu of a category bid). All product bid
        /// values must at least match the minimum category bid for the category of that product. Please reference
        /// the Pronto Category Mapping-SKU Bidding worksheet to ensure that the product bid value meets the
        /// minimum level. All products with bids supplied that are lower than the category minimum will be priced at
        /// the corresponding category bid (or category minimum). All products with no value supplied in the
        /// ‘ProductBid’ field will be priced at the corresponding category bid (or category minimum). All valid product
        /// level bids placed in correctly in the ‘ProductBid’ column will supersede any category level bids provided in
        /// the Bid Manager. Example:   .25
        /// </remarks>
        [CsvField("ProductBid", IsRequired: false)]
        public string ProductBid { get; set; }

        /// <summary>
        /// Product SKU
        /// </summary>
        /// <remarks>
        /// Description in XLS file is “Merchant product identifier only.” Example: 11122
        /// </remarks>
        [CsvField("ProductSKU", IsRequired: false)]
        public string ProductSKU { get; set; }

        /// <summary>
        /// ISBN
        /// </summary>
        /// <remarks>
        /// This should be the unique numeric code that distinguishes all books. This value is specific for books only.
        /// </remarks>
        [CsvField("ISBN", IsRequired: false)]
        public string ISBN { get; set; }

        /// <summary>
        /// UPC Code
        /// </summary>
        /// <remarks>
        /// The 12 digit UPC code for this product.
        /// </remarks>
        [CsvField("UPC Code", IsRequired: false)]
        public string UPCCode { get; set; }

        /// <summary>
        /// EAN
        /// </summary>
        /// <remarks>
        /// The 13 digit EAN for this product.
        /// </remarks>
        [CsvField("EAN", IsRequired: false)]
        public string EAN { get; set; }

        /// <summary>
        /// MPN
        /// </summary>
        /// <remarks>
        /// The manufacturer part number. 0-100 characters.
        /// </remarks>
        [CsvField("MPN", IsRequired: false)]
        public string MPN { get; set; }

        /// <summary>
        /// Sale Rank
        /// </summary>
        /// <remarks>
        /// Relative popularity of this product for the merchant, a 1 being the most popular, 2 being less popular etc.
        /// </remarks>
        [CsvField("SaleRank", IsRequired: false)]
        public string SaleRank { get; set; }

        /// <summary>
        /// Product Highlights
        /// </summary>
        /// <remarks>
        /// Up to 5 comma separated marketing points about the product. Each highlight is allowed 128 characters of text.
        /// Example: 100% cotton, breathable mesh fabric
        /// </remarks>
        [CsvField("ProductHighlights", IsRequired: false)]
        public string ProductHighlights { get; set; }

        /// <summary>
        /// AltImage0
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("AltImage0", IsRequired: false)]
        public string AltImage0 { get; set; }

        /// <summary>
        /// AltImage1
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("AltImage1", IsRequired: false)]
        public string AltImage1 { get; set; }

        /// <summary>
        /// AltImage2
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("AltImage2", IsRequired: false)]
        public string AltImage2 { get; set; }

        /// <summary>
        /// AltImage3
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("AltImage3", IsRequired: false)]
        public string AltImage3 { get; set; }

        /// <summary>
        /// AltImage4
        /// </summary>
        /// <remarks>
        /// </remarks>
        [CsvField("AltImage4", IsRequired: false)]
        public string AltImage4 { get; set; }

        #endregion

        #region Local Methods

        public ProntoFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Pronto, feedProduct)
        {

        }

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            Title = StoreFeedProduct.Title;
            Description = StoreFeedProduct.Description;
            SalePrice = StoreFeedProduct.OurPrice.ToString("N2");
            Condition = "New";
            Category = StoreFeedProduct.CustomProductCategory;

            var trackingInfo = FeedTrackingInfo;
            URL = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsPaidTrackingCode);

            ShortTitle = null;
            Color = null;
            Size = null;
            Attributes = null; // name=value pairs, up to 2048 chars
            Keywords = MakeKeywords();
            Brand = StoreFeedProduct.Brand;
            Manufacturer = StoreFeedProduct.Brand;
            ArtistAuthor = null;
            RetailPrice = StoreFeedProduct.RetailPrice.ToString("N2");
            SpecialOffer = null;
            CouponText = null;
            CouponCode = null;
            InStock = StoreFeedProduct.IsInStock ? "In Stock" : "Out of Stock";
            InventoryCount = null;
            Bundle = null;
            ReleaseDate = null;
            MobileURL = null;
            ImageURL = StoreFeedProduct.ImageUrl;
            ShippingCost = 0.ToString("N2");
            ShippingWeight = null;
            ZipCode = null;
            EstimatedShipDate = "Usually in 1-2 days";
            ProductBid = null;
            ProductSKU = StoreFeedProduct.SKU;
            ISBN = null;
            UPCCode = StoreFeedProduct.UPC;
            EAN = null;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            SaleRank = null;
            ProductHighlights = null;
            AltImage0 = null;
            AltImage1 = null;
            AltImage2 = null;
            AltImage3 = null;
            AltImage4 = null;

            ProntoCategoryID = MakeProntoCategoryID();
        }

        protected abstract string MakeProntoCategoryID();

        protected virtual string MakeKeywords()
        {
            var tags = StoreFeedProduct.Tags;

            // make the first tag Fabric|Trim|Wallpaper, etc.

            tags.Insert(0, StoreFeedProduct.ProductGroup);

            // 2048 chars max

            int tagCount = 25;

            while (true)
            {
                var s = tags.Take(tagCount).ToCommaDelimitedList();

                if (s.Length <= 2048)
                    return s;

                tagCount--;
            }
        }


        #endregion
    }
}
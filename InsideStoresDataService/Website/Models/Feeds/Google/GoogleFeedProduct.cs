using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Website
{
    
    /// <summary>
    /// Represents a single typesafe product entry for a google product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.GoogleCanada, TrackingCode: "g",
        AnalyticsOrganicTrackingCode = "utm_source=googleproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=googleproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class GoogleCanadaFeedProduct : GoogleFeedProduct
    {
        private static double? _markup;

        private double Markup
        {
            get
            {
                if (!_markup.HasValue)
                {
                    var x = WebConfigurationManager.AppSettings["PriceMarkup-CA"];
                    _markup = double.Parse(x);
                }

                return _markup.Value;
            }
        }

        public GoogleCanadaFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.GoogleCanada, feedProduct)
        {
        }

        private string ProductPageUrlWithTracking(string productPageUrl, string FeedTrackingCode, int index = 1, string AnayticsTrackingCode = null)
        {
            var sb = new StringBuilder(200);
            sb.AppendFormat("{0}?fd={1}{2}", productPageUrl, FeedTrackingCode, index);

            if (AnayticsTrackingCode != null)
                sb.AppendFormat("&{0}", AnayticsTrackingCode);

            return sb.ToString();
        }


        protected override void Populate()
        {
            // populate as usual for google, then adjust the price and link for Canada.

            base.Populate();
            Price = string.Format("{0:N2} CAD", Math.Round((double)StoreFeedProduct.OurPrice * Markup, 2)); // 11.99 CAD

            string prefix = string.Empty;

            switch(StoreFeedProduct.Store.StoreKey)
            {
                case StoreKeys.InsideAvenue:
                    prefix = "ia";
                    break;

                case StoreKeys.InsideRugs:
                    prefix = "ir";
                    break;


                case StoreKeys.InsideFabric:
                    prefix = "if";
                    break;


                case StoreKeys.InsideWallpaper:
                    prefix = "iw";
                    break;
            }

            string url = string.Format("https://www.insidestores.ca/products/{0}-{1}-{2}", prefix, StoreFeedProduct.p.ProductID, StoreFeedProduct.p.SEName).ToLower();

            var trackingInfo = FeedTrackingInfo;
            Link = ProductPageUrlWithTracking(url, trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsOrganicTrackingCode);

            AdwordsRedirect = ProductPageUrlWithTracking(url, trackingInfo.TrackingCode, 2, trackingInfo.AnalyticsPaidTrackingCode);
        }
    }


    /// <summary>
    /// Represents a single typesafe product entry for a google product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.Google, TrackingCode: "g",
        AnalyticsOrganicTrackingCode = "utm_source=googleproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=googleproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class GoogleUnitedStatesFeedProduct : GoogleFeedProduct
    {
        public GoogleUnitedStatesFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Google, feedProduct)
        {
        }
    }


    /// <summary>
    /// Represents a single typesafe product entry for a google product feed.
    /// </summary>
    public abstract class GoogleFeedProduct : FeedProduct, IGoogleFeedProduct
    {
        #region Feed Columns

        // optional fields not included below:  tax, pattern, material

        // Two of these three items are needed:   Brand, UPC, MPN

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
        /// This is the name of your item which is required. We recommend you include characteristics 
        /// such as color or brand in the title which differentiates the item from other products.
        /// </summary>
        /// <remarks>
        /// Google displays 64 characters on results page, but about 85 on shopping list page.
        /// Roma Cotton Rich Bootcut Jeans with Belt - Size 8 Tall
        /// Roma Cotton Rich Bootcut Jeans with Belt - Size 8 Standard
        /// Merlin: Series 3 - Volume 2 - 3 DVD Box set
        /// </remarks>
        [CsvField("title", IsRequired: true)]
        public string Title { get; set; }


        /// <summary>
        /// Include only information relevant to the item, but be comprehensive since we use this text to find your item. 
        /// We recommend you submit around 500 to 1000 characters, but you can submit up to 5000 characters.
        /// Make sure to follow our Editorial guidelines closely. For example, do not include any promotional text such 
        /// as "Free shipping", do not use BLOCK CAPITALS, and do not include a description of your store.
        /// </summary>
        /// <remarks>
        /// Google displays about 145 characters on results page, but about 185 on shopping list page.
        /// Attractively styled and boasting stunning picture quality, the LG Flatron M2262D 22" Full HD LCD TV is an excellent television/monitor. The LG Flatron M2262D 22" Full HD LCD TV 
        /// Comes with the belt. A smart pair of bootcut jeans in stretch cotton. The flower print buckle belt makes it extra stylish.
        /// </remarks>
        [CsvField("description", IsRequired: true)]
        public string Description { get; set; }

        /// <summary>
        /// Google product category.
        /// </summary>
        /// <remarks>
        /// Must be an exact phrase from Google's published taxonomy.
        /// Apparel & Accessories > Clothing > Jeans
        /// Media > DVDs & Movies > Television Shows
        /// http://support.google.com/merchants/bin/answer.py?hl=en&answer=160081&topic=30064&ctx=topic
        /// </remarks>
        [CsvField("google_product_category", IsRequired: true)]
        public string GoogleProductCategory { get; set; }

        /// <summary>
        /// The product_type attribute: For this attribute, you can use either one of the categories 
        /// defined in the Google product taxonomy, or your own category names.
        /// </summary>
        ///<remarks>
        ///Can be any private word, phrase, taxonomy. Max 750. Can include own breadcrumbs or from Google's taxonomy.
        ///</remarks>
        [CsvField("product_type", IsRequired: true)]
        public string ProductType { get; set; }


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
        /// Alternate target link for mobile searchers.
        /// </summary>
        /// <remarks>
        /// Not presently used by us.
        /// </remarks>
        [CsvField("mobile_link")]
        public string MobileLink { get; set; }

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
        [CsvField("image_link", IsRequired: true)]
        public string ImageLink { get; set; }

        /// <summary>
        /// Additional images, separate URLs with a comma.
        /// </summary>
        /// <remarks>
        /// Not presently used.
        /// </remarks>
        [CsvField("additional_image_link")]
        public string AdditionalImageLink { get; set; }


        /// <summary>
        /// What is the condition of the item, new?
        /// </summary>
        /// <remarks>
        /// used, new, refurbished
        /// </remarks>
        [CsvField("condition", IsRequired: true)]
        public string Condition { get; set; }


        /// <summary>
        /// Is the product in stock, available for ordering.
        /// </summary>
        /// <remarks>
        /// in stock --- will be in transit within 3 business days
        /// out of stock
        /// preorder
        /// </remarks>
        [CsvField("availability", IsRequired: true)]
        public string Availability { get; set; }

        [CsvField("availability_date")]
        public string AvailabilityDate { get; set; }

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
        /// Sale price if special promotion.
        /// </summary>
        [CsvField("sale_price")]
        public string SalePrice { get; set; }

        /// <summary>
        /// Unit pricing measure. Optional.
        /// </summary>
        /// <remarks>
        /// The measure and dimension of your product as it is sold. Example: 1.5kg
        /// Supported units: Length: in, ft, yd, cm, m, Weight: oz, lb, mg, g, kg, etc.
        /// </remarks>
        //[CsvField("unit_pricing_measure")]
        //public string UnitPricingMeasure { get; set; }

        /// <summary>
        /// Unit pricing base measure. Optional.
        /// </summary>
        /// <remarks>
        /// The product’s base measure for pricing (e.g. 100ml means the price is calculated based on a 100ml units)
        /// </remarks>
        //[CsvField("unit_pricing_base_measure")]
        //public string UnitPricingBaseMeasure { get; set; }

        
        /// <summary>
        /// Starting date of optional sale.
        /// </summary>
        /// <remarks>
        /// 2011-03-01T16:00-08:00/2011-03-03T16:00-08:00
        /// </remarks>
        [CsvField("sale_price_effective_date")]
        public string SalePriceEffectiveDate { get; set; }

        /// <summary>
        /// Name of brand.
        /// </summary>
        [CsvField("brand", IsRequired: true)]
        public string Brand { get; set; }

        /// <summary>
        /// Universal Product Code (UPC). Required (For all new products with a gtin assigned by the manufacturer)
        /// </summary>
        /// <remarks>
        /// A unique numerical identifier for commercial products that's usually associated with a barcode printed on retail merchandise.
        /// In the US - this would be the UPC code if one is known. Not required.
        /// </remarks>
        [CsvField("gtin")]
        public string GTIN { get; set; }

        /// <summary>
        /// The number which uniquely identifies the product to its manufacturer.
        /// </summary>
        /// <remarks>
        /// Required (Only if your new product does not have a manufacturer assigned gtin).
        /// </remarks>
        [CsvField("mpn", IsRequired: true)]
        public string MPN { get; set; }

        /// <summary>
        /// To be used if your new product doesn’t have a GTIN or MPN.
        /// </summary>
        [CsvField("identifier_exists")]
        public string IdentifierExists { get; set; }

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
        [CsvField("age_group")]
        public string AgeGroup { get; set; }


        /// <summary>
        /// regular, petite, plus, big and tall, maternity
        /// </summary>
        [CsvField("size_type")]
        public string SizeType { get; set; }

        /// <summary>
        /// US, UK, EU, etc.
        /// </summary>
        [CsvField("size_system")]
        public string SizeSystem { get; set; }

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

        //[CsvField("material")]
        //public string Material { get; set; }

        //[CsvField("pattern")]
        //public string Pattern { get; set; }

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
        [CsvField("item_group_id")]
        public string ItemGroupID { get; set; }

        /// <summary>
        /// Specified in Merchant center.
        /// </summary>
        /// <remarks>
        /// US:CA:8.25:n   - for CA at 8.25% and do not charge tax on shipping.
        /// </remarks>
        [CsvField("tax")]
        public string Tax { get; set; }

        //[CsvField("tax_category")]
        //public string TaxCategory { get; set; }

        /// <summary>
        /// Specified in Google merchant center.
        /// </summary>
        /// <remarks>
        /// US::Standard Free Shipping:0 USD
        /// US::Standard Rate:4.95 USD,US::Next Day:8.50 USD
        /// US::Standard:14.95 USD
        /// </remarks>
        [CsvField("shipping")]
        public string Shipping { get; set; }

        [CsvField("shipping_weight")]
        public string ShippingWeight { get; set; }

        [CsvField("shipping_width")]
        public string ShippingWidth { get; set; }

        [CsvField("shipping_height")]
        public string ShippingHeight { get; set; }

        [CsvField("shipping_label")]
        public string ShippingLabel { get; set; }

        /// <summary>
        /// The longest amount of time between when an order is placed for a product and when the product ships. 
        /// </summary>
        //[CsvField("max_handling_time")]
        //public string MaxHandlingTime { get; set; }

        /// <summary>
        /// The shortest amount of time between when an order is placed for a product and when the product ships.
        /// </summary>
        //[CsvField("min_handling_time")]
        //public string MinHandlingTime { get; set; }


        //[CsvField("multipack")]
        //public string Multipack { get; set; }

        //[CsvField("is_bundle")]
        //public string IsBundle { get; set; }

        /// <summary>
        /// Energy class. Optional.
        /// </summary>
        //[CsvField("energy_efficiency_class")]
        //public string EnergyEfficiencyClass { get; set; }

        //[CsvField("min_energy_efficiency_class")]
        //public string MinEnergyEfficiencyClass { get; set; }

        //[CsvField("max_energy_efficiency_class")]
        //public string MaxEnergyEfficiencyClass { get; set; }


        [CsvField("adult")]
        public string IsAdult { get; set; }

        // extra fields for adwords

        /// <summary>
        /// Grouping filter. Single word.
        /// </summary>
        /// <remarks>
        /// Ex:   comforters
        /// </remarks>
        [CsvField("adwords_grouping")]
        public string AdwordsGrouping { get; set; }

        /// <summary>
        /// Tags to associate.
        /// </summary>
        /// <remarks>
        /// clothing, shoes
        /// </remarks>
        [CsvField("adwords_labels")]
        public string AdwordsLabels { get; set; }

        /// <summary>
        /// URL to product page specific to adwords.
        /// </summary>
        [CsvField("adwords_redirect")]
        public string AdwordsRedirect { get; set; }

        [CsvField("custom_label_0")]
        public string CustomLabel0 { get; set; }

        [CsvField("custom_label_1")]
        public string CustomLabel1 { get; set; }

        [CsvField("custom_label_2")]
        public string CustomLabel2 { get; set; }

        [CsvField("custom_label_3")]
        public string CustomLabel3 { get; set; }

        [CsvField("custom_label_4")]
        public string CustomLabel4 { get; set; }


        //[CsvField("excluded_destination")]
        //public string ExcludedDestination { get; set; }

        //[CsvField("expiration_date")]
        //public string ExpirationDate { get; set; }


        #endregion

        #region Local Methods

        public GoogleFeedProduct(ProductFeedKeys key, IStoreFeedProduct feedProduct)
            : base(key, feedProduct)
        {
        }        

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {

            ID = StoreFeedProduct.SKU;
            Title = StoreFeedProduct.Title;
            Description = StoreFeedProduct.Description;

            GoogleProductCategory = MakeGoogleProductCategory();
            ProductType = StoreFeedProduct.CustomProductCategory;

            var trackingInfo = FeedTrackingInfo;
            Link = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsOrganicTrackingCode);
            MobileLink = null;
            ImageLink = StoreFeedProduct.ImageUrl;
            AdditionalImageLink = null;

            Condition = "new";
            Availability = StoreFeedProduct.IsInStock ? "in stock" : "out of stock";

            Price = string.Format("{0:N2} USD", StoreFeedProduct.OurPrice); // 11.99 USD
            SalePrice = null;
            SalePriceEffectiveDate = null;

            Brand = StoreFeedProduct.Brand;
            GTIN = StoreFeedProduct.UPC;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            IdentifierExists = "yes";

            Gender = null;
            AgeGroup = null;
            IsAdult = "no";
            SizeType = null;
            SizeSystem = null;
            Color = StoreFeedProduct.Color;
            Size = StoreFeedProduct.Size;

            // material
            // pattern

            ItemGroupID = MakeItemGroupID();

            // Tax
            Tax = "US:NV:8.25:n";

            Shipping = null; // specified in google merchant center
            ShippingWeight = null;
            //ShippingWidth
            //ShippingHeight
            //ShippingLabel

            //Multipack
            //IsBundle

            IsAdult = "FALSE";

            // adwords fields
            // grouping used as a filter in CPC bidding. Can be as granular as desired, just create different groups

            AdwordsGrouping = MakeAdwordsGrouping();

            AdwordsLabels = MakeAdwordsLabels();

            AdwordsRedirect = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 2, trackingInfo.AnalyticsPaidTrackingCode);

            CustomLabel0 = null;
            CustomLabel1 = null;
            CustomLabel2 = null;
            CustomLabel3 = null;
            CustomLabel4 = null;
        }

        protected abstract string MakeGoogleProductCategory();
        protected abstract string MakeAdwordsGrouping();
        protected abstract string MakeAdwordsLabels();

        protected virtual string MakeItemGroupID()
        {
            return null;
        }

        #endregion

    }
}
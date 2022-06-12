using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a Shopify product feed.
    /// </summary>
    /// <remarks>
    /// Actual specifications can be found here:
    /// https://help.shopify.com/manual/products/import-export#product-csv-file-format
    /// </remarks>
    [FeedProduct(ProductFeedKeys.Shopify)]
    public abstract class ShopifyFeedProduct : FeedProduct
    {
        #region Feed Columns

        /// <summary>
        /// Handles are unique names for each product. Required.
        /// </summary>
        /// <remarks>
        /// They can contain letters, dashes and numbers, but no spaces. 
        /// A handle is used in the URL for each product. For example, the handle for a "Women's Snowboard" should 
        /// be womens-snowboard, and the product's URL would be https://yourstore.myshopify.com/product/womens-snowboard
        /// Every line in the CSV starting with a different handle is treated as a new product. If you want to add multiple 
        /// images to a product, or want the product to have variants, you should have multiple lines with the same handle.
        /// </remarks>
        [CsvField("Handle", IsRequired: true)]
        public string Handle { get; set; }


        /// <summary>
        /// The title of your product. Example: Women's Snowboard. Required.
        /// </summary>
        [CsvField("Title", IsRequired: true)]
        public string Title { get; set; }


        /// <summary>
        /// The description of the product in HTML format. This can also be plain text without any formatting. Required.
        /// </summary>
        [CsvField("Body (HTML)", IsRequired: true)]
        public string Description { get; set; }


        /// <summary>
        /// The name of the vendor for your product. For example, John's Apparel. Required.
        /// </summary>
        /// <remarks>
        /// Minimum 2 characters.
        /// </remarks>
        [CsvField("Vendor", IsRequired: true)]
        public string Vendor { get; set; }


        /// <summary>
        /// The Product type. For example, Snowboard. Required.
        /// </summary>
        [CsvField("Type", IsRequired: true)]
        public string Type { get; set; }


        /// <summary>
        /// Comma-separated list of tags used to tag the product. Optional.
        /// </summary>
        /// <remarks>
        /// Most spreadsheet applications automatically add quotes around the tags for you. If you are using a plain 
        /// text editor, you will need to manually add the quote. For example, "tag1, tag2, tag3". 
        /// </remarks>
        [CsvField("Tags")]
        public string Tags { get; set; }


        /// <summary>
        /// States whether or not a product is published on your storefront. Required.
        /// </summary>
        /// <remarks>
        /// Valid values are TRUE if the product is 
        /// published on your storefront, or FALSE if the product is hidden from your storefront. Leaving the 
        /// field blank will publish the product.
        /// </remarks>
        [CsvField("Published", IsRequired: true)]
        public string Published { get; set; }


        /// <summary>
        /// If a product has an option, enter its name. For example, Color. Required.
        /// </summary>
        /// <remarks>
        /// For products with only a single option, this should be set to "Title".
        /// </remarks>
        [CsvField("Option1 Name", IsRequired: true)]
        public string Option1Name { get; set; }


        /// <summary>
        /// If a product has an option, enter its value. For example, Black. Required.
        /// </summary>
        /// <remarks>
        /// For products with only a single option, this should be set to "Default Title".
        /// </remarks>
        [CsvField("Option1 Value", IsRequired: true)]
        public string Option1Value { get; set; }


        /// <summary>
        /// If a product has a second option, enter its name. For example, Size. Optional.
        /// </summary>
        [CsvField("Option2 Name")]
        public string Option2Name { get; set; }


        /// <summary>
        /// If a product has a second option, enter its value. For example, Large. Optional.
        /// </summary>
        [CsvField("Option2 Value")]
        public string Option2Value { get; set; }


        /// <summary>
        /// If a product has a third option, enter its name. Optional.
        /// </summary>
        [CsvField("Option3 Name")]
        public string Option3Name { get; set; }


        /// <summary>
        /// If a product has a third option, enter the value of the option. Optional.
        /// </summary>
        [CsvField("Option3 Value")]
        public string Option3Value { get; set; }


        /// <summary>
        /// The SKU of the product or variant. Used to track inventory with inventory tracking services. 
        /// </summary>
        /// <remarks>
        /// Optional for Shopify, but we require it.
        /// </remarks>
        [CsvField("Variant SKU", IsRequired: true)]
        public string VariantSKU { get; set; }


        /// <summary>
        /// The weight of the product or variant in grams. Required.
        /// </summary>
        /// <remarks>
        /// Do not add a unit of measurement, just the number.
        /// </remarks>
        [CsvField("Variant Grams", IsRequired: true)]
        public string VariantGrams { get; set; }


        /// <summary>
        /// Include your inventory tracking for this variant or product. Optional,
        /// but we require it to be 'shopify'
        /// </summary>
        /// <remarks>
        /// Valid values include "shopify", "shipwire", 
        /// "amazon_marketplace_web", or blank if inventory is not tracked.
        /// </remarks>
        [CsvField("Variant Inventory Tracker")]
        public string VariantInventoryTracker { get; set; }


        /// <summary>
        /// The number of items you have in stock of this product or variant. Required.
        /// </summary>
        [CsvField("Variant Inventory Qty", IsRequired: true)]
        public string VariantInventoryQty { get; set; }


        /// <summary>
        /// How to handle orders when inventory level for this product or variant has reached zero. Required.
        /// </summary>
        /// <remarks>
        /// Valid values are "deny", or "continue". "deny" will stop selling when inventory reaches 0, 
        /// and "continue" will allow sales to continue into negative inventory levels.
        /// </remarks>
        [CsvField("Variant Inventory Policy", IsRequired: true)]
        public string VariantInventoryPolicy { get; set; }


        /// <summary>
        /// The product or variant fulfillment service used. Required.
        /// </summary>
        /// <remarks>
        /// Valid values are: "manual", "shipwire", "webgistix", "amazon_marketplace_web”. If you use a custom fulfillment
        /// service, you can add the name of the service in this column. For the custom name, use only lowercase letters. 
        /// Spaces aren't allowed—replace them with a dash (-). Periods and other special characters are removed. 
        /// For example, if "Mr. Fulfiller" is your fulfillment service's name, enter "mr-fulfiller" in the CSV file.
        /// </remarks>
        [CsvField("Variant Fulfillment Service", IsRequired: true)]
        public string VariantFulfillmentService { get; set; }


        /// <summary>
        /// The price of the product or variant. Don't place any currency symbol there. For example, 9.99. Required.
        /// </summary>
        [CsvField("Variant Price", IsRequired: true)]
        public string VariantPrice { get; set; }


        /// <summary>
        /// The "Compare at Price" of the product or variant. Don't place any currency symbol there. Optional.
        /// </summary>
        /// <remarks>
        /// For example, 9.99. Used for when a product is listed on sale. This would be the higher price.
        /// </remarks>
        [CsvField("Variant Compare At Price")]
        public string VariantCompareAtPrice { get; set; }


        /// <summary>
        /// The option to require shipping. Valid values are "TRUE", "FALSE", or blank. Required.
        /// </summary>
        /// <remarks>
        /// blank = FALSE. Shopify allows blank, but we require it.
        /// </remarks>
        [CsvField("Variant Requires Shipping", IsRequired: true)]
        public string VariantRequiresShipping { get; set; }


        /// <summary>
        /// Apply taxes to this variant. Valid values are "TRUE", "FALSE", or blank. Required.
        /// </summary>
        /// <remarks>
        /// blank = FALSE. Shopify allows blank, but we require it.
        /// </remarks>
        [CsvField("Variant Taxable", IsRequired: true)]
        public string VariantTaxable { get; set; }


        /// <summary>
        /// The barcode, ISBN or UPC of the product. Optional.
        /// </summary>
        /// <remarks>
        /// Can be left blank.
        /// </remarks>
        [CsvField("Variant Barcode")]
        public string VariantBarcode { get; set; }


        /// <summary>
        /// Put the URL for the product image. Required.
        /// </summary>
        /// <remarks>
        /// Shopify will download the images during the import and re-upload them into your store. 
        /// These images are not variant specific. The variant image column is where you specify variant images.
        /// You won't be able to change your image filename after that image has been uploaded to your shop. 
        /// Don't upload images that have _thumb, _small, or _medium suffixes in their names.
        /// </remarks>
        [CsvField("Image Src", IsRequired: true)]
        public string ImageSrc { get; set; }


        /// <summary>
        /// The text that describes an image. Optional.
        /// </summary>
        /// <remarks>
        /// Useful if an image cannot be displayed or a screenreader passes over an image—the text replaces this element.
        /// Can be left blank.
        /// </remarks>
        [CsvField("Image Alt Text")]
        public string ImageAltText { get; set; }


        /// <summary>
        /// States whether the product is a Gift Card or not. Required.
        /// </summary>
        /// <remarks>
        /// Valid values are "TRUE", or "FALSE". The addition of this column also allows you to edit other Gift Card details, 
        /// such as the Body or Tags columns, and import these changes. A gift card can only be created and activated in 
        /// the Shopify admin. You can't initially create a gift card through a product CSV import.
        /// </remarks>
        [CsvField("Gift Card", IsRequired: true)]
        public string GiftCard { get; set; }


        /// <summary>
        /// The MPN, or Manufacturer Part Number, is a string of alphanumeric digits of various lengths (0-9, A-Z).
        /// </summary>
        [CsvField("Google Shopping / MPN")]
        public string GoogleShoppingMPN { get; set; }


        /// <summary>
        /// What age group does this product target? Valid values are Adult or Kids only.
        /// </summary>
        [CsvField("Google Shopping / Age Group")]
        public string GoogleShoppingAgeGroup { get; set; }


        /// <summary>
        /// What gender does this product target? Valid values are Female, Male, or Unisex.
        /// </summary>
        [CsvField("Google Shopping / Gender")]
        public string GoogleShoppingGender { get; set; }


        /// <summary>
        /// Google taxonomy category.
        /// </summary>
        /// <remarks>
        /// Google has a proprietary set of product categories. The full list is quite large to allow merchants to be very specific 
        /// towards their target audience. You can upload any value you want using the CSV file, however if your language format
        /// does not match Google's full product taxonomy, you might not be able to publish the products to Google.
        /// </remarks>
        [CsvField("Google Shopping / Google Product Category")]
        public string GoogleShoppingProductCategory { get; set; }


        /// <summary>
        /// SEO Title for search engines.
        /// </summary>
        /// <remarks>
        /// The SEO Title is found on a product's details page under the Search Engines header.
        /// The SEO Title has a character (letters & numbers) limit of 70. (not sure if length enforced by shopify)
        /// </remarks>
        [CsvField("SEO Title", IsRequired: true)]
        public string SEOTitle { get; set; }


        /// <summary>
        /// SEO Descripton for search engines.
        /// </summary>
        /// <remarks>
        /// The SEO Description is also found on a product's details page under the Search Engines header. 
        /// The SEO Description has a character (letters & numbers) limit of 160. (not sure if length enforced by shopify)
        /// </remarks>
        [CsvField("SEO Description", IsRequired: true)]
        public string SEODescription { get; set; }


        /// <summary>
        /// Group products for adwords.
        /// </summary>
        /// <remarks>
        /// This is used to group products in an arbitrary way. It can be used for Product Filters to limit a campaign to a
        /// group of products, or Product Targets to bid differently for a group of products. You can enter any "string"
        /// data (letters and numbers).
        /// </remarks>
        [CsvField("Google Shopping / AdWords Grouping")]
        public string GoogleShoppingAdwordsGrouping { get; set; }


        /// <summary>
        /// Very similar to adwords_grouping, but it will only only work on Cost Per Click (CPC). 
        /// </summary>
        /// <remarks>
        /// It can hold multiple values, allowing a product to be tagged with multiple labels.
        /// </remarks>
        [CsvField("Google Shopping / AdWords Labels")]
        public string GoogleShoppingAdwordsLabels { get; set; }


        /// <summary>
        /// State what condition the product will be in at the time of sale (what quality?). 
        /// </summary>
        /// <remarks>
        /// Valid values are new, used, or refurbished.
        /// </remarks>
        [CsvField("Google Shopping / Condition")]
        public string GoogleShoppingCondition { get; set; }


        /// <summary>
        /// False means that this product does not have an MPN.
        /// </summary>
        /// <remarks>
        /// False means that this product does not have an MPN or a unique product identifier (UPC, ISBN, EAN, JAN) set as a 
        /// variant barcode. Valid values are TRUE or FALSE. Learn more here.
        /// </remarks>
        [CsvField("Google Shopping / Custom Product")]
        public string GoogleShoppingCustomProduct { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// You can have up to 5 custom labels for your product numbered 0 through 4. You can identify a specific definition 
        /// for each label and specify a value. For example, Sale.
        /// </remarks>
        [CsvField("Google Shopping / Custom Label 0")]
        public string GoogleShoppingCustomLabel0 { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// You can have up to 5 custom labels for your product numbered 0 through 4. You can identify a specific definition for 
        /// each label and specify a value. For example, ReleaseDate.
        /// </remarks>
        [CsvField("Google Shopping / Custom Label 1")]
        public string GoogleShoppingCustomLabel1 { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// You can have up to 5 custom labels for your product numbered 0 through 4. You can identify a specific definition for each 
        /// label and specify a value. For example, Season.
        /// </remarks>
        [CsvField("Google Shopping / Custom Label 2")]
        public string GoogleShoppingCustomLabel2 { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// You can have up to 5 custom labels for your product numbered 0 through 4. You can identify a specific definition for each 
        /// label and specify a value. For example, Clearance.
        /// </remarks>
        [CsvField("Google Shopping / Custom Label 3")]
        public string GoogleShoppingCustomLabel3 { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// You can have up to 5 custom labels for your product numbered 0 through 4. You can identify a specific definition for each label 
        /// and specify a value. For example, SellingRate.
        /// </remarks>
        [CsvField("Google Shopping / Custom Label 4")]
        public string GoogleShoppingCustomLabel4 { get; set; }


        /// <summary>
        /// URL for your variant-specific image.
        /// </summary>
        /// <remarks>
        /// Shopify will download the images during the import and re-upload them into your store.
        /// </remarks>
        [CsvField("Variant Image")]
        public string VariantImage { get; set; }


        /// <summary>
        /// Convert the variant grams field to a different unit of measure. Optional.
        /// </summary>
        /// <remarks>
        /// Convert the variant grams field to a different unit of measure by entering kg, g, oz, or lb. If this field is left blank, the weight 
        /// will be uploaded as grams and then converted to your store's default weight unit.
        /// </remarks>
        [CsvField("Variant Weight Unit")]
        public string VariantWeightUnit { get; set; }

        /// <summary>
        /// Optionally add product to this collection upon import. Optional.
        /// </summary>
        /// </remarks>
        /// Organize your products into collections during the CSV upload, you can add a new column anywhere in your CSV file with the header name 
        /// Collection. This is the only column you can add to the CSV that will not break the format. Enter the name of the collection you want to add 
        /// this product to. If the collection does not already exist, one will be created for you. You can only add a product to one collection using this method.
        /// </remarks>
        [CsvField("Collection", IsRequired: false)]
        public string Collection { get; set; }


        #endregion

        #region Local Methods

        public ShopifyFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.Shopify, feedProduct)
        {
        }        

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {
            Handle = MakeHandle();
            Title = MakeTitle();
            Description = MakeHtmlDescription();
            Vendor = StoreFeedProduct.Brand;
            Type = MakeProductType();
            Tags = MakeTags();
            Published = "TRUE";
            Option1Name = "Title";
            Option1Value = "Default Title";
            Option2Name = null;
            Option2Value = null;
            Option3Name = null;
            Option3Value = null;
            VariantSKU = StoreFeedProduct.SKU;
            VariantGrams = (28 * 16).ToString(); // 1 pound default.
            VariantInventoryTracker = "shopify";
            VariantInventoryQty = ((StoreFeedProduct.IsInStock == true) ? 999999 : 0).ToString(); // either 999999 or 0.
            VariantInventoryPolicy = "deny"; // deny or continue
            VariantFulfillmentService = "manual";
            VariantPrice = string.Format("{0:N2}", StoreFeedProduct.OurPrice); // 11.99
            VariantCompareAtPrice = null;
            VariantRequiresShipping = "TRUE";
            VariantTaxable = "TRUE";
            VariantBarcode = MakeBarCode();
            ImageSrc = StoreFeedProduct.ImageUrl;
            ImageAltText = Title;
            GiftCard = "FALSE";
            GoogleShoppingMPN = StoreFeedProduct.ManufacturerPartNumber; 
            GoogleShoppingAgeGroup = null;
            GoogleShoppingGender = null;
            GoogleShoppingProductCategory = MakeGoogleProductCategory();
            SEOTitle = MakeSEOTitle();
            SEODescription = MakeSEODescription();
            GoogleShoppingAdwordsGrouping = MakeAdwordsGrouping();
            GoogleShoppingAdwordsLabels = MakeAdwordsLabels();
            GoogleShoppingCondition = "new";
            GoogleShoppingCustomProduct = "FALSE";
            GoogleShoppingCustomLabel0 = StoreFeedProduct.ProductPageUrl;
            GoogleShoppingCustomLabel1 = MakeGoogleShoppingCustomLabel1(); // unit of measure
            GoogleShoppingCustomLabel2 = MakeGoogleShoppingCustomLabel2(); // minimum:increment for purchase
            GoogleShoppingCustomLabel3 = StoreFeedProduct.Store.StoreKey.ToString(); // InsideAvenue|InsideFabric|etc
            GoogleShoppingCustomLabel4 = StoreFeedProduct.p.ProductID.ToString(); // productID within store
            VariantImage = null;
            VariantWeightUnit = null;
            Collection = null;
        }

        /// <summary>
        /// Swaps the location of the brand within the input string.
        /// </summary>
        /// <remarks>
        /// Used primarily to make our titles different between the sites.
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string SwapBrandInName(string input)
        {
            var index = input.IndexOf(" by ");

            if (index == -1)
                return input; // not found, return untouched

            var brand = input.Substring(index + 4);
            var withoutBrand = input.Substring(0, index);

            return string.Format("{0} {1}", brand, withoutBrand);
        }

        protected abstract string MakeHandle();
        protected abstract string MakeTitle();
        protected abstract string MakeHtmlDescription();
        protected abstract string MakeSEOTitle();
        protected abstract string MakeSEODescription();
        protected abstract string MakeGoogleProductCategory();
        protected abstract string MakeAdwordsGrouping();
        protected abstract string MakeAdwordsLabels();


        protected virtual string MakeBarCode()
        {
            return null;
        }

        protected virtual string MakeTags()
        {
            return null;
        }

        protected virtual string MakeProductType()
        {
            return StoreFeedProduct.ProductGroup; 
        }

        protected virtual string MakeGoogleShoppingCustomLabel1()
        {
            // unit of measure, default is Each
            return "Each";
        }

        protected virtual string MakeGoogleShoppingCustomLabel2()
        {
            // minimum:increment, default min 1, increment 1.
            return "1:1";
        }


        #endregion

    }
}
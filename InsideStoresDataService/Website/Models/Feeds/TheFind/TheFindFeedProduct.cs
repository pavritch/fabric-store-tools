using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a TheFind product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.TheFind, TrackingCode: "tf",
        AnalyticsOrganicTrackingCode = "utm_source=thefindproduct&utm_medium=organic&utm_campaign=products",
        AnalyticsPaidTrackingCode = "utm_source=thefindproduct&utm_medium=paid&utm_campaign=products")]
    public abstract class TheFindFeedProduct : FeedProduct
    {
        #region Feed Columns
        // required fields

        /// <summary>
        /// Title
        /// </summary>
        /// <remarks>
        /// The name of the product. Please ensure that the title only includes information about the 
        /// product and not about anything else.
        /// </remarks>
        [CsvField("Title", IsRequired: true)]
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        /// <remarks>
        /// A description of the product. Please ensure that the description only includes information about the
        /// product and not about anything else. No keyword spamming/stuffing.
        /// </remarks>
        [CsvField("Description", IsRequired: true)]
        public string Description { get; set; }

        /// <summary>
        /// Image Link
        /// </summary>
        /// <remarks>
        /// The URL of an image of the product. For best viewing on TheFind, the image referred to by this URL should 
        /// be the largest, best quality picture available online, at least 150 pixels wide and 150 pixels high. 
        /// If a product has no image please specify “no image”.
        /// </remarks>
        [CsvField("Image_Link", IsRequired: true)]
        public string ImageLink { get; set; }

        /// <summary>
        /// Page Url
        /// </summary>
        /// <remarks>
        /// The URL of the product page. A product page typically shows the details of a single product, 
        /// along with a button to buy the product.
        /// </remarks>
        [CsvField("Page_URL", IsRequired: true)]
        public string PageUrl { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        /// <remarks>
        /// The price of the product.
        /// </remarks>
        [CsvField("Price", IsRequired: true)]
        public string Price { get; set; }

        // recommended

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The item's SKU number in your store. This does not need to be unique as many items may share the same SKU.
        /// </remarks>
        [CsvField("SKU", IsRequired: false)]
        public string SKU { get; set; }

        /// <summary>
        /// UPC
        /// </summary>
        /// <remarks>
        /// Universal Product Code or EAN number
        /// </remarks>
        [CsvField("UPC-EAN", IsRequired: false)]
        public string UPC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The item's Manufacturer's Product Number, the unique number assigned to the product by the manufacturer.
        /// </remarks>
        [CsvField("MPN", IsRequired: false)]
        public string MPN { get; set; }

        /// <summary>
        /// ISBN
        /// </summary>
        /// <remarks>
        /// The ISBN number for a book.
        /// </remarks>
        [CsvField("ISBN", IsRequired: false)]
        public string ISBN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// If the item you are listing has a unique id in your system, please include it.
        /// </remarks>
        [CsvField("Unique_ID", IsRequired: false)]
        public string UniqueID { get; set; }

        /// <summary>
        /// Style ID
        /// </summary>
        /// <remarks>
        /// The id number for a particular style of a product. The style should be labeled distinctly in 
        /// the Style_Label field. Styles may have the same SKU but different prices, UPCs, available sizes, etc.
        /// </remarks>
        [CsvField("Style_ID", IsRequired: false)]
        public string StyleID { get; set; }

        /// <summary>
        /// Style Name
        /// </summary>
        /// <remarks>
        /// The style name given to a variation of a product. Styles often share the same SKU number but may 
        /// have different colors, materials, or price. (e.g. a particular shoe’s style may be "Black snake" 
        /// or "Silver soft kid")
        /// </remarks>
        [CsvField("Style_Name", IsRequired: false)]
        public string StyleName { get; set; }

        /// <summary>
        /// Sale
        /// </summary>
        /// <remarks>
        /// If the item is on sale enter Yes.
        /// </remarks>
        [CsvField("Sale", IsRequired: false)]
        public string Sale { get; set; }

        /// <summary>
        /// Sale Price
        /// </summary>
        /// <remarks>
        /// The sale price of the product. This differs from the value of the Price attribute if this product is on sale. 
        /// If the product is not on sale, the Sale_Price value may be left empty or may be equal to the price attribute.
        /// </remarks>
        [CsvField("Sale_Price", IsRequired: false)]
        public string SalePrice { get; set; }

        /// <summary>
        /// Shipping Cost
        /// </summary>
        /// <remarks>
        /// Enter a shipping cost to override TheFind Merchant Center settings.
        /// </remarks>
        [CsvField("Shipping Cost", IsRequired: false)]
        public string ShippingCost { get; set; }

        /// <summary>
        /// Free Shipping
        /// </summary>
        /// <remarks>
        /// If shipping is free for this product, specify as Free Shipping. This will override Merchant Center settings 
        /// and any amount in Shipping Cost field.
        /// </remarks>
        [CsvField("Free Shipping", IsRequired: false)]
        public string FreeShipping { get; set; }

        /// <summary>
        /// Online Only
        /// </summary>
        /// <remarks>
        /// If the item is only sold online, specify Yes. Otherwise, No or leave blank.
        /// </remarks>
        [CsvField("Online_Only", IsRequired: false)]
        public string OnlineOnly { get; set; }

        /// <summary>
        /// Stock Quantity
        /// </summary>
        /// <remarks>
        /// How many are in stock. 0=out of stock online, but may be available locally.
        /// </remarks>
        [CsvField("Stock_Quantity", IsRequired: false)]
        public string StockQuantity { get; set; }

        /// <summary>
        /// User Rating
        /// </summary>
        /// <remarks>
        /// How well users rate the product, from 1 to 5 with 5 being the best rating. 
        /// </remarks>
        [CsvField("User_Rating", IsRequired: false)]
        public string UserRating { get; set; }

        /// <summary>
        /// User Review Link
        /// </summary>
        /// <remarks>
        /// The URL of the user reviews page.
        /// </remarks>
        [CsvField("User_Review_Link", IsRequired: false)]
        public string UserReviewLink { get; set; }

        // additionally recommended attributes

        /// <summary>
        /// Brand
        /// </summary>
        /// <remarks>
        /// The brand of the product.
        /// </remarks>
        [CsvField("Brand", IsRequired: false)]
        public string Brand { get; set; }

        /// <summary>
        /// Categories
        /// </summary>
        /// <remarks>
        /// Categorize the item in a single category or in a breadcrumb format (e.g. Appliances > Washing Machines)
        /// </remarks>
        [CsvField("Categories", IsRequired: false)]
        public string Categories { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        /// <remarks>
        /// A comma-separated list of colors a product comes in. For example, "red,green,teal".
        /// </remarks>
        [CsvField("Color", IsRequired: false)]
        public string Color { get; set; }

        /// <summary>
        /// Compatible With
        /// </summary>
        /// <remarks>
        /// A list of SKUs that are compatible with the product (e.g. ink toner with a list of printers, or belts with a pair of shoes)
        /// </remarks>
        [CsvField("Compatible_With", IsRequired: false)]
        public string CompatibleWith { get; set; }

        /// <summary>
        /// Condition
        /// </summary>
        /// <remarks>
        /// Whether the product is new or used. Acceptable values for this field include "new", "used", and "refurbished".
        /// </remarks>
        [CsvField("Condition", IsRequired: false)]
        public string Condition { get; set; }

        /// <summary>
        /// Coupons
        /// </summary>
        /// <remarks>
        /// A list of the ids of coupons or discounts in TheFind Merchant Center that this product is eligible for.
        /// </remarks>
        [CsvField("Coupons", IsRequired: false)]
        public string Coupons { get; set; }

        /// <summary>
        /// Made In
        /// </summary>
        /// <remarks>
        /// The country where the product was made.
        /// </remarks>
        [CsvField("Made_In", IsRequired: false)]
        public string MadeIn { get; set; }

        /// <summary>
        /// Model
        /// </summary>
        /// <remarks>
        /// The model name of the product (for example, Powershot SD660).
        /// </remarks>
        [CsvField("Model", IsRequired: false)]
        public string Model { get; set; }

        /// <summary>
        /// Model Number
        /// </summary>
        /// <remarks>
        /// Just the number/letter string given to the model (for example, SD660).
        /// </remarks>
        [CsvField("Model_Number", IsRequired: false)]
        public string ModelNumber { get; set; }

        /// <summary>
        /// Similar To
        /// </summary>
        /// <remarks>
        /// A list of SKUs that may be similar to the product.
        /// </remarks>
        [CsvField("Similar_To", IsRequired: false)]
        public string SimilarTo { get; set; }

        /// <summary>
        /// Tags Keywords
        /// </summary>
        /// <remarks>
        /// Provide up to 10 additional keywords describing the product.
        /// </remarks>
        [CsvField("Tags-Keywords", IsRequired: false)]
        public string TagsKeywords { get; set; }

        /// <summary>
        /// Unit Quantity
        /// </summary>
        /// <remarks>
        /// The quantity of items included (e.g. package contains 8 units of toothpaste)
        /// </remarks>
        [CsvField("Unit_Quantity", IsRequired: false)]
        public string UnitQuantity { get; set; }

        /// <summary>
        /// Video Link
        /// </summary>
        /// <remarks>
        /// The URL of a product video.
        /// </remarks>
        [CsvField("Video_Link", IsRequired: false)]
        public string VideoLink { get; set; }

        /// <summary>
        /// Video Title
        /// </summary>
        /// <remarks>
        /// The title of the product video.
        /// </remarks>
        [CsvField("Video Title", IsRequired: false)]
        public string VideoTitle { get; set; }

        /// <summary>
        /// Weight
        /// </summary>
        /// <remarks>
        /// The weight of the product. This can be used in the Merchant Center to trigger Shipping Costs.
        /// </remarks>
        [CsvField("Weight", IsRequired: false)]
        public string Weight { get; set; }

        /// <summary>
        /// Hot or Not
        /// </summary>
        /// <remarks>
        /// Defines whether the product meets one of the 2 criteria:
        /// 1. It is a top selling product. i.e. among the products that have the highest conversion.
        /// 2. It is a product that is new and expected to be popular, e.g. the new Fall Collection or 
        /// the next model of an electronic device. At most 5% of the products from any retailer are expected
        /// to be marked with this flag. It is a binary field with “1” being hot and “0” being not
        /// </remarks>
        [CsvField("Hot or Not", IsRequired: false)]
        public string HotorNot { get; set; }

        /// <summary>
        /// Ordinal Sales Rank
        /// </summary>
        /// <remarks>
        /// The sales rank of the product. Where 1 is the top selling product.
        /// </remarks>
        [CsvField("Ordinal Sales rank", IsRequired: false)]
        public string OrdinalSalesRank { get; set; }


        // product-specific attributes

        /// <summary>
        /// Actors
        /// </summary>
        /// <remarks>
        /// The actors starring in the product.
        /// </remarks>
        [CsvField("Actors", IsRequired: false)]
        public string Actors { get; set; }

        /// <summary>
        /// AgeRange
        /// </summary>
        /// <remarks>
        /// Suggested age range for the toy.
        /// </remarks>
        [CsvField("Age_Range", IsRequired: false)]
        public string AgeRange { get; set; }

        /// <summary>
        /// Artist
        /// </summary>
        /// <remarks>
        /// The artists who created the product.
        /// </remarks>
        [CsvField("Artist", IsRequired: false)]
        public string Artist { get; set; }

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        /// <remarks>
        /// The aspect ratio of the screen. (e.g. 16:9)
        /// </remarks>
        [CsvField("Aspect_Ratio", IsRequired: false)]
        public string AspectRatio { get; set; }

        /// <summary>
        /// Author
        /// </summary>
        /// <remarks>
        /// The author of the book.
        /// </remarks>
        [CsvField("Author", IsRequired: false)]
        public string Author { get; set; }

        /// <summary>
        /// Battery Life
        /// </summary>
        /// <remarks>
        /// The average life of the battery, if the computer is a laptop, in hours.
        /// </remarks>
        [CsvField("Battery_Life", IsRequired: false)]
        public string BatteryLife { get; set; }

        /// <summary>
        /// Binding
        /// </summary>
        /// <remarks>
        /// The binding of the product. (Hardcover, softcover, e-book)
        /// </remarks>
        [CsvField("Binding", IsRequired: false)]
        public string Binding { get; set; }

        /// <summary>
        /// Capacity
        /// </summary>
        /// <remarks>
        /// For electronic devices, the amount of memory included in a product. For appliances, 
        /// the volume of space within the appliance.
        /// </remarks>
        [CsvField("Capacity", IsRequired: false)]
        public string Capacity { get; set; }

        /// <summary>
        /// Color Output
        /// </summary>
        /// <remarks>
        /// Information about whether or not the printer is a color printer.
        /// </remarks>
        [CsvField("Color_Output", IsRequired: false)]
        public string ColorOutput { get; set; }

        /// <summary>
        /// Department
        /// </summary>
        /// <remarks>
        /// The department of a clothing item (e.g. Mens or Womens).
        /// </remarks>
        [CsvField("Department", IsRequired: false)]
        public string Department { get; set; }

        /// <summary>
        /// Director
        /// </summary>
        /// <remarks>
        /// The director of the movie.
        /// </remarks>
        [CsvField("Director", IsRequired: false)]
        public string Director { get; set; }

        /// <summary>
        /// Display Type
        /// </summary>
        /// <remarks>
        /// The type of display on the television or monitor (e.g. LCD)
        /// </remarks>
        [CsvField("Display_Type", IsRequired: false)]
        public string DisplayType { get; set; }

        /// <summary>
        /// Edition
        /// </summary>
        /// <remarks>
        /// The edition of the product. (E.g. Collectors, box set, etc.)
        /// </remarks>
        [CsvField("Edition", IsRequired: false)]
        public string Edition { get; set; }

        /// <summary>
        /// Focus Type
        /// </summary>
        /// <remarks>
        /// The type of focus a camera has. (E.g. Auto)
        /// </remarks>
        [CsvField("Focus_Type", IsRequired: false)]
        public string FocusType { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        /// <remarks>
        /// Format of the product (e.g. DVD)
        /// </remarks>
        [CsvField("Format", IsRequired: false)]
        public string Format { get; set; }

        /// <summary>
        /// Genre
        /// </summary>
        /// <remarks>
        /// The genre of the product. (e.g. rock and roll, country)
        /// </remarks>
        [CsvField("Genre", IsRequired: false)]
        public string Genre { get; set; }

        /// <summary>
        /// Heel Height
        /// </summary>
        /// <remarks>
        /// The heel height of a shoe.
        /// </remarks>
        [CsvField("Heel_Height", IsRequired: false)]
        public string HeelHeight { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        /// <remarks>
        /// The height of the product.
        /// </remarks>
        [CsvField("Height", IsRequired: false)]
        public string Height { get; set; }

        /// <summary>
        /// Installation
        /// </summary>
        /// <remarks>
        /// How a product is installed (e.g. wall-mount)
        /// </remarks>
        [CsvField("Installation", IsRequired: false)]
        public string Installation { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        /// <remarks>
        /// The length of a product (can be a comma-separated list of the lengths).
        /// </remarks>
        [CsvField("Length", IsRequired: false)]
        public string Length { get; set; }

        /// <summary>
        /// Load_Type
        /// </summary>
        /// <remarks>
        /// The type of loading for a washer.
        /// </remarks>
        [CsvField("Load_Type", IsRequired: false)]
        public string LoadType { get; set; }

        /// <summary>
        /// Material
        /// </summary>
        /// <remarks>
        /// The material the product is made out of.
        /// </remarks>
        [CsvField("Material", IsRequired: false)]
        public string Material { get; set; }

        /// <summary>
        /// Media Rating
        /// </summary>
        /// <remarks>
        /// The rating of the product. For example, PG-13.
        /// </remarks>
        [CsvField("Media_Rating", IsRequired: false)]
        public string MediaRating { get; set; }

        /// <summary>
        /// Megapixels
        /// </summary>
        /// <remarks>
        /// The resolution of a digital imaging device.
        /// </remarks>
        [CsvField("Megapixels", IsRequired: false)]
        public string Megapixels { get; set; }

        /// <summary>
        /// Memory Card Slot
        /// </summary>
        /// <remarks>
        /// The available memory card slots in a printer.
        /// </remarks>
        [CsvField("Memory_Card_Slot", IsRequired: false)]
        public string MemoryCardSlot { get; set; }

        /// <summary>
        /// Occasion
        /// </summary>
        /// <remarks>
        /// The special occasion the jewelry is intended for, if applicable.
        /// </remarks>
        [CsvField("Occasion", IsRequired: false)]
        public string Occasion { get; set; }

        /// <summary>
        /// Optical Drive
        /// </summary>
        /// <remarks>
        /// The type of optical drive included with a computer.
        /// </remarks>
        [CsvField("Optical_Drive", IsRequired: false)]
        public string OpticalDrive { get; set; }

        /// <summary>
        /// Pages
        /// </summary>
        /// <remarks>
        /// Number of pages in the book.
        /// </remarks>
        [CsvField("Pages", IsRequired: false)]
        public string Pages { get; set; }

        /// <summary>
        /// Gaming Platform
        /// </summary>
        /// <remarks>
        /// The platform the game operates on.
        /// </remarks>
        [CsvField("Gaming_Platform", IsRequired: false)]
        public string GamingPlatform { get; set; }

        /// <summary>
        /// Processor Speed
        /// </summary>
        /// <remarks>
        /// The processor speed for the product.
        /// </remarks>
        [CsvField("Processor_Speed", IsRequired: false)]
        public string ProcessorSpeed { get; set; }

        /// <summary>
        /// Publisher
        /// </summary>
        /// <remarks>
        /// The publisher of the product.
        /// </remarks>
        [CsvField("Publisher", IsRequired: false)]
        public string Publisher { get; set; }

        /// <summary>
        /// Recommended Usage
        /// </summary>
        /// <remarks>
        /// Recommended usage of a computer. (e.g. home or office)
        /// </remarks>
        [CsvField("Recommended_Usage", IsRequired: false)]
        public string RecommendedUsage { get; set; }

        /// <summary>
        /// Sales Rank
        /// </summary>
        /// <remarks>
        /// The rank of this product in terms of sales in your store. Lower numbers indicate they are sold more often.
        /// </remarks>
        [CsvField("Sales_Rank", IsRequired: false)]
        public string SalesRank { get; set; }

        /// <summary>
        /// Screen Size
        /// </summary>
        /// <remarks>
        /// The diagonal screen size. (E.g. 42 inches)
        /// </remarks>
        [CsvField("Screen_Size", IsRequired: false)]
        public string ScreenSize { get; set; }

        /// <summary>
        /// Shoe Width
        /// </summary>
        /// <remarks>
        /// The widths that a shoe comes in.
        /// </remarks>
        [CsvField("Shoe_Width", IsRequired: false)]
        public string ShoeWidth { get; set; }

        /// <summary>
        /// Sizes
        /// </summary>
        /// <remarks>
        /// The sizes that a product comes in (e.g. S,M,L, XL, or shoe sizes)
        /// </remarks>
        [CsvField("Sizes", IsRequired: false)]
        public string Sizes { get; set; }

        /// <summary>
        /// Sizes In Stock
        /// </summary>
        /// <remarks>
        /// The sizes that are currently in stock (e.g. 4,7,8,9,11)
        /// </remarks>
        [CsvField("Sizes_In_Stock", IsRequired: false)]
        public string SizesInStock { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        /// <remarks>
        /// The subject of a book.
        /// </remarks>
        [CsvField("Subject", IsRequired: false)]
        public string Subject { get; set; }

        /// <summary>
        /// Tech Spec Link
        /// </summary>
        /// <remarks>
        /// The URL of technical specifications of the product, if available. This should not forward to another URL; 
        /// it must point directly to the target page. The domain name may not be an IP address.
        /// </remarks>
        [CsvField("Tech_Spec_Link", IsRequired: false)]
        public string TechSpecLink { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        /// <remarks>
        /// The width of a product (can be a comma separated list of the widths).
        /// </remarks>
        [CsvField("Width", IsRequired: false)]
        public string Width { get; set; }

        /// <summary>
        /// Wireless Interface
        /// </summary>
        /// <remarks>
        /// Wireless interface that the cell phone uses.
        /// </remarks>
        [CsvField("Wireless_Interface", IsRequired: false)]
        public string WirelessInterface { get; set; }

        /// <summary>
        /// Year
        /// </summary>
        /// <remarks>
        /// The year of the product's issue.
        /// </remarks>
        [CsvField("Year", IsRequired: false)]
        public string Year { get; set; }

        /// <summary>
        /// Zoom
        /// </summary>
        /// <remarks>
        /// The maximum amount a camera can zoom. (E.g. 6x)
        /// </remarks>
        [CsvField("Zoom", IsRequired: false)]
        public string Zoom { get; set; }

        /// <summary>
        /// Alt Image 1
        /// </summary>
        /// <remarks>
        /// URL for alternate image view of product.
        /// </remarks>
        [CsvField("Alt_Image_1", IsRequired: false)]
        public string AltImage1 { get; set; }

        /// <summary>
        /// Alt Image 2
        /// </summary>
        /// <remarks>
        /// URL for alternate image view of product.
        /// </remarks>
        [CsvField("Alt_Image_2", IsRequired: false)]
        public string AltImage2 { get; set; }

        /// <summary>
        /// Alt Image 3
        /// </summary>
        /// <remarks>
        /// URL for alternate image view of product.
        /// </remarks>
        [CsvField("Alt_Image_3", IsRequired: false)]
        public string AltImage3 { get; set; }

        /// <summary>
        /// Option 1
        /// </summary>
        /// <remarks>
        /// Reserved for custom feed attributes.
        /// </remarks>
        [CsvField("Option_1", IsRequired: false)]
        public string Option1 { get; set; }

        /// <summary>
        /// Option 2
        /// </summary>
        /// <remarks>
        /// Reserved for custom feed attributes.
        /// </remarks>
        [CsvField("Option_2", IsRequired: false)]
        public string Option2 { get; set; }

        /// <summary>
        /// Option 3
        /// </summary>
        /// <remarks>
        /// Reserved for custom feed attributes.
        /// </remarks>
        [CsvField("Option_3", IsRequired: false)]
        public string Option3 { get; set; }

        #endregion

        #region Local Methods

        public TheFindFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.TheFind, feedProduct)
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
            ImageLink = StoreFeedProduct.ImageUrl;

            var trackingInfo = FeedTrackingInfo;
            PageUrl = StoreFeedProduct.ProductPageUrlWithTracking(trackingInfo.TrackingCode, 1, trackingInfo.AnalyticsOrganicTrackingCode);

            Price = StoreFeedProduct.OurPrice.ToString("N2");
            SKU = StoreFeedProduct.SKU;
            UPC = StoreFeedProduct.UPC;
            MPN = StoreFeedProduct.ManufacturerPartNumber;
            ISBN = null;
            UniqueID = StoreFeedProduct.ID;
            StyleID = null;
            StyleName = null;
            Sale = null;
            SalePrice = null;
            ShippingCost = null;
            FreeShipping = "Yes";
            OnlineOnly = "Yes";
            StockQuantity = StoreFeedProduct.IsInStock ? "30" : "0"; 
            UserRating = null;
            UserReviewLink = null;
            Brand = StoreFeedProduct.Brand;
            Categories = StoreFeedProduct.CustomProductCategory;
            Color = null;
            CompatibleWith = null;
            Condition = "new";
            Coupons = null;
            MadeIn = null;
            Model = null;
            ModelNumber = null;
            TagsKeywords = MakeTags();
            UnitQuantity = null;
            VideoLink = null;
            VideoTitle = null;
            Weight = null;
            HotorNot = null;
            OrdinalSalesRank = null;
            Actors = null;
            AgeRange = null;
            Artist = null;
            AspectRatio = null;
            Author = null;
            BatteryLife = null;
            Binding = null;
            Capacity = null;
            ColorOutput = null;
            Department = null;
            Director = null;
            DisplayType = null;
            Edition = null;
            FocusType = null;
            Format = null;
            Genre = null;
            HeelHeight = null;
            Height = null;
            Installation = null;
            Length = null;
            LoadType = null;
            Material = null;
            MediaRating = null;
            Megapixels = null;
            MemoryCardSlot = null;
            Occasion = null;
            OpticalDrive = null;
            Pages = null;
            GamingPlatform = null;
            ProcessorSpeed = null;
            Publisher = null;
            RecommendedUsage = null;
            SalesRank = null;
            ScreenSize = null;
            ShoeWidth = null;
            Sizes = null;
            SizesInStock = null;
            Subject = null;
            TechSpecLink = null;
            Width = null;
            WirelessInterface = null;
            Year = null;
            Zoom = null;
            AltImage1 = null;
            AltImage2 = null;
            AltImage3 = null;
            Option1 = null;
            Option2 = null;
            Option3 = null;

            SimilarTo = MakeSimilarTo();
        }

        protected abstract string MakeSimilarTo();

        protected virtual string MakeTags()
        {
            var tags = StoreFeedProduct.Tags;

            // make the first tag Fabric|Trim|Wallpaper, etc.

            tags.Insert(0, StoreFeedProduct.ProductGroup);

            // TheFind limits tags to 10

            return tags.Take(10).ToCommaDelimitedList();
        }

        #endregion


    }
}
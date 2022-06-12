using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entry for a 1stDibs product feed.
    /// </summary>
    [FeedProduct(ProductFeedKeys.FirstDibs)]
    public abstract class DibsFeedProduct : FeedProduct
    {
        #region Feed Columns

        /// <summary>
        /// The title of your product. 
        /// </summary>
        [CsvField("Item Title", IsRequired: true)]
        public string Title { get; set; }


        /// <summary>
        /// The description of the product in HTML format. This can also be plain text without any formatting. Required.
        /// </summary>
        [CsvField("Item Description", IsRequired: true)]
        public string Description { get; set; }


        /// <summary>
        /// The product category. Required.
        /// </summary>
        [CsvField("Category", IsRequired: true)]
        public string Category { get; set; }


        /// <summary>
        /// Unit of purchase. Typically Individual Item. Required.
        /// </summary>
        [CsvField("Sold As:", IsRequired: true)]
        public string SoldAs { get; set; }

        /// <summary>
        /// How many pieces in a set when Sold As set. Required only when Set.
        /// </summary>
        [CsvField("Pieces in Set:", IsRequired: false)]
        public string PiecesInSet { get; set; }


        /// <summary>
        /// Quantity available. Set to Unlimited. Required.
        /// </summary>
        [CsvField("Quantity", IsRequired: true)]
        public string Quantity { get; set; }

        /// <summary>
        /// Is the price negotiable. Set to No. Required.
        /// </summary>
        [CsvField("Price Negotiable", IsRequired: true)]
        public string PriceNegotiable { get; set; }

        /// <summary>
        /// Currency. Set to USD. Required.
        /// </summary>
        [CsvField("Currency", IsRequired: true)]
        public string Currency { get; set; }


        /// <summary>
        /// The price of the productt. Don't place any currency symbol there. For example, 9.99. Required.
        /// </summary>
        [CsvField("List Price", IsRequired: true)]
        public string ListPrice { get; set; }


        /// <summary>
        /// The discount offered to 1stDibs. Set to 15%. Required.
        /// </summary>
        [CsvField("Net Discount", IsRequired: true)]
        public string NetDiscount { get; set; }


        /// <summary>
        /// Attribute. Set to By. Optional.
        /// </summary>
        [CsvField("Attribute Type", IsRequired: false)]
        public string AttributeType { get; set; }

        /// <summary>
        /// Vendor name. Optional.
        /// </summary>
        [CsvField("Creator Name", IsRequired: false)]
        public string CreatorName { get; set; }


        /// <summary>
        /// The role of the creator. Set to Manufacturer or Designer. Optional.
        /// </summary>
        [CsvField("Creator Role", IsRequired: false)]
        public string CreatorRole { get; set; }


        /// <summary>
        /// Style of the item. Set to Modern. Optional.
        /// </summary>
        /// <remarks>
        /// They have a long ist of predefined styles.
        /// </remarks>
        [CsvField("Style", IsRequired: false)]
        public string Style { get; set; }


        /// <summary>
        /// Attribute on style. Set to "In the style of" Optional.
        /// </summary>
        [CsvField("Style Field Type", IsRequired: false)]
        public string StyleFieldType { get; set; }

        /// <summary>
        /// Continent of Origin. "United States" Required.
        /// </summary>
        [CsvField("Continent of Origin", IsRequired: true)]
        public string ContinentOfOrigin { get; set; }


        /// <summary>
        /// Date of Manufacturer. "2018" Required.
        /// </summary>
        [CsvField("Date of Manufacture", IsRequired: true)]
        public string DateOfManufacture { get; set; }

        /// <summary>
        /// Period. "21st Century" Required.
        /// </summary>
        [CsvField("Period", IsRequired: true)]
        public string Period { get; set; }


        /// <summary>
        /// Production type. "Current Production" Required.
        /// </summary>
        [CsvField("Production Type", IsRequired: true)]
        public string ProductionType { get; set; }


        /// <summary>
        /// Production type. "Available Now" Required.
        /// </summary>
        [CsvField("Lead Time", IsRequired: true)]
        public string LeadTime { get; set; }


        /// <summary>
        /// Variation Details. Required if Variations are available.  Optional.
        /// </summary>
        [CsvField("Variation Details", IsRequired: false)]
        public string VariationDetails { get; set; }

        /// <summary>
        /// Materials. "Cotton"  Optional.
        /// </summary>
        [CsvField("Materials", IsRequired: false)]
        public string Materials { get; set; }

        /// <summary>
        /// Technique. "Woven"  Optional.
        /// </summary>
        [CsvField("Technique", IsRequired: false)]
        public string Technique { get; set; }

        /// <summary>
        /// Measurement unit. "Inches"  Required.
        /// </summary>
        [CsvField("Measurement Unit", IsRequired: true)]
        public string MeasurementUnit { get; set; }

        /// <summary>
        /// Depth. Required.
        /// </summary>
        [CsvField("Depth", IsRequired: true)]
        public string Depth { get; set; }


        /// <summary>
        /// Width. Required.
        /// </summary>
        [CsvField("Width", IsRequired: true)]
        public string Width { get; set; }

        /// <summary>
        /// Height. Optional.
        /// </summary>
        [CsvField("Height", IsRequired: false)]
        public string Height { get; set; }


        /// <summary>
        /// Handling time. "3 Business Days" Required.
        /// </summary>
        [CsvField("Handling Time", IsRequired: true)]
        public string HandlingTime { get; set; }

        /// <summary>
        /// Return Policy. "All sales are final." Required.
        /// </summary>
        [CsvField("Return Policy", IsRequired: true)]
        public string ReturnPolicy { get; set; }


        /// <summary>
        /// Seller Ref No. Our SKU. Optional.
        /// </summary>
        [CsvField("Seller Ref No.", IsRequired: false)]
        public string SellerRefNo { get; set; }

        /// <summary>
        /// Publish Options. "Marketplace" Required.
        /// </summary>
        [CsvField("Publish Options", IsRequired: true)]
        public string PublishOptions { get; set; }


        /// <summary>
        /// Image 1. Required.
        /// </summary>
        [CsvField("Image 1", IsRequired: true)]
        public string Image1 { get; set; }

        /// <summary>
        /// Image 2. Optional.
        /// </summary>
        [CsvField("Image 2", IsRequired: false)]
        public string Image2 { get; set; }

        /// <summary>
        /// Image 3. Optional.
        /// </summary>
        [CsvField("Image 3", IsRequired: false)]
        public string Image3 { get; set; }

        // ++++++++++++++++++++++++++++++++++++++++++++

        // fields added by Peter

        /// <summary>
        /// The description of the product in HTML format. Not part of the original template.
        /// </summary>
        [CsvField("Html Description", IsRequired: false)]
        public string HtmlDescription { get; set; }

        #endregion

        #region Local Methods

        public DibsFeedProduct(IStoreFeedProduct feedProduct)
            : base(ProductFeedKeys.FirstDibs, feedProduct)
        {
        }        

        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected override void Populate()
        {

            Title = MakeTitle();
            Description = StoreFeedProduct.Description.Replace(" Swatches available.", ""); 
            Category = MakeCategory();
            SoldAs = "Individual Item";
            PiecesInSet = "";
            Quantity = "Unlimited";
            PriceNegotiable = "No";
            Currency = "USD";
            ListPrice = string.Format("{0:N2}", StoreFeedProduct.OurPrice); // 11.99;
            NetDiscount = "0%";
            AttributeType = "By";
            CreatorName = StoreFeedProduct.Brand;
            CreatorRole = "Manufacturer";
            Style = "Modern";
            StyleFieldType = "In the style of";
            ContinentOfOrigin = MakeContinentOfOrigin();
            DateOfManufacture = DateTime.Now.Year.ToString();
            Period = "21st Century";
            ProductionType = "Current Production";
            LeadTime = "Available Now";
            VariationDetails = "";
            Materials = MakeMaterials();
            Technique = "";
            MeasurementUnit = "Inches";

            Width = MakeWidth(); 
            Depth = MakeDepth();

            Height = "";
            HandlingTime = "3 Business Days";
            ReturnPolicy = "All sales are final.";
            SellerRefNo = StoreFeedProduct.SKU;
            PublishOptions = "Marketplace";
            Image1 = StoreFeedProduct.ImageUrl;
            Image2 = "";
            Image3 = "";

            // custom fields 

            HtmlDescription = MakeHtmlDescription();

        }


        protected abstract string MakeTitle();
        protected abstract string MakeHtmlDescription();


        protected abstract string MakeContinentOfOrigin();
        protected abstract string MakeWidth();
        protected abstract string MakeDepth();
        protected abstract string MakeMaterials();
        protected abstract string MakeCategory();


        #endregion

    }
}
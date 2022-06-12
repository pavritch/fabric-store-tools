using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public class ProductSummary
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public int ManufacturerID { get; set; }
        public int CategoryID { get; set; }

        public string ImageLargeUrl { get; set; } // on our main website
        public string ImageMediumUrl { get; set; } // on our main website
        public string ImageSmallUrl { get; set; } // on our main website
        public string ImageIconUrl { get; set; } // on our main website
        public string ProductUrl { get; set; }  // on our main website
        public bool IsPretty { get; set; }

        public decimal Cost { get; set; }
        public decimal OurPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal MSRP { get; set; }
    }
}
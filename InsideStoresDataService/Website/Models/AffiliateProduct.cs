using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Represents a product from Share a Sale.
    /// </summary>
    public class AffiliateProduct
    {
        public long ProductID { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string ProductUrl { get; set; }
        public decimal Price { get; set; }
    }
}
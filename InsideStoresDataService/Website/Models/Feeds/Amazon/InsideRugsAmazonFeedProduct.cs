using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Gen4.Util.Misc;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;

namespace Website
{
    /// <summary>
    /// Amazon supports their own plus the google format. Below is the Google format
    /// with slightly different data than the feed submitted to google.
    /// </summary>
    public class InsideRugsAmazonFeedProduct : AmazonFeedProduct
    {

        public InsideRugsAmazonFeedProduct(InsideRugsFeedProduct feedProduct) : base(feedProduct)
        {
            try
            {
                if (!StoreFeedProduct.IsValid)
                    return;

                Populate();

                IsValid = IsValidFeedProduct(this);
            }
            finally
            {
                StoreFeedProduct = null;
            }
        }

        protected override string MakeItemGroupID()
        {
            return StoreFeedProduct.p.ProductID.ToString();
        }

        protected override string MakeShipping()
        {
            return "US::Standard Free Shipping:0 USD";
        }

        protected override string MakeAmazonProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideRugsFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

    }
}
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
    public class InsideWallpaperAmazonFeedProduct : AmazonFeedProduct
    {

        public InsideWallpaperAmazonFeedProduct(InsideWallpaperFeedProduct feedProduct) : base(feedProduct)
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
                // release the memory, no longer needed.
                StoreFeedProduct = null;
            }
        }


        protected override string MakeShipping()
        {
            return "US::Standard Free Shipping:0 USD";
        }

        protected override string MakeAmazonProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideWallpaperFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

    }
}
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
    public class InsideWallpaperTheFindFeedProduct : TheFindFeedProduct
    {

        public InsideWallpaperTheFindFeedProduct(InsideWallpaperFeedProduct feedProduct) : base(feedProduct)
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

        protected override string MakeSimilarTo()
        {
            var feedProduct = StoreFeedProduct as InsideWallpaperFeedProduct;

            var skuList = feedProduct.SimilarByPattern;

            if (skuList == null || skuList.Count() == 0)
                return null;

            return skuList.Take(20).ToCommaDelimitedList();
        }

    }
}
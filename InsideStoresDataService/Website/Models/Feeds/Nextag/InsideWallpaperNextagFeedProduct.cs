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
    public class InsideWallpaperNextagFeedProduct : NextagFeedProduct
    {

        public InsideWallpaperNextagFeedProduct(InsideWallpaperFeedProduct feedProduct) : base(feedProduct)
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

        protected override string MakeCategoryNextagNumericID()
        {
            return "2700468 : More Categories / Home & Garden / Furnishings / Home Decorating";
        }

        protected override string MakeProductName(IStoreFeedProduct FeedProduct)
        {
            var index = FeedProduct.Title.IndexOf(" by ", 0, StringComparison.OrdinalIgnoreCase);

            // if " by " not found return standard title

            if (index == -1)
                return FeedProduct.Title;

            // truncate the manufacturer

            return FeedProduct.Title.Substring(0, index);
        }

    }
}
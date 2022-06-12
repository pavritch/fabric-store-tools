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

    public class InsideFabricGoogleUnitedStatesFeedProduct : GoogleUnitedStatesFeedProduct
    {
        public InsideFabricGoogleUnitedStatesFeedProduct(InsideFabricFeedProduct feedProduct)
            : base(feedProduct)
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


        protected override string MakeGoogleProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideFabricFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

        protected override string MakeAdwordsGrouping()
        {
            var feedProduct = StoreFeedProduct as InsideFabricFeedProduct;
            return string.Format("{0}:{1}", feedProduct.BrandKeyword, StoreFeedProduct.ProductGroup.Replace("Wallcovering", "Wallpaper")).ToLower();
        }

        protected override string MakeAdwordsLabels()
        {
            var tags = StoreFeedProduct.Tags;

            // google apparently limits label count to 10

            if (tags.Count() > 0)
                return tags.Take(10).ToCommaDelimitedList();

            return null;
        }

    }

    public class InsideFabricGoogleCanadaFeedProduct : GoogleCanadaFeedProduct
    {
        public InsideFabricGoogleCanadaFeedProduct(InsideFabricFeedProduct feedProduct)
            : base(feedProduct)
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


        protected override string MakeGoogleProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideFabricFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

        protected override string MakeAdwordsGrouping()
        {
            var feedProduct = StoreFeedProduct as InsideFabricFeedProduct;
            return string.Format("{0}:{1}", feedProduct.BrandKeyword, StoreFeedProduct.ProductGroup.Replace("Wallcovering", "Wallpaper")).ToLower();
        }

        protected override string MakeAdwordsLabels()
        {
            var tags = StoreFeedProduct.Tags;

            // google apparently limits label count to 10

            if (tags.Count() > 0)
                return tags.Take(10).ToCommaDelimitedList();

            return null;
        }

    }

}
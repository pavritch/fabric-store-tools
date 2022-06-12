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

    public class InsideAvenueGoogleUnitedStatesFeedProduct : GoogleUnitedStatesFeedProduct
    {
        public InsideAvenueGoogleUnitedStatesFeedProduct(InsideAvenueFeedProduct feedProduct)
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
            var feedProduct = StoreFeedProduct as InsideAvenueFeedProduct;
            return feedProduct.GoogleProductCategory;
        }

        protected override string MakeAdwordsGrouping()
        {
            return string.Empty;
        }

        protected override string MakeAdwordsLabels()
        {
            return string.Empty;
        }
    }

    public class InsideAvenueGoogleCanadaFeedProduct : GoogleCanadaFeedProduct
    {
        public InsideAvenueGoogleCanadaFeedProduct(InsideAvenueFeedProduct feedProduct)
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
            var feedProduct = StoreFeedProduct as InsideAvenueFeedProduct;
            return feedProduct.GoogleProductCategory;
        }

        protected override string MakeAdwordsGrouping()
        {
            return string.Empty;
        }

        protected override string MakeAdwordsLabels()
        {
            return string.Empty;
        }
    }

}
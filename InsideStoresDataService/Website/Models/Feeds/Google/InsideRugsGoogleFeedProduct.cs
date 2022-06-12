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
    public class InsideRugsGoogleUnitedStatesFeedProduct : GoogleUnitedStatesFeedProduct
    {
        public InsideRugsGoogleUnitedStatesFeedProduct(InsideRugsFeedProduct feedProduct)
            : base(feedProduct)
        {
            try
            {
                if (!StoreFeedProduct.IsValid)
                    return;

                Populate();

                IsValid = IsValidFeedProduct(this);
                Size = StoreFeedProduct.pv.Dimensions;

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

        protected override string MakeGoogleProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideRugsFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

        protected override string MakeAdwordsGrouping()
        {
            var feedProduct = StoreFeedProduct as InsideRugsFeedProduct;
            return string.Format("{0}:{1}", feedProduct.BrandKeyword, StoreFeedProduct.ProductGroup).ToLower();
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

    public class InsideRugsGoogleCanadaFeedProduct : GoogleCanadaFeedProduct
    {
        public InsideRugsGoogleCanadaFeedProduct(InsideRugsFeedProduct feedProduct)
            : base(feedProduct)
        {
            try
            {
                if (!StoreFeedProduct.IsValid)
                    return;

                Populate();

                IsValid = IsValidFeedProduct(this);
                Size = StoreFeedProduct.pv.Dimensions;

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

        protected override string MakeGoogleProductCategory()
        {
            var feedProduct = StoreFeedProduct as InsideRugsFeedProduct;
            return feedProduct.KindOfProduct.Description();
        }

        protected override string MakeAdwordsGrouping()
        {
            var feedProduct = StoreFeedProduct as InsideRugsFeedProduct;
            return string.Format("{0}:{1}", feedProduct.BrandKeyword, StoreFeedProduct.ProductGroup).ToLower();
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
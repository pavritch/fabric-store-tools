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
    public class InsideFabricShopzillaFeedProduct : ShopzillaFeedProduct
    {

        public InsideFabricShopzillaFeedProduct(InsideFabricFeedProduct feedProduct) : base(feedProduct)
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

        protected override string MakeCategoryID()
        {
            return "13,020,210"; // Miscellaneous Home Decor
        }
    }
}
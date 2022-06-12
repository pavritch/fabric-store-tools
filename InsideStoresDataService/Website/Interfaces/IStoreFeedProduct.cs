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
    /// Interface for common properties in store feed product classes - like InsideFabricFeedProduct
    /// </summary>
    public interface IStoreFeedProduct
    {
        IWebStore Store { get;}

        Product p { get; }

        ProductVariant pv { get; }

        Manufacturer m { get; }

        List<Category> categories { get; }

        bool IsValid { get; }

        string SKU { get; }

        string ID { get; }

        string Title { get; }

        string UPC { get; }

        string Description { get; }

        decimal OurPrice { get; }

        decimal RetailPrice { get; }

        bool IsInStock { get; }

        string Brand { get; }

        string ManufacturerPartNumber { get; }

        string ProductPageUrl { get; }

        string ProductPageUrlWithTracking(string FeedTrackingCode, int index = 1, string AnayticsTrackingCode=null);

        string ImageUrl { get; }

        string ProductGroup { get; }

        string CustomProductCategory { get; }

        string Size { get; } // can also be dimensions, else null

        List<string> Tags { get; }

        string Color { get; } // null if not defined
    }
}
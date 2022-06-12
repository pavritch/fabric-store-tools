using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ProductFeedAttribute : System.Attribute
    {
        public ProductFeedKeys FeedKey { get; set; }

        public ProductFeedAttribute(ProductFeedKeys FeedKey)
        {
            this.FeedKey = FeedKey;
        }
    }
}
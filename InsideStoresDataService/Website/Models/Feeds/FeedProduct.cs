using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Gen4.Util.Misc;
using System.Reflection;

namespace Website
{
    /// <summary>
    /// Represents a single typesafe product entity
    /// </summary>
    public abstract class FeedProduct
    {
        public bool IsValid { get; set; }
        protected IStoreFeedProduct StoreFeedProduct { get; set; }
        protected ProductFeedKeys FeedKey { get; private set; }

        public FeedProduct(ProductFeedKeys feedKey, IStoreFeedProduct feedProduct)
        {
            IsValid = false;
            this.StoreFeedProduct = feedProduct;
            this.FeedKey = feedKey;
        }

        public virtual string FeedTrackingCode
        {
            get
            {
                //shopzilla: sz
                //amazon: a
                //bing: b
                //google: g
                //nextag: n
                //pricegrabber: pg
                //pronto: p
                //shopping: s
                //thefind: tf

                var fp = this.GetType().Attributes<FeedProductAttribute>().First();
                return fp.TrackingCode;
            }
        }

        public virtual FeedProductAttribute FeedTrackingInfo
        {
            get
            {
                var fp = this.GetType().Attributes<FeedProductAttribute>().First();

                return fp;
            }
        }

        protected virtual bool IsValidFeedProduct(object obj)
        {

            // use reflection to get the field names from the attributes on AmazonFeedProduct

            var properties = obj.GetType().MemberProperties().ToList();
            foreach (PropertyInfo pi in properties)
            {
                var csvAttribute = pi.Attributes<CsvFieldAttribute>().FirstOrDefault();
                if (csvAttribute == null || !csvAttribute.IsRequired)
                    continue;

                // make sure has a value

                if (pi.PropertyType != typeof(string))
                    continue;

                // if the required field is not populated, return with IsValid set to false.

                var value = (string)pi.GetValue(this, null);
                if (string.IsNullOrWhiteSpace(value))
                {
                    //Debug.WriteLine(string.Format("Field empty: {0}", pi.Name));
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Perform common population through IStoreFeedProduct.
        /// </summary>
        /// <param name="FeedProduct"></param>
        protected abstract void Populate();

        protected bool IsSafeFilename(string filename)
        {
            foreach (var c in filename)
            {
                if (c >= 'A' && c <= 'Z')
                    continue;

                if (c >= 'a' && c <= 'z')
                    continue;

                if (c >= '0' && c <= '9')
                    continue;

                if (".-_".Contains(c))
                    continue;

                return false;
            }

            return true;
        }
    }
}
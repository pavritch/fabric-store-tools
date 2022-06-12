using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using Website.Entities;
using System.IO;
using System.Configuration;
using Gen4.Util.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;
using InsideFabric.Data;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Website
{
    public abstract class InsideStoresProductBase
    {
        protected static object lockObj = new object();

        protected AspStoreDataContext dc;
        protected Dictionary<string, object> _extensionData4;
        protected bool _isExtensionData4Dirty;

        public IWebStore Store;

        public Product p { get; private set; }

        public ProductVariant pv { get; private set; }
        public List<ProductVariant> variants { get; private set; }

        public Manufacturer m { get; private set; }

        public List<Category> categories { get; private set; }

        public bool IsValid { get; protected set; }


        public void Initialize(IWebStore store, Product p, List<ProductVariant> variants, Manufacturer m, List<Category> categories, AspStoreDataContext dc)
        {
            this.Store = store;
            this.p = p;
            this.pv = variants.First();
            this.variants = variants;
            this.m = m;
            this.categories = categories;
            this.dc = dc;

            // check for any reasons to summarily reject

            if (string.IsNullOrWhiteSpace(p.SKU) || p.SKU.Length <= 3)
                return;

            // if gets to here then all is good

            IsValid = true;
        }

        public string ProductPageUrl
        {
            get
            {
                // need to make sure SEName does not contain any unicode characters

                var adjustedSEName = p.SEName;

                if (!IsSafeFilename(adjustedSEName))
                {
                    Debug.WriteLine(string.Format("Invalid url for {0} {1}: {2}", p.ProductID, p.Name, p.SEName));
                    // use the manufacturer's name for that component - ASPDNSF ignores anyway
                    adjustedSEName = m.SEName;
                }

                return string.Format("http://www.{2}/p-{0}-{1}.aspx", p.ProductID, adjustedSEName, Store.Domain);
            }
        }

        public string ProductPageUrlWithTracking(string FeedTrackingCode, int index = 1)
        {
            return string.Format("{0}?fd={1}{2}", ProductPageUrl, FeedTrackingCode, index);
        }

        public string ImageUrl
        {
            get
            {
                return string.Format("http://www.{0}{1}", Store.Domain, ImageName(p.ProductID, p.ImageFilenameOverride));
            }
        }

        public decimal OurPrice
        {
            get
            {
                return pv.Price;
            }
        }

        public decimal? SalePrice
        {
            get
            {
                return pv.SalePrice;
            }
        }

        public decimal RetailPrice
        {
            get
            {
                return pv.MSRP.GetValueOrDefault();
            }
        }


        public bool IsInStock
        {
            get
            {
                return pv.Inventory > 0;
            }
        }

        public string ProductGroup
        {
            get
            {
                return p.ProductGroup;
            }
        }


        public bool IsRug
        {
            get
            {
                return ProductGroup == "Rug";
            }
        }


        public bool IsFabric
        {
            get
            {
                return ProductGroup == "Fabric";
            }
        }


        public bool IsWallpaper
        {
            get
            {
                return (ProductGroup == "Wallcovering");
            }
        }

        public bool IsTrim
        {
            get
            {
                return (ProductGroup == "Trim");
            }
        }


        /// <summary>
        /// Force full regeneration of SEDescription and SEKeywords.
        /// </summary>
        [RunProductAction("RegenerateDescriptions")]
        public void RegenerateDescriptions()
        {
            SpinUpDescriptions();
        }

        /// <summary>
        /// Forces new generation of SEDescription and SEKeywords.
        /// </summary>
        /// <returns></returns>
        public void SpinUpDescriptions()
        {
            try
            {
                SpinSEDescription();
                SpinSEKeywords();
                SpinDescription();
                SpinFroogleDescription();

                dc.Products.UpdateDescriptions(p.ProductID, p.Description, p.SEDescription, p.SEKeywords, p.FroogleDescription);

                p.Summary = GoogleProductCategory;
                dc.Products.UpdateGoogleProductCategory(p.ProductID, p.Summary); // the Summary column in SQL

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        /// <summary>
        /// Key entry point for adding/crafting certain fields after a new product is added.
        /// </summary>
        /// <returns></returns>
        public virtual  void SpinUpMissingDescriptions()
        {
            // Summary (holds google product category for microdata)
            // Description (text description mostly for microdata)
            // SEDescription
            // SEKeywords

            try
            {
                bool isUpdated = false;

                if (string.IsNullOrWhiteSpace(p.Description))
                {
                    SpinDescription();
                    isUpdated = true;
                }

                if (string.IsNullOrWhiteSpace(p.FroogleDescription))
                {
                    SpinFroogleDescription();
                    isUpdated = true;
                }

                if (string.IsNullOrWhiteSpace(p.SEDescription))
                {
                    SpinSEDescription();
                    isUpdated = true;
                }

                if (string.IsNullOrWhiteSpace(p.SEKeywords))
                {
                    SpinSEKeywords();
                    isUpdated = true;
                }

                if (isUpdated)
                    dc.Products.UpdateDescriptions(p.ProductID, p.Description, p.SEDescription, p.SEKeywords, p.FroogleDescription);

                if (string.IsNullOrWhiteSpace(p.Summary))
                {
                    p.Summary = GoogleProductCategory;
                    dc.Products.UpdateGoogleProductCategory(p.ProductID, p.Summary); // the Summary column in SQL
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }

        protected abstract string GoogleProductCategory { get; }
        protected abstract bool SpinDescription();
        protected abstract bool SpinFroogleDescription();
        protected abstract bool SpinSEKeywords();
        protected abstract bool SpinSEDescription();


        public Dictionary<string, object> ExtData4
        {
            get
            {
                if (_extensionData4 == null)
                {
                    var extData = ExtensionData4.Deserialize(p.ExtensionData4);

                    _extensionData4 = extData.Data;
                }

                return _extensionData4;
            }
        }

        public Dictionary<string, string> OriginalRawProperties
        {
            get
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.OriginalRawProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;
                    return dic;
                }
                throw new Exception(string.Format("Missing OriginalRawProperties for ProductID: {0}", p.ProductID));
            }
        }


        protected virtual void SaveExtData4()
        {
            if (!_isExtensionData4Dirty)
                return;

            var extData = new ExtensionData4();
            extData.Data = ExtData4;

            var json = extData.Serialize();
            p.ExtensionData4 = json;

            dc.Products.UpdateExtensionData4(p.ProductID, json);
            _isExtensionData4Dirty = false;
        }


        protected virtual void MarkExtData4Dirty()
        {
            _isExtensionData4Dirty = true;
        }


        protected string ImageName(int ProductID, string OverrideName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(OverrideName))
                    return string.Format("/images/product/large/{0}", OverrideName);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

            throw new Exception("Missing image.");
        }


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

                if (".-".Contains(c))
                    continue;

                return false;
            }

            return true;
        }

        protected void GetImageDimensions(byte[] ContentData, out int? Width, out int? Height)
        {
            Width = null;
            Height = null;

            if (ContentData.Length > 0)
            {
                try
                {
                    using (var bmp = ContentData.FromImageByteArrayToBitmap())
                    {
                        Width = bmp.Width;
                        Height = bmp.Height;
                    }
                }
                catch
                {
                }
            }
        }

    }
}
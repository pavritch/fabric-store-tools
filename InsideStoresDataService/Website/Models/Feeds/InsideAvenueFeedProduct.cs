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
using InsideFabric.Data;

namespace Website
{
    /// <summary>
    /// Helper class to provide common answers for feed fields.
    /// </summary>
    /// <remarks>
    /// This class does most of the heavy lifting for any of the common kinds
    /// of information required by most feeds.
    /// </remarks>
    public class InsideAvenueFeedProduct : IStoreFeedProduct
    {



        #region Locals

        private const int DesignerParentCategoryID = 162;

        private string _description;
        private AspStoreDataContextReadOnly dc;
        private Dictionary<string, object> _extensionData4;

        const int CLASS_ROOT_CATEGORYID = 1;

        private bool primaryCategoryIDIsValid;
        private int? primaryCategoryID;


        #endregion

        #region Ctors
		
        public InsideAvenueFeedProduct()
        {
            IsValid = false;
        }

        public InsideAvenueFeedProduct(IWebStore store, Product p, ProductVariant pv, Manufacturer m, List<Category> categories, AspStoreDataContextReadOnly dc) : this()
        {
            this.Store = store;
            this.p = p;
            this.pv = pv;
            this.m = m;
            this.categories = categories;
            this.dc = dc;

            // check for any reasons to summarily reject

            if (p.Published == 0 || p.Deleted == 1)
                return;

            if (string.IsNullOrWhiteSpace(p.SKU) || p.SKU.Length <=3)
                return;

            if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                return;

            if (pv.Price == 0)
                return;

            // only want to include products with a single variant

            if (dc.ProductVariants.Where(e => e.ProductID == p.ProductID && e.Deleted==0 && e.Published==1).Count() > 1)
                return;

            // if gets to here then all is good

            IsValid = true;
        }

    	#endregion

        #region Properties



        public IWebStore Store { get; private set; }

        public Product p { get; private set; }

        public ProductVariant pv { get; private set; }

        public Manufacturer m { get; private set; }

        public List<Category> categories { get; private set; }

        public bool IsValid { get; private set; }


        public InsideAvenueWebStore InsideAvenueStore
        {
            get
            {
                return Store as InsideAvenueWebStore;
            }
        }

        public string SKU
        {
            get
            {
                return p.SKU.ToUpper();
            }
        }

        public string ID
        {
            get
            {
                // IA for InsideAvenue, then the actual productID
                return string.Format("IA{0}", p.ProductID);  
            }
        }

        public string Title
        {
            get
            {
                return p.Name;
            }
        }

        public string Description
        {
            get
            {
                if (_description == null)
                    _description = MakeDescription();

                return _description;
            }
        }

        public decimal OurPrice
        {
            get
            {
                return pv.Price;
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

        public string Brand
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Features.Brand))
                    return Features.Brand;

                if (!p.SKU.StartsWith("YW-"))
                    return m.Name;

                return string.Empty;
            }
        }

        public string UPC
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Features.UPC))
                    return Features.UPC;

                return null;
            }
        }

        public string ManufacturerPartNumber
        {
            get
            {
                return pv.ManufacturerPartNumber.ToUpper();
            }
        }

        public string Size
        {
            get
            {

                Func<double?, bool> isDefined = (v) =>
                    {
                        if (v.GetValueOrDefault() > 0)
                            return true;

                        return false;
                    };

                Func<string> addDiameter = () =>
                    {
                        if (Features.Features != null && Features.Features.ContainsKey("Diameter") && !string.IsNullOrWhiteSpace(Features.Features["Diameter"]) && Features.Features["Diameter"] != "0")
                        {
                            return string.Format(", Diameter {0}", Features.Features["Diameter"]);
                        }
                        else
                            return string.Empty;
                    };

                if (isDefined(Features.Depth) || isDefined(Features.Width) || isDefined(Features.Height))
                {
                    // create a string

                    if (isDefined(Features.Depth) && isDefined(Features.Width) && isDefined(Features.Height))
                    {
                        return string.Format("{0}\" W x {1}\" H x {2}\" D", Features.Width.Value.ToString("#.####"), Features.Height.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####"));
                    }
                    else if (isDefined(Features.Width) && isDefined(Features.Height))
                    {
                        return string.Format("{0}\" W x {1}\" H", Features.Width.Value.ToString("#.####"), Features.Height.Value.ToString("#.####"));
                    }
                    else if (isDefined(Features.Width) && isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" W x {1}\" D", Features.Width.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####")) + addDiameter();
                    }
                    else if (isDefined(Features.Height) && isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" H x {1}\" D", Features.Height.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Width))
                    {
                        return string.Format("{0}\" W", Features.Width.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Height))
                    {
                        return string.Format("{0}\" H", Features.Height.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" D", Features.Depth.Value.ToString("#.####")) + addDiameter();
                    }
                    else
                        return null;
                }
                else if (Features.Features!= null && Features.Features.ContainsKey("Diameter") && !string.IsNullOrWhiteSpace(Features.Features["Diameter"]) && Features.Features["Diameter"] !="0")
                {
                    return string.Format("Diameter {0}", Features.Features["Diameter"]);
                }

                return null; 
            }
        }


        public List<string> Tags
        {
            get
            {
                // TODO-FEED
                return new List<string>(); 
            }
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


        public string ProductPageUrlWithTracking(string FeedTrackingCode, int index = 1, string AnayticsTrackingCode = null)
        {
#if true
            // we're not presently using tracking for this sort-of google feed
            return ProductPageUrl;
#else
            var sb = new StringBuilder(200);
            sb.AppendFormat("{0}?fd={1}{2}", ProductPageUrl, FeedTrackingCode, index);

            if (AnayticsTrackingCode != null)
                sb.AppendFormat("&{0}", AnayticsTrackingCode);

            return sb.ToString();
#endif
        }

        public string ImageUrl
        {
            get
            {
                return string.Format("http://www.{0}{1}", Store.Domain, ImageName(p.ProductID, p.ImageFilenameOverride));
            }
        }

        public string ProductGroup
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// A custom category using our own private taxonomy. Allowed by some feeds.
        /// </summary>
        public string CustomProductCategory
        {
            get
            {
                return GoogleProductCategory;
            }
        }

        /// <summary>
        /// Category breadcrumb based on google taxonomy.
        /// </summary>
        public string GoogleProductCategory
        {
            get
            {
                var schemaCategory = "Home & Garden > Decor"; // default
                // change to something specific if we've got something, else leave default
                if (PrimaryCategory.HasValue)
                {
                    // associations might be missing from the original CSV file of our internal categories.
                    // will only yield a non-default result here if all required pieces found.

                    int googleCategoryID;
                    if (InsideAvenueStore.PrimaryCategoriesGoogleTaxonomyID.TryGetValue(PrimaryCategory.Value, out googleCategoryID))
                    {
                        string googleCategoryText;
                        if (InsideAvenueStore.GoogleTaxonomyMap.TryGetValue(googleCategoryID, out googleCategoryText))
                            schemaCategory = googleCategoryText;
                    }
                }

                return schemaCategory;
            }
        }

        public string Color
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Features.Color))
                    return Features.Color;

                return null;
            }
        }

        public List<string> CategoryAncestorList
        {
            get
            {
                var result = new List<string>() { "Homeware" };
                var iaStore = Store as InsideAvenueWebStore;

                List<int> ancestors; // includes self, up to but not including root
                if (PrimaryCategory.HasValue && iaStore.PrimaryCategoryAncestors.TryGetValue(PrimaryCategory.Value, out ancestors))
                {
                    foreach (var catID in ancestors)
                    {
                        string name;
                        if (iaStore.PrimaryCategoryNames.TryGetValue(catID, out name))
                            result.Add(name);
                    }
                }

                // most granular at the front of the list
                result.Reverse();
                return result;
            }
        }
        public string PrimaryCategoryName
        {
            get
            {
                var iaStore = Store as InsideAvenueWebStore;

                if (!PrimaryCategory.HasValue)
                    return "Homeware";

                string name;
                if (iaStore.PrimaryCategoryNames.TryGetValue(PrimaryCategory.Value, out name))
                    return name;

                return "Homeware";
            }
        }


        public List<string> ExtractProductDescriptionTextFromExtension4()
        {

            try
            {
                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.HomewareProductFeatures, out obj))
                {
                    var pf = obj as HomewareProductFeatures;

                    if (pf.Description != null && pf.Description.Count() > 0)
                        return pf.Description;
                    else
                        return null;
                }
            }
            catch
            {

            }

            return null;
        }

        public NameValueCollection ExtractProductDescriptionValuesFromExtension4()
        {
            var col = new NameValueCollection();

            try
            {
                Action<string, string> add = (name, value) =>
                {
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                        return;

                    col.Add(name, value);
                };


                Action<string, double?> addDimension = (name, value) =>
                {
                    if (value.GetValueOrDefault() < .01)
                        return;

                    add(name, value.Value.ToInchesMeasurement());
                };


                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.HomewareProductFeatures, out obj))
                {
                    var pf = obj as HomewareProductFeatures;

                    addDimension("Height", pf.Height);
                    addDimension("Width", pf.Width);
                    addDimension("Depth", pf.Depth);

                    add("Color", pf.Color);

                    if (pf.Features != null)
                    {
                        var suppressedFeatures = new string[] { /* none yet  */ };

                        foreach (var item in pf.Features.Where(e => !suppressedFeatures.Contains(e.Key)))
                        {
                            add(item.Key, item.Value);
                        }
                    }

                    if (pf.ShippingWeight.HasValue && pf.ShippingWeight.Value > .01)
                    {
                        int weight = (int)Math.Ceiling(pf.ShippingWeight.Value);
                        string label = weight == 1 ? "lb." : "lbs.";
                        add("Shipping Weight", string.Format("{0} {1}", weight, label));
                    }

                    add("Lead Time", pf.LeadTime);

                    add("Note", pf.PleaseNote);
                }
            }
            catch
            {
            }

            return col;
        }

        #endregion

        #region Local Methods

        private string ImageName(int ProductID, string OverrideName)
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


        private string MakeDescription()
        {
            var sb = new StringBuilder(10000);

            if (!p.SKU.StartsWith("AG-") && Features.Description != null && Features.Description.Count() > 0)
            {
#if true
                var para = Features.Description.First();
                if (!string.IsNullOrWhiteSpace(para))
                    sb.Append(para);
#else
                // creates problems with feed CSV

                int lineCount = 0;
                foreach (var para in Features.Description.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    if (lineCount > 0)
                        sb.Append("\r\r");

                    sb.Append(para);
                    lineCount++;
                }
#endif
            }

            if (sb.ToString().Contains("<!--"))
                sb.Clear();

            if (sb.Length == 0)
                sb.Append(p.Name);

            //Debug.WriteLine(p.SKU + " : " + sb.ToString());
            return sb.ToString();
        }

        private bool IsSafeFilename(string filename)
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


        private int? PrimaryCategory
        {
            get
            {
                if (primaryCategoryIDIsValid)
                    return primaryCategoryID;

                var iaStore = Store as InsideAvenueWebStore;

                foreach (var categoryID in categories.Select(e => e.CategoryID))
                {
                    if (categoryID == CLASS_ROOT_CATEGORYID)
                        continue;

                    if (iaStore.PrimarySqlCategories.Contains(categoryID))
                    {
                        primaryCategoryID = categoryID;
                        break;
                    }
                }

                primaryCategoryIDIsValid = true;
                return primaryCategoryID;
            }
        }

        private Dictionary<string, object> ExtData4
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

        private HomewareProductFeatures Features
        {
            get
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.HomewareProductFeatures, out obj))
                {
                    var f = obj as HomewareProductFeatures;
                    return f;
                }
                throw new Exception(string.Format("Missing HomewareProductFeatures for ProductID: {0}", p.ProductID));
            }
        }

        #endregion
    }
}
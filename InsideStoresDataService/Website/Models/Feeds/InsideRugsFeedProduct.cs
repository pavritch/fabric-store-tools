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
using System.IO;
using System.Xml.Linq;

namespace Website
{
    /// <summary>
    /// Helper class to provide common answers for feed fields.
    /// </summary>
    /// <remarks>
    /// This class does most of the heavy lifting for any of the common kinds
    /// of information required by most feeds. Called once per rug variant.
    /// </remarks>
    public class InsideRugsFeedProduct : IStoreFeedProduct
    {
        #region ProductKind Enum
        /// <summary>
        /// What kind of product this is.
        /// </summary>
        /// <remarks>
        /// The description relates to Google. Other feeds may require something different.
        /// The description attribute must be one of the official Google taxonomy.
        /// See http://support.google.com/merchants/bin/answer.py?hl=en&answer=160081&topic=2473824&ctx=topic
        /// </remarks>
        public enum ProductKind
        {
            [Description("Home & Garden > Decor > Rugs")]
            Rug,
        }
        #endregion


        #region Locals

        private string _description;
        private AspStoreDataContextReadOnly dc;
        private XElement xmlExtensionData;

        #endregion

        #region Ctors
		
        public InsideRugsFeedProduct()
        {
            IsValid = false;
        }

        public InsideRugsFeedProduct(IWebStore store, Product p, ProductVariant pv, Manufacturer m, List<Category> categories, AspStoreDataContextReadOnly dc) : this()
        {
            this.Store = store;
            this.p = p;
            this.pv = pv;
            this.m = m;
            this.categories = categories;
            this.dc = dc;

            // check for any reasons to summarily reject

            if (string.IsNullOrWhiteSpace(p.SKU) || p.SKU.Length <=3)
                return;

            if (string.IsNullOrWhiteSpace(pv.ManufacturerPartNumber))
                return;

            if (pv.Price == 0)
                return;

            if (ImagePath == null)
                return;

            // noticed 11 products had missing suffixes
            if (pv.SKUSuffix == "-")
                return;

            // noticed about 20 variants had these bad UPC codes
            if (UPC.StartsWith("1000"))
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


        public string Color
        {
            get
            {
                return null;
            }
        }

        public ProductKind KindOfProduct
        {
            get
            {
                return ProductKind.Rug;
            }
        }

        public string SKU
        {
            get
            {
                var sku = p.SKU.ToUpper() + pv.SKUSuffix.ToUpper();
                return sku;
            }
        }

        public string ID
        {
            get
            {
                return string.Format("R{0}", pv.VariantID);  
            }
        }

        public string Title
        {
            get
            {
                // [manufacturer-name] [collection-xml] Rug [pv.mpn] [size-xml]

                var s = string.Format("{0} {1} Rug {2} {3}", m.Name, Collection, ManufacturerPartNumber, Size);

                return s;
            }
        }

        public string TitleWithoutManufacturer
        {
            get
            {
                // [collection-xml] Rug [pv.mpn] [size-xml]

                var s = string.Format("{0} Rug {1} {2}", Collection, ManufacturerPartNumber, Size);

                return s;
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

        #region XML Properties


        public string UPC
        {
            get
            { 
                var upc = GetXmlExtensionValue("UPC");
#if false
                if (upc.StartsWith("1000"))
                {
                    Debug.WriteLine(string.Format("Invalid UPC for VariantID {0}: {1}", pv.VariantID, upc));
                }
#endif
                return upc;
            }
        }

        public string Material
        {
            get { return GetXmlExtensionValue("Material"); }
        }

        public string PileHeight
        {
            get { return GetXmlExtensionValue("PileHeight"); }
        }

        public string Country
        {
            get { return GetXmlExtensionValue("Country"); }
        }

        public string Construction
        {
            get { return GetXmlExtensionValue("Construction"); }
        }

        public string Outdoor
        {
            get { return GetXmlExtensionValue("Outdoor"); }
        }

        public string Style
        {
            get { return GetXmlExtensionValue("Style"); }
        }

        public string Colors
        {
            get { return GetXmlExtensionValue("Colors"); }
        }

        public string Size
        {
            get { return GetXmlExtensionValue("Size"); }
        }

        public string Manufacturer
        {
            get { return GetXmlExtensionValue("Manufacturer"); }
        }

        public string Collection
        {
            get { return GetXmlExtensionValue("Collection"); }
        }

        #endregion


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
                return m.Name;
            }
        }


        public string ManufacturerPartNumber
        {
            get
            {
                return pv.ManufacturerPartNumber.ToUpper();
            }
        }

        public string BrandKeyword
        {
            get
            {
                return m.SEName.ToLower();
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
            var sb = new StringBuilder(200);
            sb.AppendFormat("{0}?fd={1}{2}", ProductPageUrl, FeedTrackingCode, index);

            if (AnayticsTrackingCode != null)
                sb.AppendFormat("&{0}", AnayticsTrackingCode);

            return sb.ToString();
        }


        public string ImageUrl
        {
            get
            {
                return string.Format("http://www.{0}{1}", Store.Domain, ImagePath);
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


        /// <summary>
        /// A custom category using our own private taxonomy. Allowed by some feeds.
        /// </summary>
        public string CustomProductCategory
        {
            get
            {
                return "Home & Garden > Decor > Rugs";
            }
        }

        public List<string> Tags
        {
            get
            {
                var moreAttributes = new List<string>();
                moreAttributes.Add(Collection);
                moreAttributes.Add(Style);
                moreAttributes.Add(Material);
                moreAttributes.Add(ProductGroup);
                return moreAttributes;
            }
        }


        #endregion

        #region Local Methods

        private string GetXmlExtensionValue(string key)
        {
            try
            {
                if (xmlExtensionData == null)
                    xmlExtensionData = XElement.Parse(pv.ExtensionData);

                var el = xmlExtensionData.Descendants().Where(e => e.Name == key).FirstOrDefault();

                if (el == null)
                    return null;

                return el.Value;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }

        private string ImagePath
        {
            get
            {
                // first look for populated imagegfilenameoverride in pv
                // then look in p
                // then bail

                if (!string.IsNullOrEmpty(pv.ImageFilenameOverride))
                    return string.Format("/images/variant/large/{0}", pv.ImageFilenameOverride);


                if (!string.IsNullOrEmpty(p.ImageFilenameOverride))
                {
                    // need to fake out so it appears as each variant has its own image
                    // the {variantID}.jpg will be ignored, and we'll rewrite this to be /images/product/large/{fileNoExt}.jpg
                    // which maps it back to the parent product image
                    var fileNoExt = Path.GetFileNameWithoutExtension(p.ImageFilenameOverride);
                    return string.Format("/feeds/images/large/{0}/{1}.jpg", fileNoExt, pv.VariantID);
                }

                return null;
            }
        }

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

        private string ContainsColors
        {
            get
            {
                var ary = Colors.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

                if (ary.Length == 0)
                    return string.Empty;

                var sb = new StringBuilder(128);

                switch (ary.Length)
                {
                    case 0:
                        break;

                    case 1:
                        sb.AppendFormat("Colors include {0}.", ary[0].Trim());
                        break;

                    case 2:
                        sb.AppendFormat("Colors include {0} and {1}.", ary[0].Trim(), ary[1].Trim());
                        break;

                    default:
                        sb.Append("Color ");
                        for (int i = 0; i < ary.Length - 1; i++)
                        {
                            if (i > 0)
                                sb.Append(", ");
                            sb.Append(ary[i].Trim());
                        }
                        sb.AppendFormat(" and {0}.", ary[ary.Length-1].Trim());
                        break;
                }

                return sb.ToString();
            }
        }

        private string MakeDescription()
        {
            var sb = new StringBuilder(1024);

            sb.AppendFormat("{0} Rug. {1} {2}{3} {4} ", Brand, Collection, ManufacturerPartNumber, (Style != "Other" ? " " + Style : string.Empty), Construction);

            sb.AppendFormat("Size {0}. ", Size);

            if (UPC != null)
                sb.AppendFormat("UPC {0}, ", UPC);

            sb.AppendFormat("{0}. {1}", Material, ContainsColors);

            return sb.ToString().Trim();
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
        #endregion
    }
}
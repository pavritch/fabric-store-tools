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
    public class InsideWallpaperFeedProduct : IStoreFeedProduct
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
            [Description("Home & Garden > Decor")]
            Fabric,

            [Description("Hardware > Painting & Wall Covering Supplies > Wallpaper")]
            Wallpaper,

            [Description("Home & Garden > Decor")]
            Bullion,

            [Description("Home & Garden > Decor")]
            Cord,

            [Description("Home & Garden > Decor")]
            Fringe,

            [Description("Home & Garden > Decor")]
            Gimp,

            [Description("Home & Garden > Decor")]
            Tassels,

            [Description("Home & Garden > Decor")]
            Tieback,

            // for when not having any more granular kind of trim
            [Description("Home & Garden > Decor")]
            Trim,
        }
        #endregion

        #region ProductKindCategoryAssoc
        private class ProductKindCategoryAssoc
        {
            public ProductKind Kind { get; set; }
            public List<int> Categories { get; set; }

            public ProductKindCategoryAssoc(ProductKind productKind, List<int> categories)
            {
                this.Kind = productKind;
                this.Categories = categories;
            }
        }
        #endregion

        #region Locals

        private const int DesignerParentCategoryID = 162;

        private ProductKind? _productKind;
        private string _description;
        private AspStoreDataContextReadOnly dc;

        /// <summary>
        /// Lookup table for using categoryID to figure out what kind of product this is.
        /// </summary>
        private static ProductKindCategoryAssoc[] productKinds = new ProductKindCategoryAssoc[]
        {
            new ProductKindCategoryAssoc(ProductKind.Wallpaper, new List<int> {79, 147, 148, 149}),
            new ProductKindCategoryAssoc(ProductKind.Cord, new List<int> {111}),
            new ProductKindCategoryAssoc(ProductKind.Gimp, new List<int> {112}),
            new ProductKindCategoryAssoc(ProductKind.Tassels, new List<int> {113}),
            new ProductKindCategoryAssoc(ProductKind.Tieback, new List<int> {114}),
            new ProductKindCategoryAssoc(ProductKind.Fringe, new List<int> {115}),
            new ProductKindCategoryAssoc(ProductKind.Bullion, new List<int> {116}),
        };

        #region Propert List (ordered)

        /// <summary>
        /// List of which description properties we wish to include in the feed when found.
        /// The list is ordered - will show in this order.
        /// </summary>
        /// <remarks>
        /// This list derrived on 4/23/2012 after review of universe of labels within the data.
        /// List is prior to unified taxonomy. Will need to adjust upon unification.
        /// </remarks>
        private static string[] DescriptionKeepList = new string[]
        {
            "Brand",
            "Item Number",
            "Product",
            "Large Cord",
            "Cord",
            "Cordette",
            "Tassel",
            "Product Name",
            "Pattern",
            "Pattern Name",
            "Pattern Number",
            "Color",
            "Color Name",
            "Color Group",
            "Book",
            "Collection",
            // "Designer", not included since handled separately in output.
            "Category",
            "Group",
            "Product Use",
            "Product Type",
            "Type",
            "Material",
            "Style",
            "Upholstery Use",
            "Use",
            "Dimensions"
        };

        #endregion


        #endregion

        #region Ctors
		
        public InsideWallpaperFeedProduct()
        {
            IsValid = false;
        }

        public InsideWallpaperFeedProduct(IWebStore store, Product p, ProductVariant pv, Manufacturer m, List<Category> categories, AspStoreDataContextReadOnly dc) : this()
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

            if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                return;

            if (string.IsNullOrWhiteSpace(pv.ManufacturerPartNumber))
                return;

            if (pv.Price == 0)
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
                // cache the result

                if (_productKind.HasValue)
                    return _productKind.Value;

                // need to figure out the kind for the first time

                // take whatever hits first, not going to split hairs over which is better kind
                // if something ends up with multiple category hits - our data is not that refined
                // at this point.

                foreach (var catID in categories.Select(e => e.CategoryID))
                {
                    foreach (var item in productKinds)
                    {
                        if (item.Categories.Contains(catID))
                        {
                            _productKind = item.Kind;
                            return _productKind.Value;
                        }
                    }
                }

                switch (p.ProductGroup ?? string.Empty)
                {
                    case "Trim":
                        _productKind = ProductKind.Trim;
                        break;

                    case "Wallcovering":
                        _productKind = ProductKind.Wallpaper;
                        break;

                    // if nothing above hit, then would be fabric

                    default:
                        _productKind = ProductKind.Fabric;
                        break;

                }

                return _productKind.Value;
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
                // F for fabric, then the actual productID
                return string.Format("F{0}", p.ProductID);  
            }
        }

        public string Title
        {
            get
            {
                return string.Format("{0} {1}", p.Name, KindOfProduct.ToString());
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
                return m.Name;
            }
        }

        public string UPC
        {
            get
            {
                // actual UPC returned as of 2/21/2017 since seems google now rejected certain brands whereby they know to exist.
                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.OriginalRawProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;

                    string upcCode;
                    if (dic.TryGetValue("UPC", out upcCode))
                        return upcCode;
                }

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
                return null; // fabrics do not have a size
            }
        }


        public string BrandKeyword
        {
            get
            {
                var aryParts = m.SEName.Split('-');

                var countParts = aryParts.Length;

                var lastPhrase = aryParts[countParts - 1].ToLower();

                if (lastPhrase.StartsWith("fabric"))
                {
                    var newAry = new string[countParts - 1];
                    for (int i = 0; i < countParts-1; i++)
                        newAry[i] = aryParts[i].ToLower();

                    return newAry.Join("-");
                }
                else
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
                return string.Format("http://www.{0}{1}", Store.Domain, ImageName(p.ProductID, p.ImageFilenameOverride));
            }
        }

        public string ProductGroup
        {
            get
            {
                return p.ProductGroup;
            }
        }


        public string Designer
        {
            get
            {
                var cat = categories.Where(e => e.ParentCategoryID == DesignerParentCategoryID).FirstOrDefault();

                if (cat == null || string.IsNullOrWhiteSpace(cat.Name))
                    return null;

                return cat.Name;
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
                return (ProductGroup == "Wallcovering" || categories.Any(e => e.ParentCategoryID == 80));
            }
        }

        public bool IsTrim
        {
            get
            {
                return (ProductGroup == "Trim" || categories.Any(e => e.ParentCategoryID == 105));
            }
        }

        public List<string> FabricTypes
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 3).Select(e => e.Name).ToList();
            }
        }

        public List<string> FabricColors
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 37).Select(e => e.Name).ToList();
            }
        }

        public List<string> FabricPatterns
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 118).Select(e => e.Name).ToList();
            }
        }

        /// <summary>
        /// A custom category using our own private taxonomy. Allowed by some feeds.
        /// </summary>
        public string CustomProductCategory
        {
            get
            {
                switch (KindOfProduct)
                {
                    case ProductKind.Wallpaper:
                        return "Wallpaper";

                    default:
                        return string.Format("Home & Garden > Decor > {0}", KindOfProduct.ToString());
                }
            }
        }

        public List<string> Tags
        {
            get
            {
                var moreAttributes = new List<string>();
                moreAttributes.AddRange(FabricColors);
                moreAttributes.AddRange(FabricTypes);
                moreAttributes.AddRange(FabricPatterns);

                return moreAttributes;
            }
        }

        public List<string> SimilarByPattern
        {
            get
            {
                var list = new List<string>();

                if (!(IsFabric || IsWallpaper))
                    return list;

                // must have a p.mpn since that is where we have traditionally kept the 
                // base pattern (without the color component

                if (string.IsNullOrWhiteSpace(p.ManufacturerPartNumber))
                    return list;

                list = dc.Products.FindSimilarSkuByPattern(m.ManufacturerID, p.ManufacturerPartNumber).Where(e => !string.Equals(e, p.SKU, StringComparison.OrdinalIgnoreCase)).ToList();
                
                return list;
            }
        }


        public NameValueCollection ExtractProductDescriptionValuesFromExtension4()
        {
            var col = new NameValueCollection();

            try
            {
                var excludedKeys = new List<string> { "Wholesale Price", "Note", "Comments", "Product Detail URL", "Unit" };

                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.OriginalRawProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;

                    // remove excluded items
                    excludedKeys.ForEach(e => dic.Remove(e));

                    foreach (var label in InsideFabric.Data.ProductProperties._properties.Intersect(dic.Keys))
                    {
                        col.Add(label, dic[label]);
                        dic.Remove(label);
                    }

                    // pick up any remaining properties (any order) just in case
                    // there were some not on the original list...resulting in 
                    // all properties in the dic being accounted for.

                    foreach (var item in dic)
                        col.Add(item.Key, item.Value);
                }
            }
            catch
            {
            }

            return col;
        }


        #endregion

        #region Local Methods

        /// <summary>
        /// Helper to take an html page and return a parsed DOM.
        /// </summary>
        /// <param name="htmlPage"></param>
        /// <returns></returns>
        private HtmlAgilityPack.HtmlDocument ParseHtmlPage(string htmlPage)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            //htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionUseIdAttribute = true;
            htmlDoc.LoadHtml(htmlPage);

            return htmlDoc;
        }


#if false
        // obsolete - we now use the name/value pairs from extensiondata4

        private NameValueCollection ExtractProductDescriptionValues()
        {
            var col = new NameValueCollection();

            const string document = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title>Untitled Page</title></head><body>{0}</body></html>";

            // notice that we add a correction for bad html - hopefully all corrected in SQL by now
            var html = string.Format(document, p.Description).Replace("<td></tr>", "</td></tr>").Replace("<TD></TD>", "</TD>");

            //html = html.Replace("<br>", string.Empty);

            var doc = ParseHtmlPage(html);

            foreach (var row in doc.DocumentNode.Descendants("tr"))
            {
                try
                {
                    var tagTH = row.Descendants("th").First();
                    var tagTD = row.Descendants("td").First();

                    var name = tagTH.InnerHtml.Trim();

                    if (string.IsNullOrWhiteSpace(name))
                        name = "BlankName";
                    else
                        name = name.HtmlDecode().TitleCase().Replace("*", "");

                    var value = tagTD.InnerHtml.Trim().HtmlDecode().Replace("\x0d", string.Empty).Replace("\x0a", " ").TitleCase();

                    if (value.IndexOf("Additional Product Info") != -1)
                        continue;

                    if (name.IndexOf("Additional Product Info") != -1)
                        continue;

                    if (name == "Brand" && m.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1)
                        continue;


                    if (name.Contains("Sunbrella"))
                        continue;

                    if (value.Contains("Sunbrella"))
                        continue;

                    if (name.Contains("100%"))
                        continue;

                    col.Add(name, value);

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.ToString());
                }
            }

            return col;
        }
#endif

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
            var sb = new StringBuilder(500);

            // this old way extracted the html fields from original description column
            //var col = ExtractProductDescriptionValues();

            var col = ExtractProductDescriptionValuesFromExtension4();

            var designer = Designer;

            if (designer != null)
            {
                sb.AppendFormat("{0} {1} collection.", m.Name, designer);
            }
            else
                sb.AppendFormat("{0} {1}.", m.Name, KindOfProduct.ToString());

            sb.Append(" ");

            // add in all the special stuff
            // skip any name/value pairs that we choose to ignore
            // when writing descriptions.

            int countItems = 0;
            foreach (var key in DescriptionKeepList.Intersect(col.AllKeys))
            {
                var value = col[key];

                if (countItems > 0)
                    sb.Append(", ");

                sb.AppendFormat("{0}: {1}", key, value);

                countItems++;
            }

            var moreAttributes = new List<string>();
            moreAttributes.AddRange(FabricColors);
            moreAttributes.AddRange(FabricTypes);
            moreAttributes.AddRange(FabricPatterns);

            sb.AppendFormat(". {0}.", moreAttributes.ToCommaDelimitedList());

            if (IsWallpaper)
            {
                if (pv.Name == "Roll")
                    sb.Append(" Sold by the roll."); 
            }

            if (IsTrim)
                sb.Append(" Home decororating trim.");

            if (KindOfProduct == ProductKind.Fabric)
                sb.Append(" Sold by the yard. Swatches available.");

            return sb.ToString().Replace("Wallpaper Wallpaper", "Wallpaper").Replace("Fabrics Fabric", "Fabric").Replace("Fabric Fabric", "Fabric").Replace("Fabric Wallpaper", "Wallpaper").Replace("\t", " ").Replace("..", ". ").Replace(".  .", ". ").Replace("\x0d", string.Empty).Replace("\x0a", " ").Replace(". .", ".").Replace(" (137.2 cm)", string.Empty).Replace(", Category: Fabric.", ".");
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
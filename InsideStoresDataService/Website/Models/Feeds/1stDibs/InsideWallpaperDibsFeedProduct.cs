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
    public class InsideWallpaperDibsFeedProduct : DibsFeedProduct
    {
        private const decimal MinimumPricePerUnit = 100m;

        private InsideWallpaperFeedProduct WallpaperFeedProduct
        {
            get
            {
                return StoreFeedProduct as InsideWallpaperFeedProduct;
            }
        }


        protected bool IsOutlet
        {
            get
            {
                return WallpaperFeedProduct.categories.Where(e => e.CategoryID == StoreFeedProduct.Store.OutletCategoryID).Count() > 0;
            }
        }


        private Dictionary<string, string> _properties;
        private Dictionary<string, string> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, string>();
                    var props = WallpaperFeedProduct.ExtractProductDescriptionValuesFromExtension4();
                    foreach (var key in props.AllKeys)
                        _properties[key] = props[key];
                }

                return _properties;
            }
        }

        private string GetPropertyValueOrDefault(string name, string defaultValue = null)
        {
            string value;
            if (Properties.TryGetValue(name, out value))
            {
                return value;
            }

            return defaultValue;
        }

        private static HashSet<string> allowedManufacturers = null;

        /// <summary>
        /// These are the manufacturers allowed to be in the feed for 1stDibs
        /// </summary>
        /// <returns></returns>
        private HashSet<string> GetAllowedManufacturers()
        {
            if (allowedManufacturers == null)
            {
                var list = new List<int>() {
                    93, //Andrew Martin Wallpaper	
                    108, //Baker Lifestyle Wallpaper	
                    76, //Brewster Wallcovering	
                    63, //Clarence House Wallpaper	
                    80, //Clarke & Clarke	
                    109, //Cole & Son Wallpaper	
                    67, //Fabricut Wallpaper	
                    110, //G P & J Baker	
                    111, //Groundworks	
                    107, //Innovations	
                    97, //JF Wallpapers	
                    5, //Kravet Wallpaper	
                    8, //Lee Jofa Wallpaper	
                    56, //Maxwell Wallcovering	
                    112, //Mulberry Home	
                    52, //Ralph Lauren Wallpaper	
                    88, //S. Harris Wallpaper	
                    59, //Scalamandre Wallpaper	
                    30, //Schumacher Wallpaper	
                    69, //Stroheim Wallpaper	
                    115, //Threads	
                    68, //Vervain Wallpaper	
                    74 //York Wallcoverings	
                };

                using (var dc = new AspStoreDataContextReadOnly(StoreFeedProduct.Store.ConnectionString))
                {
                    var names = dc.Manufacturers.Where(e => list.Contains(e.ManufacturerID) && e.Published == 1 && e.Deleted == 0).Select(e => e.Name).ToList();
                    allowedManufacturers = new HashSet<string>(names);
                }
            }
            return allowedManufacturers;
        }

        public InsideWallpaperDibsFeedProduct(InsideWallpaperFeedProduct feedProduct)
            : base(feedProduct)
        {
            try
            {
                if (!StoreFeedProduct.IsValid || StoreFeedProduct.ProductGroup != "Wallcovering" || IsOutlet || StoreFeedProduct.OurPrice < MinimumPricePerUnit || !StoreFeedProduct.IsInStock)
                    return;

                // take only a few brands
                var filterOnVendors = GetAllowedManufacturers();
                if (!filterOnVendors.Contains(StoreFeedProduct.Brand))
                    return;

                Populate();

                IsValid = IsValidFeedProduct(this);
            }
            finally
            {
                StoreFeedProduct = null;
            }
        }


        protected override string MakeTitle()
        {
            return WallpaperFeedProduct.p.Name;
        }

        protected override string MakeContinentOfOrigin()
        {
            return GetPropertyValueOrDefault("Country of Origin", "USA").Replace("USA", "United States").ToInitialCaps();
        }

        protected override string MakeWidth()
        {
            return GetPropertyValueOrDefault("Width", string.Empty).Replace(" inches", "").Replace("\"", "");
        }

        protected override string MakeDepth()
        {
            // replace yards with an inches metric
            var value = GetPropertyValueOrDefault("Length", null);

            if (value == null)
                return string.Empty;

            return value.Replace(" inches", "").Replace("\"", "");
        }

        protected override string MakeMaterials()
        {
            return GetPropertyValueOrDefault("Content", "");
        }

        protected override string MakeCategory()
        {
            return "Furniture > Wall Decorations > Wallpaper";
        }


        /// <summary>
        /// Makes the HTML description.
        /// </summary>
        /// <returns></returns>
        protected override string MakeHtmlDescription()
        {

            var sb = new StringBuilder(1024 * 10);
            var sku = StoreFeedProduct.SKU;

            Action beginContainer = () =>
            {
                sb.Append("<div id='productDescriptionContainer'>");
            };

            Action endContainer = () =>
            {
                sb.Append("</div>");
            };

            try
            {
                // nesting of CSS
                // #productDescriptionContainer
                //    productNoticesContainer
                //        productNotice productOutletNotice productOutletNotice1 - inner P
                //        productNotice productOutletNotice productOutletNotice2 - inner P
                //        productNotice productPackagingNotice - inner P

                //    productDetailsGridContainer
                //        productSoldBy
                //        productDetailsTitle
                //        productDetailsTable


                beginContainer();

                // present HTML in this order:
                // notices
                // sold by units
                // details header
                // details grid


                // notices

                // the entire notices area has a container
                sb.Append("<div class=\"productNoticesContainer\" >");

                string rollSize = null;
                var props = WallpaperFeedProduct.ExtractProductDescriptionValuesFromExtension4();
                if (props.AllKeys.Contains("Packaging"))
                    rollSize = props["Packaging"];

 
                sb.Append("</div>"); // productNoticesContainer

                // the entire table thing has its own container
                sb.Append("<div class=\"productDetailsGridContainer\" >");

                // sold by units
                sb.Append(WallpaperFeedProduct.pv.Name == "Each" ? "<div class=\"productSoldBy\">Sold Individually</div>" : string.Format("<div class=\"productSoldBy\">Sold by the {0}</div>", WallpaperFeedProduct.pv.Name));

                // details header
                sb.Append("<div class=\"productDetailsTitle\"><h2>Product Details</h2></div>");

                // details grid

                // add row to product grid table
                Action<string, string> addRow = (n, v) =>
                {
                    sb.AppendFormat("<tr><th>{0}</th><td>{1}</td></tr>", n.HtmlEncode(), v.HtmlEncode());
                };

                sb.Append("<table class=\"productDetailsTable\">");


                addRow("SKU", StoreFeedProduct.p.SKU);
                addRow("Product Type", StoreFeedProduct.p.ProductGroup.Replace("Trim", "Trimming")); // Fabric|Trimming|Wallcovering
                addRow("Manufacturer", StoreFeedProduct.Brand);

                // categories
                var cats = StoreFeedProduct.categories.Select(e => e.Name).OrderBy(e => e).ToList();
                addRow("Categories", cats.ToCommaDelimitedList());

                // list of properties
                foreach (var key in props.AllKeys)
                    addRow(key, props[key]);

                sb.Append("</table>");

                sb.Append("</div>"); // productDetailsGridContainer
                endContainer();
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                sb.Clear();
                beginContainer();
                sb.Append("<p>Details not available.</p>");
                endContainer();
            }

            return sb.ToString();
        }

    }
}
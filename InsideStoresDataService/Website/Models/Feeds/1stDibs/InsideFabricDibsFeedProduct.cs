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
    public class InsideFabricDibsFeedProduct : DibsFeedProduct
    {
        private const decimal MinimumPricePerUnit = 100m;

        private InsideFabricFeedProduct FabricFeedProduct
        {
            get
            {
                return StoreFeedProduct as InsideFabricFeedProduct;
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
                    var props = FabricFeedProduct.ExtractProductDescriptionValuesFromExtension4();
                    foreach (var key in props.AllKeys)
                        _properties[key] = props[key];
                }

                return _properties;
            }
        }

        private string GetPropertyValueOrDefault(string name, string defaultValue=null)
        {
            string value;
            if (Properties.TryGetValue(name, out value))
            {
                return value;
            }

            return defaultValue;
        }

        protected bool IsOutlet
        {
            get
            {
                return FabricFeedProduct.categories.Where(e => e.CategoryID == StoreFeedProduct.Store.OutletCategoryID).Count() > 0;
            }
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
                    93, //Andrew Martin	
                    73, // B. Berger	
                    108, // Baker Lifestyle	
                    9, // Beacon Hill Fabric	
                    63, // Clarence House 	
                    80, // Clarke & Clarke	
                    11, // Duralee Fabric	
                    67, // Fabricut Fabric	
                    110, // G P & J Baker	
                    111, // Groundworks	
                    19, // Highland Court Fabric	
                    97, // JF Fabrics	
                    5, // Kravet Fabrics	
                    8, // Lee Jofa Fabric	
                    112, // Mulberry Home	
                    52, // Ralph Lauren Fabric	
                    6, // Robert Allen Fabric	
                    88, // S. Harris Fabric	
                    59, // Scalamandre Fabric	
                    30, // Schumacher Fabric	
                    69, // Stroheim Fabric	
                    115, // Threads	
                    70, // Trend Fabric	
                    68 // Vervain Fabric	
                };

                using (var dc = new AspStoreDataContextReadOnly(StoreFeedProduct.Store.ConnectionString))
                {
                    var names = dc.Manufacturers.Where(e => list.Contains(e.ManufacturerID) && e.Published == 1 && e.Deleted == 0).Select(e => e.Name).ToList();
                    allowedManufacturers = new HashSet<string>(names);
                }
            }
            return allowedManufacturers;
        }

        public InsideFabricDibsFeedProduct(InsideFabricFeedProduct feedProduct) : base(feedProduct)
        {
            try
            {
                // note that trim currently excluded, leather excluded

                if (!StoreFeedProduct.IsValid || StoreFeedProduct.ProductGroup != "Fabric" || IsOutlet || StoreFeedProduct.OurPrice < MinimumPricePerUnit || FabricFeedProduct.pv.Name == "Square Foot" || !StoreFeedProduct.IsInStock)
                    return;

                // take only a few brands
                var filterOnVendors = GetAllowedManufacturers();
                if (!filterOnVendors.Contains(StoreFeedProduct.Brand))
                    return;

                //I think we can start with Fabricut, Beacon Hill, JF Fabrics.
                //Those have high MAP and we can make money on.
                //Second tier is the ones we have discounts on, so that would help margin
                //Duralee, Robert Allen, Scalamandre, Schumacher.  I’ll ask David for the rest.

 

                Populate();

                IsValid = IsValidFeedProduct(this);
            }
            finally
            {
                StoreFeedProduct = null;
            }
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
            return "36";
        }


        protected override string MakeMaterials()
        {
            return GetPropertyValueOrDefault("Content", "");
        }

        protected override string MakeCategory()
        {
            return "Furniture > More Furniture and Collectibles > Textiles";
        }



        protected override string MakeTitle()
        {
            return FabricFeedProduct.p.Name;
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


                sb.Append("</div>"); // productNoticesContainer

                // the entire table thing has its own container
                sb.Append("<div class=\"productDetailsGridContainer\" >");

                // sold by units
                sb.Append(FabricFeedProduct.pv.Name == "Each" ? "<div class=\"productSoldBy\">Sold Individually</div>" : string.Format("<div class=\"productSoldBy\">Sold by the {0}</div>", FabricFeedProduct.pv.Name));

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
                var props = FabricFeedProduct.ExtractProductDescriptionValuesFromExtension4();
                foreach (var key in props.AllKeys)
                    addRow(key, props[key]);

                sb.Append("</table>");

                sb.Append("</div>"); // productDetailsGridContainer
                endContainer();
            }
            catch(Exception Ex)
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
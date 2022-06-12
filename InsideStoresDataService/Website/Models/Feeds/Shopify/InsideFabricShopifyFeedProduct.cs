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
    public class InsideFabricShopifyFeedProduct : ShopifyFeedProduct
    {


        private InsideFabricFeedProduct FabricFeedProduct
        {
            get
            {
                return StoreFeedProduct as InsideFabricFeedProduct;
            }
        }

        protected bool IsOutlet
        {
            get
            {
                return FabricFeedProduct.categories.Where(e => e.CategoryID == StoreFeedProduct.Store.OutletCategoryID).Count() > 0;
            }
        }

        /// <summary>
        /// Makes the SEO title.
        /// </summary>
        /// <returns></returns>
        protected override string MakeSEOTitle()
        {
            var title = FabricFeedProduct.p.SETitle;


            if (title.ContainsIgnoreCase(FabricFeedProduct.p.ProductGroup))
                return SwapBrandInName(title);

            return SwapBrandInName(FabricFeedProduct.p.ProductGroup + " " + title);
        }

        /// <summary>
        /// Makes the SEO description.
        /// </summary>
        /// <returns></returns>
        protected override string MakeSEODescription()
        {
            // froogle, so will be different from main site
            var desc = FabricFeedProduct.p.FroogleDescription;

            if (string.IsNullOrWhiteSpace(desc))
                desc = FabricFeedProduct.p.SEDescription;

            return desc;
        }


        public InsideFabricShopifyFeedProduct(InsideFabricFeedProduct feedProduct) : base(feedProduct)
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

        /// <summary>
        /// Makes the handle. Must be unique - used for URL slug by Shopify.
        /// </summary>
        /// <returns></returns>
        protected override string MakeHandle()
        {
            var adjustedSEName = FabricFeedProduct.p.SEName;

            if (!IsSafeFilename(adjustedSEName))
            {
                Debug.WriteLine(string.Format("Invalid url for {0} {1}: {2}", FabricFeedProduct.p.ProductID, FabricFeedProduct.p.Name, FabricFeedProduct.p.SEName));
                adjustedSEName = FabricFeedProduct.m.SEName;
            }

            return string.Format("if-{0}-{1}", FabricFeedProduct.p.ProductID, adjustedSEName);
        }

        protected override string MakeTitle()
        {
            var title = FabricFeedProduct.p.SETitle;

            if (title.ContainsIgnoreCase(FabricFeedProduct.p.ProductGroup))
                return SwapBrandInName(title);

            return SwapBrandInName(FabricFeedProduct.p.ProductGroup + " " + title);
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

                if (IsOutlet)
                {
                    if (FabricFeedProduct.pv.Name == "Yard")
                    {
                        sb.Append("<div class=\"productNotice productOutletNotice productOutletNotice1\"><p><b>OUTLET ITEM:</b> All sales final. No returns or CFAs. Limited stock and will not be reordered. Minimum order 5 yards.</p></div>");
                    }
                    else
                    {
                        sb.Append("<div class=\"productNotice productOutletNotice productOutletNotice1\"><p><b>OUTLET ITEM:</b> All sales final. No returns or CFAs. Limited stock and will not be reordered.</p></div>");
                    }

                    if (FabricFeedProduct.p.MiscText == "Limited Availability")
                    {
                        sb.Append("<div class=\"productNotice productOutletNotice productOutletNotice2\"><p>CLEARANCE ITEM: Limited stock and will not be reordered.</p></div>");
                    }
                }

                switch (FabricFeedProduct.p.ProductGroup)
                {
                    case "Wallcovering":
                        if (FabricFeedProduct.pv.Name == "Roll" && FabricFeedProduct.pv.MinimumQuantity.GetValueOrDefault() < 3 && (sku.StartsWith("CS-") || sku.StartsWith("GP-") || sku.StartsWith("BL-") || sku.StartsWith("MB-")))
                        {
                            // nothing
                            sb.Append("");
                        }
                        if (FabricFeedProduct.pv.Name == "Roll" && FabricFeedProduct.pv.MinimumQuantity.GetValueOrDefault() == 3)
                        {
                            sb.Append("<div  class=\"productNotice productPackagingNotice\"><p>This wallpaper is priced per single roll but is packaged as long triple rolls. Please order in multiples of three rolls (3, 6, 9, etc.).</p><p>This means that if you order 3 single rolls, you will receive 1 long triple roll, since it is packaged as three single rolls per triple roll.  If you order 6, you will receive 2 triple rolls, if you order 9 you will receive 3 triple rolls, etc.</p></div>");
                        }
                        else if (FabricFeedProduct.pv.Name == "Roll")
                        {
                            sb.Append("<div  class=\"productNotice productPackagingNotice\"><p>This wallpaper is priced per single roll but is packaged as long double rolls. Please order in multiples of two rolls (2, 4, 6, etc.).</p><p>This means that if you order 2 single rolls, you will receive 1 long double roll, since it is packaged as two single rolls per double roll.  If you order 4, you will receive 2 double rolls, if you order 8 you will receive 4 double rolls, etc.</p></div>");
                        }
                        break;

                    default: // fabric, trim
                        if (FabricFeedProduct.pv.Name == "Square Foot")
                            sb.Append("<div  class=\"productNotice productPackagingNotice\"><p>Leather is sold by the hide only. An average hide is about 45 square feet but can vary in sizes.  Please contact us to order this item.</p></div>");
                        break;

                }

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

                Func<string> linkText = () =>
                    {
                        return MakeSEOTitle();
                    };

                // add link back to original website
                sb.AppendFormat("<tr><th>{2} Website</th><td><a href=\"{0}\">{1}</a></td></tr>", StoreFeedProduct.ProductPageUrl, linkText().HtmlEncode(), FabricFeedProduct.Store.StoreKey.ToString());

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

        protected override string MakeGoogleProductCategory()
        {
            return FabricFeedProduct.KindOfProduct.Description();
        }

        protected override string MakeAdwordsGrouping()
        {
            return string.Format("{0}:{1}", FabricFeedProduct.BrandKeyword, StoreFeedProduct.ProductGroup).ToLower();
        }

        protected override string MakeAdwordsLabels()
        {
            var tags = StoreFeedProduct.Tags;

            // google apparently limits label count to 10

            if (tags.Count() > 0)
                return tags.Take(10).ToCommaDelimitedList();

            return null;
        }

        protected override string MakeGoogleShoppingCustomLabel1()
        {
            // unit of measure
            return FabricFeedProduct.pv.Name;
        }

        protected override string MakeGoogleShoppingCustomLabel2()
        {
            // has been checked to exactly match website

            // minimum:increment

            // fabric rules are simple:
            // if stated min qty is null, 0 or 1, then is 1, else stated value
            // if Ext2 has some value, then use it

            // it could be that some lower-priced items have a min qty more than 1
            // clearance items would typically have min set to 5

            int increment = 1;
            if (!string.IsNullOrWhiteSpace(FabricFeedProduct.pv.ExtensionData))
            {
                if (!int.TryParse(FabricFeedProduct.pv.ExtensionData, out increment))
                    increment = 1;
            }
            
            // if gt 1, use it, else assume 1
            var minimum = FabricFeedProduct.pv.MinimumQuantity.GetValueOrDefault() > 1 ? FabricFeedProduct.pv.MinimumQuantity.Value : 1;
            return string.Format("{0}:{1}", minimum, increment);
        }

    }
}
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
    public class InsideAvenueShopifyFeedProduct : ShopifyFeedProduct
    {


        private InsideAvenueFeedProduct AvenueFeedProduct
        {
            get
            {
                return StoreFeedProduct as InsideAvenueFeedProduct;
            }
        }


        protected bool IsOutlet
        {
            get
            {
                return AvenueFeedProduct.categories.Where(e => e.CategoryID == StoreFeedProduct.Store.OutletCategoryID).Count() > 0;
            }
        }


        /// <summary>
        /// Makes the SEO title.
        /// </summary>
        /// <returns></returns>
        protected override string MakeSEOTitle()
        {
            return string.Format("{0} | {1}",  AvenueFeedProduct.p.SETitle, AvenueFeedProduct.PrimaryCategoryName);
        }

        /// <summary>
        /// Makes the SEO description.
        /// </summary>
        /// <returns></returns>
        protected override string MakeSEODescription()
        {
            var sb = new StringBuilder(1024);

            sb.AppendFormat("Shop {0}. ", AvenueFeedProduct.PrimaryCategoryName);
            sb.Append(AvenueFeedProduct.p.Name);
            sb.Append(".");

            var color = AvenueFeedProduct.Color;
            if (!string.IsNullOrWhiteSpace(color))
                sb.AppendFormat(" Color: {0}.", color);

            var size = AvenueFeedProduct.Size;
            if (!string.IsNullOrWhiteSpace(size))
                sb.AppendFormat(" Size: {0}.", size);

            sb.Append(" Free shipping.");

            return sb.ToString();
        }


        public InsideAvenueShopifyFeedProduct(InsideAvenueFeedProduct feedProduct)
            : base(feedProduct)
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

        protected override string MakeProductType()
        {
            return AvenueFeedProduct.PrimaryCategoryName;
        }

        /// <summary>
        /// Makes the handle. Must be unique - used for URL slug by Shopify.
        /// </summary>
        /// <returns></returns>
        protected override string MakeHandle()
        {
            var adjustedSEName = AvenueFeedProduct.p.SEName;

            if (!IsSafeFilename(adjustedSEName))
            {
                Debug.WriteLine(string.Format("Invalid url for {0} {1}: {2}", AvenueFeedProduct.p.ProductID, AvenueFeedProduct.p.Name, AvenueFeedProduct.p.SEName));
                adjustedSEName = AvenueFeedProduct.m.SEName;
            }

            return string.Format("ia-{0}-{1}", AvenueFeedProduct.p.ProductID, adjustedSEName);
        }

        protected override string MakeTitle()
        {
            return AvenueFeedProduct.p.SETitle;
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

                //    productDetailsGridContainer
                //        productDetailsTable


                beginContainer();


                // the entire notices area has a container
                //sb.Append("<div class=\"productNoticesContainer\" >");
                //sb.Append("</div>"); // productNoticesContainer

                var descriptionText = AvenueFeedProduct.ExtractProductDescriptionTextFromExtension4();
                if (descriptionText != null)
                {

                    sb.Append("<div class=\"productsDetailDescription\">");

                    foreach (var p in descriptionText)
                        sb.AppendFormat("<p>{0}</p>", HttpUtility.HtmlEncode(p));

                    sb.Append("</div>");
                }

                // the entire table thing has its own container
                sb.Append("<div class=\"productDetailsGridContainer\" >");

                // details header
                //sb.Append("<div class=\"productDetailsTitle\"><h2>Product Details</h2></div>");

                // details grid

                // add row to product grid table
                Action<string, string> addRow = (n, v) =>
                {
                    sb.AppendFormat("<tr><th>{0}</th><td>{1}</td></tr>", n.HtmlEncode(), v.HtmlEncode());
                };

                sb.Append("<table class=\"productDetailsTable\">");


                addRow("SKU", StoreFeedProduct.p.SKU);
                addRow("Manufacturer", StoreFeedProduct.Brand);

                // categories
                addRow("Categories", AvenueFeedProduct.CategoryAncestorList.ToCommaDelimitedList());

                // list of properties
                var props = AvenueFeedProduct.ExtractProductDescriptionValuesFromExtension4();
                foreach (var key in props.AllKeys)
                    addRow(key, props[key]);


                Func<string> linkText = () =>
                {
                    return AvenueFeedProduct.p.Name;
                };

                // add link back to original website
                sb.AppendFormat("<tr><th>{2} Website</th><td><a href=\"{0}\">{1}</a></td></tr>", StoreFeedProduct.ProductPageUrl, linkText().HtmlEncode(), AvenueFeedProduct.Store.StoreKey.ToString());


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

        protected override string MakeTags()
        {
            return AvenueFeedProduct.CategoryAncestorList.ToCommaDelimitedList();
        }

        protected override string MakeGoogleProductCategory()
        {
            return AvenueFeedProduct.GoogleProductCategory;
        }

        protected override string MakeAdwordsGrouping()
        {
            return string.Empty;
        }

        protected override string MakeAdwordsLabels()
        {
            var tags = StoreFeedProduct.Tags;

            // google apparently limits label count to 10

            if (tags.Count() > 0)
                return tags.Take(10).ToCommaDelimitedList();

            return null;
        }

        protected override string MakeBarCode()
        {
            return AvenueFeedProduct.UPC;
        }

        protected override string MakeGoogleShoppingCustomLabel1()
        {
            return "Each";
        }

        protected override string MakeGoogleShoppingCustomLabel2()
        {
            // has been checked to exactly match website

            // minimum:increment

            int minimum;
            int increment;
            
            increment = 1;
            if (!string.IsNullOrWhiteSpace(AvenueFeedProduct.pv.ExtensionData))
            {
                if (!int.TryParse(AvenueFeedProduct.pv.ExtensionData, out increment))
                    increment = 1;
            }

            // if gt 1, use it, else assume 1
            minimum = AvenueFeedProduct.pv.MinimumQuantity.GetValueOrDefault() > 1 ? AvenueFeedProduct.pv.MinimumQuantity.Value : 1;

            return string.Format("{0}:{1}", minimum, increment);
        }

    }
}
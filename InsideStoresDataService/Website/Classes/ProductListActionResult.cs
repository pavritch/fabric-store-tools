using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security;
using InsideStores.Imaging;

namespace Website
{
    /// <summary>
    /// Output XML for the specified list of products.
    /// </summary>
    /// <remarks>
    /// Includes product XML and page count of entire result set since
    /// that count is often needed to build data pager controls.
    /// </remarks>
    public class ProductListActionResult : ActionResult
    {
        private List<CacheProduct> productList;
        private int pageCount;
        private string rootElementName;

        public ProductListActionResult(List<CacheProduct> productList, int pageCount, string rootElementName = "Products")
        {
            this.productList = productList;
            this.pageCount = pageCount;
            this.rootElementName = rootElementName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml"; 

            StringBuilder sbXml = new StringBuilder(25000);

            sbXml.AppendFormat("<{0} pages=\"{1}\">", rootElementName, pageCount);

            if (productList != null && productList.Count() > 0)
            {
                foreach (var product in productList)
                    sbXml.Append(MakeProductXml(product));
            }

            sbXml.AppendFormat("</{0}>", rootElementName);

            var s = sbXml.ToString();

            context.HttpContext.Response.Write(s);
        }

        private string GetManufacturerName(CacheProduct product)
        {
            var  store = product.GetWebStore();
            // can be null for cross marketing product lists
            return store != null ? store.LookupManufacturerName(product.ManufacturerID) : string.Empty;
        }

        protected virtual string MakePrice(decimal price)
        {
            return string.Format("{0:c}", price);
        }

        protected virtual StringBuilder MakeProductXml(CacheProduct product)
        {
            StringBuilder sbProduct = new StringBuilder(1000);
            sbProduct.Append("<Product>\n");
            sbProduct.AppendFormat("   <ProductID>{0}</ProductID>\n", product.ProductID);
            sbProduct.AppendFormat("   <SKU>{0}</SKU>\n", SecurityElement.Escape(product.SKU));
            sbProduct.AppendFormat("   <Name>{0}</Name>\n", SecurityElement.Escape(product.Name));
            //sbProduct.AppendFormat("   <AlternateName>{0}</AlternateName>\n", SecurityElement.Escape(product.AlternateName != null ? product.AlternateName : product.Name));
            sbProduct.AppendFormat("   <ImageUrl>{0}</ImageUrl>\n", product.ImageUrl);
            sbProduct.AppendFormat("   <ImageFilename>{0}</ImageFilename>\n", product.ImageFilename ?? string.Empty);
            sbProduct.AppendFormat("   <ProductUrl>{0}</ProductUrl>\n", product.ProductUrl);
            sbProduct.AppendFormat("   <StockStatus>{0}</StockStatus>\n", product.StockStatus.ToString());
            sbProduct.AppendFormat("   <ManufacturerID>{0}</ManufacturerID>\n", product.ManufacturerID.ToString());
            sbProduct.AppendFormat("   <ManufacturerName>{0}</ManufacturerName>\n", SecurityElement.Escape(GetManufacturerName(product)));

            // temporarily not new: rugs and homeware
            if (product.GetType() == typeof(InsideRugsCacheProduct) || product.GetType() == typeof(InsideAvenueCacheProduct))
                sbProduct.AppendFormat("   <New>{0}</New>\n", false.ToString());
            else
                sbProduct.AppendFormat("   <New>{0}</New>\n", product.IsNew.ToString());

            if (product.GetType() == typeof(InsideAvenueCacheProduct))
            {
                var iaProduct = product as InsideAvenueCacheProduct;
                sbProduct.AppendFormat("   <CategoryID>{0}</CategoryID>\n", iaProduct.CategoryID.HasValue ? iaProduct.CategoryID.Value.ToString() : string.Empty);
            }

            sbProduct.AppendFormat("   <IsMissingImage>{0}</IsMissingImage>\n", product.IsMissingImage.ToString());
            sbProduct.AppendFormat("   <Outlet>{0}</Outlet>\n", product.IsOutlet.ToString());
            sbProduct.AppendFormat("   <ProductGroup>{0}</ProductGroup>\n", product.ProductGroup.HasValue ? product.ProductGroup.DescriptionAttr() : string.Empty);

            // this would be the default variant
            sbProduct.AppendFormat("   <RetailPrice>{0}</RetailPrice>\n", MakePrice(product.RetailPrice));
            sbProduct.AppendFormat("   <OurPrice>{0}</OurPrice>\n", MakePrice(product.OurPrice));
            sbProduct.AppendFormat("   <SalePrice>{0}</SalePrice>\n", product.SalePrice.HasValue ? MakePrice(product.SalePrice.GetValueOrDefault()) : string.Empty);
            sbProduct.AppendFormat("   <Discount>{0}</Discount>\n", product.Discount);
            sbProduct.AppendFormat("   <DiscountGroup>{0}</DiscountGroup>\n", product.DiscountGroup);
            sbProduct.AppendFormat("   <Discontinued>{0}</Discontinued>\n", product.IsDiscontinued.ToString());

            // price ranges for all variants of this product
            sbProduct.AppendFormat("   <LowVariantRetailPrice>{0}</LowVariantRetailPrice>\n", MakePrice(product.LowVariantRetailPrice));
            sbProduct.AppendFormat("   <HighVariantRetailPrice>{0}</HighVariantRetailPrice>\n", MakePrice(product.HighVariantRetailPrice));
            sbProduct.AppendFormat("   <LowVariantOurPrice>{0}</LowVariantOurPrice>\n", MakePrice(product.LowVariantOurPrice));
            sbProduct.AppendFormat("   <HighVariantOurPrice>{0}</HighVariantOurPrice>\n", MakePrice(product.HighVariantOurPrice));

            // will only be filled in when IsImageSearchEnabled
            if (product.Colors != null)
            {
                // make a string like:  #FF00FF;#0000FF;#808080
                sbProduct.AppendFormat("   <Colors>{0}</Colors>\n", product.Colors.ToRGBColorsString());
            }
            else
                sbProduct.AppendLine("   <Colors />");

            // will only be filled in when IsCorrelatedProductsEnabled is enabled
            if (product.CorrelatedProducts != null)
            {
                // the TOTAL here is the true total, since we only give out the first three here
                sbProduct.AppendFormat("   <CorrelatedProducts Total=\"{0}\">", product.CorrelatedProducts.Count() - 1); // less self because the list is inclusive

                // take up to the first three, not including self

                foreach (var cp in product.CorrelatedProducts.Where(e => e.ProductID != product.ProductID && !e.IsDiscontinued && !e.IsMissingImage).Take(3))
                {
                    sbProduct.AppendLine("       <CProduct>");

                    sbProduct.AppendFormat("           <ProductID>{0}</ProductID>\n", cp.ProductID);
                    sbProduct.AppendFormat("           <Name>{0}</Name>\n", SecurityElement.Escape(cp.Name));
                    sbProduct.AppendFormat("           <ProductUrl>{0}</ProductUrl>\n", cp.ProductUrl);
                    sbProduct.AppendFormat("           <ImageUrl>{0}</ImageUrl>\n", cp.ImageUrl);

                    sbProduct.AppendLine("       </CProduct>");
                }

                sbProduct.AppendLine("   </CorrelatedProducts>");
            }
            else
            {
                sbProduct.AppendLine("   <CorrelatedProducts Total=\"0\" />");
            }

            // specific to rugs

            if (product.GetType() == typeof(InsideRugsCacheProduct))
            {
                var irProduct = product as InsideRugsCacheProduct;
                sbProduct.AppendFormat("   <Shapes>{0}</Shapes>\n", irProduct.Shapes ?? string.Empty);
            }

            sbProduct.AppendLine("</Product>");

            return sbProduct;
        }
    }
}
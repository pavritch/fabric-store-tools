using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security;

namespace Website
{
    /// <summary>
    /// Output XML for the specified list of affiliate products.
    /// </summary>
    /// <remarks>
    /// Includes product XML and page count of entire result set since
    /// that count is often needed to build data pager controls.
    /// </remarks>
    public class AffiliateProductListActionResult : ActionResult
    {
        private List<AffiliateProduct> productList;
        private string rootElementName;

        public AffiliateProductListActionResult(List<AffiliateProduct> productList,  string rootElementName = "Products")
        {
            this.productList = productList;
            this.rootElementName = rootElementName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml"; 

            StringBuilder sbXml = new StringBuilder(25000);

            sbXml.AppendFormat("<{0}>", rootElementName);

            if (productList != null && productList.Count() > 0)
            {
                foreach (var product in productList)
                    sbXml.Append(MakeProductXml(product));
            }

            sbXml.AppendFormat("</{0}>", rootElementName);

            var s = sbXml.ToString();

            context.HttpContext.Response.Write(s);
        }

        protected virtual string MakePrice(decimal price)
        {
            return string.Format("{0:c}", price);
        }

        protected virtual StringBuilder MakeProductXml(AffiliateProduct product)
        {
            StringBuilder sbProduct = new StringBuilder(1000);

            sbProduct.Append("<Product>\n");
            sbProduct.AppendFormat("   <ProductID>{0}</ProductID>\n", product.ProductID.ToString());
            sbProduct.AppendFormat("   <Name>{0}</Name>\n", SecurityElement.Escape(product.Name));
            sbProduct.AppendFormat("   <ImageUrl>{0}</ImageUrl>\n", SecurityElement.Escape(product.ImageUrl));
            sbProduct.AppendFormat("   <ProductUrl>{0}</ProductUrl>\n", SecurityElement.Escape(product.ProductUrl));
            sbProduct.AppendFormat("   <Price>{0}</Price>\n", MakePrice(product.Price));
            sbProduct.Append("</Product>\n");

            return sbProduct;
        }
    }
}
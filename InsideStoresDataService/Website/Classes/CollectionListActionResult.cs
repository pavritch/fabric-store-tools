using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security;
using Website.Entities;

namespace Website
{
    /// <summary>
    /// Output XML for the specified list of collections.
    /// </summary>
    /// <remarks>
    /// Includes product XML and page count of entire result set since
    /// that count is often needed to build data pager controls.
    /// </remarks>
    public class CollectionListActionResult : ActionResult
    {
        private readonly List<ProductCollection> CollectionList;
        private readonly int pageCount;
        private readonly string rootElementName;

        public CollectionListActionResult(List<ProductCollection> CollectionList, int pageCount, string rootElementName = "Collections")
        {
            this.CollectionList = CollectionList;
            this.pageCount = pageCount;
            this.rootElementName = rootElementName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml"; 

            StringBuilder sbXml = new StringBuilder(25000);

            sbXml.AppendFormat("<{0} pages=\"{1}\">", rootElementName, pageCount);

            if (CollectionList != null && CollectionList.Count() > 0)
            {
                foreach (var col in CollectionList)
                    sbXml.Append(MakeCollectionXml(col));
            }

            sbXml.AppendFormat("</{0}>", rootElementName);

            var s = sbXml.ToString();

            context.HttpContext.Response.Write(s);
        }

        protected virtual string MakePrice(decimal price)
        {
            return string.Format("{0:c}", price);
        }

        protected virtual string MakePriceRange(decimal lowPrice, decimal highPrice)
        {
            if (lowPrice == highPrice)
                return MakePrice(lowPrice);

            return string.Format("{0:c} - {1:c} ", lowPrice, highPrice);
        }

        private string MakeImageUrl(string imageFilenameOverride, string kind)
        {
            if (string.IsNullOrWhiteSpace(imageFilenameOverride))
                return string.Format("/images/missing-{0}-collection-image.jpg", kind).ToLower();
            return string.Format("/images/collections/{0}", imageFilenameOverride).ToLower();
        }

        private string MakeCollectionUrl(ProductCollection productCollection)
        {
            return string.Format("/{0}-{1}-{2}.aspx", GetKind(productCollection.Kind), productCollection.ProductCollectionID, productCollection.SEName).ToLower();
        }

        private string GetKind(int kind)
        {
            switch(kind)
            {
                case 0: return "Book";
                case 1: return "Pattern";
                case 2: return "Collection";
            }

            return "Unknown";
        }

        protected virtual StringBuilder MakeCollectionXml(ProductCollection productCollection)
        {
            var createdAfter = DateTime.Now.AddDays(0 - 90);
            var isNew = productCollection.CreatedOn >= createdAfter;

            StringBuilder sbProduct = new StringBuilder(1000);
            sbProduct.Append("<Collection>\n");
            sbProduct.AppendFormat("   <CollectionID>{0}</CollectionID>\n", productCollection.ProductCollectionID);
            sbProduct.AppendFormat("   <Kind>{0}</Kind>\n", GetKind(productCollection.Kind));
            sbProduct.AppendFormat("   <ManufacturerID>{0}</ManufacturerID>\n", productCollection.ManufacturerID);
            sbProduct.AppendFormat("   <Name>{0}</Name>\n", SecurityElement.Escape(productCollection.Name));
            sbProduct.AppendFormat("   <ShortName>{0}</ShortName>\n", SecurityElement.Escape(productCollection.ShortName));
            sbProduct.AppendFormat("   <SEName>{0}</SEName>\n", SecurityElement.Escape(productCollection.SEName));
            sbProduct.AppendFormat("   <SETitle>{0}</SETitle>\n", SecurityElement.Escape(productCollection.SETitle));
            sbProduct.AppendFormat("   <SEDescription>{0}</SEDescription>\n", SecurityElement.Escape(productCollection.SEDescription));
            sbProduct.AppendFormat("   <ProductGroup>{0}</ProductGroup>\n", SecurityElement.Escape(productCollection.ProductGroup));
            sbProduct.AppendFormat("   <CountTotal>{0}</CountTotal>\n", productCollection.CountTotal.ToString("N0"));
            sbProduct.AppendFormat("   <CountLive>{0}</CountLive>\n", (productCollection.CountTotal - productCollection.CountDiscontinued).ToString("N0"));
            sbProduct.AppendFormat("   <CountDiscontinued>{0}</CountDiscontinued>\n", productCollection.CountDiscontinued.ToString("N0"));
            sbProduct.AppendFormat("   <CountNoStock>{0}</CountNoStock>\n", productCollection.CountNoStock.ToString("N0"));
            sbProduct.AppendFormat("   <HighPrice>{0}</HighPrice>\n", MakePrice(productCollection.HighPrice));
            sbProduct.AppendFormat("   <LowPrice>{0}</LowPrice>\n", MakePrice(productCollection.LowPrice));
            sbProduct.AppendFormat("   <PriceRange>{0}</PriceRange>\n", MakePriceRange(productCollection.LowPrice, productCollection.HighPrice));
            sbProduct.AppendFormat("   <ImageUrl>{0}</ImageUrl>\n", MakeImageUrl(productCollection.ImageFilenameOverride, GetKind(productCollection.Kind)));
            sbProduct.AppendFormat("   <CollectionUrl>{0}</CollectionUrl>\n", MakeCollectionUrl(productCollection));
            sbProduct.AppendFormat("   <New>{0}</New>\n", isNew.ToString());
            sbProduct.Append("</Collection>\n");

            return sbProduct;
        }
    }
}
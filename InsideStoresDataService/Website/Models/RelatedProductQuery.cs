using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class RelatedProductQuery : ProductQueryBase
    {
        public int ProductID { get; set; }
        public int ParentCategoryID { get; set; }

        public RelatedProductQuery(int productID, int parentCategoryID)
        {
            QueryMethod = QueryRequestMethods.ListRelatedProducts;
            this.ProductID = productID;
            this.ParentCategoryID = parentCategoryID;
        }
    }
}
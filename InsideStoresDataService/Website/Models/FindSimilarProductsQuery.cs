using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Image search
    /// </summary>
    public class FindSimilarProductsQuery : ProductQueryBase
    {
        public int ProductID { get; set; }

        public FindSimilarProductsQuery(int ProductID, QueryRequestMethods method)
        {
            Debug.Assert(method == QueryRequestMethods.FindSimilarProducts
                || method == QueryRequestMethods.FindSimilarProductsByTexture || method == QueryRequestMethods.FindSimilarProductsByColor);

            QueryMethod = method;
            this.ProductID = ProductID;
        }
    }
}
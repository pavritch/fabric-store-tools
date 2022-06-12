using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class ProductSetProductQuery : ProductQueryBase
    {
        public List<int> ProductsSet { get; set; }

        public ProductSetProductQuery(List<int> productsSet)
        {
            QueryMethod = QueryRequestMethods.ProductSet;
            this.ProductsSet = productsSet;
        }
    }
}
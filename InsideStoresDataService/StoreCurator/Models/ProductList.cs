using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public class ProductList
    {
        // these are needed for the pager control
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalResults { get; set; }

        public List<ProductSummary> Products { get; set; }
    }
}
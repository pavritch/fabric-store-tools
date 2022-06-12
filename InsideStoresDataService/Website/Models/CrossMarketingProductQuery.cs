using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class CrossMarketingProductQuery : ProductQueryBase
    {
        public string ReferenceIdentifier { get; set; }
        public bool AllowResultsFromSelf { get; set; }

        public CrossMarketingProductQuery(string referenceIdentifier, bool allowResultsFromSelf)
        {
            ReferenceIdentifier = referenceIdentifier ?? string.Empty;
            AllowResultsFromSelf = allowResultsFromSelf;

            QueryMethod = QueryRequestMethods.CrossMarketingProducts;
        }
    }
}
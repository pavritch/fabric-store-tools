using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InsideFabric.Data;

namespace Website
{


    public class FacetSearchProductQuery : ProductQueryBase
    {
        public FacetSearchCriteria Criteria { get; set; }

        public FacetSearchProductQuery(FacetSearchCriteria criteria)
        {
            QueryMethod = QueryRequestMethods.FacetSearch;
            this.Criteria = criteria;
        }
    }
}
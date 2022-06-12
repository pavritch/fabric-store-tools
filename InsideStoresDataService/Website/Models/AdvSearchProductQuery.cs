using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class AdvSearchProductQuery : ProductQueryBase
    {
        public SearchCriteria Criteria;

        public AdvSearchProductQuery(SearchCriteria criteria)
        {
            QueryMethod = QueryRequestMethods.AdvancedSearch;
            this.Criteria = criteria;
        }
    }
}
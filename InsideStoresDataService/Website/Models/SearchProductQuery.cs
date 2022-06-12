using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class SearchProductQuery : ProductQueryBase
    {
        public string SearchPhrase { get; set; }
        public List<int> RecentlyViewed { get; set; }

        public SearchProductQuery(string searchPhrase, List<int> recentlyViewed=null)
        {
            QueryMethod = QueryRequestMethods.Search;
            this.SearchPhrase = searchPhrase;
            this.RecentlyViewed = recentlyViewed;
        }
    }
}
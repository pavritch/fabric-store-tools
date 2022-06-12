using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsideFabric.Data
{
    
    public class FacetItem
    {
        public string FacetKey { get; set; }
        public List<int> Members { get; set; }

        public FacetItem()
        {
            Members = new List<int>();
        }

        public FacetItem(FacetItem other)
        {
            FacetKey = other.FacetKey;
            Members = other.Members.Select(e => e).ToList();
        }
    }

    /// <summary>
    /// A facet query contains an optional search phrase plus 0-N facet items.
    /// Everything is AND'd together for the search.
    /// </summary>
    public class FacetSearchCriteria
    {
        public string SearchPhrase { get; set; }
        public List<int> RecentlyViewed { get; set; }
        public List<FacetItem> Facets { get; set; }
        public int SerialNumber { get; set; }

        public FacetSearchCriteria()
        {
            SearchPhrase = null;
            RecentlyViewed = new List<int>();
            Facets = new List<FacetItem>();
            SerialNumber = 0;
        }

        public FacetSearchCriteria(FacetSearchCriteria other)
        {
            SearchPhrase = other.SearchPhrase;
            SerialNumber = other.SerialNumber;
            RecentlyViewed = other.RecentlyViewed.Select(e => e).ToList();
            // note that there is an embedded list in chid list objects
            Facets = other.Facets.Select(e => new FacetItem(e)).ToList();
        }

    }
}

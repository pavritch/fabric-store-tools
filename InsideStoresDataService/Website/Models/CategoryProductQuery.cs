using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class CategoryProductQuery : ProductQueryBase
    {
        public int CategoryID { get; set; }

        /// <summary>
        /// If true, then the FilterByxxxx properties will be filled in and will not be null.
        /// </summary>
        public bool IsFiltered { get; set; }

        // lists of category IDs
        public List<int> FilterByTypeList { get; set; }
        public List<int> FilterByPatternList { get; set; }
        public List<int> FilterByColorList { get; set; }
        // list of manufacturer IDs
        public List<int> FilterByBrandList { get; set; }

        // when using filters:
        // the base category (the one the page is on right now) must have its ID included
        // in one of the category filter lists - and for that specific list, that one category ID 
        // should be the only member. This makes it easier to morph into an advanced search when
        // filters are applied.

        public CategoryProductQuery()
        {
            IsFiltered = false;
            FilterByTypeList = null;
            FilterByPatternList = null;
            FilterByColorList = null;
            FilterByBrandList = null;
        }

        public CategoryProductQuery(int categoryID) : this()
        {
            QueryMethod = QueryRequestMethods.ListByCategory;
            this.CategoryID = categoryID;
        }
    }
}
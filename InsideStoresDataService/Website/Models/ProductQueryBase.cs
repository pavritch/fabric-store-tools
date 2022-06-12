using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Baseline typesafe object to submit query for a set of products.
    /// </summary>
    /// <remarks>
    /// Each kind of query has its own needed parameters - and should derrive 
    /// from this class to add in additional typesafe parameters.
    /// </remarks>
    public class ProductQueryBase : IProductQuery
    {
        public string RequestUrl { get; set; } 

        /// <summary>
        /// Indicates the kind of query - list by category, manufacturer, search, etc.
        /// </summary>
        public QueryRequestMethods QueryMethod { get; set; }

        // common parameters

        public int PageNo { get; set; } // 1-rel
        public int PageSize { get; set; }
        public ProductSortOrder OrderBy { get; set; }

        /// <summary>
        /// Method to call upon completion. List of products plus total page count.
        /// </summary>
        /// <remarks>
        /// List of products will never be null, but it can be empty list.
        /// These match the inputs needed by the ProductListActionResult().
        /// </remarks>
        public Action<List<CacheProduct>, int> CompletedAction { get; set; }

        public ProductQueryBase()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                RequestUrl = HttpContext.Current.Request.Url.OriginalString;
            }

        }
    }
}
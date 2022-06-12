using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;

namespace Website
{
    /// <summary>
    /// Baseline typesafe object to submit query books or patterns.
    /// </summary>
    /// <remarks>
    /// Each kind of query has its own needed parameters - and should derrive 
    /// from this class to add in additional typesafe parameters.
    /// </remarks>
    public class CollectionQueryBase : IProductCollectionQuery
    {
        public string RequestUrl { get; set; } 

        /// <summary>
        /// Indicates the kind of query - list by category, manufacturer, search, etc.
        /// </summary>
        public QueryRequestMethods QueryMethod { get; set; }

        // common parameters

        public int PageNo { get; set; } // 1-rel
        public int PageSize { get; set; }

        /// <summary>
        /// Method to call upon completion. List of collection result members plus total page count.
        /// </summary>
        public Action<List<ProductCollection>, int> CompletedAction { get; set; }

        public CollectionQueryBase()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                RequestUrl = HttpContext.Current.Request.Url.OriginalString;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Single query/list/search request to be submitted to the IWebStore
    /// </summary>
    public interface IProductQuery : IQueryRequest
    {
        // common parameters

        int PageNo { get; } // 1-rel
        int PageSize { get; }
        ProductSortOrder OrderBy { get; }

        /// <summary>
        /// Method to call upon completion. List of products plus total page count.
        /// </summary>
        /// <remarks>
        /// List of products will never be null, but it can be empty list.
        /// These match the inputs needed by the ProductListActionResult().
        /// </remarks>
        Action<List<CacheProduct>, int> CompletedAction { get; }
    }
}
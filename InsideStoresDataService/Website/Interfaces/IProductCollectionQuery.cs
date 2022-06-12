using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;

namespace Website
{
    /// <summary>
    /// Single query/list/search request to be submitted to the IWebStore
    /// </summary>
    public interface IProductCollectionQuery : IQueryRequest
    {
        // common parameters

        int PageNo { get; } // 1-rel
        int PageSize { get; }

        /// <summary>
        /// Method to call upon completion. List of products plus total page count.
        /// </summary>
        /// <remarks>
        /// List of collections will never be null, but it can be empty list.
        /// These match the inputs needed by the ProductListActionResult().
        /// </remarks>
        Action<List<ProductCollection>, int> CompletedAction { get; }
    }
}
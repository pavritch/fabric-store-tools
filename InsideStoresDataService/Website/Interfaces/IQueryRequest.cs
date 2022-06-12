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
    public interface IQueryRequest
    {
        /// <summary>
        /// Indicates the kind of query - list by category, manufacturer, search, etc.
        /// </summary>
        QueryRequestMethods QueryMethod { get; }
    }
}
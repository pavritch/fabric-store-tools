using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Creates and manages category filters under the Filters (named) top level category.
    /// </summary>
    /// <remarks>
    /// For maintenance tasks only. Not runtime web serving.
    /// </remarks>
    public interface ICategoryFilterManager
    {
        int CategoryID { get; }
        string Name { get; }
        Guid CategoryGUID { get; }
        void Classify(object product);

        /// <summary>
        /// All leaf node (subcategories under first level roots) CategoryID.
        /// </summary>
        /// <remarks>
        /// Specifically - this would be the complete list of IDs actually used for ProductCategory assoc.
        /// </remarks>
        HashSet<int> AllCategoryFilters { get; }

        /// <summary>
        /// Buld rebulding of color filters using a global-centric rather than product centric approach.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <param name="reportProgress"></param>
        /// <param name="reportStatus"></param>
        void RebuildColorFilters(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null);
    }
}
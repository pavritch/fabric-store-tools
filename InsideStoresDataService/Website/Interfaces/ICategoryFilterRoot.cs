using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Top level root category (Patterns, Brands, etc.) under Filters parent.
    /// </summary>
    /// <remarks>
    /// For maintenance tasks only. Not runtime web serving.
    /// </remarks>
    public interface ICategoryFilterRoot<TProduct> where TProduct : class
    {
        int CategoryID { get; }
        string Name { get; }
        Guid CategoryGUID { get; }
        void Initialize(int parentCategoryID);
        IEnumerable<ICategoryFilter<TProduct>> Children { get; }
        void Classify(TProduct product);

        /// <summary>
        /// All leaf node (subcategories under first level roots) CategoryID.
        /// </summary>
        /// <remarks>
        /// Specifically - this would be the complete list of IDs actually used for ProductCategory assoc.
        /// </remarks>
        HashSet<int> AllCategoryFilters { get; }
    }
}
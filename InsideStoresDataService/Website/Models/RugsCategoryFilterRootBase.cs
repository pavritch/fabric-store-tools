//------------------------------------------------------------------------------
// 
// Class: CategoryFilterRootBase 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace Website
{
    public class RugsCategoryFilterRootBase : CategoryFilterBase
    {
        private CategoryFilterInformation categoryInfo;
        private int parentCategoryID;
        private List<ICategoryFilter<InsideRugsProduct>> _children;
        private HashSet<int> _allCategoryFilters;

        public RugsCategoryFilterRootBase(IWebStore Store)
            : base(Store)
        {


        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<ICategoryFilter<InsideRugsProduct>> Children
        {
            get
            {
                return _children;
            }
        }

        public int CategoryID
        {
            get
            {
                return categoryInfo.CategoryID;
            }
        }

        public string Name
        {
            get
            {
                return categoryInfo.Name;
            }
        }

        public Guid CategoryGUID
        {
            get
            {
                return categoryInfo.CategoryGUID;
            }
        }

        /// <summary>
        /// All leaf node (subcategories under first level roots) CategoryID.
        /// </summary>
        /// <remarks>
        /// Specifically - this would be the complete list of IDs actually used for ProductCategory assoc.
        /// </remarks>
        public HashSet<int> AllCategoryFilters
        {
            get
            {
                return _allCategoryFilters;
            }
        }

        public virtual void Initialize(int parentCategoryID, CategoryFilterInformation categoryInfo, IEnumerable<CategoryFilterInformation> childrenCategoryInfo)
        {
            // make sure all the GUIDs are unique
            Debug.Assert(childrenCategoryInfo.Select(e => e.CategoryGUID.ToString()).Distinct().Count() == childrenCategoryInfo.Count());

            // make sure all the names are unique
            Debug.Assert(childrenCategoryInfo.Select(e => e.Name).Distinct().Count() == childrenCategoryInfo.Count());

            // make sure all the sename are unique
            Debug.Assert(childrenCategoryInfo.Select(e => e.SEName.ToLower()).Distinct().Count() == childrenCategoryInfo.Count());

            // if all the display order is 0, then assign in order of appearance
            if (childrenCategoryInfo.All(e => e.DisplayOrder == 0))
            {
                int order = 1;
                foreach(var child in childrenCategoryInfo)
                    child.DisplayOrder = order++;
            }

            _children = new List<ICategoryFilter<InsideRugsProduct>>();
            this.parentCategoryID = parentCategoryID;
            this.categoryInfo = categoryInfo;

            // because this comes from SQL rather than fixed within the code.
            categoryInfo.ParentCategoryID = this.parentCategoryID;

            categoryInfo.CategoryID = EnsureCategoryExists(categoryInfo);

            if (categoryInfo.CategoryID == 0)
                throw new Exception("Unable to ensure filter category root: " + categoryInfo.Name);

            foreach(var child in childrenCategoryInfo)
            {
                var c = FilterFactory(child);
                _children.Add(c);
                c.Initialize();
            }

            // clean up any categories that we no longer need
            RemoveUnprotectedCategories(CategoryID, Children.Select(e => e.CategoryID).ToList());

            _allCategoryFilters = new HashSet<int>(Children.Select(e => e.CategoryID));
        }

        /// <summary>
        /// Gives subclass opportunity to instantiate classes on a custom basis.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="childClassifyCallback"></param>
        /// <returns></returns>
        protected virtual RugsCategoryFilter FilterFactory(CategoryFilterInformation info)
        {
            var c = new RugsCategoryFilter(Store, CategoryID, info);
            return c;
        }
    }
}
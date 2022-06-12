//------------------------------------------------------------------------------
// 
// Class: CategoryFilterManager 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class CategoryFilterManagerBase<TProduct> : CategoryFilterBase where TProduct : class
    {
        /// <summary>
        /// This root node is never intended to be directly presented on websites.
        /// </summary>
        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            CategoryGUID = Guid.Parse("{E5DFA7F2-F9CE-49F7-8EC5-A190FCF189FA}"),
            ParentCategoryID = 0,
            Name = "Filters",
            Title = "Filters",
            DisplayOrder = 1,
            SEName = "filters",
        };

        private List<ICategoryFilterRoot<TProduct>> _rootCategories;
        private HashSet<int> _allFilterCategories;

        public IEnumerable<ICategoryFilterRoot<TProduct>> RootCategories
        {
            get
            {
                return _rootCategories;
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

        public void Classify(TProduct product)
        {
            foreach(var child in RootCategories)
            {
                child.Classify(product);
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
                return _allFilterCategories;
            }
        }

        public CategoryFilterManagerBase(IWebStore Store) : base(Store)
        {
            _rootCategories = new List<ICategoryFilterRoot<TProduct>>();
        }

        protected void Initialize(IEnumerable<ICategoryFilterRoot<TProduct>> rootCategories)
        {
            categoryInfo.CategoryID = EnsureCategoryExists(categoryInfo);

            if (categoryInfo.CategoryID == 0)
                throw new Exception("Unable to ensure (create) root filters category.");

            foreach (var c in rootCategories)
            {
                _rootCategories.Add(c);
                // passing our very top root CategoryID so the child root nodes know who their parent is.
                c.Initialize(CategoryID);
            }

            // clean up any categories that we no longer need
            RemoveUnprotectedCategories(CategoryID, RootCategories.Select(e => e.CategoryID).ToList());

            _allFilterCategories = new HashSet<int>();
            foreach (var root in RootCategories)
                _allFilterCategories.UnionWith(root.AllCategoryFilters);
        }

    }
}
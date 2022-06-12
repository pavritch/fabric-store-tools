//------------------------------------------------------------------------------
// 
// Class: RugCategoryFilter
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class RugsCategoryFilter : CategoryFilterBase, ICategoryFilter<InsideRugsProduct>
    {
        private CategoryFilterInformation categoryInfo;
        private int parentCategoryID;

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
        /// Classifies the specified product.
        /// </summary>
        /// <remarks>
        /// Not all roots actually will call down to leaf nodes. Each root decides how best to do its processing.
        /// Some advanced leaf nodes will override this method.
        /// </remarks>
        /// <param name="product">The product.</param>
        public virtual void Classify(InsideRugsProduct product)
        {

        }

        public RugsCategoryFilter(IWebStore Store, int parentCategoryID, CategoryFilterInformation categoryInfo)
            : base(Store)
        {
            this.parentCategoryID = parentCategoryID;
            this.categoryInfo = categoryInfo;

            // because this comes from SQL rather than fixed within the code.
            categoryInfo.ParentCategoryID = this.parentCategoryID;
        }

        public void Initialize()
        {
            categoryInfo.CategoryID = EnsureCategoryExists(categoryInfo);

            if (categoryInfo.CategoryID == 0)
                throw new Exception("Unable to ensure filter category: " + categoryInfo.Name);
        }

    }
}
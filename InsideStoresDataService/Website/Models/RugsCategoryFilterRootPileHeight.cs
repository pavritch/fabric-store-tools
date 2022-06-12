//------------------------------------------------------------------------------
// 
// Class: RugsCategoryFilterRootBase
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class RugsCategoryFilterRootPileHeight : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {

        #region PileHeightFilter Class
        private class PileHeightFilter : CategoryFilterInformation
        {
            private double maxHeight;

            public PileHeightFilter(double maxHeight)
            {
                this.maxHeight = maxHeight;
            }

            public bool IsMatch(double pileHeight)
            {
                return pileHeight <= maxHeight;
            }
        }
        #endregion

        #region Root Category Definition
        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{48C64BAD-FA86-43A6-98A8-FEC12DC7C5ED}"),
            Name = "Pile Height",
            Title = "Rugs by Pile Height",
            DisplayOrder = 6,
            SEName = "rugs-by-pile-height",
        }; 
        #endregion

        #region Child Definitions

        private static List<PileHeightFilter> children = new List<PileHeightFilter>()
        {
            // the variants for a given main product frequently have different 
            // pile heights - so could easily end up being more than one choice here.
            // Take the union selections across all variants.

            //Flat  (under 1/4")
            //Low  (1/4" to 1/2")
            //Medium (1/2" to 1")
            //High (over 1")

            // must be listed in order of height

            //Flat  (under 1/4")
		    new PileHeightFilter(.2499)
            {
                CategoryGUID = Guid.Parse("{731C5BB3-BAFF-4256-9C77-E7F2459164FD}"),
                Name = "Flat  (under 1/4\")",
                Title = "Flat Pile Rugs (under 1/4\")",
                Keywords = "flat pile",
                SEName = "flat-pile-rugs",
            },

            //Low  (1/4" to 1/2")
		    new PileHeightFilter(.5)
            {
                CategoryGUID = Guid.Parse("{06624970-0FBF-4A5E-8093-4EB7C9C11507}"),
                Name = "Low  (1/4\" to 1/2\")",
                Title = "Low Pile Rugs (1/4\" to 1/2\")",
                Keywords = "low pile",
                SEName = "low-pile-rugs",
            },

            //Medium (1/2" to 1")
		    new PileHeightFilter(1.0)
            {
                CategoryGUID = Guid.Parse("{387B1A20-298E-4AA5-B127-9116A42F5497}"),
                Name = "Medium (1/2\" to 1\")",
                Title = "Medium Pile Rugs (1/2\" to 1\")",
                Keywords = "medium pile",
                SEName = "medium-pile-rugs",
            },

            //High (over 1")
		    new PileHeightFilter(double.MaxValue)
            {
                CategoryGUID = Guid.Parse("{53495E03-A2EB-43AD-9C83-B05650A3745B}"),
                Name = "High (over 1\")",
                Title = "High Pile Rugs (over 1\")",
                Keywords = "high pile",
                SEName = "high-pile-rugs",
            },


        };
        #endregion

        #region Initialization

        public RugsCategoryFilterRootPileHeight(IWebStore Store)
            : base(Store)
        {

        }

        public void Initialize(int parentCategoryID)
        {
            Initialize(parentCategoryID, categoryInfo, children);
        }
        
        #endregion

        #region Classify
        /// <summary>
        /// Classifies the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        public virtual void Classify(InsideRugsProduct product)
        {
            // caller has already removed all ProductCategory associations

            // keeps track of associations, hash to make sure no duplicates
            var memberCategories = new HashSet<int>();

            foreach (var vf in product.VariantFeatures)
            {
                // early out?
                if (!vf.PileHeight.HasValue || string.IsNullOrWhiteSpace(vf.Shape) || vf.Shape.Equals("Sample", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                foreach (var child in children)
                {
                    if (child.IsMatch(vf.PileHeight.Value))
                    {
                        // the list of children is required to be in order of height, and we take
                        // the first match

                        memberCategories.Add(child.CategoryID);
                        break;
                    }
                }
            }

            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }
        #endregion

    }
}
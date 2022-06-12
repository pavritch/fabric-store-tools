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
    public class RugsCategoryFilterRootPrice : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region PriceFilter Class
        private class PriceFilter : CategoryFilterInformation
        {
            private decimal minPrice;
            private decimal maxPrice;

            public PriceFilter(decimal min, decimal max)
            {
                this.minPrice = min;
                this.maxPrice = max;
            }

            public bool IsMatch(decimal price)
            {
                return price >= minPrice && price <= maxPrice;
            }
        } 
        #endregion

        #region Root Category Definition
        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{C332CE2F-C19A-4FDB-A5DA-185EB9BFD9C9}"),
            Name = "Price",
            Title = "Rugs by Price",
            DisplayOrder = 5,
            SEName = "rugs-by-price",
        }; 
                #endregion

        #region Child Definitions
        // the categoryID and parentCategoryID are updated by the runtime.
        private static List<PriceFilter> children = new List<PriceFilter>()
            {
                //Under $199
                //$200 to $399
                //$400 to $599
                //$600 to $799
                //$800 to $999
                //$1,000 to $1,999
                //$2,000 and up

		        new PriceFilter(0M, 199.99M)
                {
                    CategoryGUID = Guid.Parse("{982B63F4-857A-4AE2-BFD8-361B215267C4}"),
                    Name = "Under $199",
                    Title = "Rugs Under $199",
                    Keywords = "",
                    SEName = "rugs-under-199",
                },

                new PriceFilter(200M, 399.99M)
                {
                    CategoryGUID = Guid.Parse("{7AD1E67A-F7EA-4621-83BB-1921A4D38F26}"),
                    Name = "$200 to $399",
                    Title = "Rugs $200 to $399",
                    Keywords = "",
                    SEName = "rugs-200-to-399",
                },


                new PriceFilter(400M, 599.99M)
                {
                    CategoryGUID = Guid.Parse("5F050730-C003-4DF2-B403-9629CD3B0013"),
                    Name = "$400 to $599",
                    Title = "Rugs $400 to $599",
                    Keywords = "",
                    SEName = "rugs-400-to-599",
                },


                new PriceFilter(600M, 799.99M)
                {
                    CategoryGUID = Guid.Parse("{61277873-F828-4845-B352-D58D71AE4B88}"),
                    Name = "$600 to $799",
                    Title = "Rugs $600 to $799",
                    Keywords = "",
                    SEName = "rugs-600-to-799",
                },

                new PriceFilter(800M, 999.99M)
                {
                    CategoryGUID = Guid.Parse("{4241B5B1-ABC2-4FB8-8C3C-63B98B471196}"),
                    Name = "$800 to $999",
                    Title = "Rugs $800 to $999",
                    Keywords = "",
                    SEName = "rugs-800-to-999",
                },

                new PriceFilter(1000M, 1999.99M)
                {
                    CategoryGUID = Guid.Parse("{57EEA4EF-591D-4962-AA74-74577DC63C39}"),
                    Name = "$1,000 to $1,999",
                    Title = "Rugs $1,000 to $1,999",
                    Keywords = "",
                    SEName = "rugs-1000-to-1999",
                },

                new PriceFilter(2000M, Decimal.MaxValue)
                {
                    CategoryGUID = Guid.Parse("{AE01F6F5-F31F-4E92-858D-EACA73FA03CB}"),
                    Name = "$2,000 and up",
                    Title = "Rugs $2,000 and Up",
                    Keywords = "",
                    SEName = "rugs-over-2000",
                }, 
            };
        #endregion

        #region Initialization
        public RugsCategoryFilterRootPrice(IWebStore Store)
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
            // caller has already removed all ProductCategory associations, so no need to do it here.
            // However, here is how it would be correctly done at this level - to clear out all associations for this root.
            // RemoveProductCategoryAssociationsForProduct(product.p.ProductID, AllCategoryFilters);

            // keeps track of associations, hash to make sure no duplicates
            var memberCategories = new HashSet<int>();

            // if on sale, use the sale price, else the normal price
            // note that samples are ignored here when establishing price filter memberships

            foreach (var price in product.variants.Where(e => !string.Equals(e.Dimensions, "Sample", StringComparison.OrdinalIgnoreCase)).Select(e => e.SalePrice ?? e.Price))
            {
                // just in case, but don't expect to hit unless some problem
                if (price < 5M)
                    continue;

                foreach (var child in children)
                {
                    if (child.IsMatch(price))
                    {
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
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
    public class RugsCategoryFilterRootShape : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region ShapeFilter Class
        private class ShapeFilter : CategoryFilterInformation
        {
            private string shape;

            public ShapeFilter(InsideFabric.Data.ImageVariantType shape)
            {
                this.shape = shape.DescriptionAttr();
            }

            public bool IsMatch(string productShape)
            {
                return string.Equals(shape, productShape, StringComparison.OrdinalIgnoreCase);
            }
        }
        #endregion

        #region Root Category Definition
        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{AE8A8762-06E8-464E-9E7F-6303FE8EA47B}"),
            Name = "Shape",
            Title = "Rugs by Shape",
            DisplayOrder = 4,
            SEName = "rugs-by-shape",
        }; 
        #endregion

        #region Child Definitions

        private static List<ShapeFilter> children  = new List<ShapeFilter>()
        {
            // the categoryID and parentCategoryID are updated by the runtime.

            //Rectangular
            //Square
            //Oval
            //Round
            //Octagon
            //Runner
            //Star
            //Heart
            //Kidney
            //Animal
            //Novelty


            //Rectangular
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Rectangular)
            {
                CategoryGUID = Guid.Parse("{EE80ED53-2E9B-4611-B4F7-59C1514AF9F2}"),
                Name = "Rectangular",
                Title = "Rectangular Rugs",
                Keywords = "rectangular, rectangle",
                SEName = "rectangular-rugs",
            },

            //Square
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Square)
            {
                CategoryGUID = Guid.Parse("{B1C380A9-2E76-4317-B2D7-850D10964B1D}"),
                Name = "Square",
                Title = "Square Rugs",
                Keywords = "square",
                SEName = "square-rugs",
            },

            //Oval
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Oval)
            {
                CategoryGUID = Guid.Parse("{D9A853F2-6189-45F4-9E7E-336736129D9A}"),
                Name = "Oval",
                Title = "Oval Rugs",
                Keywords = "oval",
                SEName = "oval-rugs",
            },

            //Round
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Round)
            {
                CategoryGUID = Guid.Parse("{B2F483E7-0C61-43F0-AF04-2E8A2C5D66AF}"),
                Name = "Round",
                Title = "Round Rugs",
                Keywords = "round",
                SEName = "round-rugs",
            },

            //Octagon
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Octagon)
            {
                CategoryGUID = Guid.Parse("{A073DDAA-F3CA-49E4-A048-DE6B40340CD3}"),
                Name = "Octagon",
                Title = "Octagon Shaped Rugs",
                Keywords = "octagon",
                SEName = "octagon-rugs",
            },

            //Runner
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Runner)
            {
                CategoryGUID = Guid.Parse("{A4B13DE9-F9B1-4834-BAA4-8694DE472968}"),
                Name = "Runner",
                Title = "Runner Rugs",
                Keywords = "runner",
                SEName = "runner-rugs",
            },

            //Star
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Star)
            {
                CategoryGUID = Guid.Parse("{AF35525D-EFB2-4E13-95C0-54EB00C85EE5}"),
                Name = "Star",
                Title = "Star Shaped Rugs",
                Keywords = "star shape",
                SEName = "star-shaped-rugs",
            },

            //Heart
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Heart)
            {
                CategoryGUID = Guid.Parse("{C74F3CCD-AFBD-4BBE-9757-C31C7B4F6DE0}"),
                Name = "Heart",
                Title = "Heart Shaped Rugs",
                Keywords = "heart shape",
                SEName = "heart-shaped-rugs",
            },

            //Kidney
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Kidney)
            {
                CategoryGUID = Guid.Parse("{7DC6CDD4-A758-4EDB-8014-F2BF5E9A9B6B}"),
                Name = "Kidney",
                Title = "Kidney Shaped Rugs",
                Keywords = "kidney shape",
                SEName = "kidney-shaped-rugs",
            },

            //Animal
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Animal)
            {
                CategoryGUID = Guid.Parse("{AC080A32-0B12-4AFD-AAD9-01C83097C8FE}"),
                Name = "Animal",
                Title = "Animal Shaped Rugs",
                Keywords = "animal shape",
                SEName = "animal-shaped-rugs",
            },

            //Novelty
		    new ShapeFilter(InsideFabric.Data.ImageVariantType.Novelty)
            {
                CategoryGUID = Guid.Parse("{8ED73056-9DA8-4C27-94B7-B7702F2A8CF4}"),
                Name = "Novelty",
                Title = "Novelty Shaped Rugs",
                Keywords = "novelty shape",
                SEName = "novelty-shaped-rugs",
            },


        };
        #endregion

        #region Initialization

        public RugsCategoryFilterRootShape(IWebStore Store)
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

            foreach (var shape in product.variants.Select(e => e.Dimensions))
            {
                // early out?
                if (string.IsNullOrWhiteSpace(shape) || shape.Equals("Sample", StringComparison.OrdinalIgnoreCase) || shape.Equals("Other", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var child in children)
                {
                    if (child.IsMatch(shape))
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
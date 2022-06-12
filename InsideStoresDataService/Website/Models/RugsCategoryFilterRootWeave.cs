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
    public class RugsCategoryFilterRootWeave : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region WeaveFilter Class
        private class WeaveFilter : CategoryFilterInformation
        {
            private HashSet<string> matchSet = new HashSet<string>();

            // simple matching mechanism until final concepts put in place

            public WeaveFilter(string matchList)
            {
                foreach (var s in matchList.ParseCommaDelimitedList())
                    matchSet.Add(s.ToLower());                
            }

            public bool IsMatch(string weave)
            {
#if true
                // fuzzy match
                // tag must be lower, not use word rugs
                foreach (var phrase in matchSet)
                {
                    if (weave.ContainsIgnoreCase(phrase))
                        return true;
                    
                    if (phrase.LevenshteinDistance(weave) <= 2)
                        return true;
                }

                return false;
#else
                // exact match
                return matchSet.Contains(weave.ToLower());
#endif
            }
        }

        #endregion

        #region Root Category Definition

        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{F84273F4-9FA9-4E7C-8608-38B158D841BF}"),
            Name = "Weave",
            Title = "Rugs by Weave",
            DisplayOrder = 8,
            SEName = "rugs-by-weave",
        };


        #endregion

        #region Child Definitions

        private static List<WeaveFilter> children = new List<WeaveFilter>()
        {
            //Braided
		    new WeaveFilter("braided, braid")
            {
                CategoryGUID = Guid.Parse("{2AA71BEB-A23F-416B-9F93-4DF348582AA3}"),
                Name = "Braided",
                Title = "Braided Rugs",
                Keywords = "braided",
                SEName = "braided-rugs",
            },

            //Hand Woven
		    new WeaveFilter("hand woven")
            {
                CategoryGUID = Guid.Parse("{B43E392D-067B-403E-9FAE-663784C9EDC7}"),
                Name = "Hand Woven",
                Title = "Hand Woven Rugs",
                Keywords = "hand woven",
                SEName = "hand-woven-rugs",
            },

            //Hand Tufted
		    new WeaveFilter("hand tufted")
            {
                CategoryGUID = Guid.Parse("{D53F4061-C2D3-4874-84BB-795AEB9E2921}"),
                Name = "Hand Tufted",
                Title = "Hand Tufted Rugs",
                Keywords = "hand tufted",
                SEName = "hand-tufted-rugs",
            },

            //Hand Knotted
		    new WeaveFilter("hand knotted")
            {
                CategoryGUID = Guid.Parse("{AB342071-94FA-4DD9-A2BF-E02EB7E13DA8}"),
                Name = "Hand Knotted",
                Title = "Hand Knotted Rugs",
                Keywords = "hand knotted",
                SEName = "hand-knotted-rugs",
            },

            //Hand Hooked
		    new WeaveFilter("hand hooked")
            {
                CategoryGUID = Guid.Parse("{9CB65F59-D8C5-4487-91C4-5035A1AF51A5}"),
                Name = "Hand Hooked",
                Title = "Hand Hooked Rugs",
                Keywords = "hand hooked",
                SEName = "hand-hooked-rugs",
            },

            //Hand Looped
		    new WeaveFilter("hand looped")
            {
                CategoryGUID = Guid.Parse("{C7887A06-4E41-4A52-9C6C-5167225B19FE}"),
                Name = "Hand Looped",
                Title = "Hand Looped Rugs",
                Keywords = "hand looped",
                SEName = "hand-looped-rugs",
            },

            //Hand Loomed
		    new WeaveFilter("hand loomed")
            {
                CategoryGUID = Guid.Parse("{55BEBEC2-C207-4095-B2B4-AB4C455F5104}"),
                Name = "Hand Loomed",
                Title = "Hand Loomed Rugs",
                Keywords = "hand loomed",
                SEName = "hand-loomed-rugs",
            },

            //Hand Crafted
		    new WeaveFilter("hand")
            {
                CategoryGUID = Guid.Parse("{5B8B922F-B112-4B07-801D-EB3ADF20726C}"),
                Name = "Hand Crafted",
                Title = "Hand Crafted Rugs",
                Keywords = "hand crafted",
                SEName = "hand-crafted-rugs",
            },

            //Flat Woven
		    new WeaveFilter("flat woven, flatweave, flat")
            {
                CategoryGUID = Guid.Parse("{B2C038B3-23D1-4018-B6A8-D480AAE729A2}"),
                Name = "Flat Woven",
                Title = "Flat Woven Rugs",
                Keywords = "flat woven",
                SEName = "flat-woven-rugs",
            },

            //Machine Tufted
		    new WeaveFilter("machine tufted")
            {
                CategoryGUID = Guid.Parse("{34F56FBD-2ADC-4349-BCCF-A27B76200324}"),
                Name = "Machine Tufted",
                Title = "Machine Tufted Rugs",
                Keywords = "machine tufted",
                SEName = "machine-tufted-rugs",
            },

            //Machine Woven
		    new WeaveFilter("machine woven")
            {
                CategoryGUID = Guid.Parse("{45E764EB-610E-46D7-981A-914686F7A0CE}"),
                Name = "Machine Woven",
                Title = "Machine Woven Rugs",
                Keywords = "machine woven",
                SEName = "machine-woven-rugs",
            },

            //Machine Made
		    new WeaveFilter("machine")
            {
                CategoryGUID = Guid.Parse("{2FCF4BA9-48E1-4EF6-9253-BE2625221305}"),
                Name = "Machine Made",
                Title = "Machine Made Rugs",
                Keywords = "machine made",
                SEName = "machine-made-rugs",
            },

            //Power Loomed
		    new WeaveFilter("power loomed")
            {
                CategoryGUID = Guid.Parse("{8317A65B-F221-4274-8162-37D3973BC1F7}"),
                Name = "Power Loomed",
                Title = "Power Loomed Rugs",
                Keywords = "power loomed",
                SEName = "power-loomed-rugs",
            },

            //Shag
		    new WeaveFilter("shag, shaggy")
            {
                CategoryGUID = Guid.Parse("{DE23AE48-7DB6-4FAF-9A3B-07824368A85E}"),
                Name = "Shag",
                Title = "Shag Rugs",
                Keywords = "shag",
                SEName = "shag-rugs",
            },

        };
        #endregion

        #region Initialization
        public RugsCategoryFilterRootWeave(IWebStore Store)
            : base(Store)
        {


        }

        public void Initialize(int parentCategoryID)
        {
            // expecting that all the variants would have the same choice here so any
            // single product would fall into only one choice


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


            if (product.Features == null || string.IsNullOrWhiteSpace(product.Features.Weave))
                return;

            var cleanWeave = product.Features.Weave.ToLower();
            foreach (var child in children)
            {
                if (child.IsMatch(cleanWeave))
                    memberCategories.Add(child.CategoryID);
            }

            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }
        #endregion
    }
}
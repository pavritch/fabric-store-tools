//------------------------------------------------------------------------------
// 
// Class: RugsCategoryFilterRootStyle
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class RugsCategoryFilterRootStyle : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region StyleFilter Class
        private class StyleFilter : CategoryFilterInformation
        {
            private HashSet<string> matchSet;

            public StyleFilter(string matchList)
            {
                this.matchSet = new HashSet<string>();
                foreach (var s in matchList.ParseCommaDelimitedList())
                    matchSet.Add(s.ToLower());
            }

            public bool IsMatch(string tag)
            {
#if true
                // fuzzy match
                // tag must be lower, not use word rugs
                foreach(var phrase in matchSet)
                {
                    if (phrase.LevenshteinDistance(tag) <= 1)
                        return true;
                }

                return false;
#else
                // exact match
                return matchSet.Contains(color.ToLower());
#endif
            }

        }
        #endregion

        #region Root Category Definition
        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{BF5DA437-147E-4376-8D75-5392E96D445F}"),
            Name = "Style",
            Title = "Rugs by Style",
            DisplayOrder = 1,
            SEName = "rugs-by-style",
        };


        
        #endregion

        #region Child Definitions

        private List<StyleFilter> children = new List<StyleFilter>()
        {
            // although the choices would tend to be identical amongst the variants
            // for a given main product, it is very possible that a given product
            // will fit more than one choice.

            //Contemporary
            //Transitional
            //Traditional
            //Southwestern
            //Solid
            //Striped
            //Geometric
            //Casuals
            //Outdoor
            //Shags
            //Country
            //Floral & Plants
            //Kids
            //Novelty
            //Animal Print
            //Vintage
            //Abstract
            //Border
            //Oriental
            //Moroccan
            //Damask
            //Classic
            //Modern
            //Trellis
            //Paisley
            //Braided
            //Ikat
            //Gradient
            //Plush
            //Tribal
            //Mid-Century
            //Industrial
            //Preppy
            //Glam
            //Rustic
            //Tropical
            //Eastern
            //Chevron
            //Tibetan 
            //Plaid
            //Polka Dots
            //Camouflage 
            //Beach / Nautical
            //Sports
            //Letters & Numbers
            //Global Inspirational

            //Contemporary
		    new StyleFilter ("contemporary")
            {
                CategoryGUID = Guid.Parse("{1DE36026-1AAB-4749-9148-BC23AFEECDA4}"),
                Name = "Contemporary",
                Title = "Contemporary Rugs",
                Keywords = "contemporary",
                SEName = "contemporary-rugs",
            },

            //Transitional
		    new StyleFilter ("transitional")
            {
                CategoryGUID = Guid.Parse("{1570567B-C2A2-4D33-9E01-A25E380687F4}"),
                Name = "Transitional",
                Title = "Transitional Rugs",
                Keywords = "transitional",
                SEName = "transitional-rugs",
            },

            //Traditional
		    new StyleFilter ("traditional")
            {
                CategoryGUID = Guid.Parse("{718B5E91-998A-418F-AF23-4F528EF55D6C}"),
                Name = "Traditional",
                Title = "Traditional Rugs",
                Keywords = "traditional",
                SEName = "traditional-rugs",
            },

            //Southwestern
		    new StyleFilter ("southwestern, southwest")
            {
                CategoryGUID = Guid.Parse("{A5DD6872-687F-4D60-B9DA-BE1C844617B3}"),
                Name = "Southwestern",
                Title = "Southwestern Rugs",
                Keywords = "southwestern",
                SEName = "southwestern-rugs",
            },

            //Solid
		    new StyleFilter ("solid, solid color, solids")
            {
                CategoryGUID = Guid.Parse("{8F2DFE0C-ECAB-480B-8445-36A7C37D27D5}"),
                Name = "Solid",
                Title = "Solid Color Rugs",
                Keywords = "solid",
                SEName = "solid-color-rugs",
            },

            //Striped
		    new StyleFilter ("striped, stripes, stripe")
            {
                CategoryGUID = Guid.Parse("{4A97386A-CC83-4451-B405-578317A91657}"),
                Name = "Striped",
                Title = "Striped Rugs",
                Keywords = "striped",
                SEName = "striped-rugs",
            },

            //Geometric
		    new StyleFilter ("geometric")
            {
                CategoryGUID = Guid.Parse("{EFFB0E54-8FD9-40C1-99B4-AE74D3692C99}"),
                Name = "Geometric",
                Title = "Geometric Pattern Rugs",
                Keywords = "geometric",
                SEName = "geometric-pattern-rugs",
            },

            //Casuals
		    new StyleFilter ("casual, casuals")
            {
                CategoryGUID = Guid.Parse("{557B8FEA-DCC6-48E4-ABB5-54F433BAF487}"),
                Name = "Casuals",
                Title = "Casual Rugs",
                Keywords = "casual",
                SEName = "casual-rugs",
            },

            //Outdoor
		    new StyleFilter ("outdoor, indoor / outdoor, indoor/outdoor, indoor-outdoor, interior/exterior")
            {
                CategoryGUID = Guid.Parse("{E4A8B210-0C51-4FBA-9F0A-F707C09D956D}"),
                Name = "Outdoor",
                Title = "Outdoor Rugs",
                Keywords = "outdoor",
                SEName = "outdoor-rugs",
            },

            //Shags
		    new StyleFilter ("shag, shags")
            {
                CategoryGUID = Guid.Parse("{67B775A8-76BF-41E5-8A8C-55814F70D85F}"),
                Name = "Shags",
                Title = "Shag Rugs",
                Keywords = "shag",
                SEName = "shag-rugs",
            },

            //Country
		    new StyleFilter ("country, cottage, craft, crafts")
            {
                CategoryGUID = Guid.Parse("{69201063-BE3D-4D18-A9E0-964266EDA876}"),
                Name = "Country",
                Title = "Country Style Rugs",
                Keywords = "country",
                SEName = "country-style-rugs",
            },

            //Floral & Plants
		    new StyleFilter ("floral, plants, plant, botanical, botanical, flowers, floral & plants, foliage")
            {
                CategoryGUID = Guid.Parse("{11C254F7-BB29-44F5-9E77-03E95FF11C05}"),
                Name = "Floral & Plants",
                Title = "Floral & Plants Rugs",
                Keywords = "floral",
                SEName = "floral-rugs",
            },

            //Kids
		    new StyleFilter ("kid, kids, child, childrens, kids rugs, baby")
            {
                CategoryGUID = Guid.Parse("{6F3EAD00-CA41-48F8-A89B-66D22AE221DB}"),
                Name = "Kids",
                Title = "Kids Rugs",
                Keywords = "kids",
                SEName = "kids-rugs",
            },

            //Novelty
		    new StyleFilter ("novelty")
            {
                CategoryGUID = Guid.Parse("{B2FA3F8C-198E-417A-8DE1-F8FDF5D6C301}"),
                Name = "Novelty",
                Title = "Novelty Rugs",
                Keywords = "novelty",
                SEName = "novelty-rugs",
            },

            //Animal Print
		    new StyleFilter ("animal print, animal")
            {
                CategoryGUID = Guid.Parse("{B8722876-154B-4AC1-8D4B-1331EF8E4A1A}"),
                Name = "Animal Print",
                Title = "Animal Print Rugs",
                Keywords = "animal print",
                SEName = "animal-print-rugs",
            },

            //Vintage
		    new StyleFilter ("vintage")
            {
                CategoryGUID = Guid.Parse("{862DD7A7-2207-4918-845E-17E9C1BCE61C}"),
                Name = "Vintage",
                Title = "Vintage Rugs",
                Keywords = "vintage",
                SEName = "vintage-rugs",
            },

            //Abstract
		    new StyleFilter ("abstract")
            {
                CategoryGUID = Guid.Parse("{3B66EF93-A95E-4244-A2A4-4C7A1E4944C3}"),
                Name = "Abstract",
                Title = "Abstract Design Rugs",
                Keywords = "abstract",
                SEName = "abstract-rugs",
            },

            //Border
		    new StyleFilter ("border, borders, boarder") // because found spelled wrong in vendor data
            {
                CategoryGUID = Guid.Parse("{9337F4D8-90DB-4DE5-A178-85A3CAFCF80A}"),
                Name = "Border",
                Title = "Rugs with Borders",
                Keywords = "border",
                SEName = "rugs-with-borders",
            },

            //Oriental
		    new StyleFilter ("oriental")
            {
                CategoryGUID = Guid.Parse("{834B7560-E2A6-4DFA-9F2A-73FD2976D941}"),
                Name = "Oriental",
                Title = "Oriental Rugs",
                Keywords = "oriental",
                SEName = "oriental-rugs",
            },

            //Moroccan
		    new StyleFilter ("moroccan")
            {
                CategoryGUID = Guid.Parse("{6C059018-75EA-45DD-9460-67FA84CA4100}"),
                Name = "Moroccan",
                Title = "Moroccan Rugs",
                Keywords = "moroccan",
                SEName = "moroccan-rugs",
            },

            //Damask
		    new StyleFilter ("damask, damasks")
            {
                CategoryGUID = Guid.Parse("{C7BC4B63-FDB0-42B2-9B86-AA2DF6158A4F}"),
                Name = "Damask",
                Title = "Damask Rugs",
                Keywords = "damask",
                SEName = "damask-rugs",
            },


            //Classic
		    new StyleFilter ("classic")
            {
                CategoryGUID = Guid.Parse("{F15A0E37-3185-4A54-AC37-161EE0DE509D}"),
                Name = "Classic",
                Title = "Classic Rugs",
                Keywords = "classic",
                SEName = "classic-rugs",
            },

            //Modern
		    new StyleFilter ("modern")
            {
                CategoryGUID = Guid.Parse("{F9AE1081-5711-4361-A846-44B5786BE1E2}"),
                Name = "Modern",
                Title = "Modern Rugs",
                Keywords = "modern",
                SEName = "modern-rugs",
            },

            //Trellis
		    new StyleFilter ("trellis")
            {
                CategoryGUID = Guid.Parse("{CA58214C-3159-41E1-90F6-D46D146E4E7A}"),
                Name = "Trellis",
                Title = "Trellis Rugs",
                Keywords = "trellis",
                SEName = "trellis-rugs",
            },

            //Paisley
		    new StyleFilter ("paisley")
            {
                CategoryGUID = Guid.Parse("{56FC26E9-AFF4-49AD-9261-FBB9E0BD7B06}"),
                Name = "Paisley",
                Title = "Paisley Rugs",
                Keywords = "paisley",
                SEName = "paisley-rugs",
            },

            //Braided
		    new StyleFilter ("braided")
            {
                CategoryGUID = Guid.Parse("{F722000A-C4B0-40E6-9032-A3CA3A64D806}"),
                Name = "Braided",
                Title = "Braided Rugs",
                Keywords = "braided",
                SEName = "braided-rugs",
            },

            //Ikat
		    new StyleFilter ("ikat")
            {
                CategoryGUID = Guid.Parse("{A712DF71-7E6B-4E1A-B53B-B1A4BA8FA290}"),
                Name = "Ikat",
                Title = "Ikat Rugs",
                Keywords = "ikat",
                SEName = "ikat-rugs",
            },

            //Gradient
		    new StyleFilter ("gradient")
            {
                CategoryGUID = Guid.Parse("{EB241830-0926-472A-9017-F4C1E0A466F0}"),
                Name = "Gradient",
                Title = "Gradient Design Rugs",
                Keywords = "gradient",
                SEName = "gradient-design-rugs",
            },

            //Plush
		    new StyleFilter ("plush")
            {
                CategoryGUID = Guid.Parse("{EC53514A-F1E1-4EE2-A672-727852F3A580}"),
                Name = "Plush",
                Title = "Plush Rugs",
                Keywords = "plush",
                SEName = "plush-rugs",
            },

            //Tribal
		    new StyleFilter ("tribal, tribals")
            {
                CategoryGUID = Guid.Parse("{5F544F7D-E859-491B-9550-C151FEEB5756}"),
                Name = "Tribal",
                Title = "Tribal Rugs",
                Keywords = "tribal",
                SEName = "tribal-rugs",
            },

            //Mid-Century
		    new StyleFilter ("mid-century")
            {
                CategoryGUID = Guid.Parse("{B3B632BC-6E6C-4F89-B6DA-ED6EB72D4023}"),
                Name = "Mid-Century",
                Title = "Mid-Century Style Rugs",
                Keywords = "mid-century",
                SEName = "mid-century-style-rugs",
            },

            //Industrial
		    new StyleFilter ("industrial")
            {
                CategoryGUID = Guid.Parse("{C2964E29-5C8B-4860-8C94-390609B60C4B}"),
                Name = "Industrial",
                Title = "Industrial Rugs",
                Keywords = "industrial",
                SEName = "industrial-rugs",
            },


            //Preppy
		    new StyleFilter ("preppy")
            {
                CategoryGUID = Guid.Parse("{2ABB1C64-3B6F-4093-B35D-862678FDB611}"),
                Name = "Preppy",
                Title = "Preppy Rugs",
                Keywords = "preppy",
                SEName = "preppy-rugs",
            },

            //Glam
		    new StyleFilter ("glam")
            {
                CategoryGUID = Guid.Parse("{24CF9DB0-9EAB-4466-8A3B-27D99C9A2BBF}"),
                Name = "Glam",
                Title = "Glam Rugs",
                Keywords = "glam",
                SEName = "glam-rugs",
            },

            //Rustic
		    new StyleFilter ("rustic, cabin, lodge")  // cabin
            {
                CategoryGUID = Guid.Parse("{533938B3-AA70-4016-8E01-D0742AACE93E}"),
                Name = "Rustic",
                Title = "Rustic & Cabin Rugs",
                Keywords = "rustic",
                SEName = "rustic-and-cabin-rugs",
            },

            //Tropical
		    new StyleFilter ("tropical")
            {
                CategoryGUID = Guid.Parse("{472D2AA4-6D91-478C-98DC-E0C1D756D611}"),
                Name = "Tropical",
                Title = "Tropical Rugs",
                Keywords = "tropical",
                SEName = "tropical-rugs",
            },

            //Eastern
		    new StyleFilter ("eastern")
            {
                CategoryGUID = Guid.Parse("{E5E69C4D-5033-43B2-855C-EDC2317CDEF6}"),
                Name = "Eastern",
                Title = "Eastern Rugs",
                Keywords = "eastern",
                SEName = "eastern-rugs",
            },


            //Chevron
		    new StyleFilter ("chevron")
            {
                CategoryGUID = Guid.Parse("{DF0F5D08-C17D-44AB-86CC-F0A642D7CAD3}"),
                Name = "Chevron",
                Title = "Chevron Rugs",
                Keywords = "chevron",
                SEName = "chevron-rugs",
            },

            //Tibetan 
		    new StyleFilter ("tibetan")
            {
                CategoryGUID = Guid.Parse("{1B1C26C8-028E-46AE-9F61-92FC32D5EC4B}"),
                Name = "Tibetan",
                Title = "Tibetan Rugs",
                Keywords = "tibetan",
                SEName = "tibetan-rugs",
            },

            //Plaid
		    new StyleFilter ("plaid")
            {
                CategoryGUID = Guid.Parse("{7DDA5E94-BDE2-4DF8-BD53-3D22D6DCC56C}"),
                Name = "Plaid",
                Title = "Plaid Rugs",
                Keywords = "plaid",
                SEName = "plaid-rugs",
            },

            //Polka Dots
		    new StyleFilter ("polka dots, dots")
            {
                CategoryGUID = Guid.Parse("{533E8C75-E952-4143-A5B4-FAE4B43EFEA8}"),
                Name = "Polka Dots",
                Title = "Polka Dots Rugs",
                Keywords = "polka dots",
                SEName = "polka-dots-rugs",
            },

            //Camouflage 
		    new StyleFilter ("camouflage")
            {
                CategoryGUID = Guid.Parse("{C47CCEA4-BC6F-436E-8E2B-349684556522}"),
                Name = "Camouflage",
                Title = "Camouflage Rugs",
                Keywords = "camouflage",
                SEName = "camouflage-rugs",
            },

            //Beach / Nautical
		    new StyleFilter ("beach, nautical, boat, coastal") 
            {
                CategoryGUID = Guid.Parse("{A437DF0A-0A69-4EB2-A43D-7537373FF90E}"),
                Name = "Beach & Nautical",
                Title = "Beach & Nautical Rugs",
                Keywords = "beach, nautical",
                SEName = "beach-and-nautical-rugs",
            },

            //Sports
		    new StyleFilter ("sports")
            {
                CategoryGUID = Guid.Parse("{C0C2B84E-5334-4698-BCD2-E649F5F29514}"),
                Name = "Sports",
                Title = "Sports Themed Rugs",
                Keywords = "sports",
                SEName = "sports-themed-rugs",
            },

            //Numbers & Letters
		    new StyleFilter ("letters, numbers")
            {
                CategoryGUID = Guid.Parse("{8C03F05B-4592-4A5A-87F4-DC66905E2D89}"),
                Name = "Letters & Numbers",
                Title = "Rugs with Letters & Numbers",
                Keywords = "letters, numbers",
                SEName = "rugs-with-letters-and-numbers",
            },

            //Global Inspirational
		    new StyleFilter ("inspirational, global, global inspirational")
            {
                CategoryGUID = Guid.Parse("{C81C2D67-2639-41A0-BB98-13E8BC6D6EBA}"),
                Name = "Global Inspirational",
                Title = "Global Inspirational Rugs",
                Keywords = "inspirational",
                SEName = "global-inspirational-rugs",
            },


        };

        #endregion

        #region Initialization

		public RugsCategoryFilterRootStyle(IWebStore Store): base(Store)
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
            if (product.Features == null)
                return;

            // tags is required - but this is for safety since during dev have seen some missing.
            if (product.Features.Tags == null)
                return;

            // caller has already removed all ProductCategory associations

            // keeps track of associations, hash to make sure no duplicates
            var memberCategories = new HashSet<int>();

            foreach(var tag in product.Features.Tags)
            {
                var cleanTag = tag.ToLower().Replace(" rugs", "").Replace(" rug", "");

                foreach(var child in children)
                {
                    if (child.IsMatch(cleanTag))
                        memberCategories.Add(child.CategoryID);
                }
            }

            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }
        #endregion
    }
}
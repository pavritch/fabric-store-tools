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
    public class RugsCategoryFilterRootMaterial : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region MaterialFilter Class
        private class MaterialFilter : CategoryFilterInformation
        {
            private HashSet<string> matchSet = new HashSet<string>();

            // simple matching mechanism until final concepts put in place

            public MaterialFilter(string matchList)
            {
                foreach (var s in matchList.ParseCommaDelimitedList())
                    matchSet.Add(s.ToLower());                
            }

            public bool IsMatch(string material)
            {
#if true
                // fuzzy match
                // material must be lower
                foreach (var phrase in matchSet)
                {
                    if (material.ContainsIgnoreCase(phrase))
                        return true;

                    if (phrase.LevenshteinDistance(material) <= 1)
                        return true;
                }

                return false;
#else
                // exact match
                // material must be lower
                return matchSet.Contains(material);
#endif
            }

            public bool IsMatch(IEnumerable<string> materials)
            {
                // inputs must be clean lower
                foreach (var name in materials)
                {
                    if (IsMatch(name))
                        return true;
                }

                return false;
            }
        }

        #endregion

        #region Root Category Definition

        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{9BDC2B53-2A49-447A-BE54-A62891E56584}"),
            Name = "Material",
            Title = "Rugs by Material",
            DisplayOrder = 7,
            SEName = "rugs-by-material",
        };
        
        #endregion

        #region Child Definitions

        private static List<MaterialFilter> children = new List<MaterialFilter>()
        {
            // a product (and variants) would have a single association

            //Acrylic
            //Bamboo
            //Chenille
            //Cotton
            //Cowhide
            //Hemp
            //Jute
            //Leather
            //Linen
            //Natural Fiber
            //Nylon
            //Polyester
            //Rayon
            //Rubber
            //Sari Silk
            //Seagrass
            //Silk
            //Sisal
            //Synthetic
            //Velvet
            //Viscose
            //Wool


            //Acrylic
		    new MaterialFilter("acrylic")
            {
                CategoryGUID = Guid.Parse("{B1B6C45A-B873-42E9-84A2-139A1A70842E}"),
                Name = "Acrylic",
                Title = "Acrylic Rugs",
                Keywords = "acrylic",
                SEName = "acrylic-rugs",
            },

            //Bamboo
		    new MaterialFilter("bamboo")
            {
                CategoryGUID = Guid.Parse("{DBA3DD21-F928-4F2E-A14A-8333564EE3BF}"),
                Name = "Bamboo",
                Title = "Bamboo Rigs",
                Keywords = "bamboo",
                SEName = "bamboo-rugs",
            },

            //Chenille
		    new MaterialFilter("chenille")
            {
                CategoryGUID = Guid.Parse("{9CC85F27-6077-4962-A7BF-C95DBD49620C}"),
                Name = "Chenille",
                Title = "Chenille Rugs",
                Keywords = "chenille",
                SEName = "chenille-rugs",
            },

            //Cotton
		    new MaterialFilter("cotton")
            {
                CategoryGUID = Guid.Parse("{70F722AA-8655-40C2-AEFD-ECC1A3A30885}"),
                Name = "Cotton",
                Title = "Cotton Rugs",
                Keywords = "cotton",
                SEName = "cotton-rugs",
            },

            //Cowhide
		    new MaterialFilter("cowhide")
            {
                CategoryGUID = Guid.Parse("{FE94B880-B980-4F0B-B9AE-3DBE9228FBBC}"),
                Name = "Cowhide",
                Title = "Cowhide Rugs",
                Keywords = "",
                SEName = "cowhide-rugs",
            },

            //Hemp
		    new MaterialFilter("hemp")
            {
                CategoryGUID = Guid.Parse("{1CC8DA09-3C8B-4267-9DC0-895E680ABF78}"),
                Name = "Hemp",
                Title = "Hemp Rugs",
                Keywords = "hemp",
                SEName = "hemp-rugs",
            },

            //Jute
		    new MaterialFilter("jute")
            {
                CategoryGUID = Guid.Parse("{9B8D07C1-02DC-412B-9051-1DB4EF73F07E}"),
                Name = "Jute",
                Title = "Jute Rugs",
                Keywords = "jute",
                SEName = "jute-rugs",
            },

            //Leather
		    new MaterialFilter("leather")
            {
                CategoryGUID = Guid.Parse("{731ED516-5A60-4588-B0F8-8520FA7EF6E8}"),
                Name = "Leather",
                Title = "Leather Rugs",
                Keywords = "leather",
                SEName = "leather-rugs",
            },

            //Linen
		    new MaterialFilter("linen")
            {
                CategoryGUID = Guid.Parse("{263A0114-5B18-414E-974D-53BB5ACEC90F}"),
                Name = "Linen",
                Title = "Linen Rugs",
                Keywords = "linen",
                SEName = "linen-rugs",
            },

            //Natural Fiber
		    new MaterialFilter("natural")
            {
                CategoryGUID = Guid.Parse("{E9F7DFC8-5C0B-484F-A656-DFB7F31351B7}"),
                Name = "Natural Fiber",
                Title = "Natural Fiber Rugs",
                Keywords = "natural fiber",
                SEName = "natural-fiber-rugs",
            },

            //Nylon
		    new MaterialFilter("nylon")
            {
                CategoryGUID = Guid.Parse("{F5CC035D-3685-4595-BD51-4C78D57B3773}"),
                Name = "Nylon",
                Title = "Nylon Rugs",
                Keywords = "nylon",
                SEName = "nylon-rugs",
            },

            //Polyester
		    new MaterialFilter("polyester")
            {
                CategoryGUID = Guid.Parse("{BB5773C1-DADB-44D7-B7F8-05FBA1FA75F3}"),
                Name = "Polyester",
                Title = "Polyester Rugs",
                Keywords = "polyester",
                SEName = "polyester-rugs",
            },

            //Rayon
		    new MaterialFilter("rayon")
            {
                CategoryGUID = Guid.Parse("{2621BA21-9E73-483E-838C-E4F96F9F4029}"),
                Name = "Rayon",
                Title = "Rayon Rugs",
                Keywords = "rayon",
                SEName = "rayon-rugs",
            },

            //Rubber
		    new MaterialFilter("rubber")
            {
                CategoryGUID = Guid.Parse("{13F669B7-7734-49CF-83E9-039EC2954DB7}"),
                Name = "Rubber",
                Title = "Rubber Rugs",
                Keywords = "rubber",
                SEName = "rubber-rugs",
            },

            //Sari Silk
		    new MaterialFilter("Sari Silk")
            {
                CategoryGUID = Guid.Parse("{0F358DF7-C782-4FCC-A5C9-9769283DF333}"),
                Name = "Sari Silk",
                Title = "Sari Silk Rugs",
                Keywords = "sari silk",
                SEName = "sari-silk-rugs",
            },

            //Seagrass
		    new MaterialFilter("seagrass")
            {
                CategoryGUID = Guid.Parse("{4C5B44C7-DAB3-4B87-B7DA-AED99853EF90}"),
                Name = "Seagrass",
                Title = "Seagrass Rugs",
                Keywords = "seagrass",
                SEName = "seagrass-rugs",
            },

            //Silk
		    new MaterialFilter("silk")
            {
                CategoryGUID = Guid.Parse("{5B51CFFB-6C36-43D6-AF10-599F666BDABC}"),
                Name = "Silk",
                Title = "Silk Rugs",
                Keywords = "silk",
                SEName = "silk-rugs",
            },

            //Sisal
		    new MaterialFilter("sisal")
            {
                CategoryGUID = Guid.Parse("{CACAEBFD-1480-4186-9C63-2EAB11498045}"),
                Name = "Sisal",
                Title = "Sisal Rugs",
                Keywords = "sisal",
                SEName = "sisal-rugs",
            },

            //Synthetic
		    new MaterialFilter("synthetic, polysynthetic")
            {
                CategoryGUID = Guid.Parse("{48629099-1BD3-4472-80A5-7140C46E96A3}"),
                Name = "Synthetic",
                Title = "Synthetic Rugs",
                Keywords = "synthetic",
                SEName = "synthetic-rugs",
            },

            //Velvet
		    new MaterialFilter("velvet")
            {
                CategoryGUID = Guid.Parse("{0F4D0F77-2017-4639-B2F5-FBE63CCA0995}"),
                Name = "Velvet",
                Title = "Velvet Rugs",
                Keywords = "velvet",
                SEName = "velvet-rugs",
            },

            //Viscose
		    new MaterialFilter("viscose")
            {
                CategoryGUID = Guid.Parse("{6563138C-8294-49DB-8E28-3968E391A3EE}"),
                Name = "Viscose",
                Title = "Viscose Rugs",
                Keywords = "",
                SEName = "viscose-rugs",
            },

            //Wool
		    new MaterialFilter("wool")
            {
                CategoryGUID = Guid.Parse("{346BBD90-BE25-4E94-AE18-F649419FA67E}"),
                Name = "Wool",
                Title = "Wool Rugs",
                Keywords = "wool",
                SEName = "wool-rugs",
            },
        };
        #endregion

        #region Initialization

        public RugsCategoryFilterRootMaterial(IWebStore Store)
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

            var materials = product.Features.Material.Keys.Select(e => e.ToLower());

            foreach (var child in children)
            {
                if (child.IsMatch(materials))
                    memberCategories.Add(child.CategoryID);
            }

            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }
        #endregion
    }
}
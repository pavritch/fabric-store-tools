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
using System.Diagnostics;

namespace Website
{
    public class RugsCategoryFilterRootSize : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {

        #region GeneralSizeFilter Class
        private class GeneralSizeFilter : CategoryFilterInformation
        {
            public double MaximumArea {get; set;}

            public GeneralSizeFilter (double area)
	        {
                this.MaximumArea = area;
	        }
        }
        #endregion

        #region RunnerSizeFilter Class
        private class RunnerSizeFilter : CategoryFilterInformation
        {
            public double Length {get; set;}

            public RunnerSizeFilter (double length)
	        {
                this.Length = length;
	        }
        }
        #endregion

        #region SpecificSizeFilter Class
        private class SpecificSizeFilter : CategoryFilterInformation
        {
            private double[] vector;

            public SpecificSizeFilter (double width, double length)
	        {
                vector = MakeVector(width, length);
	        }

            public double GetDistance(double[] rugVector)
            {
                return Math.Abs(vector.EuclidianDistance(rugVector));
            }

            /// <summary>
            /// Construct a vector of basic properties which can be used for Euclidean distance calc.
            /// </summary>
            /// <param name="w">width</param>
            /// <param name="l">length</param>
            /// <returns></returns>
            public static double[] MakeVector(double w, double l)
            {
                // anything with same length (circle,square,oct) gets a metric that is greatly
                // out in a new space, so will not match anything but like kind

                // ensure that we're always working with width as the smaller dimension
                var width = Math.Min(w, l);
                var length = Math.Max(w, l);

                var v = new double[6];
                v[0] = width;
                v[1] = length;
                v[2] = width * length;
                v[3] = width==length ? 1000.0 : 1; 
                v[4] = width * 2 + length * 2;
                v[5] = width/length;

                return v;
            }
        }

        #endregion

        #region Root Category Definition

        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{07E29915-49B0-4F27-8542-AE6C39A61E6D}"),
            Name = "Size",
            Title = "Rugs by Size",
            DisplayOrder = 3,
            SEName = "rugs-by-size",
        };

        #endregion

        #region Child Definitions

            //Small
            //Medium
            //Large
            //Oversize
            //Runner (short)
            //Runner (long)

            //2' x 3'
            //3' x 5'
            //4' x 6'
            //5' x 8'
            //6' x 9'
            //7' x 9'
            //7' x 10'
            //8' x 10'
            //8' x 11'
            //9' x 10'
            //9' x 12'
            //10' x 13'
            //10' x 14'
            //12' x 15'

            //3' to 4' Round/Square
            //5' to 6' Round/Square
            //7' to 8' Round/Square
            //9' to 10' Round/Square

            // most popular sizes seem to be:
            // 2x3, 3x5, 5x8, 7x9, 8x10, 9x12

            private static List<CategoryFilterInformation> children = new List<CategoryFilterInformation>()
            {
                // general - take the first area that matches

                //Small  (under 24.5 sf)
		        new GeneralSizeFilter(area:24.5)
                {
                    CategoryGUID = Guid.Parse("{502C9405-EA29-4981-AFAE-44FA19CF5022}"),
                    Name = "Small",
                    Title = "Small Size Rugs",
                    Keywords = "",
                    SEName = "small-rugs",
                },

                //Medium (between 25 and just under 100 sf)
		        new GeneralSizeFilter(area:99.99)
                {
                    CategoryGUID = Guid.Parse("{7C3BA10C-3AEB-4CA5-9DEE-0A4E2881CDE6}"),
                    Name = "Medium",
                    Title = "Medium Size Rugs",
                    Keywords = "",
                    SEName = "medium-rugs",
                },

                //Large ( >= 100 sf)
		        new GeneralSizeFilter(area: 149.99)
                {
                    CategoryGUID = Guid.Parse("{855DE6AA-BB84-45CA-BEAC-D470D7659269}"),
                    Name = "Large",
                    Title = "Large Size Rugs",
                    Keywords = "",
                    SEName = "large-rugs",
                },

                //Oversize ( >= 150 sf)
		        new GeneralSizeFilter(area: double.MaxValue)
                {
                    CategoryGUID = Guid.Parse("{DBFFF082-A9AF-4CAE-A617-320D942F3828}"),
                    Name = "Oversize",
                    Title = "Oversize Rugs",
                    Keywords = "",
                    SEName = "oversize-rugs",
                },

                // runners - take the lowest one that matches

                //Runner (short)
		        new RunnerSizeFilter(9.99)
                {
                    CategoryGUID = Guid.Parse("{1A354068-993A-478B-8EE2-2222604BAE8E}"),
                    Name = "Runner (short)",
                    Title = "Short Runner Rugs (under 10')",
                    Keywords = "",
                    SEName = "short-runner-rugs",
                },

                //Runner (long)
		        new RunnerSizeFilter(double.MaxValue)
                {
                    CategoryGUID = Guid.Parse("{0736652F-DDEC-493B-886A-3BF378195EDF}"),
                    Name = "Runner (long)",
                    Title = "Long Runner Rugs (over 10')",
                    Keywords = "",
                    SEName = "long-runner-rugs",
                },

                // specific sizes - take the best fit

                //2' x 3'
		        new SpecificSizeFilter(2.0, 3.0)
                {
                    CategoryGUID = Guid.Parse("{914AC04F-9CC5-447A-BA03-6EEE6F3BEEA1}"),
                    Name = "2' x 3'",
                    Title = "2' x 3' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-2x3",
                },

                //3' x 5'
		        new SpecificSizeFilter(3.0, 5.0)
                {
                    CategoryGUID = Guid.Parse("{5580ACC0-922C-4264-AD4E-B3AF087C42A8}"),
                    Name = "3' x 5'",
                    Title = "3' x 5' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-3x5",
                },

                //4' x 6'
		        new SpecificSizeFilter(4.0, 6.0)
                {
                    CategoryGUID = Guid.Parse("{02E77C4C-640C-40E3-87CA-2EFDA65CFA47}"),
                    Name = "4' x 6'",
                    Title = "4' x 6' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-4x6",
                },

                //5' x 8'
		        new SpecificSizeFilter(5.0, 8.0)
                {
                    CategoryGUID = Guid.Parse("{5E630289-5271-4A1E-BBB4-EF046893F4F0}"),
                    Name = "5' x 8'",
                    Title = "5' x 8' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-5x8",
                },

                //6' x 9'
		        new SpecificSizeFilter(6.0, 9.0)
                {
                    CategoryGUID = Guid.Parse("{7E6A156E-63D7-427D-A93B-0CB8FA5F5AEE}"),
                    Name = "6' x 9'",
                    Title = "6' x 9' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-6x9",
                },

                //7' x 9'
		        new SpecificSizeFilter(7.0, 9.0)
                {
                    CategoryGUID = Guid.Parse("{C46294C8-5898-4400-BBB3-3558C908C418}"),
                    Name = "7' x 9'",
                    Title = "7' x 9' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-7x9",
                },

                //7' x 10'
		        new SpecificSizeFilter(7.0, 10.0)
                {
                    CategoryGUID = Guid.Parse("{FDC492F9-71C4-446B-8911-8A7747A40554}"),
                    Name = "7' x 10'",
                    Title = "7' x 10' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-7x10",
                },

                //8' x 10'
		        new SpecificSizeFilter(8.0, 10.0)
                {
                    CategoryGUID = Guid.Parse("{EFCB2551-B2D0-411F-B46B-D0C3FE27697F}"),
                    Name = "8' x 10'",
                    Title = "8' x 10' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-8x10",
                },

                //8' x 11'
		        new SpecificSizeFilter(8.0, 11.0)
                {
                    CategoryGUID = Guid.Parse("{33DF511D-A5DF-4F43-9B99-5AA6E3B5F652}"),
                    Name = "8' x 11'",
                    Title = "8' x 11' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-8x11",
                },

                //9' x 10'
		        new SpecificSizeFilter(9.0, 10.0)
                {
                    CategoryGUID = Guid.Parse("{C16D7B46-DD45-4ACF-A3B6-775C9CA3B709}"),
                    Name = "9' x 10'",
                    Title = "9' x 10' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-9x10",
                },

                //9' x 12'
		        new SpecificSizeFilter(9.0, 12.0)
                {
                    CategoryGUID = Guid.Parse("{BEC5B7FD-A7C6-41A1-8396-56B5EBB788F8}"),
                    Name = "9' x 12'",
                    Title = "9' x 12' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-9x12",
                },

                //10' x 13'
		        new SpecificSizeFilter(10.0, 13.0)
                {
                    CategoryGUID = Guid.Parse("{27F1A4DB-E1CF-4892-ADE0-9D5FCF04FE8E}"),
                    Name = "10' x 13'",
                    Title = "10' x 13' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-10x13",
                },

                //10' x 14'
		        new SpecificSizeFilter(10.0, 14.0)
                {
                    CategoryGUID = Guid.Parse("{9BF2A79D-0B8F-4E9B-B444-7B8272750D53}"),
                    Name = "10' x 14'",
                    Title = "10' x 14' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-10x14",
                },

                //12' x 15'
		        new SpecificSizeFilter(12.0, 15.0)
                {
                    CategoryGUID = Guid.Parse("{4C458496-1A3F-4D88-AD7A-2758FDD8167B}"),
                    Name = "12' x 15'",
                    Title = "12' x 15' Rugs",
                    Keywords = "",
                    SEName = "rugs-size-12x15",
                },

                //3' to 4' Round/Square
		        new SpecificSizeFilter(3.5, 3.5)
                {
                    CategoryGUID = Guid.Parse("{7CAE4EED-D453-4773-9767-145303A48945}"),
                    Name = "3' to 4' Round/Square",
                    Title = "3' to 4' Round & Square Rugs",
                    Keywords = "",
                    SEName = "rugs-size-3-to-4-round-square",
                },

                //5' to 6' Round/Square
		        new SpecificSizeFilter(5.5, 5.5)
                {
                    CategoryGUID = Guid.Parse("{D6452C19-C8C7-4445-A2E2-3F8114569EC3}"),
                    Name = "5' to 6' Round/Square",
                    Title = "5' to 6' Round & Square Rugs",
                    Keywords = "",
                    SEName = "rugs-size-5-to-6-round-square",
                },

                //7' to 8' Round/Square
		        new SpecificSizeFilter(7.5, 7.5)
                {
                    CategoryGUID = Guid.Parse("{0A6C43B1-AA95-4BDA-BF79-A43DEBDBF6B0}"),
                    Name = "7' to 8' Round/Square",
                    Title = "7' to 8' Round & Square Rugs",
                    Keywords = "",
                    SEName = "rugs-size-7-to-8-round-square",
                },

                //9' to 10' Round/Square
		        new SpecificSizeFilter(9.5, 9.5)
                {
                    CategoryGUID = Guid.Parse("{B61EF7CD-DED8-45D0-8AB0-AEFECE0E4E5D}"),
                    Name = "9' to 10' Round/Square",
                    Title = "9' to 10' Round & Square Rugs",
                    Keywords = "",
                    SEName = "rugs-size-9-to-10-round-square",
                },

            };
        #endregion

        public RugsCategoryFilterRootSize(IWebStore Store)
            : base(Store)
        {

        }


        public void Initialize(int parentCategoryID)
        {
            // variants come in all sizes - take the union of choices amongst
            // all the variants for each product.

            // further, there are some groups with aggregate 


            Initialize(parentCategoryID, categoryInfo, children);
        }


        #region Classify



        /// <summary>
        /// Classifies the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        public virtual void Classify(InsideRugsProduct product)
        {
            // safety
            if (product.VariantFeatures == null)
                return;

            // caller has already removed all ProductCategory associations

            // keeps track of associations, hash to make sure no duplicates
            var memberCategories = new HashSet<int>();

            // general (runner) - take first/lowest that matches

            foreach (var vf in product.VariantFeatures)
            {
                if (string.IsNullOrWhiteSpace(vf.Shape) || vf.Shape != "Runner")
                    continue;

                foreach (RunnerSizeFilter child in children.Where(e => e.GetType() == typeof(RunnerSizeFilter)))
                {
                    if (vf.Length < child.Length * 12.0)
                    {
                        memberCategories.Add(child.CategoryID);
                        break;
                    }
                }
            }


            foreach (var vf in product.VariantFeatures)
            {
                var excludedShapes = new string[] {"Sample", "Other", "Runner"};

                if (string.IsNullOrWhiteSpace(vf.Shape) || excludedShapes.Contains(vf.Shape))
                    continue;

                // general (non runner) - take first/lowest that matches
                
                foreach (GeneralSizeFilter child in children.Where(e => e.GetType() == typeof(GeneralSizeFilter)))
                {
                    if (vf.AreaSquareFeet < child.MaximumArea)
                    {
                        memberCategories.Add(child.CategoryID);
                        break;
                    }
                }

                // specific sizes - take best fit from the range of choices

                // if greater than this, then is oversize and we don't classify here

                if (vf.Width < (13.0 * 12.0) && vf.Length < (16.0 * 12.0))
                {
                    int bestCategoryID = 0;
                    double bestDistance = double.MaxValue;

                    var rugVector = SpecificSizeFilter.MakeVector(vf.Width / 12.0, vf.Length / 12.0);

                    foreach (SpecificSizeFilter child in children.Where(e => e.GetType() == typeof(SpecificSizeFilter)))
                    {
                        var distance = child.GetDistance(rugVector);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestCategoryID = child.CategoryID;
                        }
                    }

                    if (bestCategoryID > 0)
                        memberCategories.Add(bestCategoryID);
                }
            }


            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }
        #endregion
    }
    
}
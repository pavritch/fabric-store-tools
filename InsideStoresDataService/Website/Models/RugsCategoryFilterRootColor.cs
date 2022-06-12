//------------------------------------------------------------------------------
// 
// Class: RugsCategoryFilterRootBase
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using InsideFabric.Data;
using InsideStores.Imaging;
using i4o;
using System.Threading;

//using Gen4.Util.Misc;
namespace Website
{
    public class RugsCategoryFilterRootColor : RugsCategoryFilterRootBase, ICategoryFilterRoot<InsideRugsProduct>
    {
        #region ColorFilter Class
        private class ColorFilter : CategoryFilterInformation
        {
            private HashSet<string> phraseMatchSet;
            private List<System.Windows.Media.Color> rgbColors;
            private List<Lab> labColors;
            private bool isMulticolor;

            /// <summary>
            /// One kind of color we recognize as a first-class menu item.
            /// </summary>
            /// <param name="colors">list of color targets that would qualify:  #000000, #111111, etc.</param>
            /// <param name="matchList">Optional list of keywords.</param>
            /// <param name="isMulticolor"></param>
            public ColorFilter(string colors, string matchList, bool isMulticolor=false)
            {
                this.isMulticolor = isMulticolor;
                rgbColors = new List<System.Windows.Media.Color>();

                if (!string.IsNullOrWhiteSpace(colors))
                {
                    foreach (var c in colors.ParseCommaDelimitedList())
                        rgbColors.Add((System.Windows.Media.Color)ColorConverter.ConvertFromString(c));
                }
                else
                {
                    rgbColors.Add((System.Windows.Media.Color)ColorConverter.ConvertFromString("#ffffffff"));
                }

                // precompute lab values for faster distance comparisons

                labColors = rgbColors.ToLabColors();

                // parse optional keyword phrases

                this.phraseMatchSet = new HashSet<string>();
                foreach (var s in matchList.ParseCommaDelimitedList())
                    phraseMatchSet.Add(s.ToLower());
            }


            public bool IsMulticolor
            {
                get
                {
                    return isMulticolor;
                }
            }

            public List<System.Windows.Media.Color> Colors
            {
                get
                {
                    Debug.Assert(!isMulticolor);
                    return rgbColors;
                }
            }

            public List<Lab> LabColors
            {
                get
                {
                    Debug.Assert(!isMulticolor);
                    return  labColors;
                }
            }

            public bool IsPhraseMatch(string colorNamePhrase)
            {
                if (IsMulticolor)
                    return colorNamePhrase.ContainsIgnoreCase("multi");

                // fuzzy match
                foreach (var phrase in phraseMatchSet)
                {
                    if (phrase.LevenshteinDistance(colorNamePhrase.ToLower()) <= 1)
                        return true;
                }

                return false;
            }

        }
        #endregion

        #region Root Category Filter

        private static CategoryFilterInformation categoryInfo = new CategoryFilterInformation()
        {
            // Warning: parentID and categoryID not in this definition - filled in from the base class.
            CategoryGUID = Guid.Parse("{DDC01E1F-CF5D-4466-BFFA-631BCDB7F9E5}"),
            Name = "Color",
            Title = "Rugs by Color",
            DisplayOrder = 2,
            SEName = "rugs-by-color",
        };

        
        #endregion

        #region Child Definitions
        private static List<ColorFilter> children = new List<ColorFilter>()
        {

            //Blacks
            //Silver & Grays
            //Blues
            //Browns
            //Burgundies ***
            //Greens
            //Multicolored
            //Oranges
            //Reds
            //Pinks
            //Purples
            //Cream & Ivories
            //Whites
            //Tans & Beiges ****
            //Yellows & Golds


            //Blacks done
		    new ColorFilter("#ff222222, #ff000000, #ff383838, #ff191919", "black, coal, ebony, onyx, noir")
            {
                CategoryGUID = Guid.Parse("{96964059-64BA-43CA-B356-08071B9E5BEC}"),
                Name = "Blacks",
                Title = "Black Rugs",
                Keywords = "black",
                SEName = "black-rugs",
            },

            //Silver & Grays done
		    new ColorFilter("#ff9b9b9b, #ffa9a9a9, #ff808080, #ff778899, #ff708090, #ff999999, #ffc6c6c6, #ffc0c0c0, #ff868686, #ff737373, #ffe5e5e5, #ffd2d2d2", "silver, gray, grey, charcoal, silver, pearl, smoke, stone, ash, slate, graphite, sterling")
            {
                CategoryGUID = Guid.Parse("{0D784692-52DC-4F77-990C-8008562133DA}"),
                Name = "Silver & Grays",
                Title = "Silver & Gray Rugs",
                Keywords = "silver, gray",
                SEName = "silver-and-gray-rugs",
            },


            //Blues done
		    new ColorFilter("#ff375c8b, #ff4682b4, #ffadd8e6, #ff87ceeb, #ff87cefa, #ff00bfff, #ff1e90ff, #ff6495ed, #ff4169e1, #ff0000ff, #ff0000cd, #ff00008b, #ff000080, #ff191970, #ff6392bd, #ff2164a1, #ffa6c1d9, #ff1a5080, #ff133c60, #ff27408b", "blue, sky, light blue, azure, cobalt, robin's egg, turquoise, indigo, cerulean, midnight, aqua, ice blue, navy, teal, baby blue")
            {
                CategoryGUID = Guid.Parse("{99673FD4-0605-4B76-B73A-11BE4A6C7EF0}"),
                Name = "Blues",
                Title = "Blue Rugs",
                Keywords = "blue",
                SEName = "blue-rugs",
            },

            //Browns
		    new ColorFilter("#ff755341, #ff8b4513, #ffa0522d, #ff4b2821, #ff735852, #ff87706b, #ff9b8884, #ff8b6969, #ff8b4c39", "brown, chocolate, mocha, coffee, bronze, cinnamon, cocoa, coffee, copper, auburn, amber, umber, expresso, sienna, mahogany, ochre, russet, chestnut, earth, cappucino, cafe, java, cognac, hazelnut, praline")
            {
                CategoryGUID = Guid.Parse("{698A4987-E1E7-4A8C-994A-479A1C23DBAB}"),
                Name = "Browns",
                Title = "Brown Rugs",
                Keywords = "brown",
                SEName = "brown-rugs",
            },

            //Burgundies
		    new ColorFilter("#ff7b1e26, #ff800000, #ff8b2252, #ff8d244b, #ff98395d, #ff621934, #ff951111", "burgundy, bordeaux, brick, claret, magenta, garnet, wine, venetian, quince, tuscan, watermelon, paprika, port, pomegranate, grenadine, ruby")
            {
                CategoryGUID = Guid.Parse("{803338D7-0E23-45D9-B1C0-B5CF7256065B}"),
                Name = "Burgundies",
                Title = "Burgundy Rugs",
                Keywords = "burgundy",
                SEName = "burgundy-rugs",
            },

            //Greens done
		    new ColorFilter("#ff8f9d59, #ff00ff7f, #ff3cb371, #ff2e8b57, #ff228b22, #ff008000, #ff006400, #ff9acd32, #ff6b8e23, #ff808000, #ff556b2f, #ffcdcd00, #ffbdb76b", "green, light green, malachite, moss, olive, aquamarine, jade, kelly, green apple, blue grass, kelly, meadow, mint, clover, sage, basil, vert, hunter, grass, celadon, leaf")
            {
                CategoryGUID = Guid.Parse("{FA89EF14-E4D3-44B7-9CDC-D6568BE86FE1}"),
                Name = "Greens",
                Title = "Green Rugs",
                Keywords = "green",
                SEName = "green-rugs",
            },

            //Multicolored
		    new ColorFilter(null, "multicolor, multi", isMulticolor:true)
            {
                CategoryGUID = Guid.Parse("{17CAB07D-EE65-4A1B-9BEB-CFB254A64BDB}"),
                Name = "Multicolored",
                Title = "Multicolor Rugs",
                Keywords = "multicolor",
                SEName = "multicolor-rugs",
            },

            //Oranges
		    new ColorFilter("#ffc37145, #ffffa07a, #ffff7f50, #ffff6347, #ffff8c00, #ffffa500, #ffffa54f, #ffff7f24", "orange, rust, spice, cinnamon, coral, peach, salmon, tangerine, apricot, cinnabar, apricot, persimmon, copper, brick")
            {
                CategoryGUID = Guid.Parse("{BF5B57D3-583E-41F4-A85F-93F80F8860F3}"),
                Name = "Oranges",
                Title = "Orange Rugs",
                Keywords = "orange",
                SEName = "orange-rugs",
            },

            //Reds
		    new ColorFilter("#ffa90c30, #ffb20000, #ffcc0000, #ffe50000, #ffff0000, #ffab1414", "red, brick, cherry, watermelon, paprika, port, pomegranate, ruby")
            {
                CategoryGUID = Guid.Parse("{2E2EB5CD-D901-4894-9A83-0D21A52CED2A}"),
                Name = "Reds",
                Title = "Red Rugs",
                Keywords = "red",
                SEName = "red-rugs",
            },

            //Pinks
		    new ColorFilter("#ffffc0cb, #ffffb6c1, #ffff69b4, #ffff1493, #ffdb7093", "pink, blush, rose, fuschia, coral, melon, strawberry, cerise, cherry, poppy, peony, berry, cranberry, primrose")
            {
                CategoryGUID = Guid.Parse("{439270A1-11CE-41AD-A32F-06B7BE8A0881}"),
                Name = "Pinks",
                Title = "Pink Rugs",
                Keywords = "pink",
                SEName = "pink-rugs",
            },

            //Purples done
		    new ColorFilter("#ff744d8c, #ffdda0dd, #ffee82ee, #ffda70d6, #ffba55d3, #ff9370db, #ff663399, #ff8a2be2, #ff9400d3, #ff8b008b, #ff800080, #ff4b0082", "purple, lavender, violet, plum, wine, magenta, mauve, lilac, rasberry, wisteria, amethyst")
            {
                CategoryGUID = Guid.Parse("{21BE8127-4AB9-49A9-BB22-AB62ED0F7575}"),
                Name = "Purples",
                Title = "Purple Rugs",
                Keywords = "purple",
                SEName = "purple-rugs",
            },

            // Cream & Ivories done
		    new ColorFilter("#ffece6d9, #fffdf5e6, #fffffaf0, #fffffff0, #fffaebd7, #fffaf0e6 ", "cream, biscuit, oatmeal, fawn, champagne, parchment, chablis, barley, creme, wheat")
            {
                CategoryGUID = Guid.Parse("{376A3C04-D536-4D51-8191-7AC6C2A7482E}"),
                Name = "Cream & Ivories",  
                Title = "Cream & Ivory Rugs",
                Keywords = "cream, ivory",
                SEName = "cream-and-ivory-rugs",
            },

            //Whites done
		    new ColorFilter("#fff5f1e9, #ffffffff, #fffffafa, #fff5f5f5, #fff8f8ff", "white, off white, cream, snow, ivory, snow, winter")
            {
                CategoryGUID = Guid.Parse("{A3295F9D-8C6A-464C-803C-4928073EA930}"),
                Name = "Whites",
                Title = "White Rugs",
                Keywords = "white",
                SEName = "white-rugs",
            },

            //Tans & Beiges done (sort of)
		    new ColorFilter("#ffc7b49e, #ffffebcd, #ffffdead, #ffdeb887, #ffd2b48c, #ffcdc5bf, #ffffe7ba", "beige, cream, taupe, tan, biscuit, buff, neutral, oatmeal, sand, camel, fawn, khaki, natural, mushroom, champagne, parchment, chablis, sandalwood, creme brulee, caramel, truffle, barley, creme, wheat")
            {
                CategoryGUID = Guid.Parse("{D9ADEBB8-CD70-4DB1-9759-04B3F2514B0F}"),
                Name = "Tans & Beiges",  
                Title = "Tan & Beige Rugs",
                Keywords = "beige, tan",
                SEName = "tan-and-beige-rugs",
            },

            //Yellows & Golds done
		    new ColorFilter("#ffecc473, #ffffd700, #ffffff00, #ffffffe0, #fff0e68c", "gold, yellow, amber, lemon, vermillion, butter, copper, cashew, coin")
            {
                CategoryGUID = Guid.Parse("{91486D6E-E4EE-405B-966A-F5971F74C7E3}"),
                Name = "Yellows & Golds",
                Title = "Yellow & Gold Rugs",
                Keywords = "yellow, gold",
                SEName = "yellow-and-gold-rugs",
            },

        };

#if false
        // not up to date
[
{name:"blacks", colors:"#222222, #000000, #2f4f4f, #383838, #191919"},
{name:"Silver/Grays", colors:"#9b9b9b, #a9a9a9, #808080, #778899, #708090, #999999, #c6c6c6, #c0c0c0, #868686, #737373, #e5e5e5, #d2d2d2"},
{name:"Blues", colors:"#375c8b, #4682b4, #add8e6, #87ceeb, #87cefa, #00bfff, #1e90ff, #6495ed, #4169e1, #0000ff, #0000cd, #00008b, #000080, #191970, #6392bd, #2164a1, #a6c1d9, #1a5080, #133c60, #27408b"},
{name:"Browns", colors:"#755341, #8b4513, #a0522d, #381109, #4b2821, #735852, #87706b, #9b8884, #8b6969, #8b4c39"},
{name:"Burgundies", colors:"#7b1e26, #800000, #8b2252, #8d244b, #98395d, #621934, #461225"},
{name:"Greens", colors:"#8f9d59, #00ff7f, #3cb371, #2e8b57, #228b22, #008000, #006400, #9acd32, #6b8e23, #808000, #556b2f"},
{name:"Oranges", colors:"#c37145, #ffa07a, #ff7f50, #ff6347, #ff4500, #ff8c00, #ffa500, #ffa54f, #ff7f24"},
{name:"Reds", colors:"#a90c30, #b20000, #cc0000, #e50000, #ff0000, #ab1414, #951111"},
{name:"Pinks", colors:"#ffc0cb, #ffb6c1, #ff69b4, #ff1493, #c71585, #db7093"},
{name:"Purples", colors:"#744d8c, #dda0dd, #ee82ee, #da70d6, #ba55d3, #9370db, #663399, #8a2be2, #9400d3, #8b008b, #800080, #4b0082"},
{name:"Cream/Ivories", colors:"#ece6d9, #fdf5e6, #fffaf0, #fffff0, #faebd7, #faf0e6"},
{name:"Whites", colors:"#f5f1e9, #ffffff, #fffafa, #f5f5f5, #f8f8ff"},
{name:"Tans/Beiges", colors:"#c7b49e, #ffebcd, #ffdead, #deb887, #d2b48c, #cdc5bf, #ffe7ba"},
{name:"Yellows & Golds", colors:"#ecc473, #ffd700, #ffff00, #ffffe0, #f0e68c, #bdb76b, #daa520, #cdcd00"}
]
#endif
        #endregion


        #region Initialization

        public RugsCategoryFilterRootColor(IWebStore Store)
            : base(Store)
        {

        }

        public void Initialize(int parentCategoryID)
        {
            // generally a single color, but could include multiple colors
            // if there are several clear dominant colors - since taking an avg
            // would not really be appropriate

            // look for duplicates

            {
                foreach(var child in children.Where(e => !e.IsMulticolor))
                {
                    // make sure unique within self
                    var c1 = child.Colors.Distinct().Count();
                    var c2 = child.Colors.Count();
                    if (c1 != c2)
                        Debug.WriteLine(string.Format("Color filter for {1} has duplicate entries: {0}", child.Name, Store.StoreKey));
                    
                    Debug.Assert(c1 == c2);

                    // make sure unique amongst others
                    foreach(var color in child.Colors)
                    {
                        // for each color this filter has, make sure no other filter has the same listed for itself

                        foreach(var sibling in children.Where(e => !e.IsMulticolor && !object.ReferenceEquals(e, child)))
                        {

                            foreach (var sibColor in sibling.Colors)
                            {
                                if (sibColor.Equals(color))
                                    Debug.WriteLine(string.Format("Duplicate color for {0}: {1}", child.Name, child.Colors.First().ToRGBColorsString()));
                            }
                        }
                    }

                }
            }

            var count1 = children.Where(e => !e.IsMulticolor).SelectMany(e => e.Colors).Distinct().Count();
            var count2 = children.Where(e => !e.IsMulticolor).SelectMany(e => e.Colors).Count();

            // make sure palette colors are actually unique
            Debug.Assert(count1 == count2);

            Initialize(parentCategoryID, categoryInfo, children);
            //CreateMappingPalette();
            //AssignLocalHistogramBins();
        }
        
#if false // old CEDD histogram stuff
        // this chunk of code was an attempt to use CEDD color histogram for matching up colors,
        // but seemed not so reliable compared to other methods

        /// <summary>
        /// 24 entries - one for each cedd color mapped to the best matching local palette color.
        /// </summary>
        private ColorFilter[] colorMapper; // ColorFilter[24]


        /// <summary>
        /// The layout of the local palette histograms we'll create during classify.
        /// </summary>
        /// <remarks>
        /// Tells the the number of bins (via length) and which actual filter is assigned to that bin.
        /// Multicolor does not get a bin.
        /// </remarks>
        private ColorFilter[] histogramBinFilterAssignments;

        private int[] mapHistogramBins; //  int[24]


        /// <summary>
        /// Create a mapping from CEDD 24 colors to this local palatte.
        /// </summary>
        private void CreateMappingPalette()
        {
            colorMapper = new ColorFilter[CEDD.ColorCount];

            for (int i = 0; i < CEDD.ColorCount; i++)
            {
                Color ceddColor = CEDD.DescriptorColors[i];
                ColorFilter bestFilter = null;
                double bestDistance = double.MaxValue;

                foreach(var filter in children.Where(e => !e.IsMulticolor))
                {
                    double distance = DistanceHelpers.Distance(ceddColor, filter.Color);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestFilter = filter;
                    }
                }
                colorMapper[i] = bestFilter;
            }
        }

        /// <summary>
        /// Create a standard bin layout of the filter colors that are real (not multi),
        /// and then map each CEDD color to its respective bin number so we can quickly
        /// associate to the right bin during classification.
        /// </summary>
        private void AssignLocalHistogramBins()
        {
            // these will be the bins (one for each color, excluding funny ones like multi)
            histogramBinFilterAssignments = children.Where(e => !e.IsMulticolor).ToArray();

            // for each of the 24 CEDD colors, figure out which bin to associate it to
            mapHistogramBins = new int[colorMapper.Length];

            for(var j=0; j < colorMapper.Length; j++)
            {
                var filter = colorMapper[j];

                // find the index in the local histogram bin layout to match

                for(int i=0; i < histogramBinFilterAssignments.Length; i++)
                {
                    if (Object.ReferenceEquals(histogramBinFilterAssignments[i], filter))
                    {
                        mapHistogramBins[j] = i;
                        break;
                    }
                }
            }

        }

        private List<ColorFilter> GetBestFilters(byte[] cedd)
        {
            var bestFilters = new List<ColorFilter>();

            var localHistogram = CreateLocalHistogram(cedd);

            var bestIndexes = new List<int>();

            // simple - take the greatest bin -- should be greatly improved!!!

            int highCount = int.MinValue;
            int bestIndex = -1;
            for (int i = 0; i < localHistogram.Length; i++ )
            {
                if (localHistogram[i] > highCount)
                {
                    highCount = localHistogram[i];
                    bestIndex = i;
                }
            }

            bestIndexes.Add(bestIndex);


            bestFilters = bestIndexes.Select(e => histogramBinFilterAssignments[e]).ToList();

            return bestFilters;
        }

        /// <summary>
        /// Creates a histogram based on local palette.
        /// </summary>
        /// <remarks>
        /// Bin positions are relative to established mapping array.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        private int[] CreateLocalHistogram(byte[] cedd)
        {
            var ceddHistogram = CEDD.GetColorHistogram(cedd); // int[24]

            var histogram = new int[histogramBinFilterAssignments.Length];

            // for each of the 24 colors in product cedd histogram, add their
            // count to the mapped local palette histogram -- essentially just reduced down to the smaller hist.

            for(int i=0; i < ceddHistogram.Length; i++)
            {
                var binIndex = mapHistogramBins[i];
                histogram[binIndex] += ceddHistogram[i];
            }

            return histogram;
        }

#endif
        #endregion

        #region Classify




        /// <summary>
        /// Find single best fitting filter for the specified color.
        /// </summary>
        /// <remarks>
        /// Checks against all color points listed for each filter.
        /// </remarks>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        private ColorFilter FindClosestFilter(System.Windows.Media.Color targetColor)
        {
            ColorFilter bestFilter = null;
            double bestDistance = double.MaxValue;

            var targetLabColor = targetColor.ToLabColor();
            var comparisonMethod = new Cie1976Comparison();

            foreach(var filter in children)
            {
                // do not check multi here since not sensible for this operation
                if (filter.IsMulticolor)
                    continue;

                // each filter can have several color targets, so must spin
                // through them all

                foreach(var labColor in filter.LabColors)
                {
                    var distance = labColor.Compare(targetLabColor, comparisonMethod);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestFilter = filter;
                    }
                }
            }

            return bestFilter;
        }

        /// <summary>
        /// Use found dominant colors to find the best filter.
        /// </summary>
        /// <remarks>
        /// Presently taking only the top color.
        /// </remarks>
        /// <param name="colors"></param>
        /// <returns></returns>
        private List<ColorFilter> GetBestFiltersForDominantColors(List<string> colors)
        {
            if (colors == null || colors.Count() == 0)
                return new List<ColorFilter>();

            var rgbColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(colors[0]);

            return new List<ColorFilter>() { FindClosestFilter(rgbColor)};
        }

        private System.Windows.Media.Color PickRepresentativeColor(string singleColor, List<string> domColors )
        {
            // singleColor is "BestColor" from the input data, which is essentially the color we get with a palette of 1 color

            // experimenting to find what seems to work the best - trying to key off of the best dom color that is closest
            // in distance to the single color

            double bestDistance = double.MaxValue;
            string bestColorString = null; 

            var comparisonMethod = new Cie1976Comparison();
            var singleLabColor = singleColor.ToColor().ToLabColor();

            foreach (var colorString in domColors)
            {
                var distance = singleLabColor.Compare(colorString.ToColor().ToLabColor(), comparisonMethod);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestColorString = colorString;
                }
            }

            return bestColorString.ToColor();
        }

        /// <summary>
        /// Classifies the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        public virtual void Classify(InsideRugsProduct product)
        {
            if (product.Features == null)
                return;

            // caller has already removed all ProductCategory associations

            // keeps track of associations, hash to make sure no duplicates
            var memberCategories = new HashSet<int>();

#if false
            // using the  global bulk processing for the moment since tends to be a little better

            if (product.ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures))
            {
                var imgFeatures = product.ExtData4[ExtensionData4.ProductImageFeatures] as ImageFeatures;

                if (imgFeatures == null)
                    return;

                if (!string.IsNullOrWhiteSpace(imgFeatures.BestColor))
                {
                    var filters = new List<ColorFilter>();

                    //var bestColor = imgFeatures.BestColor.ToColor();

                    // try to come up with a color to best represent this image
                    var representativeColor = PickRepresentativeColor(imgFeatures.BestColor, imgFeatures.DominantColors);

                    filters.Add(FindClosestFilter(representativeColor));

                    foreach (var filter in filters)
                        memberCategories.Add(filter.CategoryID);
                }
            }
#endif
            // additionally process any provided color names

            Action<string> tryColorName = (color) =>
            {
                if (string.IsNullOrWhiteSpace(color))
                    return;

                var cleanColor = color.ToLower();
                foreach (var child in children)
                {
                    if (child.IsPhraseMatch(cleanColor))
                    {
                        memberCategories.Add(child.CategoryID);
                        return;
                    }
                }
            };

            Action<List<string>> tryColorNames = (colors) =>
            {
                if (colors == null)
                    return;

                foreach (var clr in colors)
                    tryColorName(clr);
                
            };

            // NOTE: since presently using global color processing, only multi here will actually survive that process

            tryColorNames(product.Features.Colors);
            tryColorName(product.Features.ColorGroup);

            // update ProductCategory table for assocations needed for this product
            AddProductCategoryAssociationsForProduct(product.p.ProductID, memberCategories.ToList());
        }

        public void RebuildColorFilters(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
        {
            Action<string> sendStatus = (msg) =>
                {
                    if (reportStatus == null)
                        return;

                    reportStatus(msg);
                };

            int lastPercent = -1;
            Action<int> sendProgress = (pct) =>
            {
                if (reportProgress == null || pct == lastPercent)
                    return;

                reportProgress(pct);
                lastPercent = pct;
                System.Threading.Thread.Sleep(10);
            };

            sendProgress(0);

            int progress = 0;
            int progressPerFilter = (100 / (children.Count()-1)) / 2; //  minus 1 for multi excluded

            if (!Store.IsPopulated)
            {
                sendStatus("Stpre products not yet populated. Terminating");
                System.Threading.Thread.Sleep(5000);
            }

            foreach(var filter in children.Where(e => !e.IsMulticolor))
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                var products = new HashSet<int>();

                if (cancelToken.IsCancellationRequested)
                    break;

                sendStatus(string.Format("Analyzing {0}...", filter.Name));
                System.Threading.Thread.Sleep(1000);

                // do analysis

                foreach(var rgbColor in filter.Colors)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    // make a bitmap with this color

                    using (var b = new System.Drawing.Bitmap(400, 400))
                    using (var g = System.Drawing.Graphics.FromImage(b))
                    {
                        System.Drawing.Color rgb = System.Drawing.Color.FromArgb(rgbColor.R, rgbColor.G, rgbColor.B);
                        g.FillRectangle(new System.Drawing.SolidBrush(rgb), 0, 0, 400, 400);

                        var bmsrc = b.ToBitmapSource();
                        var cedd = bmsrc.CalculateDescriptor();

                        var searchResults = Store.ImageSearch.FindSimilarProductsByColor(null, cedd, 30, 100);
                        foreach(var productID in searchResults)
                        {
                            if (cancelToken.IsCancellationRequested)
                                break;

                            // this one is a keeper
                            products.Add(productID);

                            // and now find others pretty much like it

                            var similarResults = Store.ImageSearch.FindSimilarProductsByColor(productID, 10, 20);
                            foreach(var productID2 in similarResults)
                            {
                                if (cancelToken.IsCancellationRequested)
                                    break;

                                products.Add(productID2);
                                //Store.ImageSearch.FindSimilarProducts(productID2, 20, 15).ForEach(e => products.Add(e));
                            }

#if false
                            if (cancelToken.IsCancellationRequested)
                                break;

                            var similarByColorResults = Store.ImageSearch.FindSimilarProductsByColor(productID, 10, 10);
                            foreach (var productID3 in similarByColorResults)
                            {
                                if (cancelToken.IsCancellationRequested)
                                    break;

                                products.Add(productID3);
                                //Store.ImageSearch.FindSimilarProducts(productID3, 20, 15).ForEach(e => products.Add(e));
                            }
#endif
                        }
                    }

                }

                sendStatus(string.Format("Saving {0}...", filter.Name));
                progress += progressPerFilter;
                sendProgress(progress);
                System.Threading.Thread.Sleep(1000);

                if (cancelToken.IsCancellationRequested)
                    break;

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {

                    // add delta -- but does not remove, so additive
                    //var existingProducts = new HashSet<int>(dc.ProductCategories.Where(e => e.CategoryID == filter.CategoryID).Select(e => e.ProductID));
                    //var toBeAddedProducts = products.Where(e => !existingProducts.Contains(e));
                    //dc.ProductCategories.AddProductCategoryAssociationsForCategory(filter.CategoryID, toBeAddedProducts);

                    // wipe and add
                    // remove any existing
                    dc.ProductCategories.RemoveProductCategoryAssociationsForCategory(filter.CategoryID);
                    dc.ProductCategories.AddProductCategoryAssociationsForCategory(filter.CategoryID, products);
                }

                progress += progressPerFilter;
                sendProgress(progress);
            }
        }

        #endregion
    }
}
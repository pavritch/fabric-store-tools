using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{

    public class RugsDescriptionMakerBase
    {
        private static Dictionary<string, string> dicColorMapper = new Dictionary<string, string>()
        {
            { "blacks", "Black" },
            { "silver & grays", "Silver & Gray" },
            { "blues", "Blue" },
            { "browns", "Brown" },
            { "burgundies", "Burgany" },
            { "greens", "Green" },
            { "multicolored", "Multicolor" },
            { "oranges", "Orange" },
            { "reds", "Red" },
            { "pinks", "Pink" },
            { "purples", "Purple" },
            { "cream & ivories", "Cream & Ivory" },
            { "whites", "White" },
            { "tans & beiges", "Tan & Biege" },
            { "yellows & golds", "Yellow & Gold" },
        };


        private static Dictionary<string, string> dicCountryMapper = new Dictionary<string, string>()
        {
            { "TURKEY", "Turkey" },
            { "INDIA", "India" },
            { "CHINA", "China" },
        };


        protected readonly InsideRugsProduct product;
        protected Dictionary<string, string> mergeCodes;
        protected bool hasColor;
        protected int countColors; // number of colors separated by slashes
        protected bool hasStyle;
        protected bool hasWeave;
        protected bool hasCountry;
        protected bool hasOutdoor;
        protected int countSizes = 1;
        protected int countShapes = 1;
        protected bool hasRound;
        protected bool hasOval;
        protected bool hasRectangular;
        protected bool hasRunners;


        protected readonly PhraseManager mgr;

        public RugsDescriptionMakerBase(InsideRugsProduct product)
        {
            this.product = product;

            PopulateMergeCodes();

            // the SKU is used only as a seed for random numbers
            mgr = new PhraseManager(product.SKU);

            //Debug.WriteLine(SEDescription);
        }

        /// <summary>
        /// The result of the generator.
        /// </summary>
        public string Description
        {
            get
            {
                return mgr.ToString();
            }
        }

        /// <summary>
        /// Populate a dictionary with merge codes from product attributes.
        /// </summary>
        protected virtual void PopulateMergeCodes()
        {
            mergeCodes = new Dictionary<string, string>();

            Action<string, string> add = (n, v) =>
            {
                if (string.IsNullOrEmpty(v))
                    return;

                mergeCodes[n] = v;
            };

            Func<string, string> InitialCap = (s) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return string.Empty;

                    return Char.ToUpper(s[0]) + s.Substring(1);
                };

            var style = string.Empty;
            if (product.Features.Tags != null && product.Features.Tags.Count() > 0)
            {
                var t = product.Features.Tags.Where(e => !e.ContainsIgnoreCase("outdoor")).FirstOrDefault();
                if (t != null)
                    style = t.ToLower();
            }

            if (style == "solids")
                style = "solid";

            if (!string.IsNullOrWhiteSpace(style))
                hasStyle = true;

            string color = null; 
            if (!string.IsNullOrWhiteSpace(product.Features.Color))
            {
                hasColor = true;
                color = product.Features.Color.ToLower().Replace(" ", "").Replace(",", "/");
                var ary = color.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
                countColors = ary.Length;
            }

            if (!hasColor)
            {
                // see about getting from our categories
                // parentID is 295 for colors

                var colorCategories = product.categories.Where(e => e.ParentCategoryID == 295).ToList();
                if (colorCategories.Count() > 0)
                {
                    // take only the first for now.
                    color = colorCategories.First().Name;

                    dicColorMapper.TryGetValue(color.ToLower(), out color);
                    color = color.Replace(" & ", "/");
                    hasColor = true;
                    countColors = 1;
                    color = color.ToLower();
                }
            }

            if (countColors > 3)
            {
                var ary = color.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                color = string.Join("/", ary.Take(3).ToArray());
                countColors = 3;
            }

            string weave = null;
            // see if we have a category - parent 431
            var weaveCategories = product.categories.Where(e => e.ParentCategoryID == 431).ToList();
            if (weaveCategories.Count() > 0)
            {
                // take only the first for now.
                weave = weaveCategories.First().Name.ToLower();
                hasWeave = true;
            }

            if (!hasWeave && !string.IsNullOrWhiteSpace(product.Features.Weave))
            {
                hasWeave = true;
                weave = product.Features.Weave.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(product.Features.CountryOfOrigin))
                hasCountry = true;

            countSizes = product.variants.Where(e => e.Dimensions != "Sample").Count();
                
            hasOutdoor = product.Features.Tags != null && product.Features.Tags.Any(e => e.ContainsIgnoreCase("outdoor"));

            countShapes = product.variants.Where(e => e.Dimensions != "Sample").Select(e => e.Dimensions).Distinct().Count();
            hasRound = product.VariantFeatures.Where(e => e.Shape == "Round").Count() > 0;
            hasOval = product.VariantFeatures.Where(e => e.Shape == "Oval").Count() > 0;
            hasRectangular = product.VariantFeatures.Where(e => e.Shape == "Rectangular").Count() > 0;

            hasRunners = product.VariantFeatures.Where(e => e.Shape == "Runner").Count() > 0;
            
            add("Brand", product.m.Name);

            add("SKU", product.SKU);
            add("MPN", product.ManufacturerPartNumber); // our correlator
            add("Name", product.p.Name.Replace(" by ", " area rug by "));

            if (hasRound && hasOval)
            {
                add("CountSizes", countSizes.ToString() + " sizes including round and oval");
            }
            else if (hasOval)
            {
                add("CountSizes", countSizes.ToString() + " sizes including oval");
            }
            else if (hasRound)
            {
                add("CountSizes", countSizes.ToString() + " sizes including round");
            }
            else 
                add("CountSizes", countSizes.ToString() + " sizes");

            add("CountShapes", countShapes.ToString());

            if (hasWeave)
            {
                add("Weave", InitialCap(weave));
                add("weave", weave);
            }

            if (hasColor)
            {
                add("Color", InitialCap(color));
                add("color", color);
            }

            if (hasCountry)
            {
                string country = null;
                if (dicCountryMapper.TryGetValue(product.Features.CountryOfOrigin, out country))
                    add("Country", country);
                else
                    add("Country", product.Features.CountryOfOrigin);
            }
                

            if (hasStyle)
            {
                add("Style", InitialCap(style));
                add("style", style);
            }

            add("Outdoor", hasOutdoor ? "Indoor/outdoor." : "");

            if (product.VariantFeatures.Where(e => e.Shape != "Sample").Select(e => e.Description).Distinct().Count() > 1)
            {
                var smallestSize = product.VariantFeatures.Where(e => e.Shape != "Sample").OrderBy(e => e.AreaSquareFeet).Select(e => e.Description).First();
                var largestSize = product.VariantFeatures.Where(e => e.Shape != "Sample").OrderByDescending(e => e.AreaSquareFeet).Select(e => e.Description).First();
                add("SizeRange", string.Format("{0} to {1}", smallestSize, largestSize));
            }

            var adjs = new string[] { "beautiful", "elegant", "gorgeous", "lovely", "charming", "delightful", "magnificent", "exquisite", "impressive", "attractive", "stunning", "superb", "engaging", "enchanting" };

            var rand = new Random();
            var adjPick1 = adjs[rand.Next(0, adjs.Length)];

            string adjPick2;
            do
            {
                adjPick2 = adjs[rand.Next(0, adjs.Length)];
            } while (adjPick2 == adjPick1);
            

            add("Adj", InitialCap(adjPick1));
            add("adj", adjPick1);

            add("Adj2", InitialCap(adjPick2));
            add("adj2", adjPick2);
            
        }

        protected string Merge(string s, bool encloseInHtml = false)
        {
            // string s will have {merge} phrases which get replaced.

            // if the dictionary does not have a given term, then return 
            // null to indicate a fail on this sentence so caller can resubmit or skip.

            try
            {
                var text = TranslateMergeCodesInText(s);

                if (encloseInHtml)
                    return string.Format("<p>{0}</p>", HttpUtility.HtmlEncode(text));

                return text;
            }
            catch
            {
                return null;
            }
        }

        protected string TranslateMergeCodesInText(string Text)
        {
            if (string.IsNullOrEmpty(Text))
                return string.Empty;

            return Regex.Replace(Text, @"\{(?<key>([#$a-zA-Z0-9\:\-])+)\}", delegate(Match match)
            {
                // the captured value here does not include the braces, to get the 
                // braces, use match.ToString() rather than match.Groups[1].ToString()

                var value = match.Groups[1].ToString();
                if (!mergeCodes.ContainsKey(value))
                    throw new Exception("Missing merge key: " + value);

                return TranslateMergeCode(value) ?? value;
            }, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        }


        protected string TranslateMergeCodesInHtml(string Text)
        {
            if (string.IsNullOrEmpty(Text))
                return string.Empty;

            return Regex.Replace(Text, @"\{(?<key>([#$a-zA-Z0-9\:\-])+)\}", delegate(Match match)
            {
                // the captured value here does not include the braces, to get the 
                // braces, use match.ToString() rather than match.Groups[1].ToString()

                var value = match.Groups[1].ToString();
                if (!mergeCodes.ContainsKey(value))
                    throw new Exception("Missing merge key: " + value);

                var mergedValue = TranslateMergeCode(value);

                return (mergedValue != null) ? HttpUtility.HtmlEncode(mergedValue) : value;

            }, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        }


        protected string TranslateMergeCode(string key)
        {
            return mergeCodes[key];
        }

    }


}
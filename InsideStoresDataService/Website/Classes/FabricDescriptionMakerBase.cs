using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{

    public class FabricDescriptionMakerBase
    {
        protected readonly InsideFabricProduct product;
        protected Dictionary<string, string> mergeCodes;

        protected bool hasCountry;
        protected bool hasBrand;
        protected bool hasSKU;
        protected bool hasName;
        protected bool hasGroup;
        protected bool hasColor;
        protected bool hasStyle;
        protected bool hasDesigner;
        protected bool hasWidth;
        protected bool hasCollection;
        protected bool hasPattern;
        protected bool hasMPN;

        protected readonly PhraseManager mgr;

        public FabricDescriptionMakerBase(InsideFabricProduct product)
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

            var dicProperties = product.OriginalRawProperties;

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

            var rand = new Random(product.p.ProductID); // productID is seed

            Func<string[], string> pickOne = (choices) =>
                {
                    return choices[rand.Next(0, choices.Length)];
                };


            if (dicProperties.ContainsKey("Country of Origin"))
            {
                string country = dicProperties["Country of Origin"];
                if (!string.IsNullOrWhiteSpace(country))
                {
                    hasCountry = true;
                    add("Country", country);
                }
            }


            // determine pattern

            foreach (var patternKey in new string[] { "Pattern Name", "Pattern"})
            {
                if (dicProperties.ContainsKey(patternKey))
                {
                    var patternValue = dicProperties[patternKey];
                    add("Pattern", patternValue);
                    hasPattern = true;
                }
            }


            if (dicProperties.ContainsKey("Collection"))
            {
                string collection = dicProperties["Collection"];
                if (!string.IsNullOrWhiteSpace(collection))
                {
                    hasCollection = true;
                    add("Collection", collection);
                }
            }

            if (dicProperties.ContainsKey("Designer"))
            {
                string designer = dicProperties["Designer"];
                if (!string.IsNullOrWhiteSpace(designer))
                {
                    hasDesigner = true;
                    add("Designer", designer);
                }
            }

            if (dicProperties.ContainsKey("Width"))
            {
                string width = dicProperties["Width"];
                if (!string.IsNullOrWhiteSpace(width))
                {
                    hasWidth = true;
                    add("width", width.Replace("\"", " inches").Replace(".00", ""));
                }
            }


            if (dicProperties.ContainsKey("Item Number"))
            {
                string mpn = dicProperties["Item Number"];
                if (!string.IsNullOrWhiteSpace(mpn))
                {
                    hasMPN = true;
                    add("MPN", mpn);
                }
            }
            else if (!string.IsNullOrEmpty(product.pv.ManufacturerPartNumber))
            {
                hasMPN = true;
                add("MPN", product.pv.ManufacturerPartNumber);
            }

            var brand = product.m.Name;
            if (brand.EndsWith(" Fabrics"))
                brand = brand.Replace(" Fabrics", "");
            if (brand.EndsWith(" Fabric"))
                brand = brand.Replace(" Fabric", "");
            if (brand.EndsWith(" Wallcoverings"))
                brand = brand.Replace(" Wallcoverings", "");
            if (brand.EndsWith(" Wallcovering"))
                brand = brand.Replace(" Wallcovering", "");

            add("Brand", brand);
            hasBrand = true;

            add("SKU", product.SKU);
            hasSKU = true;

            add("Name", product.p.Name);
            hasName = true;

            add("group", product.p.ProductGroup.ToLower());
            add("Group", product.p.ProductGroup);
            hasGroup = true;

            switch (product.ProductGroup)
            {
                case "Fabric":
                    add("kind", pickOne(new string[] {"fabric", "decorator fabric", "decorating fabric", "home fabric", "drapery and upholstery fabric", "upholstery fabric"}));
                    break;

                case "Trim":
                    add("kind", pickOne(new string[] {"trim", "decorator trim", "trimming", "decorator trimming"}));
                    break;

                case "Wallcovering":
                    add("kind", pickOne(new string[] {"wallcovering", "indoor wallcovering", "designer wallcovering"}));
                    break;

            }

            string color = null; // Color, Color Name, Color Group
            string style = null; // Style, Design, Category

            // determine color

            foreach (var colorKey in new string[] { "Color Name", "Color", "Color Group" })
            {
                if (dicProperties.ContainsKey(colorKey))
                {
                    var colorValue = dicProperties[colorKey];
                    var colorTokens = new List<string>();
                    foreach (var ct in colorValue.Split(new char[] { '/', ',', '&', ';' }))
                    {
                        var trimToken = ct.Trim();
                        if (trimToken.Length == 0 || trimToken.Any(e => Char.IsNumber(e)) || trimToken.Any(e => Char.IsSymbol(e)))
                            continue;

                        colorTokens.Add(trimToken);
                    }

                    if (colorTokens.Count() > 0)
                    {
                        color = string.Join("/", colorTokens.Take(3).ToArray());
                        break;
                    }
                }
            }

            // determine style

            foreach (var styleKey in new string[] { "Style", "Design", "Type" })
            {
                if (dicProperties.ContainsKey(styleKey))
                {
                    var styleValue = dicProperties[styleKey];
                    var styleTokens = new List<string>();
                    foreach (var ct in styleValue.Split(new char[] { '/', ',', '&', ';' }))
                    {
                        var trimToken = ct.Trim();
                        if (trimToken.Length == 0 || trimToken.Any(e => Char.IsNumber(e)) || trimToken.Any(e => Char.IsSymbol(e)))
                            continue;

                        styleTokens.Add(trimToken);
                    }

                    if (styleTokens.Count() > 0)
                    {
                        // take only the first
                        style = styleTokens[0];
                        break;
                    }
                }
            }
            
            if (color != null)
            {
                hasColor = true;
                add("Color", InitialCap(color));
                add("color", color.ToLower());
            }

            if (style != null)
            {
                hasStyle = true;
                add("Style", InitialCap(style));
                add("style", style.ToLower());
            }

            var adjs = new string[] { "beautiful", "captivating", "inspiring", "elegant", "spectacular", "striking", "fabulous", "exceptional", "remarkable", "fantastic", "splendid", 
                "marvelous", "extraordinary", "amazing", "dazzling", "gorgeous", "refreshing", "lovely", "charming", "delightful", "deluxe", "magnificent", "memorable", "exquisite", 
                "fashionable", "popular", "handsome", "impressive", "attractive", "stunning", "superb", "engaging", "enchanting", "stylish", "inviting", "impeccable", "tasteful" };

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
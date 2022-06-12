using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using InsideFabric.Data;

namespace Website
{
    public class InsideFabricSearchGalleryManager : SearchGalleryManager
    {
        // these are taken from the SQL match queries, maybe use as extra tags on the pages.
        // where used for tags, duplicates weeded out, all lower case
        private static Dictionary<int, List<string>> extraColorNames = new Dictionary<int, List<string>>()
        {
            {38, new List<string>() { "beige", "cream", "taupe", "biscuit", "neutral", "oatmeal", "sand", "camel", "fawn", "khaki", "mushroom", "champagne", "parchment", "chablis", "sandalwood", "creme brulee", "caramel", "truffle", "barley", "wheat"}},
            {40, new List<string>() { "black", "coal", "ebony", "onyx", "jet", "slate", "ink", "noir"}},
            {43, new List<string>() { "blue", "sky", "light blue", "azure", "cobalt", "robin's egg", "turquoise", "indigo", "cerulean", "midnight", "aqua", "ice blue", "navy", "teal", "baby blue"}},
            {39, new List<string>() { "brown", "chocolate", "mocha", "coffee", "bronze", "cinnamon", "cocoa", "copper", "auburn", "amber", "umber", "expresso", "sienna", "mahogany", "ochre", "russet", "chestnut", "earth", "cappucino", "cafe", "java", "cognac", "hazelnut", "praline"}},
            {44, new List<string>() { "red", "burgundy", "bordeaux", "brick", "cherry", "claret", "magenta", "garnet", "wine", "venetian", "quince", "tuscan", "watermelon", "paprika", "port", "pomegranate", "grenadine", "ruby"}},
            {47, new List<string>() { "gold", "yellow", "amber", "lemon", "vermillion", "butter", "copper", "cashew", "coin"}},
            {45, new List<string>() { "green", "light green", "malachite", "moss", "olive", "aquamarine", "jade", "kelly", "apple", "blue grass", "meadow", "mint", "clover", "sage", "basil", "vert", "hunter", "grass", "celadon", "leaf"}},
            {54, new List<string>() { "grey", "charcoal", "silver", "pearl", "smoke", "stone", "ash", "slate", "graphite", "sterling"}},
            {55, new List<string>() { "orange", "rust", "spice", "cinnamon", "coral", "peach", "salmon", "tangerine", "apricot", "cinnabar", "persimmon", "copper", "brick"}},
            {53, new List<string>() { "pink", "blush", "rose", "fuschia", "coral", "melon", "strawberry", "cerise", "cherry", "poppy", "peony", "berry", "cranberry", "primrose"}},
            {50, new List<string>() { "purple", "lavender", "violet", "plum", "wine", "magenta", "mauve", "lilac", "rasberry", "wisteria", "amethyst"}},
            {56, new List<string>() { "white", "off white", "cream", "snow", "ivory", "winter"}},
        };

        private Dictionary<int, string> dicManufacturers;
        private Dictionary<int, string> dicCategories;

        public InsideFabricSearchGalleryManager(IWebStore store) : base (store)
        {
            doSpinQueries();
            // use below for debug only, since uses parallel loop and sucks up all CPU
#if DEBUG
            //doSpinFullRecords();
            // doSpinFullRecordsDelta();
#endif
        }



        protected override void BeginSpinProcessing()
        {
            PopulateSpinData();
        }

        protected override void EndSpinProcessing()
        {
            UnPopulateSpinData();
        }

        private void PopulateSpinData()
        {

            try
            {

                using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                {
                    dicManufacturers = dc.Manufacturers.Where(e => e.Published == 1 && e.Deleted == 0).Select(e => new { e.ManufacturerID, e.Name}).ToDictionary(k => k.ManufacturerID, v => v.Name);
                    dicCategories = dc.Categories.Where(e => e.Published == 1 && e.Deleted == 0).Select(e => new { e.CategoryID, e.Name }).ToDictionary(k => k.CategoryID, v => v.Name);

                    foreach (var key in dicManufacturers.Keys.ToList())
                    {
                        var value = dicManufacturers[key];
                        if (!value.StartsWith("JF "))
                            dicManufacturers[key] = value.Replace(" Fabrics", "").Replace(" Fabric", "").Trim();
                    }

                    foreach (var key in dicCategories.Keys.ToList())
                    {
                        var value = dicCategories[key];
                        dicCategories[key] = value.Replace(" Fabric", "").Trim();
                    }

                    // only use first component when has slash
                    foreach (var key in dicCategories.Keys.ToList())
                    {
                        var value = dicCategories[key];
                        if (value.Contains("/"))
                        {
                            var ary = value.Split('/');
                            dicCategories[key] = ary[0].Trim();
                        }
                    }
                }
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        private void UnPopulateSpinData()
        {
            dicManufacturers = null;
            dicCategories = null;
        }


        // facet keys:  Manufacturer, Type, Color, Pattern, Designer, Trim, Price
        // nothing permutates with price at the moment

        #region Spin Content
        // SEName
        protected override string SpinSEName(FacetSearchCriteria criteria)
        {
            return SpinName(criteria).MakeSafeSEName();
        }

        // SETitle
        protected override string SpinSETitle(FacetSearchCriteria criteria)
        {
            return SpinName(criteria);
        }

        // SEDescription
        protected override string SpinSEDescription(FacetSearchCriteria criteria)
        {
            var maker = new GallerySEDescriptionMaker(criteria, SpinName(criteria), dicCategories, dicManufacturers);
            return maker.Description;
        }

        // SEKeywords
        protected override string SpinSEKeywords(FacetSearchCriteria criteria)
        {
            var list = new List<string>()
            {
                "drapery fabric",
                "upholstery fabric",
                "fabric store",
                "discount fabric"
            };

            list.AddRange(GetExplicitTags(criteria));

            return list.ToCommaDelimitedList().ToLower();
        }


        private string GetQueryCategoryName(FacetSearchCriteria criteria, string key)
        {
            foreach (var facet in criteria.Facets.Where(e => e.FacetKey == key))
            {
                return dicCategories[facet.Members.First()];
            }

            return null;
        }

        private int? GetQueryCategoryId(FacetSearchCriteria criteria, string key)
        {
            foreach (var facet in criteria.Facets.Where(e => e.FacetKey == key))
            {
                return facet.Members.First();
            }

            return null;
        }


        private string GetQuerManufacturerName(FacetSearchCriteria criteria)
        {
            foreach (var facet in criteria.Facets.Where(e => e.FacetKey == "Manufacturer"))
            {
                return dicManufacturers[facet.Members.First()];
            }

            return null;
        }

        
        // Name
        protected override string SpinName(FacetSearchCriteria criteria)
        {
            // green upholstery and drapery fabric
            // chevron upholstery and drapery fabric
            // green stripes upholstery and drapery fabric
            // green stripes upholstery and drapery fabric by kravet|designer
            // brown fringe
            // brown fringe by Kravet|designer

            #region Conditionals
            Func<string> qManufacturer = () =>
            {
                return GetQuerManufacturerName(criteria);
            };

            Func<bool> qHasManufacturer = () =>
            {
                return qManufacturer() != null;
            };


            Func<string> qColor = () =>
            {
                return GetQueryCategoryName(criteria, "Color");
            };

            Func<bool> qHasColor = () =>
                {
                    return qColor() != null;
                };

            Func<string> qType = () =>
            {
                return GetQueryCategoryName(criteria, "Type");
            };

            Func<bool> qHasType = () =>
            {
                return qType() != null;
            };


            Func<string> qPattern = () =>
            {
                return GetQueryCategoryName(criteria, "Pattern");
            };

            Func<bool> qHasPattern = () =>
            {
                return qPattern() != null;
            };


            Func<string> qDesigner = () =>
            {
                return GetQueryCategoryName(criteria, "Designer");
            };

            Func<bool> qHasDesigner = () =>
            {
                return qDesigner() != null;
            };

            Func<string> qTrim = () =>
            {
                return GetQueryCategoryName(criteria, "Trim");
            };

            Func<bool> isTrim = () =>
            {
                return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 0;
            };
            Func<bool> isMutiTrim = () =>
            {
                return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 1;
            };

            #endregion

            var sb = new StringBuilder(128);

            if (qHasColor())
                sb.Append(qColor());

            if (qHasPattern())
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(qPattern());
            }

            if (qHasType())
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(qType());
            }

            if (isTrim())
            {
                // trim
                if (isMutiTrim())
                    sb.Append(" Trim");
                else
                    sb.AppendFormat(" {0}", qTrim());
            }
            else
            {
                // fabric

                if (qHasType() && qType() == "Outdoor")
                {
                    sb.Append(" Fabric");
                }
                else if (qHasType() && qType() == "Drapery")
                {
                    sb.Append(" Fabric");
                }
                else if (qHasType() && qType() == "Upholstery")
                {
                    sb.Append(" Fabric");
                }
                else
                    sb.Append(" Upholstery and Drapery Fabric");
            }

            if (qHasManufacturer())
                sb.AppendFormat(" by {0}", qManufacturer());
            else if (qHasDesigner())
                sb.AppendFormat(" by {0}", qDesigner());

            return sb.ToString().Trim();
        }


        // Description
        protected override string SpinDescription(FacetSearchCriteria criteria)
        {
            return null;
        }

        // AnchorTextList
        protected override string SpinAnchorTextList(FacetSearchCriteria criteria)
        {

            #region Conditionals
            Func<string> qManufacturer = () =>
            {
                return GetQuerManufacturerName(criteria);
            };

            Func<bool> qHasManufacturer = () =>
            {
                return qManufacturer() != null;
            };


            Func<string> qColor = () =>
            {
                return GetQueryCategoryName(criteria, "Color");
            };

            Func<bool> qHasColor = () =>
            {
                return qColor() != null;
            };

            Func<string> qType = () =>
            {
                return GetQueryCategoryName(criteria, "Type");
            };

            Func<bool> qHasType = () =>
            {
                return qType() != null;
            };


            Func<string> qPattern = () =>
            {
                return GetQueryCategoryName(criteria, "Pattern");
            };

            Func<bool> qHasPattern = () =>
            {
                return qPattern() != null;
            };


            Func<string> qDesigner = () =>
            {
                return GetQueryCategoryName(criteria, "Designer");
            };

            Func<bool> qHasDesigner = () =>
            {
                return qDesigner() != null;
            };

            Func<string> qTrim = () =>
            {
                return GetQueryCategoryName(criteria, "Trim");
            };

            Func<bool> isTrim = () =>
            {
                return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 0;
            };
            Func<bool> isMutiTrim = () =>
            {
                return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 1;
            };
            #endregion

            var list = new List<string>();

            var name = SpinName(criteria);
            list.Add(name);
            var plural = name.Replace(" Fabric", " Fabrics").Replace(" Wallcovering", " Wallcoverings").Replace(" Wallpaper", " Wallpapers").Replace(" Trim", " Trims").Replace(" Trimming", " Trimmings");
            // do some corrections
            plural = plural.Replace(" Fabricss", " Fabrics").Replace(" Wallcoveringss", " Wallcoverings").Replace("Fabricsut", "Fabricut");
            list.Add(plural);

            return list.ConvertToLines();
        }

        // Tags - very complete set, but not to contain things that are too much like sales stuff
        protected override string SpinTags(FacetSearchCriteria criteria)
        {
            var list = new List<string>()
            {
                "drapery fabric",
                "upholstery fabric",
            };

            var hash = new HashSet<string>();

            GetExplicitTags(criteria).ForEach(e => hash.Add(e.ToLower()));

            var colorCategoryID = GetQueryCategoryId(criteria, "Color");
            if (colorCategoryID.HasValue)
            {
                List<string> extraColors;
                if (extraColorNames.TryGetValue(colorCategoryID.Value, out extraColors))
                    extraColors.ForEach(e => hash.Add(e.ToLower()));
            }

            var colorName = GetQueryCategoryName(criteria, "Color");
            if (colorName != null)
            {
                var lowerName = colorName.ToLower().Trim();
                if (lowerName != "white" && lowerName != "black")
                {
                    hash.Add("light " + colorName);
                    hash.Add("dark " + colorName);
                }
            }

            foreach (var item in hash)
                list.Add(item);

            return list.ToCommaDelimitedList();
        }

        private List<string> GetExplicitTags(FacetSearchCriteria criteria)
        {
            var list = new List<string>();

            // see if has manufacturer association

            foreach (var facet in criteria.Facets.Where(e => e.FacetKey == "Manufacturer"))
            {
                // will be 0 or 1 result
                foreach (var id in facet.Members)
                    if (dicManufacturers.ContainsKey(id))
                        list.Add(dicManufacturers[id]);
            }

            // then add in any categories

            foreach (var facet in criteria.Facets.Where(e => e.FacetKey != "Manufacturer"))
            {
                foreach (var id in facet.Members)
                    if (dicCategories.ContainsKey(id))
                        list.Add(dicCategories[id]);
            }

            if (criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 0)
            {
                list.Add("Trim");
                list.Add("Trimming");
            }


            return list;
        }

        #endregion

        #region SEDescription Maker

        private class GallerySEDescriptionMaker : GalleryDescriptionMakerBase
        {
            public GallerySEDescriptionMaker(FacetSearchCriteria criteria, string name, Dictionary<int, string> dicCategories, Dictionary<int, string> dicManufacturers)
                : base(criteria, name, dicCategories, dicManufacturers)
            {

                List<Action<PhraseManager>> list = (hasFabric) ? FabricMethods() : TrimMethods();
                mgr.AddContributions(list);
            }

            #region Action Method Lists
            private List<Action<PhraseManager>> FabricMethods()
            {
                var list = new List<Action<PhraseManager>>()
                {
                    ContributeFabricIntro,
                    ContributeFabricDesigner,
                    ContributeFabricOther,
                    ContributeFabricPostamble,
                };

                return list;
            }

            private List<Action<PhraseManager>> TrimMethods()
            {
                var list = new List<Action<PhraseManager>>()
                {
                    ContributeTrimIntro,
                    ContributeTrimDesigner,
                    ContributeTrimOther,
                    ContributeTrimPostamble,
                };
                return list;
            }

            #endregion

            #region Description Contributors

            private void ContributeFabricIntro(PhraseManager mgr)
            {
                List<PhraseVariant> currentList = null;

                Action<string, int> add = (s, w) =>
                {
                    // will have null input when a merge fails to resolve; so not used
                    if (!string.IsNullOrWhiteSpace(s))
                        currentList.Add(new PhraseVariant(s, w));
                };

                var list1 = new List<PhraseVariant>();
                currentList = list1;

                add(Merge("{Adj2} {altname}."), 100);
                add(Merge("{AltName}."), 100);

                var list2 = new List<PhraseVariant>();

                currentList = list2;

                if (hasBrand || hasDesigner)
                {
                    add(Merge("Free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} luxury fabric."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} luxury fabrics."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} designer fabric."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} designer fabrics."), 100);
                    add(Merge("Fast, free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Fast, free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Fast, free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Big discounts and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Big discounts and free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Low prices and fast free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Low prices and fast free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Low prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Best prices and fast free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Best prices and fast free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Lowest prices and fast free shipping on {BrandOrDesigner} fabric."), 100);
                    add(Merge("Lowest prices and fast free shipping on {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Lowest prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Save on {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner} luxury fabric. Free shipping!"), 100);
                    add(Merge("Save big on {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Save big on {BrandOrDesigner}. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner}. Big discounts and free shipping!"), 100);
                    add(Merge("Huge savings on {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Huge savings on {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Huge savings on {BrandOrDesigner} luxury fabric. Free shipping!"), 100);

                    add(Merge("Free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} luxury fabric."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} luxury fabrics."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} designer fabric."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} designer fabrics."), 100);
                    add(Merge("Fast, free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Fast, free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Big discounts and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Big discounts and free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Low prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} {BrandOrDesigner} fabric."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} {BrandOrDesigner} fabrics."), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} luxury fabric. Free shipping!"), 100);
                    add(Merge("Save big on {adj} {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Save big on {adj} {BrandOrDesigner}. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner}. Big discounts and free shipping!"), 100);
                    add(Merge("Huge savings on {adj} {BrandOrDesigner} fabric. Free shipping!"), 100);
                    add(Merge("Huge savings on {adj} {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Huge savings on {adj} {BrandOrDesigner} luxury fabric. Free shipping!"), 100);

                }
                else
                {
                    add("Free shipping on most orders.", 100);
                    add("Free shipping on fabric.", 100);
                    add("Free shipping on fabrics.", 100);
                    add("Free shipping on decorating products.", 100);
                    add("Free shipping on luxury fabric.", 100);
                    add("Free shipping on luxury fabrics.", 100);
                    add("Free shipping on designer fabric.", 100);
                    add("Free shipping on designer fabrics.", 100);
                    add("Fast, free shipping.", 100);
                    add("Fast, free shipping on fabric.", 100);
                    add("Fast, free shipping on fabrics.", 100);
                    add("Best prices and free shipping on fabric.", 100);
                    add("Discount pricing and free shipping on fabric.", 100);
                    add("Discount pricing and free shipping on fabrics.", 100);
                    add("Big discounts and free shipping on fabric.", 100);
                    add("Big discounts and free shipping on fabrics.", 100);
                    add("Low prices and free shipping on fabrics.", 100);
                    add("Low prices and free shipping on fabric.", 100);
                    add("Low prices and free shipping on decorating products.", 100);
                    add("Low prices and free shipping.", 100);
                    add("Low prices and fast free shipping on fabric.", 100);
                    add("Low prices and fast free shipping on fabrics.", 100);
                    add("Low prices and fast free shipping.", 100);
                    add("Best prices and free shipping on fabrics.", 100);
                    add("Best prices and free shipping on fabric.", 100);
                    add("Best prices and free shipping on decorating products.", 100);
                    add("Best prices and free shipping.", 100);
                    add("Best prices and fast free shipping on fabric.", 100);
                    add("Best prices and fast free shipping on fabrics.", 100);
                    add("Best prices and fast free shipping.", 100);
                    add("Lowest prices and free shipping on fabrics.", 100);
                    add("Lowest prices and free shipping on fabric.", 100);
                    add("Lowest prices and free shipping on decorating products.", 100);
                    add("Lowest prices and free shipping.", 100);
                    add("Lowest prices and fast free shipping on fabric.", 100);
                    add("Lowest prices and fast free shipping on fabrics.", 100);
                    add("Lowest prices and fast free shipping.", 100);
                    add("Save on fabric. Free shipping!", 100);
                    add("Save on fabric products. Free shipping!", 100);
                    add("Save on luxury fabric. Free shipping!", 100);
                    add("Save big on fabric. Free shipping!", 100);
                    add("Save big on home decorating products. Free shipping!", 100);
                    add("Save on our massive collection of decorator fabrics. Big discounts and free shipping!", 100);
                    add("Huge savings on our massive assortment of fabric. Free shipping!", 100);
                    add("Huge savings on our massive assortment of fabric products. Free shipping!", 100);
                    add("Huge savings on our massive selection of luxury fabric. Free shipping!", 100);

                    add(Merge("Free shipping on {adj} fabric."), 100);
                    add(Merge("Free shipping on {adj} fabrics."), 100);
                    add(Merge("Free shipping on {adj} decorating products."), 100);
                    add(Merge("Free shipping on luxury fabric."), 100);
                    add(Merge("Free shipping on luxury fabrics."), 100);
                    add(Merge("Free shipping on {adj} designer fabric."), 100);
                    add(Merge("Free shipping on {adj} designer fabrics."), 100);
                    add(Merge("Fast, free shipping on {adj} fabric."), 100);
                    add(Merge("Fast, free shipping on {adj} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} fabric."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} fabrics."), 100);
                    add(Merge("Big discounts and free shipping on {adj} fabric."), 100);
                    add(Merge("Big discounts and free shipping on {adj} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {adj} fabrics."), 100);
                    add(Merge("Low prices and free shipping on {adj} fabric."), 100);
                    add(Merge("Low prices and free shipping on {adj} decorating products."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} fabric."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} fabrics."), 100);
                    add(Merge("Best prices and free shipping on {adj} fabric."), 100);
                    add(Merge("Best prices and free shipping on {adj} decorating products."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} fabric."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} fabrics."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} fabrics."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} fabric."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} decorating products."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} fabric."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} fabrics."), 100);
                    add(Merge("Save on {adj} fabric. Free shipping!"), 100);
                    add(Merge("Save on {adj} fabric products. Free shipping!"), 100);
                    add(Merge("Save on {adj} luxury fabric. Free shipping!"), 100);
                    add(Merge("Save {adj} big on fabric. Free shipping!"), 100);
                    add(Merge("Save big on {adj} home decorating products. Free shipping!"), 100);

                }

                if (list1.Count() > 0)
                {
                    mgr.BeginPhraseSet();
                    mgr.PickAndAddPhraseVariant(list1);
                    mgr.EndPhraseSet();
                }

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list2);
                mgr.EndPhraseSet();

            }

            private void ContributeTrimIntro(PhraseManager mgr)
            {
                List<PhraseVariant> currentList = null;

                Action<string, int> add = (s, w) =>
                {
                    // will have null input when a merge fails to resolve; so not used
                    if (!string.IsNullOrWhiteSpace(s))
                        currentList.Add(new PhraseVariant(s, w));
                };

                var list1 = new List<PhraseVariant>();
                currentList = list1;

                add(Merge("{Adj2} {altname}."), 100);
                add(Merge("{AltName}."), 100);

                var list2 = new List<PhraseVariant>();
                currentList = list2;

                if (hasBrand || hasDesigner)
                {
                    add(Merge("Free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} luxury trims."), 100);
                    add(Merge("Free shipping on {BrandOrDesigner} designer trims."), 100);
                    add(Merge("Fast, free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Fast, free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Discount pricing and free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Low prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Low prices and fast free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Low prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Best prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Best prices and fast free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner} products."), 100);
                    add(Merge("Lowest prices and free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Lowest prices and fast free shipping on {BrandOrDesigner} trims."), 100);
                    add(Merge("Lowest prices and fast free shipping on {BrandOrDesigner}."), 100);
                    add(Merge("Save on {BrandOrDesigner} trims, cords and tassels. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner} luxury trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {BrandOrDesigner} trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {BrandOrDesigner}. Free shipping!"), 100);
                    add(Merge("Save on {BrandOrDesigner}. Big discounts and free shipping!"), 100);


                    add(Merge("Free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} luxury trims."), 100);
                    add(Merge("Free shipping on {adj} {BrandOrDesigner} designer trims."), 100);
                    add(Merge("Fast, free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Low prices and free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Low prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} {BrandOrDesigner} products."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} {BrandOrDesigner} trims."), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} trims, cords and tassels. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} products. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner} luxury trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {adj} {BrandOrDesigner} trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {adj} {BrandOrDesigner}. Free shipping!"), 100);
                    add(Merge("Save on {adj} {BrandOrDesigner}. Big discounts and free shipping!"), 100);
                
                }
                else
                {
                    add("Free shipping.", 100);
                    add("Free shipping on trims.", 100);
                    add("Free shipping on home decorating products.", 100);
                    add("Free shipping on luxury trims.", 100);
                    add("Free shipping on designer trims.", 100);
                    add("Fast, free shipping.", 100);
                    add("Fast, free shipping on trims.", 100);
                    add("Best prices and free shipping on trims.", 100);
                    add("Discount pricing and free shipping on trims.", 100);
                    add("Low prices and free shipping on trims.", 100);
                    add("Low prices and free shipping on home decorating products.", 100);
                    add("Low prices and free shipping.", 100);
                    add("Low prices and fast free shipping on trims.", 100);
                    add("Low prices and fast free shipping.", 100);
                    add("Best prices and free shipping on trims.", 100);
                    add("Best prices and free shipping on home decorating products.", 100);
                    add("Best prices and free shipping.", 100);
                    add("Best prices and fast free shipping on trims.", 100);
                    add("Best prices and fast free shipping.", 100);
                    add("Lowest prices and free shipping on trims.", 100);
                    add("Lowest prices and free shipping on home decorating products.", 100);
                    add("Lowest prices and free shipping.", 100);
                    add("Lowest prices and fast free shipping on trims.", 100);
                    add("Lowest prices and fast free shipping.", 100);
                    add("Save on trims, cords and tassels. Free shipping!", 100);
                    add("Save on trims and decorating products. Free shipping!", 100);
                    add("Save on luxury trims and cords. Free shipping!", 100);
                    add("Save big on trims and cords. Free shipping!", 100);
                    add("Save big on trimming and fabric products. Free shipping!", 100);
                    add("Save on trimming. Big discounts and free shipping!", 100);

                    add(Merge("Free shipping on {adj} trims."), 100);
                    add(Merge("Free shipping on {adj} home decorating products."), 100);
                    add(Merge("Free shipping on {adj} luxury trims."), 100);
                    add(Merge("Free shipping on {adj} designer trims."), 100);
                    add(Merge("Fast, free shipping on {adj} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} trims."), 100);
                    add(Merge("Discount pricing and free shipping on {adj} trims."), 100);
                    add(Merge("Low prices and free shipping on {adj} trims."), 100);
                    add(Merge("Low prices and free shipping on {adj} home decorating products."), 100);
                    add(Merge("Low prices and fast free shipping on {adj} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} trims."), 100);
                    add(Merge("Best prices and free shipping on {adj} home decorating products."), 100);
                    add(Merge("Best prices and fast free shipping on {adj} trims."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} trims."), 100);
                    add(Merge("Lowest prices and free shipping on {adj} home decorating products."), 100);
                    add(Merge("Lowest prices and fast free shipping on {adj} trims."), 100);
                    add(Merge("Save on {adj} trims, cords and tassels. Free shipping!"), 100);
                    add(Merge("Save on {adj} trims and decorating products. Free shipping!"), 100);
                    add(Merge("Save on {adj} luxury trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {adj} trims and cords. Free shipping!"), 100);
                    add(Merge("Save big on {adj} trimming and fabric products. Free shipping!"), 100);
                    add(Merge("Save on {adj} trimming. Big discounts and free shipping!"), 100);


                }

                if (list1.Count() > 0)
                {
                    mgr.BeginPhraseSet();
                    mgr.PickAndAddPhraseVariant(list1);
                    mgr.EndPhraseSet();
                }

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list2);
                mgr.EndPhraseSet();
            }

            private void ContributeFabricOther(PhraseManager mgr)
            {
                var list1 = new List<PhraseVariant>()
                {
                    new PhraseVariant("Only 1st Quality.", 100),
                    new PhraseVariant("Only first quality.", 100),
                    new PhraseVariant("Always 1st Quality.", 100),
                    new PhraseVariant("Always first quality.", 100),
                    new PhraseVariant("Strictly 1st Quality.", 100),
                    new PhraseVariant("Strictly first quality.", 100),
                };

                var list2 = new List<PhraseVariant>()
                {
                    new PhraseVariant("Search thousands of patterns.", 100),
                    new PhraseVariant("Search thousands of fabric patterns.", 100),
                    new PhraseVariant("Find thousands of patterns.", 100),
                    new PhraseVariant("Over 100,000 patterns.", 100),
                    new PhraseVariant("Over 100,000 fabric patterns.", 100),

                    new PhraseVariant("Search thousands of designer fabrics.", 100),
                    new PhraseVariant("Find thousands of designer patterns.", 100),
                    new PhraseVariant("Over 100,000 designer patterns.", 100),

                    new PhraseVariant("Search thousands of luxury fabrics.", 100),
                    new PhraseVariant("Find thousands of luxury patterns.", 100),
                    new PhraseVariant("Over 100,000 luxury patterns and colors.", 100),
                };

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list1);

                if (!hasDesigner)
                    mgr.PickAndAddPhraseVariant(list2);

                mgr.EndPhraseSet();
            }

            private void ContributeTrimOther(PhraseManager mgr)
            {

                var list2 = new List<PhraseVariant>()
                {
                    new PhraseVariant("Search thousands of items.", 100),
                    new PhraseVariant("Search thousands of products.", 100),
                    new PhraseVariant("Over 100,000 fabric patterns.", 100),
                    new PhraseVariant("Search thousands of designer trims.", 100),
                    new PhraseVariant("Find thousands of designer trims.", 100),
                    new PhraseVariant("Search thousands of luxury trims.", 100),
                };

                mgr.BeginPhraseSet();

                if (!hasDesigner)
                    mgr.PickAndAddPhraseVariant(list2);

                mgr.EndPhraseSet();
            }

            private void ContributeFabricPostamble(PhraseManager mgr)
            {
                List<PhraseVariant> currentList = null;

                Action<string, int> add = (s, w) =>
                {
                    // will have null input when a merge fails to resolve; so not used
                    if (!string.IsNullOrWhiteSpace(s))
                        currentList.Add(new PhraseVariant(s, w));
                };

                var list1 = new List<PhraseVariant>();
                currentList = list1;

                add(Merge("Sold by the yard."), 50);
                add(Merge("Swatches available."), 100);

                var list2 = new List<PhraseVariant>();
                currentList = list2;

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list1);
                if (list2.Count() > 0)
                    mgr.PickAndAddPhraseVariant(list2);
                mgr.EndPhraseSet();
            }

            private void ContributeTrimPostamble(PhraseManager mgr)
            {

                List<PhraseVariant> currentList = null;

                Action<string, int> add = (s, w) =>
                {
                    // will have null input when a merge fails to resolve; so not used
                    if (!string.IsNullOrWhiteSpace(s))
                        currentList.Add(new PhraseVariant(s, w));
                };

                var list1 = new List<PhraseVariant>();
                currentList = list1;

                add(Merge("Sold by the yard."), 50);
                add(Merge("Samples available."), 100);

                if (list1.Count() > 0)
                {
                    mgr.BeginPhraseSet();
                    mgr.PickAndAddPhraseVariant(list1);
                    mgr.EndPhraseSet();
                }
            }

            private void ContributeFabricDesigner(PhraseManager mgr)
            {
                // contribute single sentence about designer.

                if (!hasDesigner)
                    return;

                var list = new List<PhraseVariant>()
                {
                    new PhraseVariant(Merge("Featuring {Designer} and many other top designers."), 100),
                    new PhraseVariant(Merge("Featuring {Designer} and many other designers."), 100),
                    new PhraseVariant(Merge("{Designer} and many other top designers."), 100),
                    new PhraseVariant(Merge("{Designer} and many other designers."), 100),
                    new PhraseVariant(Merge("Featuring the {Designer} collection."), 100),
                    new PhraseVariant(Merge("Search the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("See the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("Find the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other designers."), 20),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other top designers."), 20),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other popular designers."), 20),
                    new PhraseVariant(Merge("Search the entire {Designer} collection and thousands of popular patterns."), 20),
                    new PhraseVariant(Merge("Search the entire {Designer} collection and thousands of other popular fabrics."), 20),
                    new PhraseVariant(Merge("See the entire {Designer} collection and thousands of designer patterns."), 20),
                    new PhraseVariant(Merge("See the entire {Designer} collection and thousands of designer fabrics."), 20),
                    new PhraseVariant(Merge("Find the entire {Designer} collection along with thousands of top designer fabrics"), 20),
                };

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list);
                mgr.EndPhraseSet();
            }

            private void ContributeTrimDesigner(PhraseManager mgr)
            {
                // contribute single sentence about designer.

                if (!hasDesigner)
                    return;

                var list = new List<PhraseVariant>()
                {
                    new PhraseVariant(Merge("Featuring {Designer} and many other top designers."), 100),
                    new PhraseVariant(Merge("Featuring {Designer} and many other designers."), 100),
                    new PhraseVariant(Merge("{Designer} and many other top designers."), 100),
                    new PhraseVariant(Merge("{Designer} and many other designers."), 100),
                    new PhraseVariant(Merge("Featuring the {Designer} collection."), 100),
                    new PhraseVariant(Merge("Search the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("See the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("Find the entire {Designer} collection."), 100),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other designers."), 20),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other top designers."), 20),
                    new PhraseVariant(Merge("Featuring the {Designer} collection and many other popular designers."), 20),
                    new PhraseVariant(Merge("Search the entire {Designer} collection and thousands of popular patterns."), 20),
                    new PhraseVariant(Merge("Search the entire {Designer} collection and thousands of other popular home fabric trims."), 20),
                    new PhraseVariant(Merge("See the entire {Designer} collection and thousands of designer fabrics trims"), 20),
                    new PhraseVariant(Merge("Find the entire {Designer} collection along with thousands of top designer fabric trims."), 20),
                };

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list);
                mgr.EndPhraseSet();
            }

            #endregion

        }

        private class GalleryDescriptionMakerBase
        {
            protected FacetSearchCriteria criteria;
            protected Dictionary<int, string> dicCategories;
            protected Dictionary<int, string> dicManufacturers;
            protected string name;

            protected int randomSeed;
            protected Dictionary<string, string> mergeCodes;

            protected bool hasName;
            protected bool hasBrand;
            protected bool hasDesigner;
            protected bool hasColor;
            protected bool hasType;
            protected bool hasPattern;
            protected bool hasFabric;
            protected bool hasTrim;

            protected readonly PhraseManager mgr;

            public GalleryDescriptionMakerBase(FacetSearchCriteria criteria, string name, Dictionary<int, string> dicCategories, Dictionary<int, string> dicManufacturers)
            {
                this.criteria = criteria;
                this.name = name;
                this.dicCategories = dicCategories;
                this.dicManufacturers = dicManufacturers;

                randomSeed = criteria.ToJSON(SearchGalleryManager.SerializerSettings).ToSeed();
                mgr = new PhraseManager(new Random(randomSeed));

                PopulateMergeCodes();
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

            protected string GetQueryCategoryName(FacetSearchCriteria criteria, string key)
            {
                foreach (var facet in criteria.Facets.Where(e => e.FacetKey == key))
                {
                    return dicCategories[facet.Members.First()];
                }

                return null;
            }

            protected string GetQuerManufacturerName(FacetSearchCriteria criteria)
            {
                foreach (var facet in criteria.Facets.Where(e => e.FacetKey == "Manufacturer"))
                {
                    return dicManufacturers[facet.Members.First()];
                }

                return null;
            }


            /// <summary>
            /// Populate a dictionary with merge codes from query.
            /// </summary>
            protected virtual void PopulateMergeCodes()
            {

                mergeCodes = new Dictionary<string, string>();

                #region Conditionals
                Func<string> qManufacturer = () =>
                {
                    return GetQuerManufacturerName(criteria);
                };

                Func<bool> qHasManufacturer = () =>
                {
                    return qManufacturer() != null;
                };


                Func<string> qColor = () =>
                {
                    return GetQueryCategoryName(criteria, "Color");
                };

                Func<bool> qHasColor = () =>
                {
                    return qColor() != null;
                };

                Func<string> qType = () =>
                {
                    return GetQueryCategoryName(criteria, "Type");
                };

                Func<bool> qHasType = () =>
                {
                    return qType() != null;
                };


                Func<string> qPattern = () =>
                {
                    return GetQueryCategoryName(criteria, "Pattern");
                };

                Func<bool> qHasPattern = () =>
                {
                    return qPattern() != null;
                };


                Func<string> qDesigner = () =>
                {
                    return GetQueryCategoryName(criteria, "Designer");
                };

                Func<bool> qHasDesigner = () =>
                {
                    return qDesigner() != null;
                };

                Func<string> qTrim = () =>
                {
                    return GetQueryCategoryName(criteria, "Trim");
                };

                Func<bool> isTrim = () =>
                {
                    return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 0;
                };

                Func<bool> isMutiTrim = () =>
                {
                    return criteria.Facets.Where(e => e.FacetKey == "Trim").Count() > 1;
                };
                #endregion

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

                var rand = new Random(randomSeed); // query json is seed

                Func<string[], string> pickOne = (choices) =>
                {
                    return choices[rand.Next(0, choices.Length)];
                };

                // for each property type, do:
                //    set the hasXXX
                //    add() if true

                hasTrim = isTrim();
                hasFabric = !isTrim();

                if (hasTrim)
                {
                    add("Trim", isMutiTrim() ? "Trim" : InitialCap(qTrim().ToLower()));
                    add("trim", isMutiTrim() ? "trim" : qTrim().ToLower());
                    add("kind", pickOne(new string[] { "trim", "decorator trim", "trimming", "decorator trimming" }));
                }

                if (hasFabric)
                {
                    if (qHasType() && qType() == "Upholstery")
                    {
                        add("kind", pickOne(new string[] { "fabric", "decorator fabric", "decorating fabric", "home fabric", "upholstery fabric" }));
                    }
                    else if (qHasType() && qType() == "Drapery")
                    {
                        add("kind", pickOne(new string[] { "fabric", "decorator fabric", "decorating fabric", "home fabric", "drapery fabric" }));
                    }
                    else if (qHasType() && qType() == "Outdoor")
                    {
                        add("kind", pickOne(new string[] { "fabric", "decorator fabric", "decorating fabric", "home fabric", "outdoor fabric" }));
                    }
                    else
                        add("kind", pickOne(new string[] { "fabric", "decorator fabric", "decorating fabric", "home fabric", "drapery and upholstery fabric", "upholstery fabric" }));
                }

                // hasName
                hasName = true;
                var nLower = name.ToLower();
                if (qHasDesigner())
                    nLower = nLower.Replace(qDesigner().ToLower(), qDesigner());
                if (qHasManufacturer())
                    nLower = nLower.Replace(qManufacturer().ToLower(), qManufacturer());

                add("name", nLower); // all lower except proper names
                add("Name", InitialCap(nLower)); // sentence case.

                var sb = new StringBuilder(128);

                if (qHasColor())
                    sb.Append(qColor());

                if (qHasPattern())
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    sb.Append(qPattern());
                }

                if (qHasType())
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    sb.Append(qType());
                }

                sb.AppendFormat(" {0}", mergeCodes["kind"]);

                if (qHasManufacturer())
                    sb.AppendFormat(" by {0}", qManufacturer());

                var altName = sb.ToString();
                var altNameLower = altName.ToLower();
                if (qHasDesigner())
                    altNameLower = altNameLower.Replace(qDesigner().ToLower(), qDesigner());
                if (qHasManufacturer())
                    altNameLower = altNameLower.Replace(qManufacturer().ToLower(), qManufacturer());

                add("altname", altNameLower); // all lower except proper names
                add("AltName", InitialCap(altNameLower)); // sentence case.

                // hasBrand
                hasBrand = qHasManufacturer();
                if (hasBrand)
                {
                    add("Brand", qManufacturer());
                    add("BrandOrDesigner", qManufacturer());
                }

                // hasDesigner
                hasDesigner = qHasDesigner();
                if (hasDesigner)
                {
                    add("Designer", qDesigner());
                    add("BrandOrDesigner", qDesigner());
                }

                // hasColor
                hasColor = qHasColor();
                if (hasColor)
                {
                    add("Color", InitialCap(qColor().ToLower()));
                    add("color", qColor().ToLower());
                }

                // hasType
                hasType = qHasType();
                if (hasType)
                {
                    add("Type", InitialCap(qType().ToLower()));
                    add("type", qType().ToLower());
                }

                // hasPattern
                hasPattern = qHasPattern();
                if (hasPattern)
                {
                    add("Pattern", InitialCap(qPattern().ToLower()));
                    add("pattern", qPattern().ToLower());
                }


                var adjs = new string[] { "beautiful", "captivating", "inspiring", "elegant", "spectacular", "striking", "fabulous", "exceptional", "remarkable", "fantastic", "splendid", 
                "marvelous", "extraordinary", "amazing", "dazzling", "gorgeous", "refreshing", "lovely", "charming", "delightful", "magnificent", "memorable", "exquisite", 
                "fashionable", "popular",  "impressive", "attractive", "stunning", "superb", "engaging", "enchanting", "stylish", "inviting", "impeccable", "tasteful" };

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
        #endregion

        #region Query Permutations
        /// <summary>
        /// Entry point to spin up queue of permutations in advance of spinning full records.
        /// </summary>
        public override void SpinQueryPermutations()
        {
            var completeList = new List<FacetSearchCriteria>();

            // fabric
            completeList.AddRange(GetGenericFabricQueries().Where(e => e.Facets.Count() >= 2));
            completeList.AddRange(GetManufacturerFabricQueries());
            completeList.AddRange(GetDesignerFabricQueries());

            // trim
            completeList.AddRange(GetGenericTrimQueries().Where(e => e.Facets.Count() >= 2));
            completeList.AddRange(GetManufacturerTrimQueries());
            completeList.AddRange(GetDesignerTrimQueries());

            // Counts found:
            // GetGenericFabricQueries: 4,147.00
            // GetManufacturerFabricQueries: 174,174.00
            // GetDesignerFabricQueries: 149,292.00
            // FABRIC: 330K

            // GetGenericTrimQueries: 78.00
            // GetManufacturerTrimQueries: 3,276.00
            // GetDesignerTrimQueries: 2,808.00
            // TRIM: 6K


            // we now have a list of everything we want in the queue,
            // save to SQL

            Debug.WriteLine(string.Format("Query count: {0:N0}", completeList.Count()));

            completeList.Shuffle();
            completeList.Shuffle();
            //completeList = completeList.Take(10000).ToList();

            int remaining = completeList.Count();
            int skip = 0;
            int take = 200;
            while (remaining > 0)
            {
                AddQueryPermutationToQueue(completeList.Skip(skip).Take(take));
                skip += take;
                remaining -= take;
            }
        }

        protected override List<int> GetManufacturerList()
        {
            var list = base.GetManufacturerList();
            var onlyWallpaper = new List<int>() { 76, 107, 92, 117, 71, 90, 74 };
            list.RemoveAll(e => onlyWallpaper.Contains(e));
            return list;
        }

        private List<FacetSearchCriteria> GetManufacturerFabricQueries()
        {
            var list = new List<FacetSearchCriteria>();

            foreach (var manufacturerID in GetManufacturerList())
            {
                var genericList = GetGenericFabricQueries();
                foreach (var item in genericList)
                    item.Facets.Add(new FacetItem() { FacetKey = "Manufacturer", Members = new List<int>() { manufacturerID } });

                list.AddRange(genericList);
            }
            return list;
        }

        private List<FacetSearchCriteria> GetManufacturerTrimQueries()
        {
            var list = new List<FacetSearchCriteria>();

            foreach (var manufacturerID in GetManufacturerList())
            {
                var genericList = GetGenericTrimQueries();
                foreach (var item in genericList)
                    item.Facets.Add(new FacetItem() { FacetKey = "Manufacturer", Members = new List<int>() { manufacturerID } });

                list.AddRange(genericList);
            }
            return list;
        }

        private List<FacetSearchCriteria> GetDesignerFabricQueries()
        {
            var list = new List<FacetSearchCriteria>();

            foreach (var id in GetDesigners())
            {
                var genericList = GetGenericFabricQueries();
                foreach (var item in genericList)
                    item.Facets.Add(new FacetItem() { FacetKey = "Designer", Members = new List<int>() { id } });

                list.AddRange(genericList);
            }
            return list;
        }

        private List<FacetSearchCriteria> GetDesignerTrimQueries()
        {
            var list = new List<FacetSearchCriteria>();

            foreach (var id in GetDesigners())
            {
                var genericList = GetGenericTrimQueries();
                foreach (var item in genericList)
                    item.Facets.Add(new FacetItem() { FacetKey = "Designer", Members = new List<int>() { id } });

                list.AddRange(genericList);
            }
            return list;
        }

        private List<FacetSearchCriteria> GetGenericFabricQueries()
        {
            var list = new List<FacetSearchCriteria>();

            // color
            foreach (var colorID in GetColors())
            {
                var item = new FacetSearchCriteria();
                item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                list.Add(item);
            }

            // pattern
            foreach (var patternID in GetPatterns())
            {
                var item = new FacetSearchCriteria();
                item.Facets.Add(new FacetItem() { FacetKey = "Pattern", Members = new List<int>() { patternID } });
                list.Add(item);
            }


            // type
            foreach (var typeID in GetTypes())
            {
                var item = new FacetSearchCriteria();
                item.Facets.Add(new FacetItem() { FacetKey = "Type", Members = new List<int>() { typeID } });
                list.Add(item);
            }


            // color, pattern
            foreach (var colorID in GetColors())
            {
                foreach (var patternID in GetPatterns())
                {
                    var item = new FacetSearchCriteria();
                    item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                    item.Facets.Add(new FacetItem() { FacetKey = "Pattern", Members = new List<int>() { patternID } });
                    list.Add(item);
                }
            }


            // color, type
            foreach (var colorID in GetColors())
            {
                foreach (var typeID in GetTypes())
                {
                    var item = new FacetSearchCriteria();
                    item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                    item.Facets.Add(new FacetItem() { FacetKey = "Type", Members = new List<int>() { typeID } });
                    list.Add(item);
                }
            }

            // pattern, type
            foreach (var patternID in GetPatterns())
            {
                foreach (var typeID in GetTypes())
                {
                    var item = new FacetSearchCriteria();
                    item.Facets.Add(new FacetItem() { FacetKey = "Pattern", Members = new List<int>() { patternID } });
                    item.Facets.Add(new FacetItem() { FacetKey = "Type", Members = new List<int>() { typeID } });
                    list.Add(item);
                }
            }


            // color, pattern, type
            foreach (var colorID in GetColors())
            {
                foreach (var patternID in GetPatterns())
                {
                    foreach (var typeID in GetTypes())
                    {
                        var item = new FacetSearchCriteria();
                        item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                        item.Facets.Add(new FacetItem() { FacetKey = "Pattern", Members = new List<int>() { patternID } });
                        item.Facets.Add(new FacetItem() { FacetKey = "Type", Members = new List<int>() { typeID } });
                        list.Add(item);

                    }
                }
            }

            return list;
        }

        private List<FacetSearchCriteria> GetGenericTrimQueries()
        {
            var list = new List<FacetSearchCriteria>();

            // any trim
            var trim = new FacetSearchCriteria();
            trim.Facets.Add(new FacetItem() { FacetKey = "Trim", Members = GetTrims() });
            list.Add(trim);

            // any trim kind, each color
            foreach (var colorID in GetColors())
            {
                var item = new FacetSearchCriteria();
                item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                item.Facets.Add(new FacetItem() { FacetKey = "Trim", Members = GetTrims() });
                list.Add(item);
            }

            // each kind of trim
            foreach (var trimID in GetTrims())
            {
                var item = new FacetSearchCriteria();
                item.Facets.Add(new FacetItem() { FacetKey = "Trim", Members = new List<int>() { trimID } });
                list.Add(item);
            }

            // color, trim kind
            foreach (var colorID in GetColors())
            {
                foreach (var trimID in GetTrims())
                {
                    var item = new FacetSearchCriteria();
                    item.Facets.Add(new FacetItem() { FacetKey = "Color", Members = new List<int>() { colorID } });
                    item.Facets.Add(new FacetItem() { FacetKey = "Trim", Members = new List<int>() { trimID } });
                    list.Add(item);
                }
            }

            return list;
        }


        private List<int> GetColors()
        {
            return GetCategoryList(37);
        }

        private List<int> GetPatterns()
        {
            return GetCategoryList(118);
        }

        private List<int> GetTypes()
        {
            return GetCategoryList(3);
        }

        private List<int> GetDesigners()
        {
            return GetCategoryList(162);
        }

        private List<int> GetTrims()
        {
            return GetCategoryList(105);
        }

        
        #endregion
    }
}
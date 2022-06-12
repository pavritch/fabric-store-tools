using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using Website.Entities;
using System.IO;
using System.Configuration;
using Gen4.Util.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;
using InsideFabric.Data;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Website
{
    /// <summary>
    /// Helper class to provide common answers for feed fields.
    /// </summary>
    /// <remarks>
    /// This class does most of the heavy lifting for any of the common kinds
    /// of information required by most feeds.
    /// </remarks>
    public class InsideRugsProduct : InsideStoresProductBase, IUpdatableProduct
    {

        #region ProductProperties
        private class ProductProperties
        {
            private InsideRugsProduct p;
            private RugProductFeatures productFeatures;
            private IEnumerable<RugProductVariantFeatures> productVariantsFeatures;

            public ProductProperties(InsideRugsProduct p, RugProductFeatures productFeatures, IEnumerable<RugProductVariantFeatures> productVariantsFeatures)
            {
                this.p = p;
                this.productFeatures = productFeatures;
                this.productVariantsFeatures = productVariantsFeatures;
            }

            /// <summary>
            /// Returns list of property values which should be added to Ext2, which is then used for
            /// autosuggest and full text search.
            /// </summary>
            /// <remarks>
            /// Do not worry about duplicates - the caller will sort all that out and make a distinct list
            /// combined with any other inputs it happens to use.
            /// </remarks>
            public List<string> SearchableProperties
            {
                get
                {
                    var set = new HashSet<string>();

                    Action<string> add = (s) =>
                    {
                        if (string.IsNullOrWhiteSpace(s))
                            return;

                        var s2 = s.ToLower().Trim();
                        if (!string.IsNullOrWhiteSpace(s2))
                            set.Add(s2);
                    };


                    // parent product

                    if (productFeatures.Colors != null)
                    {
                        foreach (var color in productFeatures.Colors)
                            add(color);
                    }
                    
                    add(productFeatures.ColorGroup);
                    add(productFeatures.Weave);
                    add(productFeatures.CountryOfOrigin);
                    add(productFeatures.Designer);
                    add(productFeatures.PatternName);
                    add(productFeatures.PatternNumber);
                    add(productFeatures.Collection);

                    if (productFeatures.Tags != null)
                    {
                        foreach(var t in productFeatures.Tags)
                            add(t);
                    }

                    if (productFeatures.Material != null)
                    {
                        foreach (var item in productFeatures.Material)
                            add(item.Key);
                    }
                    
                    // variants

                    foreach (var vf in productVariantsFeatures)
                    {
                        add(vf.Shape);
                        add(vf.UPC);
                    }

                    return set.ToList();
                }
            }

            #region Method Handlers
            

            #endregion

        }

        #endregion

        private static List<int> _protectedRugCategories;


        #region ProductKind Enum
        /// <summary>
        /// What kind of product this is.
        /// </summary>
        /// <remarks>
        /// The description relates to Google. Other feeds may require something different.
        /// The description attribute must be one of the official Google taxonomy.
        /// See http://support.google.com/merchants/bin/answer.py?hl=en&answer=160081&topic=2473824&ctx=topic
        /// </remarks>
        public enum ProductKind
        {
            [Description("Home & Garden > Decor > Rugs")]
            Rug,
        }
        #endregion


        #region Ctors

        public InsideRugsProduct()
        {
            IsValid = false;
        }

        public InsideRugsProduct(IWebStore store, Product p, List<ProductVariant> variants, Manufacturer m, List<Category> categories, AspStoreDataContext dc)
            : this()
        {
            Initialize(store, p, variants, m, categories, dc);
        }


        #endregion

        #region Properties

        protected override string GoogleProductCategory
        {
            get
            {
                return KindOfProduct.Description();
            }
        }

        public ProductKind KindOfProduct
        {
            get
            {
                return ProductKind.Rug;
            }
        }


        public RugProductFeatures Features
        {
            get
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.RugProductFeatures, out obj))
                {
                    var f = obj as RugProductFeatures;
                    return f;
                }
                throw new Exception(string.Format("Missing RugProductFeatures for ProductID: {0}", p.ProductID));
            }
        }

        private List<RugProductVariantFeatures> _variantFeatures;

        public List<RugProductVariantFeatures> VariantFeatures
        {
            get
            {
                if (_variantFeatures != null)
                    return _variantFeatures;

                if (ExtData4.ContainsKey(ExtensionData4.RugProductFeatures))
                {
                    _variantFeatures = new List<RugProductVariantFeatures>();

                    Func<ProductVariant, Dictionary<string, object>> makeExtData4 = (pvar) =>
                    {
                        var extData = ExtensionData4.Deserialize(pvar.ExtensionData4);
                        return extData.Data;
                    };

                    foreach (var v in variants)
                    {
                        var vExtData4 = makeExtData4(v);
                        if (vExtData4.ContainsKey(ExtensionData4.RugProductVariantFeatures))
                        {
                            object obj;
                            if (vExtData4.TryGetValue(ExtensionData4.RugProductVariantFeatures, out obj))
                            {
                                var f = obj as RugProductVariantFeatures;
                                _variantFeatures.Add(f);
                            }
                        }
                    }


                }
                else
                {
                    _variantFeatures = new List<RugProductVariantFeatures>();
                }
                
                return _variantFeatures;
            }
        }


        public string ManufacturerPartNumber
        {
            get
            {
                // 15K Kravet/Pinder products have null MPN since they have been updated
                // to exactly sync with CSV files

                if (pv.ManufacturerPartNumber == null)
                    return string.Empty;

                return pv.ManufacturerPartNumber.ToUpper();
            }
        }

        public string SKU
        {
            get
            {
                return p.SKU.ToUpper();
            }
        }

        public string Brand
        {
            get
            {
                return m.Name.Replace(" Rugs", "");
            }
        }


        #endregion

        #region Public Methods

        public AlgoliaProductRecord MakeAlgoliaProductRecord()
        {
            try
            {
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && !Store.SupportedProductGroups.Contains(group.Value))
                    return null;

                var rec = new AlgoliaProductRecord(p.ProductID)
                {
                    sku = p.SKU,
                    name = p.Name,
                    brand = Brand,
                    mpn = string.Format("{0} {1}", Brand, pv.ManufacturerPartNumber),
                    isLive = p.ShowBuyButton == 1,
                    rank = (p.ShowBuyButton == 1) ? 10 : 2
                };

                foreach (var cat in categories)
                {
                    if (m.Name.StartsWith("$"))
                        continue;
                    
                    rec.AddCategory(cat.Name.ToLower());
                }

                // all the various features and properties

                var props = new HashSet<string>();

                Action<string> add = (value) =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return;

                    if (value.Length < 5 && value.IsNumeric())
                        return;

                    if (value.ContainsIgnoreCase(" inches"))
                        return;

                    if (value.ContainsIgnoreCase(" feet"))
                        return;

                    if (value.Contains(","))
                    {
                        foreach (var splitValue in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else if (value.Contains(";"))
                    {
                        foreach (var splitValue in value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else if (value.Contains("/"))
                    {
                        foreach (var splitValue in value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else
                        props.Add(value.ToLower());
                };

                add(Features.Color);
                if (Features.Colors != null)
                {
                    foreach (var c in Features.Colors)
                        add(c);
                }
                add(Features.ColorGroup);
                add(Features.Weave);
                add(Features.Collection);
                
                if (Features.Tags != null)
                {
                    foreach (var t in Features.Tags)
                        add(t);
                }
                add(Features.Backing);
                add(Features.Designer);
                add(Features.PatternName);
                add(Features.PatternNumber);
                add(Features.Collection);
                if (Features.Material != null)
                {
                    foreach(var mat in Features.Material.Keys)
                    {
                        add(mat);
                    }
                }

                var dicShapes = (from v in VariantFeatures
                                 group v by v.Shape into grp
                                 select new { Shape = grp.Key, cnt = grp.Count() }).ToDictionary(k => k.Shape, v => v.cnt);

                foreach (var shape in dicShapes.Keys)
                    add(shape);

                foreach (var prop in props)
                    rec.AddProperty(prop);

                return rec;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }


        // for rugs, a very limited number of values are kept
        private static string[] allowedProductLabels = new string[]
        {
            "Pattern", "Pattern Name", "Pattern Number", "Collection"
        };

        public void RebuildTaxonomy()
        {
            // for rugs, a very limited number of values are kept

            // assumes table has been truncated
            foreach (var item in OriginalRawProperties.Where(e => allowedProductLabels.Contains(e.Key)))
            {
                dc.InsertProductLabel(p.ProductID, item.Key, item.Value);
            }
        }

        public void RefreshTaxonomy()
        {
            // delete all for this product first, then add back
            dc.DeleteProductLabels(p.ProductID);
            RebuildTaxonomy();
        }

        public void AddMissingProductToProductLabelsTable()
        {
            // note that for rugs, it is okay (and frequent) that a product not have entries
            // in the table since only a few keys are supported

            if (OriginalRawProperties.Count() == 0)
                return;

            // add for products which are not yet included in the table.
            if (dc.ProductLabels.Where(e => e.ProductID == p.ProductID).Count() == 0)
                RebuildTaxonomy();
        }



        /// <summary>
        /// Main entry point for per-product which figures out the phrase list to pump out.  Filters on supported product groups.
        /// </summary>
        /// <remarks>
        /// Repopulate Ext2 with a string which has one word or word phrase on each line which is to be 
        /// associated with this product. These phrases will participate in full text search and contribute
        /// to the unique phrase list which powers the auto suggest feature. No further post processing is done
        /// by other modules. This data should be as clean as can be made.
        /// </remarks>
        /// <param name="CategoryKeywordMgr"></param>
        public void MakeAndSaveExt2KeywordList()
        {
            try
            {
                var set = new HashSet<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var s2 = s.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(s2))
                        set.Add(s2);
                };

                // only fill in data when this product belongs to one of the supported groups for this store.
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && Store.SupportedProductGroups.Contains(group.Value))
                {
                    // need to add data for parent product and the variants

                    // parent
                    add(ProductGroup);
                    add(Brand);

                    // variants

                    foreach (var v in variants)
                    {
                        add(ManufacturerPartNumber);
                        if (!string.IsNullOrWhiteSpace(v.SKUSuffix))
                            add(SKU + v.SKUSuffix);
                    }

                    if (p.ShowBuyButton == 0)
                        add("discontinued");

                    // do a reverse lookup on the categories for this product, and add their respective keywords

                    // this is a comma sep list, could have spaces
                    var keywords = categories.Where(e => Store.CategoryFilterManager.AllCategoryFilters.Contains(e.CategoryID) && !string.IsNullOrWhiteSpace(e.Summary)).Select(e => e.Summary).ToList();
                    foreach(var item in keywords)
                    {
                        foreach (var phrase in item.ParseCommaDelimitedList())
                            add(phrase);
                    }

                    // this data is required, but making sure during early dev where we might not
                    // have everything in place

                    if (ExtData4.ContainsKey(ExtensionData4.RugProductFeatures))                    
                    {
                        var props = new ProductProperties(this, Features, VariantFeatures);
                        props.SearchableProperties.ForEach(e => add(e));
                    }
                }

                var algolia = MakeAlgoliaProductRecord();
                add(algolia.brand);
                add(algolia.mpn);
                algolia.categories.ForEach(e => add(e));
                algolia.properties.ForEach(e => add(e));

                var ext2 = set.ToList().ConvertToLines();

                dc.Products.UpdateExtensionData2(p.ProductID, ext2);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        /// <summary>
        /// The Ext3 data is used as a temporary bridge or crutch during the transition to tune FTS. Filters on supported product groups.
        /// </summary>
        /// <remarks>
        /// Write out one line per phrase which will contribute to FTS - but not autocomplete.
        /// </remarks>
        /// <param name="CategoryKeywordMgr"></param>
        public void MakeAndSaveExt3KeywordList()
        {
            try
            {
                var set = new HashSet<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var s2 = s.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(s2))
                        set.Add(s2);
                };

                // only need to add data that is  not already inserted into Ext2 (autosuggest) since both Ext2 and Ext3 are indexed
                // for full text search

                // only fill in data when this product belongs to one of the supported groups for this store.
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && Store.SupportedProductGroups.Contains(group.Value))
                {
                    add(p.Name);
                }

                var ext3 = set.ToList().ConvertToLines();

                dc.Products.UpdateExtensionData3(p.ProductID, ext3);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        private IEnumerable<string> parseNameTokens(string input)
        {
            var set = new HashSet<string>();

            if (string.IsNullOrWhiteSpace(input))
                return set;

            var ary = input.Replace(",", " ").Replace("-", " ").Replace("  ", " ").Split(' ');

            if (ary.Length == 0)
                return set;

            foreach (var s in ary)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                var trimmedToken = s.TrimToNull();
                if (string.IsNullOrWhiteSpace(trimmedToken) || trimmedToken.Length < 3)
                    continue;

                set.Add(trimmedToken);
            }

            // also pick up first two-word combination

            if (ary.Length >=2)
            {
                if (!string.IsNullOrWhiteSpace(ary[0]) && !string.IsNullOrWhiteSpace(ary[1]))
                    set.Add(string.Format("{0} {1}", ary[0], ary[1]));
            }

            return set;
        }



        private NameValueCollection ExtractProductDescriptionValuesFromExtension4()
        {
            var col = new NameValueCollection();

            try
            {

                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.OriginalRawProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;

                    foreach (var item in dic)
                        col.Add(item.Key, item.Value);
                }
            }
            catch
            {
            }

            return col;
        }

        protected override bool SpinDescription()
        {
            try
            {
                if (p.ProductGroup == null || p.ProductGroup != "Rug")
                    return false;

                var maker = new LongDescriptionMakerRugs(this);
                p.Description = maker.Description;
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }


        protected override bool SpinFroogleDescription()
        {
            try
            {
                if (p.ProductGroup == null || p.ProductGroup != "Rug")
                    return false;

                var maker = new FroogleDescriptionMakerRugs(this);
                p.FroogleDescription = maker.Description;
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }


        protected override bool SpinSEDescription()
        {
            if (p.ProductGroup == null || p.ProductGroup != "Rug")
                return false;

            var maker = new SEDescriptionMakerRugs(this);
            p.SEDescription = maker.Description;

            return true;
        }

        protected override bool SpinSEKeywords()
        {
            if (p.ProductGroup == null || p.ProductGroup != "Rug")
                return false;

            var list = new List<string>();

            list.Add("rug");
            list.Add("area rug");
            list.Add(m.Name.ToLower());

            // style, color, material, weave, shape
            var parentCategories = new List<int>() { 359, 295, 311, 431, 347 };

            foreach (var cat in categories.Where(e => parentCategories.Contains(e.ParentCategoryID)))
                list.Add(cat.Name.ToLower());

            list.Add(p.SKU);

            p.SEKeywords = list.ToCommaDelimitedList();

            return true;
        }


        private Dictionary<string, string> MakeDictionaryFromXml(string extensionXml)
        {
            var xml = XElement.Parse(extensionXml.Replace(" & ", " &amp; "));
            var dic = new Dictionary<string, string>();

            foreach (var node in xml.Descendants())
            {
                string name = null;
                string value = null;

                name = node.Name.ToString();
                value = node.Value;

                if (name != null && value != null && !dic.ContainsKey(name))
                    dic[name] = value;
            }

            return dic;
        }




        private List<int> ProtectedProductCategories
        {
            get
            {
                lock(lockObj)
                {
                    if (_protectedRugCategories != null)
                        return _protectedRugCategories;

                    _protectedRugCategories = new List<int>();

                    // clearance
                    _protectedRugCategories.Add(Store.OutletCategoryID);

                    // curated collections
                    _protectedRugCategories.AddRange(dc.Categories.Where(e => e.ParentCategoryID == 4).Select(e => e.CategoryID));

                    return _protectedRugCategories;
                }
            }
        }
        /// <summary>
        /// Forced rebuild of category associations for this product.
        /// </summary>
        [RunProductAction("RebuildProductCategories")]
        public void RebuildProductCategories()
        {
            try
            {
                UpdateProductCategories(forceUpdate: true);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }

        /// <summary>
        /// For products recently inserted/updated, rebuilds their category associations.
        /// </summary>
        /// <remarks>
        /// Uses LastFullUpdate in Ext4 private dic to determine if work needs to be done.
        /// </remarks>
        [RunProductAction("UpdateProductCategories")]
        public void UpdateProductCategories()
        {
            UpdateProductCategories(forceUpdate: false);
        }

        /// <summary>
        /// Update category filters. Uses date flags, unless force is true.
        /// </summary>
        /// <remarks>
        /// Must be a live product to be in filter categories.
        /// </remarks>
        /// <param name="forceUpdate"></param>
        public void UpdateProductCategories(bool forceUpdate)
        {
            #region Functions

            Func<bool> isClassifyRequired = () =>
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.PrivateProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;
                    if (dic == null)
                        return false;

                    string isRequired;
                    if (dic.TryGetValue(ExtensionData4.PrivatePropertiesKeys.RequiresClassifyUpdate, out isRequired))
                        return bool.Parse(isRequired);

                    return false;
                }

                return false;
            };



            Func<string, DateTime?> getDate = (key) =>
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.PrivateProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;
                    if (!dic.ContainsKey(key))
                        return null;

                    DateTime dt;
                    if (DateTime.TryParse(dic[key], out dt))
                        return dt;

                    return null;
                }
                return null;
            };

            Action<string, DateTime> setDate = (key, dt) =>
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.PrivateProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;
                    if (dic == null)
                        return;

                    dic[key] = dt.ToShortDateString();

                    if (key == ExtensionData4.PrivatePropertiesKeys.LastClassifyUpdate)
                        dic.Remove(ExtensionData4.PrivatePropertiesKeys.RequiresClassifyUpdate);
                }
            };

            Action markDate = () =>
                {
                    setDate(ExtensionData4.PrivatePropertiesKeys.LastClassifyUpdate, DateTime.Now);
                    MarkExtData4Dirty();
                    SaveExtData4();
                };

            #endregion

            try
            {
                if (p.ShowBuyButton == 1 && !forceUpdate)
                {
                    // both persisted as DateTime.Now.ToShortDateString()
                    var lastFullUpdate = getDate(ExtensionData4.PrivatePropertiesKeys.LastFullUpdate);
                    var lastClassifyUpdate = getDate(ExtensionData4.PrivatePropertiesKeys.LastClassifyUpdate);

                    // we now use explicit trigger flag
                    if (lastClassifyUpdate.HasValue && lastFullUpdate.HasValue && !isClassifyRequired())
                        return;
                }



                // if gets to here, we have some work to do on this product. The product could be totally new (no categories at all),
                // recently updated (so categories exist, but may be stale), or REBUILD mode, whereby the caller just wiped out all the
                // non-protected properties and we have a clean slate (pretty much as is just inserted).

                // the goal is to shape the logic so that we have smart classes for each top level category (root). And we'll call upon
                // these classes to have them organize things as they need to be.

                // in theory, there should be zero cross-over between what these classes do -- they only deal with their own subtree, so
                // could invoke actions in parallel if wanted; but products are already being run in parallel, so might add too much complexity (but should work)

                bool hasBeenUpdated = false;

                var currentFilterCategories = categories.Where(e => Store.CategoryFilterManager.AllCategoryFilters.Contains(e.CategoryID)).Select(e => e.CategoryID).ToList();

                if (currentFilterCategories.Count() > 0)
                {
                    // remove all associations under high filter root - gives classification a clean slate.
                    dc.ProductCategories.RemoveProductCategoryAssociationsForProduct(p.ProductID, currentFilterCategories);
                    hasBeenUpdated = true;
                }

                // must be a live product to be in the filters

                // we also require that a photo exists - and that it's been processed for digital attributes
                // without a photo, we cannot make many of the determinations
                // bottom line is if no photo, we really don't care much about this product - does not need to be found, and should not clutter things up.

                if (ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures) && !string.IsNullOrWhiteSpace(p.ImageFilenameOverride) &&p.ShowBuyButton == 1)
                    Store.CategoryFilterManager.Classify(this);

                // hack to update RugShapes - regardless of if was classified here along the way
                {
                    var dicShapes = (from v in VariantFeatures
                                  group v by v.Shape into grp
                                  select new { Shape = grp.Key, cnt = grp.Count() }).ToDictionary(k => k.Shape, v => v.cnt);

                    ExtData4[ExtensionData4.RugShapes] = dicShapes;
                    hasBeenUpdated = true;
                    SaveProductShapeFeatures(dicShapes);
                }

                if (hasBeenUpdated)
                    markDate();
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        private void CreateImageFeatures()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                {
                    var processor = new ProductImageProcessor(this.Store);

                    var features = processor.MakeImageFeatures(p.ImageFilenameOverride);

                    //features will be null if the image file does not exist, or if something goes wrong

                    if (features != null)
                    {
                        ExtData4[ExtensionData4.ProductImageFeatures] = features;
                        // update ProductFeatures table (just 3 of the properties, upsert op)
                        ProductImageProcessor.SaveProductImageFeatures(dc, p.ProductID, features);
                    }
                    else
                    {
                        ExtData4.Remove(ExtensionData4.ProductImageFeatures);
                        dc.ProductFeatures.RemoveProductFeatures(p.ProductID);
                    }

                    MarkExtData4Dirty();
                    SaveExtData4();
                }
                else
                {
                    if (ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures))
                    {
                        ExtData4.Remove(ExtensionData4.ProductImageFeatures);
                        MarkExtData4Dirty();
                        SaveExtData4();
                        dc.ProductFeatures.RemoveProductFeatures(p.ProductID);
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                throw;
            }
        }

        [RunProductAction("FillMissingImageFeatures")]
        public void FillMissingImageFeatures()
        {
            try
            {

                // nothing to do if already has this record
                if (!ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures) || !ExtData4.ContainsKey(ExtensionData4.RugProductFeatures))
                    return;

                // nothing to do if no image available
                if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                    return;

                var imgFeatures = ExtData4[ExtensionData4.ProductImageFeatures] as ImageFeatures;
                var bestColor = imgFeatures.BestColor;

                Action<List<string>> writeLines = (lst) =>
                    {
                        lock (lockObj)
                        {
                            using (StreamWriter sw = File.AppendText(@"c:\temp\colordata.txt"))
                            {
                                foreach (var line in lst)
                                    sw.WriteLine(line);
                            }
                        }
                    };

                var lines = new List<string>();
                var colorCategory = categories.Where(e => e.ParentCategoryID == 295 && e.CategoryID != 302).FirstOrDefault();

                if (Features.Colors != null)
                {
                    foreach (var colorPhrase in Features.Colors)
                    {
                        int colorCatID = 0;
                        string colorCatName = "null";

                        if (colorCategory != null)
                        {
                            colorCatID = colorCategory.CategoryID;
                            colorCatName = colorCategory.Name;
                        }
                        var line = string.Format("{0},{1},{2},{3},{4}", p.ProductID, bestColor, colorCatID, colorCatName, colorPhrase);
                        lines.Add(line);
                    }
                }

                if (lines.Count() > 0)
                    writeLines(lines);

            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
            //CreateImageFeatures();
        }






        /// <summary>
        /// Fully recompute the CEDD image features for this product.
        /// </summary>
        /// <remarks>
        /// Persist results to Ext4 and ProductFeatures table for Colors, ImageDescriptor, TinyImageDescriptor.
        /// References to Similar by xxx in ProductFeatures needs to be done in a separate pass after all
        /// feature data is available.
        /// </remarks>
        [RunProductAction("ReCreateImageFeatures")]
        public void ReCreateImageFeatures()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            CreateImageFeatures();

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }

        /// <summary>
        /// Fills in SimilarXXX columns in ProductFeatures table.
        /// </summary>
        /// <remarks>
        /// Requires that descriptors have already been created via ReCreateImageFeatures() or equiv.
        /// </remarks>
        [RunProductAction("ReCreateSimilarByProductFeatures")]
        public void ReCreateSimilarByProductFeatures()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }

        #endregion

        #region Local Methods

        /// <summary>
        /// Persist some of this data to a fast table for quick bulk loading by data service.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="productID"></param>
        /// <param name="features"></param>
        private void SaveProductShapeFeatures(Dictionary<string, int> dicShapes)
        {
            try
            {
                var sb = new StringBuilder();
                bool isFirst = true;

                foreach (var shape in dicShapes)
                {
                    if (shape.Value == 0)
                        continue;

                    if (!isFirst)
                        sb.Append(";");

                    sb.AppendFormat("{0}={1}", shape.Key, shape.Value);
                    isFirst = false;
                }

                var shapes = sb.ToString();
 
                var productFeatures = dc.ProductFeatures.Where(e => e.ProductID == p.ProductID).FirstOrDefault();
                if (productFeatures == null)
                {
                    // insert
                    productFeatures = new ProductFeature()
                    {
                        ProductID = p.ProductID,
                        Shapes = shapes
                    };
                    dc.ProductFeatures.InsertOnSubmit(productFeatures);
                    dc.SubmitChanges();
                }
                else
                {
                    if (productFeatures.Shapes == shapes)
                        return;

                    // update
                    productFeatures.Shapes = shapes;
                    dc.SubmitChanges();
                }
            }
            catch
            { }
        }


        #endregion

#if false


        /// <summary>
        /// Copy from images original folder to cache folder for any that don't exist in cache.
        /// </summary>
        [RunProductAction("UpdateImageDownloadCache")]
        public void UpdateImageDownloadCache()
        {
            try
            {
#if false
                var productImages = ExtData4[ExtensionData4.ProductImages] as List<ProductImage>;
                var listFiles = (from productImage in productImages
                                 where productImage != null && !string.IsNullOrWhiteSpace(productImage.SourceUrl)
                                 select new
                                 {
                                     CacheFilepath = ProductImageProcessor.MakeCacheFilename(productImage.SourceUrl),
                                     Url = productImage.SourceUrl,
                                     Filename = productImage.Filename,
                                     SKU = p.SKU,
                                     Shape = productImage.ImageVariant
                                 }).ToList();

                lock (lockObj)
                {
                    using (StreamWriter sw = File.AppendText(@"D:\InsideRugs-Dev\images\product\files.txt"))
                    {
                        foreach (var file in listFiles)
                        {
                            var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", p.ProductID, file.SKU, file.Filename, file.Shape, file.Url, file.CacheFilepath);
                            sw.WriteLine(line);
                        }
                    }
                }
#endif
#if false
                if (!ExtData4.ContainsKey(ExtensionData4.AvailableImageFilenames) || !ExtData4.ContainsKey(ExtensionData4.ProductImages))
                    return;

                var availImages = ExtData4[ExtensionData4.AvailableImageFilenames] as List<string>;
                var productImages = ExtData4[ExtensionData4.ProductImages] as List<ProductImage>;

                Func<string, string> makeCacheFilepath = (url) =>
                    {
                        return Path.Combine(Store.PathWebsiteRoot, "images\\product", "Cache", ProductImageProcessor.MakeCacheFilename(url));
                    };

                Func<string, string> makeOriginalFilepath = (fileName) =>
                    {
                        return Path.Combine(Store.PathWebsiteRoot, "images\\product", "Original", fileName);
                    };

                // for each image we know about, associate up with it's source URL, compute a corresponding deterministic cache name (via SHA 256)


                var listFiles = (from filename in availImages
                                 let productImage = productImages.Where(e => e.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
                                 where productImage != null && !string.IsNullOrWhiteSpace(productImage.SourceUrl)
                                 select new
                                 {
                                     CacheFilepath = makeCacheFilepath(productImage.SourceUrl),
                                     OriginalFilepath = makeOriginalFilepath(filename)
                                 }).ToList();

                // we have a list of potential work to do - copy the file from original to cache for any which does exist
                // in original but does not exist in cache. Note that we do not overwrite cache.

                foreach(var file in listFiles)
                {
                    if (File.Exists(file.OriginalFilepath) && !File.Exists(file.CacheFilepath))
                    {
                        var binaryImage = file.OriginalFilepath.ReadBinaryFile();
                        binaryImage.WriteBinaryFile(file.CacheFilepath);
                    }
                }
#endif
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }



        /// <summary>
        /// Write out a list of descriptions to temp folder for development only.
        /// </summary>
        [RunProductAction("ListRugProductDescriptions")]
        public void ListRugProductDescriptions()
        {

#if DEBUG
            try
            {
                if (!ExtData4.ContainsKey(ExtensionData4.RugProductFeatures))
                    return;

                if (Features.Description == null)
                    return;

                // could have more than one sentence

                var sb = new StringBuilder(1000);

                sb.AppendFormat("{0} {1} {2}: ", p.ProductID, p.SKU, Features.Description.Count());
                sb.Append(Features.Description.First());

                lock (lockObj)
                {
                    using (StreamWriter sw = File.AppendText(@"c:\temp\rug-descriptions.txt"))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
#else
            throw new NotImplementedException("ListRugProductDescriptions not valid for release build.");
#endif
        }


        /// <summary>
        /// Recreate XML data in Extension1 based on JSON data.
        /// </summary>
        /// <remarks>
        /// Useful when need to reorder the items due to changes in list priority.
        /// </remarks>
        [RunProductAction("RefreshXmlExtensionData")]
        public void RefreshXmlExtensionData()
        {
            try
            {
                var xml = InsideFabric.Data.ProductProperties.MakePropertiesXml(OriginalRawProperties).ToString();
                dc.Products.UpdateExtensionData1(p.ProductID, xml);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }
#endif

    }


}
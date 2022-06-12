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
    public class InsideAvenueProduct : InsideStoresProductBase, IUpdatableProduct
    {
        const int CLASS_ROOT_CATEGORYID = 1;
        const int UNCLASSIFIED_CATEGORYID = 117;
        const int REVIEW_CATEGORYID = 121;

        private bool primaryCategoryIDIsValid;
        private int? primaryCategoryID;

        #region Ctors

        public InsideAvenueProduct()
        {
            IsValid = false;
        }

        public InsideAvenueProduct(IWebStore store, Product p, List<ProductVariant> variants, Manufacturer m, List<Category> categories, AspStoreDataContext dc)
            : this()
        {
            Initialize(store, p, variants, m, categories, dc);
        }

        #endregion

        #region Properties


        protected int? PrimaryCategory
        {
            get
            {
                if (primaryCategoryIDIsValid)
                    return primaryCategoryID;

                var iaStore = Store as InsideAvenueWebStore;
 
                foreach (var categoryID in categories.Select(e => e.CategoryID))
                {
                    if (categoryID == CLASS_ROOT_CATEGORYID)
                        continue;

                    if (iaStore.PrimarySqlCategories.Contains(categoryID))
                    {
                        primaryCategoryID = categoryID;
                        break;
                    }
                }

                primaryCategoryIDIsValid = true;
                return primaryCategoryID;
            }
        }

        public string PrimaryCategoryName
        {
            get
            {
                var iaStore = Store as InsideAvenueWebStore;

                if (!PrimaryCategory.HasValue)
                    return "Homeware";

                string name;
                if (iaStore.PrimaryCategoryNames.TryGetValue(PrimaryCategory.Value, out name))
                    return name;

                return "Homeware";
            }
        }

        public List<string> CategoryAncestorList
        {
            get
            {
                var result = new List<string>() { "Homeware" };
                var iaStore = Store as InsideAvenueWebStore;

                List<int> ancestors; // includes self, up to but not including root
                if (PrimaryCategory.HasValue && iaStore.PrimaryCategoryAncestors.TryGetValue(PrimaryCategory.Value, out ancestors))
                {
                    foreach (var catID in ancestors)
                    {
                        string name;
                        if (iaStore.PrimaryCategoryNames.TryGetValue(catID, out name))
                            result.Add(name);
                    }
                }

                // most granular at the front of the list
                result.Reverse();
                return result;
            }
        }

        protected override string GoogleProductCategory
        {
            get
            {
                var schemaCategory = "Home & Garden > Decor"; // default
                // change to something specific if we've got something, else leave default
                if (PrimaryCategory.HasValue)
                {
                    var iaStore = Store as InsideAvenueWebStore;

                    // associations might be missing from the original CSV file of our internal categories.
                    // will only yield a non-default result here if all required pieces found.

                    int googleCategoryID;
                    if (iaStore.PrimaryCategoriesGoogleTaxonomyID.TryGetValue(PrimaryCategory.Value, out googleCategoryID))
                    {
                        string googleCategoryText;
                        if (iaStore.GoogleTaxonomyMap.TryGetValue(googleCategoryID, out googleCategoryText))
                            schemaCategory = googleCategoryText;
                    }
                }

                return schemaCategory;
            }
        }


        public string Dimensions
        {
            get
            {

                Func<double?, bool> isDefined = (v) =>
                {
                    if (v.GetValueOrDefault() > 0)
                        return true;

                    return false;
                };

                Func<string> addDiameter = () =>
                {
                    if (Features.Features != null && Features.Features.ContainsKey("Diameter") && !string.IsNullOrWhiteSpace(Features.Features["Diameter"]) && Features.Features["Diameter"] != "0")
                    {
                        return string.Format(", Diameter {0}", Features.Features["Diameter"]);
                    }
                    else
                        return string.Empty;
                };

                if (isDefined(Features.Depth) || isDefined(Features.Width) || isDefined(Features.Height))
                {
                    // create a string

                    if (isDefined(Features.Depth) && isDefined(Features.Width) && isDefined(Features.Height))
                    {
                        return string.Format("{0}\" W x {1}\" H x {2}\" D", Features.Width.Value.ToString("#.####"), Features.Height.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####"));
                    }
                    else if (isDefined(Features.Width) && isDefined(Features.Height))
                    {
                        return string.Format("{0}\" W x {1}\" H", Features.Width.Value.ToString("#.####"), Features.Height.Value.ToString("#.####"));
                    }
                    else if (isDefined(Features.Width) && isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" W x {1}\" D", Features.Width.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####")) + addDiameter();
                    }
                    else if (isDefined(Features.Height) && isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" H x {1}\" D", Features.Height.Value.ToString("#.####"), Features.Depth.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Width))
                    {
                        return string.Format("{0}\" W", Features.Width.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Height))
                    {
                        return string.Format("{0}\" H", Features.Height.Value.ToString("#.####")) + addDiameter();

                    }
                    else if (isDefined(Features.Depth))
                    {
                        return string.Format("{0}\" D", Features.Depth.Value.ToString("#.####")) + addDiameter();
                    }
                    else
                        return null;
                }
                else if (Features.Features != null && Features.Features.ContainsKey("Diameter") && !string.IsNullOrWhiteSpace(Features.Features["Diameter"]) && Features.Features["Diameter"] != "0")
                {
                    return string.Format("Diameter {0}", Features.Features["Diameter"]);
                }

                return null;
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

                var f = Features;
                var excludedProperties = new string[] { "dimensions", "bulbs", "country", "diameter", "shipping weight" };
                var excludedPropertyValues = new string[] { "yes", "no"};

                var rec = new AlgoliaProductRecord(p.ProductID)
                {
                    sku = p.SKU,
                    name = p.Name,
                    brand = m.Name,
                    mpn = string.Format("{0} {1}", m.Name, pv.ManufacturerPartNumber),
                    isLive = p.ShowBuyButton == 1,
                    rank = (p.ShowBuyButton == 1) ? 10 : 2
                };

                if (!string.IsNullOrEmpty(f.UPC))
                    rec.upc = string.Format("UPC {0}", f.UPC);

                foreach (var name in CategoryAncestorList)
                    rec.AddCategory(name.ToLower());

                // all the various features and properties

                if (f.Features != null)
                {
                    if (f.Features != null)
                    {
                        foreach (var item in f.Features)
                        {
                            if (string.IsNullOrWhiteSpace(item.Value))
                                continue;

                            var value = item.Value.Trim().ToLower();

                            if (excludedProperties.Contains(item.Key.ToLower()))
                                continue;

                            if (excludedPropertyValues.Contains(value))
                                continue;

                            if (value.Length < 5 && value.IsNumeric())
                                continue;

                            if (value.ContainsIgnoreCase(" inches"))
                                continue;

                            if (value.Contains(","))
                            {
                                foreach (var splitValue in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    rec.AddProperty(splitValue.Trim());
                            }
                            else if (value.Contains(";"))
                            {
                                foreach (var splitValue in value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                    rec.AddProperty(splitValue.Trim());
                            }
                            else
                                rec.AddProperty(value);
                        }
                    }
                }

                return rec;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }


        }

        public HomewareProductFeatures Features
        {
            get
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.HomewareProductFeatures, out obj))
                {
                    var f = obj as HomewareProductFeatures;
                    return f;
                }
                throw new Exception(string.Format("Missing HomewareProductFeatures for ProductID: {0}", p.ProductID));
            }
        }

        public HumanHomewareProductFeatures HumanFeatures
        {
            get
            {
                object obj;
                if (ExtData4.TryGetValue(ExtensionData4.HumanHomewareProductFeatures, out obj))
                {
                    var f = obj as HumanHomewareProductFeatures;
                    return f;
                }

                return null;
            }
        }


        /// <summary>
        /// Main entry point for per-product which figures out the phrase list to pump out.
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
                var iaStore = Store as InsideAvenueWebStore;

                var set = new HashSet<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var s2 = s.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(s2))
                        set.Add(s2);
                };

                // also include sku suffix when has variants

                if (p.ShowBuyButton == 0)
                    add("discontinued");

                add(m.Name);

                foreach (var pv in variants)
                {
                    add(pv.ManufacturerPartNumber);
                    add(pv.Name);

                    if (!string.IsNullOrWhiteSpace(pv.SKUSuffix))
                    {
                        add(p.SKU + pv.SKUSuffix);
                    }
                }

                if (variants.Count() == 1)
                    add(p.SKU);

                List<int> ancestors; // includes self, up to but not including root
                if (PrimaryCategory.HasValue && iaStore.PrimaryCategoryAncestors.TryGetValue(PrimaryCategory.Value, out ancestors))
                {
                    foreach(var catID in ancestors)
                    {
                        string name;
                        if (iaStore.PrimaryCategoryNames.TryGetValue(catID, out name))
                            add(name);
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
        /// The Ext3 data is used as a temporary bridge or crutch during the transition to tune FTS.
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

                add(pv.Name);

                // if anything to add, do it here
                // if anything to add, do it here
                // if anything to add, do it here

                var ext3 = set.ToList().ConvertToLines();

                dc.Products.UpdateExtensionData3(p.ProductID, ext3);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        /// <summary>
        /// Find products which have been human-classified and add a record
        /// to Ext4 to prevent scanner from overriding in the future.
        /// </summary>
        /// <remarks>
        /// Intended as one-pass jump start as human edit logic is brought live.
        /// </remarks>
        [RunProductAction("DetectHumanReviewedProducts")]
        public void DetectHumanReviewedProducts()
        {
            try
            {
                if (p.Published == 0 || p.Deleted == 1)
                    return;

                var iaStore = Store as InsideAvenueWebStore;

                var categoryConverter = new Dictionary<int, int>()
                    {
                        {1015, 1012}, // lanterns
                        {1011, 1012}, // candleabras
                        {1013, 1012}, // candle sconce
                        {1010, 1012}, // candlesticks
                        {1014, 1012}, // hurricane
                        {1016, 1012}, // pillar holders
                        {1006, 1088}, // decorative bowls

                        {1007, 1033}, // Magazine Racks
                        {1102, 1033}, // Wine Racks

                    };

                // what is the true internalID (1000 range).
                // not sure if above dic is needed, but safe to have. Not sure if ID in features already has these remaps

                int internalCategory;
                if (!categoryConverter.TryGetValue(Features.Category, out internalCategory))
                    internalCategory = Features.Category;
                
                // starting out...we only have knowledge of primary categories

                if (PrimaryCategory != null)
                {
                    int sqlCategoryIDmappedFromInternalID;
                    if (!iaStore.InternalToSqlCategoryMapping.TryGetValue(internalCategory, out sqlCategoryIDmappedFromInternalID))
                        sqlCategoryIDmappedFromInternalID = 0;

                    if (sqlCategoryIDmappedFromInternalID != PrimaryCategory.Value)
                    {
                        // has been changed

                        // should actually always get back null since this is  a jump start
                        var humanChanges = HumanFeatures ?? new HumanHomewareProductFeatures();

                        // see if already preserved
                        if (humanChanges.PrimarySqlCategory.HasValue && humanChanges.PrimarySqlCategory.Value == PrimaryCategory)
                            return;

                        humanChanges.PrimarySqlCategory = PrimaryCategory;

                        string scannerCategoryName = null;
                        if (!iaStore.PrimaryCategoryNames.TryGetValue(sqlCategoryIDmappedFromInternalID, out scannerCategoryName))
                            scannerCategoryName = "UnClassified";

                        // Debug.WriteLine(string.Format("Reclassified {3} ({0}) from {1} to {2}: {4}", p.ProductID, scannerCategoryName, PrimaryCategoryName, p.SKU, p.Name));

                        //Action writeLine = () =>
                        //{
                        //    lock (lockObj)
                        //    {
                        //        using (StreamWriter sw = File.AppendText(@"c:\temp\reclassified.txt"))
                        //        {
                        //            var line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", p.ProductID, p.SKU, p.Name, internalCategory, PrimaryCategory, scannerCategoryName, PrimaryCategoryName);
                        //                sw.WriteLine(line);
                        //        }
                        //    }
                        //};

                        //writeLine();

                        ExtData4[ExtensionData4.HumanHomewareProductFeatures] = humanChanges;
                        MarkExtData4Dirty();
                        SaveExtData4();
                    }
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }
        


        [RunProductAction("RebuildCategoriesFromExt")]
        public void RebuildCategoriesFromExt()
        {
            try
            {
                var internalCategory = Features.Category;
                if (internalCategory == 0)
                    return;

                var sqlRootNode = dc.Categories.Where(e => e.CategoryID == 1).FirstOrDefault();

                if (sqlRootNode != null && !string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                {
                    var lookupMap = sqlRootNode.ExtensionData.FromJSON<Dictionary<int, int>>();

                    int sqlCategoryID = 0;

                    var categoryConverter = new Dictionary<int, int>()
                    {
                        {1015, 1012}, // lanterns
                        {1011, 1012}, // candleabras
                        {1013, 1012}, // candle sconce
                        {1010, 1012}, // candlesticks
                        {1014, 1012}, // hurricane
                        {1016, 1012}, // pillar holders
                        {1006, 1088}, // decorative bowls

                        {1007, 1033}, // Magazine Racks
                        {1102, 1033}, // Wine Racks

                    };

                    if (categoryConverter.ContainsKey(internalCategory))
                        internalCategory = categoryConverter[internalCategory];

                    dc.ProductCategories.RemoveProductCategoryAssociationsForProduct(p.ProductID);
                    if (lookupMap.TryGetValue(internalCategory, out sqlCategoryID))
                    {
                        dc.ProductCategories.AddProductToCategory(sqlCategoryID, p.ProductID);
                    }
                    else
                    {
                        // default to accessories
                        dc.ProductCategories.AddProductToCategory(41, p.ProductID);
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }


        /// <summary>
        /// For all products, make sure belongs to only a single primary category.
        /// </summary>
        /// <remarks>
        /// When multiple found, keep only the newest. Put orpans into 117-unclassified.
        /// </remarks>
        [RunProductAction("EnsureSinglePrimaryCategory")]
        public void EnsureSinglePrimaryCategory()
        {
            try
            {
                var iaStore = Store as InsideAvenueWebStore;
                
                var primaryCats = new List<Category>();

                foreach(var cat in categories)
                {
                    if (iaStore.PrimarySqlCategories.Contains(cat.CategoryID))
                        primaryCats.Add(cat);
                }

                // if has a primary and is also in unclassified, remove from unclassified
                if (primaryCats.Count != 0 && categories.Where(e => e.CategoryID == UNCLASSIFIED_CATEGORYID).Count() > 0)
                    dc.ProductCategories.RemoveProductFromCategory(UNCLASSIFIED_CATEGORYID, p.ProductID);

                // all is good
                if (primaryCats.Count == 1)
                {
                    return;
                }

                // orphan, add to unclassified for human review
                if (primaryCats.Count == 0)
                {
                    // do not put dead products into unclassified, since that would mean unnecessary human review
                    if (p.Published == 0 || p.Deleted == 1)
                        return;

                    Debug.WriteLine(string.Format("Product is category orphan: {0}", p.ProductID));
                    dc.ProductCategories.AddProductToCategory(UNCLASSIFIED_CATEGORYID, p.ProductID);
                    return;
                }

                // else, multiple, keep newest
                var targetCatIDs = primaryCats.Select(e => e.CategoryID).ToList();
                var pc = dc.ProductCategories.Where(e => e.ProductID == p.ProductID && targetCatIDs.Contains(e.CategoryID)).OrderByDescending(e => e.CreatedOn).ToList();
                // remove newest from the list
                targetCatIDs.RemoveAll(e => e == pc.First().CategoryID);
                foreach (var id in targetCatIDs)
                {
                    dc.ProductCategories.RemoveProductFromCategory(id, p.ProductID);
                    Debug.WriteLine(string.Format("Remove category {0} for product {1}", id, p.ProductID));
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        /// <summary>
        /// For all products, clear and repopulate SQL ProductLabels table.
        /// </summary>
        [RunProductAction("RebuildProductLabels")]
        public void RebuildProductLabels()
        {
            var dic = new Dictionary<string, string>();

            Action<string, string> add = (n, v) =>
                {
                    if (string.IsNullOrWhiteSpace(n) || string.IsNullOrWhiteSpace(v))
                        return;

                    dic[n] = v;
                };


            Action<string, double?> addDimension = (name, value) =>
            {
                if (value.GetValueOrDefault() < .01)
                {
                    add(name, "0");
                    return;
                }

                Func<double, string> toInches = (inches) =>
                {
                    return string.Format("{0} {1}", inches.ToString("#.####"), (inches == 1.0 ? "inch" : "inches"));
                };

                add(name, toInches(value.Value));
            };

            var f = this.Features;

            addDimension("P:Depth", f.Depth);
            addDimension("P:Height", f.Height);
            addDimension("P:Width", f.Width);

            if (f.ShippingWeight.HasValue)
            {
                if (f.ShippingWeight.Value > .01)
                {
                    int weight = (int)Math.Ceiling(f.ShippingWeight.Value);
                    string label = weight == 1 ? "lb." : "lbs.";
                    add("P:Shipping Weight", string.Format("{0} {1}", weight, label));
                }
                else
                {
                    add("P:Shipping Weight", "0");
                }
            }

            if (f.Description != null && f.Description.Count() > 0)
                add("P:Description", f.Description.First());

            add("P:CareInstructions", f.CareInstructions);

            add("P:Color", f.Color);

            add("P:Category", f.Category.ToString());

            if (f.Features != null)
            {
                foreach (var item in f.Features)
                    add(item.Key, item.Value);
            }

            add("P:Lead Time", f.LeadTime);
            add("P:Note", f.PleaseNote);
            add("P:UPC", f.UPC);

            if (f.Bullets != null && f.Bullets.Count() > 0)
                add("P:BulletCount", f.Bullets.Count().ToString());
            else
                add("P:BulletCount", "0");

            if (f.Description == null || f.Description.Count() == 0)
            {
                add("P:DescriptionCount", "0");
            }
            else
                add("P:DescriptionCount", f.Description.Count().ToString());


            add("P:Note", f.PleaseNote);


            dc.UpdateProductLabels(p.ProductID, dic);
        }


        protected override bool SpinDescription()
        {
            try
            {
                // TODO: SpinDescription

                p.Description = "<p>Temporary description.</p>";

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
                // TODO: SpinFroogleDescription

                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;

                //var maker = new FroogleDescriptionMakerFabric(this);
                //p.FroogleDescription = maker.Description;
                p.FroogleDescription = "Temporary froogle description.";
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
            try
            {
                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;

                var sb = new StringBuilder(1024);

                sb.AppendFormat("Shop {0}. ", PrimaryCategoryName);
                sb.Append(p.Name);
                sb.Append(".");

                if (!string.IsNullOrWhiteSpace(Features.Color))
                    sb.AppendFormat(" Color: {0}.", Features.Color);

                var size = Dimensions;
                if (!string.IsNullOrWhiteSpace(size))
                    sb.AppendFormat(" Size: {0}.", size);

                sb.Append(" Free shipping.");

                p.SEDescription = sb.ToString();
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }


        protected override bool SpinSEKeywords()
        {
            try
            {
                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;

                p.SEKeywords = CategoryAncestorList.ToCommaDelimitedList();
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }




        #endregion

        #region Local Methods


        #endregion
    }


}
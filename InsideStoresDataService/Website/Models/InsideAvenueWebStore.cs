using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel.Composition;
using System.Threading;
using Gen4.Util.Misc;
using System.Diagnostics;
using Website.Entities;
using System.Data;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace Website
{
    /// <summary>
    /// InsideAvenue store implementation.
    /// </summary>
    [Export("InsideAvenue", typeof(IWebStore))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InsideAvenueWebStore : WebStoreBase<ProductDataCache>, IWebStore
    {
        /// <summary>
        /// The categoryID of the root SQL entity under which our classes/subclasses categories are created.
        /// </summary>
        protected const int CLASS_ROOT_CATEGORYID = 1;

        private static List<ProductGroup> _supportedProductGroups = new List<ProductGroup>() { ProductGroup.Homeware }; // if multiple, the primary must be listed first

        private const string StoreFriendlyName = "Inside Avenue";
        private const string StoreDomainName = "insideavenue.com";
        private const string ClassesRootCategoryGuid = "{E5DFA7F2-F9CE-49F7-8EC5-A190FCF189FA}";

        /// <summary>
        /// From google taxonomy text file: Dic[id, text category breadcrumbs]
        /// </summary>
        public Dictionary<int, string> GoogleTaxonomyMap = new Dictionary<int, string>();

        /// <summary>
        /// Dic[sqlCatID, googleTaxID], for known/valid associations.
        /// </summary>
        public Dictionary<int, int> PrimaryCategoriesGoogleTaxonomyID = new Dictionary<int, int>();

        /// <summary>
        /// List of our primary categories having products either direct or in subclass.
        /// </summary>
        public HashSet<int> CategoriesWithProducts = new HashSet<int>();

        /// <summary>
        /// Mapping of internal category identifier to SQL categoryID; dic[internalID, sqlID]
        /// </summary>
        public Dictionary<int, int> InternalToSqlCategoryMapping = new Dictionary<int, int>();

        /// <summary>
        /// List of SQL categories corresponding to our internal category identifiers.
        /// </summary>
        public List<int> PrimarySqlCategories = new List<int>();

        /// <summary>
        /// Names corresponding to the primary categories - for FTS and autosuggest.
        /// </summary>
        public Dictionary<int, string> PrimaryCategoryNames = new Dictionary<int, string>();


        /// <summary>
        /// Dic[sqlCatID, ordered list of parents from root, not including root(1) node, does include self]
        /// </summary>
        public Dictionary<int, List<int>> PrimaryCategoryAncestors = new Dictionary<int, List<int>>();

        public InsideAvenueWebStore()
            : base(StoreKeys.InsideAvenue)
        {
            // tickler campaigns

            if (IsTicklerCampaignsEnabled)
                TicklerCampaignsManager = new TicklerCampaignsManager(this as IWebStore);

            // stop here if just initializing a fresh database
            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                if (dc.Categories.Count() < 5 || dc.Products.Count() == 0)
                    return;
            }

            LoadGoogleTaxonomyMap();
            LoadPrimaryCategoriesGoogleTaxonomyID();
            LoadPrimaryCategories(); // and PrimaryCategoryNames
            LoadPrimaryCategoryAncestors();
            LoadCategoriesWithProducts();
        }


        #region Private Initialization

        private void LoadPrimaryCategories()
        {
            // and PrimaryCategoryNames

            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                if (sqlRootNode == null)
                    return;

                // extension data will contain a Dictionary<int, int>

                if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                    return;

                InternalToSqlCategoryMapping = JsonConvert.DeserializeObject<Dictionary<int, int>>(sqlRootNode.ExtensionData);

                PrimarySqlCategories = InternalToSqlCategoryMapping.Values.ToList();

                PrimaryCategoryNames = dc.Categories.Where(e => PrimarySqlCategories.Contains(e.CategoryID)).Select(e => new { e.CategoryID, e.Name }).ToDictionary(k => k.CategoryID, v => v.Name);
            }
        }


        private void LoadGoogleTaxonomyMap()
        {
            var filepath = ConfigurationManager.AppSettings["GoogleTaxonomyPath"];

            if (string.IsNullOrEmpty(filepath))
                throw new Exception("Missing Appsetting: GoogleTaxonomyPath");

            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // sample line formatting
                // 4433 - Animals & Pet Supplies > Pet Supplies > Cat Supplies > Cat Beds
                // 3367 - Animals & Pet Supplies > Pet Supplies > Cat Supplies > Cat Food

                var aryParts = line.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                if (aryParts.Length != 2)
                    continue;

                int id;
                if (!int.TryParse(aryParts[0], out id))
                    continue;

                GoogleTaxonomyMap[id] = aryParts[1].Trim();
            }
        }

        private void LoadPrimaryCategoriesGoogleTaxonomyID()
        {
            // applied only to InsideAvenue

            const int CLASS_ROOT_CATEGORYID = 1;

            try
            {
                using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                {
                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        return;

                    // extension data will contain a Dictionary<int, int>

                    if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                        return;

                    var dicMap = JsonConvert.DeserializeObject<Dictionary<int, int>>(sqlRootNode.ExtensionData);

                    var mgr = new InsideFabric.Data.HomewareCategoryManager();
                    var nodes = mgr.LoadCsvFile();

                    // dic[sqlCatID, googleTaxID]
                    var dic = new Dictionary<int, int>();

                    foreach (var node in nodes)
                    {
                        // 0 means null, no value
                        if (node.GoogleTaxonomyId == 0)
                            continue;

                        if (!GoogleTaxonomyMap.ContainsKey(node.GoogleTaxonomyId))
                            continue;

                        int sqlCatID;
                        if (!dicMap.TryGetValue(node.Id, out sqlCatID))
                            continue;

                        dic[sqlCatID] = node.GoogleTaxonomyId;
                    }
                    PrimaryCategoriesGoogleTaxonomyID = dic;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

        }


        private void LoadCategoriesWithProducts()
        {
            CategoriesWithProducts = new HashSet<int>();

            var mgr = new InsideFabric.Data.HomewareCategoryManager();

            // build up a mapping of our internal catID to SQL catID.
            Func<Dictionary<int, int>> populateLookup = () =>
            {
                using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                {
                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        throw new Exception(string.Format("Missing root category node (CategoryID={0}).", CLASS_ROOT_CATEGORYID));

                    // could be we are initializing from scratch, so detect that and don't bomb out
                    if (string.IsNullOrEmpty(sqlRootNode.ExtensionData) && dc.Categories.Count() < 5)
                        return new Dictionary<int, int>();

                    // extension data will contain a Dictionary<int, int>

                    if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                        throw new Exception(string.Format("Missing root category node extension data (CategoryID={0}).", CLASS_ROOT_CATEGORYID));

                    return JsonConvert.DeserializeObject<Dictionary<int, int>>(sqlRootNode.ExtensionData);
                }
            };

            Func<Dictionary<int, int>> getProductCounts = () =>
            {
                // return a dic of name=catID, value = number of products in SQL ProductCategory table
                using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                {
                    return dc.ProductCategories.GroupBy(e => e.CategoryID).ToDictionary(k => k.Key, v => v.Count());
                }
            };


            var rootNode = mgr.LoadTree();
            var lookupMap = populateLookup();
            var productCounts = getProductCounts();

            Action<InsideFabric.Data.HomewareCategoryNode> buildCollection = null;
            buildCollection = (node) =>
            {
                int sqlCategoryID;
                if (!lookupMap.TryGetValue(node.Id, out sqlCategoryID))
                    return;

                // need to recurse all childreen first
                foreach (var child in node.Children)
                    buildCollection(child);

                bool hasProducts = false;

                int directProducts;
                if (!productCounts.TryGetValue(sqlCategoryID, out directProducts) || directProducts == 0)
                {
                    // no direct products, see if any children had products
                    foreach (var child in node.Children)
                    {
                        int childSqlCategoryID;
                        if (!lookupMap.TryGetValue(child.Id, out childSqlCategoryID))
                            continue;

                        if (CategoriesWithProducts.Contains(childSqlCategoryID))
                        {
                            hasProducts = true;
                            break;
                        }
                    }
                }
                else
                    hasProducts = true;

                if (hasProducts)
                    CategoriesWithProducts.Add(sqlCategoryID);
            };

            buildCollection(rootNode);
        }


        private void LoadPrimaryCategoryAncestors()
        {
            // applied only to InsideAvenue

            const int CLASS_ROOT_CATEGORYID = 1;

            try
            {
                using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
                {


                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        return;

                    // extension data will contain a Dictionary<int, int>

                    if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                        return;

                    var dicMap = JsonConvert.DeserializeObject<Dictionary<int, int>>(sqlRootNode.ExtensionData);

                    var mgr = new InsideFabric.Data.HomewareCategoryManager();

                    // root has parentID=0, its own ID is 1000
                    var root = mgr.LoadTree();

                    Action<InsideFabric.Data.HomewareCategoryNode> populateAncestors = null;
                    populateAncestors = (node) =>
                    {
                        var sqlID = dicMap[node.Id];

                        if (node.Parent != null)
                        {
                            // SQL IDs - first is closest to root - includes self
                            var ancestors = new List<int>()
                                {
                                    sqlID
                                };

                            var parent = node.Parent;
                            while (parent != null && parent.ParentId != 0)
                            {
                                ancestors.Add(dicMap[parent.Id]);
                                parent = parent.Parent;
                            }
                            ancestors.Reverse();
                            PrimaryCategoryAncestors[sqlID] = ancestors;
                        }

                        foreach (var child in node.Children.Where(e => e.Included))
                            populateAncestors(child);
                    };

                    populateAncestors(root);
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }

        }


        #endregion


        public override string FriendlyName
        {
            get { return StoreFriendlyName; }
        }

        public override string Domain
        {
            get { return StoreDomainName; }
        }

        /// <summary>
        /// When true - indicates store supports tracking inventory.
        /// </summary>
        public override bool HasAutomatedInventoryTracking
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns a list of the supported product groups for this store.
        /// </summary>
        /// <remarks>
        /// Many operations will filter results to return only products within these listed groups.
        /// </remarks>
        public override List<ProductGroup> SupportedProductGroups
        {
            get
            {
                return _supportedProductGroups;
            }
        }



        public override IProductFeedManager ProductFeedManager
        {
            get
            {
                if (productFeedManager == null)
                    productFeedManager = new InsideAvenueProductFeedManager(this);

                return productFeedManager;
            }
        }

        #region RunAction

        /// <summary>
        /// Run an internally designated maint action for all products.
        /// </summary>
        /// <remarks>
        /// The action needs to be hard coded. Generally, the base does all the work, but
        /// we need to put in this stub to make it spring into action.
        /// </remarks>
        /// <returns>Message to display on website.</returns>
        public override string RunActionForAllProducts(string actionName, string tag)
        {
            return RunActionForAllProducts<InsideAvenueProduct>(actionName, tag);
        }

        /// <summary>
        /// Run an internally designated maint action for the store.
        /// </summary>
        /// <remarks>
        /// The action needs to be hard coded. Generally, the base does all the work, but
        /// we need to put in this stub to make it spring into action.
        /// </remarks>
        /// <returns>Message to display on website.</returns>
        public override string RunAction(string actionName, string tag)
        {
            return RunActionForStore(actionName, tag);
        }

        /// <summary>
        /// Perfom a variety of general cleanup actions. Can be run as often as desired. No harm.
        /// </summary>
        /// <remarks>
        /// Cleans up orphan impages. Prunes any deleted entities and related maps.
        /// </remarks>
        [RunStoreAction("Cleanup")]
        public void Cleanup(CancellationToken cancelToken)
        {
            // NotifyStoreActionProgress(i);

            NotifyStoreActionProgress(10);
            RunSQLCleanup();

            NotifyStoreActionProgress(20);

            EnsureAllProductsHaveADefaultVariant();

            if (cancelToken.IsCancellationRequested)
                return;

            NotifyStoreActionProgress(50);

            bool bPerformModifications = true;
            MaintenanceHub.NotifyRunStoreActionStatus("Processing product images...");            

            // be at 50% here for final steps

            CleanupOrphanProductImages(bPerformModifications, cancelToken, (pct) =>
                {
                    var adjPct = (int)Math.Round(pct * .5M, 0);

                    NotifyStoreActionProgress(adjPct + 50);
                });

            NotifyStoreActionProgress(100);

        }


        #endregion

        private void EnsureAllProductsHaveADefaultVariant()
        {
            try
            {
                using (var dc = new AspStoreDataContext(ConnectionString))
                {
                    var products = dc.Products.Where(e => e.Deleted==0 && e.Published==1).Select(e => e.ProductID).ToList();
                    foreach(var productID in products)
                    {
                        var variants = dc.ProductVariants.Where(e => e.ProductID == productID).ToList();

                        if (variants.Count() == 0)
                            continue;

                        // none that are deleted or unpublished should be default

                        foreach (var v in variants.Where(e => e.Deleted == 0 || e.Published == 0))
                            v.IsDefault = 0;

                        if (variants.Where(e => e.Deleted == 0 && e.Published == 1).Count() == 0)
                            continue;

                        var countDefault = variants.Where(e => e.IsDefault == 1).Count();

                        if (countDefault == 0)
                        {
                            // set first born to be default
                            variants.Where(e => e.Deleted == 0 && e.Published == 1).OrderBy(e => e.VariantID).First().IsDefault = 1;
                        }
                        else if (countDefault > 1)
                        {
                            foreach (var dup in variants.Where(e =>  e.Deleted == 0 && e.Published == 1 && e.IsDefault == 1).OrderBy(e => e.VariantID).Skip(1))
                                dup.IsDefault = 0;
                        }
                    }

                    dc.SubmitChanges();

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected override DataTable SearchBySku(string query)
        {
            // called from WebStoreBaseQuerries
            
            // for inside avenue - skip the step of trying to see if might be exactly a SKU number
            // and let it be found via the FTS logic.

            // the core problem with IA is that the SkuSuffix field is important when using variants

            // not really any performance difference anyway now that advanced logic is in place.

            return null;
        }


        protected override void RebuildAllStoreSpecificTasks()
        {
            // any misc tasks like spinning up missing descriptions, etc.
            SpinUpMissingDescriptions(this.ConnectionString);
        }

        protected override AlgoliaProductRecord MakeAlgoliaProductRecord(int productID, AspStoreDataContext dc)
        {
            AlgoliaProductRecord record = null;
            ProductUpdateManager<InsideAvenueProduct>.ProcessProduct(this, productID, (product) =>
            {
                record = product.MakeAlgoliaProductRecord();
            }, dc);

            return record;
        }


        protected void SpinUpMissingDescriptions(string connectionString)
        {
            var mgr = new ProductUpdateManager<InsideWallpaperProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel

                p.SpinUpMissingDescriptions();

                return true;
            }, useParallelOperations: true);

        }


        /// <summary>
        /// Do all the work to rebuild the search data. Throw if error.
        /// </summary>
        /// <remarks>
        /// This is the main worker entry point when the "Rebuild Search Data" button is clicked.
        /// </remarks>
        protected override void RebuildProductSearchDataEx()
        {
            using (var dc = new AspStoreDataContext(ConnectionString))
            {
                // fill in Ext2 for each product - one line per keyword phrase
                PopulateProductKeywords(ConnectionString);

                // fill in AutoSuggest table using mostly product keywords in Ext2
                PopulateAutoSuggestTable(dc);
            }

            // now that the preliminary processing is complete, rebuild the FTS catelog

            var scriptFilepath = MapPath(string.Format(@"~/App_Data/Sql/{0}RebuildSearchData.sql", StoreKey));

            // this is the SQL command to rebuild the FTS catalog

            //USE [InsideAvenue]
            //ALTER FULLTEXT CATALOG [Inside Ave Products]
            //REBUILD WITH ACCENT_SENSITIVITY = ON
            //GO

            // this operation returns immediately, even though the FTS rebuild occurs in the background within SQL server.
            RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        }

#if false
        /// <summary>
        /// Allow subclass to manually inject specific phrases.
        /// </summary>
        protected override List<string> InjectedAutoSuggestPhrases
        {
            get
            {
                return new List<string>()
                {
                    //"some phrase",
                    //"some phrase",
                    //"some phrase",
                };
            }
        }
#endif

        private void PopulateProductKeywords(string connectionString)
        {

            var mgr = new ProductUpdateManager<InsideAvenueProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel

                p.MakeAndSaveExt2KeywordList(); // FTS and autosuggest

                // Ext3 is used as a temporary bridge/crutch to keep FTS working to some reasonable
                // degree while we finalize Ext2 and autocomplete. FTS only. Not for autosuggest.
                p.MakeAndSaveExt3KeywordList();

                return true;
            }, useParallelOperations: true);
        }

        /// <summary>
        /// Spin through any queued up images to process.
        /// </summary>
        /// <remarks>
        /// Collects new images, saves, resizes, stores. 
        /// </remarks>
        /// <param name="cancelToken"></param>
        [RunStoreAction("ProcessImageQueue")]
        public void ProcessImageQueue(CancellationToken cancelToken)
        {
            var mgr = new InsideAvenueProductImageProcessor(this);

            var progress = new Progress<int>((pct) =>
            {
                NotifyStoreActionProgress(pct);
            });

            // will throw if problem - and caught by caller
            mgr.ProcessQueue(cancelToken, progress, (status) =>
            {
                MaintenanceHub.NotifyRunStoreActionStatus(status);
            });

            NotifyStoreActionProgress(100);
        }

        /// <summary>
        /// Ad-hoc maint method to inject one or more internal-to-sql category associtions into
        /// the mapping table in root node category.ExtensionData.
        /// </summary>
        /// <remarks>
        /// Fill in "toBeInjected" and run once. JSON will be updated and saved.
        /// </remarks>
        /// <param name="cancelToken"></param>
        [RunStoreAction("InjectCategories")]
        public void InjectCategories(CancellationToken cancelToken)
        {
            try
            {
                var mgr = new InsideFabric.Data.HomewareCategoryManager();

                System.IProgress<int> progress = new Progress<int>((pct) =>
                {
                    NotifyStoreActionProgress(pct);
                });


                progress.Report(10);

                // build up a mapping of our internal catID to SQL catID.
                var lookupMap = new Dictionary<int, int>();

                using (var dc = new AspStoreDataContext(ConnectionString))
                {

                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        throw new Exception(string.Format("Missing root category node (CategoryID={0}).", CLASS_ROOT_CATEGORYID));

                    if (sqlRootNode.CategoryGUID != Guid.Parse(ClassesRootCategoryGuid))
                        throw new Exception("The GUID for the root category must be: " + ClassesRootCategoryGuid);

                    // extension data will contain a Dictionary<int, int> unless raw

                    // mapping dic already existing, hydrate
                    lookupMap = sqlRootNode.ExtensionData.FromJSON<Dictionary<int, int>>();

                    var toBeInjected = new Dictionary<int, int>()
                    {
                        {1141, 118}, // vanity lights 5/31/2017
                    };

                    foreach(var item in toBeInjected)
                    {
                        lookupMap[item.Key] = item.Value; 
                    }
                        
                    sqlRootNode.ExtensionData = lookupMap.ToJSON();
                    dc.SubmitChanges();
                }

                progress.Report(100);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }


        [RunStoreAction("PrintCategoryTree")]
        public void PrintCategoryTree(CancellationToken cancelToken)
        {
            const string filename = @"c:\temp\categoryTree.txt";
            try
            {
                var mgr = new InsideFabric.Data.HomewareCategoryManager();

                System.IProgress<int> progress = new Progress<int>((pct) =>
                {
                    NotifyStoreActionProgress(pct);
                });

                File.Delete(filename);
                Action<string> print = (s) =>
                    {
                        using (StreamWriter sw = File.AppendText(filename))
                        {
                            sw.WriteLine(s);
                        }
                    };

                progress.Report(10);

                var rootNode = mgr.LoadTree();
                var menuNames = mgr.LoadCsvFile().ToDictionary(k => k.Id, v => v.MenuName);

                // build up a mapping of our internal catID to SQL catID.
                var lookupMap = new Dictionary<int, int>();

                Action<InsideFabric.Data.HomewareCategoryNode, int> printTree = null;
                printTree = (node, depth) =>
                {
                    int sqlCatID=-1;
                    lookupMap.TryGetValue(node.Id, out sqlCatID);
                    var padding = "".PadLeft(depth * 5);
                    if (node.Included)
                        print(string.Format("{3}{0} ({1} -> {2})", node.MenuName, node.Id, sqlCatID, padding));
                    else
                        print(string.Format("{2}{0} ({1}, Excluded)", node.MenuName, node.Id, padding));

                    foreach (var childNode in node.Children)
                        printTree(childNode, depth + 1);
                };


                using (var dc = new AspStoreDataContext(ConnectionString))
                {

                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        throw new Exception(string.Format("Missing root category node (CategoryID={0}).", CLASS_ROOT_CATEGORYID));

                    if (sqlRootNode.CategoryGUID != Guid.Parse(ClassesRootCategoryGuid))
                        throw new Exception("The GUID for the root category must be: " + ClassesRootCategoryGuid);

                    // extension data will contain a Dictionary<int, int> unless raw

                    // mapping dic already existing, hydrate
                    lookupMap = sqlRootNode.ExtensionData.FromJSON<Dictionary<int, int>>();

                    lookupMap[rootNode.Id] = CLASS_ROOT_CATEGORYID;
                }

                print("Mapping of Internal CategoryID to SQL CategoryID");
                foreach (var item in lookupMap.OrderBy(e => e.Key))
                {
                    string menu = "Unknown";
                    menuNames.TryGetValue(item.Key, out menu);
                    print(string.Format("{0, -25} {1} -> {2}", menu, item.Key, item.Value));
                }
                print("\n\n");

                print("Menu Tree");

                printTree(rootNode, 0);

                print("\n\n");

                progress.Report(100);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }

        [RunStoreAction("PopulateCategories")]
        public void PopulateCategories(CancellationToken cancelToken)
        {
            try
            {
                // assumes this script used to start from scratch

                // truncate table Category
                // truncate table ProductCategory
                // SET Identity_Insert Category ON
                // insert Category(CategoryID, Name) values (1, 'Class Root')
                // insert Category(CategoryID, Name) values (2, 'Curated Root')
                // insert Category(CategoryID, Name) values (3, 'Outlet')
                // SET Identity_Insert Category OFF
                // DBCC CHECKIDENT ('[Category]', RESEED, 10);
                // update Category set CategoryGUID = '{E5DFA7F2-F9CE-49F7-8EC5-A190FCF189FA}' where CategoryID=1
                // update Category set CategoryGUID = '{8BC084D2-6B35-4BB7-8E5F-EC3A1A2F8742}' where Name='Home Improvement'
                // update Category set CategoryGUID = '{04EADA1C-1AB9-458E-9E2F-144403056CC0}' where Name='Outdoor'
                // update Category set CategoryGUID = '{830CA2F6-268B-4379-815B-E7A3E93677F2}' where Name='Decor'
                // update Category set CategoryGUID = '{FE0FBF81-254E-489A-A9CF-228306C02DC2}' where Name='Lighting'
                // update Category set CategoryGUID = '{527FB18F-3696-4CF6-9FD8-434429121B3A}' where Name='Furniture'

                // root (1) GUID must be E5DFA7F2-F9CE-49F7-8EC5-A190FCF189FA

                var mgr = new InsideFabric.Data.HomewareCategoryManager();

                System.IProgress<int> progress = new Progress<int>((pct) =>
                {
                    NotifyStoreActionProgress(pct);
                });

                progress.Report(10);

                var rootNode = mgr.LoadTree();

                Action<InsideFabric.Data.HomewareCategoryNode> fillWithDefaults = null;
                fillWithDefaults = (node) =>
                {
                    if (string.IsNullOrEmpty(node.Title))
                        node.Title = node.MenuName;

                    if (string.IsNullOrEmpty(node.Description))
                        node.Description = string.Format("Shop for {0}.", node.MenuName);

                    if (string.IsNullOrEmpty(node.SeKeywords))
                        node.SeKeywords = string.Format("{0}, shopping, temporary", node.MenuName);

                    if (string.IsNullOrEmpty(node.SeDescription))
                        node.SeDescription = string.Format("Shop for {0}. Temporary SeDescription.", node.MenuName);

                    foreach (var childNode in node.Children)
                        fillWithDefaults(childNode);

                };

                // build up a mapping of our internal catID to SQL catID.
                var lookupMap = new Dictionary<int, int>();

                Action<AspStoreDataContext, InsideFabric.Data.HomewareCategoryNode> persist = null;
                persist = (ctx, node) =>
                {
                    Website.Entities.Category cat = null;
                    if (lookupMap.ContainsKey(node.Id))
                        cat = ctx.Categories.Where(e => e.CategoryID == lookupMap[node.Id]).FirstOrDefault();

                    if (node.Included)
                    {
                        // validate include/exclude rule....that for every included node, all ancestors must also be included

                        var parentNode = node.Parent;
                        while(parentNode != null)
                        {
                            if (!parentNode.Included)
                                throw new Exception(string.Format("Parent node {0} ({1}) is excluded, yet has included child node(s): {2}", parentNode.MenuName, parentNode.Id, node.MenuName));
                            parentNode = parentNode.Parent;
                        }

                        // cannot correctly insert a child node unless the parent is already valid
                        if (node.Parent != null && !lookupMap.ContainsKey(node.ParentId))
                            throw new Exception(string.Format("lookupMap dictionary does not contain Parent node {0} for child node: {1}", node.ParentId, node.MenuName));

                        if (cat != null)
                        {
                            // already in SQL, update

                            cat.ParentCategoryID = lookupMap[node.ParentId]; // in case tree position changed
                            cat.Name = node.MenuName;
                            cat.Description = node.Title != null && node.Description != null ? string.Format("<h1>{0}</h1><p>{1}</p>", node.Title.HtmlEncode(), node.Description.HtmlEncode())
                                : string.Format("<h1>{0}</h1>", node.MenuName.HtmlEncode());
                            cat.SEName = node.MenuName.ToLower().Replace(" ", "-").Replace("&", "and");
                            cat.SETitle = node.Title ?? string.Format("{0}", node.MenuName);
                            cat.SEDescription = node.SeDescription;
                            cat.SEKeywords = node.SeKeywords;
                            cat.Summary = node.SearchTerms;

                            ctx.SubmitChanges();
                            Debug.WriteLine(string.Format("Updating category: {0}", cat.CategoryID));

                        }
                        else
                        {
                            // not in SQL, insert

                            cat = new Category()
                            {
                                CategoryGUID = Guid.NewGuid(),
                                ParentCategoryID = lookupMap[node.ParentId],
                                Name = node.MenuName,
                                Description = node.Title != null && node.Description != null ? string.Format("<h1>{0}</h1><p>{1}</p>", node.Title.HtmlEncode(), node.Description.HtmlEncode())
                                    : string.Format("<h1>{0}</h1>", node.MenuName.HtmlEncode()),
                                DisplayOrder = 0,
                                SEName = node.MenuName.ToLower().Replace(" ", "-").Replace("&", "and"),
                                XmlPackage = "entity.gridwithprices.xml.config",
                                SETitle = node.Title ?? string.Format("{0}", node.MenuName),
                                SEDescription = node.SeDescription,
                                SEKeywords = node.SeKeywords,
                                Summary = node.SearchTerms,
                                Published = 1,
                                Deleted = 0,
                                ExtensionData = null,
                                ImageFilenameOverride = null,

                                // below - required for insert to work
                                TemplateName = string.Empty,
                                CreatedOn = DateTime.Now,
                            };

                            ctx.Categories.InsertOnSubmit(cat);
                            ctx.SubmitChanges();
                            

                            Debug.WriteLine(string.Format("Adding category: {0}", cat.CategoryID));

                        }

                        lookupMap[node.Id] = cat.CategoryID;
                    }
                    else
                    {
                        // if excluded node is in SQL, remove

                        if (cat != null)
                        {
                            Debug.WriteLine(string.Format("Removing category: {0}", cat.CategoryID));
                            ctx.ProductCategories.RemoveProductCategoryAssociationsForCategory(cat.CategoryID);
                            ctx.Categories.RemoveCategory(cat.CategoryID);
                            lookupMap[node.Id] = -1; // mark for removal
                        }
                    }

                    foreach (var childNode in node.Children)
                        persist(ctx, childNode);
                };


                using (var dc = new AspStoreDataContext(ConnectionString))
                {


                    fillWithDefaults(rootNode);

                    lookupMap[rootNode.Id] = CLASS_ROOT_CATEGORYID;

                    var sqlRootNode = dc.Categories.Where(e => e.CategoryID == CLASS_ROOT_CATEGORYID).FirstOrDefault();

                    if (sqlRootNode == null)
                        throw new Exception(string.Format("Missing root category node (CategoryID={0}).", CLASS_ROOT_CATEGORYID));

                    if (sqlRootNode.CategoryGUID != Guid.Parse(ClassesRootCategoryGuid))
                        throw new Exception("The GUID for the root category must be: " + ClassesRootCategoryGuid);

                    // extension data will contain a Dictionary<int, int> unless raw

                    if (string.IsNullOrEmpty(sqlRootNode.ExtensionData))
                    {
                        // baseline insert, has only root node defined
                        sqlRootNode.ExtensionData = lookupMap.ToJSON();
                        dc.SubmitChanges();
                    }
                    else
                    {
                        // mapping dic already existing, hydrate
                        lookupMap = sqlRootNode.ExtensionData.FromJSON<Dictionary<int, int>>();
                    }

                    lookupMap[rootNode.Id] = CLASS_ROOT_CATEGORYID;

#if true
                    // repair lookup map using menu names - technically very close but not exact if names have been changed
                    lookupMap.Clear();
                    var fixMap = dc.Categories.Select(e => new { e.CategoryID, e.Name }).ToDictionary(k => k.Name, v => v.CategoryID);
                    foreach(var csvNode in mgr.LoadCsvFile())
                    {
                        if (fixMap.ContainsKey(csvNode.MenuName))
                        {
                            lookupMap[csvNode.Id] = fixMap[csvNode.MenuName];
                        }
                    }
                    lookupMap[rootNode.Id] = CLASS_ROOT_CATEGORYID;
                    sqlRootNode.ExtensionData = lookupMap.ToJSON();
                    dc.SubmitChanges();
#endif
                    foreach (var childNode in rootNode.Children)
                        persist(dc, childNode);

                    // remove any nodes that are no longer valid before persisting back; were flagged above to be deleted
                    lookupMap.RemoveAll(e => e.Value == -1);

                    // need to persist back the potentially updated mapping data
                    sqlRootNode.ExtensionData = lookupMap.ToJSON();
                    dc.SubmitChanges();
                }

                progress.Report(100);


            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }
    }
}
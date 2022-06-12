using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Website
{
    /// <summary>
    /// InsideRugs store implementation.
    /// </summary>
    [Export("InsideRugs", typeof(IWebStore))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InsideRugsWebStore : WebStoreBase<ProductDataCache>, IWebStore
    {
        private static List<ProductGroup> _supportedProductGroups = new List<ProductGroup>() { ProductGroup.Rug }; // if multiple, the primary must be listed first
        private ICategoryFilterManager _categoryFilterManager = null;

        private const string StoreFriendlyName = "Inside Rugs";
        private const string StoreDomainName = "insiderugs.com";
                
        public InsideRugsWebStore()
            : base(StoreKeys.InsideRugs)
        {
            // used for creating/maintaining the category tree under Filters top level parent.
            _categoryFilterManager = new RugsCategoryFilterManager(this);

            // tickler campaigns

            if (IsTicklerCampaignsEnabled)
                TicklerCampaignsManager = new TicklerCampaignsManager(this as IWebStore);
        }

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

        public override ICategoryFilterManager CategoryFilterManager
        {
            get
            {
                return _categoryFilterManager;
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
                    productFeedManager = new InsideRugsProductFeedManager(this);

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
            return RunActionForAllProducts<InsideRugsProduct>(actionName, tag);
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
            MaintenanceHub.NotifyRunStoreActionStatus("SQL Cleanup...");
            RunSQLCleanup();

            NotifyStoreActionProgress(20);

            bool bPerformModifications = true;
            MaintenanceHub.NotifyRunStoreActionStatus("Processing product images...");

            CleanupOrphanProductImages(bPerformModifications, cancelToken, (pct) =>
            {
                var adjPct = (int)Math.Round(pct * .8M, 0);

                NotifyStoreActionProgress(adjPct + 20);
            });

            if (cancelToken.IsCancellationRequested)
                return;

            NotifyStoreActionProgress(100);

            MaintenanceHub.NotifyRunStoreActionStatus("Processing collection images...");
            NotifyStoreActionProgress(20);


            CleanupOrphanCollectionImages(bPerformModifications, cancelToken, (pct) =>
            {
                var adjPct = (int)Math.Round(pct * .8M, 0);

                NotifyStoreActionProgress(adjPct + 20);
            });

            if (cancelToken.IsCancellationRequested)
                return;

            NotifyStoreActionProgress(100);


        }

        #endregion


        [RunStoreAction("RebuildProductLabelsTable")]
        public void RebuildProductLabelsTable(CancellationToken cancelToken)
        {

            NotifyStoreActionProgress(10);

            using (var dc = new AspStoreDataContext(ConnectionString))
            {
                NotifyStoreActionProgress(15);

                dc.ProductLabels.TruncateTable();

            }
            NotifyStoreActionProgress(25);

            PopulateTaxonomy();
            NotifyStoreActionProgress(100);
        }


        /// <summary>
        /// Global rebuild of color filters.
        /// </summary>
        /// <param name="cancelToken"></param>
        [RunStoreAction("RebuildColorFilters")]
        public void RebuildColorFilters(CancellationToken cancelToken)
        {
            MaintenanceHub.NotifyRunStoreActionStatus("Rebuilding Color Filters...");
            System.Threading.Thread.Sleep(2000);
            this.CategoryFilterManager.RebuildColorFilters(cancelToken, (pct) =>
            {
                NotifyStoreActionProgress(pct);
            }, (status) =>
            {
                MaintenanceHub.NotifyRunStoreActionStatus(status);
            });

            NotifyStoreActionProgress(100);
        }


        [RunStoreAction("RepopulateAutoSuggestTable")]
        public void RepopulateAutoSuggestTable(CancellationToken cancelToken)
        {
            // evaluates existing ExtensionData2 for all products and refigures SQL table autosuggestphrases.

            NotifyStoreActionProgress(10);

            using (var dc = new AspStoreDataContext(ConnectionString))
            {
                NotifyStoreActionProgress(50);

                PopulateAutoSuggestTable(dc);
            }

            NotifyStoreActionProgress(100);
        }






        /// <summary>
        /// Do all the work to rebuild the search data. Throw if error.
        /// </summary>
        /// <remarks>
        /// This is the main worker entry point for rugs when the "Rebuild Search Data" button is clicked.
        /// </remarks>
        protected override void RebuildProductSearchDataEx()
        {
            using (var dc = new AspStoreDataContext(ConnectionString))
            {
                // rebuild the product label taxonomy when not fully populated - takes 20 minutes or so on Peter's PC
                // used only for sleuthing - logic below will repop when found to be empty or nearly empty - so if want
                // new good data, just truncate that table and run a FTS index.

                if (dc.ProductLabels.Count() < 1000)
                {
                    dc.ProductLabels.TruncateTable();
                    PopulateTaxonomy();
                }

                // fill in Ext2 for each product - one line per keyword phrase
                PopulateProductKeywords();

                // fill in AutoSuggest table using mostly product keywords in Ext2
                PopulateAutoSuggestTable(dc);
            }

            // now that the preliminary processing is complete, rebuild the FTS catelog

            var scriptFilepath = MapPath(string.Format(@"~/App_Data/Sql/{0}RebuildSearchData.sql", StoreKey));

            // this is the SQL command to rebuild the FTS catalog

            //USE [InsideRugs]
            //ALTER FULLTEXT CATALOG [InsideRugsProducts]
            //REBUILD WITH ACCENT_SENSITIVITY = ON
            //GO

            // this operation returns immediately, even though the FTS rebuild occurs in the background within SQL server.
            RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        }


        private void PopulateTaxonomy()
        {
            var mgr = new ProductUpdateManager<InsideRugsProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel
                p.RebuildTaxonomy();

                return true;
            }, useParallelOperations: true);

        }

        /// <summary>
        /// Allow subclass to manually inject specific phrases.
        /// </summary>
        protected override List<string> InjectedAutoSuggestPhrases
        {
            get
            {
                return new List<string>()
                {
                    "pads",
                    "rug pads",
                };
            }
        }


        private void PopulateProductKeywords()
        {
            var mgr = new ProductUpdateManager<InsideRugsProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel

                p.MakeAndSaveExt2KeywordList();

                // Ext3 is used as a temporary bridge/crutch to keep FTS working to some reasonable
                // degree while we finalize Ext2 and autocomplete.
                p.MakeAndSaveExt3KeywordList();

                return true;
            }, useParallelOperations: true);
        }

        /// <summary>
        /// Spin through any queued up images to process.
        /// </summary>
        /// <remarks>
        /// Collects new images, saves, resizes, stores. Nearly identical to the original process
        /// performed by the original fabric updater project.
        /// </remarks>
        /// <param name="cancelToken"></param>
        [RunStoreAction("ProcessImageQueue")]
        public void ProcessImageQueue(CancellationToken cancelToken)
        {
            var mgr = new InsideRugsProductImageProcessor(this);

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
        /// The final step of RebuildAll() after base rebuilds ext data, categories, etc.
        /// </summary>
        /// <remarks>
        /// RebuildAll() is called after new products added/updated with product scanner.
        /// Cann also be invoked via the URL command line.
        /// </remarks>
        protected override void RebuildAllStoreSpecificTasks()
        {
            Debug.WriteLine("Begin SpinUpMissingDescriptions()");
            SpinUpMissingDescriptions(this.ConnectionString);
        }

        protected override AlgoliaProductRecord MakeAlgoliaProductRecord(int productID, AspStoreDataContext dc)
        {
            AlgoliaProductRecord record = null;
            ProductUpdateManager<InsideRugsProduct>.ProcessProduct(this, productID, (product) =>
            {
                record = product.MakeAlgoliaProductRecord();
            }, dc);

            return record;
        }


        protected void SpinUpMissingDescriptions(string connectionString)
        {
            var mgr = new ProductUpdateManager<InsideRugsProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel

                p.SpinUpMissingDescriptions();

                return true;
            }, useParallelOperations: true);

        }

#if false
        [RunStoreAction("RenumberSkus")]
        public void RenumberSkus(CancellationToken cancelToken)
        {

            using (var dc = new AspStoreDataContext(ConnectionString))
            {
                NotifyStoreActionProgress(0);

                var manufacturers = dc.Manufacturers.Where(e => e.Published == 1).ToList();

                int progressPct = 0;

                foreach(var m in manufacturers)
                {
                    if (m.ManufacturerID == 200)
                        continue;

                    NotifyStoreActionProgress(progressPct);

                    var products = (from p in dc.Products 
                                   join pm in dc.ProductManufacturers on p.ProductID equals pm.ProductID where pm.ManufacturerID == m.ManufacturerID
                                   select new
                                   {
                                       ProductID = p.ProductID,
                                       SKU = p.SKU,
                                   }).ToList();

                    MaintenanceHub.NotifyRunStoreActionStatus(string.Format("Updating {0} prodcuts for {1}...", products.Count(), m.Name));

                    int skuNumber = 101;

                    foreach(var product in products)
                    {
                        var skuPrefix = product.SKU.Substring(0, 3);
                        var newSKU = string.Format("{0}{1}", skuPrefix, skuNumber++);
                        dc.Products.UpdateProductSKU(product.ProductID, newSKU);
                    }


                    progressPct += 7;
                }
            }

            NotifyStoreActionProgress(100);
        }

#endif
    }
}
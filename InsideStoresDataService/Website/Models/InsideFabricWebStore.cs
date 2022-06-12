using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Website.Entities;
using Gen4.Util.Misc;
using System.Threading;
using System.Threading.Tasks;

namespace Website
{
    /// <summary>
    /// InsideFabric store implementation.
    /// </summary>
    [Export("InsideFabric", typeof(IWebStore))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InsideFabricWebStore : WebStoreBase<ProductDataCache>, IWebStore
    {
        // it's important that fabric be listed first (as the most important)
        private static List<ProductGroup> _supportedProductGroups = new List<ProductGroup>() { ProductGroup.Fabric, ProductGroup.Trim};

        private const string StoreFriendlyName = "Inside Fabric";
        private const string StoreDomainName = "insidefabric.com";
        private CancellationTokenSource ctsBackgroundTask = null;
        private bool isBackgroundTaskRunning;

        public InsideFabricWebStore()
            : base(StoreKeys.InsideFabric)
        {
            if (this.RunBackgroundTask)
            {
                // inject temporary task to rebuild all the resized images in the background

                Task.Factory.StartNew(async () =>
                {
                    // will compute image features (CEDD, colors, etc.)

                    try
                    {
                        isBackgroundTaskRunning = true;
                        ctsBackgroundTask = new CancellationTokenSource();

                        // startup delay, just to let the system deal with more important things first
                        await Task.Delay(TimeSpan.FromSeconds(60));

                        var mgr = new ProductUpdateManager<InsideFabricProduct>(this);

                        // not run as a parallel operation

                        var rnd = new Random();

                        mgr.Run((p) =>
                        {
                            try
                            {
                                p.ReCreateImageFeatures();

                                // introduce a bit of a random delay so we don't hog resources

                                Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(50, 150)), ctsBackgroundTask.Token).Wait();
                            }
                            catch { }

                            return true;

                        }, ctsBackgroundTask.Token, false, null, "looks:1");
                    }
                    catch { }
                    finally
                    {
                        isBackgroundTaskRunning = false;
                    }

                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }


            // tickler campaigns

            if (IsTicklerCampaignsEnabled)
                TicklerCampaignsManager = new TicklerCampaignsManager(this as IWebStore);

        }

        /// <summary>
        /// Cancel any possibly running background task.
        /// </summary>
        /// <remarks>
        /// Typically called when shutting down.
        /// </remarks>
        public override void CancelBackgroundTask()
        {
            if (ctsBackgroundTask != null)
                ctsBackgroundTask.Cancel();
        }

        public override bool IsBackgroundTaskRunning
        {
            get
            {
                return isBackgroundTaskRunning;
            }
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
                    productFeedManager = new InsideFabricProductFeedManager(this);

                return productFeedManager;
            }
        }

        #region Run Action

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
            return RunActionForAllProducts<InsideFabricProduct>(actionName, tag);
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
            MaintenanceHub.NotifyRunStoreActionStatus("SQL Cleanup...");            

            NotifyStoreActionProgress(10);
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
            var mgr = new InsideFabricProductImageProcessor(this);

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

        #endregion

        /// <summary>
        /// The final step of RebuildAll() after base rebuilds ext data, categories, etc.
        /// </summary>
        /// <remarks>
        /// RebuildAll() is called after new products added/updated with fabric updater.
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
            ProductUpdateManager<InsideFabricProduct>.ProcessProduct(this, productID, (product) =>
            {
                record = product.MakeAlgoliaProductRecord();
            }, dc);

            return record;
        }


        protected void SpinUpMissingDescriptions(string connectionString)
        {
            var mgr = new ProductUpdateManager<InsideFabricProduct>(this);

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
        /// This is the main worker entry point for fabric when the "Rebuild Search Data" button is clicked.
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
                PopulateProductKeywords(ConnectionString);

                // fill in AutoSuggest table using mostly product keywords in Ext2
                PopulateAutoSuggestTable(dc);
            }

            // now that the preliminary processing is complete, rebuild the FTS catelog

            var scriptFilepath = MapPath(string.Format(@"~/App_Data/Sql/{0}RebuildSearchData.sql", StoreKey));

            // this is the SQL command to rebuild the FTS catalog

            //USE [InsideFabric]
            //ALTER FULLTEXT CATALOG [InsideFabricProducts]
            //REBUILD WITH ACCENT_SENSITIVITY = ON
            //GO

            // this operation returns immediately, even though the FTS rebuild occurs in the background within SQL server.
            RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        }

        private void PopulateTaxonomy()
        {
            var mgr = new ProductUpdateManager<InsideFabricProduct>(this);

            mgr.Run((p) =>
            {
                // for each product, perform this logic, return true if update to data was made
                // must be thread safe - runs in parallel
                p.RebuildTaxonomy();

                return true;
            }, useParallelOperations:true);

        }



        private void PopulateProductKeywords(string connectionString)
        {
            InsideStoresCategories CategoryKeywordMgr = null;

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                var categories = dc.Categories.ToList();
                CategoryKeywordMgr = new InsideStoresCategories(categories);
            }

            var mgr = new ProductUpdateManager<InsideFabricProduct>(this);

            mgr.Run((p) =>
                {
                    // for each product, perform this logic, return true if update to data was made
                    // must be thread safe - runs in parallel

                    p.MakeAndSaveExt2KeywordList(CategoryKeywordMgr);

                    // Ext3 is used as a temporary bridge/crutch to keep FTS working to some reasonable
                    // degree while we finalize Ext2 and autocomplete.
                    p.MakeAndSaveExt3KeywordList(CategoryKeywordMgr);

                    return true;
                }, useParallelOperations: true);
        }



        protected override void UpdateProductLabels(CancellationToken cancelToken, Action<int> reportProgress = null)
        {
            // called as part of updating collection tables to ensure all new patterns and collections are included.

            List<int> productList = null;


            // find all productID for products already in ProductLabels
            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                productList = dc.Products.Where(e => !dc.ProductLabels.Select(pl => pl.ProductID).Distinct().Contains(e.ProductID) && e.Deleted == 0 && e.Published == 1).Select(e => e.ProductID).ToList();
            }

            Debug.WriteLine(string.Format("Products adding to ProductLabels table: {0:N0}", productList.Count()));
            var mgr = new ProductUpdateManager<InsideFabricProduct>(this);

            mgr.progressCallback = reportProgress;

            mgr.Run((p) =>
            {
                p.RebuildTaxonomy();

                return true;
            }, cancelToken, true, productList);
        }

#if false
        /// <summary>
        /// This is the main workhorse for generating cross links.
        /// </summary>
        /// <remarks>
        /// Can be called directly from query command line, or in batch mode as part of a larger workflow.
        /// </remarks>
        /// <param name="progressCallback"></param>
        /// <param name="cancelToken"></param>
        protected void GenerateCrossLinks(IProgress<int> progressCallback, CancellationToken cancelToken)
        {
            const int RequiredInboundLinks = 20;

            var mgr = new ProductCrossLinksManager<InsideFabricProduct>(this);
            mgr.Run(RequiredInboundLinks, progressCallback, cancelToken);
        }

         /// <summary>
        /// Generate cross links for any product page not yet having the standard number of links.
        /// </summary>
        /// <remarks>
        /// This is the command line proxy to the internal method that does the real work.
        /// </remarks>
        /// <param name="cancelToken"></param>
        [RunStoreAction("GenerateCrossLinks")]
        public void GenerateCrossLinks(CancellationToken cancelToken)
        {
            var progress = new Progress<int>((pct) =>
            {
                NotifyStoreActionProgress(pct);
            });

            GenerateCrossLinks(progress, cancelToken);
        }

#endif

    }
}
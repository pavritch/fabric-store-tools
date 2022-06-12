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
    /// InsideWallpaper store implementation.
    /// </summary>
    [Export("InsideWallpaper", typeof(IWebStore))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InsideWallpaperWebStore : WebStoreBase<ProductDataCache>, IWebStore
    {
        private static List<ProductGroup> _supportedProductGroups = new List<ProductGroup>() { ProductGroup.Wallcovering }; // if multiple, the primary must be listed first

        private const string StoreFriendlyName = "Inside Wallpaper";
        private const string StoreDomainName = "insidewallpaper.com";

        public InsideWallpaperWebStore()
            : base(StoreKeys.InsideWallpaper)
        {
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
                    productFeedManager = new InsideWallpaperProductFeedManager(this);

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
            return RunActionForAllProducts<InsideWallpaperProduct>(actionName, tag);
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
            ProductUpdateManager<InsideWallpaperProduct>.ProcessProduct(this, productID, (product) =>
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
        /// This is the main worker entry point for when the "Rebuild Search Data" button is clicked.
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

            //USE [InsideWallpaper]
            //ALTER FULLTEXT CATALOG [InsideWallpaperProducts]
            //REBUILD WITH ACCENT_SENSITIVITY = ON
            //GO

            // this operation returns immediately, even though the FTS rebuild occurs in the background within SQL server.
            RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        }

        private void PopulateTaxonomy()
        {
            var mgr = new ProductUpdateManager<InsideWallpaperProduct>(this);

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

            var mgr = new ProductUpdateManager<InsideWallpaperProduct>(this);

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

            var mgr = new ProductUpdateManager<InsideWallpaperProduct>(this);

            mgr.progressCallback = reportProgress;

            mgr.Run((p) =>
            {
                p.RebuildTaxonomy();

                return true;
            }, cancelToken, true, productList);
        }


        [RunStoreAction("SyncProductsFromInsideFabric")]
        public void SyncProductsFromInsideFabric(CancellationToken cancelToken)
        {
            var ifStore = MvcApplication.Current.GetWebStore(StoreKeys.InsideFabric);
            var mgr = new InsideWallpaperProductSync(ifStore.ConnectionString, this.ConnectionString, ifStore.PathWebsiteRoot, this.PathWebsiteRoot, this.ImageFolderNames, this.OutletCategoryID);

            mgr.RunFullSyncAll(cancelToken, (pct) =>
            {
                // progress bar
                NotifyStoreActionProgress(pct);
            }, (msg) =>
            {
                // status message
                MaintenanceHub.NotifyRunStoreActionStatus(msg);
            });

            //if (!cancelToken.IsCancellationRequested)
            //    RebuildAll();

            // not included here is updating the product collections - which can be done manually.
        }

        [RunStoreAction("SyncProductsFromInsideFabricSingleVendor")]
        public void SyncProductsFromInsideFabricSingleVendor(string tag, CancellationToken cancelToken)
        {
            // tag can be manufacaturerID or SKU prefix

            var ifStore = MvcApplication.Current.GetWebStore(StoreKeys.InsideFabric);
            var mgr = new InsideWallpaperProductSync(ifStore.ConnectionString, this.ConnectionString, ifStore.PathWebsiteRoot, this.PathWebsiteRoot, this.ImageFolderNames, this.OutletCategoryID);

            int manufacturerID = 0;

            if (string.IsNullOrWhiteSpace(tag))
                throw new Exception("Missing tag parameter.");

            if (tag.IsNumeric())
            {
                if (!int.TryParse(tag, out manufacturerID) || manufacturerID < 1)
                    throw new Exception("Invalid tag parameter. Must be valid manufacturerID when numberic.");

                using (var dc = new AspStoreDataContext(ConnectionString))
                {
                    var count = dc.Manufacturers.Where(e => e.ManufacturerID == manufacturerID).Count();
                    if (count != 1)
                        throw new Exception("Invalid tag parameter. Not a known manufacturerID.");
                }

            }
            else if (tag.Count() == 2)
            {
                // is SKU prefix, try to find manufacturer
                using (var dc = new AspStoreDataContext(ConnectionString))
                {
                    var productID = dc.Products.Where(e => e.SKU.StartsWith(tag + '-')).Select(e => e.ProductID).FirstOrDefault();
                    if (productID == 0)
                        throw new Exception("Invalid tag parameter.");

                    var mID = dc.ProductManufacturers.Where(e => e.ProductID == productID).Select(e => e.ManufacturerID).FirstOrDefault();
                    if (mID == 0)
                        throw new Exception("Invalid tag parameter.");

                    manufacturerID = mID;
                }
            }
            else
                    throw new Exception("Invalid tag parameter.");

            mgr.RunFullSyncSingleVendor(manufacturerID, cancelToken, (pct) =>
            {
                // progress bar
                NotifyStoreActionProgress(pct);
            }, (msg) =>
            {
                // status message
                MaintenanceHub.NotifyRunStoreActionStatus(msg);
            });

            //if (!cancelToken.IsCancellationRequested)
            //    RebuildAll();

            // not included here is updating the product collections - which can be done manually.
        }


        [RunStoreAction("SyncProductsFromInsideFabricInsert")]
        public void SyncProductsFromInsideFabricInsert(CancellationToken cancelToken)
        {
            var ifStore = MvcApplication.Current.GetWebStore(StoreKeys.InsideFabric);
            var mgr = new InsideWallpaperProductSync(ifStore.ConnectionString, this.ConnectionString, ifStore.PathWebsiteRoot, this.PathWebsiteRoot, this.ImageFolderNames, this.OutletCategoryID);

            mgr.RunInsert(cancelToken, (pct) =>
            {
                // progress bar
                NotifyStoreActionProgress(pct);
            }, (msg) =>
            {
                // status message
                MaintenanceHub.NotifyRunStoreActionStatus(msg);
            });

            //if (!cancelToken.IsCancellationRequested)
            //    RebuildAll();

            // not included here is updating the product collections - which can be done manually.
        }




        /// <summary>
        /// Quick sync updates only items already in wallpaper store which have recently been updated in fabric store.
        /// </summary>
        /// <remarks>
        /// Only deals with cost, price, msrp, sale price, in/out, discontinued.
        /// </remarks>
        /// <param name="cancelToken"></param>
        [RunStoreAction("SyncProductsFromInsideFabricQuick")]
        public void SyncProductsFromInsideFabricQuick(CancellationToken cancelToken)
        {
            var ifStore = MvcApplication.Current.GetWebStore(StoreKeys.InsideFabric);
            var mgr = new InsideWallpaperProductSync(ifStore.ConnectionString, this.ConnectionString, ifStore.PathWebsiteRoot, this.PathWebsiteRoot, this.ImageFolderNames, this.OutletCategoryID);

            mgr.RunQuick(cancelToken, (pct) =>
            {
                // progress bar
                NotifyStoreActionProgress(pct);
            }, (msg) =>
            {
                // status message
                MaintenanceHub.NotifyRunStoreActionStatus(msg);
            });
        }

    }

}
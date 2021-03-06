using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using InsideFabric.Data;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Core interface which must be implemented by each supported store.
    /// </summary>
    public interface IWebStore
    {
        StoreKeys StoreKey { get; }
        string FriendlyName { get; }
        string Domain { get; }
        string PathWebsiteRoot { get; }
        string ConnectionString { get; }
        string UploadsFolder { get; }
        bool IsGeneratingProductFeeds { get; }
        bool IsPopulated { get; }
        bool IsPopulatingProductCache { get;  }
        bool IsRebuildingCategories { get; }
        bool EnableImageDomains { get; }
        bool ImageDomainsUseSSL { get; }

        bool IsImageSearchEnabled { get; }
        bool IsFilteredSearchEnabled { get; }
        bool IsCorrelatedProductsEnabled { get; }

        // default widths for product images
        int ProductImageWidthMicro { get; }
        int ProductImageWidthMini { get; }
        int ProductImageWidthIcon { get; }
        int ProductImageWidthSmall { get; }
        int ProductImageWidthMedium { get; }
        int ProductImageWidthLarge { get; }

        // runs a defined background task - usually hard coded, enabled via web.config
        bool RunBackgroundTask { get; }
        bool IsBackgroundTaskRunning { get; }
        bool IsTicklerCampaignsEnabled { get; }
        bool IsTicklerCampaignsTimerEnabled { get; }
        bool UseImageDownloadCache { get; }
        string ImageDownloadCacheFolder { get; }

        object ExclusiveDatabaseLockObject { get; }
        IProductDataCache ProductData { get; }
        PerformanceMonitor Performance { get; }
        DateTime? TimeWhenPopulationCompleted { get; }
        TimeSpan? TimeToPopulate { get; }

        /// <summary>
        /// The common reqex we use to validate email addresses.
        /// </summary>
        string EmailRegEx { get; }

        /// <summary>
        /// Manages all product feeds generated by the store.
        /// </summary>
        /// <remarks>
        /// Google, Amazone, TheFind, etc.
        /// </remarks>
        IProductFeedManager ProductFeedManager { get; }
        IStockCheckManager StockCheckManager { get; }

        ITicklerCampaignsManager TicklerCampaignsManager { get; }

        /// <summary>
        /// Cancel any possibly running background task.
        /// </summary>
        /// <remarks>
        /// Typically called when shutting down.
        /// </remarks>
        void CancelBackgroundTask();

        void SubmitProductQuery(IQueryRequest productQuery);

        /// <summary>
        /// Autosuggest for top of page.
        /// </summary>
        /// <param name="query"></param>
        void SubmitAutoSuggestQuery(AutoSuggestQuery query);

        /// <summary>
        /// Autosuggest for filtered collection lists.
        /// </summary>
        /// <param name="query"></param>
        void SubmitAutoSuggestQuery(CollectionsAutoSuggestQuery query);

        /// <summary>
        /// Autosuggest for filtered product collections.
        /// </summary>
        /// <param name="query"></param>
        void SubmitAutoSuggestQuery(ProductCollectionAutoSuggestQuery query);

        /// <summary>
        /// Autosuggest for new products.
        /// </summary>
        /// <param name="query"></param>
        void SubmitAutoSuggestQuery(NewProductsByManufacturerAutoSuggestQuery query);

        /// <summary>
        /// Repopulate product data into memory cache.
        /// </summary>
        /// <remarks>
        /// This is a synchronous operation. If there is existing data, it will be replaced
        /// by the new data only after the data object(s) are complete.
        /// </remarks>
        /// <returns>true if successful.</returns>
        bool RepopulateProducts(int millisecondsTimeout = 0);

        /// <summary>
        /// Rebuild the SQL ProductCategory table which associates the categories for each product.
        /// </summary>
        /// <remarks>
        /// This is a synchronous operation.
        /// </remarks>
        /// <returns>true if successful.</returns>
        bool RebuildProductCategoryTable(int millisecondsTimeout = 0);

        /// <summary>
        /// Rebuild the extension data column in SQL Product table to facilitate full text search.
        /// Also populates auto-suggest table.
        /// </summary>
        /// <remarks>
        /// Builds up text never seen by users to give the FTS engine something good to chew on.
        /// This is a synchronous operation.
        /// </remarks>
        /// <returns>true if successful.</returns>
        bool RebuildProductSearchExtensionData(int millisecondsTimeout = 0);

        /// <summary>
        /// Get the weighting value to apply to the named manufacturer.
        /// </summary>
        /// <remarks>
        /// The range does not matter - more so what matters is the relative value
        /// in comparison to peers. A weight of 200 will have products display
        /// twice as often in a list against another of 100. The weights of all 
        /// manufacturers are summed and used to figure out a combined display ratio.
        /// The purpose of all this is to tweak the display output of product listing pages.
        /// </remarks>
        /// <param name="ManufacturerID"></param>
        /// <returns></returns>
        int GetManufacturerDisplayWeight(int ManufacturerID);

        /// <summary>
        /// Initiate a complete sweep of rebuilding/repopulating.
        /// </summary>
        /// <remarks>
        /// Only one can run at a time. 
        /// </remarks>
        bool RebuildAll();

        /// <summary>
        /// Run an internally designated maint action for all products.
        /// </summary>
        /// <remarks>
        /// The action needs to be hard coded.
        /// </remarks>
        /// <returns>Message to display on website.</returns>
        string RunActionForAllProducts(string actionName, string tag);

        /// <summary>
        /// Run an internally designated maint action for the store.
        /// </summary>
        /// <remarks>
        /// The action needs to be hard coded.
        /// </remarks>
        /// <returns>Message to display on website.</returns>
        string RunActionForStore(string actionName, string tag);

        /// <summary>
        /// Cancel any running product action.
        /// </summary>
        void CancelRunActionForAllProducts();

        /// <summary>
        /// Cancel any running store action.
        /// </summary>
        void CancelRunActionForStore();

        /// <summary>
        /// Manage email subscriber list.
        /// </summary>
        /// <param name="listKey">Target subscriber list - must be known to store.</param>
        /// <param name="action">Verb - only Add supported</param>
        /// <param name="email">Email address to be managed</param>
        /// <returns></returns>
        Task<bool> Subscribers(string listKey, CampaignSubscriberListAction action, string email);

        /// <summary>
        /// List of folders this store knows about for images.
        /// </summary>
        /// <remarks>
        /// Traditionally, Icon, Medium, Large. Use this list anywhere in logic that loops through.
        /// Intended to use with Path.Combine() to form full path. This is just the final leaf.
        /// </remarks>
        List<string> ImageFolderNames { get; }

        /// <summary>
        /// Return information about any currently running maintenance operations
        /// </summary>
        /// <returns></returns>
        WebStoreMaintenanceStatus GetStoreMaintenanceStatus();

        /// <summary>
        /// Send an email created using the Postal template engine.
        /// </summary>
        /// <remarks>
        /// Uses whatever system level sending channel is defined - such as SendGrid.
        /// </remarks>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> SendEmail(EmailTemplate emailTemplate);

        /// <summary>
        /// Returns a list of the supported product groups for this store.
        /// </summary>
        /// <remarks>
        /// Many operations will filter results to return only products within these listed groups.
        /// </remarks>
        List<ProductGroup> SupportedProductGroups { get; }

        /// <summary>
        /// When true (IW, IF, but not IA) - indicates store supports tracking inventory.
        /// </summary>
        /// <remarks>
        /// IA does not support this feature; most other stores do.
        /// </remarks>
        bool HasAutomatedInventoryTracking { get; }

        /// <summary>
        /// Common acessor for caregory filter features (maintenance tasks only, not web visitors).
        /// </summary>
        ICategoryFilterManager CategoryFilterManager { get; }

        /// <summary>
        /// Low level features to search CEDD and dominant colors.
        /// </summary>
        IImageSearch ImageSearch {get;}

        /// <summary>
        /// Return the full name of the manufacturer for an id.
        /// </summary>
        /// <param name="manufacturerID"></param>
        /// <returns></returns>
        string LookupManufacturerName(int manufacturerID);

        /// <summary>
        /// The ASPDNSF categoryID we use for outlet/clearance.
        /// </summary>
        /// <remarks>
        /// Specified in web.config. Not always 151.
        /// </remarks>
        int OutletCategoryID { get; }

        /// <summary>
        /// How many minutes to wait until sending customer an email for cart abandonment.
        /// </summary>
        int CartAbandonmentEmailNotificationDelayMinutes { get; }

        /// <summary>
        /// Indicates if queue processing is paused (wakeup), but if so, population can continue.
        /// </summary>
        bool IsTicklerCampaignQueueProcessingPaused { get; }

        /// <summary>
        /// In-memory pause of queue processing (wakeup).
        /// </summary>
        void PauseTicklerCampaignProcessing();

        /// <summary>
        /// In-memory resume of queue processing (wakeup).
        /// </summary>
        void ResumeTicklerCampaignProcessing();

        /// <summary>
        /// Transition any staged campaigns to running.
        /// </summary>
        void MoveStagedTicklerCampaignsToRunning();

        /// <summary>
        /// Delete all rows marked as staged.
        /// </summary>
        void DeleteStagedTicklerCampaigns();

        void SuspendRunningTicklerCampaigns();

        void ResumeSuspendedTicklerCampaigns();

        /// <summary>
        /// Is email address on global suppression list
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool IsEmailSuppressed(string email);

        /// <summary>
        /// Notification from website that a new order has been received.
        /// </summary>
        /// <remarks>
        /// Up to individual stores to decide how to process.
        /// </remarks>
        /// <param name="orderNumber"></param>
        void NewOrderNotification(int orderNumber);

        /// <summary>
        /// Location where search match photos uploaded by users are stored.
        /// </summary>
        string UploadedPhotosFolder { get; }

        byte[] ResizeUploadedPhotoAsSquare(string guid, int size);
        
         // algolia
        string AlgoliaApplicationID {get;}
        string AlgoliaSearchOnlyApiKey {get;}
        string AlgoliaMonitorApiKey {get;}
        string AlgoliaAdminApiKey { get; }
        bool IsAlgoliaEnabled { get; }

        void LogFacetSearch(FacetSearchCriteria criteria, int page, string visitorID);

        SearchGalleryManager SearchGalleryManager { get; }

        int FacetSearchProductsResultCount(FacetSearchCriteria searchCriteria);
        List<int> FacetSearchProductsResultSet(FacetSearchCriteria searchCriteria);

        List<ProductCountsByManufacturerMetric> GetProductCountsByManufacturerMetrics();
    }
}
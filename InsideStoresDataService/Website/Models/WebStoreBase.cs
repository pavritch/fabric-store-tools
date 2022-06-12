using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Algolia.Search;
using Gen4.Util.Misc;
using InsideFabric.Data;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json.Linq;
using RestSharp;
using Website.Emails;
using Website.Entities;

namespace Website
{
  /// <summary>
  /// Common base class used for all web stores.
  /// </summary>
  public abstract partial class WebStoreBase<TProductDataCache> where TProductDataCache : class, IProductDataCache, new()
  {

    #region ProductQueryWorkItem

    private class QueryRequestWorkItem
    {
      public Amib.Threading.IWorkItemResult<bool> workResult { get; set; }
      public IQueryRequest QueryRequest { get; set; }
      public QueryRequestWorkItem(IQueryRequest query)
      {
        QueryRequest = query;
      }
    }

    #endregion

    #region static data

    private static HashSet<string> emailSupressionList;
    private static bool EnableSearchCaching { get; set; }

    #endregion

    #region Constants

    private const string _emailRegEx = @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$";

    #endregion

    #region Locals

    private StoreKeys storeKey;
    private string connectionString = null;
    private string pathWebsiteRoot = null;
    private bool isPopulated = false;
    private bool isRunBackgroundTask = false;
    private bool useImageDownloadCache = false;
    private int outletCategoryID;
    private string imageDownloadCacheFolder;
    private FacetSearchLogger facetSearchLogger;
    private bool isTicklerCampaignsEnabled = false;
    private bool isTicklerCampaignsTimerEnabled = false;
    private bool isImageSearchEnabled = false;
    private bool isFilteredSearchEnabled = false;
    private bool isCorrelatedProductsEnabled = false;
    private ImageSearch imageSearch = null;
    private SearchGalleryManager searchGalleryManager;

    private Amib.Threading.SmartThreadPool threadPool;
    private object lockObject = new object();
    private object exclusiveDatabaseLockObject = new object();
    private int rebuildAllQueueCount = 0;
    public bool EnableImageDomains { get; private set; }
    public bool ImageDomainsUseSSL { get; private set; }
    public int CartAbandonmentEmailNotificationDelayMinutes { get; private set; }
    public string UploadedPhotosFolder { get; private set; }

    public string AlgoliaApplicationID { get; private set; }
    public string AlgoliaSearchOnlyApiKey { get; private set; }
    public string AlgoliaMonitorApiKey { get; private set; }
    public string AlgoliaAdminApiKey { get; private set; }
    public bool IsAlgoliaEnabled { get; private set; }

    /// <summary>
    /// This is the main data store, which operates as an ASP.NET memory cache.
    /// </summary>
    protected ProductDataService<TProductDataCache> productData { get; private set; }

    protected IProductFeedManager productFeedManager = null;

    protected bool isProductActionRunning = false;
    protected CancellationTokenSource productActionCancelTokenSource = null;
    protected string productActionName = null;
    protected DateTime? productActionActionTimeStarted = null;
    protected int productActionPercentComplete = 0;


    protected int storeActionPercentComplete = 0;
    protected bool isStoreActionRunning = false;
    protected string storeActionName = null;
    protected DateTime? storeActionTimeStarted = null;
    protected CancellationTokenSource storeActionCancelTokenSource = null;


    protected string uploadsFolder = null;


    private int productImageWidthMicro;
    private int productImageWidthMini;
    private int productImageWidthIcon;
    private int productImageWidthSmall;
    private int productImageWidthMedium;
    private int productImageWidthLarge;

    #endregion

    #region Ctor



    static WebStoreBase()
    {
      EnableSearchCaching = bool.Parse(ConfigurationManager.AppSettings["EnableSearchCaching"]);
      LoadEmailSupressionList();
    }


    public WebStoreBase(StoreKeys storeKey)
    {
      this.storeKey = storeKey;
      Performance = new PerformanceMonitor(this as IWebStore);

      // the connection string and path use convention-based keys in web.config

      connectionString = WebConfigurationManager.ConnectionStrings[string.Format("{0}ConnectionString", storeKey.ToString())].ConnectionString;

      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}RunBackgroundTask", storeKey.ToString())], out isRunBackgroundTask);
      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}UseImageDownloadCache", storeKey.ToString())], out useImageDownloadCache);

      imageDownloadCacheFolder = WebConfigurationManager.AppSettings[string.Format("{0}ImageDownloadCacheFolder", storeKey.ToString())];

      // algolia - presently the same for all stores
      AlgoliaApplicationID = WebConfigurationManager.AppSettings["AlgoliaApplicationID"];
      AlgoliaSearchOnlyApiKey = WebConfigurationManager.AppSettings["AlgoliaSearchOnlyApiKey"];
      AlgoliaMonitorApiKey = WebConfigurationManager.AppSettings["AlgoliaMonitorApiKey"];
      AlgoliaAdminApiKey = WebConfigurationManager.AppSettings["AlgoliaAdminApiKey"];

      bool isAlgoliaEnabled = false;
      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}EnableAlgolia", storeKey.ToString())], out isAlgoliaEnabled);
      IsAlgoliaEnabled = isAlgoliaEnabled;

      facetSearchLogger = new FacetSearchLogger(this as IWebStore, WebConfigurationManager.AppSettings["FacetSearchLogsRootPath"]);

      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}EnableImageSearch", storeKey.ToString())], out isImageSearchEnabled);
      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}EnableFilteredSearch", storeKey.ToString())], out isFilteredSearchEnabled);
      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}EnableCorrelatedProducts", storeKey.ToString())], out isCorrelatedProductsEnabled);
      bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}EnableTicklerCampaigns", storeKey.ToString())], out isTicklerCampaignsEnabled);

      // shared setting by all stores. Determines if timer to wake up and send emails will trigger.
      bool.TryParse(WebConfigurationManager.AppSettings["TicklerCampaignsMasterTimerEnabled"], out isTicklerCampaignsTimerEnabled);


      // how long to wait before sending email about an abandoned cart - common to all stores.
      int delayMinutes = 60 * 24;
      int.TryParse(ConfigurationManager.AppSettings["CartAbandonmentEmailNotificationDelayMinutes"], out delayMinutes);
      CartAbandonmentEmailNotificationDelayMinutes = delayMinutes;

      int.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}OutletCategoryID", storeKey.ToString())], out outletCategoryID);


      StockCheckManager = new StockCheckManager(this as IWebStore); // needs connection string to be good

      pathWebsiteRoot = WebConfigurationManager.AppSettings[string.Format("{0}WebsiteRootPath", storeKey.ToString())];

      UploadedPhotosFolder = WebConfigurationManager.AppSettings[string.Format("{0}UploadedPhotosFolder", storeKey.ToString())];

      uploadsFolder = WebConfigurationManager.AppSettings["FileUploadsFolder"]; // common for all

      bool enableImageDomains = false;
      bool.TryParse(ConfigurationManager.AppSettings[string.Format("{0}EnableImageDomains", storeKey.ToString())] ?? "false", out enableImageDomains);
      EnableImageDomains = enableImageDomains;

      bool imageDomainsUseSSL = false;
      bool.TryParse(ConfigurationManager.AppSettings[string.Format("{0}ImageDomainsUseSSL", storeKey.ToString())] ?? "false", out imageDomainsUseSSL);
      ImageDomainsUseSSL = imageDomainsUseSSL;

      var productImageWidths = WebConfigurationManager.AppSettings[string.Format("{0}ProductImageWidths", storeKey.ToString())];

      if (string.IsNullOrWhiteSpace(productImageWidths))
        productImageWidths = "50,100,150,225,350,800";

      var ary = productImageWidths.Replace(" ", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      productImageWidthMicro = int.Parse(ary[0]);
      productImageWidthMini = int.Parse(ary[1]);
      productImageWidthIcon = int.Parse(ary[2]);
      productImageWidthSmall = int.Parse(ary[3]);
      productImageWidthMedium = int.Parse(ary[4]);
      productImageWidthLarge = int.Parse(ary[5]);

      // make sure all folder names for images exist

      foreach (var folderName in ImageFolderNames)
      {
        var folderPath = Path.Combine(PathWebsiteRoot, "images\\product", folderName);
        if (!Directory.Exists(folderPath))
          Directory.CreateDirectory(folderPath);
      }

      // folder which stores thumbs for collections/books/patterns

      {
        var folderPath = Path.Combine(PathWebsiteRoot, "images\\collections");
        if (!Directory.Exists(folderPath))
          Directory.CreateDirectory(folderPath);
      }

      // optional cache folder

      if (useImageDownloadCache)
      {
        // make sure exists one, else create

        if (string.IsNullOrWhiteSpace(imageDownloadCacheFolder))
          imageDownloadCacheFolder = Path.Combine(PathWebsiteRoot, "images\\product", "Cache");

        if (!Directory.Exists(imageDownloadCacheFolder))
          Directory.CreateDirectory(imageDownloadCacheFolder);
      }

      // CEDD and dominant color search manager
      imageSearch = new ImageSearch(this as IWebStore);

      var poolInfo = new Amib.Threading.STPStartInfo()
      {
        MinWorkerThreads = 30,
        MaxWorkerThreads = 100,
        IdleTimeout = 3 * 60 * 1000,
      };

      threadPool = new Amib.Threading.SmartThreadPool(poolInfo);

      switch (storeKey)
      {
        case StoreKeys.InsideAvenue:
          searchGalleryManager = new InsideAvenueSearchGalleryManager(this as IWebStore);
          break;

        case StoreKeys.InsideRugs:
          searchGalleryManager = new InsideRugsSearchGalleryManager(this as IWebStore);
          break;

        case StoreKeys.InsideFabric:
          searchGalleryManager = new InsideFabricSearchGalleryManager(this as IWebStore);
          break;

        case StoreKeys.InsideWallpaper:
          searchGalleryManager = new InsideWallpaperSearchGalleryManager(this as IWebStore);
          break;

      }

    }

    #endregion

    #region static methods

    /// <summary>
    /// Load in global list of suppressed emails (known bounces, spam blocks, etc.)
    /// </summary>
    /// <remarks>
    /// Called by static ctor.
    /// </remarks>
    private static void LoadEmailSupressionList()
    {
      try
      {
        emailSupressionList = new HashSet<string>();
        var filepath = MapPath(@"~/App_Data/Misc/email-suppression-list.txt");

        var list = File.ReadAllLines(filepath);
        foreach (var email in list)
        {
          if (!string.IsNullOrWhiteSpace(email))
            emailSupressionList.Add(email.ToLower());
        }

      }
      catch { }
    }

    #endregion

    #region Properties

    public SearchGalleryManager SearchGalleryManager
    {
      get
      {
        return searchGalleryManager;
      }
    }

    public bool RunBackgroundTask
    {
      get
      {
        return isRunBackgroundTask;
      }
    }

    public ITicklerCampaignsManager TicklerCampaignsManager { get; protected set; }

    /// <summary>
    /// Indicates if queue processing is paused (wakeup), but if so, population can continue.
    /// </summary>
    /// <remarks>
    /// On startup, default to not paused. Pause/resume takes place via web pages.
    /// </remarks>
    public bool IsTicklerCampaignQueueProcessingPaused { get; protected set; }

    /// <summary>
    /// The ASPDNSF categoryID we use for outlet/clearance.
    /// </summary>
    /// <remarks>
    /// Specified in web.config. Not always 151.
    /// </remarks>
    public int OutletCategoryID
    {
      get
      {
        return outletCategoryID;
      }
    }

    public IImageSearch ImageSearch
    {
      get
      {
        return imageSearch;
      }
    }

    public bool IsImageSearchEnabled
    {
      get
      {
        return isImageSearchEnabled;
      }
    }

    public bool IsTicklerCampaignsEnabled
    {
      get
      {
        return isTicklerCampaignsEnabled;
      }
    }

    public bool IsTicklerCampaignsTimerEnabled
    {
      get
      {
        return isTicklerCampaignsTimerEnabled;
      }
    }
    public bool IsFilteredSearchEnabled
    {
      get
      {
        return isFilteredSearchEnabled;
      }
    }

    public bool IsCorrelatedProductsEnabled
    {
      get
      {
        return isCorrelatedProductsEnabled;
      }
    }


    /// <summary>
    /// Returns a list of the supported product groups for this store.
    /// </summary>
    /// <remarks>
    /// Many operations will filter results to return only products within these listed groups.
    /// </remarks>
    public virtual List<ProductGroup> SupportedProductGroups
    {
      get
      {
        return new List<ProductGroup>();
      }
    }

    public virtual ICategoryFilterManager CategoryFilterManager
    {
      get
      {
        // base implementation returns null, since related features only supported by some stores.
        return null;
      }
    }

    public string EmailRegEx
    {
      get
      {
        return _emailRegEx;
      }
    }

    public string UploadsFolder
    {
      get
      {
        return uploadsFolder;
      }

    }

    /// <summary>
    /// For development. Downloaded image files are saved to a Cache folder.
    /// </summary>
    /// <remarks>
    /// Configured via web.config - one setting for all stores.
    /// </remarks>
    public bool UseImageDownloadCache
    {
      get
      {
        return useImageDownloadCache;
      }
    }

    public string ImageDownloadCacheFolder
    {
      get
      {
        return imageDownloadCacheFolder;
      }
    }


    public int ProductImageWidthIcon
    {
      get
      {
        return productImageWidthIcon;
      }
    }

    public int ProductImageWidthMedium
    {
      get
      {
        return productImageWidthMedium;
      }
    }

    public int ProductImageWidthLarge
    {
      get
      {
        return productImageWidthLarge;
      }
    }

    public int ProductImageWidthMicro
    {
      get
      {
        return productImageWidthMicro;
      }
    }

    public int ProductImageWidthMini
    {
      get
      {
        return productImageWidthMini;
      }
    }

    public int ProductImageWidthSmall
    {
      get
      {
        return productImageWidthSmall;
      }
    }

    public bool IsGeneratingProductFeeds
    {
      get
      {
        return ProductFeedManager.RegisteredFeeds.Any(e => e.IsBusy);
      }
    }


    public virtual List<string> ImageFolderNames
    {
      get
      {
        return new List<string>
                 {
                     "Micro", // 50 square
                     "Mini",  // 100 square
                     "Icon",  // 150 square (what about rugs?)
                     "Small", // 225 square (what about rugs?)
                     "Medium", // 350 wide
                     "Large",  // 800 wide
                     "Original",
                 };
      }
    }


    /// <summary>
    /// Unique key used to identify this store.
    /// </summary>
    public StoreKeys StoreKey
    {
      get { return storeKey; }
    }

    /// <summary>
    /// When true (IW, IF, but not IA) - indicates store supports tracking inventory.
    /// </summary>
    /// <remarks>
    /// IA does not support this feature; most other stores do.
    /// </remarks>
    public virtual bool HasAutomatedInventoryTracking
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Absolute path to the root of ASPDNSF website folder.
    /// </summary>
    public string PathWebsiteRoot
    {
      get { return pathWebsiteRoot; }
    }

    /// <summary>
    /// SQL connection string.
    /// </summary>
    public string ConnectionString
    {
      get { return connectionString; }
    }

    public PerformanceMonitor Performance { get; private set; }

    /// <summary>
    /// Human friendly name.
    /// </summary>
    /// <remarks>
    /// Example: Inside Fabric
    /// </remarks>
    public abstract string FriendlyName { get; }

    /// <summary>
    /// Domain name - without www
    /// </summary>
    public abstract string Domain { get; }

    /// <summary>
    /// Indicates if the product data has been fully populated.
    /// </summary>
    public bool IsPopulated
    {
      get { return isPopulated; }
    }

    public bool IsPopulatingProductCache { get; protected set; }

    public bool IsRebuildingCategories { get; protected set; }

    public bool IsRebuildingSearchData { get; protected set; }

    public bool IsRebuildingAll { get; protected set; }

    public IStockCheckManager StockCheckManager { get; private set; }

    public IProductDataCache ProductData
    {
      get
      {
        if (productData == null)
          return null;

        return productData.Data;
      }
    }

    public virtual IProductFeedManager ProductFeedManager
    {
      get
      {
        throw new NotImplementedException("WebStoreBase:ProductFeedManager");
      }
    }




    /// <summary>
    /// Lock object specifically meant to synchronize database operations which should be exclusive.
    /// </summary>
    /// <remarks>
    /// These operations are assumed to have been created as synchronous methods.
    /// </remarks>
    public object ExclusiveDatabaseLockObject
    {
      get { return exclusiveDatabaseLockObject; }
    }

    /// <summary>
    /// Time when last population operation completed.
    /// </summary>
    public DateTime? TimeWhenPopulationCompleted
    {
      get
      {
        if (productData == null)
          return null;

        var cache = productData.Data;
        if (cache == null)
          return null;

        return cache.TimeWhenPopulationCompleted;
      }
    }

    /// <summary>
    /// Amount of time needed to complete the last population.
    /// </summary>
    public TimeSpan? TimeToPopulate
    {
      get
      {
        if (productData == null)
          return null;

        var cache = productData.Data;
        if (cache == null)
          return null;

        return cache.TimeToPopulate;
      }
    }

    #endregion

    #region RunAction

    /// <summary>
    /// Run an internally designated maint action for all products.
    /// </summary>
    /// <remarks>
    /// The action needs to be hard coded. Implement RunActionForAllProducts[T] to 
    /// let this base class do all the work.
    /// </remarks>
    /// <returns>Message to display on website.</returns>
    public virtual string RunActionForAllProducts(string actionName, string tag)
    {
      return "Unsupported by this store.";
    }


    /// <summary>
    /// Run an internally designated maint action for store.
    /// </summary>
    /// <remarks>
    /// The action needs to be hard coded. Implement RunActionForStore[T] to 
    /// let this base class do all the work.
    /// </remarks>
    /// <returns>Message to display on website.</returns>
    public virtual string RunAction(string actionName, string tag)
    {
      return "Unsupported by this store.";
    }


    /// <summary>
    /// Return information about any currently running maintenance operations
    /// </summary>
    /// <returns></returns>
    public WebStoreMaintenanceStatus GetStoreMaintenanceStatus()
    {
      Func<DateTime?, string> makeTimeString = (dt) =>
          {
            if (dt == null)
              return string.Empty;

            return dt.Value.ToString();
          };

      Func<bool, string> makeBoolString = (b) =>
          {
            return b ? "Yes" : "No";
          };

      Func<bool, int, string> makePercentString = (running, pct) =>
          {
            if (!running)
              return string.Empty;

            return string.Format("{0}%", pct);
          };

      // return all values as strings ready for direct output in view

      var rec = new WebStoreMaintenanceStatus()
      {
        StoreKey = StoreKey.ToString(),
        IsStoreActionRunning = makeBoolString(isStoreActionRunning),
        StoreActionName = storeActionName ?? string.Empty,
        StoreActionPercentComplete = makePercentString(isStoreActionRunning, storeActionPercentComplete),
        StoreActionTimeStarted = makeTimeString(storeActionTimeStarted),
        IsProductActionRunning = makeBoolString(isProductActionRunning),
        ProductActionName = productActionName ?? string.Empty,
        ProductActionPercentComplete = makePercentString(isProductActionRunning, productActionPercentComplete),
        ProductActionTimeStarted = makeTimeString(productActionActionTimeStarted),
        IsRebuildingAll = makeBoolString(IsRebuildingAll),
        IsRebuildingCategories = makeBoolString(IsRebuildingCategories),
        IsRebuildingSearchData = makeBoolString(IsRebuildingSearchData),
        IsBackgroundTaskRunning = makeBoolString(IsBackgroundTaskRunning)
      };

      return rec;
    }

    public virtual bool IsBackgroundTaskRunning
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Cancel any possibly running background task.
    /// </summary>
    /// <remarks>
    /// Typically called when shutting down.
    /// </remarks>
    public virtual void CancelBackgroundTask()
    {
      // default is do nothing.
    }

    /// <summary>
    /// Returns a dict of named actions (key) and corresponding method name to run.
    /// </summary>
    /// <returns></returns>
    protected virtual Dictionary<string, string> GetAvailableProductActions(Type targetClass)
    {
      var dic = new Dictionary<string, string>();

      foreach (var m in targetClass.GetMethods())
      {
        foreach (var a in m.Attributes<RunProductActionAttribute>())
        {
          if (!string.IsNullOrWhiteSpace(a.Name))
            dic.Add(a.Name.ToLower(), m.Name);
          else
            dic.Add(m.Name.ToLower(), m.Name);
        }
      }
      return dic;
    }

    /// <summary>
    /// Run an internally designated maint action for all products.
    /// </summary>
    /// <remarks>
    /// The action needs to be hard coded on a class which implements IUpdatableProduct. The
    /// action is a void, take no parameters. Should have RunProductAction attribute.
    /// </remarks>
    /// <returns></returns>
    public virtual string RunActionForAllProducts<TProduct>(string actionName, string tag) where TProduct : class, IUpdatableProduct, new()
    {
      var actions = GetAvailableProductActions(typeof(TProduct));
      string memberName;

      lock (this)
      {
        if (isProductActionRunning)
          return "An action is already running. Current command ignored.";

        isProductActionRunning = true;
        productActionCancelTokenSource = new CancellationTokenSource();
      }

      if (!actions.TryGetValue(actionName.ToLower(), out memberName))
      {
        Task.Factory.StartNew(async () =>
        {
          await Task.Delay(2000);
          MaintenanceHub.NotifyRunProductActionStatus("Invalid command.");
        });

        isProductActionRunning = false;
        productActionCancelTokenSource = null;
        return string.Format("The action '{0}' is not supported.", actionName);
      }

      Task.Factory.StartNew(
          async () =>
          {
            productActionName = actionName;
            var startTime = DateTime.Now;
            productActionActionTimeStarted = startTime;
            productActionPercentComplete = 0;

            await Task.Delay(2000);
            MaintenanceHub.NotifyRunProductActionStatus("Running...");
            await Task.Delay(100);

            Debug.WriteLine(string.Format("Started: RunActionForAllProducts({0})", StoreKey));

            var mgr = new ProductUpdateManager<TProduct>((IWebStore)this);

            mgr.progressCallback = (pct) =>
                      {
                    productActionPercentComplete = pct;
                    MaintenanceHub.NotifyRunProductActionPctComplete(pct);
                  };

            bool isParallelOperation = true;

#if DEBUG
                  isParallelOperation = false;
#endif

                  mgr.Run((p) =>
                  {
                      //p.RunAction();

                      typeof(TProduct).InvokeMember(memberName,
                          System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                          null, p, null);

                return true;
              }, productActionCancelTokenSource.Token, isParallelOperation, null, tag);

            var duration = DateTime.Now - startTime;


            if (productActionCancelTokenSource.IsCancellationRequested)
            {
              MaintenanceHub.NotifyRunProductActionStatus("Cancelled.");
            }
            else
            {
              Debug.WriteLine(string.Format("Completed: RunActionForAllProducts({0}), method: {1}, duration: {2}", StoreKey, memberName, duration));
              MaintenanceHub.NotifyRunProductActionStatus(string.Format("Done. Time to complete: {0}", duration.ToString(@"d\.hh\:mm\:ss")));
            }

            lock (this)
            {
              isProductActionRunning = false;
              productActionCancelTokenSource = null;
              productActionName = null;
              productActionActionTimeStarted = null;
            }

          }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      return "Started.";
    }

    /// <summary>
    /// Cancel any running product action.
    /// </summary>
    public virtual void CancelRunActionForAllProducts()
    {
      lock (this)
      {
        if (productActionCancelTokenSource != null)
          productActionCancelTokenSource.Cancel();
      }
    }

    /// <summary>
    /// Cancel any running store action.
    /// </summary>
    public virtual void CancelRunActionForStore()
    {
      lock (this)
      {
        if (storeActionCancelTokenSource != null)
          storeActionCancelTokenSource.Cancel();
      }
    }



    /// <summary>
    /// Returns a dict of named actions (key) and corresponding method name to run.
    /// </summary>
    /// <returns></returns>
    protected virtual Dictionary<string, string> GetAvailableStoreActions()
    {
      var dic = new Dictionary<string, string>();

      foreach (var m in this.GetType().GetMethods())
      {
        foreach (var a in m.Attributes<RunStoreActionAttribute>())
        {
          if (!string.IsNullOrWhiteSpace(a.Name))
            dic.Add(a.Name.ToLower(), m.Name);
          else
            dic.Add(m.Name.ToLower(), m.Name);
        }
      }
      return dic;
    }


    protected void NotifyStoreActionProgress(int pctCompleted)
    {
      bool sendNotification = false;

      lock (this)
      {

        if (pctCompleted != storeActionPercentComplete)
        {
          // don't want to hold the lock while sending the notification
          sendNotification = true;
          storeActionPercentComplete = pctCompleted;
        }
      }

      if (sendNotification)
        MaintenanceHub.NotifyRunStoreActionPctComplete(storeActionPercentComplete);
    }



    /// <summary>
    /// Run an internally designated maint action for the store.
    /// </summary>
    /// <remarks>
    /// The action needs to be hard coded on a class which implements IUpdatableProduct. The
    /// action is a void, take no parameters. Should have RunProductAction attribute.
    /// </remarks>
    /// <returns></returns>
    public virtual string RunActionForStore(string actionName, string tag)
    {
      var actions = GetAvailableStoreActions();
      string memberName;

      lock (this)
      {
        if (isStoreActionRunning)
          return "An action is already running. Current command ignored.";

        isStoreActionRunning = true;
        storeActionCancelTokenSource = new CancellationTokenSource();
      }

      if (!actions.TryGetValue(actionName.ToLower(), out memberName))
      {
        Task.Factory.StartNew(async () =>
        {
          await Task.Delay(2000);
          MaintenanceHub.NotifyRunStoreActionStatus("Invalid command.");
        });

        isStoreActionRunning = false;
        storeActionCancelTokenSource = null;
        return string.Format("The action '{0}' is not supported.", actionName);
      }

      Task.Factory.StartNew(
          async () =>
          {
            storeActionPercentComplete = 0;
            var startTime = DateTime.Now;
            storeActionTimeStarted = startTime;
            storeActionName = actionName;

            await Task.Delay(2000);
            MaintenanceHub.NotifyRunStoreActionStatus("Running...");
            await Task.Delay(10);


            Debug.WriteLine(string.Format("Started: RunActionForStore({0})", StoreKey));

                  //p.RunAction(cancelToken);

                  object[] parameters;
            if (tag != null)
            {
              parameters = new object[] { tag, storeActionCancelTokenSource.Token };
            }
            else
            {
              parameters = new object[] { storeActionCancelTokenSource.Token };
            }
            Action<string> sendMessage = (s) =>
                  {
                MaintenanceHub.NotifyRunStoreActionStatus(s);
              };

            try
            {

              this.GetType().InvokeMember(memberName,
                        System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, this, parameters);

              var duration = DateTime.Now - startTime;

              if (storeActionCancelTokenSource.IsCancellationRequested)
              {
                sendMessage("Cancelled.");
              }
              else
              {
                Debug.WriteLine(string.Format("Completed: RunActionForStore({0}), method: {1}, duration: {2}", StoreKey, memberName, duration));
                sendMessage(string.Format("Done. Time to complete: {0}", duration.ToString(@"d\.hh\:mm\:ss")));
              }

            }
            catch (Exception Ex)
            {
              sendMessage(string.Format("Failed: {0}", Ex.Message));
            }

                  // ultimate indicator to browser client that this operation is over - for whatever reason.
                  // causes cancel button and UX to show as no longer doing anything.
                  sendMessage("[[finished]]");

            lock (this)
            {
              isStoreActionRunning = false;
              storeActionCancelTokenSource = null;
              storeActionName = null;
              storeActionTimeStarted = null;
            }


          }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      return "Started.";
    }


    #endregion

    #region Rebuild All

    /// <summary>
    /// Initiate a complete sweep of rebuilding/repopulating.
    /// </summary>
    /// <remarks>
    /// Only one can run at a time. 
    /// </remarks>
    /// <returns>True if commenced new operation, false if queued up.</returns>
    public bool RebuildAll()
    {
      var isAlreadyRunning = IsRebuildingAll;

      Task.Factory.StartNew(
          () =>
          {

                  // only one thread will ever be doing the real work at any one time. If a new request
                  // comes in and sees another is already running, it will just bump the queue count
                  // and bail so the existing task can just keep cycling through until it finishes

                  bool bHasLock = false;

            try
            {
              Interlocked.Increment(ref rebuildAllQueueCount);

              int timeout = (1000 * 60 * 10);

                    // if was already running an op, don't wait around if cannot get lock right off since
                    // the existing thread will cycle around and process the queue

                    Monitor.TryEnter(ExclusiveDatabaseLockObject, (IsRebuildingAll ? 0 : timeout), ref bHasLock);

              if (!bHasLock)
                return;

              IsRebuildingAll = true;

                    // if more than one operation builds up while the first is running, 
                    // the subsequent operations can be consolidated

                    while (Interlocked.Exchange(ref rebuildAllQueueCount, 0) > 0)
              {
                      // notice the order of processing!

                      Debug.WriteLine("Begin RebuildProductSearchExtensionData()");
                RebuildProductSearchExtensionData(timeout);

                Debug.WriteLine("Begin RebuildProductCategoryTable()");
                RebuildProductCategoryTable(timeout);

                Debug.WriteLine("Begin RepopulateProducts()");
                RepopulateProducts(timeout);

                Debug.WriteLine("Begin RebuildAllStoreSpecificTasks()");
                RebuildAllStoreSpecificTasks();
              }

            }
            catch (Exception Ex)
            {
              var ev2 = new WebsiteRequestErrorEvent(string.Format("Failed on RebuildAll() for {0}.", storeKey), this, WebsiteEventCode.UnhandledException, Ex);
              ev2.Raise();
              return;
            }
            finally
            {
              if (bHasLock)
              {
                IsRebuildingAll = false;
                Monitor.Exit(ExclusiveDatabaseLockObject);
              }
            }
          }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      return !isAlreadyRunning;
    }

    protected virtual void RebuildAllStoreSpecificTasks()
    {
      // to be overriden by store specific class when extra steps are needed
      // for a rebuild all operation.

      // any misc tasks like spinning up missing descriptions, etc.

    }


    #endregion

    #region Repopulate Product Cache
    /// <summary>
    /// Repopulate product data into memory cache. Synchronous.
    /// </summary>
    /// <remarks>
    /// This is a synchronous operation. If there is existing data, it will be replaced
    /// by the new data only after the data object(s) are complete.
    /// </remarks>
    /// <returns>true if successful.</returns>
    public virtual bool RepopulateProducts(int millisecondsTimeout = 0)
    {
      bool bHasLock = false;

      try
      {
        Monitor.TryEnter(ExclusiveDatabaseLockObject, millisecondsTimeout, ref bHasLock);

        if (!bHasLock)
          return false;

        IsPopulatingProductCache = true;


        if (productData == null)
          productData = new ProductDataService<TProductDataCache>(storeKey, connectionString);
        else
          productData.Refresh();

        isPopulated = (productData != null) && (productData.Data != null);

        return IsPopulated;
      }
      catch (Exception Ex)
      {
        var ev2 = new WebsiteRequestErrorEvent(string.Format("Failed on RePopulateProducts() for {0}.", storeKey), this, WebsiteEventCode.UnhandledException, Ex);
        ev2.Raise();

        return false;

      }
      finally
      {
        if (bHasLock)
        {
          IsPopulatingProductCache = false;
          Monitor.Exit(ExclusiveDatabaseLockObject);
        }
      }

    }
    #endregion

    #region Rebuild Categories

    /// <summary>
    /// Rebuild the SQL ProductCategory table which associates the categories for each product. Synchronous.
    /// </summary>
    /// <remarks>
    /// This base implementation looks for a file named {storeKey}RebuildCategories.sql in the App_Data folder.
    /// </remarks>
    /// <returns>true if successful.</returns>
    public virtual bool RebuildProductCategoryTable(int millisecondsTimeout = 0)
    {
      bool bHasLock = false;


      try
      {
        if (IsGeneratingProductFeeds)
          throw new Exception("Product feeds are being generated. Abort request to rebuild categories.");

        Monitor.TryEnter(ExclusiveDatabaseLockObject, millisecondsTimeout, ref bHasLock);

        if (!bHasLock)
          return false;

        IsRebuildingCategories = true;

        // this would be a very rare event - but just in case a rebuild sneaks through while
        // generating feeds, we want to stop the feed since it could be contaminated.

        foreach (var feed in ProductFeedManager.RegisteredFeeds.Where(e => e.IsBusy))
          feed.CancelOperation();

        var scriptFilepath = MapPath(string.Format(@"~/App_Data/Sql/{0}RebuildCategories.sql", storeKey));

        RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        // remove and products which have been manually excluded from categories which may have been
        // added back in by some of the generic logic

        scriptFilepath = MapPath(@"~/App_Data/Sql/PurgeProductCategoryExclusions.sql");

        RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);

        RebuildProductCategoriesEx(); // allow stores to add additional logic

        return true;

      }
      catch (Exception Ex)
      {
        Debug.WriteLine(string.Format("RebuildProductCategoryTable() exception for {0}:\n{1}", storeKey, Ex.ToString()));

        var ev2 = new WebsiteRequestErrorEvent(string.Format("Failed to RebuildProductCategoryTable for {0}.", storeKey), this, WebsiteEventCode.UnhandledException, Ex);
        ev2.Raise();

        return false;
      }
      finally
      {
        if (bHasLock)
        {
          IsRebuildingCategories = false;
          Monitor.Exit(ExclusiveDatabaseLockObject);
        }
      }
    }

    #endregion

    #region Search Galleries

    [RunStoreAction("ReviewSearchGalleries")]
    public void ReviewSearchGalleries(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Review search galleries...");
      SearchGalleryManager.ReviewSearchGalleries(cancelToken, (pct) =>
      {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
      });

      NotifyStoreActionProgress(100);
    }

    [RunStoreAction("PopulateMissingSearchGalleries")]
    public void PopulateMissingSearchGalleries(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Populate missing search galleries...");
      SearchGalleryManager.PopulateMissingSearchGalleries(cancelToken, (pct) =>
      {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
      });

      NotifyStoreActionProgress(100);
    }

    #endregion

    #region Autosuggest

    /// <summary>
    /// Allow subclass to manually inject specific phrases.
    /// </summary>
    protected virtual List<string> InjectedAutoSuggestPhrases
    {
      get
      {
        return new List<string>();
      }
    }

    /// <summary>
    /// Truncate and repopulate the AutoSuggest table. Ext2 only.
    /// </summary>
    /// <remarks>
    /// Ext3 does not contribute to autosuggest.
    /// </remarks>
    /// <param name="dc"></param>
    protected virtual void PopulateAutoSuggestTable(AspStoreDataContext dc)
    {
      // guard for early stage development when SQL is wiped clean
      if (dc.Products.Count() == 0)
        return;

      // evaluates existing ExtensionData2 for all products and refigures SQL table autosuggestphrases.

      // get all the ext2 data

      var maxProductID = dc.Products.Max(e => e.ProductID);
      var beginProductID = dc.Products.Min(e => e.ProductID);

      var distinctPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      int count = 200;

      // read in batches of 5,000 records, process into individual (distinct) lines

      // what we're doing here is building a hash set (lower case) which has every word or phrase
      // we wish to be part of the autosuggest database - and then once we have the list in memory
      // it's written to the SQL table, truncate first, always start fresh.

      while (true)
      {
        var phraseSets = dc.Products
            .Where(e => (e.ProductID >= beginProductID) && (e.ProductID < (beginProductID + count) && e.Published == 1 && e.Deleted == 0 && e.ExtensionData2 != null))
            .Select(e => e.ExtensionData2).ToList();

        if (phraseSets.Count() > 0)
        {
          foreach (var record in phraseSets)
          {
            if (string.IsNullOrWhiteSpace(record))
              continue;

            // the ext2 data is one long string (with line breaks) so we break it into the individual lines.
            record.ConvertToListOfLines().ForEach(e => distinctPhrases.Add(e.ToLower()));
          }
        }

        beginProductID += count;

        if (beginProductID > maxProductID)
          break;
      }

      foreach (var s in InjectedAutoSuggestPhrases)
        distinctPhrases.Add(s.ToLower());

      // if the store supports a category filter manager, get the phrases from the Summary columns of the
      // sub-cats and add them as autosuggest phrases

      // one could debate if this is meaningful logic, because in theory, this yields terms for all the
      // categories whereas since the products use reverse lookup to include these same terms, the better list
      // would actually come from the products. However, in the end, likely is don't care since if there's even
      // a single product for each term, it all works out the same.

      if (this.CategoryFilterManager != null)
      {
        // add to distinctPhrases hash

        // this is a comma sep list, could have spaces (toList needed on hashset because SQL has limited support for contains operator)
        var keywords = dc.Categories.Where(e => (this.CategoryFilterManager.AllCategoryFilters).ToList().Contains(e.CategoryID)).Select(e => e.Summary).ToList();
        foreach (var item in keywords)
        {
          foreach (var s in item.ParseCommaDelimitedList())
            distinctPhrases.Add(s.ToLower());
        }
      }

      // instead of truncating, is better to prune and adjust in place - so does not take down
      // the live autosuggest

#if false
            // remove all existing entries in the table
            dc.AutoSuggestPhrases.TruncateTable();

            // save each distinct phrase to SQL
            // initially here, all have same priority

            foreach (var phrase in distinctPhrases)
            {
                var item = new AutoSuggestPhrase
                {
                    PhraseListID = 0,
                    Phrase = phrase,
                    Priority = 1,
                };

                dc.AutoSuggestPhrases.InsertAutoSuggestPhrase(item);
            }
#else
      // intent to only figure out the delta between what is there now and what's needed

      // save each distinct phrase to SQL
      // initially here, all have same priority

      Dictionary<string, int> dicExistingPhrases;
      dicExistingPhrases = dc.AutoSuggestPhrases.Where(e => e.PhraseListID == 0).Select(e => new { e.PhraseID, e.Phrase }).ToDictionary(k => k.Phrase, v => v.PhraseID);

      // for each existing phrase that is not still in the new phrase set, remove from SQL

      var countRemoved = 0;
      foreach (var item in dicExistingPhrases)
      {
        if (!distinctPhrases.Contains(item.Key))
        {
          dc.AutoSuggestPhrases.DeleteAutoSuggestPhrase(item.Value);
          countRemoved++;
        }
      }
      Debug.WriteLine(string.Format("Autosuggest removed: {0:N0}", countRemoved));

      // for each value in new set which is not in the existing set, add to SQL

      var countAdded = 0;
      foreach (var phrase in distinctPhrases)
      {
        try
        {
          if (dicExistingPhrases.ContainsKey(phrase))
            continue;

          var item = new AutoSuggestPhrase
          {
            PhraseListID = 0,
            Phrase = phrase,
            Priority = 1,
          };

          dc.AutoSuggestPhrases.InsertAutoSuggestPhrase(item);
          countAdded++;

        }
        catch (Exception Ex)
        {
          Debug.WriteLine("Exception inserting autosuggest phrase: {0}\n{1}", phrase, Ex.Message);
        }
      }

      Debug.WriteLine(string.Format("Autosuggest added: {0:N0}", countAdded));
#endif

    }

    #endregion

    #region Algolia

    [RunStoreAction("PopulateAlgoliaSuggestions")]
    public void PopulateAlgoliaSuggestions(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Populating Algolia...");
      PopulateAlgoliaSuggestions(cancelToken, (pct) =>
      {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
      });


      NotifyStoreActionProgress(100);
    }

    protected virtual void PopulateAlgoliaSuggestions(CancellationToken cancelToken, Action<int> reportProgress = null)
    {
      var indexName = string.Format("{0}Suggestions", storeKey.ToString());
      var algoliaClient = new AlgoliaClient(this.AlgoliaApplicationID, this.AlgoliaAdminApiKey);
      var algoliaIndex = algoliaClient.InitIndex(indexName);

      Func<string, HashSet<string>> loadFile = (filename) =>
          {
            var filepath = MapPath(string.Format(@"~/App_Data/AutoSuggest/{0}-{1}.txt", StoreKey, filename));
            var lines = File.ReadAllLines(filepath);

            var lowerLines = new HashSet<string>();
            lines.ForEach(e => lowerLines.Add(e.ToLower().Trim()));
            return lowerLines;
          };

      reportProgress(10);

      var brands = loadFile("Brands");
      var categories = loadFile("Categories");
      var combined = loadFile("Combined");

      var records = new List<AlgoliaSuggestionRecord>();

      foreach (var item in combined)
      {
        int rank = 10; // default property rank

        // boost rank for vendors and categories

        if (brands.Contains(item))
          rank = 100;
        else if (categories.Contains(item))
          rank = 50;

        records.Add(new AlgoliaSuggestionRecord(item, rank));
      }


      reportProgress(20);

      algoliaIndex.ClearIndex();
      algoliaIndex.AddObjects(records);

      reportProgress(80);
    }


    [RunStoreAction("GenerateAlgoliaSuggestions")]
    public void GenerateAlgoliaSuggestions(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Generage Algolia Suggestions...");
      SyncAlgoliaProducts(false, true, cancelToken, (pct) =>
      {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
      });


      NotifyStoreActionProgress(100);
    }

    [RunStoreAction("SyncAlgoliaProductsAction")]
    public void SyncAlgoliaProductsAction(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Syncing to Algolia...");
      SyncAlgoliaProducts(true, false, cancelToken, (pct) =>
      {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
      });


      NotifyStoreActionProgress(100);
    }

    protected virtual void SyncAlgoliaProducts(bool pushUpdates, bool enableWriteFiles, CancellationToken cancelToken, Action<int> reportProgress = null)
    {
      //if (!this.IsAlgoliaEnabled)
      //    return;

      var indexName = string.Format("{0}Products", storeKey.ToString());
      var algoliaClient = new AlgoliaClient(this.AlgoliaApplicationID, this.AlgoliaAdminApiKey);
      var algoliaIndex = algoliaClient.InitIndex(indexName);

      reportProgress(0);

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        // need to see about pruning missing
        dc.MarkAlgoliaProductOrphansToBeDeleted();
      }

      reportProgress(5);

      if (cancelToken.IsCancellationRequested)
        return;

      // delete operations
      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        var deletedProducts = dc.AlgoliaProducts.Where(e => e.Action == (int)AlgoliaAction.Delete).Select(e => e.ProductID).ToList();

        var objectList = new List<string>();
        foreach (var product in deletedProducts)
          objectList.Add(product.ToString());

        if (deletedProducts.Count() > 0)
        {
          algoliaIndex.DeleteObjects(objectList);

          // update SQL

          reportProgress(10);

          // spin through 250 at a time
          while (deletedProducts.Count() > 0)
          {
            var slice = new HashSet<int>(deletedProducts.Take(250));
            dc.AlgoliaProducts.RemoveRecords(slice);
            deletedProducts.RemoveAll(e => slice.Contains(e));
          }
        }
      }

      reportProgress(15);

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        // find new products and insert into the algolia table
        dc.AlgoliaProducts.FindAndInsertNewProducts(SupportedProductGroups);
      }

      if (cancelToken.IsCancellationRequested)
        return;

      reportProgress(20);

      if (cancelToken.IsCancellationRequested)
        return;

      // upsert operations

      int upsertCompleted = 0;
      int lastReportedProgressPct = 20; // because was 20 through delete
      int countUpsertTotal = 0;

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        countUpsertTotal = dc.AlgoliaProducts.Where(e => e.Action == (int)AlgoliaAction.Upsert).Count();
      }

      Action<int> notifyProgress = (completed) =>
      {
        var pct = countUpsertTotal == 0 ? 0 : (completed * 100) / countUpsertTotal;

        var proratedPct = (int)(20 + (pct * .80));

        lastReportedProgressPct = proratedPct;
        if (reportProgress != null)
          reportProgress(lastReportedProgressPct);
      };

      var keyBrands = new HashSet<string>();
      var keyCategories = new HashSet<string>();
      var keyProperties = new HashSet<string>();

      while (!cancelToken.IsCancellationRequested)
      {
        using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
        {
          var upsertProducts = dc.AlgoliaProducts.Where(e => e.Action == (int)AlgoliaAction.Upsert).Select(e => e.ProductID).Take(100).ToList();

          if (upsertProducts.Count() == 0)
            break;

          var objectList = new List<AlgoliaProductRecord>();
          var toBeDeleted = new List<int>();

          Action<int> processSingleProduct = (product) =>
              {
                try
                {
                  var obj = MakeAlgoliaProductRecord(product, null);
                  if (obj != null)
                  {
                    lock (objectList)
                    {
                      objectList.Add(obj);
                      if (enableWriteFiles)
                      {
                        keyBrands.Add(obj.brand);
                        obj.categories.ForEach(e => keyCategories.Add(e));
                        obj.properties.ForEach(e =>
                                  {
                                    if (e.Length < 4)
                                      return;

                                    if (e.Any(char.IsDigit))
                                      return;

                                    keyProperties.Add(e);
                                  });
                      }
                    }
                  }
                  else
                  {
                    lock (toBeDeleted)
                    {
                      toBeDeleted.Add(product);
                    }
                  }

                }
                catch (Exception Ex)
                {
                  Debug.WriteLine(Ex.ToString());
                }

              };
#if true
          foreach (var product in upsertProducts)
          {
            processSingleProduct(product);
            if (cancelToken.IsCancellationRequested)
              break;
          }
#else
                    Parallel.ForEach(upsertProducts, (product, state) =>
                    {
                        process(product);
                        if (cancelToken.IsCancellationRequested)
                            state.Break();
                    });
#endif

          if (cancelToken.IsCancellationRequested)
            continue;

          foreach (var id in toBeDeleted)
          {
            dc.AlgoliaProducts.RemoveRecord(id);
          }

          if (pushUpdates)
            algoliaIndex.AddObjects(objectList);

          upsertCompleted += upsertProducts.Count();

          // update SQL

          // spin through 100 at a time
          while (upsertProducts.Count() > 0)
          {
            var slice = new HashSet<int>(upsertProducts.Take(100));
            dc.AlgoliaProducts.UpdateAlgoliaProductsAction(slice, AlgoliaAction.None);
            upsertProducts.RemoveAll(e => slice.Contains(e));
          }

          // report
          notifyProgress(upsertCompleted);
        }
      }

      if (enableWriteFiles)
      {
        Func<string, string> makeFilepath = (filename) =>
            {
              return MapPath(string.Format(@"~/App_Data/AutoSuggest/{0}-{1}.txt", StoreKey, filename));
            };

        // brands
        using (StreamWriter sw = File.AppendText(makeFilepath("Brands")))
        {
          foreach (var item in keyBrands)
            sw.WriteLine(item);
        }
        // categories
        using (StreamWriter sw = File.AppendText(makeFilepath("Categories")))
        {
          foreach (var item in keyCategories)
            sw.WriteLine(item);
        }
        // properties
        using (StreamWriter sw = File.AppendText(makeFilepath("Properties")))
        {
          foreach (var item in keyProperties)
            sw.WriteLine(item);
        }

        using (StreamWriter sw = File.AppendText(makeFilepath("Combined")))
        {
          var combined = new HashSet<string>();
          keyBrands.ForEach(e => combined.Add(e.ToLower()));
          keyCategories.ForEach(e => combined.Add(e.ToLower()));
          keyProperties.ForEach(e => combined.Add(e.ToLower()));

          foreach (var item in combined)
            sw.WriteLine(item);
        }
      }
    }

    protected virtual AlgoliaProductRecord MakeAlgoliaProductRecord(int productID, AspStoreDataContext dc)
    {
      return new AlgoliaProductRecord(productID);
    }

    #endregion

    #region Update ProductCollections


    protected virtual void UpdateProductLabels(CancellationToken cancelToken, Action<int> reportProgress = null)
    {
      // to be overriden by store specific class when extra steps are needed
      // for a rebuild all operation.

      // any misc tasks like spinning up missing descriptions, etc.

    }

    /// <summary>
    /// Refresh the ProductCollections table. Never harmful to run.
    /// </summary>
    /// <remarks>
    /// Adds new books, patterns, etc.
    /// </remarks>
    [RunStoreAction("UpdateProductCollections")]
    public void UpdateProductCollections(CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Updating ProductLabels table...");
      UpdateProductLabels(cancelToken, (pct) =>
       {
              // don't show 100 percent here to avoid triger done
              NotifyStoreActionProgress(Math.Min(pct, 99));
       });

      UpdateProductCollectionsWorker(cancelToken, (pct) =>
      {
        NotifyStoreActionProgress(pct);
      }, (status) =>
      {
        MaintenanceHub.NotifyRunStoreActionStatus(status);
      });

      NotifyStoreActionProgress(100);
    }

    public void UpdateProductCollectionsWorker(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
    {
#if true
      RunProductCollectionTask((updater) =>
          {
            return updater.RunUpdate(cancelToken, reportStatus);
          }, cancelToken, reportProgress, reportStatus);
#else
            // works - before refactor
            int lastReportedProgressPct = 0;
            List<int> listManufacturers = new List<int>();
            int countTotal = 0;

            Action<int> notifyProgressAndRemaining = (completed) =>
            {
                var pct = countTotal == 0 ? 0 : (completed * 100) / countTotal;

                lastReportedProgressPct = pct;
                if (reportProgress != null)
                    reportProgress(lastReportedProgressPct);
            };

            using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
            {
                listManufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1 && e.Name != string.Empty && e.SEName != string.Empty).Select(e => e.ManufacturerID).ToList();
            }

            int countErrors = 0;

            countTotal = listManufacturers.Count * 2; // books and patterns for each

            int countCompleted = 0;
            notifyProgressAndRemaining(countCompleted);

            foreach (var manufacturerID in listManufacturers)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                string resultText;

                var bookUpdater = new BookProductCollectionUpdater(this as IWebStore, manufacturerID);
                resultText = bookUpdater.RunUpdate(cancelToken, reportStatus);

                // because could also get back "Cancelled"
                if (cancelToken.IsCancellationRequested)
                    break;

                if (resultText != "Success")
                    countErrors++;

                countCompleted++;
                notifyProgressAndRemaining(countCompleted);

                if (cancelToken.IsCancellationRequested)
                    break;

                var patternUpdater = new PatternProductCollectionUpdater(this as IWebStore, manufacturerID);
                resultText = patternUpdater.RunUpdate(cancelToken, reportStatus); // "Success"

                // because could also get back "Cancelled"
                if (cancelToken.IsCancellationRequested)
                    break;

                if (resultText != "Success")
                    countErrors++;

                countCompleted++;
                notifyProgressAndRemaining(countCompleted);
            }

            // if any problems along the way, throw an exception to trigger the desired
            // on-screen display since otherwise shows only that it completed

            if (!cancelToken.IsCancellationRequested)
            {
                if (countErrors > 0)
                    throw new Exception(string.Format("{0:N0} errors reported.", countErrors));
            }
#endif
    }


    protected virtual void UpdateProductLabels()
    {
      // called as part of updating collection tables to ensure all new patterns and collections are included.
    }

    /// <summary>
    /// Recreate all the images for product collections. Never harmful to run.
    /// </summary>
    /// <remarks>
    /// Use for when changing specifications, need a clean sweep.
    /// </remarks>
    [RunStoreAction("RebuildProductCollectionImages")]
    public void RebuildProductCollectionImages(CancellationToken cancelToken)
    {
      RebuildProductCollectionImagesWorker(cancelToken, (pct) =>
      {
        NotifyStoreActionProgress(pct);
      }, (status) =>
      {
        MaintenanceHub.NotifyRunStoreActionStatus(status);
      });

      NotifyStoreActionProgress(100);
    }

    public void RebuildProductCollectionImagesWorker(CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
    {
      RunProductCollectionTask((updater) =>
          {
            return updater.RunRebuildImages(cancelToken, reportStatus);
          }, cancelToken, reportProgress, reportStatus);

    }

    protected virtual void RunProductCollectionTask(Func<IProductCollectionUpdater, string> callback, CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
    {
      int lastReportedProgressPct = 0;
      List<int> listManufacturers = new List<int>();
      int countTotal = 0;

      Action<int> notifyProgress = (completed) =>
      {
        var pct = countTotal == 0 ? 0 : (completed * 100) / countTotal;

        lastReportedProgressPct = pct;
        if (reportProgress != null)
          reportProgress(lastReportedProgressPct);
      };

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        listManufacturers = dc.Manufacturers.Where(e => e.Deleted == 0 && e.Published == 1 && e.Name != string.Empty && e.SEName != string.Empty).Select(e => e.ManufacturerID).ToList();
      }

      int countErrors = 0;

      countTotal = listManufacturers.Count * 3; // books, collections and patterns for each

      int countCompleted = 0;
      notifyProgress(countCompleted);

      foreach (var manufacturerID in listManufacturers)
      {
        if (cancelToken.IsCancellationRequested)
          break;

        IProductCollectionUpdater updater;

        Action<IProductCollectionUpdater> executeUpdater = (u) =>
            {
              var resultText = callback(u);

              if (resultText != "Success")
                countErrors++;

              countCompleted++;
              notifyProgress(countCompleted);
            };

        // books
        updater = new BookProductCollectionUpdater(this as IWebStore, manufacturerID);
        executeUpdater(updater);

        // because could also get back "Cancelled"
        if (cancelToken.IsCancellationRequested)
          break;

        // collections
        updater = new CollectionProductCollectionUpdater(this as IWebStore, manufacturerID);
        executeUpdater(updater);

        // because could also get back "Cancelled"
        if (cancelToken.IsCancellationRequested)
          break;

        // patterns
        updater = new PatternProductCollectionUpdater(this as IWebStore, manufacturerID);
        executeUpdater(updater);
      }

      // if any problems along the way, throw an exception to trigger the desired
      // on-screen display since otherwise shows only that it completed

      if (!cancelToken.IsCancellationRequested)
      {
        if (countErrors > 0)
          throw new Exception(string.Format("{0:N0} errors reported.", countErrors));
      }
    }


    #endregion

    #region Helper Methods

    /// <summary>
    /// Return the full name of the manufacturer for an id.
    /// </summary>
    /// <param name="manufacturerID"></param>
    /// <returns></returns>
    public string LookupManufacturerName(int manufacturerID)
    {
      ManufacturerInformation record;

      if (this.ProductData.ManufacturerInfo.TryGetValue(manufacturerID, out record))
        return record.Name;

      return string.Empty;
    }

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
    public virtual int GetManufacturerDisplayWeight(int ManufacturerID)
    {
      // the default is to weight all equally. Individual stores
      // can therefore override with a more granular formula if needed

      return 100;
    }

    /// <summary>
    /// Run the given sql script. 
    /// </summary>
    /// <remarks>
    /// Return false if file not found. Throw on execution errors.
    /// </remarks>
    /// <param name="scriptFilepath"></param>
    /// <returns></returns>
    protected bool RunSQLScript(string scriptFilepath, int timeoutSeconds = 300)
    {
      try
      {
        if (File.Exists(scriptFilepath))
        {
          var scriptText = ReadFile(scriptFilepath);

          using (SqlConnection conn = new SqlConnection(ConnectionString))
          {
            Server server = new Server(new ServerConnection(conn));
            server.ConnectionContext.StatementTimeout = timeoutSeconds;
            server.ConnectionContext.ExecuteNonQuery(scriptText);
          }

          return true;
        }

      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
        LogException("RunSQLScript", Ex);
      }

      return false;
    }

    private void LogException(string methodName, Exception Ex)
    {
      var msg = string.Format("InsideStores Data Service\nUnhandled exception in {0}: {1}", this.StoreKey, methodName);
      var ev = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
      ev.Raise();
    }


    /// <summary>
    /// Override this method to run custom C# code for a specific store.
    /// </summary>
    /// <remarks>
    /// Throw exception if there is an error.
    /// </remarks>
    /// <returns></returns>
    protected virtual void RebuildProductSearchDataEx()
    {


    }

    /// <summary>
    /// Override this method to run custom C# code for a specific store.
    /// </summary>
    /// <remarks>
    /// Throw exception if there is an error.
    /// </remarks>
    /// <returns></returns>
    protected virtual void RebuildProductCategoriesEx()
    {


    }


    /// <summary>
    /// Rebuild the extension data column in SQL Product table to facilitate full text search. Synchronous.
    /// </summary>
    /// <remarks>
    /// Builds up text never seen by users to give the FTS engine something good to chew on.
    /// </remarks>
    /// <returns>true if successful.</returns>
    public virtual bool RebuildProductSearchExtensionData(int millisecondsTimeout = 0)
    {
      bool bHasLock = false;

      try
      {
        Monitor.TryEnter(ExclusiveDatabaseLockObject, millisecondsTimeout, ref bHasLock);

        if (!bHasLock)
          return false;

        IsRebuildingSearchData = true;

        // will throw if error.
        RebuildProductSearchDataEx();

        return true;
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(string.Format("RebuildProductSearchExtensionData() exception for {0}:\n{1}", storeKey, Ex.ToString()));

        var ev2 = new WebsiteRequestErrorEvent(string.Format("Failed to RebuildProductSearchExtensionData for {0}.", storeKey), this, WebsiteEventCode.UnhandledException, Ex);
        ev2.Raise();

        return false;
      }
      finally
      {
        if (bHasLock)
        {
          IsRebuildingSearchData = false;
          Monitor.Exit(ExclusiveDatabaseLockObject);
        }
      }

    }

    public void LogFacetSearch(FacetSearchCriteria criteria, int page, string visitorID)
    {
      facetSearchLogger.LogFacetSearch(criteria, page, visitorID);
    }

    protected string ReadFile(string filename)
    {
      using (var stream = System.IO.File.OpenText(filename))
      {
        return stream.ReadToEnd();
      }
    }

    protected string ReadTextFileWithCache(string sFilepath)
    {
      // cache contents on file read to reduce latency, then add a cache file dependency

      string CacheKey = sFilepath.ToUpper(); // salt added to ensure unique
      string sResult;
      var cache = HttpRuntime.Cache;

      if ((sResult = (string)cache[CacheKey]) == null)
      {
        using (StreamReader objStreamReader = File.OpenText(sFilepath))
          sResult = objStreamReader.ReadToEnd();

        if (sResult.Length > 0)
          cache.Insert(CacheKey, sResult, new CacheDependency(sFilepath));
      }
      return sResult;
    }

    protected static string MapPath(string input)
    {
      // the path relative to the application root folder
      string appPath;

      if (input.StartsWith("~/"))
      {
        appPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, input.Substring(2)).Replace("/", "\\");
      }
      else
        appPath = input.Replace("/", "\\");


      if (appPath.StartsWith("\\"))
        appPath = appPath.Substring(1);

      var result = Path.Combine(HttpRuntime.AppDomainAppPath, appPath);

#if DEBUG
      // double check to make sure we're yielding identical results.
      if (HttpContext.Current != null)
      {
        var oldWay = HttpContext.Current.Server.MapPath(input);
        Debug.Assert(string.Equals(result, oldWay, StringComparison.OrdinalIgnoreCase));
      }
#endif

      return result;
    }

    #endregion

    #region Campaigns

    private bool SendGridAddToMarketingList(string listName, string email)
    {
      Debug.Assert(listName == "master");

      // we only use a single list now
      if (listName != "master")
        return false;

      try
      {
        // there is a main CONTACTS DB which is the universe of emails
        // then there are lists which have only identifiers back to main list, and these identifiers are simply the base64 of the email address

        // credentials
        var apiKey = WebConfigurationManager.AppSettings["sendgridApiKey"];
        // master list
        var listID = int.Parse(WebConfigurationManager.AppSettings["sendgridMasterContactList"]);

        // must first add to main contact db, then reference in the master list (1728679)

        var client = new RestClient("https://api.sendgrid.com/v3/contactdb/recipients");
        var request = new RestRequest(Method.POST);
        request.AddHeader("content-type", "application/json");
        request.AddHeader("authorization", "Bearer " + apiKey);
        request.AddParameter("application/json", string.Format("[{{\"email\":\"{0}\"}}]", email.ToLower()), ParameterType.RequestBody);
        IRestResponse response = client.Execute(request);

        if (!IsHTTPSuccess(response.StatusCode))
        {
          LogSystemEvent(string.Format("Failed to add email subscriber to contacts db from {0}: {1}", StoreKey.ToString(), email));
          return false;
        }
        var base64Email = email.ToLower().Base64Encode();

        client = new RestClient(string.Format("https://api.sendgrid.com/v3/contactdb/lists/{0}/recipients/", listID) + base64Email);
        request = new RestRequest(Method.POST);
        request.AddHeader("authorization", "Bearer " + apiKey);
        request.AddParameter("undefined", "null", ParameterType.RequestBody);
        response = client.Execute(request);
        //HTTP:201
        if (!IsHTTPSuccess(response.StatusCode))
        {
          LogSystemEvent(string.Format("Failed to add email subscriber to Master list from {0}: {1}", StoreKey.ToString(), email));
          return false;
        }

        RecordNewEmailSubscriber(email);
        LogSystemEvent(string.Format("Added email subscriber to Master list from {0}: {1}", StoreKey.ToString(), email));

        return true;
      }
      catch (Exception Ex)
      {
        // adding to an invalid list name results in 401
        Debug.WriteLine(Ex.Message);
        return false;
      }
    }


    private bool IsHTTPSuccess(HttpStatusCode code)
    {
      if ((int)code >= 200 && (int)code < 300)
        return true;

      return false;
    }

    private bool SendGridRemoveFromMarketingList(string listName, string email)
    {
      try
      {
        // there is a main CONTACTS DB which is the universe of emails
        // then there are lists which have only identifiers back to main list, and these identifiers are simply the base64 of the email address

        // if remove from contacts, then automatically also removed from any lists

        // credentials
        var apiKey = WebConfigurationManager.AppSettings["sendgridApiKey"];

        var base64Email = email.ToLower().Base64Encode();
        var client = new RestClient("https://api.sendgrid.com/v3/contactdb/recipients/" + base64Email);
        var request = new RestRequest(Method.DELETE);
        request.AddHeader("authorization", "Bearer " + apiKey);
        request.AddParameter("undefined", "null", ParameterType.RequestBody);
        IRestResponse response = client.Execute(request);

        if (IsHTTPSuccess(response.StatusCode))
        {
          LogSystemEvent(string.Format("Removed email subscriber from list {0}: {1}", listName, email));
          return true;
        }
        else
        {
          // typically 404 when recpient does not exist
          LogSystemEvent(string.Format("Failed to remove email subscriber from list {0}: {1}", listName, email));
          return false;
        }
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
        return false;
      }
    }

    private void RecordNewEmailSubscriber(string email)
    {
      try
      {
        using (var dc = new AspStoreDataContext(ConnectionString))
        {
          var subscriber = new EmailSubscriber()
          {
            Email = email,
            CreatedOn = DateTime.Now
          };

          dc.EmailSubscribers.InsertOnSubmit(subscriber);
          dc.SubmitChanges();
        }
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
      }
    }

    private string ComputeEmailDigest(string email)
    {
      // this must exactly match the data service and store website
      var digest = (email.ToLower() + "peterandtessa").SHA256Digest();
      return digest.ToLower();
    }

    private async Task<bool> SendGridTransactionalMessage(string templateID, string email, bool includeUnsubscribe, string category = null)
    {
      try
      {
        // credentials
        var username = WebConfigurationManager.AppSettings["sendgridUserName"];
        var password = WebConfigurationManager.AppSettings["sendgridPassword"];
        var credentials = new NetworkCredential(username, password);

        var message = new SendGrid.SendGridMessage();
        message.AddTo(email);
        message.From = new System.Net.Mail.MailAddress("sales@example.com", "Inside Stores");

        // subject and body will come from the template engine
        // these entries seem to be needed to make things work even though we really want to ignore them.
        message.Subject = " ";
        message.Text = " ";
        message.Html = "<p></p>";

        message.EnableTemplateEngine(templateID);

        if (!string.IsNullOrEmpty(category))
          message.SetCategory(category);

        message.EnableOpenTracking();
        message.EnableClickTracking();

        if (includeUnsubscribe)
        {
          var unsubLink = string.Format("http://www.insidefabric.com/unsubscribe.aspx?e={0}&d={1}", email.Base64Encode(), ComputeEmailDigest(email));
          message.Header.AddSubstitution("-unsubscribe-", new string[] { unsubLink });
        }

        // this works - just not needed
        //var uniqueArgs = new Dictionary<string, string>()
        //{
        //    {"ticks", DateTime.Now.Ticks.ToString()},
        //    {"coupon-id", "-couponid-"},
        //};
        //message.AddUniqueArgs(uniqueArgs);

        //message.EnableBypassListManagement();

        var transportWeb = new SendGrid.Web(credentials);
        await transportWeb.DeliverAsync(message);

        return true;
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Send an email created using the Postal template engine.
    /// </summary>
    /// <remarks>
    /// Uses whatever system level sending channel is defined - such as SendGrid.
    /// </remarks>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> SendEmail(EmailTemplate emailTemplate)
    {
      try
      {
        var emailService = new Postal.EmailService();
        var emailMessage = emailService.CreateMailMessage(emailTemplate);

        var emailAddress = emailMessage.To[0].Address;

        // credentials
        var username = WebConfigurationManager.AppSettings["sendgridUserName"];
        var password = WebConfigurationManager.AppSettings["sendgridPassword"];
        var credentials = new NetworkCredential(username, password);

        var message = new SendGrid.SendGridMessage();
        message.AddTo(emailAddress);
        message.From = emailMessage.From;

        message.Subject = emailMessage.Subject;

        // if includes a plain text version, then add as alternate content.

        var plainTextView = emailMessage.AlternateViews.FirstOrDefault(e => e.ContentType.MediaType == "text/plain");
        var htmlView = emailMessage.AlternateViews.FirstOrDefault(e => e.ContentType.MediaType == "text/html");

        if (plainTextView == null && htmlView == null)
        {
          // comes through here when there is only a single message content type

          // the SampleTextOnlyEmail comes through here - sends correctly.
          // the SampleHtmlOnlyEMail comes there there - sends correctly.

          if (emailMessage.IsBodyHtml)
            message.Html = emailMessage.Body;
          else
            message.Text = emailMessage.Body;
        }
        else
        {
          if (htmlView != null)
          {
            var html = htmlView.ContentStream.StreamToString();

            if (emailTemplate.UsePremailer)
            {
              var premailerResult = PreMailer.Net.PreMailer.MoveCssInline(html, true);
              html = premailerResult.Html;
            }

            message.Html = html;
          }

          if (plainTextView != null)
          {
            var plainText = plainTextView.ContentStream.StreamToString();
            message.Text = plainText;
          }
        }

        if (!string.IsNullOrEmpty(emailTemplate.SendGridTemplateID))
          message.EnableTemplateEngine(emailTemplate.SendGridTemplateID);

        if (!string.IsNullOrEmpty(emailTemplate.Category))
          message.SetCategory(emailTemplate.Category);

        if (!emailTemplate.SkipOpenTracking)
          message.EnableOpenTracking();

        if (!emailTemplate.SkipClickTracking)
          message.EnableClickTracking();

        if (emailTemplate.IncludeUnsubscribeSubstitution)
        {
          var unsubLink = string.Format("http://www.insidefabric.com/unsubscribe.aspx?e={0}&d={1}", emailAddress.Base64Encode(), ComputeEmailDigest(emailAddress));
          message.Header.AddSubstitution(emailTemplate.UnsubscribePlaceholder, new string[] { unsubLink });
        }

        if (emailTemplate.Substitutions != null)
        {
          foreach (var item in emailTemplate.Substitutions)
            message.Header.AddSubstitution(item.Key, new string[] { item.Value });
        }

        bool isSimulationToLocalFolder = false;
        bool.TryParse(WebConfigurationManager.AppSettings["SendGridSimulationToLocalFolder"], out isSimulationToLocalFolder);
        if (isSimulationToLocalFolder)
        {
          SmtpClient client = new SmtpClient()
          {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = @"C:\Temp\TestMailPickup",
          };
          System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(new MailAddress(message.From.Address, message.From.DisplayName), new MailAddress(message.To[0].Address, message.To[0].DisplayName));

          msg.Subject = message.Subject;

          msg.IsBodyHtml = true;
          msg.Body = message.Html;
          client.Send(msg);
        }
        else
        {
          var transportWeb = new SendGrid.Web(credentials);
          await transportWeb.DeliverAsync(message);
        }
        return true;
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
        return false;
      }
    }


    public async Task<bool> SubscribersViaSendGrid(string listKey, CampaignSubscriberListAction action, string email)
    {
      if (!Regex.IsMatch(email, EmailRegEx) || string.IsNullOrWhiteSpace(listKey))
        return false;

      Debug.WriteLine(string.Format("Subscriber {0} {1}: {2}", action, listKey, email));

#if DEBUG
      if (email.ContainsIgnoreCase("@good.com"))
      {
        await Task.Delay(1);
        RecordNewEmailSubscriber(email);
        return true;
      }
      else
        return false;
#else

            try
            {
                switch (listKey.ToLower())
                {
                    case "master":
                        // add to master marketing list
                        if (action == CampaignSubscriberListAction.Add)
                            return SendGridAddToMarketingList("master", email);

                        // remove from master list
                        if (action == CampaignSubscriberListAction.Remove)
                            return SendGridRemoveFromMarketingList("master", email);

                        break;

                    case "fabric20off":
                        // transactional - send coupon
                        // template engine: ce0822cc-c441-486a-b7b6-9150dc67023b
                        if (action == CampaignSubscriberListAction.Add)
                        {
                            SendGridAddToMarketingList("master", email); // add to master mailing list too
                            return await SendGridTransactionalMessage("ce0822cc-c441-486a-b7b6-9150dc67023b", email, true, "Coupon");

                        }
                        break;

                    case "wallpaper20off":
                        // transactional - send coupon
                        // template engine: ea226a42-25cc-4e17-beac-68e5285183af
                        if (action == CampaignSubscriberListAction.Add)
                        {
                            SendGridAddToMarketingList("master", email); // add to master mailing list too
                            return await SendGridTransactionalMessage("ea226a42-25cc-4e17-beac-68e5285183af", email, true, "Coupon");
                        }
                        break;

                    case "rugs20off":
                        // transactional - send coupon
                        // template engine: 28aff704-20e6-4272-b29c-4694585b7970
                        if (action == CampaignSubscriberListAction.Add)
                        {
                            SendGridAddToMarketingList("master", email); // add to master mailing list too
                            return await SendGridTransactionalMessage("28aff704-20e6-4272-b29c-4694585b7970", email, true, "Coupon");
                        }
                        break;

                    case "swatch":
                        // transactional - send notice about order
                        // template engine: b23cd130-b840-4857-95a8-f1af30a51bf9
                        if (action == CampaignSubscriberListAction.Add)
                            return await SendGridTransactionalMessage("b23cd130-b840-4857-95a8-f1af30a51bf9", email, false, "Transaction");
                        break;

                    case "decor20off":
                        // transactional - send coupon
                        // template engine: 86c6bb4b-d13f-4132-8215-8b05f4662c16
                        if (action == CampaignSubscriberListAction.Add)
                        {
                            SendGridAddToMarketingList("master", email); // add to master mailing list too
                            return await SendGridTransactionalMessage("86c6bb4b-d13f-4132-8215-8b05f4662c16", email, true, "Coupon");
                        }
                        break;

                    default:
                        return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Handle some other failure
                Debug.WriteLine(ex.ToString());
                return false;
            }
#endif
    }

    public async Task<bool> Subscribers(string listKey, CampaignSubscriberListAction action, string email)
    {
      return await SubscribersViaSendGrid(listKey, action, email);
    }

    #endregion

    #region Cleanup Operations

    protected virtual void RunSQLCleanup()
    {
      try
      {
        var scriptFilepath = MapPath(string.Format(@"~/App_Data/Sql/{0}Cleanup.sql", StoreKey));
        RunSQLScript(scriptFilepath, timeoutSeconds: 60 * 15);
      }
      catch
      {
      }
    }

    /// <summary>
    /// Purge system of orphan images - main only, no review of productvariant images in /images/variant folder.
    /// </summary>
    /// <param name="bPerformModifications"></param>
    /// <param name="cancelToken"></param>
    /// <param name="reportProgress"></param>
    protected virtual void CleanupOrphanProductImages(bool bPerformModifications, CancellationToken cancelToken, Action<int> reportProgress)
    {
      try
      {
        var dtStart = DateTime.Now;


        if (string.IsNullOrWhiteSpace(PathWebsiteRoot) || !Directory.Exists(PathWebsiteRoot))
          return;

        var si = new SanitizeImages((IWebStore)this, bPerformModifications, cancelToken);
        si.Run();

        Debug.WriteLine("Sanitize Image Results:");

        Debug.WriteLine(string.Format("     Icon files: {0}", si.IconFiles.Count().ToString("N0")));
        Debug.WriteLine(string.Format("     Medium files: {0}", si.MediumFiles.Count().ToString("N0")));
        Debug.WriteLine(string.Format("     Large files: {0}", si.LargeFiles.Count().ToString("N0")));
        Debug.WriteLine(string.Format("     Original files: {0}", si.OriginalFiles.Count().ToString("N0")));
        Debug.WriteLine(string.Format("     Count cleared references: {0}", si.CountClearedReferences.ToString("N0")));
        Debug.WriteLine(string.Format("     Count sanitized: {0}", si.CountSanitizedJpg.ToString("N0")));
        Debug.WriteLine(string.Format("     Count duplicates: {0}", si.DuplicateFilenames.Count().ToString("N0")));

        Debug.WriteLine(string.Format("Time to complete: {0}", DateTime.Now - dtStart));
        Debug.WriteLine("Done.");
      }
      catch (Exception Ex)
      {
        Debug.WriteLine("Exception" + Ex.Message);
      }
    }

    /// <summary>
    /// Remove any orphan images for collections.
    /// </summary>
    /// <param name="bPerformModifications"></param>
    /// <param name="cancelToken"></param>
    /// <param name="reportProgress"></param>
    protected virtual void CleanupOrphanCollectionImages(bool bPerformModifications, CancellationToken cancelToken, Action<int> reportProgress)
    {
      try
      {
        var dtStart = DateTime.Now;

        if (string.IsNullOrWhiteSpace(PathWebsiteRoot) || !Directory.Exists(PathWebsiteRoot))
          return;

        var si = new SanitizeCollectionImages((IWebStore)this, bPerformModifications, cancelToken);
        si.Run();

        Debug.WriteLine(string.Format("Deleted orphan collection images: {0:N0}", si.CountDeletedFiles));
        Debug.WriteLine(string.Format("Time to complete: {0}", DateTime.Now - dtStart));
        Debug.WriteLine("Done.");
      }
      catch (Exception Ex)
      {
        Debug.WriteLine("Exception" + Ex.Message);
      }
    }


    #endregion

    #region Tickler Campaigns

    public void NewOrderNotification(int orderNumber)
    {
      // only have logic for IF and IW
      if (!(StoreKey == StoreKeys.InsideFabric || StoreKey == StoreKeys.InsideWallpaper))
        return;

      if (!IsTicklerCampaignsEnabled || TicklerCampaignsManager == null)
        throw new Exception("Tickler camapaigns not enabled.");

      int customerID = 0;
      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        var order = dc.Orders.Where(e => e.OrderNumber == orderNumber).Select(e => new { e.OrderNumber, e.CustomerID }).FirstOrDefault();
        if (order == null)
          throw new Exception(string.Format("Order {0} does not exist.", orderNumber));
        customerID = order.CustomerID;
      }

      TicklerCampaignsManager.NewOrderNotification(customerID, orderNumber);
    }

    public void PauseTicklerCampaignProcessing()
    {
      IsTicklerCampaignQueueProcessingPaused = true;
    }

    public void ResumeTicklerCampaignProcessing()
    {
      IsTicklerCampaignQueueProcessingPaused = false;
    }

    public void MoveStagedTicklerCampaignsToRunning()
    {
      using (var dc = new AspStoreDataContext(ConnectionString))
      {
        dc.TicklerCampaigns.MoveStagedTicklerCampaignsToRunning();
      }
    }

    public void DeleteStagedTicklerCampaigns()
    {
      using (var dc = new AspStoreDataContext(ConnectionString))
      {
        dc.TicklerCampaigns.DeleteByStatus(TicklerCampaignStatus.Staged);
      }
    }


    public void SuspendRunningTicklerCampaigns()
    {
      using (var dc = new AspStoreDataContext(ConnectionString))
      {
        dc.TicklerCampaigns.SuspendRunningTicklerCampaigns();
      }
    }

    public void ResumeSuspendedTicklerCampaigns()
    {
      using (var dc = new AspStoreDataContext(ConnectionString))
      {
        dc.TicklerCampaigns.ResumeSuspendedTicklerCampaigns();
      }
    }


    [RunStoreAction("StageTicklerLongTimeNoSee")]
    public void StageTicklerLongTimeNoSee(string tag, CancellationToken cancelToken)
    {
      MaintenanceHub.NotifyRunStoreActionStatus("Staging tickler records...");

      // the tag is actually a json object with input parameters

      dynamic jsonObj = JValue.Parse(tag);
      DateTime startDate = jsonObj.startDate;
      DateTime endDate = jsonObj.endDate;
      endDate += TimeSpan.FromDays(1);

      int minimumDaysSinceLastEmail = jsonObj.minimumDaysSinceLastEmail;

      StageTicklerLongTimeNoSeeWorker(startDate, endDate, minimumDaysSinceLastEmail, cancelToken, (pct) =>
      {
        NotifyStoreActionProgress(pct);
      }, (status) =>
      {
        MaintenanceHub.NotifyRunStoreActionStatus(status);
      });

      NotifyStoreActionProgress(100);
    }

    /// <summary>
    /// Actual work for populating TicklerCampaigns table with appropriate records per the inputs.
    /// </summary>
    /// <remarks>
    /// Take records whereby user visited our website within specified date range AND has not rec'd a similar email from us within so many days.
    /// </remarks>
    /// <param name="startDate">Filter for last website visit</param>
    /// <param name="endDate">Filter for last website visit</param>
    /// <param name="minimumDaysSinceLastEmail">Don't include anyone if they already rec'd email within this many days of type:abandonedCart</param>
    /// <param name="cancelToken"></param>
    /// <param name="reportProgress"></param>
    /// <param name="reportStatus"></param>
    public void StageTicklerLongTimeNoSeeWorker(DateTime startDate, DateTime endDate, int minimumDaysSinceLastEmail, CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
    {
      int lastReportedProgressPct = 0;
      int countTotal = 100; // gets updated below with real count

      Action<int> notifyProgressAndRemaining = (completed) =>
      {
        var pct = countTotal == 0 ? 0 : (completed * 100) / countTotal;

        lastReportedProgressPct = pct;
        if (reportProgress != null)
          reportProgress(lastReportedProgressPct);
      };

      Action<string> sendStatus = (msg) =>
      {
        if (reportStatus == null)
          return;

        reportStatus(msg);
      };


      int countCompleted = 0;
      notifyProgressAndRemaining(countCompleted);
      sendStatus("Initializing...");
      System.Threading.Thread.Sleep(1000);

      var listCustomers = new List<int>();

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        // customers having last accessed our site between date range AND still has bookmarks or cart items
        listCustomers = (from c in dc.Customers
                         where c.UpdatedOn >= startDate && c.UpdatedOn < endDate && c.Email != ""
                         let hasCartItems = dc.ShoppingCarts.Where(e => e.CustomerID == c.CustomerID).Count() > 0
                         where hasCartItems == true
                         select c.CustomerID).Distinct().ToList();
      }
      listCustomers.Shuffle(); // mostly so any dead emails will be spread out so we don't get spam rating

      countTotal = listCustomers.Count();

      Debug.WriteLine(string.Format("StageTicklerLongTimeNoSeeWorker customer set: {0:N0}", countTotal));

      sendStatus(string.Format("Customer count: {0:N0}", countTotal));
      System.Threading.Thread.Sleep(3000);


      sendStatus(string.Format("Staging {0:N0} customers...", countTotal));

      var rollingTriggerTime = DateTime.Now + TimeSpan.FromMinutes(60);
      int finalCount = 0;

      foreach (var customerID in listCustomers)
      {
        if (cancelToken.IsCancellationRequested)
          break;


        try
        {
          if (TicklerCampaignsManager.CreateLongTimeNoSeeCampaign(customerID, minimumDaysSinceLastEmail, rollingTriggerTime).Result == true)
          {
            finalCount++;
            rollingTriggerTime += TimeSpan.FromMilliseconds(4000);
          }
        }
        catch (Exception Ex)
        {
          Debug.WriteLine(Ex.Message);
        }

        countCompleted++;
        notifyProgressAndRemaining(countCompleted);
      }

      countCompleted = 100;
      notifyProgressAndRemaining(countCompleted);
      sendStatus("Finished!");
      System.Threading.Thread.Sleep(2000);

      sendStatus(string.Format("Final count: {0:N0} customers.", finalCount));
      System.Threading.Thread.Sleep(5000);

    }



    [RunStoreAction("StageSwatchOnlyBuyers")]
    public void StageSwatchOnlyBuyers(string tag, CancellationToken cancelToken)
    {
      if (!(this.storeKey == StoreKeys.InsideFabric || this.storeKey == StoreKeys.InsideWallpaper))
        throw new Exception("This action only valid for InsideFabric and InsideWallpaper.");

      MaintenanceHub.NotifyRunStoreActionStatus("Staging tickler records...");

      // the tag is actually a json object with input parameters

      dynamic jsonObj = JValue.Parse(tag);
      DateTime startDate = jsonObj.startDate;
      DateTime endDate = jsonObj.endDate;
      endDate += TimeSpan.FromDays(1);

      int minimumDaysSinceLastEmail = jsonObj.minimumDaysSinceLastEmail;

      StageSwatchOnlyBuyersWorker(startDate, endDate, minimumDaysSinceLastEmail, cancelToken, (pct) =>
      {
        NotifyStoreActionProgress(pct);
      }, (status) =>
      {
        MaintenanceHub.NotifyRunStoreActionStatus(status);
      });

      NotifyStoreActionProgress(100);
    }

    /// <summary>
    /// Actual work for populating TicklerCampaigns table with appropriate records per the inputs.
    /// </summary>
    /// <remarks>
    /// Take records whereby user visited our website within specified date range AND has not rec'd a similar email from us within so many days.
    /// </remarks>
    /// <param name="startDate">Filter for last website visit</param>
    /// <param name="endDate">Filter for last website visit</param>
    /// <param name="minimumDaysSinceLastEmail">Don't include anyone if they already rec'd email within this many days of type:abandonedCart</param>
    /// <param name="cancelToken"></param>
    /// <param name="reportProgress"></param>
    /// <param name="reportStatus"></param>
    public void StageSwatchOnlyBuyersWorker(DateTime startDate, DateTime endDate, int minimumDaysSinceLastEmail, CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
    {
      int lastReportedProgressPct = 0;
      int countTotal = 100; // gets updated below with real count

      Action<int> notifyProgressAndRemaining = (completed) =>
      {
        var pct = countTotal == 0 ? 0 : (completed * 100) / countTotal;

        lastReportedProgressPct = pct;
        if (reportProgress != null)
          reportProgress(lastReportedProgressPct);
      };

      Action<string> sendStatus = (msg) =>
      {
        if (reportStatus == null)
          return;

        reportStatus(msg);
      };


      int countCompleted = 0;
      notifyProgressAndRemaining(countCompleted);
      sendStatus("Initializing...");
      System.Threading.Thread.Sleep(1000);

      var listCustomers = new List<int>();

      using (var dc = new AspStoreDataContextReadOnly(ConnectionString))
      {
        // basic starting list to spin through is any customer who purchased a swatch between date range
        // cannot use e.OrderedProductVariantName=="Swatch" since LINQ cannot evaluate NTEXT columns
        listCustomers = (from c in dc.Customers
                         where c.Email != ""
                         let hasSwatchPurchases = dc.Orders_ShoppingCarts.Where(e => e.CustomerID == c.CustomerID && e.CreatedOn >= startDate && e.CreatedOn < endDate && e.Quantity == 1 && e.FreeShipping == 1 && e.OrderedProductPrice.GetValueOrDefault() < 10M).Count() > 0
                         where hasSwatchPurchases == true
                         select c.CustomerID).Distinct().ToList();
      }

      listCustomers.Shuffle(); // mostly so any dead emails will be spread out so we don't get spam rating

      countTotal = listCustomers.Count();

      Debug.WriteLine(string.Format("StageSwatchOnlyBuyersWorker customer set: {0:N0}", countTotal));

      sendStatus(string.Format("Customer count: {0:N0}", countTotal));
      System.Threading.Thread.Sleep(3000);


      sendStatus(string.Format("Staging {0:N0} customers...", countTotal));

      var rollingTriggerTime = DateTime.Now + TimeSpan.FromMinutes(2);
#if !DEBUG
            rollingTriggerTime += TimeSpan.FromMinutes(120);
#endif

      int finalCount = 0;

      foreach (var customerID in listCustomers)
      {
        if (cancelToken.IsCancellationRequested)
          break;

        try
        {
          if (TicklerCampaignsManager.CreateSwatchOnlyBuyerCampaign(customerID, minimumDaysSinceLastEmail, rollingTriggerTime).Result == true)
          {
            finalCount++;
            rollingTriggerTime += TimeSpan.FromMilliseconds(4000);
          }
        }
        catch (Exception Ex)
        {
          Debug.WriteLine(Ex.Message);
        }

        countCompleted++;
        notifyProgressAndRemaining(countCompleted);
      }

      countCompleted = 100;
      notifyProgressAndRemaining(countCompleted);
      sendStatus("Finished!");
      System.Threading.Thread.Sleep(2000);

      sendStatus(string.Format("Final count: {0:N0} customers.", finalCount));
      System.Threading.Thread.Sleep(5000);
    }


    public bool IsEmailSuppressed(string email)
    {
      return emailSupressionList.Contains(email.ToLower());
    }

    #endregion

    public byte[] ResizeUploadedPhotoAsSquare(string guid, int size)
    {
      // always returns jpg
      var filepath = FindUploadedPhotoFilepath(guid);

      if (filepath == null)
        throw new HttpException(404, "Not Found");

      var imageBytes = filepath.ReadBinaryFile();
      var resizedBytes = imageBytes.ResizeImageAsSquare(size);
      return resizedBytes;
    }

    protected void LogSystemEvent(string msg)
    {
      new WebsiteApplicationLifetimeEvent(msg, this, WebsiteEventCode.Notification).Raise();
    }

    public List<ProductCountsByManufacturerMetric> GetProductCountsByManufacturerMetrics()
    {
      return ProductCountsByManufacturerMetric.GetMetrics(this as IWebStore);
    }

  }
}
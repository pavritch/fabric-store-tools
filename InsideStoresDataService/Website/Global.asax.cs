using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Optimization;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShopifySharp;

namespace Website
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private string serviceHostName;
        private CompositionContainer container;
        private static MvcApplication _current;
        private static readonly List<SimpleWorkerServiceBase> serviceThreads = new List<SimpleWorkerServiceBase>();
        private static readonly List<IShutdownNotify> registeredShutdownNotifications = new List<IShutdownNotify>();
        private static readonly Dictionary<StoreKeys, IWebStore> webStores = new Dictionary<StoreKeys, IWebStore>();
        private static AppFeedManager feedManager;
        private static RobotsManager robotsManager;
        private static ShareASaleManager shareASaleManager;
        private static ShopifyStore shopifyStore;

        /// <summary>
        /// For debugging when we don't init some stores to save time.
        /// </summary>
        /// <remarks>
        /// Never true for release or live.
        /// </remarks>
        private static bool bSomeStoresMissing = false;

        /// <summary>
        /// Mostly for debugging. True when initial population of all enabled stores completed.
        /// </summary>
        private static bool bAllStoresPopulated = false;

        #region Properties

        public static MvcApplication Current
        {
            get { return _current; }
        }

        public CompositionContainer Container
        {
            get
            {
                return container;
            }
        }

        public string ServiceHostName
        {
            get { return serviceHostName; }
        }

        #endregion

        public IWebStore GetWebStore(StoreKeys storeKey)
        {
            IWebStore store = null;
            webStores.TryGetValue(storeKey, out store);
            return store;
        }

        public Dictionary<StoreKeys, IWebStore> WebStores
        {
            get { return webStores; }
        }

        public ShopifyStore ShopifyStore
        {
            get { return shopifyStore; }
        }

        public IRobotsManager RobotManager
        {
            get { return robotsManager; }
        }

        public IShareASaleManager ShareASaleManager
        {
            get { return shareASaleManager; }
        }

        public AppFeedManager FeedManager
        {
            get { return feedManager; }
        }

        public bool IsSomeStoresMissing
        {
            get
            {
                return bSomeStoresMissing;
            }
        }

        public bool IsAllStoresPopulated
        {
            get
            {
                return bAllStoresPopulated;
            }
        }

        private void InitializeMEF()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            var catalog = new DirectoryCatalog(path);
            container = new CompositionContainer(catalog);
            DependencyResolver.SetResolver(new MEFDependencyResolver(container));
        }


        private void InitializeWebStores()
        {


            foreach (var storeKey in Gen4.Util.Misc.LibEnum.GetNames<StoreKeys>())
            {
                try
                {
                    bool isEnabled = false;
                    bool.TryParse(WebConfigurationManager.AppSettings[string.Format("{0}Enabled", storeKey.ToString())], out isEnabled);

                    if (!isEnabled)
                        continue;

                    var store = Container.GetExportedValues<IWebStore>(storeKey).First();
                    webStores.Add(store.StoreKey, store);
                }
                catch(Exception Ex)
                {
                    LogException(storeKey, Ex);
                    Debug.WriteLine(Ex.Message);
                }
            }

            // so can display a warning on home/index page as a reminder
            bSomeStoresMissing = webStores.Count() != Gen4.Util.Misc.LibEnum.GetNames<StoreKeys>().Count();

            // use a new thread to get the initial population completed

            var thWorker = new Thread(new ThreadStart(() =>
            {
                // run through them serially so we do not overwhelm the server
                // after a little sleep to let the rest of the system fire up

                Thread.Sleep(2000);
                var timeStart = DateTime.Now;
                foreach (var s in webStores)
                    s.Value.RepopulateProducts();
                bAllStoresPopulated = true;
                Debug.WriteLine("Total time to populate products: {0}", DateTime.Now - timeStart);
            }));
            thWorker.Start();
        }


        protected void Application_Start()
        {
            _current = this;
            Debug.WriteLine("Application_Start()");

            // return enums as strings 
            SerializeSettings(GlobalConfiguration.Configuration);

            serviceHostName = WebConfigurationManager.AppSettings["ServiceHostName"];
            if (string.IsNullOrWhiteSpace(serviceHostName))
                throw new Exception("AppSettings ServiceHostName missing.");

            new WebsiteApplicationLifetimeEvent("StoresDataService started.", this, WebsiteEventCode.ApplicationStart).Raise();

            InitializeMEF();

            Dump.AddHiddenProperty(typeof(ProductQueryBase), "CompletedAction");

            // helps image processing when HTTPS 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;

#if !DEBUG
            serviceThreads.Add(new StockCheckNotificationService());
#endif
            serviceThreads.Add(new TicklerCampaignsService());
            serviceThreads.ForEach(service => service.Start());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            InitializeWebStores();

            // master enable for shopify must be true else don't even fire up the class, so will
            // be as if never existed.
            shopifyStore = null;
            if (bool.Parse(WebConfigurationManager.AppSettings["ShopifySupportEnabled"]))
                shopifyStore = new ShopifyStore(WebStores);

            registeredShutdownNotifications.Add(shopifyStore);

            feedManager = new AppFeedManager();
            robotsManager = new RobotsManager(WebConfigurationManager.AppSettings["RobotsDataRootPath"]);
            shareASaleManager = new ShareASaleManager(WebConfigurationManager.AppSettings["ShareASaleRootPath"]);

#if false
            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(30 * 1000);
                    var store = WebStores[StoreKeys.InsideWallpaper];
                    store.ProductFeedManager.Generate(ProductFeedKeys.Google);
                    store.ProductFeedManager.Generate(ProductFeedKeys.GoogleCanada);
                    store = WebStores[StoreKeys.InsideFabric];
                    store.ProductFeedManager.Generate(ProductFeedKeys.Google);
                    store.ProductFeedManager.Generate(ProductFeedKeys.GoogleCanada);
                });
#endif

        }


        #region Application Init


        /// <summary>
        /// Factory initialization. Called once per HttpApplication instance created. Could be
        /// many created since engine factory keeps a pool of HttpApplication objects.
        /// </summary>
        public override void Init()
        {
            Debug.WriteLine("HttpApplication Init.");
            base.Init();

            //#if DEBUG
            //            // All modules have been loaded up
            //            foreach (var mod in this.Modules)
            //            {
            //                // print out the name that appears in <httpModules> name attribute
            //                Debug.WriteLine(String.Format("Loaded module: {0}", mod));
            //            }
            //#endif
#if !DEBUG
            // make sure compilation not set for debug
            var compSection = (CompilationSection)WebConfigurationManager.GetWebApplicationSection("system.web/compilation");
            if (compSection.Debug)
                HealthEvents.RaiseConfigurationError(this, "Web.config compilation mode set to DEBUG for release build.");

#endif
        }


        private void LogException(string storeName, Exception Ex)
        {
            var msg = string.Format("InsideStores Data Service\nUnhandled exception initializing store: {0}", storeName);
            var ev = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
            ev.Raise();
        }

        private void SerializeSettings(HttpConfiguration config)
        {
            // these settings apply ONLY when the JsonNetResult class is used,
            // not when JsonResult!

            // do the enum string thing individually for safety

            //var jsonSetting = new JsonSerializerSettings();
            //jsonSetting.Converters.Add(new StringEnumConverter());
            //config.Formatters.JsonFormatter.SerializerSettings = jsonSetting;
        }

        #endregion

        #region Application End
        protected void Application_End(object sender, EventArgs e)
        {
            // gracefully terminate each of the registered service threads

            // it is possible that the invocation of this method might not be on the exact
            // same HttpApplication object instance as for start call, so such should be taken
            // into account for any needed access to state data.

            serviceThreads.ForEach(service => service.Stop());

            // alert all other classes wishing to be notified that we're about to end
            registeredShutdownNotifications.ForEach(f => f.Stop());

            // in case the store is indpendently running something
            foreach (var store in WebStores.Values)
                store.CancelBackgroundTask();

            // log shutdown event                
            new WebsiteApplicationLifetimeEvent("StoresDataService stopped.", this, WebsiteEventCode.ApplicationStop).Raise();
        }
        #endregion

    }
}
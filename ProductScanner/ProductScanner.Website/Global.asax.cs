using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Caching;
using ProductScanner.Core.Config;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.LoadTesting;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.Aggregators;
using ProductScanner.Core.StockChecks.Caching;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.StockCheckManagers;
using ProductScanner.Data;
using ProductScanner.StoreData;
using ProductScanner.Website.Controllers;
using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;

namespace ProductScanner.Website
{
    public class WebApiApplication : HttpApplication
    {
        private Container ConfigureContainer()
        {
            var pluginFolder = ConfigurationManager.AppSettings["VendorModulePluginsFolder"];
            var pluginAssemblies = new DirectoryInfo(pluginFolder).GetFiles()
                .Where(x => x.Extension.ToLower() == ".dll")
                .Select(x => Assembly.LoadFile(x.FullName)).ToList();

            Vendor.SetVendors(pluginAssemblies);

            var container = new Container();
            container.Register<IAppSettings, AppSettings>();
            container.Register<IVendorStockCheckMediator, VendorStockCheckMediator>();
            container.Register<IVendorSessionMediator, VendorSessionMediator>();
            container.Register<IVendorPerformanceMediator, VendorPerformanceMediator>();

            container.Register<ILoadTester, LoadTester>();
            container.Register<IRandomRequestGenerator, RandomRequestGenerator>();

            container.RegisterOpenGeneric(typeof(IStockCheckManager<>), typeof(StockCheckManager<>));
            container.RegisterDecorator(typeof(IStockCheckManager<>), typeof(SqlLoadStockCheckManager<>));
            container.RegisterDecorator(typeof(IStockCheckManager<>), typeof(CachedStockCheckManager<>));
            container.RegisterDecorator(typeof(IStockCheckManager<>), typeof(MonitoredStockCheckManager<>));

            container.RegisterOpenGeneric(typeof(IStoreDatabase<>), typeof(BaseStoreDatabase<>));
            container.Register<IPlatformDatabase, PlatformDatabase>();

            container.RegisterManyForOpenGeneric(typeof(IVendorAuthenticator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IVendorAuthenticator<>), typeof(NullVendorAuthenticator<>));

            container.Register<IMemoryCacheService, MemoryCacheService>(Lifestyle.Singleton);
            container.RegisterOpenGeneric(typeof(IStorePerformanceMonitor<>), typeof(StorePerformanceMonitor<>), Lifestyle.Singleton);
            container.RegisterOpenGeneric(typeof(IVendorStatusManager<>), typeof(VendorStatusManager<>), Lifestyle.Singleton);
            container.RegisterOpenGeneric(typeof(IVendorPerformanceMonitor<>), typeof (VendorPerformanceMonitor<>), Lifestyle.Singleton);
            container.RegisterOpenGeneric(typeof(IVendorSessionManager<>), typeof(VendorSessionManager<>), Lifestyle.Singleton);

            container.RegisterManyForOpenGeneric(typeof(IStockChecker<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IStockChecker<>), typeof(NullStockChecker<>));

            container.RegisterManyForOpenGeneric(typeof(IStockCheckAggregator<>), typeof(IStockCheckAggregator<>).Assembly);
            container.RegisterOpenGeneric(typeof(IStockCheckAggregator<>), typeof(StockCheckAggregator<>));
            container.RegisterDecorator(typeof(IStockCheckAggregator<>), typeof(PreFlightStockCheckAggregator<>));
            container.RegisterDecorator(typeof(IStockCheckAggregator<>), typeof(AuthenticatingStockCheckAggregator<>));
            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            return container;
        }

        protected void Application_Start()
        {
            var container = ConfigureContainer();
            container.GetInstance<IAppSettings>().Validate();

            var activator = new SimpleInjectorHubActivator(container);
            GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => activator);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            // return enums as strings - might want to do this on a per-enum basis
            SerializeSettings(GlobalConfiguration.Configuration);
        }

        private void SerializeSettings(HttpConfiguration config)
        {
            var jsonSetting = new JsonSerializerSettings();
            jsonSetting.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.SerializerSettings = jsonSetting;
        }
    }
}

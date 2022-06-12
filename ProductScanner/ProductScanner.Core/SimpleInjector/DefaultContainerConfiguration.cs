using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Config;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Correlators;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Reports;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.Aggregators;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.StockCheckManagers;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using ProductScanner.Core.WebClient;
using SimpleInjector;
using SimpleInjector.Extensions;
using Utilities;

namespace ProductScanner.Core.SimpleInjector
{
    public static class DefaultContainerConfiguration
    {
        public static Container BuildContainer()
        {
            var pluginFolder = ConfigurationManager.AppSettings["VendorModulePluginsFolder"];
            var pluginAssemblies = new DirectoryInfo(pluginFolder).GetFiles()
                .Where(x => x.Extension.ToLower() == ".dll")
                .Select(x => Assembly.LoadFile(x.FullName)).ToList();

            Vendor.SetVendors(pluginAssemblies);

            var container = new Container();
            container.Register<IVendorStockCheckMediator, VendorStockCheckMediator>();
            container.Register<IVendorSessionMediator, VendorSessionMediator>();
            container.Register<IVendorPerformanceMediator, VendorPerformanceMediator>();
            container.Register<IDesignerFileLoader, DesignerFileLoader>();

            container.Register<IAuthenticationTester, AuthenticationTester>();

            container.Register<IVendorRunnerMediator, VendorRunnerMediator>();

            // singleton per closed generic type
            container.RegisterOpenGeneric(typeof(IVendorPerformanceMonitor<>), typeof(VendorPerformanceMonitor<>), Lifestyle.Singleton);
            container.RegisterOpenGeneric(typeof(IVendorSessionManager<>), typeof(VendorSessionManager<>), Lifestyle.Singleton);

            container.RegisterOpenGeneric(typeof(IVendorRunner<>), typeof(VendorRunner<>));
            container.RegisterOpenGeneric(typeof(IVendorScanner<>), typeof(VendorScanner<>));
            container.RegisterOpenGeneric(typeof(ICommitDataBuilder<>), typeof(CommitDataBuilder<>));
            container.RegisterOpenGeneric(typeof(ICommitValidator<>), typeof(CommitValidator<>));
            container.RegisterOpenGeneric(typeof(ICommitSubmitter<>), typeof(CommitSubmitter<>));
            container.RegisterOpenGeneric(typeof(IStoreDatabaseFactory<>), typeof(StoreDatabaseFactory<>));

            //container.RegisterOpenGeneric(typeof(IVendorScanner<>), typeof(VendorScanner<>));
            container.RegisterOpenGeneric(typeof(IPageFetcher<>), typeof(PageFetcher<>));
            container.RegisterOpenGeneric(typeof(IStorageProvider<>), typeof(FileStorageProvider<>));
            container.RegisterOpenGeneric(typeof(IAuditFileCreator<>), typeof(AuditFileCreator<>));
            container.Register<IWebClientFactory, WebClientFactory>();

            container.RegisterOpenGeneric(typeof(ICheckpointService<>), typeof(DatabaseCheckpointService<>));
            container.Register<IVendorTestMediator, VendorTestMediator>();

            container.RegisterOpenGeneric(typeof(IVendorTestRunner<>), typeof(VendorTestRunner<>));
            container.RegisterOpenGeneric(typeof(IImageValidator<>), typeof(ImageValidator<>));

            container.RegisterManyForOpenGeneric(typeof(IImageChecker<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IImageChecker<>), typeof(NullImageChecker<>));

            container.RegisterManyForOpenGeneric(typeof(IProductFileLoader<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IProductFileLoader<>), typeof(NullProductFileLoader<>));

            container.RegisterManyForOpenGeneric(typeof(IProductScraper<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IProductScraper<>), typeof(NullProductScraper<>));

            container.RegisterManyForOpenGeneric(typeof(IProductBuilder<>), pluginAssemblies);

            container.RegisterManyForOpenGeneric(typeof(IPriceCalculator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IPriceCalculator<>), typeof(DefaultPriceCalculator<>));

            container.RegisterManyForOpenGeneric(typeof(IFullUpdateChecker<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IFullUpdateChecker<>), typeof(NullFullUpdateChecker<>));

            container.RegisterManyForOpenGeneric(typeof(IProductDiscoverer<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IProductDiscoverer<>), typeof(NullProductDiscoverer<>));

            container.RegisterManyForOpenGeneric(typeof(IMetadataCollector<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IMetadataCollector<>), typeof(NullMetadataCollector<>));
            container.RegisterDecorator(typeof(IMetadataCollector<>), typeof(ErrorCheckMetadataCollector<>));

            container.RegisterManyForOpenGeneric(typeof(IProductValidator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IProductValidator<>), typeof(DefaultProductValidator<>));

            container.RegisterManyForOpenGeneric(typeof(IVariantValidator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IVariantValidator<>), typeof(DefaultVariantValidator<>));

            container.RegisterManyForOpenGeneric(typeof(IVariantMerger<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IVariantMerger<>), typeof(NullVariantMerger<>));

            container.RegisterManyForOpenGeneric(typeof(IStockChecker<>), pluginAssemblies);

            container.RegisterManyForOpenGeneric(typeof(IVendorAuthenticator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IVendorAuthenticator<>), typeof(NullVendorAuthenticator<>));

            container.RegisterOpenGeneric(typeof(IVendorScanSessionManager<>), typeof(VendorScanSessionManager<>), Lifestyle.Singleton);

            container.RegisterOpenGeneric(typeof(ICorrelatorSetter<>), typeof(CorrelatorSetter<>));

            container.Register<IFtpClient, FtpClient>();
            container.Register<IAppSettings, AppSettings>();

            var nonGenericSubscribers = OpenGenericBatchRegistrationExtensions
                .GetTypesToRegister(container,
                    typeof(IVendorTest<>),
                    AccessibilityOption.PublicTypesOnly,
                    pluginAssemblies);

            container.RegisterManyForOpenGeneric(typeof(IVendorTest<>),
                container.RegisterAll,
                nonGenericSubscribers);

            container.RegisterManyForOpenGeneric(typeof(IStockCheckAggregator<>), typeof(IStockCheckAggregator<>).Assembly);
            container.RegisterOpenGeneric(typeof(IStockCheckAggregator<>), typeof(StockCheckAggregator<>));
            container.RegisterDecorator(typeof(IStockCheckAggregator<>), typeof(PreFlightStockCheckAggregator<>));
            container.RegisterDecorator(typeof(IStockCheckAggregator<>), typeof(AuthenticatingStockCheckAggregator<>));

            return container;
        }
    }
}

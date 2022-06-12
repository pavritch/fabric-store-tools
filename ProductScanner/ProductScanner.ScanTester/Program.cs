using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.Classes.Helpers;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.Scanning;
using FabricUpdater.Core.Scanning.Checkpoints;
using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.Scraping;
using FabricUpdater.Data;
using SimpleInjector;
using SimpleInjector.Extensions;
using Website.Data.Models;

namespace FabricUpdater.ScanTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ConfigureContainer();
            var vendors = Vendor.GetAll();

            var scanner = typeof (IVendorRunner<>).MakeGenericType(vendors.Last().GetType());
            var updater = container.GetInstance(scanner) as IVendorRunner;
            updater.RunAsync();

            Console.ReadKey();
        }

        private static Container ConfigureContainer()
        {
            var pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vendors");
            var pluginAssemblies = new DirectoryInfo(pluginFolder).GetFiles()
                .Where(x => x.Extension.ToLower() == ".dll")
                .Select(x => Assembly.LoadFile(x.FullName)).ToList();
            var exportedTypes = pluginAssemblies.SelectMany(x => x.GetTypes()).ToList();

            var container = new Container();
            container.RegisterOpenGeneric(typeof(IVendorRunner<>), typeof(VendorRunner<>));
            //container.RegisterOpenGeneric(typeof(IVendorScanner<>), typeof(VendorScanner<>));
            container.RegisterOpenGeneric(typeof(IPageFetcher<>), typeof(PageFetcher<>));
            container.RegisterOpenGeneric(typeof(IStorageProvider<>), typeof(StorageProvider<>));
            container.Register<IWebClientFactory, WebClientFactory>();

            container.RegisterOpenGeneric(typeof(ICheckpointService<>), typeof(MemoryCheckpointService<>), Lifestyle.Singleton);

            container.RegisterManyForOpenGeneric(typeof(IProductFileLoader<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IProductFileLoader<>), typeof(NullProductFileLoader<>));

            container.RegisterManyForOpenGeneric(typeof(IProductScraper<>), pluginAssemblies);
            container.RegisterManyForOpenGeneric(typeof(IProductDiscoverer<>), pluginAssemblies);
            container.RegisterManyForOpenGeneric(typeof(IMetadataCollector<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IMetadataCollector<>), typeof(NullMetadataCollector<>));

            container.RegisterManyForOpenGeneric(typeof(IVendorAuthenticator<>), pluginAssemblies);
            container.RegisterOpenGeneric(typeof(IVendorAuthenticator<>), typeof(NullVendorAuthenticator<>));

            container.Register<IPlatformDatabase, PlatformDatabase>();

            container.Verify();
            return container;
        }
    }
}

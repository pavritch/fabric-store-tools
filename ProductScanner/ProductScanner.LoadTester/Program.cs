using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.LoadTesting;
using ProductScanner.StoreData;
using SimpleInjector;
using SimpleInjector.Extensions;

namespace ProductScanner.LoadTester
{
    class Program
    {
        private static int _numThreads = 10;
        private static int _maxVendorsPerRequest = 4;
        private static int _maxProductsPerVendor = 4;

        static void Main()
        {
            var container = ConfigureContainer();

            var loadTester = container.GetInstance<ILoadTester>();
            loadTester.RunAgainstAPI(_numThreads, _maxVendorsPerRequest, _maxProductsPerVendor);

            Console.WriteLine("Load Test Running with {0} threads", _numThreads);
            Console.ReadKey();
        }

        private static Container ConfigureContainer()
        {
            var pluginFolder = ConfigurationManager.AppSettings["VendorModulePluginsFolder"];
            var pluginAssemblies = new DirectoryInfo(pluginFolder).GetFiles()
                .Where(x => x.Extension.ToLower() == ".dll")
                .Select(x => Assembly.LoadFile(x.FullName)).ToList();

            Vendor.SetVendors(pluginAssemblies);

            var container = new Container();
            container.Register<IRandomRequestGenerator, RandomRequestGenerator>();
            container.Register<ILoadTester, Core.LoadTesting.LoadTester>();
            container.Register<IAuthenticationTester, AuthenticationTester>();
            container.RegisterOpenGeneric(typeof(IStoreDatabase<>), typeof(BaseStoreDatabase<>));
            return container;
        }
    }
}

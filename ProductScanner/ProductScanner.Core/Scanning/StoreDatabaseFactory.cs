using ProductScanner.Core.DataInterfaces;
using SimpleInjector;

namespace ProductScanner.Core.Scanning
{
    public class StoreDatabaseFactory<T> : IStoreDatabaseFactory<T> where T : Vendor, new()
    {
        private readonly Container _container;
        public StoreDatabaseFactory(Container container)
        {
            _container = container;
        }

        // Gets the correct store database for the vendor that we're working with
        public IStoreDatabase GetStoreDatabase()
        {
            var vendor = new T();
            var storeDatabaseType = typeof (IStoreDatabase<>).MakeGenericType(vendor.Store.GetStore().GetType());
            return _container.GetInstance(storeDatabaseType) as IStoreDatabase;
        }
    }
}
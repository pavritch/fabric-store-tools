using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.Core.Scanning
{
    public interface IStoreDatabaseFactory
    {
        IStoreDatabase GetStoreDatabase();
    }

    public interface IStoreDatabaseFactory<T> : IStoreDatabaseFactory where T : Vendor
    {
    }

}
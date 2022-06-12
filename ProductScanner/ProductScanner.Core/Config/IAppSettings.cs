using System.Threading.Tasks;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.Core.Config
{
    public interface IAppSettings
    {
        int CacheDecayTimeInSeconds { get; }
        int KeepAliveSessionTimeoutInSeconds { get; }
        int MaxSessionTimeInSeconds { get; }
        int VendorQueryDelayInMilliseconds { get; }
        bool LogExternalResponsesToDisk { get; }
        string CacheRoot { get; }
        string StaticRoot { get; }
        void Validate();

        Task<TValue> GetValueAsync<T, TValue>(AppSettingType settingType) where T : Vendor, new();
    }
}
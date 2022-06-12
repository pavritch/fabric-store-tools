using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;

namespace ProductScanner.Core.Config
{
    // configuration class that pulls settings from app.config and/or database
    public class AppSettings : IAppSettings
    {
        private readonly IPlatformDatabase _platformDatabase;

        public AppSettings(IPlatformDatabase platformDatabase)
        {
            _platformDatabase = platformDatabase;
        }

        public int CacheDecayTimeInSeconds
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["CacheDecayTimeInSeconds"]);
            }
        }

        public int KeepAliveSessionTimeoutInSeconds
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["KeepAliveSessionTimeoutInSeconds"]);
            }
        }

        public int MaxSessionTimeInSeconds
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["MaxSessionTimeInSeconds"]);
            }
        }

        public int VendorQueryDelayInMilliseconds
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["VendorQueryDelayInMilliseconds"]);
            }
        }

        public bool LogExternalResponsesToDisk
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["LogExternalResponsesToDisk"]);
            }
        }

        public void Validate()
        {
            var settingKeys = new List<string>
            {
                "CacheDecayTimeInSeconds",
                "KeepAliveSessionTimeoutInSeconds",
                "MaxSessionTimeInSeconds",
                "VendorQueryDelayInMilliseconds",
                "LogExternalResponsesToDisk",
            };
            foreach (var key in settingKeys)
                if (ConfigurationManager.AppSettings[key] == null)
                    throw new ConfigurationErrorsException(key + " not configured.");
        }

        public string CacheRoot
        {
            get { return Convert.ToString(ConfigurationManager.AppSettings["CacheRoot"]); }
        }

        public string StaticRoot
        {
            get { return Convert.ToString(ConfigurationManager.AppSettings["StaticRoot"]); }
        }

        public async Task<TValue> GetValueAsync<T, TValue>(AppSettingType settingType) where T : Vendor, new()
        {
            var vendor = new T();
            var value = ConfigurationManager.AppSettings[settingType.ToString()];
            if (value != null) return (TValue)Convert.ChangeType(ConfigurationManager.AppSettings[settingType.ToString()], typeof(TValue));

            var appSettings = await _platformDatabase.GetConfigSettingsAsync();

            // Check sql for a row for this store and manufacturer
            var setting = appSettings.SingleOrDefault(x => x.Name == settingType.ToString() && 
                x.Store == vendor.Store.ToString() && x.VendorId == vendor.Id);
            if (setting != null) return (TValue) Convert.ChangeType(setting.ConfigValue, typeof (TValue));

            // Check sql for a row for the store
            setting = appSettings.SingleOrDefault(x => x.Name == settingType.ToString() && 
                x.Store == vendor.Store.ToString() && x.VendorId == null);
            if (setting != null) return (TValue) Convert.ChangeType(setting.ConfigValue, typeof (TValue));

            // Check sql for a global row setting (store + manu = null)
            setting = appSettings.SingleOrDefault(x => x.Name == settingType.ToString() && 
                x.Store == null && x.VendorId == null);
            if (setting != null) return (TValue) Convert.ChangeType(setting.ConfigValue, typeof (TValue));

            return default(TValue);
        }
    }
}
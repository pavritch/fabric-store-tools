using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core
{
    public class HomewareVendor : Vendor
    {
        // default to 2.5M Markup
        public HomewareVendor(int id, string displayName, string skuPrefix, decimal ourPriceMarkup = 2.5M, decimal retailPriceMarkup = 4.0M)
            : base(id, displayName, StoreType.InsideAvenue, skuPrefix, StockCapabilities.None, ourPriceMarkup, retailPriceMarkup)
        {
            SwatchesEnabled = false;
            SwatchCost = 0;
            SwatchPrice = 0;

            MinimumCost = 50;
            MinimumPrice = 1;
        }
    }

    public class RugVendor : Vendor
    {
        public RugVendor(int id, string displayName, string skuPrefix, StockCapabilities capabilities = StockCapabilities.None, decimal ourPriceMarkup = 1.4M, decimal retailPriceMarkup = 2.6M)
            : base(id, displayName, StoreType.InsideRugs, skuPrefix, capabilities, ourPriceMarkup, retailPriceMarkup)
        {
            SwatchesEnabled = false;
            SwatchCost = 0;
            SwatchPrice = 0;
        }
    }

    public class Vendor
    {
        public int Id { get; set; }
        // store the vendor belongs to
        public StoreType Store { get; set; }
        public string DisplayName { get; set; }
        public decimal OurPriceMarkup { get; set; }
        public decimal RetailPriceMarkup { get; set; }
        public decimal SwatchCost { get; set; }
        public decimal SwatchPrice { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string LoginUrl { get; set; }
        public string LoginUrl2 { get; set; }
        public string PublicUrl { get; set; }

        public decimal MinimumPrice { get; set; }
        public decimal MinimumCost { get; set; }

        public string SkuPrefix { get; set; }
        public string DiscoveryNotes { get; set; }
        public string DeveloperComments { get; set; }

        public bool HasStockCheckApi { get; set; }
        public bool UsesIMAP { get; set; }
        public bool SwatchesEnabled { get; set; }
        public bool IsFullyImplemented { get; set; }
        public bool IsStockCheckerFunctional { get; set; }
        public bool RunDiscontinuedPercentageCheck { get; set; }

        public bool UsesStaticFiles { get; set; }

        private int _staticFileVersion;
        public int StaticFileVersion
        {
            get
            {
                // by default, return 1 if UsesStaticFiles
                if (_staticFileVersion == 0 && UsesStaticFiles) return 1;
                return _staticFileVersion;
            }
            set { _staticFileVersion = value; }
        }

        public int ThrottleInMs { get; set; }

        public bool ReadyForLive { get; set; }

        public List<ProductGroup> ProductGroups { get; set; }

        public bool IsClearanceSupported { get; set; }

        public StockCapabilities StockCapabilities { get; set; }

        public Vendor() { }
        public Vendor(int id, string displayName, StoreType store, string skuPrefix, StockCapabilities capabilities = StockCapabilities.None, decimal ourPriceMarkup = 1.4M, decimal retailPriceMarkup = 2.6M) // 2.0 * 1.3
        {
            ProductGroups = new List<ProductGroup>();

            Id = id; 
            DisplayName = displayName; 
            Store = store;
            OurPriceMarkup = ourPriceMarkup;
            RetailPriceMarkup = retailPriceMarkup;
            SkuPrefix = skuPrefix;
            SwatchesEnabled = true;
            IsFullyImplemented = true;
            IsStockCheckerFunctional = true;
            RunDiscontinuedPercentageCheck = true;
            StockCapabilities = capabilities;

            MinimumPrice = 10M;
            MinimumCost = 1M;

            SwatchCost = 0M;
            SwatchPrice = 7M;
        }
        private static List<Vendor> _vendors; 

        public string GetName() { return GetType().Name.Replace("Vendor", ""); }

        public string GetVendorModuleFilename()
        {
            return Path.GetFileName(GetType().Assembly.CodeBase);
        }

        public virtual string GetOurPriceMarkupDescription()
        {
            return OurPriceMarkup.ToString();
        }

        public virtual string GetRetailPriceMarkupDescription()
        {
            return RetailPriceMarkup.ToString();
        }

        public string ProductFilePath 
        {
            get { return string.Format(@"{0}\{1}\Static\Price.xlsx", Store, GetName()); }
        }

        public static Vendor GetByDisplayName(string name)
        {
            return GetAll().Single(x => x.DisplayName == name);
        }

        public static Vendor GetByName(string name)
        {
            return GetAll().Single(x => x.GetName() == name);
        }

        public static Vendor GetById(int id)
        {
            return GetAll().Single(x => x.Id == id);
        }

        public static int GetId<T>()
        {
            return GetAll().Single(x => x.GetType() == typeof(T)).Id;
        }

        public static string GetDisplayName<T>()
        {
            return GetAll().Single(x => x.GetType() == typeof(T)).DisplayName;
        }

        // Each time a vendor module is needed we want to use the very latest. We should be 
        // able to see a b-ug, cancel a running operation, update the module DLL, then restart 
        // that vendor – and have it instantiate the new module and go on its way.
        // this has to be triggered manually as far as I know - we need the app to know to unload it's dlls and reload

        // introduces temporal coupling, but I think is the simplest approach
        public static void SetVendors(List<Assembly> pluginAssemblies)
        {
            _vendors = pluginAssemblies
                .SelectMany(s => s.GetTypes())
                .Where(typeof (Vendor).IsAssignableFrom)
                .Where(t => !t.IsAbstract)
                .Where(x => !x.Name.Contains("BaseVendor"))
                .Select(x => Activator.CreateInstance(x) as Vendor)
                .ToList();
        }

        public static List<Vendor> GetAll()
        {
            return _vendors;
        }

        public static List<Vendor> GetByStore(StoreType storeType)
        {
            return GetAll().Where(x => x.Store == storeType).ToList();
        }

        public static List<Vendor> GetByStore<T>() where T : Store
        {
            var storeType = StoreType.InsideFabric;
            if (typeof(T) == typeof(InsideFabricStore)) storeType = StoreType.InsideFabric;
            if (typeof(T) == typeof(InsideWallpaperStore)) storeType = StoreType.InsideWallpaper;
            if (typeof(T) == typeof(InsideRugsStore)) storeType = StoreType.InsideRugs;
            if (typeof(T) == typeof(InsideAvenueStore)) storeType = StoreType.InsideAvenue;
            return GetAll().Where(x => x.Store == storeType).ToList();
        }
    }
}
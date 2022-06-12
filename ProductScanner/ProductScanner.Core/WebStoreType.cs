using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace ProductScanner.Core
{
    public abstract class Store
    {
        public StoreType StoreType { get; set; }
        public string ConnectionStringName { get; set; }

        // Display name for this store. Generally the same as the key.
        public string Name { get { return StoreType.ToString(); } } 

        // Two-letter nickname for store: IF, IW, IR, IH, etc.
        public string ShortName { get; set; }

        // Home page Url - http://www.insidefabric.com
        // No trailing slash. Will be used to craft other URLs. All lower case.
        public string Url { get; set; }

        protected Store(StoreType storeType, string connectionStringName, string shortName, string url)
        {
            StoreType = storeType;
            ConnectionStringName = connectionStringName;
            ShortName = shortName;
            Url = url;
        }
    }

    public class InsideFabricStore : Store
    {
        public InsideFabricStore() : base(StoreType.InsideFabric, "InsideFabricConnectionString", "IF", "http://www.insidefabric.com") { }
    }

    public class InsideWallpaperStore : Store
    {
        public InsideWallpaperStore() : base(StoreType.InsideWallpaper, "InsideWallpaperConnectionString", "IW", "http://www.insidewallpaper.com") { }
    }

    public class InsideRugsStore : Store
    {
        public InsideRugsStore() : base(StoreType.InsideRugs, "InsideRugsConnectionString", "IR", "http://www.insiderugs.com") { }
    }

    public class InsideAvenueStore : Store
    {
        public InsideAvenueStore() : base(StoreType.InsideAvenue, "InsideAvenueConnectionString", "IA", "http://www.insideavenue.com") { }
    }

    public enum StoreType
    {
        InsideFabric,
        InsideWallpaper,
        InsideRugs,
        InsideAvenue
    }

    public static class StoreExtensions
    {
        public static List<Store> GetAll()
        {
            return new List<Store>
            {
                new InsideFabricStore(),
                //new InsideWallpaperStore(),
                //new InsideRugsStore(),
                new InsideAvenueStore()
            };
        }

        public static Store GetStore(this StoreType storeType)
        {
            if (storeType == StoreType.InsideFabric) return new InsideFabricStore();
            if (storeType == StoreType.InsideWallpaper) return new InsideWallpaperStore();
            if (storeType == StoreType.InsideRugs) return new InsideRugsStore();
            if (storeType == StoreType.InsideAvenue) return new InsideAvenueStore();
            throw new Exception();
        }
    }
}
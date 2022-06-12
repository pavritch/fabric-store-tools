using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kravet.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Kravet.SubvendorComponents
{
    public class KravetFileLoader : KravetBaseFileLoader<KravetVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string>
        {
            "KRAVET BASICS", 
            "KRAVET CONTRACT", 
            "KRAVET COUTURE", 
            "KRAVET DESIGN", 
            "KRAVET SMART" 
        }; 

        public KravetFileLoader(IStorageProvider<KravetVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }

        protected override bool ShouldExclude(ScanData data)
        {
            if (base.ShouldExclude(data)) return true;

            var collection = data[ScanField.Collection];
            if (collection.ContainsIgnoreCase("Andrew Martin")) return true;

            return false;
        }
    }

    public class LeeJofaFileLoader : KravetBaseFileLoader<LeeJofaVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string>
        {
            "FIRED EARTH",
            "LEE JOFA",
            "MONKWELL",
        }; 

        public LeeJofaFileLoader(IStorageProvider<LeeJofaVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class AndrewMartinFileLoader : KravetBaseFileLoader<AndrewMartinVendor>
    {
        // pull in all of Kravet Couture then filter for Andrew Martin
        private static readonly List<string> AssociatedBrands = new List<string> { "KRAVET COUTURE" }; 
        public AndrewMartinFileLoader(IStorageProvider<AndrewMartinVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            return products.Where(x => x[ScanField.Collection].ContainsIgnoreCase("Andrew Martin")).ToList();
        }
    }

    public class BakerLifestyleFileLoader : KravetBaseFileLoader<BakerLifestyleVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "BAKER LIFESTYLE" }; 
        public BakerLifestyleFileLoader(IStorageProvider<BakerLifestyleVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class ColeAndSonFileLoader : KravetBaseFileLoader<ColeAndSonVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "COLE & SON" }; 
        public ColeAndSonFileLoader(IStorageProvider<ColeAndSonVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class GPJBakerFileLoader : KravetBaseFileLoader<GPJBakerVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "G P & J BAKER" }; 
        public GPJBakerFileLoader(IStorageProvider<GPJBakerVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class GroundworksFileLoader : KravetBaseFileLoader<GroundworksVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "GROUNDWORKS" }; 
        public GroundworksFileLoader(IStorageProvider<GroundworksVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class MulberryHomeFileLoader : KravetBaseFileLoader<MulberryHomeVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "MULBERRY" }; 
        public MulberryHomeFileLoader(IStorageProvider<MulberryHomeVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class ParkertexFileLoader : KravetBaseFileLoader<ParkertexVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "PARKERTEX" }; 
        public ParkertexFileLoader(IStorageProvider<ParkertexVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class ThreadsFileLoader : KravetBaseFileLoader<ThreadsVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "THREADS" }; 
        public ThreadsFileLoader(IStorageProvider<ThreadsVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class LauraAshleyFileLoader : KravetBaseFileLoader<LauraAshleyVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "LAURA ASHLEY" }; 
        public LauraAshleyFileLoader(IStorageProvider<LauraAshleyVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }

    public class BrunschwigAndFilsFileLoader : KravetBaseFileLoader<BrunschwigAndFilsVendor>
    {
        private static readonly List<string> AssociatedBrands = new List<string> { "BRUNSCHWIG & FILS" }; 
        public BrunschwigAndFilsFileLoader(IStorageProvider<BrunschwigAndFilsVendor> storageProvider) : base(storageProvider, AssociatedBrands) { }
    }
}

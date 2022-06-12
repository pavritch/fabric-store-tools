using Kravet.Discovery;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Storage;

namespace Kravet.SubvendorComponents
{
    public class KravetProductDiscoverer : KravetBaseProductDiscoverer<KravetVendor> { public KravetProductDiscoverer(IStorageProvider<KravetVendor> storageProvider, IProductFileLoader<KravetVendor> fileLoader) : base(storageProvider, fileLoader) { } }
    public class LeeJofaProductDiscoverer : KravetBaseProductDiscoverer<LeeJofaVendor> { public LeeJofaProductDiscoverer(IStorageProvider<LeeJofaVendor> storageProvider, IProductFileLoader<LeeJofaVendor> fileLoader) : base(storageProvider, fileLoader) { } } 

    public class BakerLifestyleProductDiscoverer : KravetBaseProductDiscoverer<BakerLifestyleVendor> { public BakerLifestyleProductDiscoverer(IStorageProvider<BakerLifestyleVendor> storageProvider, IProductFileLoader<BakerLifestyleVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class ColeAndSonProductDiscoverer : KravetBaseProductDiscoverer<ColeAndSonVendor> { public ColeAndSonProductDiscoverer(IStorageProvider<ColeAndSonVendor> storageProvider, IProductFileLoader<ColeAndSonVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class GPJBakerProductDiscoverer : KravetBaseProductDiscoverer<GPJBakerVendor> { public GPJBakerProductDiscoverer(IStorageProvider<GPJBakerVendor> storageProvider, IProductFileLoader<GPJBakerVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class GroundworksProductDiscoverer : KravetBaseProductDiscoverer<GroundworksVendor> { public GroundworksProductDiscoverer(IStorageProvider<GroundworksVendor> storageProvider, IProductFileLoader<GroundworksVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class MulberryHomeProductDiscoverer : KravetBaseProductDiscoverer<MulberryHomeVendor> { public MulberryHomeProductDiscoverer(IStorageProvider<MulberryHomeVendor> storageProvider, IProductFileLoader<MulberryHomeVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class ParkertexProductDiscoverer : KravetBaseProductDiscoverer<ParkertexVendor> { public ParkertexProductDiscoverer(IStorageProvider<ParkertexVendor> storageProvider, IProductFileLoader<ParkertexVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class ThreadsProductDiscoverer : KravetBaseProductDiscoverer<ThreadsVendor> { public ThreadsProductDiscoverer(IStorageProvider<ThreadsVendor> storageProvider, IProductFileLoader<ThreadsVendor> fileLoader) : base(storageProvider, fileLoader) { } } 

    public class LauraAshleyProductDiscoverer : KravetBaseProductDiscoverer<LauraAshleyVendor> { public LauraAshleyProductDiscoverer(IStorageProvider<LauraAshleyVendor> storageProvider, IProductFileLoader<LauraAshleyVendor> fileLoader) : base(storageProvider, fileLoader) { } } 

    public class AndrewMartinProductDiscoverer : KravetBaseProductDiscoverer<AndrewMartinVendor> { public AndrewMartinProductDiscoverer(IStorageProvider<AndrewMartinVendor> storageProvider, IProductFileLoader<AndrewMartinVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
    public class BrunschwigAndFilsProductDiscoverer : KravetBaseProductDiscoverer<BrunschwigAndFilsVendor> { public BrunschwigAndFilsProductDiscoverer(IStorageProvider<BrunschwigAndFilsVendor> storageProvider, IProductFileLoader<BrunschwigAndFilsVendor> fileLoader) : base(storageProvider, fileLoader) { } } 
}

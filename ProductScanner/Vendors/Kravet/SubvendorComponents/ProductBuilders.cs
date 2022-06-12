using Kravet.Details;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;

namespace Kravet.SubvendorComponents
{
    public class KravetProductBuilder : KravetBaseProductBuilder<KravetVendor>
    {
        public KravetProductBuilder(IPriceCalculator<KravetVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class LeeJofaProductBuilder : KravetBaseProductBuilder<LeeJofaVendor>
    {
        public LeeJofaProductBuilder(IPriceCalculator<LeeJofaVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class BakerLifestyleProductBuilder : KravetBaseProductBuilder<BakerLifestyleVendor>
    {
        public BakerLifestyleProductBuilder(IPriceCalculator<BakerLifestyleVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class ColeAndSonProductBuilder : KravetBaseProductBuilder<ColeAndSonVendor>
    {
        public ColeAndSonProductBuilder(IPriceCalculator<ColeAndSonVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class GPJBakerProductBuilder : KravetBaseProductBuilder<GPJBakerVendor>
    {
        public GPJBakerProductBuilder(IPriceCalculator<GPJBakerVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class GroundworksProductBuilder : KravetBaseProductBuilder<GroundworksVendor>
    {
        public GroundworksProductBuilder(IPriceCalculator<GroundworksVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class MulberryHomeProductBuilder : KravetBaseProductBuilder<MulberryHomeVendor>
    {
        public MulberryHomeProductBuilder(IPriceCalculator<MulberryHomeVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class ParkertexProductBuilder : KravetBaseProductBuilder<ParkertexVendor>
    {
        public ParkertexProductBuilder(IPriceCalculator<ParkertexVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class ThreadsProductBuilder : KravetBaseProductBuilder<ThreadsVendor>
    {
        public ThreadsProductBuilder(IPriceCalculator<ThreadsVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class LauraAshleyProductBuilder : KravetBaseProductBuilder<LauraAshleyVendor>
    {
        public LauraAshleyProductBuilder(IPriceCalculator<LauraAshleyVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class AndrewMartinProductBuilder : KravetBaseProductBuilder<AndrewMartinVendor>
    {
        public AndrewMartinProductBuilder(IPriceCalculator<AndrewMartinVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class BrunschwigAndFilsProductBuilder : KravetBaseProductBuilder<BrunschwigAndFilsVendor>
    {
        public BrunschwigAndFilsProductBuilder(IPriceCalculator<BrunschwigAndFilsVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }
}

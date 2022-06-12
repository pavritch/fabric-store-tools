using System;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace RyanStudio
{
    public class RyanStudioProductBuilder : ProductBuilder<RyanStudioVendor>
    {
        public RyanStudioProductBuilder(IPriceCalculator<RyanStudioVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            throw new NotImplementedException();
        }
    }
}
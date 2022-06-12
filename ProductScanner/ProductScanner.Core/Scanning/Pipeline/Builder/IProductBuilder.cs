using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Pipeline.Builder
{
    public abstract class ProductBuilder<T> : IProductBuilder<T> where T : Vendor
    {
        protected readonly IPriceCalculator<T> PriceCalculator;
        protected ProductBuilder(IPriceCalculator<T> priceCalculator) { PriceCalculator = priceCalculator; }
        public abstract VendorProduct Build(ScanData data);

        protected Dictionary<string, string> ToSpecs(Dictionary<ScanField, string> specs)
        {
            return specs.ToDictionary(x => x.Key.ToString().InsertSpacesBetweenWords(), x => x.Value);
        }

        public string GetRollType(int orderIncrement)
        {
            if (orderIncrement == 1) return "Single Roll";
            if (orderIncrement == 2) return "Double Roll";
            if (orderIncrement == 3) return "Triple Roll";
            return string.Format("{0} rolls", orderIncrement);
        }
    }

    // takes the final scan data (created from any data sources available)
    // and builds our final VendorProduct object
    // responsible for any parsing/cleaning of data
    public interface IProductBuilder<T> : IProductBuilder where T : Vendor
    {
        
    }

    public interface IProductBuilder
    {
        VendorProduct Build(ScanData data);
    }

    public interface IPriceCalculator<T> where T : Vendor
    {
        ProductPriceData CalculatePrice(ScanData data);
    }

    public class DefaultPriceCalculator<T> : IPriceCalculator<T> where T : Vendor, new()
    {
        public virtual ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new T();
            var cost = Math.Round(data.Cost, 2);
            return new ProductPriceData(cost * vendor.OurPriceMarkup, cost * vendor.RetailPriceMarkup);
        }
    }

    public interface IFullUpdateChecker<T> where T : Vendor
    {
        bool RequiresFullUpdate(VendorProduct vendorProduct, StoreProduct sqlProduct);
    }
}
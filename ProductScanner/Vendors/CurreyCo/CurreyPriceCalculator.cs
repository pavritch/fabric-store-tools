using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace CurreyCo
{
    public class CurreyPriceCalculator : IPriceCalculator<CurreyVendor>
    {
        private readonly CurreyFileLoader _productFileLoader;
        private List<ScanData> _products; 

        public CurreyPriceCalculator(CurreyFileLoader productFileLoader)
        {
            _productFileLoader = productFileLoader;
            _products = _productFileLoader.LoadPriceData();
        }

        public ProductPriceData CalculatePrice(ScanData data)
        {
            var match = _products.SingleOrDefault(x => x[ScanField.ItemNumber] == data[ScanField.ManufacturerPartNumber]);
            if (match != null)
            {
                return new ProductPriceData(match[ScanField.MAP].ToDecimalSafe(), match[ScanField.MAP].ToDecimalSafe() * 1.25m);
            }

            var map = data[ScanField.MAP].ToDecimalSafe();
            var ourRetail = map*1.25m;
            return new ProductPriceData(map, ourRetail);
        }
    }
}
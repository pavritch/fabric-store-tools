using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace YourOtherWarehouse
{
    public class YourOtherWarehouseProductDiscoverer : IProductDiscoverer<YourOtherWarehouseVendor>
    {
        private readonly YourOtherWarehouseFileLoader _fileLoader;
        private readonly YourOtherWarehouseStockFileLoader _stockFileLoader;
        private readonly YourOtherWarehousePriceFileLoader _priceFileLoader;

        public YourOtherWarehouseProductDiscoverer(YourOtherWarehouseFileLoader fileLoader, YourOtherWarehouseStockFileLoader stockFileLoader, YourOtherWarehousePriceFileLoader priceFileLoader)
        {
            _fileLoader = fileLoader;
            _stockFileLoader = stockFileLoader;
            _priceFileLoader = priceFileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = await _fileLoader.LoadProductsAsync();
            var stockInfo = _stockFileLoader.LoadStockData();
            var missingBrands = new HashSet<string>();

            foreach (var product in products)
            {
                var stockRecord = stockInfo.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
                if (stockRecord != null)
                {
                    product[ScanField.StockCount] = stockRecord[ScanField.StockCount];
                    product[ScanField.RetailPrice] = stockRecord[ScanField.RetailPrice];
                    product.Cost = stockRecord[ScanField.Cost].ToDecimalSafe();
                }
            }

            var priceMultipliers = _priceFileLoader.LoadStockData();
            foreach (var product in products)
            {
                var multiplierRecord = priceMultipliers.Where(x => MatchBrand(product[ScanField.Brand], x[ScanField.Brand]));
                if (!multiplierRecord.Any())
                {
                    missingBrands.Add(product[ScanField.Brand]);
                }
                else
                {
                    //product[ScanField.Multiplier] = multiplierRecord.First()[ScanField.Multiplier];
                }
            }

            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private bool MatchBrand(string productBrand, string multiplierBrand)
        {
            productBrand = productBrand.TitleCase();
            productBrand = productBrand.Replace("Aqua Pure", "Aqua-Pure");
            productBrand = productBrand.Replace("AquaPure", "Aqua-Pure");
            productBrand = productBrand.Replace("Bootz Industries", "Bootz");
            productBrand = productBrand.Replace("Bootz Indutries", "Bootz");
            productBrand = productBrand.Replace("Delta Breez", "Delta");
            productBrand = productBrand.Replace("Delta Electronics", "Delta");
            productBrand = productBrand.Replace("InSinkerator", "In-Sink-Erator");
            productBrand = productBrand.Replace("InSinkErator", "In-Sink-Erator");
            productBrand = productBrand.Replace("Jacuzzi Luxury Bath", "Jacuzzi");
            productBrand = productBrand.Replace("MOEN Home Care", "Moen");
            productBrand = productBrand.Replace("Mr. Steam", "Mr Steam");
            productBrand = productBrand.Replace("Newport", "Newport Brass");
            productBrand = productBrand.Replace("Safety First", "SafetyFirst");
            productBrand = productBrand.Replace("T&S Brass", "T & S Brass");
            productBrand = productBrand.Replace("T&S Brass and Bronze Works, Inc.", "T & S Brass");
            productBrand = productBrand.Replace("Weiser", "Weiser Lock");
            productBrand = productBrand.Replace("Whitehaus Collection", "Whitehaus");

            return (productBrand.ToLower() == multiplierBrand.ToLower());
        }
    }
}
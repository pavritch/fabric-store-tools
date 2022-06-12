using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.LoadTesting
{
    // creates random stock check requests based on product data - used for load testing
    public class RandomRequestGenerator : IRandomRequestGenerator
    {
        private readonly IStoreDatabase<InsideFabricStore> _storeDatabase;
        public RandomRequestGenerator(IStoreDatabase<InsideFabricStore> storeDatabase)
        {
            _storeDatabase = storeDatabase;
        }

        public async Task<List<StockCheck>> GenerateAsync(int maxVendors, int maxProductsPerVendor)
        {
            var numVendors = RandomInteger.Between(3, maxVendors);
            // only works against Fabric right now
            var allVendors = Vendor.GetByStore(StoreType.InsideFabric);
            var vendors = allVendors.GetRandom(numVendors).ToList();
            var task = Task.WhenAll(vendors.Select(x => GenerateForVendor(x, maxProductsPerVendor)).ToList());
            var results = await task;
            return results.SelectMany(x => x.ToList()).ToList();
        }

        private async Task<List<StockCheck>> GenerateForVendor(Vendor vendor, int maxProductsPerVendor)
        {
            var productsPerVendor = RandomInteger.Between(1, maxProductsPerVendor);
            var allVariants = (await _storeDatabase.GetVariantIds(vendor.Id));
            var products = allVariants.GetRandom(productsPerVendor);
            return products.Select(x => new StockCheck
            {
                VariantId = x,
                Quantity = RandomInteger.Between(1, 20),
                ForceFetch = RandomInteger.Bool()
            }).ToList();
        }

        public async Task<List<StockCheck>> GenerateForVendors(List<Vendor> vendors, int maxProductsPerVendor)
        {
            var results = await Task.WhenAll(vendors.Select(x => GenerateForVendor(x, maxProductsPerVendor)));
            return results.SelectMany(x => x).ToList();
        }
    }
}
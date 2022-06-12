using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace SilverState.Metadata
{
    public class SilverStateMetadataCollector : IMetadataCollector<SilverStateVendor>
    {
        private readonly IStoreDatabase<InsideFabricStore> _storeDatabase;
        public SilverStateMetadataCollector(IStoreDatabase<InsideFabricStore> storeDatabase)
        {
            _storeDatabase = storeDatabase;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var sunbrellaProducts = await _storeDatabase.GetProductSKUsAsync(72);
            var distinctProducts = new List<ScanData>();
            foreach (var product in products)
            {
                // should be able to match SS's pattern number (for those that have one)
                // with the first part of Sunbrella's SKU
                var match = sunbrellaProducts.FirstOrDefault(x => x.Split(new[] {'-'})[1] == 
                    product[ScanField.PatternName].Split(new [] {' '}).First().ToUpper());
                if (match == null)
                {
                    distinctProducts.Add(product);
                }
            }
            return distinctProducts;
        }
    }
}

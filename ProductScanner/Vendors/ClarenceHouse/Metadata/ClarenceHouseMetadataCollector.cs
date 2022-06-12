using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ClarenceHouse.Metadata
{
    public class ClarenceHouseMetadataCollector : IMetadataCollector<ClarenceHouseVendor>
    {
        private readonly ClarenceHouseActiveMAPFileLoader _activeFileLoader;
        private readonly ClarenceHouseLimitedMAPFileLoader _limitedFileLoader;

        public ClarenceHouseMetadataCollector(ClarenceHouseActiveMAPFileLoader activeFileLoader, ClarenceHouseLimitedMAPFileLoader limitedFileLoader)
        {
            _activeFileLoader = activeFileLoader;
            _limitedFileLoader = limitedFileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            SetMapPricing(products);
            FillMissingUnitOfMeasures(products);
            return Task.FromResult(products);
        }

        private void SetMapPricing(List<ScanData> products)
        {
            var activeMapData = _activeFileLoader.LoadMAPData();
            var limitedMapData = _limitedFileLoader.LoadMAPData();

            foreach (var product in products)
            {
                var activeMatch = activeMapData.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
                var limitedMatch = limitedMapData.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);

                if (activeMatch != null) product[ScanField.MAP] = activeMatch[ScanField.MAP];
                if (limitedMatch != null) product[ScanField.MAP] = limitedMatch[ScanField.MAP];
            }
        }

        private void FillMissingUnitOfMeasures(List<ScanData> webProducts)
        {
            var dicPatternNumberLookup = webProducts.GroupBy(e => e[ScanField.PatternNumber])
                .ToDictionary(k => k.Key, v => v.ToList());

            // attempt to fill in any missing unit of measure with ones from same pattern number
            var missingUnitOfMeasures = webProducts.Where(x => !x.ContainsKey(ScanField.UnitOfMeasure)).ToList();
            foreach (var product in missingUnitOfMeasures)
            {
                // search list of products with same pattern number to see if one might have a unit of measure filled in
                var list = dicPatternNumberLookup[product[ScanField.PatternNumber]];

                var otherProduct = list
                    .FirstOrDefault(e => e.ContainsKey(ScanField.UnitOfMeasure) && e[ScanField.ProductGroup] == product[ScanField.ProductGroup]);

                if (otherProduct != null)
                    product[ScanField.UnitOfMeasure] = otherProduct[ScanField.UnitOfMeasure];
            }
        }
    }
}
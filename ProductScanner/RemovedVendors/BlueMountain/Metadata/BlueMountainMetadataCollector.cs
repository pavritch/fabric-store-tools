using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;

namespace BlueMountain.Metadata
{
    public class BlueMountainMetadataCollector : IMetadataCollector<BlueMountainVendor>
    {
        private readonly IVendorScanSessionManager<BlueMountainVendor> _sessionManager;

        public BlueMountainMetadataCollector(IVendorScanSessionManager<BlueMountainVendor> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            // This isn't scanning for metadata, so I either need to rename this class,
            // or add another hook for this type of operation
            var duplicates = products.Where(x => x.ContainsKey(ProductPropertyType.UPC))
                .GroupBy(x => x[ProductPropertyType.UPC]).Where(x => x.Count() > 1).ToList();
            var toRemove = new List<string>();
            foreach (var group in duplicates)
            {
                // keep the first one that we found
                var duplicateProducts = group.Skip(1).Select(x => x[ProductPropertyType.ManufacturerPartNumber]).ToList();
                toRemove.AddRange(duplicateProducts);

                var validProduct = group.First()[ProductPropertyType.ManufacturerPartNumber];
                var product = products.First(x => x[ProductPropertyType.ManufacturerPartNumber] == validProduct);
                product[ProductPropertyType.AlternateItemNumber] = duplicateProducts.ToCommaDelimitedList();
            }
            
            toRemove.ForEach(x => _sessionManager.Log(new EventLogRecord("Removed Duplicate: " + x)));
            return Task.FromResult(products.Where(x => !toRemove.Contains(x[ProductPropertyType.ManufacturerPartNumber])).ToList());
        }
    }
}

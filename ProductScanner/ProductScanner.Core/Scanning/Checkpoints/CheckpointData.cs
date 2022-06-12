using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Checkpoints
{
    public class CheckpointData
    {
        public Queue<DiscoveredProduct> RemainingProducts { get; set; }
        public int TotalScans { get; set; }
        public List<ScanData> ScannedProducts { get; set; }

        public CheckpointData() { }
        public CheckpointData(IEnumerable<DiscoveredProduct> products)
        {
            RemainingProducts = new Queue<DiscoveredProduct>(products);
            TotalScans = products.Count();
            ScannedProducts = new List<ScanData>();
        }

        public DiscoveredProduct GetNextProduct()
        {
            return RemainingProducts.Peek();
        }

        public bool IsNotDone() { return RemainingProducts.Any(); }

        public void SaveResult(List<ScanData> products)
        {
            foreach (var product in products)
            {
                if (!product.ContainsKey(ScanField.ManufacturerPartNumber)) continue;
                if (ScannedProducts.Any(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber])) continue;

                ScannedProducts.Add(product);
            }
            RemainingProducts.Dequeue();
        }

        public List<ScanData> GetResults()
        {
            return ScannedProducts;
        }

        public int GetCountCompleted()
        {
            return TotalScans - RemainingProducts.Count;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Sunbrella.Discovery
{
    public class SunbrellaProductDiscoverer : IProductDiscoverer<SunbrellaVendor>
    {
        private readonly IProductFileLoader<SunbrellaVendor> _fileLoader;
        public SunbrellaProductDiscoverer(IProductFileLoader<SunbrellaVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            return fileProducts.Select(CreateDiscoveredProduct).ToList();
        }

        private DiscoveredProduct CreateDiscoveredProduct(ScanData fileData)
        {
            var product = new DiscoveredProduct(new ScanData
            {
                {ScanField.ManufacturerPartNumber, fileData[ScanField.ManufacturerPartNumber]}, 
                {ScanField.PatternName, fileData[ScanField.PatternName]}, 
                {ScanField.ColorName, fileData[ScanField.ColorName]}, 
                {ScanField.Cost, fileData[ScanField.Cost]},
                {ScanField.MAP, fileData[ScanField.MAP]},
            });
            return product;
        }
    }
}
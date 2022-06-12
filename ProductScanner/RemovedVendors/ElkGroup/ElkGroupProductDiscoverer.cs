using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElkGroup.FileLoaders;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ElkGroup
{
    public class ElkGroupProductDiscoverer : IProductDiscoverer<ElkGroupVendor>
    {
        private readonly ElkAlicoFileLoader _elkAlicoFileLoader;
        private readonly ElkCornerstoneFileLoader _elkCornerstoneFileLoader;
        private readonly ElkDimondHomeFileLoader _elkDimondHomeFileLoader;
        private readonly ElkDimondProductFileLoader _elkDimondProductFileLoader;
        private readonly ElkGuildMasterFileLoader _elkGuildMasterFileLoader;
        private readonly ElkLampFileLoader _elkLampMasterFileLoader;
        private readonly ElkMirrorMastersFileLoader _elkMirrorMastersFileLoader;
        private readonly ElkProductFileLoader _elkProductFileLoader;
        private readonly ElkSterlingFileLoader _elkSterlingFileLoader;

        public ElkGroupProductDiscoverer(ElkAlicoFileLoader elkAlicoFileLoader, ElkCornerstoneFileLoader elkCornerstoneFileLoader, ElkDimondHomeFileLoader elkDimondHomeFileLoader, ElkDimondProductFileLoader elkDimondProductFileLoader, ElkGuildMasterFileLoader elkGuildMasterFileLoader, ElkLampFileLoader elkLampMasterFileLoader, ElkMirrorMastersFileLoader elkMirrorMastersFileLoader, ElkProductFileLoader elkProductFileLoader, ElkSterlingFileLoader elkSterlingFileLoader)
        {
            _elkAlicoFileLoader = elkAlicoFileLoader;
            _elkCornerstoneFileLoader = elkCornerstoneFileLoader;
            _elkDimondHomeFileLoader = elkDimondHomeFileLoader;
            _elkDimondProductFileLoader = elkDimondProductFileLoader;
            _elkGuildMasterFileLoader = elkGuildMasterFileLoader;
            _elkLampMasterFileLoader = elkLampMasterFileLoader;
            _elkMirrorMastersFileLoader = elkMirrorMastersFileLoader;
            _elkProductFileLoader = elkProductFileLoader;
            _elkSterlingFileLoader = elkSterlingFileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _elkAlicoFileLoader.LoadInventoryData();
            products.AddRange(_elkCornerstoneFileLoader.LoadInventoryData());
            products.AddRange(_elkDimondHomeFileLoader.LoadInventoryData());
            products.AddRange(_elkDimondProductFileLoader.LoadInventoryData());
            products.AddRange(_elkProductFileLoader.LoadInventoryData());
            products.AddRange(_elkGuildMasterFileLoader.LoadInventoryData());
            products.AddRange(_elkLampMasterFileLoader.LoadInventoryData());
            products.AddRange(_elkMirrorMastersFileLoader.LoadInventoryData());
            products.AddRange(_elkSterlingFileLoader.LoadInventoryData());
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace JamesHare.Metadata
{
    public class JamesHareProductBuilder : IProductBuilder<JamesHareVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            throw new NotImplementedException();
        }
    }


    public class JamesHareMetadataCollector : IMetadataCollector<JamesHareVendor>
    {
        private readonly IProductFileLoader<JamesHareVendor> _fileLoader;
        public JamesHareMetadataCollector(IProductFileLoader<JamesHareVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var mpn = product[ProductPropertyType.ManufacturerPartNumber];
                var patternNumber = mpn.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries).First();
                var priceRecord = fileProducts.FirstOrDefault(x =>
                {
                    var number = x[ProductPropertyType.TempContent1].Split(new[] {"/"},
                        StringSplitOptions.RemoveEmptyEntries).First();
                    return number == patternNumber;
                });

                if (priceRecord == null)
                    product[ProductPropertyType.WholesalePrice] = "0";
                product[ProductPropertyType.WholesalePrice] = priceRecord[ProductPropertyType.WholesalePrice];
            }
            return products;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JamesHare.Metadata
{
    // only used to get the price
    public class JamesHareFileLoader : ProductFileLoader<JamesHareVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("PATTERN #   ", ProductPropertyType.TempContent1),
            new FileProperty("PATTERN # TO", ProductPropertyType.TempContent2),
            new FileProperty("COLLECTION", ProductPropertyType.Collection),
            new FileProperty("MANUFACTURER", ProductPropertyType.Brand),
            new FileProperty("NET", ProductPropertyType.WholesalePrice),
        };

        public JamesHareFileLoader(IStorageProvider<JamesHareVendor> storageProvider) : base(storageProvider)
        {
            _headerRow = 5;
            _startRow = 7;
            _keyProperty = ProductPropertyType.TempContent1;
            _properties = Properties;
        }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var fileProducts = await base.LoadProductsAsync();
            return fileProducts.Where(x => x[ProductPropertyType.Brand] == "JAMES HARE").ToList();
        }
    }
}
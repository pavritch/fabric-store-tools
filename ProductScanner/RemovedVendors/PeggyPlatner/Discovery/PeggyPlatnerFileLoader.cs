using System.Collections.Generic;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Storage;

namespace PeggyPlatner.Discovery
{
    public class PeggyPlatnerFileLoader : ProductFileLoader<PeggyPlatnerVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("FABRIC NAME", ProductPropertyType.PatternName),
            new FileProperty("PATTERN  #", ProductPropertyType.TempContent1),
            new FileProperty("NET PRICE", ProductPropertyType.WholesalePrice),
            new FileProperty("CONTENT", ProductPropertyType.Content),
            new FileProperty("WIDTH", ProductPropertyType.Width),
            new FileProperty("MARTINDALE ABRASION RUB TEST & COMMERCIAL RATINGS", ProductPropertyType.Durability),
            new FileProperty("COUNTRY OF ORIGIN", ProductPropertyType.CountryOfOrigin),
        };

        public PeggyPlatnerFileLoader(IStorageProvider<PeggyPlatnerVendor> storageProvider) : base(storageProvider)
        {
            _headerRow = 1;
            _startRow = 2;
            _keyProperty = ProductPropertyType.PatternName;
            _properties = Properties;
        }
    }
}
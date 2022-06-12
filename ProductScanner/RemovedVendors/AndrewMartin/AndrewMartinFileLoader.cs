using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace AndrewMartin
{
    public class AndrewMartinFileLoader : ProductFileLoader<AndrewMartinVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("PatternName", ScanField.PatternName),
            new FileProperty("Flame", ScanField.FireCode),
            new FileProperty("Railroaded", ScanField.Railroaded),
            new FileProperty("Retail", ScanField.RetailPrice),
            new FileProperty("Cost", ScanField.Cost),
        };

        public AndrewMartinFileLoader(IStorageProvider<AndrewMartinVendor> storageProvider)
            : base(storageProvider, ScanField.PatternName, Properties) { } 

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var loadedProducts = await base.LoadProductsAsync();

            // do any post processing of file data
            foreach (var product in loadedProducts)
            {
                var trimmer = new Regex(@"\s\s+");

                var patternName = product[ScanField.PatternName];
                patternName = patternName.Replace("*", "");
                patternName = trimmer.Replace(patternName, " ");
                patternName = patternName.Replace((char)0xA0, ' ');
                product[ScanField.PatternName] = patternName;
            }
            return loadedProducts;
        }
    }
}
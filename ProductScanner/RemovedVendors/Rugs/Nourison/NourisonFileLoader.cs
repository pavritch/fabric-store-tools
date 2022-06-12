using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nourison
{
    public class NourisonFileLoader : ProductFileLoader<NourisonVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("ID", ScanField.ItemNumber),
            new FileProperty("CMPY", ScanField.Ignore),
            new FileProperty("CUST", ScanField.Ignore),
            new FileProperty("CUSTOMER", ScanField.Ignore),
            new FileProperty("COLL", ScanField.Ignore),
            new FileProperty("COLLECTION", ScanField.Collection),
            new FileProperty("STYLE", ScanField.PatternNumber),
            new FileProperty("STYLE", ScanField.Ignore),
            new FileProperty("SIZE", ScanField.Size),
            new FileProperty("BACK", ScanField.Ignore),
            new FileProperty("FIBER", ScanField.Ignore),
            new FileProperty("PATTERN", ScanField.Ignore),
            new FileProperty("ROLL", ScanField.Cost),
            new FileProperty("CUT", ScanField.Ignore),
            new FileProperty("UM", ScanField.Ignore),
            new FileProperty("END", ScanField.Ignore),
            new FileProperty("", ScanField.Ignore),
        };

        public NourisonFileLoader(IStorageProvider<NourisonVendor> storageProvider)
            : base(storageProvider, ScanField.ItemNumber, Properties, ProductFileType.Xlsx, 1, 3) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x =>
            {
                var size = x[ScanField.Size].Replace("'", "");

                if (size.Contains("RND"))
                {
                    var dim = size.Split(Convert.ToChar("X")).First();
                    size = string.Format("{0}X{0}", dim);
                }
                x[ScanField.Size] = size;
            });
            return products;
        }
    }
}
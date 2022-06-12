using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nourison
{
    public class NourisonImapFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Number", ScanField.ItemNumber),
            new FileProperty("Brand", ScanField.Pattern),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Price", ScanField.Cost)
        };

        private readonly IStorageProvider<NourisonVendor> _storageProvider;
        public NourisonImapFileLoader(IStorageProvider<NourisonVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var stockFilePath = Path.Combine(_storageProvider.GetStaticFolder(), "Nourison_IMAP.xlsx");
            var fileLoader = new ExcelFileLoader();
            var data = fileLoader.Load(stockFilePath, Properties, ScanField.ItemNumber, 1, 2);
            foreach (var row in data)
            {
                row[ScanField.Size] = row[ScanField.Size].Trim();

                if (row[ScanField.Size].Trim().EndsWith("’"))
                    row[ScanField.Size] = (row[ScanField.Size] + "0");

                row[ScanField.Size] = row[ScanField.Size].Replace("’x", ".0x").Replace("’", ".").Replace("”", "").ToLower();
            }
            return data;
        }
    }
}
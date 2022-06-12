using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InnovationsUsa.Discovery;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace InnovationsUsa.Metadata
{
    public class InnovationsUsaMetadataCollector : IMetadataCollector<InnovationsUsaVendor>
    {
        private readonly InnovationsUsaFileLoader _fileLoader;

        public InnovationsUsaMetadataCollector(InnovationsUsaFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileData = await _fileLoader.LoadProductsAsync();

            foreach (var product in products)
            {
                var matchingFileData = fileData.FirstOrDefault(x => x[ScanField.PatternName].ToLower() == product[ScanField.PatternName].ToLower());
                if (matchingFileData == null) continue;

                product[ScanField.Cost] = matchingFileData[ScanField.Cost];
                product[ScanField.MinimumOrder] = matchingFileData[ScanField.MinimumOrder];
                product[ScanField.UnitOfMeasure] = matchingFileData[ScanField.UnitOfMeasure];

                var cutFee = matchingFileData[ScanField.CutFee];
                if (cutFee == "Y")
                {
                    product[ScanField.Cost] = (product[ScanField.Cost].ToDoubleSafe() + 3).ToString();
                }
            }
            return products;
        }
    }
}
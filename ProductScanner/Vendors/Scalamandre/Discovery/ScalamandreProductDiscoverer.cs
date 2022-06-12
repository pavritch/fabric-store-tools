using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Renci.SshNet;
using Utilities.Extensions;

namespace Scalamandre.Discovery
{
    public class ScalamandreProductDiscoverer : IProductDiscoverer<ScalamandreVendor>
    {
        private readonly IStorageProvider<ScalamandreVendor> _storageProvider;
        private readonly IProductFileLoader<ScalamandreVendor> _fileLoader;

        public ScalamandreProductDiscoverer(IStorageProvider<ScalamandreVendor> storageProvider, IProductFileLoader<ScalamandreVendor> fileLoader)
        {
            _storageProvider = storageProvider;
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var filePath = _storageProvider.GetProductsFileCachePath(ProductFileType.Xls);
            using (SftpClient client = new SftpClient("168.61.170.58", 9063, "EDI_Insidestores", "S*or@s0056"))
            {
                client.Connect();
                var res = client.ListDirectory("EDI_Insidestores")
                    .OrderByDescending(x => x.LastWriteTime)
                    .Select(x => x.Name)
                    .Where(x => x.Contains(".csv")).ToList();
                var latestFile = res.First();


                var fileStream = new MemoryStream();
                client.DownloadFile("EDI_Insidestores/" + latestFile, fileStream);

                fileStream.Position = 0;
                FileExtensions.ConvertCSVToXLS(fileStream, filePath);
            }

            return (await _fileLoader.LoadProductsAsync())
                .Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}
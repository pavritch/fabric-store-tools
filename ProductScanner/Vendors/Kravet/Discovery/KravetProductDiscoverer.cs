using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ionic.Zip;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Kravet.Discovery
{
    public class KravetBaseProductDiscoverer<T> : IProductDiscoverer<T> where T : Vendor
    {
        private readonly IStorageProvider<T> _storageProvider;
        private readonly IProductFileLoader<T> _fileLoader; 
        public KravetBaseProductDiscoverer(IStorageProvider<T> storageProvider, IProductFileLoader<T> fileLoader)
        {
            _storageProvider = storageProvider;
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            DownloadProductFile();
            return (await _fileLoader.LoadProductsAsync())
                .Select(x => new DiscoveredProduct(x)).ToList();
        }

        private void DownloadProductFile()
        {
            // had to use version 1.9.1.8 becuase of this issue
            // https://github.com/haf/DotNetZip.Semverd/issues/4
            
            var filePath = _storageProvider.GetProductsFileCachePath(ProductFileType.Xls);
            if (File.Exists(filePath)) return;

            var zipUrl = "ftp://insideave:inside00@file.kravet.com/insideave.zip";
            var zipStream = new MemoryStream(new WebClient().DownloadData(zipUrl));
            using (var zip1 = ZipFile.Read(zipStream))
            {
                foreach (var e in zip1)
                {
                    using (var csvStream = new MemoryStream())
                    {
                        e.Extract(csvStream);
                        csvStream.Position = 0;
                        FileExtensions.ConvertCSVToXLS(csvStream, filePath);
                    }
                }
            }
        }
    }
}
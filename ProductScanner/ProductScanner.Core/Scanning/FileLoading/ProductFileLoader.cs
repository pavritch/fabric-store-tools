using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public enum ProductFileType
    {
        Xls,
        Xlsx
    }

    public abstract class ProductFileLoader<T> : IProductFileLoader<T> where T : Vendor, new()
    {
        private readonly int _headerRow;
        private readonly int _startRow;
        private readonly ScanField _keyProperty;
        private readonly List<FileProperty> _properties;
        private readonly ProductFileType _fileExtension = ProductFileType.Xlsx;

        private readonly IStorageProvider<T> _storageProvider;
        protected ProductFileLoader(IStorageProvider<T> storageProvider, ScanField keyProperty, List<FileProperty> properties, 
            ProductFileType fileExtension = ProductFileType.Xlsx, int headerRow = 1, int startRow = 2)
        {
            _storageProvider = storageProvider;

            _headerRow = headerRow;
            _startRow = startRow;
            _keyProperty = keyProperty;
            _properties = properties;
            _fileExtension = fileExtension;
        }

        public virtual Task<List<ScanData>> LoadProductsAsync()
        {
            // check for products file in the cache or static folder
            var cacheFilePath = _storageProvider.GetProductsFileCachePath(_fileExtension);
            var staticFilePath = _storageProvider.GetProductsFileStaticPath(_fileExtension);

            string filePath = null;
            if (File.Exists(cacheFilePath)) filePath = cacheFilePath;
            if (File.Exists(staticFilePath)) filePath = staticFilePath;
            
            if (filePath == null) throw new FileNotFoundException("Product File not Found for " + new T().DisplayName);

            var fileLoader = new ExcelFileLoader();
            return Task.FromResult(fileLoader.Load(filePath, _properties, _keyProperty, _headerRow, _startRow));
        }
    }
}
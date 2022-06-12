using System.Collections.Generic;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.FileLoading;

namespace ProductScanner.Core.Scanning.Storage
{
    public interface IStorageProvider
    {
        string GetCacheFolder();
        string GetStaticFolder();
        void DeleteCache(int olderThanDays);
        int GetStaticDataVersion();

        bool HasFile(CacheFolder folder, string filename, CacheFileType fileType);
        bool HasFile(CacheFolder folder, string filenameWithExtension);
        void SaveFile(CacheFolder folder, string filename, HtmlNode file);
        void SaveFile(CacheFolder folder, string filename, string data);
        void SaveFile(CacheFolder folder, string filename, byte[] bytes);
        HtmlNode GetHtmlFile(CacheFolder folder, string filename);
        string GetStringFile(CacheFolder folder, string filename);
        byte[] GetBinaryFile(CacheFolder folder, string filenameWithExtension);

        List<string> GetProductsTextFile();

        string GetCSVReportPath(string filename);
        string GetStockFileCachePath(ProductFileType type);
        string GetProductsFileStaticPath(ProductFileType type);
        string GetProductsFileCachePath(ProductFileType type);
        void RemoveFile(CacheFolder folder, string filename);
        string GetCacheFilesFolder();
        double GetCacheSizeInMB();
        string GetDiscontinuedFileCachePath();

        void SaveProductsFile(ProductFileType extension, byte[] productFileData);
        void SaveStockFile(ProductFileType extension, byte[] stockFileData);
    }

    public interface IStorageProvider<T> : IStorageProvider where T : Vendor
    {
    }
}
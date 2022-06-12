using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using ProductScanner.Core.Config;
using ProductScanner.Core.Scanning.FileLoading;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Storage
{
    public class FileStorageProvider<T> : IStorageProvider<T> where T : Vendor, new()
    {
        private readonly string _cacheRoot;
        private readonly string _staticRoot;

        public FileStorageProvider(IAppSettings appSettings)
        {
            _cacheRoot = appSettings.CacheRoot;
            _staticRoot = appSettings.StaticRoot;
        }

        public bool HasFile(CacheFolder folder, string filename, CacheFileType fileType)
        {
            return File.Exists(GetFullCachePath(folder, filename, fileType));
        }

        public bool HasFile(CacheFolder folder, string filenameWithExtension)
        {
            return File.Exists(GetCachePath(folder, filenameWithExtension));
        }

        public void SaveFile(CacheFolder folder, string filename, HtmlNode file)
        {
            var fullPath = GetFullCachePath(folder, filename, CacheFileType.Html);
            var parent = Directory.GetParent(fullPath).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);
            File.WriteAllText(fullPath.ToLower(), file.OuterHtml);
        }

        public void SaveFile(CacheFolder folder, string filename, string jsonData)
        {
            var fullPath = GetFullCachePath(folder, filename, CacheFileType.Txt);
            var parent = Directory.GetParent(fullPath).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);
            File.WriteAllText(fullPath.ToLower(), jsonData);
        }

        public void SaveFile(CacheFolder folder, string filenameWithExtension, byte[] bytes)
        {
            var fullPath = GetCachePath(folder, filenameWithExtension);
            var parent = Directory.GetParent(fullPath).FullName;
            if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);
            File.WriteAllBytes(fullPath.ToLower(), bytes);
        }

        public void RemoveFile(CacheFolder folder, string filename)
        {
            var fullPath = GetFullCachePath(folder, filename, CacheFileType.Html);
            File.Delete(fullPath);
        }

        public HtmlNode GetHtmlFile(CacheFolder folder, string filename)
        {
            var content = File.ReadAllText(GetFullCachePath(folder, filename, CacheFileType.Html));
            var page = new HtmlDocument();
            page.LoadHtml(content);
            return page.DocumentNode.Clone();
        }

        public string GetStringFile(CacheFolder folder, string filename)
        {
            return File.ReadAllText(GetFullCachePath(folder, filename, CacheFileType.Txt));
        }

        public byte[] GetBinaryFile(CacheFolder folder, string filenameWithExtension)
        {
            return File.ReadAllBytes(GetCachePath(folder, filenameWithExtension));
        }

        private string GetCachePath(CacheFolder folder, string filenameWithExtension)
        {
            var vendor = new T();
            return Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName(), "Cache", folder.ToString(), filenameWithExtension);
        }

        private string GetCacheFolder(CacheFolder folder)
        {
            var vendor = new T();
            return Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName(), "Cache", folder.ToString());
        }

        private string GetFullCachePath(CacheFolder folder, string filepath, CacheFileType fileType)
        {
            var invalidFileChars = Path.GetInvalidFileNameChars();
            invalidFileChars.ForEach(x => filepath = filepath.Replace(x.ToString(), ""));

            var vendor = new T();
            return Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName(), "Cache", folder.ToString(), filepath + "." + fileType.ToString().ToLower());
        }

        private string GetFullReportPath(string filepath)
        {
            var vendor = new T();
            return Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName(), "Reports", filepath);
        }

        public string GetCSVReportPath(string filename)
        {
            return GetFullReportPath(filename);
        }

        public List<string> GetProductsTextFile()
        {
            var vendor = new T();
            var productFilePath = Path.Combine(_staticRoot, vendor.Store.ToString(), vendor.GetName(),
                string.Format("{0}_Price.csv", vendor.GetName()));
            var content = File.ReadAllLines(productFilePath);
            return content.ToList();
        }

        public string GetStockFileCachePath(ProductFileType extension)
        {
            var vendor = new T();
            return Path.Combine(GetCacheFilesFolder(), string.Format("{0}_Stock.{1}", vendor.GetName(), extension.ToString().ToLower()));
        }

        public string GetProductsFileStaticPath(ProductFileType extension)
        {
            var vendor = new T();
            return Path.Combine(_staticRoot, vendor.Store.ToString(), vendor.GetName(), string.Format("{0}_Price.{1}", vendor.GetName(), extension.ToString().ToLower()));
        }

        public string GetProductsFileCachePath(ProductFileType extension)
        {
            var vendor = new T();
            return GetCachePath(CacheFolder.Files, string.Format("{0}_Price.{1}", vendor.GetName(), extension.ToString().ToLower()));
        }

        public void SaveProductsFile(ProductFileType extension, byte[] fileData)
        {
            var vendor = new T();
            SaveFile(CacheFolder.Files, string.Format("{0}_Price.{1}", vendor.GetName(), extension.ToString().ToLower()), fileData);
        }

        public void SaveStockFile(ProductFileType extension, byte[] fileData)
        {
            var vendor = new T();
            SaveFile(CacheFolder.Files, string.Format("{0}_Stock.{1}", vendor.GetName(), extension.ToString().ToLower()), fileData);
        }

        public string GetCacheFilesFolder()
        {
            return GetCacheFolder(CacheFolder.Files);
        }

        public string GetDiscontinuedFileStaticPath()
        {
            var vendor = new T();
            return Path.Combine(_staticRoot, vendor.Store.ToString(), vendor.GetName(), string.Format("{0}_Discontinued.xlsx", vendor.GetName()));
        }

        public string GetDiscontinuedFileCachePath()
        {
            var vendor = new T();
            return GetCachePath(CacheFolder.Files, string.Format("{0}_Discontinued.xlsx", vendor.GetName()));
        }

        public void DeleteCache(int olderThanDays)
        {
            var vendor = new T();
            var vendorCachePath = Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName());
            if (!Directory.Exists(vendorCachePath)) return;

            var allFiles = Directory.GetFiles(vendorCachePath, "*", SearchOption.AllDirectories);
            var olderThan = allFiles.Select(x => new FileInfo(x)).Where(f => f.CreationTime < DateTime.Now.AddDays(-olderThanDays)).ToList();
            olderThan.ForEach(x => x.Delete());
        }

        public double GetCacheSizeInMB()
        {
            var vendor = new T();
            var di = new DirectoryInfo(Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName()));
            var size = Math.Round(FileExtensions.DirSize(di) / Math.Pow(2, 20), 2);
            return size;
        }

        public string GetCacheFolder()
        {
            var vendor = new T();
            return Path.Combine(_cacheRoot, vendor.Store.ToString(), vendor.GetName());
        }

        public string GetStaticFolder()
        {
            var vendor = new T();
            return Path.Combine(_staticRoot, vendor.Store.ToString(), vendor.GetName());
        }

        public int GetStaticDataVersion()
        {
            var path = Path.Combine(GetStaticFolder(), "version.txt");
            if (!File.Exists(path)) return 0;

            var text = File.ReadAllText(path);
            return text.ToIntegerSafe();
        }
    }
}
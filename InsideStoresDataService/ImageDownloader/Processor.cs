using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ImageDownloader
{
    public class Processor
    {
        public class SingleFile
        {
            // var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", p.ProductID, file.SKU, file.Filename, file.Shape, file.Url, file.CacheFilepath);

            public int ProductID { get; set; }
            public string SKU { get; set; }
            public string Filename { get; set; }
            public string Shape { get; set; }
            public string Url { get; set; }
            public string CacheFilename { get; set; }
            public string Line { get; set; }
            public SingleFile()
            {

            }

            public SingleFile(string line)
            {
                this.Line = line;

                var ary = line.Split('|');

                if (ary.Length != 6)
                    throw new Exception();

                ProductID = int.Parse(ary[0]);
                SKU = ary[1];
                Filename = ary[2];
                Shape = ary[3];
                Url = ary[4];
                CacheFilename = ary[5];
            }
        }

        private int missingImageCount = 0;


        private static object lockObj = new object();

        private const string CacheFolder = @"D:\InsideRugs-Dev\images\product\Cache";
        private const string LogFilepath = @"D:\InsideRugs-Dev\images\product\log.txt";
        private const string InputFilepath = @"D:\InsideRugs-Dev\images\product\files.txt";
        private const string DuplicatesFilepath = @"D:\InsideRugs-Dev\images\product\duplicate-files.txt";

        //private const string CacheFolder = @"Z:\Temp\JPdownload\Cache";
        //private const string LogFilepath = @"Z:\Temp\JPdownload\log.txt";
        //private const string InputFilepath = @"Z:\Temp\JPdownload\files.txt";
        //private const string DuplicatesFilepath = @"Z:\Temp\JPdownload\duplicate-files.txt";


        public void CreatMissingFileList(string vendorPrefix)
        {
            var inputFile = File.ReadAllLines(InputFilepath);
            var allFiles = inputFile.Select(line => new SingleFile(line)).ToList();

            var targetSkuPrefixWithHyphen = vendorPrefix + "-";
            var vendorFiles = allFiles.Where(e => e.SKU.StartsWith(targetSkuPrefixWithHyphen)).ToList();
            var missingFiles = new List<SingleFile>();

            foreach (var file in vendorFiles)
            {
                var filepath = Path.Combine(CacheFolder, file.CacheFilename);
                if (!File.Exists(filepath))
                {
                    missingFiles.Add(file);
                }
            }

            var outputLines = new List<string>();

            foreach (var missingFile in missingFiles)
                outputLines.Add(missingFile.Line);

            var outputFilepath = string.Format("D:\\InsideRugs-Dev\\images\\product\\missing-{0}.txt", vendorPrefix.ToLower());
            File.Delete(outputFilepath);
            File.WriteAllLines(outputFilepath, outputLines);
            Console.WriteLine(string.Format("Missing files Identified: {0:N0}", missingFiles.Count()));
        }


        public void CreatMissingFileList()
        {
            var inputFile = File.ReadAllLines(InputFilepath);
            var allFiles = inputFile.Select(line => new SingleFile(line)).ToList();

           
            var missingFiles = new List<SingleFile>();

            foreach (var file in allFiles)
            {
                var filepath = Path.Combine(CacheFolder, file.CacheFilename);
                if (!File.Exists(filepath))
                {
                    missingFiles.Add(file);
                }
            }

            var outputLines = new List<string>();

            foreach (var missingFile in missingFiles)
                outputLines.Add(missingFile.Line);

            var outputFilepath = "D:\\InsideRugs-Dev\\images\\product\\missing.txt";
            File.Delete(outputFilepath);
            File.WriteAllLines(outputFilepath, outputLines);
            Console.WriteLine(string.Format("Missing files Identified: {0:N0}", missingFiles.Count()));
        }


        public void ProcessFiles()
        {
            var inputFile = File.ReadAllLines(InputFilepath);
            var fileList = inputFile.Select(line => new SingleFile(line)).ToList();
            fileList.Shuffle();

            Console.WriteLine(string.Format("Total files: {0:N0}", fileList.Count()));

            HashSet<string> cachedFiles = new HashSet<string>();
            foreach (var filename in Directory.GetFiles(CacheFolder).Select(e => Path.GetFileName(e)).ToList())
                cachedFiles.Add(filename);


            var distinctUrls = fileList.Select(e => e.Url).Distinct().ToList();


            Console.WriteLine(string.Format("Duplicate Urls: {0:N0}", fileList.Count() - distinctUrls.Count()));

            var dicDistinctFiles = new Dictionary<string, SingleFile>();
            var duplicateFiles = new List<SingleFile>();

            foreach (var file in fileList)
            {
                if (!dicDistinctFiles.ContainsKey(file.Url))
                {
                    dicDistinctFiles[file.Url] = file;
                }
                else
                {
                    duplicateFiles.Add(file);
                }

            }

#if false
            using (StreamWriter sw = File.AppendText(DuplicatesFilepath))
            {
                foreach(var file in duplicateFiles)
                {
                    var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", file.ProductID, file.SKU, file.Filename, file.Shape, file.Url, file.CacheFilename);
                    sw.WriteLine(line);
                }
            }
#endif
            var countExisting = 0;

            foreach (var file in dicDistinctFiles.Values)
            {
                if (cachedFiles.Contains(file.CacheFilename))
                    countExisting++;
            }

            Console.WriteLine(string.Format("Distinct Urls: {0:N0}", dicDistinctFiles.Count()));

            Console.WriteLine(string.Format("Existing files: {0:N0}", countExisting));

            Console.WriteLine(string.Format("Remaining files to download: {0:N0}", dicDistinctFiles.Count() - countExisting));

            var referencedCacheFilenames = new HashSet<string>(dicDistinctFiles.Values.Select(e => e.CacheFilename.ToLower()));
            var unreferencedFiles = cachedFiles.Where(e => !referencedCacheFilenames.Contains(e.ToLower())).ToList();
            Console.WriteLine(string.Format("Deleting {0:N0} unreferenced existing files.", unreferencedFiles.Count()));
            foreach (var f in unreferencedFiles)
            {
                var filepath = Path.Combine(CacheFolder, f);
                File.Delete(filepath);
            }

#if true
            foreach (var file in fileList)
                ProcessSingleFile(file);
#else

            Parallel.ForEach(dicDistinctFiles.Values, (file) =>
                {
                   ProcessSingleFile(file);
                });
#endif

            var msg = string.Format("Missing images: {0:N0}", missingImageCount);
            Console.WriteLine(msg);
            Log(msg);
        }

        public void ProcessSingleFile(SingleFile file)
        {
            try
            {
                var dtStart = DateTime.Now;

                var filepath = Path.Combine(CacheFolder, file.CacheFilename);

                if (File.Exists(filepath))
                    return;

                var image = file.Url.GetImageFromWeb();
                if (image == null)
                {
                    Console.WriteLine(string.Format("Null image: {0}", Path.GetFileName(file.Url)));
                    Log(file, "Null Image");
                    missingImageCount++;
                    return;
                }

                if (!image.HasImagePreamble())
                {
                    Console.WriteLine(string.Format("Invalid image: {0}", Path.GetFileName(file.Url)));

                    Log(file, "Invalid Image");
                    missingImageCount++;
                    return;
                }

                image.WriteBinaryFile(filepath);

                var dtEnd = DateTime.Now;

                Console.WriteLine(string.Format("Downloaded: {0}    ({1})", Path.GetFileName(file.Url), dtEnd - dtStart));

            }
            catch(Exception Ex)
            {
                Console.WriteLine(string.Format("Error: {0}", Path.GetFileName(file.Url)));
                var msg = string.Format("Error: {0}", Ex.Message);
                Log(file, msg);
                missingImageCount++;
                return;
            }

        }


        public void ProcessSingleFileJaipur(SingleFile file)
        {
            try
            {
                var dtStart = DateTime.Now;

                var cacheFilepath = Path.Combine(CacheFolder, file.CacheFilename);

                if (File.Exists(cacheFilepath))
                    return;

                byte[] image = null ; // get image from jp folder

                string jpFolderRoot = @"D:\InsideRugs-Dev\images\product\jp";

                var uri = new Uri(file.Url, UriKind.RelativeOrAbsolute);
                var filename = HttpUtility.UrlDecode(uri.AbsolutePath).Substring(1).Replace("/", "\\").Replace(" LRes", " HRes");

                var filepath = Path.Combine(jpFolderRoot, filename);
                var exists = (File.Exists(filepath));

                if (exists)
                {
                    image = filepath.ReadBinaryFile();

                    if (image == null)
                    {
                        Console.WriteLine(string.Format("Null image: {0}", Path.GetFileName(file.Url)));
                        Log(file, "Null Image");
                        return;
                    }

                    if (!image.HasImagePreamble())
                    {
                        Console.WriteLine(string.Format("Invalid image: {0}", Path.GetFileName(file.Url)));

                        Log(file, "Invalid Image");
                        return;
                    }

                    image.WriteBinaryFile(cacheFilepath);

                }
                else
                {
                    missingImageCount++;
                    Console.WriteLine(string.Format("Missing Image: {0}", Path.GetFileName(file.Url)));
                    Log(file, "Missing Image");
                }


                var dtEnd = DateTime.Now;

                Console.WriteLine(string.Format("Copied: {0}    ({1})", Path.GetFileName(file.Url), dtEnd - dtStart));

            }
            catch (Exception Ex)
            {
                Console.WriteLine(string.Format("Error: {0}", Path.GetFileName(file.Url)));
                var msg = string.Format("Error: {0}", Ex.Message);
                Log(file, msg);
                return;
            }

        }

        public string MakeCacheFilename(string url)
        {
            // NOTE - for Jaipur, the orignal cache name for utility was based on having LRes in the URL. This will
            // need to be manipulated such that when the code is fixed to correctly look for HRes, that we deal with
            // the fact that the cachename for cached files is based on the LRes and not Hres. So if not handled,
            // we'll think we don't have an image that we actually do have.


            // SUGGEST - recompute hash filename using HRes and then rename the files in the cache folder.
            // but can do this only after the Product.Ext4 data uses references to HRes rather than LRes.

            var lowerUrl = url.ToLower();
            var digest = lowerUrl.SHA256Digest().ToLower();
            var ext = Path.GetExtension(url).ToLower();
            var filename = string.Format("{0}{1}", digest, ext);
            return filename;
        }


        public void Log(string msg)
        {
            lock (lockObj)
            {
                using (StreamWriter sw = File.AppendText(LogFilepath))
                {
                    sw.WriteLine(msg);
                }
            }
        }


        public void Log(SingleFile file, string msg)
        {
            lock (lockObj)
            {
                using (StreamWriter sw = File.AppendText(LogFilepath))
                {
                    var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", msg, file.ProductID, file.SKU, file.Filename, file.Shape, file.Url, file.CacheFilename);
                    sw.WriteLine(line);
                }
            }
        }
    }
}

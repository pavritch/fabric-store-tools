using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Security;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using InsideFabric.Data;

namespace Website
{
    public class ProductInfo
    {
        public int ProductID { get; set; }
        public string Filename { get; set; }

        // for when the store supports Ext4 data structures, else null
        public Dictionary<string, object> Ext4 { get; set; }
    }

    public class SanitizeImages
    {
        private int countDeletedFiles = 0;
        private int countClearedReferences = 0;

        private string PathWebsiteRoot
        {
            get
            {
                var websiteRoot = Store.PathWebsiteRoot;
//#if DEBUG
//                if (Store.StoreKey == StoreKeys.InsideAvenue)
//                    websiteRoot = @"D:\IA-WebV8-June15-2013\WebV8Copy";
//                else if (Store.StoreKey == StoreKeys.InsideFabric)
//                    websiteRoot = @"D:\IF-WebV8-Dec27-2012\WebV8Copy";
//                else if (Store.StoreKey == StoreKeys.InsideWallpaper)
//                    websiteRoot = @"D:\IF-WebV8-Dec27-2012\WebV8Copy";
//                else if (Store.StoreKey == StoreKeys.InsideRugs)
//                    websiteRoot = @"D:\IR-WebV8-Final\WebV8Copy";
//#endif
                return websiteRoot;
            }
        }

        public int CountDeletedFiles
        {
            get { return countDeletedFiles; }
        }

        public int CountClearedReferences
        {
            get { return countClearedReferences; }
        }

        public int CountSanitizedJpg { get; set; }
        public List<string> DuplicateFilenames { get; private set; }

        /// <summary>
        /// This includes both ImageFilenameOverride AND any in the "available" files in the extension data (when exists).
        /// </summary>
        public Dictionary<string, ProductInfo> AllKnownFilenamesInSQL { get; private set; }




        /// <summary>
        /// Direct accessor for micro files.
        /// </summary>
        public Dictionary<string, int> MicroFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Micro"))
                    throw new Exception("Looking for Micro in Store.ImageFolderNames.");

                return FolderFiles["Micro"];
            }
        }

        /// <summary>
        /// Direct accessor for mini files.
        /// </summary>
        public Dictionary<string, int> MiniFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Mini"))
                    throw new Exception("Looking for Mini in Store.ImageFolderNames.");

                return FolderFiles["Mini"];
            }
        }


        /// <summary>
        /// Direct accessor for icon files.
        /// </summary>
        public Dictionary<string, int> IconFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Icon"))
                    throw new Exception("Looking for Icon in Store.ImageFolderNames.");

                return FolderFiles["Icon"];
            }
        }

        /// <summary>
        /// Direct accessor for small files.
        /// </summary>
        public Dictionary<string, int> SmallFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Small"))
                    throw new Exception("Looking for Small in Store.ImageFolderNames.");

                return FolderFiles["Small"];
            }
        }


        /// <summary>
        /// Direct accessor for medium files.
        /// </summary>
        public Dictionary<string, int> MediumFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Medium"))
                    throw new Exception("Looking for Medium in Store.ImageFolderNames.");

                return FolderFiles["Medium"];
            }
        }

        /// <summary>
        /// Direct accessor for large files.
        /// </summary>
        public Dictionary<string, int> LargeFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Large"))
                    throw new Exception("Looking for Large in Store.ImageFolderNames.");

                return FolderFiles["Large"];
            }
        }


        /// <summary>
        /// Direct accessor for original files.
        /// </summary>
        public Dictionary<string, int> OriginalFiles
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Original"))
                    throw new Exception("Looking for Original in Store.ImageFolderNames.");

                return FolderFiles["Original"];
            }
        }


        public string MediumImageFolder
        {
            get
            {
                if (!Store.ImageFolderNames.Contains("Medium"))
                    throw new Exception("Looking for Medium in Store.ImageFolderNames.");

                return Path.Combine(PathWebsiteRoot, "images\\product\\Medium");
            }
        }

        // key is name from folder list, value is a dic of found files in that folder
        public Dictionary<string,Dictionary<string, int>> FolderFiles;

        private readonly Dictionary<string, ProductInfo> Products;

        private readonly bool performModifications;
        private readonly CancellationToken cancelToken;

        private readonly IWebStore Store;

        public SanitizeImages(IWebStore store, bool performModifications, CancellationToken cancelToken)
        {
            this.Store = store;
            this.cancelToken = cancelToken;

            DuplicateFilenames = new List<string>();
            AllKnownFilenamesInSQL = new Dictionary<string, ProductInfo>();
            Products = new Dictionary<string, ProductInfo>();
            this.performModifications = performModifications;
        }

        public void Run()
        {
            countDeletedFiles = 0;
            countClearedReferences = 0;
            CountSanitizedJpg = 0;
            DuplicateFilenames.Clear();

            Debug.WriteLine("Populating image file lists.");
            PopulateFiles();

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                dc.CommandTimeout = 100000;

                Debug.WriteLine("Reading SQL data.");

                Func<string, Dictionary<string, object>> makeExt4 = (json) =>
                    {
                        if (string.IsNullOrWhiteSpace(json))
                            return null;

                        var extData = ExtensionData4.Deserialize(json);
                        return extData.Data;
                    };

                // 9/29/2015 changed to no longer exclude any existing (even if deleted or unpublished) product
                // bottom line - if the product is still in SQL in any form.... don't remove its images

                var rawProducts = (from p in dc.Products
                                select new ProductInfo()
                                {
                                    ProductID = p.ProductID,
                                    Filename = p.ImageFilenameOverride,
                                    Ext4 = makeExt4(p.ExtensionData4)
                                }
                ).ToList();

                Debug.WriteLine("Processing SQL data.");

                // compose a hash of all known files in sql from both the usual ImageFilenameOverride
                // and (when it exists), the available files noted in the extension data.

                // we want to be careful not to delete files that might be alternates and only known to the extension data

                foreach(var p in rawProducts)
                {
                    if (!string.IsNullOrWhiteSpace(p.Filename))
                        AllKnownFilenamesInSQL[p.Filename.ToLower()] = p;

                    if (p.Ext4 != null)
                    {

                        if (p.Ext4.ContainsKey(ExtensionData4.LiveProductImages))
                        {
                            var fileList = p.Ext4[ExtensionData4.LiveProductImages] as List<LiveProductImage>;

                            foreach (var f in fileList)
                                AllKnownFilenamesInSQL[f.Filename.ToLower()] = p;
                        }
                        else if (p.Ext4.ContainsKey(ExtensionData4.AvailableImageFilenames))
                        {
                            var fileList = p.Ext4[ExtensionData4.AvailableImageFilenames] as List<string>;

                            foreach(var f in fileList )
                                AllKnownFilenamesInSQL[f.ToLower()] = p;
                        }
                    }
                }

                //var names = new HashSet<string>();

                //foreach (var p in rawProducts.Where(e => !string.IsNullOrEmpty(e.Filename)))
                //{
                //    if (!names.Contains(p.Filename))
                //    {
                //        names.Add(p.Filename);
                //        Products.Add(p.Filename.ToLower(), p);
                //    }
                //    else
                //    {
                //        Debug.WriteLine("Duplicate Filename: " + p.Filename);
                //        DuplicateFilenames.Add(p.Filename);
                //    }
                //}

                if (cancelToken.IsCancellationRequested)
                    return;

                if (performModifications)
                {
                    foreach(var imageFolder in Store.ImageFolderNames)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return;

                        Debug.WriteLine(string.Format("Removing unreferenced files: {0}", imageFolder));
                        DeleteUnreferencedImages(FolderFiles[imageFolder]);
                    }
                }

                if (cancelToken.IsCancellationRequested)
                    return;

                //Debug.WriteLine("Clearing references without matching images.");
                //ClearReferencesWithoutMatchingImage(rawProducts);
            }
        }

        /// <summary>
        /// Not called!
        /// </summary>
        /// <param name="rawProducts"></param>
        private void ClearReferencesWithoutMatchingImage(List<ProductInfo> rawProducts)
        {
            // DANGER!
            // DANGER!
            // DANGER!
            // As of 10/22/2015, call to this method removed since does not fully deal with
            // Ext4 List<LiveProductImage>, and has assumption that an image would always exist
            // in all folders (which as of this day is no longer the case). LiveProductImages collection
            // is considered to be the master knowledge base. Other logic (post processing, etc.) can use
            // whatever combination of folders is sensible for the task at hand. But when deleting a filename,
            // would generally figure to spin through all folders and make sure it is indeed gone.

            // note that this method was slightly recrafted on 11/19/2014 to account for Ext4 data, but did not
            // want to mess with what was already known to work - so might be slightly inefficient, but safe!

            // list of products which need image filename override cleared out
            var ProductIDs = new List<int>();

            // need to rebuild working filename set since some may have been deleted along the way

            // use the Medium folder (arbitrary) to match things up - assuming that in theory, any
            // filename in the medium folder is actually in all

            var filenameOnlyCollection = new Dictionary<string, int>();
            var mediumFiles = new HashSet<string>(Directory.GetFiles(MediumImageFolder));

            foreach (var file in mediumFiles)
                filenameOnlyCollection.Add(Path.GetFileName(file).ToLower(), 0);


            foreach (var p in Products)
            {
                if (!filenameOnlyCollection.ContainsKey(p.Value.Filename.ToLower()))
                {
                    countClearedReferences++;
                    ProductIDs.Add(p.Value.ProductID);
                }
            }

            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                foreach (var id in ProductIDs)
                {
                    if (cancelToken.IsCancellationRequested)
                        return;

                    if (performModifications)
                        dc.Products.UpdateImageFilenameOverride(id, null);
                }
            }

            // below added 11/19/2014 
            // now deal with extension available files

            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                foreach(var p in rawProducts)
                {
                    if (p.Ext4 == null || !p.Ext4.ContainsKey(ExtensionData4.AvailableImageFilenames))
                        continue;

                    var fileList = p.Ext4[ExtensionData4.AvailableImageFilenames] as List<string>;

                    bool _isExtensionData4Dirty = false;

                    Action markExtData4Dirty = () =>
                    {
                        _isExtensionData4Dirty = true;
                    };

                    Action<int> saveExtData4 = (id) =>
                    {
                        if (!_isExtensionData4Dirty)
                            return;

                        var extData = new ExtensionData4();
                        extData.Data = p.Ext4;

                        var json = extData.Serialize();

                        dc.Products.UpdateExtensionData4(id, json);
                        _isExtensionData4Dirty = false;
                    };

                    foreach (var f in fileList.ToList())
                    {
                        if (!filenameOnlyCollection.ContainsKey(f.ToLower()))
                        {
                            fileList.RemoveAll(e => e == f);
                            markExtData4Dirty();
                        }
                    }

                    if (performModifications)
                        saveExtData4(p.ProductID);
                }
            }
        }


        /// <summary>
        /// If the file is not referenced in SQL (ImageFilenameOverride or Ext data), then remove it.
        /// </summary>
        /// <param name="Files"></param>
        private void DeleteUnreferencedImages(Dictionary<string, int> Files)
        {
            foreach (var f in Files)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                var filename = Path.GetFileName(f.Key).ToLower();

                if (!AllKnownFilenamesInSQL.ContainsKey(filename))
                {
                    if (performModifications)
                        File.Delete(f.Key);

                    Interlocked.Increment(ref countDeletedFiles);
                }
            }
        }


        private void PopulateFiles()
        {
            FolderFiles = new Dictionary<string,Dictionary<string,int>>();

            // for each folder hosting images, create a collection of what files are there.

            foreach (var imageFolder in Store.ImageFolderNames)
            {
                var dic = new Dictionary<string, int>();

                var folderPath = Path.Combine(PathWebsiteRoot, "images\\product", imageFolder);
                foreach (var f in Directory.GetFiles(folderPath))
                    dic.Add(f, 0);

                FolderFiles.Add(imageFolder, dic);
            }
        }


    }
}

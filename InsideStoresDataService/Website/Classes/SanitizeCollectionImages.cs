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

    public class SanitizeCollectionImages
    {
        public class CollectionInfo
        {
            public int CollectionID { get; set; }
            public string Filename { get; set; }
        }

        private int countDeletedFiles = 0;

        private string PathWebsiteRoot
        {
            get
            {
                var websiteRoot = Store.PathWebsiteRoot;
                return websiteRoot;
            }
        }

        public int CountDeletedFiles
        {
            get { return countDeletedFiles; }
        }

        public List<string> CollectionFolderImages { get; set; }

        /// <summary>
        /// This includes ImageFilenameOverride
        /// </summary>
        public Dictionary<string, CollectionInfo> AllKnownFilenamesInSQL { get; private set; }


        /// <summary>
        /// This is the folder where all images should go relating to collections.
        /// </summary>
        /// <remarks>
        /// All have CollectionID in filename, so no worry about name collision.
        /// </remarks>
        protected string CollectionImageFolder
        {
            get
            {
                return Path.Combine(Store.PathWebsiteRoot, "images\\collections");
            }
        }


        //private readonly Dictionary<string, CollectionInfo> Products;
        private readonly bool performModifications;
        private readonly CancellationToken cancelToken;

        private readonly IWebStore Store;

        public SanitizeCollectionImages(IWebStore store, bool performModifications, CancellationToken cancelToken)
        {
            this.Store = store;
            this.cancelToken = cancelToken;

            // key is lower case imagefilenameoverride
            AllKnownFilenamesInSQL = new Dictionary<string, CollectionInfo>();

            // key is lower case filepath
            CollectionFolderImages = new List<string>();

            this.performModifications = performModifications;
        }

        public void Run()
        {
            countDeletedFiles = 0;

            CollectionFolderImages = Directory.GetFiles(CollectionImageFolder).ToList();

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                dc.CommandTimeout = 100000;

                var rawCollections = (from col in dc.ProductCollections where col.ImageFilenameOverride != null
                                        select new CollectionInfo()
                                        {
                                            CollectionID = col.ProductCollectionID,
                                            Filename = col.ImageFilenameOverride,
                                        }).ToList();

                foreach (var col in rawCollections)
                {
                    if (!string.IsNullOrWhiteSpace(col.Filename))
                        AllKnownFilenamesInSQL[col.Filename.ToLower()] = col;
                }

                if (cancelToken.IsCancellationRequested)
                    return;

                DeleteUnreferencedImages(CollectionFolderImages);
            }
        }


        /// <summary>
        /// If the file is not referenced in SQL, then remove it.
        /// </summary>
        /// <param name="Files"></param>
        private void DeleteUnreferencedImages(List<string> Files)
        {
            foreach (var f in Files)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                var filename = Path.GetFileName(f).ToLower();

                if (!AllKnownFilenamesInSQL.ContainsKey(filename))
                {
                    if (performModifications)
                        File.Delete(f);

                    Interlocked.Increment(ref countDeletedFiles);
                }
            }
        }



    }
}

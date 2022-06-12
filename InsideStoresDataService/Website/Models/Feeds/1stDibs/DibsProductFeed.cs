using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.IO;
using System.Text;
using Gen4.Util.Misc;
using System.Diagnostics;
using Ionic.Zip;
using System.Web.Configuration;

namespace Website
{
    [ProductFeed(ProductFeedKeys.FirstDibs)]
    public class DibsProductFeed : ProductFeed, IProductFeed
    {

        public DibsProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.FirstDibs, storeProductFeed)
        {
            Separator = Comma;
            DefaultFileFormat = ProductFeedFormats.csv;
        }

        #region Properties

        private const bool GENERATE_FILE_PIECES = true; // if true, additionally generated 15MB pieces, add to zip file
        private const bool GENERATE_FILE_NEWPRODUCTS = true; // if true, additionally generated file with only the new products

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during generation operation, else null.
        /// </remarks>
        public Dictionary<string, DibsFeedProduct> Products { get; private set; }

        private int countFilePieces = 0; // number of broken up file pieces created to stay under 15MB limit

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as DibsFeedProduct;

            if (!Products.TryAdd(product.SellerRefNo, product))
                throw new Exception(string.Format("Duplicate Dibs feed product Handle {0} for {1}.", product.SellerRefNo, Store.StoreKey));
        }

        protected override void PopulateProducts()
        {
            // spin through products for this store, calls AddProduct() for each that qualifies.
            StoreProductFeed.PopulateProducts(this);
        }


        /// <summary>
        /// Hook for begin generation.
        /// </summary>
        protected override void BeginGeneration()
        {
            Products = new Dictionary<string, DibsFeedProduct>();
        }


        /// <summary>
        /// Hook for end generation. Allow chance to clean up.
        /// </summary>
        protected override void EndGeneration()
        {
            Products = null;
            CleanUpFilePieces();
            File.Delete(MakeNewProductsFilename());
        }

        /// <summary>
        /// Write header and one row per product.
        /// </summary>
        /// <remarks>
        /// Called with already open file stream ready for writing.
        /// </remarks>
        /// <param name="file"></param>
        protected override void WriteFileContents(StreamWriter file)
        {
            WriteFieldNamesHeader<DibsFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord(file, product);

        }

        private string MakePieceFilename(int pieceNumber)
        {
            return Path.Combine(ProductFeedsRootPath, string.Format("{0}{1}ProductFeed-{3}.{2}", Store.StoreKey, Key.ToString(), DefaultFileFormat.ToString(), pieceNumber));
        }

        private string MakeNewProductsFilename(bool isCanada=false)
        {
            return Path.Combine(ProductFeedsRootPath, string.Format("{0}{1}ProductFeed-New{3}.{2}", Store.StoreKey, Key.ToString(), DefaultFileFormat.ToString(), isCanada ? "-Canada" : ""));
        }

        /// <summary>
        /// Dibs has limit of 15MB per file upload
        /// </summary>
        private int WriteTinyFiles()
        {
            int filePieceNumber = 0; // actual numbers start at 1
            long filesize = 0L;
            string filenamePiece;
            FileStream fs = null;
            StreamWriter file = null;

            Action closeFilePiece = () =>
                {
                    if (file != null)
                    {
                        file.Close();
                        file = null;
                    }

                    if (fs != null)
                    {
                        fs.Close();
                        fs = null;
                    }

                    filesize = 0L;
                };

            foreach (var product in Products.Values)
            {
                if (fs == null)
                {
                    filenamePiece = MakePieceFilename(++filePieceNumber);

                    if (File.Exists(filenamePiece))
                        File.Delete(filenamePiece);

                    fs = new FileStream(filenamePiece, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 64);
                    file = new StreamWriter(fs, new System.Text.UTF8Encoding());

                    filesize += WriteFieldNamesHeader<DibsFeedProduct>(file);

                }

                // we now have open file ready to write records (header supplied)

                filesize += WriteProductRecord(file, product);

                // if size maxed out, close it
                if (filesize > (1024 * 1024 * 14)) // 14MB for safety
                    closeFilePiece();

            }

            // close final file if still open
            closeFilePiece();

            // number of pieces written out (filenames are numbered 1 to N)
            return filePieceNumber;
        }

        protected override int WriteProductRecord<T>(StreamWriter file, T product)
        {
            var sb = new StringBuilder(1024 * 10);
            int countFields = 0;

            Action<string, bool> add = (s, isSeparator) =>
            {
                bool hasEmbeddedQuotes = false;
                string s2;
                if (string.IsNullOrWhiteSpace(s))
                    s2 = string.Empty;
                else
                    s2 = s.Trim().Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");

                if (s2.ContainsIgnoreCase("\""))
                {
                    hasEmbeddedQuotes = true;
                    s2 = s2.Replace("\"", "\"\"");
                }
                
                try
                {
                    if (s2.Contains(',') || hasEmbeddedQuotes)
                        sb.AppendFormat("\"{0}\"", s2);
                    else
                        sb.AppendFormat(s2);

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);
                    throw;
                }

                if (isSeparator)
                    sb.Append(Separator);

                countFields++;
            };


            var properties = typeof(T).MemberProperties().Where(e => e.HasAttribute(typeof(CsvFieldAttribute))).ToList();
            var lastIndex = properties.Count - 1;

            for (int i = 0; i <= lastIndex; i++)
            {
                var pi = (PropertyInfo)properties[i];

                var value = (string)pi.GetValue(product, null);

                // do not add tab after last field

                add(value, (i != lastIndex));
            }

            // write out the single line for this product

            file.WriteLine(sb.ToString());

            return sb.Length + 2; // account for CrLf
        }


        /// <summary>
        /// Optional post processing to be performed.
        /// </summary>
        /// <param name="feedFilePath"></param>
        protected override void PostFileProcessing(string feedFilePath)
        {
            if (!File.Exists(feedFilePath))
                return;

            var info = new FileInfo(feedFilePath);

            
            // Dibs needs files less than 15MB, so create an alternate set of smaller CSV files
            // make pieces if size if more than 14MB
            if (info.Length > (1024 * 1025 * 14) && GENERATE_FILE_PIECES)
                countFilePieces = WriteTinyFiles();

        }

        /// <summary>
        /// Opportunity to add files to zip
        /// </summary>
        /// <param name="zip"></param>
        protected override void AddFilesToZip(ZipFile zip)
        {
            // only add the subfolder for parts if created more than one

            if (countFilePieces > 1)
            {
                for (int piece = 1; piece <= countFilePieces; piece++)
                {
                    var pieceFilename = MakePieceFilename(piece);
                    if (File.Exists(pieceFilename))
                        zip.AddFile(pieceFilename, "\\Parts");
                }
            }


            // if there is a file having just the new products for each of US and CA

            foreach(var isCanada in new bool[] {false, true})
            {
                var newProductsFilename = MakeNewProductsFilename(isCanada);
                if (File.Exists(newProductsFilename))
                    zip.AddFile(newProductsFilename, "\\NewProducts");
            }
        }


        private void CleanUpFilePieces()
        {
            if (countFilePieces > 0)
            {
                for (int piece = 1; piece <= countFilePieces; piece++)
                    File.Delete(MakePieceFilename(piece));

                countFilePieces = 0;
            }
        }


        /// <summary>
        /// Used for debugging to cap off the max number of products.
        /// </summary>
        public override bool IsMaxedOut
        {
            get
            {
#if false
                if (Products.Count > 5000)
                    return true;
#endif
                return false;
            }
        }


    }
}
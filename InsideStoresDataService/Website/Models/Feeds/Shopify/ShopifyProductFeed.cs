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
    [ProductFeed(ProductFeedKeys.Shopify)]
    public class ShopifyProductFeed : ProductFeed, IProductFeed
    {

        public ShopifyProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.Shopify, storeProductFeed)
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
        public Dictionary<string, ShopifyFeedProduct> Products { get; private set; }

        private int countFilePieces = 0; // number of broken up file pieces created to stay under 15MB limit

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as ShopifyFeedProduct;

            if (!Products.TryAdd(product.Handle, product))
                throw new Exception(string.Format("Duplicate Shopify feed product Handle {0} for {1}.", product.Handle, Store.StoreKey));
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
            Products = new Dictionary<string, ShopifyFeedProduct>();
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
            WriteFieldNamesHeader<ShopifyFeedProduct>(file);

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
        /// Shopify has limit of 15MB per file upload
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

                    filesize += WriteFieldNamesHeader<ShopifyFeedProduct>(file);

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

            
            // shopify needs files less than 15MB, so create an alternate set of smaller CSV files
            // make pieces if size if more than 14MB
            if (info.Length > (1024 * 1025 * 14) && GENERATE_FILE_PIECES)
                countFilePieces = WriteTinyFiles();

            if (GENERATE_FILE_NEWPRODUCTS)
            {
                CreateNewProductsFile(isCanada:false);
                CreateNewProductsFile(isCanada: true);
            }
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

        protected void CreateNewProductsFile(bool isCanada=false)
        {
            Func<string, string> makeStoreKey = (key) =>
            {
                //<add key="ShopifyProductFilePath" value="c:\temp\ShopifyProducts.json" />
                //<add key="ShopifyProductFilePath-CA" value="c:\temp\ShopifyProducts-CA.json" />
                if (isCanada)
                    return key + "-CA";
                return key;
            };

            // use the file created by the download/update tools as a filter to only select
            // products which are not already on shopify.

            // There is a single file which has data for all stores. Need to filter on the store at hand.

            var existingShopifyProductsFilepath = WebConfigurationManager.AppSettings[makeStoreKey("ShopifyProductFilePath")];
            var CanadaMarkup = double.Parse(WebConfigurationManager.AppSettings["PriceMarkup-CA"]);

            var shopifyProductInformationManager = new ShopifyCommon.ProductInformationManager(existingShopifyProductsFilepath);
            shopifyProductInformationManager.Load();

            var existingProducts = new HashSet<int>(shopifyProductInformationManager.Products.Where(e => (int)e.Store == (int)Store.StoreKey && e.Status != ShopifyCommon.ProductStatus.Deleted).Select(e => e.ProductID));

            var filename = MakeNewProductsFilename(isCanada);

            if (File.Exists(filename))
                File.Delete(filename);

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 64))
            {
                using (var file = new StreamWriter(fs, new System.Text.UTF8Encoding()))
                {
                    WriteFieldNamesHeader<ShopifyFeedProduct>(file);

                    foreach (var product in Products.Values)
                    {
                        var productID = int.Parse(product.GoogleShoppingCustomLabel4);
                        if (!existingProducts.Contains(productID))
                        {
                            if (isCanada)
                            {
                                product.VariantPrice = Math.Round(double.Parse(product.VariantPrice) * CanadaMarkup, 2).ToString();
                                //product.VariantCompareAtPrice is presently always left empty so no need to touch
                            }
                            WriteProductRecord(file, product);
                        }
                    }
                }
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
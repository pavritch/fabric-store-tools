using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using Gen4.Util.Misc;
using Ionic.Zip;
using System.Text;
using System.Reflection;

namespace Website
{
    public abstract class ProductFeed
    {
        protected const char Tab = '\t';
        protected const char Comma = ',';

        protected char Separator = Tab;
        protected bool IsQuotedStrings { get; set; }
        protected bool isBusy;
        protected bool isCancel; 

        private static string productFeedsRootPath;

        static ProductFeed()
        {
            productFeedsRootPath = ConfigurationManager.AppSettings["ProductFeedsRootPath"];
        }

        public ProductFeed(ProductFeedKeys key, IStoreProductFeed storeProductFeed)
        {
            isBusy = false;
            isCancel = false;
            IsQuotedStrings = false;

            this.DefaultFileFormat = ProductFeedFormats.txt;
            this.Key = key;
            this.StoreProductFeed = storeProductFeed;
        }

        public ProductFeedKeys Key { get; private set; }
        protected IStoreProductFeed StoreProductFeed {get; private set;}

        public IWebStore Store { get { return StoreProductFeed.Store; } }

        /// <summary>
        /// The kind of file txt|csv which is generated.
        /// </summary>
        /// <remarks>
        /// Most are tab-del txt files, but shopify is comma-del csv
        /// </remarks>
        public ProductFeedFormats DefaultFileFormat { get; protected set; }

        protected string ProductFeedsRootPath
        {
            get
            {
                return productFeedsRootPath;
            }
        }

        public virtual string FeedFilePath
        {
            get
            {
                var filename = string.Format("{0}{1}ProductFeed.{2}", Store.StoreKey, Key.ToString(), DefaultFileFormat.ToString());
                return Path.Combine(ProductFeedsRootPath, filename);
            }
        }

        public bool IsBusy
        {
            get
            {
                lock (this)
                {
                    return isBusy;
                }
            }
        }

        /// <summary>
        /// Used for debugging to cap off the max number of products.
        /// </summary>
        public virtual bool IsMaxedOut
        { 
            get
            {
                return false;
            }
        }

        
        public bool IsNeedToGenerate()
        {
            return !File.Exists(FeedFilePath);
        }

        /// <summary>
        /// Start the generation of this feed - synchronous.
        /// </summary>
        /// <returns></returns>
        public virtual bool Generate()
        {
            lock (this)
            {
                if (isBusy)
                    return false;

                isCancel = false;
                isBusy = true;
            }

            try
            {
                var startTime = DateTime.Now;
                Generate(FeedFilePath);
                var timeToGenerate = DateTime.Now - startTime;

                var msg = string.Format("Successfully generated {0} product feed for {1}.\nFile: {2}\nTime to generate: {3}", Key, Store.StoreKey, FeedFilePath, timeToGenerate.ToString(@"dd\.hh\:mm\:ss"));
                new WebsiteApplicationLifetimeEvent(msg, this, WebsiteEventCode.Notification).Raise();
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                var ev2 = new WebsiteRequestErrorEvent(string.Format("Exception generating product feed: {0}.", FeedFilePath), this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                return false;
            }
            finally
            {
                lock (this)
                {
                    isBusy = false;
                    isCancel = false;
                }
            }

        }

        protected string UniqueFilenameIdentifier()
        {
            // used to help form the temp filenames
            return Guid.NewGuid().ToString().Replace("-", "").Left(12);
        }

        public void CancelOperation()
        {
            lock (this)
            {
                isCancel = true;
            }

        }

        protected virtual void Generate(string feedFilePath)
        {
            string tempFilePath = null;

            try
            {
                // use a temporary path during generation, and rename to standard name upon successful completion

                var folder = Path.GetDirectoryName(feedFilePath);
                tempFilePath = Path.Combine(folder, string.Format("{0}{1}-{2}.tmp", Store.StoreKey, Key, UniqueFilenameIdentifier()));

                BeginGeneration();

                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                PopulateProducts();

                // this is a safety-stop just in case while during generation the system started

                lock (this)
                {
                    if (isCancel || Store.IsRebuildingCategories)
                        throw new Exception("Feed generation cancelled.");
                }

                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 64))
                {
                    using (var file = new StreamWriter(fs, new System.Text.UTF8Encoding()))
                    {
                        WriteFileContents(file);
                    }
                }

                lock (this)
                {
                    if (isCancel || Store.IsRebuildingCategories)
                        throw new Exception("Feed generation cancelled.");
                }

                // replace the existing file with the temporary working copy

                if (File.Exists(feedFilePath))
                    File.Delete(feedFilePath);

                File.Move(tempFilePath, feedFilePath);

                // any final/optional post processing
                PostFileProcessing(feedFilePath);

                // zip it up
                MakeZipFile(feedFilePath);

            }
            finally
            {
                EndGeneration();

                // clean up temp file if exists - which is only on errors
                // since is renamed on success

                if (tempFilePath != null && File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        protected void MakeZipFile(string feedFilePath)
        {
            var folder = Path.GetDirectoryName(feedFilePath);

            var filename = Path.GetFileName(feedFilePath);
            var zipFilename = Path.ChangeExtension(filename, ".zip");
            var zipFilePath = Path.Combine(folder, zipFilename);

            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            using (var zip = new ZipFile())
            {
                // add file to the root of the zip
                zip.AddFile(feedFilePath, "\\");
                AddFilesToZip(zip); // default does nothing
                zip.Save(zipFilePath);
            }
        }

        /// <summary>
        /// Opportunity to add files to zip
        /// </summary>
        /// <param name="zip"></param>
        protected virtual void AddFilesToZip(ZipFile zip)
        {
            // default do nothing
        }

        /// <summary>
        /// Optional post processing to be performed.
        /// </summary>
        /// <param name="feedFilePath"></param>
        protected virtual void PostFileProcessing(string feedFilePath)
        {
            // default do nothing
        }

        protected abstract void PopulateProducts();


        /// <summary>
        /// Write header and one row per product.
        /// </summary>
        /// <remarks>
        /// Called with already open file stream ready for writing.
        /// </remarks>
        /// <param name="file"></param>
        protected abstract void WriteFileContents(StreamWriter file);

        protected int WriteFieldNamesHeader<T>(StreamWriter file)
        {
            var sb = new StringBuilder(2048);

            Action<string, bool> add = (s, isSeparator) =>
            {
                // quote all fields, escape embedded quotes with two quotes
                if (IsQuotedStrings)
                    sb.AppendFormat("\"{0}\"", s.Replace("\"", "\"\""));
                else
                    sb.Append(s);

                if (isSeparator)
                    sb.Append(Separator);
            };

            // use reflection to get the field names from the attributes on FeedProduct

            var properties = typeof(T).MemberProperties().Where(e => e.HasAttribute(typeof(CsvFieldAttribute))).ToList();
            var lastIndex = properties.Count - 1;

            //Debug.WriteLine(string.Format("\nFeed columns: {0}", Key));

            for (int i = 0; i <= lastIndex; i++)
            {
                var pi = (PropertyInfo)properties[i];

                var csvAttribute = pi.Attributes<CsvFieldAttribute>().FirstOrDefault();

                Debug.Assert(pi.PropertyType == typeof(string));
                Debug.Assert(csvAttribute != null);

                // do not add tab after last field

                // Debug.WriteLine(string.Format("      {0}", csvAttribute.Name));

                add(csvAttribute.Name, (i != lastIndex));
            }

            file.WriteLine(sb.ToString());

            //Debug.WriteLine("\n\n");

            return sb.Length + 2; // account for CrLf
        }

        /// <summary>
        /// Writes out a single record.
        /// </summary>
        /// <typeparam name="T">of type FeedProduct</typeparam>
        /// <param name="file">Strean to write to</param>
        /// <param name="product">target product</param>
        /// <returns>Number of chars written out.</returns>
        protected virtual int WriteProductRecord<T>(StreamWriter file, T product)
        {
            // Shopify has override

            var sb = new StringBuilder(2048);
            int countFields = 0;

            Action<string, bool> add = (s, isSeparator) =>
            {
                // quote all fields, escape embedded quotes with two quotes
                // make sure does not contain any embedded tabs

                string s2;
                if (string.IsNullOrWhiteSpace(s))
                    s2 = string.Empty;
                else
                    s2 = s.Trim().Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");

                try
                {

                    if (IsQuotedStrings)
                        sb.AppendFormat("\"{0}\"", s2.Replace("\"", "\"\""));
                    else
                        sb.AppendFormat(s2);

                }
                catch(Exception Ex)
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
        /// Hook for begin generation.
        /// </summary>
        protected virtual void BeginGeneration()
        {

        }


        /// <summary>
        /// Hook for end generation. Allow chance to clean up.
        /// </summary>
        protected virtual void EndGeneration()
        {

        }


    }
}
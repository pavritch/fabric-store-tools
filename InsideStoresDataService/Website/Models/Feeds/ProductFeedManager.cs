using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace Website
{
    public class ProductFeedManager : IProductFeedManager
    {
        #region Locals
        private static string productFeedsRootPath;

        #endregion


        static ProductFeedManager()
        {
            productFeedsRootPath = ConfigurationManager.AppSettings["ProductFeedsRootPath"];
        }


        #region Properties

        protected List<IProductFeed> ProductFeeds { get; private set; }

        public IWebStore Store { get; private set; }

        #endregion

        public ProductFeedManager(IWebStore store)
        {
            Store = store;
            ProductFeeds = new List<IProductFeed>();
        }


        protected void AddFeed(IProductFeed feed)
        {
            ProductFeeds.Add(feed);
        }

        public bool Generate(ProductFeedKeys feedKey)
        {
            var feed = ProductFeeds.Where(e => e.Key == feedKey).FirstOrDefault();

            if (feed == null)
                return false;

            return feed.Generate();
        }

        public IEnumerable<IProductFeed> RegisteredFeeds
        {
            get
            {
                return ProductFeeds;
            }
        }


        /// <summary>
        /// Delete left-over temporary files for this specific store.
        /// </summary>
        /// <remarks>
        /// Generally should not see temp files left around since all are cleaned up at the end
        /// of a cycle, however, due to process restarts, etc., it's possible. So we just 
        /// sweep through and self-clean at the start of a cycle. Note also that temp files
        /// all have random names, so even a locked temp file will not cause a cycle to bomp
        /// due to file locks.
        /// </remarks>
        protected virtual void DeleteTemporaryFiles()
        {
            var filespec = string.Format("{0}*.tmp", Store.StoreKey); 

            var tmpFiles = Directory.GetFiles(productFeedsRootPath, filespec, SearchOption.TopDirectoryOnly);

            foreach (var file in tmpFiles) 
                File.Delete(file);
        }

        public virtual void GenerateMissingFeeds()
        {
            DeleteTemporaryFiles();
            foreach (var feed in ProductFeeds.Where(e => e.IsNeedToGenerate()))
                MvcApplication.Current.FeedManager.EnqueueFeedForGeneration(feed);
        }

        /// <summary>
        /// Regenerate all product feeds.
        /// </summary>
        /// <returns></returns>
        public virtual void GenerateAll()
        {
            DeleteTemporaryFiles();
            foreach (var feed in ProductFeeds)  
                MvcApplication.Current.FeedManager.EnqueueFeedForGeneration(feed);
        }
    }
}
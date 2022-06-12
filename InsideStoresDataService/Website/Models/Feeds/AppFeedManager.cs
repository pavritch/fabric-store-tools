using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amib.Threading;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

namespace Website
{
    public class AppFeedManager
    {
        private class FeedWorkItem
        {
            public IProductFeed ProductFeed { get; set; }
            public FeedWorkItem(IProductFeed feed)
            {
                ProductFeed = feed;
            }
        }

        private readonly SmartThreadPool threadPool;

        public AppFeedManager()
        {

            var maxProductFeedGeneratorThreads = int.Parse(ConfigurationManager.AppSettings["MaxProductFeedGeneratorThreads"]);

            var poolInfo = new STPStartInfo()
            {
                MinWorkerThreads = 1,
                MaxWorkerThreads = maxProductFeedGeneratorThreads,
                IdleTimeout = 60 * 1000,
            };

            threadPool = new SmartThreadPool(poolInfo);

#if !DEBUG
            // each time this application fires up we wish to ensure
            // that we're not missing any feeds - wait a tiny bit to
            // give the rest of the app a chance to settle down
            // and then build any which are missing

            var thWorker = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000 * 60 * 2);
                GenerateMissingFeeds();
            }));
            thWorker.Start();
#endif
        }

        public void EnqueueFeedForGeneration(IProductFeed feed)
        {
            var workItem = new FeedWorkItem(feed);
            threadPool.QueueWorkItem(new Amib.Threading.Func<FeedWorkItem, bool>(FeedGeneratorWorkerThread), workItem);
        }

        public bool IsAnyFeedGenerating()
        {
            foreach (var store in MvcApplication.Current.WebStores.Values)
            {
                if (store.ProductFeedManager.RegisteredFeeds.Any(e => e.IsBusy))
                    return true;
            }

            return false;
        }

        public void GenerateAllFeeds()
        {
#if false // test a single feed
            var store = MvcApplication.Current.WebStores.Values.Where(e => e.StoreKey == StoreKeys.InsideRugs).Single();
            store.ProductFeedManager.Generate(ProductFeedKeys.Google);
#else
            foreach (var store in MvcApplication.Current.WebStores.Values)
                store.ProductFeedManager.GenerateAll();
#endif
        }

        public void GenerateMissingFeeds()
        {
            foreach (var store in MvcApplication.Current.WebStores.Values)
                store.ProductFeedManager.GenerateMissingFeeds();
        }


       /// <summary>
        /// Main processor which processes the task queue.
        /// </summary>
        /// <param name="reqItem"></param>
        /// <returns></returns>
        private bool FeedGeneratorWorkerThread(FeedWorkItem reqItem)
        {
            try
            {
                var feed = reqItem.ProductFeed;

                // do not run if already running, do not run if rebuilding categories
                // as such could have bad results.

                if (!feed.IsBusy && !feed.Store.IsRebuildingCategories)
                    reqItem.ProductFeed.Generate();
            }
            catch(Exception Ex)
            {
                Debug.WriteLine("Feed exception: " + Ex.ToString());
            }

            return true;
        }
    }
}
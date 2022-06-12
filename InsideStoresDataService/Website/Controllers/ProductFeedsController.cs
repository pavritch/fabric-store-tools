using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Configuration;
using System.IO;
using Gen4.Util.Misc;

namespace Website.Controllers
{
    public class ProductFeedsController : Controller
    {

        // GET: /ProductFeeds/

        public ActionResult Download(StoreKeys storeKey, ProductFeedKeys feedKey, ProductFeedFormats? feedFormat=ProductFeedFormats.txt)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var feed = store.ProductFeedManager.RegisteredFeeds.Where(e => e.Key == feedKey).FirstOrDefault();

            if (feed == null || !System.IO.File.Exists(feed.FeedFilePath))
                return new HttpNotFoundResult();

            // switch to the right extension
            var filepath = Path.ChangeExtension(feed.FeedFilePath, (feedFormat ?? feed.DefaultFileFormat).ToString());

            var msg = string.Format("Product feed downloaded.\nStore: {0}\nFeed: {1}\nFile: {2}", store.StoreKey, feed.Key, filepath);
            new WebsiteApplicationLifetimeEvent(msg, this, WebsiteEventCode.Notification).Raise();

            return new FilePathResult(filepath, feedFormat.Description())
            {
                FileDownloadName = Path.GetFileName(filepath)
            };
        }

        public ActionResult GenerateAllProductFeeds()
        {
            MvcApplication.Current.FeedManager.GenerateAllFeeds();
            new WebsiteApplicationLifetimeEvent("Begin to generate all product feeds.", this, WebsiteEventCode.Notification).Raise();
            return View();
        }

        public ActionResult GenerateProductFeed(StoreKeys storeKey, ProductFeedKeys feedKey, bool? download)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var feed = store.ProductFeedManager.RegisteredFeeds.Where(e => e.Key == feedKey).FirstOrDefault();

            if (feed == null)
                return new HttpNotFoundResult();

            // if ?download=true included, then download after being generated

            if (download.HasValue && download == true)
            {
                feed.Generate();
                var feedFormat = feed.DefaultFileFormat;
                // switch to the right extension
                var filepath = Path.ChangeExtension(feed.FeedFilePath, feedFormat.ToString());

                return new FilePathResult(filepath, feedFormat.Description())
                {
                    FileDownloadName = Path.GetFileName(filepath)
                };
            }

            // otherwise, enqueue for processing only

            MvcApplication.Current.FeedManager.EnqueueFeedForGeneration(feed);
            new WebsiteApplicationLifetimeEvent("Begin to generate single product feed for " + store.FriendlyName + ": " + feed.Key.ToString(), this, WebsiteEventCode.Notification).Raise();

            ViewBag.StoreName = store.FriendlyName;
            ViewBag.Feed = feed.Key;

            return View();

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Gen4.Util.Misc;
using System.Diagnostics;

namespace Website
{
    [ProductFeed(ProductFeedKeys.PriceGrabber)]
    public class PriceGrabberProductFeed : ProductFeed, IProductFeed
    {

        public PriceGrabberProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.PriceGrabber, storeProductFeed)
        {
        }

        #region Properties

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during gernaration operation, else null.
        /// </remarks>
        public Dictionary<string, PriceGrabberFeedProduct> Products { get; private set; }

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as PriceGrabberFeedProduct;

            if (!Products.TryAdd(product.Retsku, product))
                throw new Exception(string.Format("Duplicate PriceGrabber feed product ID {0} for {1}.", product.Retsku, Store.StoreKey));
        }

        protected override void PopulateProducts()
        {
            StoreProductFeed.PopulateProducts(this);
        }



        /// <summary>
        /// Helper to check to see if all the image URLs are unique.
        /// </summary>
        /// <returns></returns>
        protected int CountDuplicateImageLinks()
        {
            int countDuplicates = 0;
            var dic = new Dictionary<string, PriceGrabberFeedProduct>();

            foreach (var product in Products.Values)
            {
                var key = product.PrimaryImageURL.ToLower();
                if (!dic.TryAdd(key, product))
                {
                    var existingProduct = dic[key];
                    Debug.WriteLine(string.Format("Duplicate image link for SKU {0} and {1}: {2}", product.Retsku, existingProduct.Retsku, product.PrimaryImageURL));
                    countDuplicates++;
                }
            }

            Debug.WriteLine("Duplicate image links: {0:N0}", countDuplicates);
            return countDuplicates;
        }



        /// <summary>
        /// Helper to check to see if all the image URLs are unique.
        /// </summary>
        /// <returns></returns>
        protected int CountDuplicateTitles()
        {
            int countDuplicates = 0;
            var dic = new Dictionary<string, PriceGrabberFeedProduct>();

            foreach (var product in Products.Values)
            {
                var key = product.ProductTitle.ToLower();
                if (!dic.TryAdd(key, product))
                {
                    var existingProduct = dic[key];
                    Debug.WriteLine(string.Format("Duplicate image title for {0} and {1}: {2}", product.Retsku, existingProduct.Retsku, product.ProductTitle));
                    countDuplicates++;
                }
            }

            Debug.WriteLine("Duplicate titles: {0:N0}", countDuplicates);
            return countDuplicates;
        }


        /// <summary>
        /// Hook for begin generation.
        /// </summary>
        protected override void BeginGeneration()
        {
            Products = new Dictionary<string, PriceGrabberFeedProduct>();
        }


        /// <summary>
        /// Hook for end generation. Allow chance to clean up.
        /// </summary>
        protected override void EndGeneration()
        {
            Products = null;
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
            WriteFieldNamesHeader<PriceGrabberFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord(file, product);
        }


    }
}
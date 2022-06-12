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
    [ProductFeed(ProductFeedKeys.Amazon)]
    public class AmazonProductFeed : ProductFeed, IProductFeed
    {

        public AmazonProductFeed(IStoreProductFeed storeProductFeed) : base(ProductFeedKeys.Amazon, storeProductFeed)
        {
        }

        #region Properties

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during gernaration operation, else null.
        /// </remarks>
        public Dictionary<string, AmazonFeedProduct> Products { get; private set; }

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as AmazonFeedProduct;
            if (!Products.TryAdd(product.ID, product))
                throw new Exception(string.Format("Duplicate Amazon feed product ID {0} for {1}.", product.ID, Store.StoreKey));
        }

        protected override void PopulateProducts()
        {
            StoreProductFeed.PopulateProducts(this);
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
            WriteFieldNamesHeader<AmazonFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord(file, product);
        }

        /// <summary>
        /// Helper to check to see if all the image URLs are unique.
        /// </summary>
        /// <returns></returns>
        protected int CountDuplicateImageLinks()
        {
            int countDuplicates = 0;
            var dic = new Dictionary<string, AmazonFeedProduct>();

            foreach (var product in Products.Values)
            {
                var key = product.ImageLink.ToLower();
                if (!dic.TryAdd(key, product))
                {
                    var existingProduct = dic[key];
                    Debug.WriteLine(string.Format("Duplicate image link for SKU {0} and {1}: {2}", product.ID, existingProduct.ID, product.ImageLink));
                    countDuplicates++;
                }
            }

            Debug.WriteLine("Duplicate image links: {0:N0}", countDuplicates);
            return countDuplicates;
        }


        /// <summary>
        /// Hook for begin generation.
        /// </summary>
        protected override void BeginGeneration()
        {
            Products = new Dictionary<string, AmazonFeedProduct>();
        }


        /// <summary>
        /// Hook for end generation. Allow chance to clean up.
        /// </summary>
        protected override void EndGeneration()
        {
            Products = null;
        }


    }
}
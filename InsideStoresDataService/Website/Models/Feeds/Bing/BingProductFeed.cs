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
    [ProductFeed(ProductFeedKeys.Bing)]
    public class BingProductFeed : ProductFeed, IProductFeed
    {
        public BingProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.Bing, storeProductFeed)
        {
        }

        #region Properties

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during gernaration operation, else null.
        /// </remarks>
        public Dictionary<string, BingFeedProduct> Products { get; private set; }

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as BingFeedProduct;

            if (!Products.TryAdd(product.MerchantProductID, product))
                throw new Exception(string.Format("Duplicate Bing feed product ID {0} for {1}.", product.MerchantProductID, Store.StoreKey));
        }

        protected override void PopulateProducts()
        {
            StoreProductFeed.PopulateProducts(this);
            // bing does not like duplicate titles, so do a little recrafting magic
            RemoveDuplicateTitles();
        }

        /// <summary>
        /// Helper to check to see if all the image URLs are unique.
        /// </summary>
        /// <returns></returns>
        protected int CountDuplicateImageLinks()
        {
            int countDuplicates = 0;
            var dic = new Dictionary<string, BingFeedProduct>();

            foreach (var product in Products.Values)
            {
                var key = product.ImageUrl.ToLower();
                if (!dic.TryAdd(key, product))
                {
                    var existingProduct = dic[key];
                    Debug.WriteLine(string.Format("Duplicate image link for SKU {0} and {1}: {2}", product.MerchantProductID, existingProduct.MerchantProductID, product.ImageUrl));
                    countDuplicates++;
                }
            }

            Debug.WriteLine("Duplicate image links: {0:N0}", countDuplicates);
            return countDuplicates;
        }


        protected virtual void RemoveDuplicateTitles()
        {
            try
            {
                var dic = new Dictionary<string, BingFeedProduct>();
                var listMPID = new List<string>();

                foreach (var product in Products.Values)
                {
                    var key = product.Title.ToLower();
                    if (!dic.TryAdd(key, product))
                    {
                        var existingProduct = dic[key];
                        listMPID.Add(existingProduct.MerchantProductID);
                        listMPID.Add(product.MerchantProductID);
                    }
                }

                foreach (var mpid in listMPID.Distinct())
                {
                    BingFeedProduct product;

                    if (Products.TryGetValue(mpid, out product))
                    {
                        var newTitle = string.Format("{0} {1}", product.Title, product.MPN);
                        product.Title = newTitle;
                    }
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());                
            }
        }



        /// <summary>
        /// Helper to check to see if all the image URLs are unique.
        /// </summary>
        /// <returns></returns>
        protected int CountDuplicateTitles()
        {
            int countDuplicates = 0;
            var dic = new Dictionary<string, BingFeedProduct>();

            foreach (var product in Products.Values)
            {
                var key = product.Title.ToLower();
                if (!dic.TryAdd(key, product))
                {
                    var existingProduct = dic[key];
                    Debug.WriteLine(string.Format("Duplicate image title for {0} and {1}: {2}", product.MerchantProductID, existingProduct.MerchantProductID, product.Title));
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
            Products = new Dictionary<string, BingFeedProduct>();
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

            WriteFieldNamesHeader<BingFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord(file, product);
        }

    }
}
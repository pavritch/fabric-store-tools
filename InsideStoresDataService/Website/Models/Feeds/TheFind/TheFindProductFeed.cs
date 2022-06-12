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
    [ProductFeed(ProductFeedKeys.TheFind)]
    public class TheFindProductFeed : ProductFeed, IProductFeed
    {

        public TheFindProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.TheFind, storeProductFeed)
        {
        }

        #region Properties

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during gernaration operation, else null.
        /// </remarks>
        public Dictionary<string, TheFindFeedProduct> Products { get; private set; }

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as TheFindFeedProduct;

            if (!Products.TryAdd(product.UniqueID, product))
                throw new Exception(string.Format("Duplicate TheFind feed product ID {0} for {1}.", product.UniqueID, Store.StoreKey));
        }

        protected override void PopulateProducts()
        {
            StoreProductFeed.PopulateProducts(this);
            // TheFind does not like duplicate titles, so do a little recrafting magic
            RemoveDuplicateTitles();
        }

        protected void RemoveDuplicateTitles()
        {
            try
            {
                var dic = new Dictionary<string, TheFindFeedProduct>();
                var listMPID = new List<string>();

                foreach (var product in Products.Values)
                {
                    var key = product.Title.ToLower();
                    if (!dic.TryAdd(key, product))
                    {
                        var existingProduct = dic[key];
                        listMPID.Add(existingProduct.UniqueID);
                        listMPID.Add(product.UniqueID);
                    }
                }

                foreach (var mpid in listMPID.Distinct())
                {
                    TheFindFeedProduct product;

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
        /// Hook for begin generation.
        /// </summary>
        protected override void BeginGeneration()
        {
            Products = new Dictionary<string, TheFindFeedProduct>();
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
            WriteFieldNamesHeader<TheFindFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord<TheFindFeedProduct>(file, product);
        }

    }
}
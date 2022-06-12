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

    // 4/23/2018 Peter
    // Needed to add support for Google feed related to Shopify Canada website, so hacked a little
    // to be as non-invasive as possible by creating a layer over existing logic.

    public interface IGoogleFeedProduct
    {
        string ID { get; }
        string ImageLink { get; }
    }

    [ProductFeed(ProductFeedKeys.GoogleCanada)]
    public class GoogleCanadaProductFeed : GoogleProductFeed<GoogleCanadaFeedProduct>
    {
        public GoogleCanadaProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.GoogleCanada, storeProductFeed)
        {
        }

    }

    [ProductFeed(ProductFeedKeys.Google)]
    public class GoogleUnitedStatesProductFeed : GoogleProductFeed<GoogleUnitedStatesFeedProduct>
    {
        public GoogleUnitedStatesProductFeed(IStoreProductFeed storeProductFeed)
            : base(ProductFeedKeys.Google, storeProductFeed)
        {
        }
    }

    public class GoogleProductFeed<TFeedProduct> : ProductFeed, IProductFeed where TFeedProduct : FeedProduct, IGoogleFeedProduct 
    {

        public GoogleProductFeed(ProductFeedKeys key, IStoreProductFeed storeProductFeed)
            : base(key, storeProductFeed)
        {
        }

        #region Properties

        /// <summary>
        /// Keeps running list of products being added to feed.
        /// </summary>
        /// <remarks>
        /// Only valid during generation operation, else null.
        /// </remarks>
        public Dictionary<string, TFeedProduct> Products { get; private set; }

        #endregion

        public void AddProduct(FeedProduct feedProduct)
        {
            var product = feedProduct as TFeedProduct;
            string id = (product as IGoogleFeedProduct).ID;
            if (!Products.TryAdd(id, product))
                throw new Exception(string.Format("Duplicate Google feed product ID {0} for {1}.", product.ID, Store.StoreKey));
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
            var dic = new Dictionary<string, TFeedProduct>();

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
            Products = new Dictionary<string, TFeedProduct>();
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
            WriteFieldNamesHeader<GoogleFeedProduct>(file);

            foreach (var product in Products.Values)
                WriteProductRecord(file, product);
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
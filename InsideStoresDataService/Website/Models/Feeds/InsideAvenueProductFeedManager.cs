using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;

namespace Website
{
    public class InsideAvenueProductFeedManager : ProductFeedManager, IProductFeedManager, IStoreProductFeed
    {
        public InsideAvenueProductFeedManager(IWebStore store) : base(store)
        {
            AddFeed(new GoogleUnitedStatesProductFeed(this));
            AddFeed(new GoogleCanadaProductFeed(this));
            AddFeed(new ShopifyProductFeed(this));
        }

        private FeedProduct CreateFeedProduct(ProductFeedKeys feedKey, InsideAvenueFeedProduct storeFeedProduct)
        {
            FeedProduct feedProduct;

            switch (feedKey)
            {
                // the google feed is the only one specifically implemented.

                case ProductFeedKeys.Google:
                    feedProduct = new InsideAvenueGoogleUnitedStatesFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.GoogleCanada:
                    feedProduct = new InsideAvenueGoogleCanadaFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopify:
                    feedProduct = new InsideAvenueShopifyFeedProduct(storeFeedProduct);
                    break;

                default:
                    throw new Exception("Unknown feed key.");
            }

            return feedProduct;

        }


        /// <summary>
        /// Iterate through products to facilitate feed population - common code.
        /// </summary>
        /// <remarks>
        /// Called by each kind of fabric feed. Callers must use the callback to perform the per-item population.
        /// </remarks>
        /// <param name="ProductFeedFilePath"></param>
        /// <param name="callback"></param>
        public void PopulateProducts(IProductFeed productFeed)
        {
            // create a new FeedProduct for each product

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
#if true
                // there are 2,300 duplicates in this table - even though should not be, so need to 
                // be careful about how this collection is generated

                var pManufactures = dc.ProductManufacturers.Where(e => dc.Products.Where(f => f.ShowBuyButton == 1 && f.Published == 1 && f.Deleted == 0).Select(f => e.ProductID).Contains(e.ProductID)).ToList();
                var productManufactures = new Dictionary<int, int>();
                foreach (var pm in pManufactures)
                {
                    if (productManufactures.ContainsKey(pm.ProductID))
                    {
                        //Debug.WriteLine(string.Format("Duplicate ProductManufacturer ProductID: {0}  {1}", pm.ProductID, pm.ManufacturerID));
                        continue;
                    }

                    productManufactures.Add(pm.ProductID, pm.ManufacturerID);
                }
#else
                var productManufactures = dc.ProductManufacturers.ToDictionary(k => k.ProductID, v => v.ManufacturerID);
#endif

                var storeManufacturers = dc.Manufacturers.ToDictionary(k => k.ManufacturerID, v => v);
                var storeCategories = dc.Categories.ToDictionary(k => k.CategoryID, v => v);

                var allProductIDs = dc.Products.Where(e => e.ShowBuyButton == 1 && e.Published == 1 && e.Deleted == 0).Select(e => e.ProductID).ToList();
                int skipCount = 0;

                const int takeCount = 200;

                while (true)
                {
                    var productSet = allProductIDs.Skip(skipCount).Take(takeCount).ToList();

                    if (productSet.Count == 0)
                        break;

                    var products = dc.Products.Where(e => productSet.Contains(e.ProductID)).ToDictionary(k => k.ProductID, v => v);
                    var productVariants = dc.ProductVariants.Where(e => productSet.Contains(e.ProductID) && e.IsDefault == 1).ToDictionary(k => k.ProductID, v => v);

                    var productCategories = dc.ProductCategories.Where(e => productSet.Contains(e.ProductID)).Select(e => new { e.ProductID, e.CategoryID }).ToList();

                    foreach (var productID in productSet)
                    {
                        Product p = null;
                        ProductVariant pv = null;
                        int manufacturerID = 0;
                        Manufacturer manufacturer = null;

                        try
                        {
                            if (products.TryGetValue(productID, out p) && productVariants.TryGetValue(productID, out pv)
                                && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                            {
                                // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                // derrive the collection of categories to which this product belongs
                                var catIDs = productCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                var storeFeedProduct = new InsideAvenueFeedProduct(Store, p, pv, manufacturer, categories, dc);

                                var feedProduct = CreateFeedProduct(productFeed.Key, storeFeedProduct);

                                if (feedProduct.IsValid)
                                    productFeed.AddProduct(feedProduct);

                            }
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.ToString());
                            var ev2 = new WebsiteRequestErrorEvent(string.Format("Exception generating product feed: {0}-{1} for ProductID {2}.", Store.StoreKey, productFeed.Key, p.ProductID), this, WebsiteEventCode.UnhandledException, Ex);
                            ev2.Raise();
                        }
                    }

                    // if found less than take, then end of line

                    if (productSet.Count < takeCount)
                        break;

                    // loop for next batch

                    skipCount += takeCount;

#if false
                    Debug.WriteLine("**** Partial Database ***********");
                    break;
#endif
                }

            }
        }

    }
}
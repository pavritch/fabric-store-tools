using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;

namespace Website
{
    public class InsideRugsProductFeedManager : ProductFeedManager, IProductFeedManager, IStoreProductFeed
    {
        public InsideRugsProductFeedManager(IWebStore store)
            : base(store)
        {
            // nothing ever implemented for real, these are copy and paste placeholders.

            //AddFeed(new AmazonProductFeed(this));
            //AddFeed(new GoogleProductFeed(this));
            //AddFeed(new BingProductFeed(this));
            //AddFeed(new TheFindProductFeed(this));

            //AddFeed(new NextagProductFeed(this));
            //AddFeed(new ShopzillaProductFeed(this));
            //AddFeed(new ProntoProductFeed(this));
            //AddFeed(new PriceGrabberProductFeed(this));
            //AddFeed(new ShoppingProductFeed(this));
        }


        private FeedProduct CreateFeedProduct(ProductFeedKeys feedKey, InsideRugsFeedProduct storeFeedProduct)
        {
            FeedProduct feedProduct;

            switch (feedKey)
            {
                case ProductFeedKeys.Amazon:
                    feedProduct = new InsideRugsAmazonFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Bing:
                    feedProduct = new InsideRugsBingFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Google:
                    feedProduct = new InsideRugsGoogleUnitedStatesFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.GoogleCanada:
                    feedProduct = new InsideRugsGoogleCanadaFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.TheFind:
                    feedProduct = new InsideRugsTheFindFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Nextag:
                    feedProduct = new InsideRugsNextagFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.PriceGrabber:
                    feedProduct = new InsideRugsPriceGrabberFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Pronto:
                    feedProduct = new InsideRugsProntoFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopping:
                    feedProduct = new InsideRugsShoppingFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopzilla:
                    feedProduct = new InsideRugsShopzillaFeedProduct(storeFeedProduct);
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

                    // dictionary using ProductID as key, with value being a collection of variants for that product
                    var productVariants = (from pv in dc.ProductVariants
                                           where productSet.Contains(pv.ProductID) && pv.Published==1 && pv.Deleted==0
                                           group pv by pv.ProductID into collectionByProductID
                                           select new { ProductGrouping = collectionByProductID }).ToDictionary(k => k.ProductGrouping.Key, v => v.ProductGrouping.ToList());


                    var productCategories = dc.ProductCategories.Where(e => productSet.Contains(e.ProductID)).Select(e => new { e.ProductID, e.CategoryID }).ToList();

                    foreach (var productID in productSet)
                    {
                        Product p = null;
                        List<ProductVariant> pvCollection = null;
                        int manufacturerID = 0;
                        Manufacturer manufacturer = null;

                        try
                        {
                            if (products.TryGetValue(productID, out p) && productVariants.TryGetValue(productID, out pvCollection) 
                                && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                            {
                                // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                var group = p.ProductGroup.ToProductGroup();
                                if (group.HasValue && !Store.SupportedProductGroups.Contains(group.Value))
                                    continue;

                                // derrive the collection of categories to which this product belongs
                                var catIDs = productCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                foreach (var pv in pvCollection)
                                {
                                    try
                                    {
                                        var storeFeedProduct = new InsideRugsFeedProduct(Store, p, pv, manufacturer, categories, dc);
                                        var feedProduct = CreateFeedProduct(productFeed.Key, storeFeedProduct);

                                        if (feedProduct.IsValid)
                                            productFeed.AddProduct(feedProduct);
                                    }
                                    catch (Exception Ex)
                                    {
                                        Debug.WriteLine(Ex.ToString());
                                        var ev2 = new WebsiteRequestErrorEvent(string.Format("Exception generating product feed: {0}-{1} for VariantID {2}.", Store.StoreKey, productFeed.Key, pv.VariantID), this, WebsiteEventCode.UnhandledException, Ex);
                                        ev2.Raise();
                                    }
                                }
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
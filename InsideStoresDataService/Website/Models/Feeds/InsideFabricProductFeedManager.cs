using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;

namespace Website
{
    public class InsideFabricProductFeedManager : ProductFeedManager, IProductFeedManager, IStoreProductFeed
    {
        public InsideFabricProductFeedManager(IWebStore store) : base(store)
        {
            AddFeed(new AmazonProductFeed(this));
            AddFeed(new GoogleUnitedStatesProductFeed(this));
            AddFeed(new GoogleCanadaProductFeed(this));
            AddFeed(new BingProductFeed(this));
            AddFeed(new ShopifyProductFeed(this));
            AddFeed(new DibsProductFeed(this));

            // worked when created - but we're not supporting these companies and
            // don't want to burn up processing time for feeds

            //AddFeed(new TheFindProductFeed(this));
            //AddFeed(new NextagProductFeed(this));
            //AddFeed(new ShopzillaProductFeed(this));
            //AddFeed(new ProntoProductFeed(this));
            //AddFeed(new PriceGrabberProductFeed(this));
            //AddFeed(new ShoppingProductFeed(this));
        }

        private FeedProduct CreateFeedProduct(ProductFeedKeys feedKey, InsideFabricFeedProduct storeFeedProduct)
        {
            FeedProduct feedProduct;

            switch (feedKey)
            {
                case ProductFeedKeys.Amazon:
                    feedProduct = new InsideFabricAmazonFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Bing:
                    feedProduct = new InsideFabricBingFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Google: 
                    // united states / default
                    feedProduct = new InsideFabricGoogleUnitedStatesFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.GoogleCanada:
                    feedProduct = new InsideFabricGoogleCanadaFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopify:
                    feedProduct = new InsideFabricShopifyFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.FirstDibs:
                    feedProduct = new InsideFabricDibsFeedProduct(storeFeedProduct);
                    break;


                // all below here are inactive

                case ProductFeedKeys.Nextag:
                    feedProduct = new InsideFabricNextagFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.PriceGrabber:
                    feedProduct = new InsideFabricPriceGrabberFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Pronto:
                    feedProduct = new InsideFabricProntoFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopping:
                    feedProduct = new InsideFabricShoppingFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.Shopzilla:
                    feedProduct = new InsideFabricShopzillaFeedProduct(storeFeedProduct);
                    break;

                case ProductFeedKeys.TheFind:
                    feedProduct = new InsideFabricTheFindFeedProduct(storeFeedProduct);
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

                // notice that Trim is excluded (unless shopify), because was going over the 150K product limit of google feeds
                // wallcovering excluded too since does not apply to fabric feed

                List<int> allProductIDs = null;
                
                if (productFeed.Key == ProductFeedKeys.Shopify)
                    // shopify includes all fabric product groups, no wallpaper
                    allProductIDs = dc.Products.Where(e => e.ShowBuyButton == 1 && e.Published == 1 && e.Deleted == 0 && e.ProductGroup != "Wallcovering").Select(e => e.ProductID).ToList();
                else
                    // other feeds remove trim since have too many products for google to ingest
                    allProductIDs = dc.Products.Where(e => e.ShowBuyButton == 1 && e.Published == 1 && e.Deleted == 0 && e.ProductGroup != "Trim" && e.ProductGroup != "Wallcovering").Select(e => e.ProductID).ToList();

                int skipCount = 0;

                const int takeCount = 200;

                while (true)
                {
                    // mostly for debug to cap the quantity of records generated
                    if (productFeed.IsMaxedOut)
                        break;

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
                                // include only those that are in stock, unless shopify
                                if (pv.Inventory == 0 && productFeed.Key != ProductFeedKeys.Shopify)
                                    continue;

                                var group = p.ProductGroup.ToProductGroup();
                                if (group.HasValue && !Store.SupportedProductGroups.Contains(group.Value))
                                    continue;

                                // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                // derrive the collection of categories to which this product belongs
                                var catIDs = productCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                var storeFeedProduct = new InsideFabricFeedProduct(Store, p, pv, manufacturer, categories, dc);

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
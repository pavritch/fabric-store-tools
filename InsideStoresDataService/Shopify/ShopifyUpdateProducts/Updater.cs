using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifyCommon;
using System.IO;
using ShopifySharp;
using System.Diagnostics;

namespace ShopifyUpdateProducts
{
    public class Updater
    {
        private App app;
        private HashSet<long> updatedProducts = new HashSet<long>();

        public Updater(App app)
        {
            this.app = app;
        }

        public void Run()
        {



            var startTime = DateTime.Now;

            // if has a list of completed productID, then in the middle of something, pick up
            // where left off

            if (!File.Exists(UpdatedProductsFilepath))
            {
                // clean start
                // make a copy of the last-known state of live shopify data
                Console.WriteLine("Saving product snapshot of live Shopify data.");
                app.ProductManager.SaveSnapshot();

                // update the status for every known product

                Console.WriteLine("Analyzing current state of all known products.");

                foreach(var product in app.ProductManager.Products)
                {
                    // once deleted, always deleted,  don't let re-live product confuse things
                    if (product.Status == ProductStatus.Deleted)
                        continue;

                    // will be set deleted if showbuy=0, deleted=1 or published=0
                    product.Status = app.StockTrackers[product.Store].GetStatus(product.ProductID);

#if false
                    if (product.ShopifyHandle.StartsWith("if-") && product.ShopifyHandle.Contains("by-j-ennis"))
                        product.Status = ProductStatus.Deleted;
#endif

                    // this is a permanent condition
                    // this is a permanent condition
                    // 6/20/2019
                    //Not sure if this is easy/hard/fast to do, but can you PURGE all IA stuff from the US Shopify store?
                    //Ie, I only want fabric/wallpaper/trims on there.  Just IF & IW vendors only.
                    if (!app.IsCanada && product.ShopifyHandle.StartsWith("ia-"))
                    {
                            product.Status = ProductStatus.Deleted;
                    }


                    if (app.IsCanada && (product.ShopifyHandle.StartsWith("if-") || product.ShopifyHandle.StartsWith("iw-")))
                    {
                        // no PJ or FS in Canada for IF/IW

                        if (product.ShopifyHandle.Contains("by-phillip-jeffries"))
                            product.Status = ProductStatus.Deleted;

                        if (product.ShopifyHandle.Contains("by-schumacher"))
                            product.Status = ProductStatus.Deleted;

                    }


                    var price = app.StockTrackers[product.Store].GetPrice(product.ProductID);

                    if (app.IsCanada && price.HasValue)
                    {
                        // adjust prices for Canada
                        price = Math.Round(price.Value * app.MarkupCanada, 2);
                    }

                    if (product.Status != ProductStatus.Deleted && price.HasValue)
                        product.Price = price.Value;
                }

                // persist
                Console.WriteLine("Saving updated product file.");
                app.ProductManager.Save();

                // the snapshot and the main json files have the same records, although
                // the main file has just-now updated status and price.
            }
            else
            {
                InitUpdatedProducts();
                Console.WriteLine("Continuation of previous update operation.");
            }

            Console.WriteLine(string.Format("Begin updating {0:N0} Shopify products.", app.ProductManager.Products.Count()));

            Console.WriteLine(string.Format("Skipping {0:N0} previously updated products.", updatedProducts.Count()));

            var snapshotProducts = app.ProductManager.SnapshotProducts.ToDictionary(k => k.ShopifyProductId, v => v);

            var serviceProduct = new ShopifyProductService(app.ShopifyStoreUrl, app.ShopifyAppPassword);
            var serviceProductVariant = new UpdateProductVariantService(app.ShopifyStoreUrl, app.ShopifyAppPassword);

            // for any products whereby the current record is different from the snapshot, update shopify

            int countModified = 0;
            foreach (var product in app.ProductManager.Products)
            {
                try
                {
                    bool isModified = false;

                    // don't update products already pushed to shopify
                    if (updatedProducts.Contains(product.ShopifyProductId))
                        continue;

                    // do not attempt to process any product which is does not appear in both collections

                    ProductInformation snapshotProduct;
                    if (!snapshotProducts.TryGetValue(product.ShopifyProductId, out snapshotProduct))
                        continue;

                    
                    if (product.Status != snapshotProduct.Status || product.Price != snapshotProduct.Price)
                    {
                        // push to Shopify required because either status or price changed

                        // products = new desired state
                        // snapshot = what's on shopify for any incomplete products

                        if (product.Status == ProductStatus.Deleted && snapshotProduct.Status != ProductStatus.Deleted)
                        {
                            Console.WriteLine(string.Format("Delete: {0}", product.ShopifyHandle));
                            app.ApiSentry.WaitMyTurn().Wait();
                            // 9/27/2017 made change to delete unpublished
                            //serviceProduct.UnpublishAsync(product.ShopifyProductId).Wait();
                            serviceProduct.DeleteAsync(product.ShopifyProductId).Wait();
                            isModified = true;
                        }
                        // 9/27/2017 
                        // since now delete unpublished, no changing back
                        //else if (product.Status != ProductStatus.Discontinued && snapshotProduct.Status == ProductStatus.Discontinued)
                        //{
                        //    Console.WriteLine(string.Format("Mark Published: {0}", product.ShopifyHandle));
                        //    app.ApiSentry.WaitMyTurn().Wait();
                        //    serviceProduct.PublishAsync(product.ShopifyProductId).Wait();
                        //    isModified = true;
                        //}

                        // only update price/stock if not ever known to be deleted - once deleted, the record is locked.
                        // this prevents issues when a product becomes non-discontinued.
                        if (product.Status != ProductStatus.Deleted && snapshotProduct.Status != ProductStatus.Deleted && (product.Status != snapshotProduct.Status || product.Price != snapshotProduct.Price))
                        {
                            var variant = new ShopifySmallProductVariant()
                            {
                                Id = product.ShopifyVariantId,
                                InventoryQuantity = product.Status == ProductStatus.InStock ? 999999 : 0,
                                Price = product.Price
                            };

                            Console.WriteLine(string.Format("Update Price and Stock: {0}", product.ShopifyHandle));
                            app.ApiSentry.WaitMyTurn().Wait();
                            serviceProductVariant.UpdateInventoryAndPriceAsync(variant).Wait();
                            isModified = true;
                        }
                    }
                    if (isModified)
                        countModified++;

                    AddUpdatedProduct(product.ShopifyProductId);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine("Exception: " + Ex.Message);
                    System.Threading.Thread.Sleep(1000 * 15);
                }
            }

            Console.WriteLine("Finished updating Shopify products.");
            Console.WriteLine(string.Format("Pushed changes for {0:N0} Shopify products.", countModified));

            var endTime = DateTime.Now;
            Console.WriteLine(string.Format("Time to update Shopify: {0}", endTime - startTime));

            DeleteUpdatedProducts();

            // the snapshot file is now obsolete since we completed the entire push of changes
            app.ProductManager.DeleteSnapshotProductFile();
        }

        private string UpdatedProductsFilepath
        {
            get
            {
                var filePath = Path.GetDirectoryName(app.ProductDataFilepath);
                var filename = string.Format("UpdatedProducts{0}.txt", app.IsCanada ? "-CA" : "");
                return Path.Combine(filePath, filename);
            }
        }

        private void InitUpdatedProducts()
        {
            if (!File.Exists(UpdatedProductsFilepath))
                return;

            foreach (var line in File.ReadLines(UpdatedProductsFilepath))
            {
                long shopifyProductID;
                if (long.TryParse(line, out shopifyProductID))
                    updatedProducts.Add(shopifyProductID);

            }
        }

        private void DeleteUpdatedProducts()
        {
            File.Delete(UpdatedProductsFilepath);
        }

        private void AddUpdatedProduct(long shopifyProductID)
        {
            File.AppendAllText(UpdatedProductsFilepath, string.Format("{0}\n", shopifyProductID), Encoding.UTF8);
        }
    }
}

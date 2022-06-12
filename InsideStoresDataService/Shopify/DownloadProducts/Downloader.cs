using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ShopifyCommon;
using ShopifySharp;

namespace ShopifyDeleteProducts
{
    public class Downloader
    {
        private App app;

        public Downloader(App app)
        {
            this.app = app;
        }

        /// <summary>
        /// Capture an explicit regex pattern.
        /// </summary>
        /// <remarks>
        /// You must include a single explicit capture within the pattern.
        /// </remarks>
        /// <param name="?"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static string CaptureWithinMatchedPattern(string input, string pattern)
        {
            // Example:
            // input: ../../product_images/thumbnails/resize.php?2151055.jpg&amp;size=126
            // Pattern : @"resize.php\?(?<capture>(\d{1,7})).jpg"
            if (input == null) return null;

            string capturedText = null;

            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            var matches = Regex.Matches(input, pattern, options);

            if (matches.Count == 0 || matches[0].Groups.Count < 2)
                return null;

            capturedText = matches[0].Groups[1].ToString();

            return capturedText;
        }


        public void Run()
        {
            const string REQUIRED_FIELDS = "id,handle,published_at,variants";
            long sinceID = 0L;
            var startTime = DateTime.Now;

            app.ProductManager.DeleteProductFile();
            var serviceProduct = new ShopifyProductService(app.ShopifyStoreUrl, app.ShopifyAppPassword);
            //var serviceProductVariant = new ShopifyProductVariantService(app.ShopifyStoreUrl, app.ShopifyAppPassword);

            var countAllProducts = serviceProduct.CountAsync().Result;
            var countRemainingProducts = countAllProducts;

            Console.WriteLine("Begin downloading {0:N0} products.", countAllProducts);

            while (true)
            {
                try
                {

                    app.ApiSentry.WaitMyTurn().Wait();

                    var task = serviceProduct.ListAsync(new ShopifySharp.Filters.ShopifyProductFilter() { Limit = app.ShopifyApiProductBatchSize, Fields = REQUIRED_FIELDS, SinceId = sinceID });

                    var products = task.Result.ToList();

                    if (products.Count == 0)
                    {
                        Console.WriteLine("Products remaining: {0:N0}", 0);
                        break;
                    }


                    foreach (var product in products)
                    {
                        try
                        {

                            var productInfo = new ProductInformation();

                            productInfo.ShopifyProductId = product.Id.Value;
                            productInfo.ShopifyHandle = product.Handle;
                            productInfo.Status = ProductStatus.InStock;

                            var storeCode = product.Handle.Substring(0, 2).ToUpper();
                            switch (storeCode)
                            {
                                case "IF":
                                    productInfo.Store = StoreKey.InsideFabric;
                                    break;

                                case "IA":
                                    productInfo.Store = StoreKey.InsideAvenue;
                                    break;

                                case "IW":
                                    productInfo.Store = StoreKey.InsideWallpaper;
                                    break;

                                default:
                                    throw new Exception("Unknown store code in handle.");
                            }

                            var sProductID = CaptureWithinMatchedPattern(product.Handle, @"^\w\w-(?<capture>(\d+))-");
                            var productID = int.Parse(sProductID);
                            productInfo.ProductID = productID;

#if true
                            // note that when downloading, it is not possible to set status=deleted since that 
                            // would not make sense

                            if (product.Variants.Count() > 0)
                            {
                                var variant = product.Variants.First();
                                productInfo.ShopifyVariantId = variant.Id.Value;
                                productInfo.Price = variant.Price;
                                if (product.PublishedAt == null)
                                {
                                    productInfo.Status = ProductStatus.Unpublished;
                                }
                                else
                                {
                                    productInfo.Status = variant.InventoryQuantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
                                }

                                app.ProductManager.AddProduct(productInfo);
                            }

#else
                    app.ApiSentry.WaitMyTurn().Wait();
                    var variants = serviceProductVariant.ListAsync(productInfo.ShopifyProductId, new ShopifySharp.Filters.ShopifyListFilter() { Fields = "id,inventory_quantity" }).Result;
                    if (variants.Count() > 0)
                    {
                        var variant = variants.First();
                        productInfo.ShopifyVariantId = variant.Id.Value;
                        productInfo.Price = variant.Price;
                        
                        if (product.PublishedAt == null)
                        {
                            productInfo.Status = ProductStatus.Discontinued;
                        }
                        else
                        {
                            productInfo.Status = variant.InventoryQuantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
                        }
                        
                        app.ProductManager.AddProduct(productInfo);
                    }
#endif

                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine("Exception:" + Ex + "\n\n");
                        }


                    }

                    sinceID = products.Last().Id.GetValueOrDefault();

                    if (products.Count < app.ShopifyApiProductBatchSize)
                    {
                        Console.WriteLine("Products remaining: {0:N0}", 0);
                        break;
                    }

                    countRemainingProducts -= products.Count();
                    Console.WriteLine("Products remaining: {0:N0}", countRemainingProducts);

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Exception:" + Ex + "\n\n");
                }




            }


            var endTime = DateTime.Now;

            app.ProductManager.Save();

            Console.WriteLine("Duration for downloading products: {0}", endTime - startTime);

        }
    }
}

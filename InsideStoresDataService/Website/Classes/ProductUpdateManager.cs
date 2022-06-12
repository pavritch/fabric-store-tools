using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.IO;
using Website.Entities;
using System.Diagnostics;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Website
{

    #region UpdateManagerNuget

    public class UpdateManagerNuget
    {
        public class ProductCategory
        {
            public int ProductID { get; set; }
            public int CategoryID { get; set; }

            public ProductCategory(int pID, int cID)
            {
                ProductID = pID;
                CategoryID = cID;
            }
        }

        public int ID { get; set; }

        public List<int> ProductSet { get; private set; }
        public Dictionary<int, Product> Products { get; private set; }
        public Dictionary<int, List<ProductVariant>> ProductVariants { get; private set; }
        public List<ProductCategory> ProductCategories { get; private set; }
        public bool IsLast { get; set; }

        public UpdateManagerNuget(int ID, string connectionString, List<int> productSet)
        {
            this.ID = ID;
            IsLast = false;
            ProductSet = productSet;

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                if (ProductSet.Count == 0)
                    return;

                Products = dc.Products.Where(e => ProductSet.Contains(e.ProductID)).ToDictionary(k => k.ProductID, v => v);

                ProductVariants = dc.ProductVariants.Where(e => ProductSet.Contains(e.ProductID)).GroupBy(e => e.ProductID).ToDictionary(k => k.Key, v => v.OrderByDescending(e => e.IsDefault).OrderBy(e => e.Price).ToList());

                ProductCategories = dc.ProductCategories.Where(e => ProductSet.Contains(e.ProductID)).Select(e => new UpdateManagerNuget.ProductCategory(e.ProductID, e.CategoryID)).ToList();
            }
        }
    } 
    #endregion

    public class ProductUpdateManager<TProduct> where TProduct : class, IUpdatableProduct, new()
    {

        #region ProcessProduct

        /// <summary>
        /// Perform a complex procedure on specific product.
        /// </summary>
        /// <remarks>
        /// Rich information and associated SQL entities are gathered up into a very smart product class which
        /// is then passed to the callback method for specific processing.
        /// </remarks>
        /// <param name="ProductID"></param>
        /// <param name="callback"></param>
        public static void ProcessProduct(IWebStore Store, int ProductID, Action<TProduct> callback, AspStoreDataContext dataContext = null) 
        {
            AspStoreDataContext dc = null;
            bool createdLocalDataContext = false;

            try
            {
                if (dataContext != null)
                {
                    dc = dataContext;
                }
                else
                {
                    createdLocalDataContext = true;
                    dc = new AspStoreDataContext(Store.ConnectionString);
                }

                var p = dc.Products.Where(e => e.ProductID == ProductID).FirstOrDefault();

                if (p == null || callback == null)
                    return;

                var variants = dc.ProductVariants.Where(e => e.ProductID == ProductID).OrderByDescending(e => e.IsDefault).OrderBy(e => e.Price).ToList();

                var m = dc.Manufacturers.Where(e => e.ManufacturerID == dc.ProductManufacturers.Where(f => f.ProductID == ProductID).Select(f => f.ManufacturerID).Single()).Single();

                var cats = dc.Categories.Where(e => dc.ProductCategories.Where(f => f.ProductID == ProductID).Select(f => f.CategoryID).Contains(e.CategoryID)).ToList();

                var product = new TProduct();
                product.Initialize(Store, p, variants, m, cats, dc);

                callback(product);

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
            finally
            {
                if (createdLocalDataContext && dc != null)
                {
                    dc.Dispose();
                    dc = null;
                }
            }
        }

        #endregion

        private readonly string connectionString;
        private readonly IWebStore Store;
        private string tag;
        public int CountUpdates;
        public int CountProcessed;
        public int PercentComplete;
        public int TotalCount;
        public Action<int> progressCallback = null;

        public ProductUpdateManager(IWebStore store)
        {
            this.Store = store;
            this.connectionString = Store.ConnectionString;
        }

        private void NotifyProgress(int countCompleted, int countTotal)
        {
            var pct = countTotal == 0 ? 0 : (countCompleted * 100) / countTotal;
            NotifyProgress(pct);
        }

        private void NotifyProgress(int pct)
        {
            bool sendNotification = false;

            lock (this)
            {
                if (pct != PercentComplete)
                {
                    // don't want to hold the lock while sending the notification
                    sendNotification = true;
                    PercentComplete = pct;
                }
            }

            if (sendNotification && progressCallback != null)
                progressCallback(PercentComplete);
        }

        public void Run(Func<TProduct, bool> productFunction, bool useParallelOperations = true, string tag=null)
        {
            Run(productFunction, CancellationToken.None, useParallelOperations, null, tag);
        }

        public void Run(Func<TProduct, bool> productFunction, CancellationToken cancelToken, bool useParallelOperations = true, List<int> productList = null, string tag = null)
        {
            PercentComplete = -1; // to force a change on first 0
            CountUpdates = 0;
            CountProcessed = 0;
            this.tag = tag;

            using (var dc = new AspStoreDataContextReadOnly(connectionString))
            {
                // do it this way because otherwise bad duplicates found in SQL cause things to choke

                var pManufactures = dc.ProductManufacturers.Where(e => dc.Products.Where(f => f.Deleted == 0).Select(f => e.ProductID).Contains(e.ProductID)).Select(e => new { e.ProductID, e.ManufacturerID }).ToList();
                var productManufactures = new Dictionary<int, int>();
                foreach (var pm in pManufactures)
                {
                    if (productManufactures.ContainsKey(pm.ProductID))
                    {
                        continue;
                    }

                    productManufactures.Add(pm.ProductID, pm.ManufacturerID);
                }
                var storeManufacturers = dc.Manufacturers.ToDictionary(k => k.ManufacturerID, v => v);
                var storeCategories = dc.Categories.ToDictionary(k => k.CategoryID, v => v);

                // note that includes both current and obsolete products
                List<int> selectedProductIDs = productList;

                // if no list passed in, then take everything

                if (selectedProductIDs == null)
                {
                    switch(tag)
                    {
                        case "looks:1":
                            selectedProductIDs = dc.Products.Where(e => e.Published == 1 && e.Deleted == 0 && e.Looks==1).Select(e => e.ProductID).ToList();
                            break;

                        case null:
                        default:
                            selectedProductIDs = dc.Products.Where(e => e.Published == 1 && e.Deleted == 0).Select(e => e.ProductID).ToList();
                            break;
                    }
                    
                }

                TotalCount = selectedProductIDs.Count();

                dc.Dispose();

                if (TotalCount == 0)
                    return;

                Action notifyProgress = () =>
                    {
                        NotifyProgress(CountProcessed, TotalCount);
                    };

                notifyProgress();

                // start a task to fill the nuget queue with granular sets of products to be consumed within the while loop

                var reset = new ManualResetEventSlim(false);

                var queue = new BufferBlock<UpdateManagerNuget>();
                var t = Task.Factory.StartNew(async () =>
                {
                    const int takeCount = 200;
                    int skipCount = 0;
                    int nugetCount = 0;

                    while (true)
                    {
                        var productSet = selectedProductIDs.Skip(skipCount).Take(takeCount).ToList();
                        var nuget = new UpdateManagerNuget(++nugetCount, connectionString, productSet);
                        await queue.SendAsync(nuget);

                        // if found less than take, then end of line

                        if (nuget.ProductSet.Count < takeCount || nuget.ProductSet.Count == 0 || cancelToken.IsCancellationRequested)
                        {
                            nuget.IsLast = true;
                            break;
                        }

                        if (queue.Count >= 5)
                        {
                            reset.Wait();
                            reset.Reset();
                        }

                        skipCount += takeCount;
                    }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);


                #region Main While Loop

                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    var nuget = queue.Receive<UpdateManagerNuget>();
                    reset.Set();

                    if (nuget.ProductSet.Count == 0)
                        break;

                    var options = new ParallelOptions()
                    {
                        CancellationToken = CancellationToken.None,
                        MaxDegreeOfParallelism = 50,
                        TaskScheduler = TaskScheduler.Default,
                    };

                    if (useParallelOperations)
                    {
                        Parallel.ForEach(nuget.ProductSet, options, (int productID, ParallelLoopState loopState) =>
                        {
                            if (cancelToken.IsCancellationRequested)
                                loopState.Stop();


                            Product p = null;
                            List<ProductVariant> variants = null;
                            int manufacturerID = 0;
                            Manufacturer manufacturer = null;

                            try
                            {
                                using (var dc2 = new AspStoreDataContext(connectionString))
                                {
                                    if (nuget.Products.TryGetValue(productID, out p) && nuget.ProductVariants.TryGetValue(productID, out variants)
                                        && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                                    {
                                        // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                        // derrive the collection of categories to which this product belongs
                                        var catIDs = nuget.ProductCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                        var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                        var storeProduct = new TProduct();
                                        storeProduct.Initialize(Store, p, variants, manufacturer, categories, dc2);

                                        if (productFunction != null)
                                        {
                                            var result = productFunction(storeProduct);
                                            if (result)
                                                Interlocked.Increment(ref CountUpdates);
                                        }

                                    }
                                }
                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine(Ex.ToString());
                            }

                            Interlocked.Increment(ref CountProcessed);
                            notifyProgress();
                        });

                    }
                    else
                    {
                        foreach (var productID in nuget.ProductSet)
                        {

                            if (cancelToken.IsCancellationRequested)
                                break;


                            Product p = null;
                            List<ProductVariant> variants = null;
                            int manufacturerID = 0;
                            Manufacturer manufacturer = null;

                            try
                            {
                                using (var dc2 = new AspStoreDataContext(connectionString))
                                {
                                    if (nuget.Products.TryGetValue(productID, out p) && nuget.ProductVariants.TryGetValue(productID, out variants)
                                        && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                                    {
                                        // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                        // derrive the collection of categories to which this product belongs
                                        var catIDs = nuget.ProductCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                        var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                        var storeProduct = new TProduct();
                                        storeProduct.Initialize(Store, p, variants, manufacturer, categories, dc2);

                                        if (productFunction != null)
                                        {
                                            var result = productFunction(storeProduct);
                                            if (result)
                                                Interlocked.Increment(ref CountUpdates);
                                        }
                                    }
                                }
                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine(Ex.ToString());
                            }

                            Interlocked.Increment(ref CountProcessed);
                            notifyProgress();
                        }
                    }


                    if (nuget.IsLast)
                    {
                        Debug.WriteLine(string.Format("Last nuget ID: {0}", nuget.ID));
                        break;
                    }
                }
                #endregion

            }

            return;
        }
    }
}
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
    public class InsideFabricUpdateManager
    {

        public class Nuget
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

            public int ID {get; set;}

            public List<int> ProductSet { get; private set; }
            public Dictionary<int, Product> Products { get; private set; }
            public Dictionary<int, ProductVariant> ProductVariants { get; private set; }
            public List<ProductCategory> ProductCategories { get; private set; }
            public bool IsLast { get; set; }

            public Nuget(int ID, string connectionString, List<int> productSet)
            {
                this.ID = ID;
                IsLast = false;
                ProductSet = productSet;

                using (var dc = new AspStoreDataContextReadOnly(connectionString))
                {
                    if (ProductSet.Count == 0)
                        return;

                    Products = dc.Products.Where(e => ProductSet.Contains(e.ProductID)).ToDictionary(k => k.ProductID, v => v);
                    ProductVariants = dc.ProductVariants.Where(e => ProductSet.Contains(e.ProductID) && e.IsDefault == 1).ToDictionary(k => k.ProductID, v => v);

                    ProductCategories = dc.ProductCategories.Where(e => ProductSet.Contains(e.ProductID)).Select(e => new Nuget.ProductCategory(e.ProductID, e.CategoryID)).ToList();
                }
            }
        }



        private readonly string connectionString;

        public int CountUpdates;
        public int CountProcessed;
        public int PercentComplete;
        public int TotalCount;

        public InsideFabricUpdateManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private void NotifyProgress(int countCompleted)
        {
            bool sendNotification = false;

            lock (this)
            {
             
                var pct = (int)Math.Round(((decimal)countCompleted / (decimal)TotalCount) * 100M, 0);
                if (pct != PercentComplete)
                {
                    // don't want to hold the lock while sending the notification
                    sendNotification = true;
                    PercentComplete = pct;
                }
            }

            if (sendNotification)
                MaintenanceHub.NotifyRunProductActionPctComplete(PercentComplete);
        }

        public void Run(Func<InsideFabricProduct, bool> productFunction, bool useParallelOperations = true)
        {
            Run(productFunction, CancellationToken.None, useParallelOperations);
        }

        public void Run(Func<InsideFabricProduct, bool> productFunction, CancellationToken cancelToken, bool useParallelOperations = true)
        {
            PercentComplete = -1; // to force a change on first 0
            CountUpdates = 0;
            CountProcessed = 0;

            using (var dc = new  AspStoreDataContextReadOnly(connectionString))
            {
#if true
                // there are 2,300 duplicates in this table - even though should not be, so need to 
                // be careful about how this collection is generated

                var pManufactures = dc.ProductManufacturers.Where(e => dc.Products.Where(f => f.Deleted == 0).Select(f => e.ProductID).Contains(e.ProductID)).Select(e => new { e.ProductID, e.ManufacturerID}).ToList();
                var productManufactures = new Dictionary<int, int>();
                foreach (var pm in pManufactures)
                {
                    if (productManufactures.ContainsKey(pm.ProductID))
                    {
                        continue;
                    }

                    productManufactures.Add(pm.ProductID, pm.ManufacturerID);
                }
#else
                var productManufactures = dc.ProductManufacturers.ToDictionary(k => k.ProductID, v => v.ManufacturerID).Select(e => new { e.ProductID, e.ManufacturerID}).ToList();;
#endif
                var storeManufacturers = dc.Manufacturers.ToDictionary(k => k.ManufacturerID, v => v);
                var storeCategories = dc.Categories.ToDictionary(k => k.CategoryID, v => v);

                // note that includes both current and obsolete products

                var allProductIDs = dc.Products.Where(e => e.Published == 1 && e.Deleted == 0).Select(e => e.ProductID).ToList();
                TotalCount = allProductIDs.Count();

                dc.Dispose();

                if (TotalCount == 0)
                    return;

                NotifyProgress(0);

                // start a task to fill the nuget queue with granular sets of products to be consumed within the while loop

                var reset = new ManualResetEventSlim(false);

                var queue = new BufferBlock<Nuget>();
                var t = Task.Factory.StartNew(async () =>
                    {
                        const int takeCount = 200;
                        int skipCount = 0;
                        int nugetCount = 0;

                        while (true)
                        {
                            var productSet = allProductIDs.Skip(skipCount).Take(takeCount).ToList();
                            var nuget = new Nuget(++nugetCount, connectionString, productSet);
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

                    var nuget = queue.Receive<Nuget>();
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

                            Interlocked.Increment(ref CountProcessed);

                            Product p = null;
                            ProductVariant pv = null;
                            int manufacturerID = 0;
                            Manufacturer manufacturer = null;

                            try
                            {
                                using (var dc2 = new AspStoreDataContext(connectionString))
                                {
                                    if (nuget.Products.TryGetValue(productID, out p) && nuget.ProductVariants.TryGetValue(productID, out pv)
                                        && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                                    {
                                        // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                        // derrive the collection of categories to which this product belongs
                                        var catIDs = nuget.ProductCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                        var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                        var fabricProduct = new InsideFabricProduct(p, pv, manufacturer, categories, dc2);

                                        if (productFunction != null)
                                        {
                                            var result = productFunction(fabricProduct);
                                            if (result)
                                                NotifyProgress(Interlocked.Increment(ref CountUpdates));
                                        }

                                    }
                                }
                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine(Ex.ToString());
                            }
                        });

                    }
                    else
                    {
                        foreach (var productID in nuget.ProductSet)
                        {
                            Interlocked.Increment(ref CountProcessed);

                            Product p = null;
                            ProductVariant pv = null;
                            int manufacturerID = 0;
                            Manufacturer manufacturer = null;

                            try
                            {
                                using (var dc2 = new AspStoreDataContext(connectionString))
                                {
                                    if (nuget.Products.TryGetValue(productID, out p) && nuget.ProductVariants.TryGetValue(productID, out pv)
                                        && productManufactures.TryGetValue(productID, out manufacturerID) && storeManufacturers.TryGetValue(manufacturerID, out manufacturer))
                                    {
                                        // have both a good product and variant for default=1 as well as a ref to the manufacturer

                                        // derrive the collection of categories to which this product belongs
                                        var catIDs = nuget.ProductCategories.Where(e => e.ProductID == productID).Select(e => e.CategoryID).ToList();
                                        var categories = storeCategories.Where(e => catIDs.Contains(e.Key)).Select(e => e.Value).ToList();

                                        var fabricProduct = new InsideFabricProduct(p, pv, manufacturer, categories, dc2);

                                        if (productFunction != null)
                                        {
                                            var result = productFunction(fabricProduct);
                                            if (result)
                                                NotifyProgress(Interlocked.Increment(ref CountUpdates));
                                        }
                                    }
                                }
                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine(Ex.ToString());
                            }
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
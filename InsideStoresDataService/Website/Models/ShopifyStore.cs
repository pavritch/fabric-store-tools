using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using System.Web;
using Gen4.Util.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShopifySharp;
using Website.Shopify.Entities;

namespace Website
{
    /// <summary>
    /// Highest level entity for shopify integration.
    /// </summary>
    public class ShopifyStore : IShutdownNotify
    {
        private Dictionary<StoreKeys, IWebStore> webStores;

        public string StoreDomain { get; private set; }
        public string StoreUrl { get; private set; }
        public string AppPassword { get; private set; }
        public int ApiProductBatchSize { get; private set; }
        public bool EnableProductEventProcessing { get; private set; }
        public int ProductEventProcessingStartupDelay { get; private set; }
        public bool SupportSwatches { get; private set; }

        public string ConnectionString { get; private set; }
        public List<LongOperationProgress> LongOperations { get; private set; }

        public ShopifyApiSentry ApiSentry { get; private set; }

        public ShopifyStore(Dictionary<StoreKeys, IWebStore> webStores)
        {
            this.webStores = webStores;

            ConnectionString = ConfigurationManager.ConnectionStrings["ShopifyConnectionString"].ConnectionString;
            StoreDomain = ConfigurationManager.AppSettings["ShopifyStoreDomain"];
            StoreUrl = string.Format("https://{0}", StoreDomain);
            AppPassword = ConfigurationManager.AppSettings["ShopifyAppPassword"];
            ApiProductBatchSize = int.Parse(ConfigurationManager.AppSettings["ShopifyApiProductBatchSize"]);
            EnableProductEventProcessing = bool.Parse(ConfigurationManager.AppSettings["ShopifyEnableProductEventProcessing"]);
            ProductEventProcessingStartupDelay = int.Parse(ConfigurationManager.AppSettings["ShopifyProductEventProcessingStartupDelay"]);
            SupportSwatches = bool.Parse(ConfigurationManager.AppSettings["ShopifySupportSwatches"]);
            var bucketSize = int.Parse(ConfigurationManager.AppSettings["ShopifyApiBucketMaxLevel"]);
            var leakRate = int.Parse(ConfigurationManager.AppSettings["ShopifyApiBucketLeakRate"]);
            ApiSentry = new ShopifyApiSentry(bucketSize, leakRate);
            LongOperations = new List<LongOperationProgress>();

            WriteLog(ShopifyLogLocus.General, ShopifyLogType.Information, "Service started.");
        }

        public void RegisterLongOperation(LongOperationProgress op)
        {
            lock (this)
            {
                LongOperations.Add(op);
            }
        }

        public void DeleteLongOperation(string shortID)
        {
            lock (this)
            {
                LongOperations.RemoveAll(e => e.ShortId == shortID);
            }
        }

        public void DeleteLongOperation(Guid id)
        {
            lock (this)
            {
                LongOperations.RemoveAll(e => e.Id == id);
            }
        }


        public IWebStore GetWebStore(StoreKeys storeKey)
        {
            IWebStore store = null;
            webStores.TryGetValue(storeKey, out store);
            return store;
        }

        /// <summary>
        /// Support for IShutdownNotify
        /// </summary>
        /// <remarks>
        /// Will be notified on ASP.NET shutdown.
        /// </remarks>
        public void Stop()
        {
            ApiSentry.ShutDown();
        }

        public void TruncateProducts()
        {
            using (var dc = new ShopifyDataContext(ConnectionString))
            {
                dc.Products.TruncateTable();
            }
        }

        public void TruncateLog()
        {
            using (var dc = new ShopifyDataContext(ConnectionString))
            {
                dc.Logs.TruncateTable();
            }
        }

        public void TruncateProductEvents()
        {
            using (var dc = new ShopifyDataContext(ConnectionString))
            {
                dc.ProductEvents.TruncateTable();
            }
        }
        public void TruncateLiveShopifyProducts()
        {
            using (var dc = new ShopifyDataContext(ConnectionString))
            {
                dc.LiveShopifyProducts.TruncateTable();
            }
        }

        /// <summary>
        /// Tracker for API stats.
        /// </summary>
        /// <remarks>
        /// How eventually used is not yet determined. But should be called by all from the get go.
        /// </remarks>
        /// <param name="isSuccess"></param>
        public void NotifyApiRequest(bool isSuccess)
        {

        }

        /// <summary>
        /// Tracker for product event stats.
        /// </summary>
        /// <remarks>
        /// How eventually used is not yet determined. But should be called by all from the get go.
        /// </remarks>
        /// <param name="result"></param>
        public void NotifyProcessProductEvent(ShopifyProductEventResult result)
        {

        }

        /// <summary>
        /// Get count of current product events queue.
        /// </summary>
        /// <returns></returns>
        public int GetProductEventsCount()
        {
            using (var dc = new ShopifyDataContextReadOnly(ConnectionString))
            {
                return dc.ProductEvents.Count();
            }
        }

        #region Testing
        public void TestSentry()
        {

            const string REQUIRED_FIELDS = "id,handle";
            const int BATCH_SIZE = 10;

            for (int i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(async () =>
                {
                    long sinceID = 0L;
                    var serviceProduct = new ShopifyProductService(StoreUrl, AppPassword);

                    var rnd = new Random(i * 100);

                    while (true)
                    {
                        try
                        {
                            await Task.Delay(rnd.Next(300, 8000));
                            var start = DateTime.Now;
                            var isOkay = await ApiSentry.WaitMyTurn();
                            var end = DateTime.Now;

                            if (!isOkay)
                                break;
                            Debug.WriteLine(string.Format("Request Delayed: {0}", end - start));

                            var products = await serviceProduct.ListAsync(new ShopifySharp.Filters.ShopifyProductFilter() { Limit = BATCH_SIZE, Fields = REQUIRED_FIELDS, SinceId = sinceID });

                            if (products.Count() == 0)
                                break;

                            sinceID = products.Last().Id.GetValueOrDefault();

                            if (products.Count() < BATCH_SIZE)
                                break;
                        }
                        catch (ShopifyRateLimitException Ex)
                        {
                            Debug.WriteLine(Ex.ToString());
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.ToString());
                        }
                    }

                    Debug.WriteLine(string.Format("Task {0} completed.", i));

                });

            }


        }
        
        #endregion

        #region Logging

        public class VerboseLogEntry
        {
            public string Created { get; set; }
            public string Locus { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }

        public List<VerboseLogEntry> GetLog(int maxCount)
        {
            using (var dc = new ShopifyDataContextReadOnly(ConnectionString))
            {
                var records = (from r in dc.Logs
                              orderby r.RecordID descending
                              select new
                              {
                                  r.Created,
                                  r.Locus,
                                  r.Type,
                                  r.Message
                              }).Take(maxCount);


                var list = new List<VerboseLogEntry>();
                foreach(var r in records)
                {
                    var output = new VerboseLogEntry()
                    {
                        Created = string.Format("{0:G}", r.Created), // 2009-06-15T13:45:30 -> 6/15/2009 1:45:30 PM (en-US)
                        Locus = r.Locus.ToString(),
                        Type = r.Type.ToString(),
                        Message = r.Message
                    };

                    list.Add(output);
                }

                return list;
            }
        }

        public void WriteLog(ShopifyLogLocus locus, ShopifyLogType type, string message, object data = null)
        {
            var logEntry = new Shopify.Entities.Log()
            {
                Locus = (int)locus,
                Type = (int)type,
                Message = message.Left(1023),
                Data = data != null ? JsonConvert.SerializeObject(data, Formatting.Indented, SerializerSettings) : null,
                Created = DateTime.Now,
            };

            using (var dc = new ShopifyDataContext(ConnectionString))
            {
                dc.Logs.InsertOnSubmit(logEntry);
                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Populate SQL table by query live Shopify store.
        /// </summary>
        /// <param name="truncateExisting"></param>
        public void PopulateFromLiveProducts(bool truncateExisting)
        {
            const string REQUIRED_FIELDS = "id,handle";
            var longOp = new LongOperationProgress("PopulateFromLiveStore", "Populate SQL from Live Store");
            this.RegisterLongOperation(longOp);

            long sinceID = 0;
            int countTotal = 0;
            int countCompleted = 0;
            
            using (var dc = new ShopifyDataContextReadOnly(ConnectionString))
            {
                if (truncateExisting)
                {
                    longOp.WriteLog("Truncating existing products in table.");
                    dc.LiveShopifyProducts.TruncateTable();
                }
                countCompleted = dc.LiveShopifyProducts.Count();

                if (countCompleted > 0)
                {
                    longOp.WriteLog(string.Format("Continuation with {0:N0} existing products.", countCompleted));
                    sinceID = dc.LiveShopifyProducts.Max(e => e.ShopifyProductID);
                    longOp.WriteLog(string.Format("Highest existing Shopify ProductId: {0}", sinceID));
                }
            }

            longOp.StatusMessage = "Initializing.";

            Task.Factory.StartNew(async () =>
            {
                var serviceProduct = new ShopifyProductService(StoreUrl, AppPassword);

                await ApiSentry.WaitMyTurn();
                countTotal = await serviceProduct.CountAsync();
                NotifyApiRequest(true);

                longOp.WriteLog(string.Format("Total products: {0:N0}", countTotal));
                longOp.Update(countTotal, countCompleted);
                longOp.StatusMessage = "Reading list of existing products.";

                bool done = false;

                while (!done)
                {
                    bool requireRetry = false;
                    bool requireDelay = false;
                    int attempts = 0;

                    do
                    {
                        if (longOp.CancelToken.IsCancellationRequested)
                        {
                            done = true;
                            longOp.WriteLog("Operation cancelled.");
                            break;
                        }

                        attempts++;

                        if (attempts > 3)
                        {
                            done = true;
                            longOp.WriteLog("Terminated due to too many retries.");
                            longOp.Cancel();
                            break;
                        }

                        // inject 10-second delay
                        if (requireDelay)
                        {
                            await Task.Delay(1000 * 10);
                            requireDelay = false;
                        }

                        try
                        {
                            var isOkay = await ApiSentry.WaitMyTurn();
                            if (!isOkay )
                            {
                                longOp.WriteLog("WaitMyTurn() failed.");
                                longOp.Cancel();
                                done = true;
                                break;
                            }
                            if (longOp.CancelToken.IsCancellationRequested)
                            {
                                done = true;
                                break;
                            }

                            var products = await serviceProduct.ListAsync(new ShopifySharp.Filters.ShopifyProductFilter() { Limit = ApiProductBatchSize, Fields = REQUIRED_FIELDS, SinceId = sinceID });
                            NotifyApiRequest(true);

                            if (products.Count() == 0)
                            {
                                done = true;
                                longOp.SetFinished("Completed successfully.");
                                break;
                            }

                            // persist to SQL

                            using (var dc = new ShopifyDataContext(ConnectionString))
                            {
                                foreach (var product in products)
                                {
                                    var liveProduct = new LiveShopifyProduct();

                                    liveProduct.ShopifyProductID = product.Id.Value;
                                    liveProduct.ShopifyHandle = product.Handle;

                                    var storeCode = product.Handle.Substring(0, 2).ToUpper();
                                    switch (storeCode)
                                    {
                                        case "IF":
                                            liveProduct.StoreID = (int)StoreKeys.InsideFabric;
                                            break;

                                        case "IA":
                                            liveProduct.StoreID = (int)StoreKeys.InsideAvenue;
                                            break;

                                        case "IW":
                                            liveProduct.StoreID = (int)StoreKeys.InsideWallpaper;
                                            break;

                                        default:
                                            throw new Exception("Unknown store code in handle.");
                                    }

                                    var sProductID = product.Handle.CaptureWithinMatchedPattern(@"^\w\w-(?<capture>(\d+))-");
                                    var productID = int.Parse(sProductID);
                                    liveProduct.ProductID = productID;

                                    dc.LiveShopifyProducts.InsertOnSubmit(liveProduct);
                                }

                                dc.SubmitChanges();
                            }

                            countCompleted += products.Count();
                            sinceID = products.Last().Id.GetValueOrDefault();
                            longOp.Update(countTotal, countCompleted);

                            if (products.Count() < ApiProductBatchSize)
                            {
                                done = true;
                                longOp.SetFinished("Completed successfully.");
                                break;
                            }
                        }
                        catch (ShopifyRateLimitException Ex)
                        {
                            NotifyApiRequest(false);
                            Debug.WriteLine(Ex.ToString());
                            longOp.WriteLog("Rate limit exception.");
                            requireRetry = true;
                            requireDelay = true;
                        }
                        catch(ShopifyException Ex)
                        {
                            NotifyApiRequest(false);
                            Debug.WriteLine(Ex.ToString());
                            requireRetry = true;
                            longOp.WriteLog("Exception: " + Ex.Message);
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.ToString());
                            requireRetry = true;
                            longOp.WriteLog("Exception: " + Ex.Message);
                        }


                    } while (requireRetry);
                }

            });

            // this operation completes asynchronously in the background.
        }


        /// <summary>
        /// Insert an event for each known product to pull the origina/virgin information from live store.
        /// </summary>
        public void EnqueueVirginReadProducts()
        {
            var longOp = new LongOperationProgress("EnqueueVirginReadProducts", "Enqueue Virgin Read Products");
            this.RegisterLongOperation(longOp);

            int countTotal = 0;
            int countCompleted = 0;
            
            using (var dc = new ShopifyDataContextReadOnly(ConnectionString))
            {
                countTotal = dc.LiveShopifyProducts.Count();
                longOp.WriteLog(string.Format("Adding {0:N0} products to event queue.", countTotal));
            }

            longOp.StatusMessage = "Initializing.";

            Task.Factory.StartNew(() =>
            {
                try
                {
                    bool hasBeenCancelled = false;
                    longOp.StatusMessage = "Adding products to queue.";
                    longOp.Update(countTotal, countCompleted);

                    List<LiveShopifyProduct> liveProducts;
                    using (var dc = new ShopifyDataContextReadOnly(ConnectionString))
                    {
                        liveProducts = dc.LiveShopifyProducts.ToList();

                        if (liveProducts.Count() == 0)
                            throw new Exception("Live products table is empty.");

                        if (dc.Products.Count() > 0)
                            throw new Exception("Products table is not empty.");

                        if (dc.ProductEvents.Count() > 0)
                            throw new Exception("ProductEvents table is not empty.");
                    }

                    using (var dc = new ShopifyDataContext(ConnectionString))
                    {
                        foreach (var p in liveProducts)
                        {
                            if (longOp.CancelToken.IsCancellationRequested)
                            {
                                hasBeenCancelled = true;
                                break;
                            }

                            var productEvent = new Shopify.Entities.ProductEvent()
                            {
                                Operation = (int)ShopifyProductOperation.VirginReadProduct,
                                StoreID = p.StoreID,
                                ProductID = p.ProductID,
                                Created = DateTime.Now,
                                Data = JsonConvert.SerializeObject(p, Formatting.Indented, SerializerSettings),
                            };

                            dc.ProductEvents.InsertOnSubmit(productEvent);
                            dc.SubmitChanges();
                            countCompleted++;
                            longOp.Update(countTotal, countCompleted);
                        }

                        if (!hasBeenCancelled)
                            longOp.SetFinished("Completed successfully.");
                    }
                }
                catch (Exception Ex)
                {
                    var msg = string.Format("Exception: {0}", Ex.Message);
                    longOp.WriteLog(msg);
                    longOp.Error(msg);
                }

            });

            // this operation completes asynchronously in the background.
        }

        public void WriteProductErrorLog(string message, object data = null)
        {
            WriteLog(ShopifyLogLocus.Products, ShopifyLogType.Error, message, data);
        }

        public void WriteProductInformationLog(string message, object data = null)
        {
            WriteLog(ShopifyLogLocus.Products, ShopifyLogType.Information, message, data);
        } 
        #endregion

        #region Serializer Support

        /// <summary>
        /// Common settings used for serialization/deserialization.
        /// </summary>
        private JsonSerializerSettings SerializerSettings
        {
            get
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>() { new IsoDateTimeConverter() },
                    TypeNameHandling = TypeNameHandling.None,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                };

                return jsonSettings;
            }
        }

        #endregion 
    }
}
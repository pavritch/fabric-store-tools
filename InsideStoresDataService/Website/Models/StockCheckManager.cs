using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace Website
{
    /// <summary>
    /// Main class which interfaces with Shane's stock checking API.
    /// Each store has it's own instance.
    /// </summary>
    /// <remarks>
    /// Can be enabled/disabled via web.config. Config also has the URL endpoint.
    /// When disabled or not able to connect, will return as if all vendors have None capabilities,
    /// and product requests will return as Unavailable. Since there could be a delta between 
    /// vendor DLLs in the stock check web service and vendors in SQL, this code will ensure that
    /// all vendors are represented - by injected them as None if the API does not mention them.
    /// </remarks>
    public class StockCheckManager : IStockCheckManager
    {
        protected IWebStore Store { get; private set; }

        protected bool enableLiveStockChecks;
        protected string stockCheckServiceUrl;
        protected Dictionary<int, string> manufacturers;
        private System.Threading.Timer vendorInfoTimer;
        private bool isBusy;
        private IpAddressTracker IpTracker;

        private List<StockCheckVendorInfo> listStockCheckVendorInfo = null;

        private bool isReady = false;

        public StockCheckManager(IWebStore store)
        {
            this.Store = store;
            IpTracker = new IpAddressTracker();

            enableLiveStockChecks = bool.Parse(WebConfigurationManager.AppSettings[string.Format("{0}EnableLiveStockChecks", Store.StoreKey.ToString())]);
            stockCheckServiceUrl = WebConfigurationManager.AppSettings["StockCheckServiceUrl"];
            manufacturers = GetManufactures();

            // start out with nothing - until enabled/populated/ready
            listStockCheckVendorInfo = MakeNullVendorCapabilitiesList(manufacturers);

            if (enableLiveStockChecks)
            {
                // wait 15 sec and then cycle on 15sec period

                // wire up an app-wide one-second interval timer
                vendorInfoTimer = new System.Threading.Timer((st) =>
                {
                    // jsut in case didn't finish last time
                    if (isBusy)
                        return;

                    isBusy = true;
                    RefreshVendorInfo();
                    isBusy = false;
                }, null, 15 * 1000, 15 * 1000);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return enableLiveStockChecks;
            }
        }

        /// <summary>
        /// Return dic of manufacturers with ID and name. ID is key.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetManufactures()
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var vendors = (from m in dc.Manufacturers
                               where m.Deleted == 0 && m.Published == 1
                               select new
                               {
                                   m.ManufacturerID,
                                   m.Name,
                               }
                              ).ToDictionary(k => k.ManufacturerID, v => v.Name);
                return vendors;
            }
        }

        /// <summary>
        /// Create a result which has all vendors having None capabilities.
        /// </summary>
        /// <remarks>
        /// Useful for when a null state is needed, things disabled, offline, etc.
        /// </remarks>
        /// <param name="manufacturers"></param>
        /// <returns></returns>
        private List<StockCheckVendorInfo> MakeNullVendorCapabilitiesList(Dictionary<int, string> manufacturers)
        {
            var list = new List<StockCheckVendorInfo>();

            foreach (var item in manufacturers.OrderBy(e => e.Key))
                list.Add(new StockCheckVendorInfo(StockCapabilities.None, item.Value, item.Key));

            return list;
        }

        /// <summary>
        /// Acquire fresh vendor capabilities info from the service API.
        /// </summary>
        private async void RefreshVendorInfo()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(stockCheckServiceUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var uri = string.Format("api/1.0/{0}/vendors", Store.StoreKey);
                    HttpResponseMessage response = await client.GetAsync(uri);

                    response.EnsureSuccessStatusCode(); // throw exception if not success

                    var listVendors = await response.Content.ReadAsAsync<List<StockCheckVendorInfo>>();

                    // shape the list to strictly conform to our vendors

                    listVendors.RemoveAll(e => !manufacturers.ContainsKey(e.VendorId));
                    foreach (var m in manufacturers)
                    {
                        // if not in listVendors, then add a null record
                        if (listVendors.Where(e => e.VendorId == m.Key).Count() > 0)
                            continue;

                        listVendors.Add(new StockCheckVendorInfo(StockCapabilities.None, m.Value, m.Key));
                    }

                    lock (this)
                    {
                        listStockCheckVendorInfo = listVendors;
                        isReady = true;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);

                lock (this)
                {
                    listStockCheckVendorInfo = MakeNullVendorCapabilitiesList(manufacturers);
                    isReady = false;
                }
            }

        }

        /// <summary>
        /// For the set of variants, create a result which has all products with Unavailable stock status.
        /// </summary>
        /// <param name="variants"></param>
        /// <returns></returns>
        private List<StockCheckResult> MakeNullStockCheckResults(IEnumerable<StockCheckQuery> variants)
        {
            var list = new List<StockCheckResult>();

            foreach (var variant in variants)
            {
                var v = new StockCheckResult
                {
                    VariantId = variant.VariantId,
                    StockCheckStatus = StockCheckStatus.Unavailable
                };
                list.Add(v);
            }
            return list;
        }

        /// <summary>
        /// Query the API for status of the given set of variants.
        /// </summary>
        /// <remarks>
        /// Variants must be true products, not swatches.
        /// </remarks>
        /// <param name="variants"></param>
        /// <returns></returns>
        private async Task<List<StockCheckApiResult>> QueryStockForVariants(IEnumerable<StockCheckQuery> variants)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(stockCheckServiceUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var uri = string.Format("api/1.0/{0}/stockcheck/", Store.StoreKey);

                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, variants);

                    response.EnsureSuccessStatusCode(); // throw exception if not success

                    var listVariants = await response.Content.ReadAsAsync<List<StockCheckApiResult>>();

                    return listVariants;
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);

                // something is wrong... take offline until timer tick goes off and checks again.

                lock (this)
                {
                    listStockCheckVendorInfo = MakeNullVendorCapabilitiesList(manufacturers);
                    isReady = false;
                }

                throw;
            }
        }

        /// <summary>
        /// Return a list of all store vendors with their associated stock check capabilities.
        /// </summary>
        /// <returns></returns>
        public Task<List<StockCheckVendorInfo>> GetStockCheckVendorInformation()
        {
            lock (this)
            {
                return Task.FromResult(listStockCheckVendorInfo);
            }
        }

        /// <summary>
        /// Check stock for a single variant.
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<StockCheckResult> CheckStockForSingleVariant(int variantID, int quantity, string ip=null)
        {
            if (!enableLiveStockChecks || !isReady)
            {
                return new StockCheckResult
                {
                    VariantId = variantID,
                    StockCheckStatus = StockCheckStatus.Unavailable
                };
            }

            var query = new List<StockCheckQuery>()
            {
                new StockCheckQuery()
                {
                     VariantId = variantID,
                     Quantity = quantity,
                     ForceFetch = false,
                }
            };

            var variantResults = await CheckStockForVariants(query, ip);
            return variantResults[0];
        }


        /// <summary>
        /// Check stock for a set of variants.
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<List<StockCheckResult>> CheckStockForVariants(IEnumerable<StockCheckQuery> variants, string ip=null)
        {
            if (!enableLiveStockChecks || !isReady || !IpTracker.IsGoodAddress(ip))
                return MakeNullStockCheckResults(variants);

            try
            {
                var variantIDs = variants.Select(e => e.VariantId).ToList();

                var variantResults = await QueryStockForVariants(variants);

                var list = (from v in variantResults
                            select new StockCheckResult
                             {
                                 VariantId = v.VariantId,
                                 StockCheckStatus = v.StockCheckStatus,
                             }).ToList();

                // ensure list is correctly shaped
                list.RemoveAll(e => !variantIDs.Contains(e.VariantId));
                foreach (var id in variantIDs)
                {
                    if (list.Where(e => e.VariantId == id).Count() > 0)
                        continue;

                    list.Add(new StockCheckResult
                    {
                        VariantId = id,
                        StockCheckStatus = StockCheckStatus.Unavailable
                    });
                }
                return list;
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return MakeNullStockCheckResults(variants);
            }
        }

        /// <summary>
        /// User has requested to be notified when this item is back in stock.
        /// </summary>
        /// <remarks>
        /// Don't need to add to master mailing list since website store already did that.
        /// </remarks>
        /// <param name="variantID"></param>
        /// <param name="quantity"></param>
        /// <param name="email"></param>
        /// <returns>True if all is good, else false.</returns>
        public Task<bool> NotifyRequest(int variantID, int quantity, string email)
        {
            try
            {
                if (!Regex.IsMatch(email, Store.EmailRegEx))
                    throw new Exception("Invalid email.");

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    // status
                    // 0-Pending
                    // 1-Notified
                    // 2-Expired

                    // make sure no others for same
                    var found = dc.StockCheckNotifications.Where(e => e.Email == email && e.VariantID == variantID).Count() > 0;

                    // if already there, still report success - as all is really as it needs to be and we
                    // don't want to report a failure up the chain since we only report bool

                    if (!found)
                    {
                        var record = new Website.Entities.StockCheckNotification()
                        {
                            Created = DateTime.Now,
                            Email = email,
                            VariantID = variantID,
                            Quantity = quantity,
                            Status = 0,
                            DateNotified = null,
                        };

                        dc.StockCheckNotifications.InsertOnSubmit(record);
                        dc.SubmitChanges();
                    }
                }

                return Task.FromResult(true);
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return Task.FromResult(false);
            }
        }


        /// <summary>
        /// Peform one round of checks to see if any notifications can be sent out.
        /// </summary>
        /// <remarks>
        /// Can be run manually or via timer automation.
        /// </remarks>
        public void ReviewAndSendNotificationEmails(Func<bool> isStopRequested, int expirationDays = 180)
        {
            // expiration days is the number of days which must elapse before
            // a record/listing is considered stale and expired.

            // status:
            // 0-Pending
            // 1-Notified
            // 2-Expired

            try
            {
                Func<bool> isCancelled = () =>
                {
                    if (isStopRequested != null)
                        return isStopRequested();

                    return false;
                };

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    var urlPrefix = string.Format("https://www.{0}", Store.Domain);

                    var pendingVariants = dc.StockCheckNotifications.Where(e => e.Status == 0).Select(e => e.VariantID).Distinct().ToList();

                    var variantInfo = (from pv in dc.ProductVariants
                                       where pendingVariants.Contains(pv.VariantID)
                                       join p in dc.Products on pv.ProductID equals p.ProductID

                                       select new
                                       {
                                           VariantID = pv.VariantID,
                                           ProductID = pv.ProductID,
                                           IsDiscontinued = p.ShowBuyButton == 0 || p.Deleted == 1,
                                           IsPublished = p.Published == 1 && pv.Published == 1,
                                           InStock = pv.Inventory > 0,
                                           SKU = p.SKU + pv.SKUSuffix,
                                           Name = p.Name,
                                           OurPrice = pv.Price,
                                           ProductUrl = urlPrefix + "/p-" + p.ProductID.ToString() + "-" + p.SEName + ".aspx",
                                           ProductImageUrl = pv.ImageFilenameOverride != null ? urlPrefix + "/images/variant/icon/" + pv.ImageFilenameOverride : p.ImageFilenameOverride == null ? urlPrefix + "/images/nopicture.gif" : urlPrefix + "/images/product/icon/" + p.ImageFilenameOverride,
                                       }
                                       ).ToDictionary(k => k.VariantID, v => v);


                    var pendingItems = dc.StockCheckNotifications.Where(e => e.Status == 0).ToList();

                    // we want to send a single email when possible - ganging up all products for that
                    // customer.

                    var emails = pendingItems.Select(e => e.Email).Distinct().ToList();

                    foreach (var email in emails)
                    {
                        if (isCancelled())
                            break;

                        try
                        {
                            var itemsForEmail = pendingItems.Where(e => e.Email == email).ToList();

                            // build up a list of things we've found we can send to the user
                         
                            var inStockItems = (from e in itemsForEmail
                                        where variantInfo.ContainsKey(e.VariantID)
                                        let info = variantInfo[e.VariantID]
                                        where info.InStock && info.IsPublished && !info.IsDiscontinued
                                        select new
                                        {
                                            VariantID = e.VariantID,
                                            SqlEntity = e, // sql entity for the stock notification
                                            Info = info, // supplemental information about the product
                                        }
                                        ).ToList();

                            // did we find anything?

                            if (inStockItems.Count() == 0)
                                continue;

                            Debug.WriteLine(string.Format("Stock Check Notifier: Found {0} items for {1}", inStockItems.Count(), email));

                            // we now have a list of each item to include in the email, which is also
                            // the list of records to mark as notified.

                            // mark as sent - done before actually sending to error on the side of not sending the same email
                            // over and over again if some kind of problem.

                            foreach (var item in inStockItems)
                            {
                                item.SqlEntity.Status = 1; // 1-notified
                                item.SqlEntity.DateNotified = DateTime.Now;
                            }

                            dc.SubmitChanges();

                            // actually send the email

                            var products = (from p in inStockItems 
                                           select new Website.Emails.InStockNotificationEmail.ProductInfo()
                                            {
                                                SKU = p.Info.SKU,
                                                Name = p.Info.Name,
                                                OurPrice = p.Info.OurPrice,
                                                PageUrl = p.Info.ProductUrl,
                                                ImageUrl= p.Info.ProductImageUrl,
                                            }).ToList();

                            var emailTemplate = new Website.Emails.InStockNotificationEmail(products, email);

                            Store.SendEmail(emailTemplate).Wait();
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.Message);
                        }
                    }

                    pendingItems = null; // empty out

                    // set any that are too old as expired
                    if (!isCancelled())
                        dc.StockCheckNotifications.SetExpiredByAge(expirationDays);

                    // set any where the corresponding product is discontinued or deleted to expired

                    foreach(var variantID in pendingVariants)
                    {
                        if (isCancelled())
                            break;
                            
                        bool isExpired = false;

                        if (!variantInfo.ContainsKey(variantID))
                        {
                            // the variant no longer exists in SQL
                            isExpired = true;
                        }
                        else
                        {
                            // parent product marked discontinued
                            var item = variantInfo[variantID];
                            if (item.IsDiscontinued)
                                isExpired = true;
                        }

                        if (isExpired)
                            dc.StockCheckNotifications.SetVariantIDExpired(variantID);
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

        }
    }
}
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
using Website.Entities;
using Gen4;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;
using Website.Emails;

namespace Website
{
    // TODO - this class  is just bare bones and not ready to use

    /// <summary>
    /// Definition for Json data stored along with this record kind.
    /// </summary>
    public class OnlySwatchesCampaignData
    {
        public int CustomerID { get; set; }

        /// <summary>
        /// The list of variantIDs from prior swatch purchases.
        /// </summary>
        /// <remarks>
        /// Updated when staged.
        /// </remarks>
        public List<int> PurchasedProducts { get; set; }

        /// <summary>
        /// Optional call-out of non-default MVC template for email generation.
        /// </summary>
        /// <remarks>
        /// Null means use default.
        /// </remarks>
        public string Template { get; set; }

        /// <summary>
        /// Any optional reference information to associate with this campaign.
        /// </summary>
        public string Reference { get; set; }
    }



    public class OnlySwatchesTicklerCampaignHandler : TicklerCampaignHandler
    {



        /// <summary>
        /// Google tracking. Campaign is always ticker. But will track this source.
        /// </summary>
        public override string UtmSource
        {
            get
            {
                return "onlyswatches";
            }
        }


        public OnlySwatchesTicklerCampaignHandler(IWebStore store, ITicklerCampaignsManager manager) :
            base(store, manager)
        {

        }

        public override TicklerCampaignKind Kind
        {
            get
            {
                return TicklerCampaignKind.OnlySwatches;
            }
        }

        /// <summary>
        /// Physically send the email. All checks have been performed. Do it!
        /// </summary>
        /// <remarks>
        /// Either list or both can have products. The email template renders appropriately. 
        /// </remarks>
        /// <param name="customerID"></param>
        /// <param name="campaignID"></param>
        /// <param name="purchasedProductsInfo">Must not be null.</param>
        private void SendEmail(string template, int customerID, int campaignID, List<TicklerCampaignHandler.ProductInfo> purchasedProductsInfo, bool includeCoupon, int? orderNumber = null)
        {

            var emailAddress = GetCustomerEmailAddress(customerID);
            if (emailAddress == null)
                throw new Exception(string.Format("Missing email address for customerID: {0}", customerID));

//#if DEBUG
//            emailAddress = "peterav@pcdynamics.com";
//#endif
            Debug.Assert(template == "OnlySwatchesEmail" || template == "RecentSwatchPurchaseEmail");

            string utmSource;
            OnlySwatchesEmail email;
            switch(template)
            {
                case "RecentSwatchPurchaseEmail":
                    utmSource = UtmSource; // default
                    Website.Entities.Order order;
                    if (!orderNumber.HasValue)
                        throw new Exception("Missing order number");
                    using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
                    {
                        order = dc.Orders.Where(e => e.OrderNumber == orderNumber.Value).FirstOrDefault();
                    }

                    // for safety - should never hit unless order is purged
                    if (order == null)
                        throw new Exception("Cannot find order.");

                    email = new RecentSwatchPurchaseEmail(order, purchasedProductsInfo, emailAddress);
                    break;

                case "OnlySwatchesEmail":
                    email = new OnlySwatchesEmail(purchasedProductsInfo, emailAddress);
                    utmSource = UtmSource; // default
                    break;

                default:
                    throw new Exception("Unknown template name for swatch campaign.");
            }

            // utm_source will be one of:  abcart, abcart-ltns
            // utm_content will be one of: product, logo, unsubscribe

            // add tracking codes to all the product links

            var productClickTracking = MakeGoogleTrackingParameters(utmSource, "product");
            purchasedProductsInfo.ForEach(e => e.PageUrl += string.Format("?{0}", productClickTracking));

            email.IncludeCoupon = includeCoupon;
            email.StoreName = Store.FriendlyName;
            email.StoreLink = string.Format("https://www.{0}?{1}", Store.Domain, MakeGoogleTrackingParameters(utmSource, "logo"));
            email.StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", Store.StoreKey);
            email.UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=os&{2}", Store.Domain, MakeUnsubscribeToken(customerID, campaignID, Kind), MakeGoogleTrackingParameters(utmSource, "unsubscribe"));
            email.MyPurchasesUrl = string.Format("https://www.{0}/MyPurchases.aspx?id={1}&{2}", Store.Domain, MakeCustomerIDToken(customerID), MakeGoogleTrackingParameters(utmSource, "my-purchases"));
            email.FindSimilarUrlTemplate = string.Format("https://www.{0}/Search.aspx?SearchTerm=like:@@SKU@@&{1}", Store.Domain, MakeGoogleTrackingParameters(utmSource, "similar-products"));
            Store.SendEmail(email);
        }


        /// <summary>
        /// Wake up on trigger.
        /// </summary>
        /// <remarks>
        /// This input record is read only from SQL.
        /// </remarks>
        public override void WakeUp(TicklerCampaign record)
        {
            try
            {
                var data = Deserialize(record.JsonData);

                //  safety check, should never hit this
                if (record.TerminatedOn.HasValue || record.EmailedOn.HasValue || record.Status != TicklerCampaignStatus.Running.DescriptionAttr())
                {
                    // do a cancel to tidy up the record so won't happen again
                    CancelCampaign(record.CampaignID);
                    return;
                }

                // if unsubscribed EVER from any tickler campaign (this one or otherwise) of this kind,
                // then error on side of not being annoying and bail.

                if (IsUnsubscribedFromCampaignsOfThisKind(record.CustomerID))
                {
                    CancelCampaign(record.CampaignID);
                    return;
                }

                List<int> purchasedProductsVariantList = data.PurchasedProducts;

                var purchasedProductsInfo = GetProductInfo(purchasedProductsVariantList);

                // make sure we still have something since deleted etc pruned out

                if (purchasedProductsInfo.Count() == 0)
                {
                    CancelCampaign(record.CampaignID);
                    return;
                }

                string template = data.Template ?? "OnlySwatchesEmail";
                bool includeCoupon = true;
                int? orderNumber = null;

                // see if on-fly switch name and exclude coupon
                if (template == "OnlySwatchesNoCouponEmail")
                {
                    template = "OnlySwatchesEmail";
                    includeCoupon = false;
                }

                if (template == "RecentSwatchPurchaseEmail")
                {
                    includeCoupon = false;
                    orderNumber = int.Parse(data.Reference);
                }

                SendEmail(template, record.CustomerID, record.CampaignID, purchasedProductsInfo, includeCoupon, orderNumber);

                CompleteCampaign(record.CampaignID);

            }
            catch (Exception Ex)
            {
                LogException(Ex);
                AbortCampaign(record.CampaignID);
            }
        }

        private OnlySwatchesCampaignData Deserialize(string json)
        {
            return Deserialize<OnlySwatchesCampaignData>(json);
        }

        public bool NewOrderNotification(int customerID, int orderNumber)
        {
            try
            {
                var email = GetCustomerEmailAddress(customerID);

                if (email == null)
                {
                    Debug.WriteLine(string.Format("Missing email address for customer {0}, skipping campaign.", customerID));
                    return false;
                }

                if (Store.IsEmailSuppressed(email))
                {
                    Debug.WriteLine(string.Format("Email for customer {0} is suppressed, skipping campaign.", customerID));
                    return false;
                }

                // series of checks to ensure not annoying

                //if (IsAnyOtherRunningCampaignsOfThisKind(customerID))
                //{
                //    Debug.WriteLine(string.Format("Already running only swatches campaign for customer {0}, skipping.", customerID));
                //    return false;
                //}


                // see if this customer has unsubscribed from ANY prior only swatches campaign.
                if (IsUnsubscribedFromCampaignsOfThisKind(customerID))
                {
                    Debug.WriteLine(string.Format("Customer {0} has unsubscribed from only swatches campaigns, skipping.", customerID));
                    return false;
                }

                DateTime triggerOn = DateTime.Now;
#if !DEBUG                    
                triggerOn = triggerOn.AddDays(12);
#endif

                bool isStaged = false;

                var purchasedProducts = new List<int>(); // list of variants (could easily be to default variant, won't matter)

                using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
                {
                    // items in this order, see if any swatches
                    var allItems = dc.Orders_ShoppingCarts.Where(e => e.OrderNumber == orderNumber && e.CustomerID == customerID).Select(e => new {e.ProductID, e.VariantID, e.OrderedProductVariantName }).ToList();
                    purchasedProducts.AddRange(allItems.Where(e => string.Equals(e.OrderedProductVariantName, "Swatch", StringComparison.OrdinalIgnoreCase)).Select(e => e.VariantID));
                }

                if (purchasedProducts.Count() == 0)
                {
                    Debug.WriteLine(string.Format("Customer {0} doesn't have swatches to list, skipping.", customerID));
                    return false;
                }

                // make sure we still have something after fresh lookup since deleted etc pruned out
                var purchasedProductsInfo = GetProductInfo(purchasedProducts);
                if (purchasedProductsInfo.Count() == 0)
                {
                    Debug.WriteLine(string.Format("Customer {0} doesn't have swatches to list, skipping.", customerID));
                    return false;
                }

                var data = new OnlySwatchesCampaignData()
                {
                    CustomerID = customerID,
                    Template = "RecentSwatchPurchaseEmail",
                    PurchasedProducts = purchasedProducts,
                    Reference = orderNumber.ToString()
                };

                return CreateCampaign(customerID, triggerOn, data, isStaged);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
                return false;
            }
        }

        public bool CreateCampaign(int customerID, string template = null, DateTime? triggerOnTime = null, int? minDaysSinceLastEmail = null)
        {

            try
            {
                var email = GetCustomerEmailAddress(customerID);

                if (email == null)
                {
                    Debug.WriteLine(string.Format("Missing email address for customer {0}, skipping campaign.", customerID));
                    return false;
                }

                if (Store.IsEmailSuppressed(email))
                {
                    Debug.WriteLine(string.Format("Email for customer {0} is suppressed, skipping campaign.", customerID));
                    return false;
                }

                // series of checks to ensure not annoying

                if (IsAnyOtherRunningCampaignsOfThisKind(customerID))
                {
                    Debug.WriteLine(string.Format("Already running only swatches campaign for customer {0}, skipping.", customerID));
                    return false;
                }


                // see if this customer has unsubscribed from ANY prior only swatches campaign.
                if (IsUnsubscribedFromCampaignsOfThisKind(customerID))
                {
                    Debug.WriteLine(string.Format("Customer {0} has unsubscribed from only swatches campaigns, skipping.", customerID));
                    return false;
                }

                DateTime triggerOn = triggerOnTime ?? DateTime.Now; // present caller should always have trigger time set so spaces out
                bool isStaged = true;

                if (minDaysSinceLastEmail.HasValue)
                {
                    // if null, then never yet sent anything of this kind
                    var howLongSinceSameKindOfCampaignEmailSent = ElaspedTimeSinceSameKindOfCampaign(customerID);

                    if (howLongSinceSameKindOfCampaignEmailSent.HasValue && howLongSinceSameKindOfCampaignEmailSent.Value < TimeSpan.FromDays(minDaysSinceLastEmail.Value))
                    {
                        // let's not be annoying, skip
                        Debug.WriteLine(string.Format("Customer {0} has been emailed within minimum range, skipping.", customerID));
                        return false;
                    }
                }


                Debug.Assert(template == null || (template == "OnlySwatchesEmail"));

                var purchasedProducts = new List<int>(); // list of variants (could easily be to default variant, won't matter)

                using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
                {
                    // list of orders, most recent first
                    var orders = dc.Orders.Where(e => e.CustomerID == customerID).OrderByDescending(e => e.OrderDate).Select(e => new { e.OrderNumber, e.OrderDate }).ToList();

                    // list of everything ever ordered
                    var allItems = dc.Orders_ShoppingCarts.Where(e => e.CustomerID == customerID).Select(e => new { e.OrderNumber, e.ProductID, e.VariantID, e.OrderedProductVariantName }).ToList();

                    // go through list, most recent first, keep looping back through orders until yardage or end of list,
                    // keeping a list of variants
                    foreach(var order in orders)
                    {
                        var orderItems = allItems.Where(e => e.OrderNumber == order.OrderNumber).ToList();
                        var hasYardage = orderItems.Where(e => !string.Equals(e.OrderedProductVariantName, "Swatch", StringComparison.OrdinalIgnoreCase)).Count() > 0;
                        if (hasYardage)
                            break;

                        // should be all swatches at this point, add to running list
                        // should not really need to test for swatch again, but doing it for extra safety
                        purchasedProducts.AddRange(orderItems.Where(e => string.Equals(e.OrderedProductVariantName, "Swatch", StringComparison.OrdinalIgnoreCase)).Select(e => e.VariantID));

                        // cap it off
                        if (purchasedProducts.Count() >= 20)
                            break;
                    }
                }

                // the list will have most recent swatch orders at the front

                if (purchasedProducts.Count() == 0)
                {
                    Debug.WriteLine(string.Format("Customer {0} doesn't have swatches to list, skipping.", customerID));
                    return false;
                }

                // make sure we still have something after fresh lookup since deleted etc pruned out
                var purchasedProductsInfo = GetProductInfo(purchasedProducts);
                if (purchasedProductsInfo.Count() == 0)
                {
                    Debug.WriteLine(string.Format("Customer {0} doesn't have swatches to list, skipping.", customerID));
                    return false;
                }

                var data = new OnlySwatchesCampaignData()
                {
                    CustomerID = customerID,
                    Template = template,
                    PurchasedProducts = purchasedProducts,
                };

                return CreateCampaign(customerID, triggerOn, data, isStaged);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                LogException(Ex);
                return false;
            }
        }
    }
}
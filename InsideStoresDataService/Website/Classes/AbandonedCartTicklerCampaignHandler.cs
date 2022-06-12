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

    /// <summary>
    /// Definition for Json data stored along with this record kind.
    /// </summary>
    public class AbandonedCartCampaignData
    {
        public int CustomerID { get; set; }

        /// <summary>
        /// The list of variants found in customer's shopping cart at time of mailing.
        /// </summary>
        /// <remarks>
        /// Will be null until mailed.
        /// </remarks>
        public List<int> CartVariants { get; set; }

        /// <summary>
        /// The list of variants found in customer's favorites at time of mailing.
        /// </summary>
        /// <remarks>
        /// Will be null until mailed.
        /// </remarks>
        public List<int> FavoriteVariants { get; set; }

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



    public class AbandonedCartTicklerCampaignHandler : TicklerCampaignHandler
    {



        /// <summary>
        /// Google tracking. Campaign is always ticker. But will track this source.
        /// </summary>
        public override string UtmSource
        {
            get
            {
                return "abcart";
            }
        }


        public AbandonedCartTicklerCampaignHandler(IWebStore store, ITicklerCampaignsManager manager) :
            base(store, manager)
        {

        }

        public override TicklerCampaignKind Kind
        {
            get
            {
                return TicklerCampaignKind.AbandonedCart;
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
        /// <param name="cartProductsInfo">Must not be null.</param>
        /// <param name="favoriteProductsInfo">Must not be null.</param>
        private void SendEmail(string template, int customerID, int campaignID, List<TicklerCampaignHandler.ProductInfo> cartProductsInfo, List<TicklerCampaignHandler.ProductInfo> favoriteProductsInfo)
        {

            var emailAddress = GetCustomerEmailAddress(customerID);
            if (emailAddress == null)
                throw new Exception(string.Format("Missing email address for customerID: {0}", customerID));

//#if DEBUG
//            emailAddress = "peterav@pcdynamics.com";
//#endif
            Debug.Assert(template == "AbandonedCartEmail" || template == "LongTimeNoSeeEmail");

            string utmSource;
            AbandonedCartEmail email;
            switch(template)
            {
                case "AbandonedCartEmail":
                    email = new AbandonedCartEmail(cartProductsInfo, favoriteProductsInfo, emailAddress);
                    utmSource = UtmSource; // default
                    break;

                case "LongTimeNoSeeEmail":
                    email = new LongTimeNoSeeEmail(cartProductsInfo, favoriteProductsInfo, emailAddress);
                    utmSource = UtmSource + "-ltns"; // long time no see
                    break;

                default:
                    throw new Exception("Unknown template name for cart abandonment campaign.");
            }

            // utm_source will be one of:  abcart, abcart-ltns
            // utm_content will be one of: product, logo, unsubscribe

            // add tracking codes to all the product links

            var productClickTracking = MakeGoogleTrackingParameters(utmSource, "product");
            cartProductsInfo.ForEach(e => e.PageUrl += string.Format("?{0}", productClickTracking));
            favoriteProductsInfo.ForEach(e => e.PageUrl += string.Format("?{0}", productClickTracking));

            email.StoreName = Store.FriendlyName;
            email.StoreLink = string.Format("https://www.{0}?{1}", Store.Domain, MakeGoogleTrackingParameters(utmSource, "logo"));
            email.StoreLogoUrl = string.Format("http://image01.insidefabric.com/images/{0}Logo.png", Store.StoreKey); // works for any store, not just IF
            email.UnsubscribeUrl = string.Format("https://www.{0}/Unsubscribe.aspx?t={1}&k=ac&{2}", Store.Domain, MakeUnsubscribeToken(customerID, campaignID, Kind), MakeGoogleTrackingParameters(utmSource, "unsubscribe"));
            email.MyFavoritesUrl = string.Format("https://www.{0}/MyFavorites.aspx?id={1}&{2}", Store.Domain, MakeCustomerIDToken(customerID), MakeGoogleTrackingParameters(utmSource, "my-favorites"));

            if (Store.StoreKey != StoreKeys.InsideAvenue)
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

                // get separate lists for what's in cart vs favorites
                List<int> cartProductsVariantList = GetCartProductsVariantList(record.CustomerID);
                List<int> favoriteProductsVariantList = GetFavoritesProductVariantList(record.CustomerID);

                var cartProductsInfo = GetProductInfo(cartProductsVariantList);
                var favoriteProductsInfo = GetProductInfo(favoriteProductsVariantList);

                favoriteProductsVariantList.Shuffle();
                favoriteProductsVariantList = favoriteProductsVariantList.Take(30).ToList();

                // make sure we still have something...customer may have deleted or purchased, or products discontinued

                if (cartProductsInfo.Count() == 0 && favoriteProductsInfo.Count() == 0)
                {
                    CancelCampaign(record.CampaignID);
                    return;
                }

                Func<List<int>, List<int>, bool> isIdenticalProductSet = (a1, a2) =>
                    {
                        if (a1 == null || a2 == null)
                            return false;

                        if (a1.Count() != a2.Count())
                            return false;

                        foreach(var val in a1)
                        {
                            if (!a2.Contains(val))
                                return false;
                        }

                        return true;
                    };

                // compare to whatever we may have sent last and ensure not exactly the same
                var priorCampaignRecord = GetMostRecentMailedCampaign(record.CustomerID);
                if (priorCampaignRecord != null)
                {
                    var priorData = Deserialize(priorCampaignRecord.JsonData);
                    if (isIdenticalProductSet(priorData.CartVariants, cartProductsVariantList) && isIdenticalProductSet(priorData.FavoriteVariants, favoriteProductsVariantList))
                    {
                        CancelCampaign(record.CampaignID);
                        return;
                    }
                }

                string template = data.Template ?? "AbandonedCartEmail";

                SendEmail(template, record.CustomerID, record.CampaignID, cartProductsInfo, favoriteProductsInfo);

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    var updateRecord = dc.TicklerCampaigns.Where(e => e.CampaignID == record.CampaignID).FirstOrDefault();
                    data.CartVariants = cartProductsVariantList;
                    data.FavoriteVariants = favoriteProductsVariantList;
                    updateRecord.JsonData = Serialize(data);
                    dc.SubmitChanges();
                }

                CompleteCampaign(record.CampaignID);

            }
            catch (Exception Ex)
            {
                LogException(Ex);
                AbortCampaign(record.CampaignID);
            }
        }

        private AbandonedCartCampaignData Deserialize(string json)
        {
            return Deserialize<AbandonedCartCampaignData>(json);
        }

        public bool CreateCampaign(int customerID, string template = null, DateTime? triggerOnTime=null, int? minDaysSinceLastEmail=null)
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
                    // if one waiting (not mailed yet), then bump the trigger time to reset typical elapsed time before sending
                    // if they keep coming back within short timeline, no need to keep sending...wait until they stop,
                    // then that is the baseline for what might be left behind

                    using (var dc = new AspStoreDataContext(Store.ConnectionString))
                    {
                        var updateRecord = dc.TicklerCampaigns.Where(e => e.CustomerID == customerID && e.Kind == Kind.DescriptionAttr() && e.Status == TicklerCampaignStatus.Running.DescriptionAttr() && e.EmailedOn != null).OrderByDescending(e => e.TriggerOn).FirstOrDefault();
                        if (updateRecord != null)
                        {
                            updateRecord.TriggerOn = DateTime.Now.AddMinutes(Store.CartAbandonmentEmailNotificationDelayMinutes); 
                            dc.SubmitChanges();
                        }
                    }
                    return false;
                }


                if (!HasQualifyingCartProducts(customerID))
                {
                    Debug.WriteLine(string.Format("Nothing in shopping cart for customer {0}, skipping campaign.", customerID));
                    return false;
                }


                // see if this customer has unsubscribed from ANY prior abandonment campaign.
                if (IsUnsubscribedFromCampaignsOfThisKind(customerID))
                {
                    Debug.WriteLine(string.Format("Customer {0} has unsubscribed from abandoned cart campaigns, skipping.", customerID));
                    return false;
                }

                DateTime triggerOn;
                bool isStaged = false;

                if (template == "LongTimeNoSeeEmail" && triggerOnTime.HasValue && minDaysSinceLastEmail.HasValue)
                {
                    triggerOn = triggerOnTime.Value;
                    // if null, then never yet sent anything of this kind
                    var howLongSinceSameKindOfCampaignEmailSent = ElaspedTimeSinceSameKindOfCampaign(customerID);

                    if (howLongSinceSameKindOfCampaignEmailSent.HasValue && howLongSinceSameKindOfCampaignEmailSent.Value < TimeSpan.FromDays(minDaysSinceLastEmail.Value))
                    {
                        // let's not be annoying, skip
                        Debug.WriteLine(string.Format("Customer {0} has been emailed within minimum range, skipping.", customerID));
                        return false;
                    }
                    isStaged = true;
                }
                else
                {
                    triggerOn = DateTime.Now.AddMinutes(Store.CartAbandonmentEmailNotificationDelayMinutes);

                    // if null, then never yet sent anything of this kind
                    var howLongSinceSameKindOfCampaignEmailSent = ElaspedTimeSinceSameKindOfCampaign(customerID);

                    if (howLongSinceSameKindOfCampaignEmailSent.HasValue && howLongSinceSameKindOfCampaignEmailSent.Value < TimeSpan.FromDays(2))
                    {
                        // let's not be annoying, so bump the trigger a bunch
                        triggerOn = DateTime.Now.AddDays(2);
                    }
                }
                Debug.Assert(template == null || (template == "AbandonedCartEmail" || template == "LongTimeNoSeeEmail"));
                var data = new AbandonedCartCampaignData()
                {
                    CustomerID = customerID,
                    Template = template
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
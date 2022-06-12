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
using System.Text;

namespace Website
{

    public abstract class TicklerCampaignHandler : ITicklerCampaignHandler
    {
        public class ParsedUnsubscribeToken
        {
            public bool IsValid { get; set; }
            public int CustomerID { get; set; }
            public int CampaignID { get; set; }
        }

        public class ProductInfo
        {
            public class Compare : IEqualityComparer<TicklerCampaignHandler.ProductInfo>
            {
                public bool Equals(TicklerCampaignHandler.ProductInfo x, TicklerCampaignHandler.ProductInfo y)
                {
                    return x.ProductID == y.ProductID;
                }

                public int GetHashCode(TicklerCampaignHandler.ProductInfo p)
                {
                    return p.ProductID.GetHashCode();
                }
            }

            public int ProductID { get; set; }
            public string SKU { get; set; }
            public string Name { get; set; }
            //public decimal OurPrice { get; set; }
            //public decimal MSRP { get; set; }
            //public bool IsOnSale { get; set; }
            public string PageUrl { get; set; }
            public string ImageUrl { get; set; }
            public string ProductGroup { get; set; }
        }



        protected IWebStore Store { get; private set; }
        protected ITicklerCampaignsManager Manager {get; private set; }

        public abstract TicklerCampaignKind Kind {get;}

        /// <summary>
        /// Google analytics tracking for utm_medium
        /// </summary>
        public virtual string UtmMedium
        {
            get
            {
                return "email";
            }
        }

        public virtual string UtmCampaign
        {
            get
            {
                return "tickler";
            }
        }

        public virtual string UtmSource
        {
            get
            {
                // likely want to override this in subclass
                return ((int)Kind).ToString();
            }
        }

        public virtual string UtmContent
        {
            get
            {
                // optional, typically supplied as method parameter rather than here.
                return null;
            }
        }


        public TicklerCampaignHandler(IWebStore store, ITicklerCampaignsManager manager)
        {
            this.Store = store;
            this.Manager = manager;
        }

        protected bool CreateCampaign(int customerID, DateTime triggerOn, object data, bool isStaged=false)
        {
            return Manager.CreateCampaign(customerID, Kind, triggerOn, data, isStaged);
        }

        protected string MakeGoogleTrackingParameters(string utmSource=null, string utmContent = null)
        {
            var sb = new StringBuilder(100);
            sb.AppendFormat("utm_medium={0}&utm_campaign={1}", UtmMedium, UtmCampaign);

            if (!string.IsNullOrWhiteSpace(utmSource))
                sb.AppendFormat("&utm_source={0}", utmSource);
            else if (!string.IsNullOrWhiteSpace(UtmSource))
                sb.AppendFormat("&utm_source={0}", UtmSource);

            // if some kind of specific content is specified, append that too

            if (!string.IsNullOrWhiteSpace(utmContent))
                sb.AppendFormat("&utm_content={0}", utmContent);
            else if (!string.IsNullOrWhiteSpace(UtmContent))
                sb.AppendFormat("&utm_content={0}", UtmContent);

            return sb.ToString();
        }


        public bool IsUnsubscribedFromCampaignsOfThisKind(int customerID)
        {
            return Manager.IsUnsubscribed(customerID, Kind);
        }

        /// <summary>
        /// Wake up on trigger. Generally would figure this to be overriden.
        /// </summary>
        /// <remarks>
        /// Default processing here simply marks the record as completed successfully.
        /// </remarks>
        public virtual void WakeUp(TicklerCampaign record)
        {
            CompleteCampaign(record.CampaignID);
        }

        /// <summary>
        /// Customer asked to unsubscribe. Implicitly cancel if still running.
        /// </summary>
        /// <param name="campaignID"></param>
        public void UnsubscribeCampaign(int campaignID)
        {
            Manager.UnsubscribeCampaign(campaignID);
        }

        /// <summary>
        /// Mark campaign as having completed normally (success).
        /// </summary>
        /// <remarks>
        /// A campaign that naturally ends early because, for example, customer
        /// purchased and cart is empty...that counts for being completed, not cancelled.
        /// </remarks>
        /// <param name="campaignID"></param>
        public void CompleteCampaign(int campaignID)
        {
            Manager.CompleteCampaign(campaignID);
        }

        /// <summary>
        /// Something fatal - like exception. Kill campaign, don't send anything more.
        /// </summary>
        /// <param name="campaignID"></param>
        public void AbortCampaign(int campaignID)
        {
            Manager.AbortCampaign(campaignID);
        }

        /// <summary>
        /// Cancel running campaign, but does not adjust unsubscribed.
        /// </summary>
        /// <param name="campaignID"></param>
        public void CancelCampaign(int campaignID)
        {
            Manager.CancelCampaign(campaignID);
        }

        protected bool IsAnyOtherRunningCampaignsOfThisKind(int customerID)
        {
            return Manager.IsAnyOtherRunningCampaigns(customerID, Kind);
        }

        protected string GetCustomerEmailAddress(int customerID)
        {
            return Manager.GetCustomerEmailAddress(customerID);
        }

        protected T Deserialize<T>(string json)
        {
            return Manager.Deserialize<T>(json);
        }

        protected string Serialize(object data)
        {
            return Manager.Serialize(data);
        }

        protected TimeSpan? ElaspedTimeSinceSameKindOfCampaign(int customerID)
        {
            return Manager.ElaspedTimeSinceSameKindOfCampaign(customerID, Kind);
        }

        protected TicklerCampaign GetMostRecentMailedCampaign(int customerID)
        {
            return Manager.GetMostRecentMailedCampaign(customerID, Kind);
        }

        protected TicklerCampaign GetMostRecentRunningCampaign(int customerID)
        {
            return Manager.GetMostRecentRunningCampaign(customerID, Kind);
        }

        protected void LogException(Exception Ex)
        {
            var ev = new WebsiteRequestErrorEvent("Unhandled Exception in TicklerCampaignHandler", this, WebsiteEventCode.UnhandledException, Ex);
            ev.Raise();
        }


        /// <summary>
        /// Create a blind string token to be used for unsubscribes.
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        public static string MakeUnsubscribeToken(int customerID, int campaignID, TicklerCampaignKind kind)
        {
            // the extra salt is just to mix things up so people on outside cannot reverse back the hash
            var hash = string.Format("{0:D10}{1:D10}{2}extrasalt", customerID, campaignID, kind.DescriptionAttr()).SHA256Digest();

            return string.Format("{0:D10}{1:D10}{2}", customerID, campaignID, hash);
        }


        /// <summary>
        /// Parse and validate the unsubscribe token. All parsing must use this entry point to maintain consistency.
        /// </summary>
        /// <remarks>
        /// Does not make any effort to see if campaign matches for customer. Only checks the integrity.
        /// </remarks>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ParsedUnsubscribeToken ParseUnsubscribeToken(string token, TicklerCampaignKind kind)
        {
            var result = new ParsedUnsubscribeToken
            {
                IsValid = false,
                CustomerID = 0,
                CampaignID = 0
            };

            try
            {
                var sCustomerID = token.Substring(0, 10);
                var sCamapaignID = token.Substring(10, 10);
                var hash = token.Substring(20);

                var computedHash = string.Format("{0:D10}{1:D10}{2}extrasalt", int.Parse(sCustomerID), int.Parse(sCamapaignID), kind.DescriptionAttr()).SHA256Digest();

                if (hash == computedHash)
                {
                    result.IsValid = true;
                    result.CustomerID = int.Parse(sCustomerID);
                    result.CampaignID = int.Parse(sCamapaignID);
                }
            }
            catch
            {

            }
            return result;
        }

        /// <summary>
        /// Create a blind string token to be used for my purchases.
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static string MakeCustomerIDToken(int customerID)
        {
            // the extra salt is just to mix things up so people on outside cannot reverse back the hash
            var hash = string.Format("{0:D10}extrasalt", customerID).SHA256Digest();

            return string.Format("{0:D10}{1}", customerID, hash);
        }


        public int? ParseCustomerIDToken(string token)
        {
            // NOTE - this logic must match website MyPurchases.aspx.cs

            try
            {
                var sCustomerID = token.Substring(0, 10);
                var hash = token.Substring(10);

                var computedHash = string.Format("{0:D10}extrasalt", int.Parse(sCustomerID)).SHA256Digest();

                if (hash == computedHash)
                    return int.Parse(sCustomerID);

                return null;
            }
            catch
            {

            }
            return null;
        }



        protected bool HasQualifyingCartProducts(int customerID)
        {
            // not generally called on wakeup since would recreate datasets all over again,
            // do the final check for empty lists directly within wakeup.

            // since only care about ANY...can return true as soon as find a list with some products.
            List<int> cartProductsVariantList = GetCartProductsVariantList(customerID);
            var cartProductsInfo = GetProductInfo(cartProductsVariantList);

            if (cartProductsInfo.Count() > 0)
                return true;

            List<int> favoriteProductsVariantList = GetFavoritesProductVariantList(customerID);
          
            // need to account for if was discontinued, deleted, etc.
            var favoriteProductsInfo = GetProductInfo(favoriteProductsVariantList);

            if (favoriteProductsInfo.Count() > 0)
                return true;

            return false;
        }


        protected List<int> GetProductVariantList(int customerID, int cartType)
        {
            // cartType is 0:cart, 1:bookmarks

            // note: does not filter favorites to exclude purchases or still in cart

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                // carttype: cart:0, bookmarks:1
                var listVariants = dc.ShoppingCarts.Where(e => e.CustomerID == customerID && e.CartType == cartType).Select(e => e.VariantID).ToList();
                return listVariants;
            }
        }

        protected List<int> GetCartProductsVariantList(int customerID)
        {
            // items in actual cart
            return GetProductVariantList(customerID, cartType: 0);
        }

        protected List<int> GetFavoritesProductVariantList(int customerID)
        {
            // bookmarked items
            var listVariants = GetProductVariantList(customerID, cartType: 1);

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                // since these are bookmarks, do not include any that are presently in real cart, or were already purchased

                // carttype:0 for presently in cart
                var listCartProductID = dc.ShoppingCarts.Where(e => e.CustomerID == customerID && e.CartType == 0).Select(e => e.ProductID).ToList();

                var listPurchasedProductID = dc.Orders_ShoppingCarts.Where(e => e.CustomerID == customerID && e.CartType == 0).Select(e => e.ProductID).ToList();

                var combinedProducts = new HashSet<int>(listCartProductID);
                listPurchasedProductID.ForEach(e => combinedProducts.Add(e));

                // since we're dealing with bookmarks, which are always the default variant, we now need to grab the related default variantID
                // for any found products

                var filterOut = dc.ProductVariants.Where(e => combinedProducts.ToList().Contains(e.ProductID) && e.IsDefault == 1).Select(e => e.VariantID).ToList();

                // need to prune favorites to not include any that are in cart now, or previously purchased
                listVariants.RemoveAll(e => filterOut.Contains(e));
            }

            return listVariants;
        }

        protected List<int> GetPurchasedProductVariantList(int customerID)
        {
            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {
                var listVariants = dc.Orders_ShoppingCarts.Where(e => e.CustomerID == customerID && e.CartType == 0).Select(e => e.VariantID).Distinct().ToList();
                return listVariants;
            }
        }

        /// <summary>
        /// Return a list of products previously purchased.
        /// </summary>
        /// <remarks>
        /// Distinct, in case purchased a non-default product.
        /// </remarks>
        /// <param name="customerID"></param>
        /// <returns></returns>
        protected List<TicklerCampaignHandler.ProductInfo> GetPurchasedProductInfo(int customerID)
        {
            var variants = GetPurchasedProductVariantList(customerID);
            return GetProductInfo(variants).Distinct(new TicklerCampaignHandler.ProductInfo.Compare()).ToList();
        }

        /// <summary>
        /// Given a list of variants, return a set of corresponding full information (records to display in email).
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        protected List<TicklerCampaignHandler.ProductInfo> GetProductInfo(List<int> productVariants)
        {
            var urlPrefix = string.Format("https://www.{0}", Store.Domain);

            using (var dc = new AspStoreDataContextReadOnly(Store.ConnectionString))
            {

                var rawList = (from pv in dc.ProductVariants
                               where productVariants.Contains(pv.VariantID)
                               join p in dc.Products on pv.ProductID equals p.ProductID
                               select new
                               {
                                   VariantID = pv.VariantID,
                                   ProductID = pv.ProductID,
                                   IsDiscontinued = p.ShowBuyButton == 0 || p.Deleted == 1,
                                   IsPublished = p.Published == 1 && pv.Published == 1,
                                   InStock = pv.Inventory > 0,
                                   SKU = p.SKU,
                                   Name = p.Name,
                                   //OurPrice = pv.Price,
                                   //SalePrice = pv.SalePrice,
                                   //MSRP = pv.MSRP,
                                   ProductGroup = p.ProductGroup,
                                   ProductUrl = urlPrefix + "/p-" + p.ProductID.ToString() + "-" + p.SEName + ".aspx",
                                   ProductImageUrl = pv.ImageFilenameOverride != null ? urlPrefix + "/images/variant/icon/" + pv.ImageFilenameOverride
                                   : p.ImageFilenameOverride == null ? urlPrefix + "/images/nopicture.gif" : urlPrefix + "/images/product/icon/" + p.ImageFilenameOverride,
                               }
                    ).ToList();

                // we use a two step process simply to add a little flexibility - speed is not an issue

                // this list is the specific set of data used by the Razor template
                // the difference between products/variants conceptually not a thing - all we care about is the list
                // of items to display.
                var finalList = (from p in rawList
                                 where !p.IsDiscontinued && p.IsPublished
                                 select new TicklerCampaignHandler.ProductInfo()
                                 {
                                     ProductID = p.ProductID,
                                     SKU = p.SKU, // base SKU, no suffix
                                     Name = p.Name,
                                     ProductGroup = p.ProductGroup,
                                     //OurPrice = p.SalePrice ?? p.OurPrice, // if sale price has a value, use it
                                     //MSRP = p.MSRP.GetValueOrDefault(),
                                     //IsOnSale = p.SalePrice.HasValue,
                                     PageUrl = p.ProductUrl,
                                     ImageUrl = p.ProductImageUrl
                                 }).ToList();

                return finalList;
            }

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class OnlySwatchesEmail : EmailTemplate
  {

    public List<TicklerCampaignHandler.ProductInfo> PurchasedProducts = new List<TicklerCampaignHandler.ProductInfo>();
    public string StoreName { get; set; }
    public string StoreLink { get; set; }
    public string StoreLogoUrl { get; set; }
    public string UnsubscribeUrl { get; set; }
    public string MyPurchasesUrl { get; set; }
    public string FindSimilarUrlTemplate { get; set; }
    public bool IncludeCoupon { get; set; }

    public OnlySwatchesEmail()
    {
      Subject = "About Your Swatch Purchases";
      From = "Inside Stores <customersupport@example.com>";
      UsePremailer = true;
    }

    public OnlySwatchesEmail(List<TicklerCampaignHandler.ProductInfo> purchasedProducts, string to = null)
        : this()
    {
      this.PurchasedProducts = purchasedProducts;
      this.To = to;
    }

  }

  public class RecentSwatchPurchaseEmail : OnlySwatchesEmail
  {
    public Website.Entities.Order Order { get; set; }
    public RecentSwatchPurchaseEmail(Website.Entities.Order order, List<TicklerCampaignHandler.ProductInfo> purchasedProducts, string to = null) :
        base(purchasedProducts, to)
    {
      this.Order = order;
    }
  }

}
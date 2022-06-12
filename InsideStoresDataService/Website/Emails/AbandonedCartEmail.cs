using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class AbandonedCartEmail : EmailTemplate
  {

    public List<TicklerCampaignHandler.ProductInfo> CartProducts = new List<TicklerCampaignHandler.ProductInfo>();
    public List<TicklerCampaignHandler.ProductInfo> FavoriteProducts = new List<TicklerCampaignHandler.ProductInfo>();
    public string StoreName { get; set; }
    public string StoreLink { get; set; }
    public string StoreLogoUrl { get; set; }
    public string UnsubscribeUrl { get; set; }
    public string MyFavoritesUrl { get; set; }

    public string FindSimilarUrlTemplate { get; set; }

    public AbandonedCartEmail()
    {
      Subject = "Did you leave something behind?";
      From = "Inside Stores <customersupport@example.com>";
      UsePremailer = true;
    }

    public AbandonedCartEmail(List<TicklerCampaignHandler.ProductInfo> cartProducts, List<TicklerCampaignHandler.ProductInfo> favoriteProducts, string to = null)
        : this()
    {
      this.CartProducts = cartProducts;
      this.FavoriteProducts = favoriteProducts;
      this.To = to;
    }

  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class LongTimeNoSeeEmail : AbandonedCartEmail
  {
    public LongTimeNoSeeEmail()
    {
      Subject = "Did you leave something behind?";
      From = "Inside Stores <customersupport@example.com>";
      UsePremailer = true;
    }

    public LongTimeNoSeeEmail(List<TicklerCampaignHandler.ProductInfo> cartProducts, List<TicklerCampaignHandler.ProductInfo> favoriteProducts, string to = null)
        : this()
    {
      this.CartProducts = cartProducts;
      this.FavoriteProducts = favoriteProducts;
      this.To = to;
    }

  }
}
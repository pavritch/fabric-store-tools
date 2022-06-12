using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class InStockNotificationEmail : EmailTemplate
  {
    public class ProductInfo
    {
      public string SKU { get; set; }
      public string Name { get; set; }
      public decimal OurPrice { get; set; }
      public string PageUrl { get; set; }
      public string ImageUrl { get; set; }
    }


    public List<ProductInfo> Products = new List<ProductInfo>();

    public InStockNotificationEmail()
    {
      Subject = "Requested products are now in stock!";
      From = "Inside Stores <customersupport@example.com>";
      UsePremailer = true;
    }

    public InStockNotificationEmail(List<ProductInfo> products, string to = null) : this()
    {
      this.Products = products;
      this.To = to;
    }
  }
}
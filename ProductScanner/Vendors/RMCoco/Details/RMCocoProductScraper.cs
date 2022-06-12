using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace RMCoco.Details
{
  public class RMCocoImageChecker : IImageChecker<RMCocoVendor>
  {
    public bool CheckImage(HttpWebResponse response)
    {
      // the placeholder image has contentlength = 119598
      return response.ContentLength != 119598;
    }
  }

  /*
  public class RMCocoProductScraper : ProductScraper<RMCocoVendor>
  {
      private readonly List<string> _soldAsEach = new List<string>
      {
          "Key Tassel",
          "Tieback",
          "Chair Tie",
          "Cushion Tassel",
          "Tieback Tassel",
          "Single Tieback",
          "Triple Cushion Tieback",
          "Beaded Tieback"
      }; 

      // search this list from top to bottom, since we can have both 'Tieback Tassel' and 'Tieback' as categories
      private readonly List<string> _trimCategories = new List<string>
      {
          "Bullion Fringe 4",
          "Bullion Fringe",
          "Bullion 4",
          "Bullion 6",
          "Brush Fringe W/Tassel",
          "Brush Fringe",
          "Brushed Fringe",
          "Key Tassel",
          "Furniture Fringe",
          "Tieback Tassel",
          "Tassel Fringe",
          "Chair Tie",
          "Lipcord 6MM",
          "Lipcord 12MM",
          "Lipcord",
          "Hand Tied Tassel Frng",
          "Fringe With Tassel",
          "Dec.Cord Wtih Lip",
          "Decorative Cord",
          "Looped Rouche",
          "Rope Tassel Fringe",
          "Tie Back Tassel",
          "Border",
          "Gimp",
          "Tieback"
      }; 

      private Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
      {
          { "RETAIL PRICE", ScanField.RetailPrice},
          { "WHOLESALE", ScanField.Cost},
          { "QTY ON HAND:", ScanField.StockCount},
          { "BOOK:", ScanField.Book},
          { "SWATCH:", ScanField.HasSwatch},
          { "EXCLUSIVE:", ScanField.IsExclusive},
          { "WIDTH:", ScanField.Width},
          { "VERTICAL REPEAT:", ScanField.VerticalRepeat},
          { "DURABILITY:", ScanField.Durability},
          { "CLEANING CODE:", ScanField.Cleaning},
          { "CONTENT:", ScanField.Content},
          { "FIRE RETARDANT:", ScanField.FlameRetardant},
          { "RAILROADED:", ScanField.Railroaded},
          { "HORIZONTAL REPEAT:", ScanField.HorizontalRepeat},
          { "FIBER CONTENT:", ScanField.Ignore},
          { "PASSES:", ScanField.Flammability},
          { "FIRE RETARDANCY:", ScanField.FireCode},
          { "FINISH:", ScanField.Finish},
          { "STATUS", ScanField.Status},
          { "RMCOCO EXCLUSIVE", ScanField.Ignore},
      }; 

      public RMCocoProductScraper(IPageFetcher<RMCocoVendor> pageFetcher) : base(pageFetcher) { }
      public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
      {
          var url = context.DetailUrl.AbsoluteUri;
          var mpn = url.Replace("https://rmcoco.com/product/", "");
          var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
          var productName = detailsPage.GetFieldValue(".cart-title");

          var mainCart = detailsPage.QuerySelector(".main-cart-cont");
          if (mainCart == null || mainCart.InnerText.ContainsIgnoreCase("Record not found"))
              return new List<ScanData>();

          if (!detailsPage.InnerText.ContainsIgnoreCase("Tessa Luu"))
          {
              PageFetcher.RemoveCachedFile(CacheFolder.Details, mpn);
              throw new AuthenticationException();
          }

          var product = new ScanData();
          product[ScanField.ItemNumber] = mpn.Replace("_", "-");
          product[ScanField.ManufacturerPartNumber] = mpn.Replace("_", "-");
          product[ScanField.ProductName] = productName;
          product[ScanField.Book] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_SearchDetailsControl1_lblBook");
          product.DetailUrl = new Uri(url);

          var crumbs = detailsPage.QuerySelector(".bread-crum").InnerText;
          if (crumbs.Contains("Trim")) product[ScanField.ProductGroup] = "Trim";
          else product[ScanField.ProductGroup] = "Fabric";

          var rows = detailsPage.QuerySelectorAll(".product-details .row").ToList();
          foreach (var row in rows)
          {
              var labelDiv = row.QuerySelectorAll(".column2").First();
              var valueDiv = row.QuerySelectorAll(".column2").Last();
              var field = _fields[labelDiv.InnerText];
              product[field] = valueDiv.InnerText;
          }

          product[ScanField.UnitOfMeasure] = product[ScanField.Cost].ContainsIgnoreCase("ea") ? "Each" : "Yard";
          product.Cost = product[ScanField.Cost].Replace(" per YD", "").Replace(" per yd", "").Replace(" per ea", "").Replace("$", "").ToDecimalSafe();

          //var imageSpan = detailsPage.QuerySelector("#ctl00_ContentPlaceHolder1_SearchDetailsControl1_lblImage img");
          //var imageUrl = imageSpan.Attributes["src"].Value.Trim().Replace("medium", "large");
          //if (imageUrl != "http://online.rmcoco.com/attachments/images/large/Montage/R600%20260.jpg")
          if (!detailsPage.QuerySelector(".prod-image").InnerHtml.ContainsIgnoreCase("coming-soon.png"))
              product.AddImage(new ScannedImage(ImageVariantType.Primary, 
                  string.Format("https://d7nosm0kku14h.cloudfront.net/attachments/images/large/{0}.jpg", mpn)));
          return new List<ScanData> { product };

          //var splitId = mpn.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);

          //var colorName = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_SearchDetailsControl1_lblColor");
          //var pattern = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_SearchDetailsControl1_lblPattern");
          //var unit = "Yard";
          //var category = string.Empty;
          //if (context.ScanData[ScanField.ProductGroup] == "Trim")
          //{
          //    var trimCategory = FindTrimCategory(colorName) ?? FindTrimCategory(pattern);
          //    if (trimCategory != null)
          //        category = trimCategory;

          //    // trim categories are often listed in the color name and pattern
          //    colorName = RemoveTrimCategories(colorName);
          //    pattern = RemoveTrimCategories(pattern);

          //    if (_soldAsEach.Contains(trimCategory))
          //    {
          //        unit = "Each";
          //    }
          //}

          //var product = new ScanData(context.ScanData);
          //product[ScanField.Category] = category;
          //product[ScanField.ColorName] = colorName;
          //product[ScanField.ColorNumber] = splitId.Last();
          //product[ScanField.PatternName] = pattern;
          //product[ScanField.PatternNumber] = splitId.First();
          //product[ScanField.UnitOfMeasure] = unit;


          //return new List<ScanData> {product};
      }

      private string RemoveTrimCategories(string field)
      {
          foreach (var s in _trimCategories)
          {
              field = field.Replace(s.ToUpper(), "");
          }
          return field;
      }

      private string FindTrimCategory(string field)
      {
          return _trimCategories.FirstOrDefault(s => field.Contains(s.ToUpper()));
      }
  }
  */
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Net.Http;

namespace DataFeedsWebsite.Controllers
{
  public class HomeController : Controller
  {

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Feeds()
    {
      return View();
    }

    public async Task<ActionResult> RegenerateAllFeeds(string password)
    {
      // used as just a trigger, do not care what the response is

      if (password != "passwordhere$02")
      {
        ViewBag.Result = "Invalid password. Access denied.";
        return View(ViewBag);
      }

      var storeDataServiceUrlRoot = ConfigurationManager.AppSettings["StoreDataServiceUrlRoot"];

      var url = string.Format("{0}/ProductFeeds/GenerateAllProductFeeds", storeDataServiceUrlRoot);

      using (var httpClient = new HttpClient())
      {
        await httpClient.GetStringAsync(url);
      }

      ViewBag.Result = "Begin generation...";
      return View(ViewBag);
    }

    public async Task<ActionResult> DownloadProductFeed(string storeKey, string feedKey, string feedFile)
    {
      try
      {
        var storeDataServiceUrlRoot = ConfigurationManager.AppSettings["StoreDataServiceUrlRoot"];

        if (!(string.Equals(Path.GetExtension(feedFile), ".txt", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(feedFile), ".zip", StringComparison.OrdinalIgnoreCase)))
          throw new Exception("Invalid filename. Does not end with txt or zip.");

        var url = string.Format("{0}/ProductFeeds/{1}/{2}/{3}", storeDataServiceUrlRoot, storeKey, feedKey, Path.GetExtension(feedFile).Substring(1));

        using (var httpClient = new HttpClient())
        {
          var response = await httpClient.GetAsync(url);

          var httpData = await response.Content.ReadAsByteArrayAsync();

          // this is what will come back

          // Header X-AspNetMvc-Version = 3.0
          // Header Content-Disposition = attachment; filename=InsideFabricGoogleProductFeed.txt
          // Header Connection = Close
          // Header Content-Length = 84565184
          // Header Cache-Control = private
          // Header Content-Type = text/plain
          // Header Date = Wed, 02 May 2012 02:38:49 GMT
          // Header Server = ASP.NET Development Server/10.0.0.0
          // Header X-AspNet-Version = 4.0.30319

          var contentType = response.Content.Headers.ContentType.MediaType;

          //Debug.WriteLine(string.Format("Size of feed data: {0:N0}", httpData.Length));
          //Debug.WriteLine(string.Format("Content-Type: {0}", contentType));

          return new FileContentResult(httpData, contentType)
          {
            FileDownloadName = feedFile
          };

        }

      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.ToString());
        return new HttpNotFoundResult();
      }
    }

  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Ionic.Zip;

namespace Website
{
  /// <summary>
  /// Singleton global manager for anything releated to robots.
  /// </summary>
  /// <remarks>
  /// The robots controller calls this class to do all the work.
  /// </remarks>
  public class ShareASaleManager : IShareASaleManager
  {

    // productId,Name,Merchant Id,Organization,Link,Thumbnail,Big Image,Price,Retail Price,Category,Sub Category,Description,
    // Custom 1,Custom 2,Custom 3,Custom 4,Custom 5,Last Updated,Status,Manufacture,Part Number,Merchant Category,Merchant Sub Category,
    // Short Description,ISBN,UPC,SKU,Cross Sell,Merchant Group,Merchant Sub Group,Compatiable With,Compare To,Quantity Discount,
    // Best Seller,Add to Cart URL,Reviews URL,Option 1,Option 2,Option 3,Options 4,Option 5,ReservedForFutureUse,ReservedForFutureUse,
    // ReservedForFutureUse,ReservedForFutureUse,ReservedForFutureUse,ReservedForFutureUse,ReservedForFutureUse,ReservedForFutureUse,
    // ReservedForFutureUse,ReservedForFutureUse,WWW,Program Category,Status,Commission Text,Sale Comm,Lead Comm,Hit Comm,Cookie Length,
    // Auto Approve,Auto Deposit,Datafeed Items,Epc 7 Day,EPC 30 Day,Reversal Rate 7 Day,Reversal Rate 30 Day,Ave Sale 7 Day,Ave Sale 30 Day,
    // Ave Comm 7 Day,Ave Comm 30 Day,Powerrank Top 100 


    // 537001877| {0-productId} (**reduced**)
    // Lane Media Stand, White/Natural|  {1-Name}  (**reduced**)
    // 37430|   {2-Merchant Id}
    // One Kings Lane|   {3-Organization}
    // http://www.shareasale.com/m-pr.cfm?merchantID=37430&userID=YOURUSERID&productID=537001877|   {4-Link} (**reduced**)
    // http://okl.scene7.com/is/image/OKL/Product_HTI10000_Image_1?$small$|   {5-Thumbnail} 
    // http://okl.scene7.com/is/image/OKL/Product_HTI10000_Image_1?$large$|   {6-Big Image}(**reduced**)
    // 449.00|   {7-Price} (**reduced**)
    // 2299.00|   {8-Retail Price}
    // Home/Family|   {9-Category}
    // Furniture|   {10-Sub Category}
    // Drawing inspiration from iconic Chinese platforms, this round-legged media stand blends classic Asian elements with modern polish. Crafted from elm with a contrasting white trim.|   {11-Description}
    // |||||
    // 2015-04-18 02:16:40.967|   {17-Last Updated}
    // ||  {0-Status} {0-Manufacturer}
    // 1660139|   {0-Part Number}
    // Media & TV|   {0-Merchant Category}
    // |    {Merchant Sub Category}
    // Drawing inspiration from iconic Chinese platforms, this round-legged media stand blends classic Asian elements with modern polish. Crafted from elm with a contrasting white trim.|   {0-Short Description}
    // ||  {ISBN}  {UPC}
    // 1237360|   {0-SKU}
    // ||||||   {Cross Sell,Merchant Group,Merchant Sub Group,Compatiable With,Compare To,Quantity Discount, Best Seller,Add to Cart URL}
    // ---- seeems to not match up so clearly from here-----
    // 0| {0-Reviews}
    // http://www.shareasale.com/m-pr.cfm?merchantID=37430&userID=YOURUSERID&atc=1&productID=537001877|  
    // ||||||
    // http://www.shareasale.com/m-pr.cfm?merchantID=37430&userID=YOURUSERID&mobile=1&productID=537001877|  
    // ||||||||

    // images:
    //  small: 148x101
    //  medium: 298x303
    //  large: 459x313
    //  unstated: seems to be max dimension 320, the other is whatever

    #region Constants
    // 1668194

#if false
        // these are the original values from Tessa's account
        private const string AFFILIATE_ID = "333";
        private const string API_TOKEN = "FN1sJ1ra7KqTPuta";
        private const string API_SECRET = "ddddddd";

        private const string FTP_USERNAME = "dddd";
        private const string FTP_PASSWORD = "passwordhere$65";

        private const int ONEKINGSLANE_MERCHANTID = 888;
#endif
    private const string AFFILIATE_ID = "164464448194";

    private const string API_TOKEN = "FN1sJ433434343434qTPuta"; // NOT CURRENT VALUE
    private const string API_SECRET = "4444444"; // NOT CURRENT VALUE

    private const string FTP_USERNAME = "dddddd";
    private const string FTP_PASSWORD = "ddddddd";

    private const int HOUZZ_MERCHANTID = 4;


    #endregion

    #region Locals
    private string dataRootFolder;
    private object lockObj = new object();
    private List<AffiliateProduct> Products { get; set; }
    private static System.Threading.Timer refreshTimer;
    private bool isRefreshing;

    #endregion

    public ShareASaleManager(string dataRootFolder)
    {
      this.dataRootFolder = dataRootFolder;

      // wait a bit to first load up, then every couple hours;
      // but immediately if downloaded new data from Share a sale.

      refreshTimer = new System.Threading.Timer((st) =>
      {
        Refresh();
      }, null, TimeSpan.FromSeconds(60 * 1), TimeSpan.FromHours(2.0));

    }

    /// <summary>
    /// Return the root path where we keep the robot file data.
    /// </summary>
    private string DataFolder
    {
      get
      {
        return dataRootFolder;
      }
    }

    /// <summary>
    /// Gets a random set of products.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public List<AffiliateProduct> GetRandomProductList(int count, int? seed = null)
    {
      try
      {
        // our working snapshot of the full set of products.
        List<AffiliateProduct> localProducts;

        // use lock to deal with chance of product collection being changed out

        lock (lockObj)
        {
          localProducts = Products;
        }

        // make sure have something at all
        if (localProducts == null || localProducts.Count == 0)
          return new List<AffiliateProduct>();

        // if not much there, return what we have
        if (localProducts.Count <= count)
          return localProducts;

        // build up list to return
        var list = new List<AffiliateProduct>();

        var rnd = new Random(seed ?? (int)DateTime.Now.Ticks);
        var setProducts = new HashSet<long>();

        while (list.Count < count)
        {
          var randomIndex = rnd.Next(0, localProducts.Count);
          var product = localProducts[randomIndex];
          // make sure not already included
          if (!setProducts.Contains(product.ProductID))
          {
            list.Add(product);
            setProducts.Add(product.ProductID);
          }
        }

        return list;
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.Message);
        return new List<AffiliateProduct>();
      }
    }

    /// <summary>
    /// Downloads the most recent version of the specified merchant's data feed.
    /// This is our actual entry point from MVC controller.
    /// </summary>
    /// Saved in ShareASale folder root as MERCHANT-ID.txt.
    /// <param name="merchantID"></param>
    /// <returns></returns>
    public async Task<bool> DownloadDataFeed(int merchantID)
    {
      try
      {
        var filepath = MakeFilepath(string.Format("{0}.txt", merchantID));

        // NOTE: this means to pull a new feed we need to manually remove the file.
        // As of when I write this - don't have FTP access to any feeds, AND - 
        // might need some code changes to deal with 1GB Houzz file which unzips to 12GB

        if (!File.Exists(filepath))
        {
          var data = await GetFTPFeed(merchantID);
          File.WriteAllText(filepath, data);
        }

#if DEBUG
        //CountLinesInMerchantFile(merchantID);
        //MakeSmallMerchantFile(merchantID, int.MaxValue);
        //var columns = new int[] {9, 10, 19, 21, 22, 28, 29 };
        //foreach(var col in columns)
        //    DistinctValuesInMerchantFile(merchantID, col);
#endif

        CreatedReducedFileFromFile(merchantID, filepath);
        Refresh();

        return true;
      }
      catch (Exception Ex)
      {
        Debug.WriteLine(Ex.ToString());
        return false;
      }
    }

    private string MakeReducedFeedNameFilepath(int merchantID)
    {
      return MakeFilepath(string.Format("{0}-reduced.txt", merchantID));
    }

    private string MakeTemporaryReducedFeedNameFilepath(int merchantID)
    {
      return MakeFilepath(string.Format("{0}-reduced.tmp", merchantID));
    }


    /// <summary>
    /// This works - but moving away from it since Houzz feed cannot fit in memory.
    /// </summary>
    /// <param name="merchantID"></param>
    /// <param name="data"></param>
    private void CreatedReducedFile(int merchantID, string data)
    {
      var sb = new StringBuilder(data.Length / 2);
      int countProducts = 0;

      using (TextReader reader = new StringReader(data))
      {
        var line = reader.ReadLine();
        while (!string.IsNullOrWhiteSpace(line))
        {
          var ary = line.Split(new char[] { '|' });
          // productID, name, price, image url, product url
          sb.AppendFormat("{0}|{1}|{2}|{3}|{4}", ary[0], ary[1], ary[7], ary[5], ary[4].Replace("YOURUSERID", AFFILIATE_ID));
          sb.AppendLine();
          countProducts++;
          line = reader.ReadLine();
        }
      }

      var newData = sb.ToString();
      Debug.WriteLine(string.Format("Count download products: {0:N0}", countProducts));
      Debug.WriteLine(string.Format("Downloaded file, reduced size: {0:N0}", newData.Length));

      var filepath = MakeReducedFeedNameFilepath(merchantID);
      File.WriteAllText(filepath, newData);
    }

    private void MakeSmallMerchantFile(int merchantID, int maxCount = 10000)
    {
      var originalFilepath = MakeFilepath(string.Format("{0}.txt", merchantID));
      var smallFilepath = MakeFilepath(string.Format("{0}-small.txt", merchantID));

      File.Delete(smallFilepath);

      using (TextReader reader = File.OpenText(originalFilepath))
      {
        var lineBuffer = new List<string>();
        var line = reader.ReadLine();
        int countProducts = 0;
        while (!string.IsNullOrWhiteSpace(line))
        {
          var ary = line.Split(new char[] { '|' });

          if (ary[28] != "fabric")
          {
            line = reader.ReadLine();
            continue;
          }

          lineBuffer.Add(line);
          countProducts++; // because will possibly have flushed list, so need separate counter

          // flush buffer if getting full
          if (lineBuffer.Count >= 1024 * 20)
          {
            File.AppendAllLines(smallFilepath, lineBuffer);
            lineBuffer.Clear();
          }

          if (countProducts >= maxCount)
            break;

          line = reader.ReadLine();
        }

        // write any remaining
        if (lineBuffer.Count > 0)
          File.AppendAllLines(smallFilepath, lineBuffer);
      }
    }

    private void CountLinesInMerchantFile(int merchantID)
    {
      var originalFilepath = MakeFilepath(string.Format("{0}.txt", merchantID));
      int countProducts = 0;

      using (TextReader reader = File.OpenText(originalFilepath))
      {
        var line = reader.ReadLine();

        while (!string.IsNullOrWhiteSpace(line))
        {
          countProducts++; // because will possibly have flushed list, so need separate counter
          line = reader.ReadLine();
        }
      }

      Debug.WriteLine(string.Format("Total affiliate product count for merchant {0}: {1:N0}", merchantID, countProducts));

    }

    private void DistinctValuesInMerchantFile(int merchantID, int column)
    {
      var originalFilepath = MakeFilepath(string.Format("{0}.txt", merchantID));
      var outputFilepath = MakeFilepath(string.Format("{0}-distinct-{1}.txt", merchantID, column));

      var hash = new HashSet<string>();

      using (TextReader reader = File.OpenText(originalFilepath))
      {
        var line = reader.ReadLine();

        while (!string.IsNullOrWhiteSpace(line))
        {
          var ary = line.Split(new char[] { '|' });
          if (ary.Length > column)
          {
            var value = ary[column];
            hash.Add(value);
          }

          line = reader.ReadLine();
        }
      }

      var sortedList = hash.OrderBy(e => e).ToList();
      File.WriteAllLines(outputFilepath, sortedList);
    }


    /// <summary>
    /// Write out into temporary file, then rename. 
    /// </summary>
    /// <remarks>
    /// Keep  minimum time for when file does not exist.
    /// </remarks>
    /// <param name="merchantID"></param>
    /// <param name="rawFilepath"></param>
    /// <param name="maxCount"></param>
    private void CreatedReducedFileFromFile(int merchantID, string rawFilepath, int maxCount = int.MaxValue)
    {
      var startTime = DateTime.Now;

      var reducedFilepath = MakeReducedFeedNameFilepath(merchantID);
      var tempReducedFilepath = MakeTemporaryReducedFeedNameFilepath(merchantID);

      // make sure starting fresh
      File.Delete(tempReducedFilepath);


      var rand = new Random();
      Func<bool> isPicked = () =>
      {
        // approx one out of 40 gets picked, so we reduce from 8M to 200K
        return rand.Next(1, 40) == 10;
      };
      int countProducts = 0;

      using (TextReader reader = File.OpenText(rawFilepath))
      {
        var reducedLines = new List<string>();
        var line = reader.ReadLine();
        while (!string.IsNullOrWhiteSpace(line))
        {
          var ary = line.Split(new char[] { '|' });

          string imageUrl = (!string.IsNullOrWhiteSpace(ary[5])) ? ary[5] : ary[6]; // take thumb, then big

          // for houzz, no thumbs, big image 600x600

          if (ary[9] == "Home/Family" && ary[18] == "instock" && !string.IsNullOrWhiteSpace(imageUrl) && isPicked())
          {
            // productID, name, price, image url, product url
            var reducedLine = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}", ary[0], ary[1], ary[7], imageUrl, ary[4].Replace("YOURUSERID", AFFILIATE_ID), ary[10], ary[21], ary[22], ary[28], ary[29]);
            reducedLines.Add(reducedLine);
            countProducts++; // because will possibly have flushed list, so need separate counter

            // flush buffer if getting full
            if (reducedLines.Count >= 1024 * 20)
            {
              File.AppendAllLines(tempReducedFilepath, reducedLines);
              reducedLines.Clear();
            }

            if (countProducts >= maxCount)
              break;
          }

          line = reader.ReadLine();
        }

        // write any remaining
        if (reducedLines.Count > 0)
          File.AppendAllLines(tempReducedFilepath, reducedLines);

        // it should actually exist if we have something to work with
        if (File.Exists(tempReducedFilepath))
        {
          // remove any old, then rename temp to live
          File.Delete(reducedFilepath);
          File.Move(tempReducedFilepath, reducedFilepath);
        }
      }

      var endTime = DateTime.Now;

      Debug.WriteLine(string.Format("Affiliate products for merchant {0}: {1:N0}", merchantID, countProducts));
      Debug.WriteLine(string.Format("              *** Processing time: {0}", endTime - startTime));
    }


    /// <summary>
    /// Download zip file from FTP for merchantID, then replace affiliateID - return feed data as string.
    /// </summary>
    /// <param name="merchantID"></param>
    /// <returns></returns>
    private async Task<string> GetFTPFeed(int merchantID)
    {
      var url = string.Format("ftp://{0}:{1}@datafeeds.shareasale.com/{2}/{2}.zip", HttpUtility.UrlEncode(FTP_USERNAME), HttpUtility.UrlEncode(FTP_PASSWORD), merchantID);
      var uri = new Uri(url, UriKind.RelativeOrAbsolute);

      WebClient wc = new WebClient();

      using (var zipStream = new MemoryStream(await wc.DownloadDataTaskAsync(uri)))
      {
        using (var zip1 = ZipFile.Read(zipStream))
        {
          var file = zip1.First();
          using (var stm = new MemoryStream())
          {
            file.Extract(stm);
            stm.Position = 0;

            using (var reader = new StreamReader(stm))
            {
              var stringData = reader.ReadToEnd();
              return stringData;
            }
          }
        }
      }
    }

    /// <summary>
    /// Use their API to do search for something specific. NOT USED!!
    /// </summary>
    /// <remarks>
    /// This is a metered API with monthly limit of 200 calls. We're not intending to use it.
    /// </remarks>
    /// <param name="merchantID"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    private async Task<string> SearchProducts(int merchantID, string keyword)
    {
      // if want to use in production, will need to give it a quick test. Had it generally working.

      string actionVerb = "getProducts"; // name of api to invoke

      string ut = DateTime.Now.ToUniversalTime().ToString("r");

      SHA256Managed hasher = new SHA256Managed();

      byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(API_TOKEN + ':' + ut + ':' + actionVerb + ':' + API_SECRET));

      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < hash.Length; i++)
      {
        sb.Append(hash[i].ToString("x2"));
      }

      string authHeader = sb.ToString();

      var url = string.Format("https://shareasale.com/x.cfm?action={0}&affiliateId={1}&token={2}&version=1.8&merchantID={3}&keyword={4}",
          actionVerb, AFFILIATE_ID, API_TOKEN, merchantID, HttpUtility.UrlEncode(keyword));

      var uri = new Uri(url, UriKind.RelativeOrAbsolute);

      WebClient wc = new WebClient();

      wc.Headers.Add("x-ShareASale-Date", ut);
      wc.Headers.Add("x-ShareASale-Authentication", authHeader);

      var stringData = await wc.DownloadStringTaskAsync(uri);
      return stringData;
    }

    private void Refresh()
    {
      lock (lockObj)
      {
        if (isRefreshing)
          return;

        isRefreshing = true;
      }

      var filepath = MakeReducedFeedNameFilepath(HOUZZ_MERCHANTID);

      if (!File.Exists(filepath))
        return;

      var listProducts = new List<AffiliateProduct>();

      var lines = File.ReadAllLines(filepath);
      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line))
          continue;

        try
        {
          // productID, name, price, image url, product url
          var ary = line.Split(new char[] { '|' });

          var product = new AffiliateProduct()
          {
            ProductID = long.Parse(ary[0]),
            Name = ary[1],
            Price = decimal.Parse(ary[2]),
            ImageUrl = ary[3],
            ProductUrl = ary[4]
          };

          listProducts.Add(product);
        }
        catch
        {
        }
      }

      Debug.WriteLine(string.Format("Loaded {0:N0} affiliate products into memory.", listProducts.Count));

      lock (lockObj)
      {
        if (listProducts.Count > 0)
          Products = listProducts;

        isRefreshing = false;
      }
    }

    #region Read and Write Files

    /// <summary>
    /// Given just the filename component, combine with robots folder root.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private string MakeFilepath(string filename)
    {
      return Path.Combine(DataFolder, filename);
    }

    private List<string> GetFilenames(string filespec)
    {
      return Directory.GetFiles(DataFolder, filespec, SearchOption.TopDirectoryOnly).ToList();
    }

    #endregion
  }
}
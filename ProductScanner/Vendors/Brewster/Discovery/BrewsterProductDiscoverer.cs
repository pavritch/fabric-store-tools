using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExcelLibrary.SpreadSheet;
using OfficeOpenXml;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;

namespace Brewster.Discovery
{
  public class BrewsterProductDiscoverer : IProductDiscoverer<BrewsterVendor>
  {
    private readonly List<BrewsterExcelField> _excelFields = new List<BrewsterExcelField>
        {
            new BrewsterExcelField("#\nof Panels", ScanField.NumberOfPanels),
            new BrewsterExcelField("# of Panels", ScanField.NumberOfPanels),
            new BrewsterExcelField("# of Pieces", ScanField.NumberOfPieces),
            new BrewsterExcelField("# of Sheets", ScanField.NumberOfSheets),
            new BrewsterExcelField("Age Group", ScanField.Ignore),
            new BrewsterExcelField("Assembled Depth", ScanField.AssembledDepth),
            new BrewsterExcelField("Assembled Height", ScanField.AssembledHeight),
            new BrewsterExcelField("Assembled Length", ScanField.AssembledLength),
            new BrewsterExcelField("Assembled Width", ScanField.AssembledWidth),
            new BrewsterExcelField("Assembled Depth (in)", ScanField.AssembledDepth, s => s + " inches"),
            new BrewsterExcelField("Assembled Height (in)", ScanField.AssembledHeight, s => s + " inches"),
            new BrewsterExcelField("Assembled Width (in)", ScanField.AssembledWidth, s => s + " inches"),
            new BrewsterExcelField("Barcode", ScanField.UPC),
            new BrewsterExcelField("BookName", ScanField.Book),
            new BrewsterExcelField("Book Name", ScanField.Book),
            new BrewsterExcelField("Book #", ScanField.BookNumber),
            new BrewsterExcelField("BookNo", ScanField.BookNumber),
            new BrewsterExcelField("book", ScanField.Book),
            new BrewsterExcelField("Brand", ScanField.Brand),
            new BrewsterExcelField("Bullet Point 1", ScanField.Bullet1),
            new BrewsterExcelField("Bullet Point 2", ScanField.Bullet2),
            new BrewsterExcelField("Bullet Point 3", ScanField.Bullet3),
            new BrewsterExcelField("Bullet Point 4", ScanField.Bullet4),
            new BrewsterExcelField("Bullet Point 5", ScanField.Bullet5),
            new BrewsterExcelField("Bullet Point 6", ScanField.Bullet6),
            new BrewsterExcelField("Bullet Point 7", ScanField.Bullet7),
            new BrewsterExcelField("Bullet Point 8", ScanField.Bullet8),
            new BrewsterExcelField("Case Pack", ScanField.Ignore),
            new BrewsterExcelField("Code", ScanField.Code),
            new BrewsterExcelField("Collection", ScanField.Collection),
            new BrewsterExcelField("Color", ScanField.Color, s => s.TitleCase()),
            new BrewsterExcelField("Color Family", ScanField.ColorGroup),
            new BrewsterExcelField("Colorway", ScanField.Color),
            new BrewsterExcelField("Coverage", ScanField.Coverage),
            new BrewsterExcelField("Country of Orig", ScanField.Country),
            new BrewsterExcelField("Country of Origin", ScanField.Country),
            new BrewsterExcelField("Description", ScanField.Description),
            new BrewsterExcelField("Design Name", ScanField.ProductName),
            new BrewsterExcelField("Drop Ship Price", ScanField.Ignore),
            new BrewsterExcelField("Edge Feature", ScanField.EdgeFeature),
            new BrewsterExcelField("Height", ScanField.Height),
            new BrewsterExcelField("Height\n(ft)", ScanField.Height, s => s + " ft."),
            new BrewsterExcelField("Height in Inches", ScanField.Height, s => s + " inches"),
            new BrewsterExcelField("ImageNo", ScanField.Ignore),
            new BrewsterExcelField("League", ScanField.League),
            new BrewsterExcelField("Length (ft)", ScanField.Length, s => s + " ft."),
            new BrewsterExcelField("Map", ScanField.MAP),
            new BrewsterExcelField("Match", ScanField.Match),
            new BrewsterExcelField("Match_Desc.", ScanField.Match),
            new BrewsterExcelField("Material", ScanField.Material),
            new BrewsterExcelField("Model #", ScanField.SKU),
            new BrewsterExcelField("MSRP", ScanField.RetailPrice),
            new BrewsterExcelField("Name", ScanField.ProductName),
            new BrewsterExcelField("Number of Panels", ScanField.NumberOfPanels),
            new BrewsterExcelField("Origin", ScanField.Country),
            new BrewsterExcelField("Original Retail", ScanField.RetailPrice),
            new BrewsterExcelField("Original Retail (S/R)", ScanField.RetailPrice),
            new BrewsterExcelField("Package Height", ScanField.PackageHeight, s => s + " inches"),
            new BrewsterExcelField("Package Length", ScanField.PackageLength, s => s + " inches" ),
            new BrewsterExcelField("Package Width", ScanField.PackageWidth, s => s + " inches" ),
            new BrewsterExcelField("Package Height (in)", ScanField.PackageHeight, s => s + " inches"),
            new BrewsterExcelField("Package Length (in)", ScanField.PackageLength, s => s + " inches" ),
            new BrewsterExcelField("Package Width (in)", ScanField.PackageWidth, s => s + " inches" ),
            new BrewsterExcelField("Page", ScanField.Ignore),
            new BrewsterExcelField("Panel Length (in)", ScanField.Length, s => s + " inches"),
            new BrewsterExcelField("Panel Width (in)", ScanField.Width, s => s + " inches"),
            new BrewsterExcelField("Paste", ScanField.Prepasted),
            new BrewsterExcelField("PattNo", ScanField.PatternNumber),
            new BrewsterExcelField("Pattern", ScanField.PatternNumber),
            new BrewsterExcelField("Pattern #", ScanField.PatternNumber),
            new BrewsterExcelField("Product Category", ScanField.Category),
            new BrewsterExcelField("Product Type", ScanField.ProductType),
            new BrewsterExcelField("Product Sub-Type", ScanField.Ignore),
            new BrewsterExcelField("Removability", ScanField.Strippable),
            new BrewsterExcelField("Repeat", ScanField.Repeat, s => s + " inches"),
            new BrewsterExcelField("Repeat (in)", ScanField.Repeat, s => s + " inches"),
            new BrewsterExcelField("Retail", ScanField.RetailPrice, s => s.Replace("$", "")),
            new BrewsterExcelField("Roll Coverage", ScanField.Coverage),
            new BrewsterExcelField("Roll Length (ft)", ScanField.Length),
            new BrewsterExcelField("Roll Width", ScanField.Width),
            new BrewsterExcelField("Roll' Width (in)", ScanField.Width, s => s + " inches"),
            new BrewsterExcelField("Roll Width (in)", ScanField.Width, s => s + " inches"),
            new BrewsterExcelField("Sheet Length", ScanField.Length),
            new BrewsterExcelField("Sheet Length (in)", ScanField.Length, s => s + " inches"),
            new BrewsterExcelField("Sheet Width", ScanField.Width),
            new BrewsterExcelField("Sheet Width (in)", ScanField.Width, s => s + " inches"),
            new BrewsterExcelField("Sheets per Pack", ScanField.NumberOfSheets),
            new BrewsterExcelField("Single Roll Coverage", ScanField.Coverage ),
            new BrewsterExcelField("Single Roll Length (ft)", ScanField.Length, s => s + " ft."),
            new BrewsterExcelField("Single Roll/Spool Coverage", ScanField.Coverage ),
            new BrewsterExcelField("Single Roll/Spool Length", ScanField.Length),
            new BrewsterExcelField("SKU", ScanField.SKU),
            new BrewsterExcelField("SKU #", ScanField.SKU),
            new BrewsterExcelField("Stock Price", ScanField.Ignore),
            new BrewsterExcelField("Style", ScanField.Style),
            new BrewsterExcelField("Team", ScanField.Theme),
            new BrewsterExcelField("Theme", ScanField.Theme),
            new BrewsterExcelField("Type", ScanField.ProductType),
            new BrewsterExcelField("Unit UPC Code", ScanField.UPC),
            new BrewsterExcelField("Unit Weight (lbs)", ScanField.Weight),
            new BrewsterExcelField("Unit Weight", ScanField.Weight),
            new BrewsterExcelField("UPC", ScanField.UPC),
            new BrewsterExcelField("UPC Code", ScanField.UPC),
            new BrewsterExcelField("UPC Codes", ScanField.UPC),
            new BrewsterExcelField("Washability", ScanField.Cleaning),
            new BrewsterExcelField("Weight", ScanField.Cleaning),
            new BrewsterExcelField("Width (in)", ScanField.Width, s => s + " inches"),
            new BrewsterExcelField("Width\n(ft)", ScanField.Width, s => s + " ft."),
            new BrewsterExcelField("Width in Inches", ScanField.Width, s => s + " inches"),
        };

    // Ignored Fields:
    // - Page
    // - ImageNo - just the pattern number with .jpg after it
    // - 54" PattNo, 54in Pattern No - the last 5 of the pattNo prefixed with a W
    // - Case Pack - I don't think we care about this?

    // there is a fixed login for all dealers to the ftp site containing the images
    private const string FtpUsername = "3242322323";
    private const string FtpPassword = "32345345435435";
    private const string FtpUrl = "ftp://ftpimages.brewsterhomefashions.com/";
    private readonly IFtpClient _ftpClient;
    private readonly IStorageProvider<BrewsterVendor> _storageProvider;
    private readonly IVendorScanSessionManager<BrewsterVendor> _sessionManager;
    private readonly IPageFetcher<BrewsterVendor> _pageFetcher;

    public BrewsterProductDiscoverer(IFtpClient ftpClient, IStorageProvider<BrewsterVendor> storageProvider, IVendorScanSessionManager<BrewsterVendor> sessionManager, IPageFetcher<BrewsterVendor> pageFetcher)
    {
      _ftpClient = ftpClient;
      _storageProvider = storageProvider;
      _sessionManager = sessionManager;
      _pageFetcher = pageFetcher;
    }

    public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
    {
      try
      {
        await _pageFetcher.FetchFtpAsync(FtpUrl, CacheFolder.Search, "ftp-main", new NetworkCredential(FtpUsername, FtpPassword));
      }
      catch (Exception)
      {
        _sessionManager.Log(EventLogRecord.Error("Unable to reach " + FtpUrl));
        _sessionManager.Cancel();
        return new List<DiscoveredProduct>();
      }

      _sessionManager.VendorSessionStats.CurrentTask = "Traversing FTP directories";
      var allFiles = await ScrapeFileURLsAsync(FtpUrl);
      _sessionManager.VendorSessionStats.CurrentTask = "Traversed " + allFiles.Count + " directories";
      var spreadsheets = allFiles.Where(x => x.Contains(".xls")).ToList();
      var allSheets = await DownloadSheets(spreadsheets);
      var loadedProducts = LoadSpreadsheets(allSheets);
      loadedProducts = FilterProducts(loadedProducts);
      loadedProducts.ForEach(x => x[ScanField.ManufacturerPartNumber] = GetKey(x));
      loadedProducts = loadedProducts.Where(x => x[ScanField.ManufacturerPartNumber] != "").ToList();

      var images = GetImageUrls(allFiles).ToLookup(image => image.Segments.Last().Replace(".jpg", ""), image => image);
      _sessionManager.ForEachNotify("Setting image urls", loadedProducts, product =>
      {
        product[ScanField.ImageUrl] = AddCredentials(FindImage(product, images));
      });

      var matches = loadedProducts.Where(x => x[ScanField.Filename] == "TheVineyardData.xlsx");
      matches.ForEach(x => x[ScanField.MAP] = (x[ScanField.MAP].ToDecimalSafe() / 2m).ToString());

      return loadedProducts.Select(x => new DiscoveredProduct(x)).ToList();
    }

    private string GetKey(ScanData data)
    {
      if (data.ContainsKey(ScanField.PatternNumber))
      {
        return data[ScanField.PatternNumber].SkuTweaks();
      }
      if (data.ContainsKey(ScanField.SKU))
      {
        return data[ScanField.SKU].SkuTweaks();
      }
      return string.Empty;
    }

    private IEnumerable<Uri> GetImageUrls(IEnumerable<string> allFiles)
    {
      return allFiles.Where(x =>
          x.ContainsIgnoreCase(".jpg") &&
          !x.ContainsIgnoreCase("72dpi") &&
          !x.ContainsIgnoreCase("72 dpi") &&
          !x.ContainsIgnoreCase("Tinted") &&
          !x.ContainsIgnoreCase("CloseUp") &&
          !x.ContainsIgnoreCase("Zoomed") &&
          !x.ContainsIgnoreCase("Room"))
          .Select(x => new Uri(x)).ToList();
    }

    private string AddCredentials(Uri imageUrl)
    {
      if (imageUrl == null) return null;

      // http - don't modify
      if (!imageUrl.AbsoluteUri.StartsWith("ftp", StringComparison.OrdinalIgnoreCase)) return imageUrl.AbsoluteUri;

      // when ftp, need to add the credentials to the url
      // the login must be passed on the URL, special chars like # must be escaped.
      // ftp://dealers:Brewster%231@ftpimages.brewsterhomefashions.com/WallpaperBooks/Dollhouse VIII/Images/300dpi/Patterns/487-68832.jpg
      var credentials = string.Format("{0}:{1}@", FtpUsername.UrlEncode(), FtpPassword.UrlEncode());
      return imageUrl.AbsoluteUri.Replace("//", "//" + credentials);
    }

    private Uri FindImage(ScanData data, ILookup<string, Uri> imageLookup)
    {
      var brand = string.Empty;
      if (data.ContainsKey(ScanField.Brand))
        brand = data[ScanField.Brand];

      var id = data[ScanField.ManufacturerPartNumber];
      var imageList = imageLookup[id].ToList();
      if (imageList.Count() > 1)
      {
        // look for the brand in the url
        return imageList.FirstOrDefault(x => x.AbsoluteUri.ContainsIgnoreCase(brand.Replace(" ", "%20").Replace("�", "e")));
      }
      return imageList.FirstOrDefault();
    }

    private List<ScanData> FilterProducts(IEnumerable<ScanData> products)
    {
      return products.Where(p => !p.ContainsKey(ScanField.ProductName)
          || !string.IsNullOrWhiteSpace(p[ScanField.ProductName])).ToList();
    }

    private async Task<List<string>> DownloadSheets(List<string> bookUrls)
    {
      var spreadsheetNames = new List<string>();
      await _sessionManager.ForEachNotifyAsync("Downloading spreadsheets", bookUrls, async url =>
      {
        var bookName = url.Split(new[] { '/' }).Last();
        await DownloadFileToCacheAsync(new NetworkCredential(FtpUsername, FtpPassword), bookName, new Uri(url));
        spreadsheetNames.Add(bookName);
      });
      return spreadsheetNames;
    }

    private async Task DownloadFileToCacheAsync(NetworkCredential credential, string filename, Uri spreadsheetUrl)
    {
      var cacheHit = _storageProvider.HasFile(CacheFolder.Files, filename);
      if (!cacheHit)
      {
        var bytes = await spreadsheetUrl.DownloadBytesFromWebAsync(credential);
        if (bytes == null)
        {
          _sessionManager.Log(EventLogRecord.Error("Unable to load " + filename));
          _sessionManager.Cancel();
        }
        else
        {
          _storageProvider.SaveFile(CacheFolder.Files, filename, bytes);
        }
      }
    }

    private async Task<List<string>> ScrapeFileURLsAsync(string baseUrl)
    {
      return await FindAllFilesAsync("root", baseUrl);
    }

    private async Task<List<string>> FindAllFilesAsync(string dirname, string baseUrl)
    {
      _sessionManager.ThrowIfCancellationRequested();
      var files = new List<string>();
      var items = await ListDirectoryAsync(baseUrl.Replace(FtpUrl, "").UrlEncode() + dirname, baseUrl);
      files.AddRange(GetFullUrls(items, baseUrl));
      foreach (var item in items)
      {
        if (IsDirectory(item)) files.AddRange(await FindAllFilesAsync(item, baseUrl + item.Replace("#", "%23") + "/"));
      }
      return files;
    }

    private bool IsDirectory(string name)
    {
      return !name.Contains(".");
    }

    private IEnumerable<string> GetFullUrls(List<string> items, string baseUrl)
    {
      return items.Select(x => Path.Combine(baseUrl, x));
    }

    private async Task<List<string>> ListDirectoryAsync(string filename, string url)
    {
      var cacheHit = _storageProvider.HasFile(CacheFolder.Search, filename, CacheFileType.Txt);
      if (!cacheHit)
      {
        _sessionManager.BumpVendorRequest();
        var throttle = await _sessionManager.GetThrottleAsync();
        await Task.Delay(throttle);

        var listing = await _ftpClient.DirectoryListingAsync(new NetworkCredential(FtpUsername, FtpPassword), url);
        _storageProvider.SaveFile(CacheFolder.Search, filename, listing.ToJSON());
      }
      return _storageProvider.GetStringFile(CacheFolder.Search, filename).JSONtoList<List<string>>();
    }

    private List<ScanData> LoadSpreadsheets(List<string> spreadsheets)
    {
      var products = new List<ScanData>();
      _sessionManager.ForEachNotify("Loading files from spreadsheets", spreadsheets, sheet =>
      {
        var binary = _storageProvider.GetBinaryFile(CacheFolder.Files, sheet);
        if (binary.Length == 0) return;

        if (sheet.EndsWith(".xls")) products.AddRange(LoadProductsFromXLSSheet(binary, sheet));
        if (sheet.EndsWith(".xlsx")) products.AddRange(LoadProductsFromXLSXSheet(binary, sheet));
      });
      return products;
    }

    private List<ScanData> LoadProductsFromXLSXSheet(byte[] binary, string filename)
    {
      var products = new List<ScanData>();
      using (var xlsx = new ExcelPackage(new MemoryStream(binary)))
      {
        var sheet = xlsx.Workbook.Worksheets[1];
        var rows = sheet.GetNumRows();
        var columns = sheet.GetNumColumns();
        for (int rowNum = 2; rowNum <= rows; rowNum++)
        {
          var product = new ScanData();
          for (int colNum = 1; colNum <= columns; colNum++)
          {
            var header = sheet.GetValue<string>(1, colNum);
            if (header == null) continue;

            var property = _excelFields.SingleOrDefault(x => x.Header.ToLower() == header.Trim().ToLower());
            if (property == null) continue;

            var value = sheet.GetValue<string>(rowNum, colNum);
            if (value != null) value = value.Trim();
            if (property.PostProcessor != null) value = property.PostProcessor(value);
            product[property.AssociatedProperty] = value;
          }
          product[ScanField.Filename] = filename;
          products.Add(product);
        }
      }
      return products;
    }

    private IEnumerable<ScanData> LoadProductsFromXLSSheet(byte[] binary, string filename)
    {
      var products = new List<ScanData>();
      var workbook = Workbook.Load(new MemoryStream(binary));
      var sheet = workbook.Worksheets[0];
      var rows = sheet.GetNumRows();
      var columns = sheet.GetNumColumns();
      for (int rowNum = 1; rowNum < rows; rowNum++)
      {
        var product = new ScanData();
        for (int colNum = 0; colNum < columns; colNum++)
        {
          var header = sheet.Cells[0, colNum].StringValue;
          var property = _excelFields.SingleOrDefault(x => x.Header.ToLower() == header.Trim().ToLower());
          if (property == null)
          {
            continue;
          }

          var value = sheet.Cells[rowNum, colNum].StringValue;
          if (value != null) value = value.Trim();
          if (property.PostProcessor != null) value = property.PostProcessor(value);
          product[property.AssociatedProperty] = value;
        }
        product[ScanField.Filename] = filename;
        products.Add(product);
      }
      return products;
    }
  }
}
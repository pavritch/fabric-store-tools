using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace MirrorImage
{
  public class MirrorImageFileLoader : ProductFileLoader<MirrorImageVendor>
  {
    private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty(" Item#", ScanField.ManufacturerPartNumber),
            new FileProperty("Dimensions", ScanField.Dimensions),
            new FileProperty("Cost", ScanField.Cost),
        };

    public MirrorImageFileLoader(IStorageProvider<MirrorImageVendor> storageProvider)
        : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx, 1, 2) { }

    public override async Task<List<ScanData>> LoadProductsAsync()
    {
      var products = await base.LoadProductsAsync();
      products.ForEach(x => x.Cost = x[ScanField.Cost].ToDecimalSafe());
      return products;
    }
  }

  public class MirrorImageProductDiscoverer : IProductDiscoverer<MirrorImageVendor>
  {
    private const string SearchUrl = "http://www.mirrorimagehome.com/mirrors.html?limit=all";
    private const string WallDecorUrl = "https://www.mirrorimagehome.com/wall-decor.html?limit=all";
    private readonly IPageFetcher<MirrorImageVendor> _pageFetcher;

    public MirrorImageProductDiscoverer(IPageFetcher<MirrorImageVendor> pageFetcher)
    {
      _pageFetcher = pageFetcher;
    }

    public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
    {
      var urls = new List<string>();
      var search = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-mirrors");
      var results = search.QuerySelectorAll(".product-name a").Select(x => x.Attributes["href"].Value).ToList();
      urls.AddRange(results);

      search = await _pageFetcher.FetchAsync(WallDecorUrl, CacheFolder.Search, "search-wall-decor");
      results = search.QuerySelectorAll(".product-name a").Select(x => x.Attributes["href"].Value).ToList();
      urls.AddRange(results);
      return urls.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
    }
  }

  public class MirrorImageProductScraper : ProductScraper<MirrorImageVendor>
  {
    public MirrorImageProductScraper(IPageFetcher<MirrorImageVendor> pageFetcher) : base(pageFetcher) { }

    public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
    {
      var url = product.DetailUrl.AbsoluteUri;
      var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("https://www.mirrorimagehome.com", ""));

      var scanData = new ScanData();
      scanData[ScanField.ManufacturerPartNumber] = page.GetFieldValue("h1[itemprop='name']");
      scanData[ScanField.ProductName] = page.GetFieldValue(".short-description");
      //scanData[ScanField.StockCount] = page.GetFieldValue(".count-on-hand");
      //scanData[ScanField.Description] = page.GetFieldHtml("div[itemprop='description']");

      //var items = page.GetFieldHtml("div[itemprop='description']").Replace("<ul style=\"LIST-STYLE: NONE; \">", "");
      //var parts = items.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries);
      //foreach (var part in parts)
      //{
      //    var text = part.Trim().Replace("<li>", "").Replace("</li>", "");
      //    if (text.ContainsIgnoreCase("Designed By")) scanData[ScanField.Designer] = text;
      //    if (text.ContainsIgnoreCase("Finish")) scanData[ScanField.Finish] = text;
      //}
      //if (parts.Count() > 2) scanData[ScanField.ProductName] = parts[1].Replace("<li>", "").Replace("MIRROR", "").Trim();
      scanData.DetailUrl = product.DetailUrl;

      var image = page.QuerySelector("#amasty_zoom").Attributes["src"].Value;
      scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image));
      return new List<ScanData> { scanData };
    }
  }

  public class MirrorImageMetadataCollector : IMetadataCollector<MirrorImageVendor>
  {
    private readonly IProductFileLoader<MirrorImageVendor> _fileLoader;

    public MirrorImageMetadataCollector(IProductFileLoader<MirrorImageVendor> fileLoader)
    {
      _fileLoader = fileLoader;
    }

    public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
    {
      var fileProducts = await _fileLoader.LoadProductsAsync();
      foreach (var product in products)
      {
        var match = fileProducts.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
        if (match != null)
        {
          product.Cost = match.Cost;
          product[ScanField.Dimensions] = match[ScanField.Dimensions];
        }
      }
      return products;
    }
  }

  public class MirrorImageProductBuilder : ProductBuilder<MirrorImageVendor>
  {
    public MirrorImageProductBuilder(IPriceCalculator<MirrorImageVendor> priceCalculator) : base(priceCalculator) { }

    public override VendorProduct Build(ScanData data)
    {
      var vendor = new MirrorImageVendor();
      var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true),
          data[ScanField.ManufacturerPartNumber], BuildFeatures(data));

      homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Description]);
      homewareProduct.Name = data[ScanField.ProductName].TitleCase();
      homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
      homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
      homewareProduct.ScannedImages = data.GetScannedImages();
      return homewareProduct;
    }

    private HomewareProductFeatures BuildFeatures(ScanData data)
    {
      var dims = data[ScanField.Dimensions].Split('x').ToList();

      var features = new HomewareProductFeatures();
      features.Brand = data[ScanField.Designer].TitleCase().Replace("Designed by", "").Trim();
      features.Features = BuildSpecs(data);
      if (dims.Count > 0) features.Height = dims[0].ToDoubleSafe();
      if (dims.Count > 1) features.Width = dims[1].ToDoubleSafe();
      return features;
    }

    private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Wall_Art, desc => desc.Contains("Specimen")}
        };

    private Dictionary<string, string> BuildSpecs(ScanData data)
    {
      var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Finish, data[ScanField.Finish].TitleCase()}
            };
      if (data[ScanField.Dimensions].ContainsIgnoreCase("D"))
        specs.Add(ScanField.Diameter, data[ScanField.Dimensions].Replace(" D", ""));

      return ToSpecs(specs);
    }

    private HomewareCategory AssignCategory(string description)
    {
      foreach (var category in _categories)
        if (category.Value(description))
          return category.Key;
      return HomewareCategory.Mirrors;
    }
  }

  public class MirrorImageVendorAuthenticator : IVendorAuthenticator<MirrorImageVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new MirrorImageVendor();
      var webClient = new WebClientExtended();

      var firstPage = await webClient.DownloadPageAsync(vendor.LoginUrl);
      var loginLink = firstPage.QuerySelector("a[title='Log In']").Attributes["href"].Value;

      var loginPage = await webClient.DownloadPageAsync(loginLink);
      var formKey = loginPage.QuerySelector("input[name='form_key']").Attributes["value"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("form_key", formKey);
      nvCol.Add("login[username]", vendor.Username);
      nvCol.Add("login[password]", vendor.Password);
      nvCol.Add("send", "");

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl2, nvCol);
      if (postPage.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }


  public class MirrorImageVendor : HomewareVendor
  {
    public MirrorImageVendor()
        : base(178, "Mirror Image", "MI")
    {
      PublicUrl = "https://www.mirrorimagehome.com/";
      LoginUrl = "https://www.mirrorimagehome.com/";
      LoginUrl2 = "https://www.mirrorimagehome.com/customer/account/loginPost/";
      Username = "sales@example.com";
      Password = "password";

      UsesStaticFiles = true;
      RunDiscontinuedPercentageCheck = false;
    }
  }
}

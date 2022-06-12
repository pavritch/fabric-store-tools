using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Pindler.Discovery
{
  public class PindlerProductDiscoverer : IProductDiscoverer<PindlerVendor>
  {
    private const string CsvUrl = "https://trade.pindler.com/dataexport/insidestores/INSTORE.csv";
    private readonly IStorageProvider<PindlerVendor> _storageProvider;
    private readonly IProductFileLoader<PindlerVendor> _productFileLoader;

    public PindlerProductDiscoverer(IStorageProvider<PindlerVendor> storageProvider, IProductFileLoader<PindlerVendor> productFileLoader)
    {
      _storageProvider = storageProvider;
      _productFileLoader = productFileLoader;
    }

    public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
    {
      // Change SSL checks so that all checks pass
      ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

      var httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
          Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "insidestores", "pnp$7244!"))));

      var savePath = _storageProvider.GetProductsFileCachePath(ProductFileType.Xls);
      var webClient = new WebClient();
      webClient.Credentials = new NetworkCredential("insidestores", "pnp$7244!");
      FileExtensions.ConvertCSVToXLS(webClient.DownloadData(CsvUrl), savePath);

      ServicePointManager.ServerCertificateValidationCallback = null;

      var products = await _productFileLoader.LoadProductsAsync();
      return products.Select(x => new DiscoveredProduct(x)).ToList();
    }
  }
}
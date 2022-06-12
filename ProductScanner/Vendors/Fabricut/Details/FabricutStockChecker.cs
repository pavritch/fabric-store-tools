using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Fabricut.Details
{
  public class TrendStockChecker : FabricutBaseStockChecker<TrendVendor> { }
  public class FabricutStockChecker : FabricutBaseStockChecker<FabricutVendor> { }
  public class VervainStockChecker : FabricutBaseStockChecker<VervainVendor> { }
  public class StroheimStockChecker : FabricutBaseStockChecker<StroheimVendor> { }
  public class SHarrisStockChecker : FabricutBaseStockChecker<SHarrisVendor> { }

  public class ApiResult
  {
    public FabricStockData Stock { get; set; }
  }

  public class FabricStockData
  {
    public CurrentStockData Current { get; set; }
  }

  public class CurrentStockData
  {
    public float Total { get; set; }
    public List<float> Bolts { get; set; }
  }

  public abstract class FabricutBaseStockChecker<T> : StockChecker<T> where T : Vendor
  {
    private const string APIKey = "234234234234234";
    private const string StockUrl = "https://api.fabricut.com/v1E/{0}/product/{1}/stock+";

    public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
    {
      var page = await webClient.DownloadPageAsync(string.Format(StockUrl, APIKey, check.MPN));
      //var json = JsonConvert.DeserializeObject<ExpandoObject>(page.InnerText);
      var json = JObject.Parse(page.InnerText);
      var x = json["stock"]["current"]["bolts"].ToList();
      var values = x.Select(a => a.ToObject<float>()).ToList();
      var stock = values.Sum();

      if (check.GetPackaging() == "Double Roll") stock /= 2;
      if (check.GetPackaging() == "Triple Roll") stock /= 3;

      if (stock <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock, stock);
      if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
      return new ProductStockInfo(StockCheckStatus.InStock, stock);
    }
  }
}
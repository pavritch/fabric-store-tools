using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace PhillipJeffries
{
    public class PhillipJeffriesStockChecker : StockChecker<PhillipJeffriesVendor>
    {
        private const string SkewsJsonUrl = "https://www.phillipjeffries.com/api/products/collections/{0}/skews.json";

        public async override Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var skewUrl = string.Format(SkewsJsonUrl, check.PublicProperties["Collection Id"]);
            var skewData = await webClient.DownloadPageAsync(skewUrl);

            dynamic data = JObject.Parse(skewData.OuterHtml);
            dynamic match = null;
            foreach (var item in data.data.items)
            {
                if (item.id == check.MPN)
                {
                    match = item;
                }
            }
            var stock = match.order.wallcovering.availability;
            if (stock == "STOCKED") return new ProductStockInfo(StockCheckStatus.InStock);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}
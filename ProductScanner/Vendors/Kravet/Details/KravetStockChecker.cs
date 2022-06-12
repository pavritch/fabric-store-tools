using System;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Kravet.Details
{
    public class KravetStockChecker : KravetBaseStockChecker<KravetVendor> {}
    public class LeeJofaStockChecker : KravetBaseStockChecker<LeeJofaVendor> {}
    public class BakerLifestyleStockChecker : KravetBaseStockChecker<BakerLifestyleVendor> {}
    public class ColeAndSonStockChecker : KravetBaseStockChecker<ColeAndSonVendor> {}
    public class GPJBakerStockChecker : KravetBaseStockChecker<GPJBakerVendor> {}
    public class GroundworksStockChecker : KravetBaseStockChecker<GroundworksVendor> {}
    public class MulberryHomeStockChecker : KravetBaseStockChecker<MulberryHomeVendor> {}
    public class ParkertexStockChecker : KravetBaseStockChecker<ParkertexVendor> {}
    public class ThreadsStockChecker : KravetBaseStockChecker<ThreadsVendor> {}
    public class LauraAshleyStockChecker : KravetBaseStockChecker<LauraAshleyVendor> { }
    public class AndrewMartinStockChecker : KravetBaseStockChecker<AndrewMartinVendor> { }
    public class BrunschwigAndFilsStockChecker : KravetBaseStockChecker<BrunschwigAndFilsVendor> { }

    public class KravetBaseStockChecker<T> : StockChecker<T> where T : Vendor
    {
        private const string StockUrl = "http://www.e-designtrade.com/api/stock_check.asp?user={0}&password={1}&pattern={2}&color={3}&identifer=0&quantity={4}";

        public async override Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient)
        {
            //if (stockCheck.GetPackaging() == "Double Roll") stockCheck.Quantity /= 2;
            //if (stockCheck.GetPackaging() == "Triple Roll") stockCheck.Quantity /= 3;

            var page = await webClient.DownloadPageAsync(BuildUrl(stockCheck));

            var transactionStatus = page.QuerySelector("transaction_status").InnerText;
            if (transactionStatus == "Invalid Item") return new ProductStockInfo(StockCheckStatus.InvalidProduct);

            var itemStatus = page.QuerySelector("item_status").InnerText;
            if (itemStatus == "Discontinued") return new ProductStockInfo(StockCheckStatus.Discontinued);

            var inventoryStatus = page.QuerySelector("inventory_status").InnerText;
            if (inventoryStatus == "Y") return new ProductStockInfo(StockCheckStatus.InStock);

            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }

        private string BuildUrl(StockCheck check)
        {
            // MPNs look like this - 33462.11.0
            var mpnParts = check.MPN.Split(new []{'.'});
            if (mpnParts.Count() == 1)
            {
                var mpn = check.MPN.Replace("KR-", "");
                mpnParts = mpn.Split(new[] {'-'});
            }
            var vendor = new KravetVendor();
            return string.Format(StockUrl,
                vendor.Username,
                vendor.Password,
                mpnParts.First(),
                mpnParts[1],
                Convert.ToInt32(check.Quantity));
        }
    }
}
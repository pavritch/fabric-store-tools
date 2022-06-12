using System.Threading.Tasks;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Pindler.Details
{
    public class PindlerStockChecker : StockChecker<PindlerVendor>
    {
        private const string Token = "683150AbX72VWZ3101971tB5259532c";
        private const string RestUrl = "https://trade.pindler.com/cgi-bin/fccgi.exe?w3exec=checkstock&w3serverpool=checkstock&item={0}-{1}&yards={2}&token={3}";

        public async override Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient)
        {
            var props = stockCheck.PublicProperties;
            var key1 = ProductPropertyType.PatternNumber.DescriptionAttr();
            var key2 = ProductPropertyType.ColorName.DescriptionAttr();
            if (!props.ContainsKey(key1) || !props.ContainsKey(key2))
                return new ProductStockInfo(StockCheckStatus.OutOfStock);

            var fullUrl = string.Format(RestUrl, props[key1], props[key2], stockCheck.Quantity, Token);
            var page = await webClient.DownloadPageAsync(fullUrl);
            if (page.InnerText.ContainsIgnoreCase("INSTOCK"))
                return new ProductStockInfo(StockCheckStatus.InStock);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}
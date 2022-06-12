using System.Threading.Tasks;
using HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.StockChecks.Checkers
{
    public abstract class StockChecker<T> : IStockChecker<T> where T : Vendor
    {
        protected StockChecker(bool checkPreflight = false)
        {
            CheckPreflight = checkPreflight;
        }

        protected ProductStockInfo CreateStockInfo(HtmlNode stockElement, float quantity)
        {
            if (stockElement == null) return new ProductStockInfo(StockCheckStatus.OutOfStock);
            var stockCount = GetStockValue(stockElement.InnerText);
            if (stockCount == 0) return new ProductStockInfo(StockCheckStatus.OutOfStock, 0);
            if (stockCount < quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stockCount);
            if (stockCount >= quantity) return new ProductStockInfo(StockCheckStatus.InStock, stockCount);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }

        public abstract Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient);

        protected float GetStockValue(string stockText)
        {
            var stock = 0f;
            float.TryParse(stockText, out stock);
            return stock;
        }

        public bool CheckPreflight { get; private set; }
    }
}
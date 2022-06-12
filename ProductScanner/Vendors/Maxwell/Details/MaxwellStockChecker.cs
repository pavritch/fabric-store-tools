using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Maxwell.Details
{
    public class MaxwellStockChecker : StockChecker<MaxwellVendor>
    {
        private const string ProductUrl = "http://www.maxwellfabrics.com/p/{0}";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var url = string.Format(ProductUrl, check.MPN);
            var page = await webClient.DownloadPageAsync(url);
            var stockValue = page.GetFieldValue("div.microstockcheck");
            if (stockValue == null && !page.InnerText.ContainsIgnoreCase("only available in Canada"))
            {
                var inStock = await RunStockCheck(webClient, check.MPN);
                stockValue = inStock.ToString();
            }
            else
            {
                stockValue = page.InnerText.CaptureWithinMatchedPattern("Largest piece: (?<capture>(.*))yds");
            }

            if (stockValue == null) return new ProductStockInfo(StockCheckStatus.OutOfStock);

            var stock = GetStockValue(stockValue);
            if (stock <= 0.5) return new ProductStockInfo(StockCheckStatus.OutOfStock, stock);
            if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
            return new ProductStockInfo(StockCheckStatus.InStock, stock);
        }

        private async Task<double> RunStockCheck(IWebClientEx webClient, string item)
        {
            var url = "http://www.maxwellfabrics.com/maxwell_cart/lookup";
            var values = new NameValueCollection();
            values.Add("already_checked", "");
            values.Add("check_stock_status", "1");
            values.Add("product_details", "");
            values.Add("discontinued", "");
            values.Add("increment", "0.5");
            values.Add("minimum_order", "2.00");
            values.Add("price", "");
            values.Add("productAction", "checkstock");
            values.Add("minimum_reserve", "3");
            values.Add("getitem", item);
            values.Add("qty", "2");
            values.Add("qty_set[1]", "");
            values.Add("cut_set[1]", "");
            values.Add("qty_set[2]", "");
            values.Add("cut_set[2]", "");
            values.Add("qty_set[3]", "");
            values.Add("cut_set[3]", "");
            values.Add("order_actions", "");
            values.Add("check_stock_status", "");
            values.Add("order_action_status", "");
            values.Add("form_build_id", "form-JZ3CqG8ntKwf-yWMHZtlRt76L0ao6anLHawxfyVMcSc");
            values.Add("form_id", "maxwell_cart_add_to_cart_form");

            var stockData = await webClient.DownloadPageAsync(url, values);
            dynamic data = JObject.Parse(stockData.OuterHtml);
            return data.MemoInv;
        }
    }
}
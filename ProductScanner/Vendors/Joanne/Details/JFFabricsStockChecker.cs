using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace JFFabrics.Details
{
    public class JFFabricsStockChecker : StockChecker<JFFabricsVendor>
    {
        private const string StockUrl = "http://67.211.122.138:450/rpgsp/JF_E_ST_1.pgm";
        public JFFabricsStockChecker() : base(false) { } 

        public async override Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient)
        {
            var pattern = string.Empty;
            if (stockCheck.PublicProperties.ContainsKey(ProductPropertyType.PatternName.DescriptionAttr()))
                pattern = stockCheck.PublicProperties[ProductPropertyType.PatternName.DescriptionAttr()];
            else if (stockCheck.PublicProperties.ContainsKey(ProductPropertyType.PatternNumber.DescriptionAttr()))
                pattern = stockCheck.PublicProperties[ProductPropertyType.PatternNumber.DescriptionAttr()];

            var colorNum = stockCheck.PublicProperties[ProductPropertyType.ColorNumber.DescriptionAttr()];

            var values = new NameValueCollection();
            values.Add("PATTNO", pattern);
            values.Add("MCOLBK", colorNum);
            values.Add("QTYORD", stockCheck.Quantity.ToString());
            values.Add("UNITDESC", "YD");

            var pageOne = await webClient.DownloadPageAsync(StockUrl, values);

            values.Add("show_price_button", "_");
            var pageTwo = await webClient.DownloadPageAsync(StockUrl, values);

            // wallpaper products (starting with digits) are all marked out of stock - so we're showing those as in stock
            var inStock = pageTwo.InnerText.ContainsIgnoreCase("In Stock") || pattern.StartsWithDigit();
            if (inStock) return new ProductStockInfo(StockCheckStatus.InStock);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}
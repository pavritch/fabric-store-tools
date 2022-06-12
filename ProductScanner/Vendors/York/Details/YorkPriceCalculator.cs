using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;
using York.Metadata;

namespace York.Details
{
    public class YorkPriceCalculator : DefaultPriceCalculator<YorkVendor>
    {
        //private decimal _lowPriceThreshold = 22M;
        //private readonly List<ScanData> _priceRulesData;

        //a) If listed as temporarily on sale, use the sale pricing and special sale cost.
        //b) If indicated as MAP, use MAP price and standard cost.
        //c) Anything else becomes Price=(Cost x 1.4) with standard cost.

        public override ProductPriceData CalculatePrice(ScanData data)
        {
            // filter out ones that have really high cost - looks like a bug on their server
            if (data.Cost > 1000)
                data.Cost = 0;

            // if it's on the list but MAP = 0, then use our standard calculation
            if (data[ScanField.MAP] == "0")
            {
                var ourPrice = Math.Round(1.4m * data.Cost, 2);
                var ourRetail = Math.Round(ourPrice * 1.5m, 2);
                return new ProductPriceData(ourPrice - 1, ourRetail);
            }

            // if there's MAP listed, use that
            if (data[ScanField.MAP].ToDoubleSafe() > 0)
            {
                // b) If indicated as MAP, use MAP price and standard cost.
                return new ProductPriceData(data[ScanField.MAP].ToDecimalSafe(), data[ScanField.RetailPrice].ToDecimalSafe());
            }

            // if not on the sheet, calculate map from MSRP
            var msrp = data[ScanField.RetailPrice].Replace("$", "").ToDecimalSafe();
            var map = msrp*.85m;
            return new ProductPriceData(map, msrp);
        }
    }
}
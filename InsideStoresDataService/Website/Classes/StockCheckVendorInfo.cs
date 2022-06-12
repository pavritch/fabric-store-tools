using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Website
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StockCapabilities
    {
        //  None --- for when a phone call is needed to check stock (sad, but true)
        //  InOrOutOfStock – cannot tell us anything but if generally in our out of stock, no number hints of any kind
        //  CheckForQuantity – we can tell them a quantity we need, and they tell us if can be fullfilled
        //  ReportOnHand  -- they can tell us exactly how many (units, yards, rolls, etc) are in stock.
        None,
        Unavailable,
        InOrOutOfStock,
        CheckForQuantity,
        ReportOnHand
    }

    public class StockCheckVendorInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StockCapabilities StockCapabilities { get; set; }
        public string DisplayName { get; set; }
        public int VendorId { get; set; }

        public StockCheckVendorInfo()
        {

        }

        public StockCheckVendorInfo(StockCapabilities stockCapabilities, string displayName, int vendorId)
        {
            StockCapabilities = stockCapabilities;
            DisplayName = displayName;
            VendorId = vendorId;
        }
    }
}
namespace ProductScanner.Core.StockChecks.DTOs
{
    public enum StockCapabilities
    {
        //        None --- for when a phone call is needed to check stock (sad, but true)
        //        InOrOutOfStock – cannot tell us anything but if generally in our out of stock, no number hints of any kind
        //        CheckForQuantity – we can tell them a quantity we need, and they tell us if can be fullfilled
        //        ReportOnHand  -- they can tell us exactly how many (units, yards, rolls, etc) are in stock.
        None,
        Unavailable,
        InOrOutOfStock,
        CheckForQuantity,
        ReportOnHand
    }
}
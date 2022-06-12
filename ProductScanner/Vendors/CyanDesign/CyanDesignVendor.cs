using ProductScanner.Core;

namespace CyanDesign
{
  public class CyanDesignVendor : HomewareVendor
  {
    public CyanDesignVendor() : base(153, "Cyan Design", "CY")
    {
      LoginUrl = "http://cyan.design/customersignin.asp";
      LoginUrl2 = "http://cyan.design/customersignin.asp";
      Username = "tessa@example";
      Password = "dfdfdfdfdf";
      DiscoveryNotes = "Discovery and some details are via static spreadsheet downloaded from back-end. " +
                       "Additional details, stock, and price come from back-end.";

      UsesStaticFiles = true;
    }
  }
}

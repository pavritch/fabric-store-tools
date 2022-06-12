using ProductScanner.Core;

namespace LegendOfAsia
{
  public class LegendOfAsiaVendor : HomewareVendor
  {
    public LegendOfAsiaVendor() : base(160, "Legend of Asia", "LA")
    {
      LoginUrl = "https://www.legendofasia.com/login.php?action=check_login";
      LoginUrl2 = "http://www.legendofasia.com/account.php";
      PublicUrl = "http://www.legendofasia.com/";

      RunDiscontinuedPercentageCheck = false;
      Username = "customerservice@example.com";
      Password = "dddddd";
    }
  }
}

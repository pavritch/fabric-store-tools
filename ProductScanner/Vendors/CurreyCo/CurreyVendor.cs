using ProductScanner.Core;

namespace CurreyCo
{
  public class CurreyVendor : HomewareVendor
  {
    public CurreyVendor() : base(152, "Currey", "CU")
    {
      LoginUrl = "https://www.curreyandcompany.com/wholesale_signin.aspx";
      LoginUrl2 = "https://www.curreyandcompany.com/dashboard.aspx?skinid=1";
      Username = "sales@example.com";
      Password = "password";

      UsesIMAP = true;

      UsesStaticFiles = true;
      StaticFileVersion = 3;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "Our Price is their MSRP - for them, MAP = MSRP.";
    }
  }
}

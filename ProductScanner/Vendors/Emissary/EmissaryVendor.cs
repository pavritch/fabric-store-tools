using System.Text;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.DTOs;

namespace Emissary
{
  public class EmissaryVendor : HomewareVendor
  {
    public EmissaryVendor() : base(155, "Emissary", "EM", 2.5M, 2.5M * 1.7M)
    {
      LoginUrl = "http://www.emissaryusa.com";
      Username = "tessa@example";
      Password = "xxxxx";

      UsesStaticFiles = true;

      // stock info not online, sent by email
      // there are downloads online as well, but I can't currently login
      // also outstanding question to Peter on products (lamps specifically) that have different color options
    }
  }
}

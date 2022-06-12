using System.Globalization;
using System.Text;
using ProductScanner.Core;

namespace ElkGroup
{
  public class ElkGroupVendor : HomewareVendor
  {
    public ElkGroupVendor() : base(173, "Elk Group", "EG")
    {
      LoginUrl = "https://orders.elkgroupinternational.com/eSource/default/login.aspx";
      LoginUrl2 = "https://orders.elkgroupinternational.com/eSource/default.aspx";
      Username = "IS343434T010";
      Password = "password
      PublicUrl = "http://www.elkhospitality.com/";

      UsesStaticFiles = true;
    }
  }
}

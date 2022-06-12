using ProductScanner.Core;

namespace RyanStudio
{
  public class RyanStudioVendor : HomewareVendor
  {
    public RyanStudioVendor() : base(165, "Ryan Studio", "RS", 2.5M, 2.5M * 1.7M)
    {
      LoginUrl = "https://www.ryanstudio.biz/Login.asp";
      LoginUrl2 = "https://www.ryanstudio.biz/myaccount.asp?";
      PublicUrl = "http://www.ryanstudio.biz";
      Username = "dddddd";
      Password = "ddddd";

      // Made to order:  2-3 weeks ship
    }
  }
}
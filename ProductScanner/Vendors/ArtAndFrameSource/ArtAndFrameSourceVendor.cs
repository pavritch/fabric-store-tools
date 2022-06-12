using ProductScanner.Core;

namespace ArtAndFrameSource
{
  public class ArtAndFrameSourceVendor : HomewareVendor
  {
    public ArtAndFrameSourceVendor() : base(179, "Art and Frame Source", "AS", 2.5m, 4.0m)
    {
      LoginUrl = "http://www.artandframesourceinc.com/customer/account/login/";
      LoginUrl2 = "http://www.artandframesourceinc.com/customer/account/loginPost/";

      PublicUrl = "http://www.artandframesourceinc.com/";

      DiscoveryNotes = "All data comes from website after logging in.";

      Username = "tessa@example.com";
      Password = "passwordhere";
    }
  }
}
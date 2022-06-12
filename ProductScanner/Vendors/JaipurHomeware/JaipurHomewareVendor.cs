using ProductScanner.Core;

namespace JaipurHomeware
{
  public class JaipurHomewareVendor : HomewareVendor
  {
    public JaipurHomewareVendor() : base(157, "Jaipur", "JH")
    {
      LoginUrl = "https://www.jaipurliving.com/login";
      PublicUrl = "https://www.jaipurliving.com/";
      DiscoveryNotes = "Uses public website for discovery and product details. Uses FTP server for inventory feed and images.";
      Username = "sales@example.com";
      Password = "ddddd";

      MinimumCost = 30;
      RunDiscontinuedPercentageCheck = false;
    }
  }
}

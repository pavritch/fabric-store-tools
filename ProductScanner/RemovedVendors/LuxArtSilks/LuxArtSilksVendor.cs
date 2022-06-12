using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;

namespace LuxArtSilks
{
  public class LuxArtSilksVendor : HomewareVendor
  {
    public LuxArtSilksVendor() : base(161, "Legends of Asia", "LU")
    {
      LoginUrl = "http://www.lacefielddesigns.com/wp-admin/admin-ajax.php";
      LoginUrl2 = "http://www.lacefielddesigns.com/profile/";
      PublicUrl = "http://luxartsilks.com/";
      Username = "sales@example.com";
      Password = "34343434343K";
    }

    // Made to order:  2-3 weeks ship
  }

  public class LuxArtSilksProductDiscoverer : IProductDiscoverer<LuxArtSilksVendor>
  {
    private const string SearchUrl = "http://luxartsilks.com/products-2/?perpage=5000";
    public Task<List<ProductScanner.Core.Scanning.Products.Vendor.DiscoveredProduct>> DiscoverProductsAsync()
    {
      throw new NotImplementedException();
    }
  }

}

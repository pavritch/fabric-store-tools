using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core;

namespace MirrorImage
{
  public class MirrorImageVendor : HomewareVendor
  {
    public MirrorImageVendor() : base(162, "Mirror Image", "MI", 2.2M, 2.2M * 1.7M)
    {
      PublicUrl = "http://www.mirrorimageinc.com";
      Username = "tessa@example";
      Password = "passwordhere";
    }
  }

  // stock info not online,  use standard script
}

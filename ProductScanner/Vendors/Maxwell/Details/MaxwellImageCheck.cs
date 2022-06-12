using System.Net;
using ProductScanner.Core.Scanning;

namespace Maxwell.Details
{
    public class MaxwellImageCheck : IImageChecker<MaxwellVendor>
    {
        public bool CheckImage(HttpWebResponse response)
        {
            // the placeholder image has contentlength = 5634
            var isValid = response.ContentLength > 5634;
            return isValid;
        }
    }
}
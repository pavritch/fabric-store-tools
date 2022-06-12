using System.Net;
using ProductScanner.Core.Scanning;

namespace AllstateFloral
{
    public class AllstateImageChecker : IImageChecker<AllstateFloralVendor>
    {
        public bool CheckImage(HttpWebResponse response)
        {
            var isValid = response.ContentLength > 7312;
            return isValid;
        }
    }
}
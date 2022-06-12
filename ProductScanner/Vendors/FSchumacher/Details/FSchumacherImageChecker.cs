using ProductScanner.Core.Scanning;

namespace FSchumacher.Details
{
    public class FSchumacherImageChecker : IImageChecker<FSchumacherVendor>
    {
        public bool CheckImage(System.Net.HttpWebResponse response)
        {
            if (response.ContentLength == 6735) 
                return false;
            return true;
        }
    }
}
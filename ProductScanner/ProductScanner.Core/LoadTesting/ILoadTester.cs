namespace ProductScanner.Core.LoadTesting
{
    public interface ILoadTester
    {
        void RunAgainstAPI(int numThreads, int maxVendorsPerRequest, int maxProductsPerVendor);
    }
}
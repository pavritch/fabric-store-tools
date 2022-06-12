using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline
{
    public interface IVendorScanner<T> where T : Vendor
    {
        Task<List<ScanData>> Scan(ScanOptions options);
        Task<List<ScanData>> Resume();
        void Suspend();
        Task Cancel();
    }
}
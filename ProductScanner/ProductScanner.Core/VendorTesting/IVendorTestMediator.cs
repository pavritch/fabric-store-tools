using System.Collections.Generic;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace ProductScanner.Core.VendorTesting
{
    public interface IVendorTestMediator
    {
        List<IVendorTest> GetVendorTests<T>();
    }
}
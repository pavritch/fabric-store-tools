using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.VendorTesting.TestTypes;
using SimpleInjector;

namespace ProductScanner.Core.VendorTesting
{
    public sealed class VendorTestMediator : IVendorTestMediator
    {
        private readonly Container _container;
        public VendorTestMediator(Container container)
        {
            _container = container;
        }

        public List<IVendorTest> GetVendorTests<T>()
        {
            var testType = typeof (IVendorTest<>).MakeGenericType(typeof (T));
            return _container.GetAllInstances(testType).Cast<IVendorTest>().ToList();
        }
    }
}
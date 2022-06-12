using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace ProductScanner.App
{
    #region TestDescriptor Class
    /// <summary>
    /// A collection of these is needed by the VM to learn about and run tests.
    /// </summary>
    /// <remarks>
    /// These descriptors will be provided as an ordered list (in the order to be run).
    /// </remarks>
    public class TestDescriptor
    {
        /// <summary>
        /// The key is the unique identifier for the test assigned by the core modules.
        /// </summary>
        /// <remarks>
        /// Tests will be run by referencing them by their key.
        /// </remarks>
        public string Key { get; set; }

        /// <summary>
        /// Display description for the test.
        /// </summary>
        public string Description { get; set; }

        public IVendorTest VendorTest { get; set; }

        public TestDescriptor()
        {

        }

        public TestDescriptor(string key, string description, IVendorTest test)
        {
            Key = key;
            Description = description;
            VendorTest = test;
        }
    }
    #endregion
}

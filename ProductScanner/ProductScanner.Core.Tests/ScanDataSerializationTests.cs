using Newtonsoft.Json;
using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class ScanDataSerializationTests
    {
        [Test]
        public void Test()
        {
            var scanData = new ScanData();
            scanData[ScanField.AdditionalInfo] = "info";
            scanData.Cost = 50;
            var results = JsonConvert.SerializeObject(scanData, Formatting.Indented);
        }
    }
}
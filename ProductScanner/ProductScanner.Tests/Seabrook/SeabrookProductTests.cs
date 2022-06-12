using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.ProductProperties;
using FabricUpdater.Vendors.Seabrook;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Seabrook
{
    [TestFixture]
    class SeabrookProductTests
    {
        private ProductConfigValues _values = new ProductConfigValues(string.Empty, string.Empty, string.Empty, 0, 0);

        [TestCase("21. Vivant-Export", "Vivant")]
        [TestCase("01. Classic Elegance-Export", "Classic Elegance")]
        [TestCase("19. Alabaster-Export-Platinum", "Alabaster")]
        [TestCase("White Heron II", "White Heron II")]
        public void TestSeabrookCollection(string input, string output)
        {
            var product = new SeabrookProduct(_values);
            product.VendorProperties[ProductPropertyType.Collection] = input;
            Assert.AreEqual(output, product.Collection);
        }

        [TestCase("Faux, Moirã©, Texture-painted Effects", null)]
        public void TestSeabrookCategory(string input, string output)
        {
            var product = new SeabrookProduct(_values);
            product.VendorProperties[ProductPropertyType.Category] = input;
            Assert.AreEqual(output, product.Style);
        }
    }
}

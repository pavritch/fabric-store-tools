using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.ProductProperties;
using FabricUpdater.Vendors.Brewster;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Brewster
{
    [TestFixture]
    public class BrewsterProductTests
    {
        private ProductConfigValues _values = new ProductConfigValues(string.Empty, string.Empty, string.Empty, 0, 0);

        [TestCase("28 square feet", "28 Square Feet")]
        [TestCase("28.9 square feet", "28.9 Square Feet")]
        [TestCase("37.13 Square Feet", "37.13 Square Feet")]
        [TestCase("30.4 sq feet", "30.4 Square Feet")]
        [TestCase("Available only by the meter", null)]
        [TestCase("34.1' wide x 27.7\" high square feet", null)]
        [TestCase("36.6\" wide x 9.2' long", null)]
        [TestCase("146.4\" w x 110.2\" h", null)]
        [TestCase("Ten 24\" x 40\" Panels", null)]
        [TestCase("82.5\" wide x 110\" high", null)]
        [TestCase("6'4\" x 8'10\"", null)]
        public void TestBrewsterCoverage(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.Coverage] = input;
            Assert.AreEqual(output, product.Coverage);
        }

        [TestCase("3 5/16\"", "3.3125 inches")]
        [TestCase("6\"", "6 inches")]
        [TestCase("100", "100 inches")]
        [TestCase("106.3", "106.3 inches")]
        [TestCase("a. 18 b. 18 c. 18 d. 18 e. 18", null)]
        [TestCase("12.204699999999999", "12.2047 inches")]
        [TestCase("4.625", "4.625 inches")]
        [TestCase("26.7716", "26.7716 inches")]
        public void TestBrewsterHeight(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.Height] = input;
            Assert.AreEqual(output, product.Height);
        }

        [TestCase("16.5", "16.5 inches")]
        [TestCase("15", "15 inches")]
        [TestCase("16.5'", "16.5 ft.")]
        [TestCase("27.559", "27.559 inches")]
        [TestCase("(27.56 - Cork) (13 - Metallic)", null)]
        public void TestBrewsterLength(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.Length] = input;
            Assert.AreEqual(output, product.Length);
        }

        [TestCase("25 3/16\"", "25.1875 inches")]
        [TestCase("21\"", "21 inches")]
        [TestCase("5.21653825", "5.2165 inches")]
        [TestCase("3.4645688000000003", "3.4646 inches")]
        [TestCase(".19\"", null)]
        [TestCase("72\"h x 84.5\"w", null)]
        [TestCase("0", null)]
        [TestCase("0\"", null)]
        [TestCase("12'9\" x 8'10\"", null)]
        [TestCase("Random", "Random")]
        [TestCase("13IN 32cm", "13 inches")]
        public void TestBrewsterRepeat(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.Repeat] = input;
            Assert.AreEqual(output, product.Repeat);
        }

        [TestCase("Non Woven", "Non Woven")]
        [TestCase("Non-Woven", "Non Woven")]
        [TestCase("Non-woven", "Non Woven")]
        [TestCase("Heavyweight Vinyl", "Heavy Weight Vinyl")]
        [TestCase("Heavy-Weight Vinyl", "Heavy Weight Vinyl")]
        [TestCase("Heavy Weight Vinyl", "Heavy Weight Vinyl")]
        [TestCase("Heavy-weight Vinyl", "Heavy Weight Vinyl")]
        public void TestBrewsterMaterial(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.Material] = input;
            Assert.AreEqual(output, product.Material);
        }

//                "Wallpaper",
//                "Contract Wallcoverings",
//                "Borders",
//                "Murals",
//                "Wall Appliques",
//                "Fabrics",
//                "Embellishments",
        [TestCase("W", "Wallpaper")]
        [TestCase("B", "Borders")]
        [TestCase("M", "Murals")]
        [TestCase("Mural", "Murals")]
        [TestCase("Wall Mural", "Murals")]
        [TestCase("Appliqués", "Appliqués")]
        [TestCase("G", null)]
        [TestCase("GC", null)]
        public void TestBrewsterProductType(string input, string output)
        {
            var product = new BrewsterProduct(_values);
            product.VendorProperties[ProductPropertyType.ProductType] = input;
            Assert.AreEqual(output, product.ProductType);
        }
    }
}
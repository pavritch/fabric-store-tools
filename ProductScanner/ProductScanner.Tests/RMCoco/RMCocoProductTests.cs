using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.ProductProperties;
using FabricUpdater.Vendors.RMCoco;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.RMCoco
{
    [TestFixture]
    public class RMCocoProductTests
    {
        private ProductConfigValues _values = new ProductConfigValues(string.Empty, string.Empty, string.Empty, 0, 0);

        [TestCase("41% Rayon, 34% Polyester, ", "41% Rayon, 34% Polyester, 25% Other")]
        [TestCase("41% Rayon, 34% Polyester, 25%", "41% Rayon, 34% Polyester, 25% Other")]
        [TestCase("41% Rayon, 34% Polyester, 25% Co", "41% Rayon, 34% Polyester, 25% Cotton")]
        [TestCase("66% Rayon, 33% Polyester, 1% Nyl", "66% Rayon, 33% Polyester, 1% Nylon")]
        [TestCase("45% Cotton, 35% Polyester, 20% V", "45% Cotton, 35% Polyester, 20% Viscose")]
        [TestCase("72% Viscose, 17% Cotton, 11% Pol", "72% Viscose, 17% Cotton, 11% Polyester")]
        [TestCase("52% Polyester, 48% Polyamide", "52% Polyester, 48% Polyamide")]
        [TestCase("100% Cotton", "100% Cotton")]
        [TestCase("45% Cotton, 35% Polyester, 20% V", "45% Cotton, 35% Polyester, 20% Viscose")]
        [TestCase("54% Acrylic, 36% Polyester, 10%", "54% Acrylic, 36% Polyester, 10% Other")]
        [TestCase("100% Cotton, Soil & Stain Repell", "100% Cotton")]
        [TestCase("Face: 100% Polyester Vinyl, Back", "100% Polyester Vinyl")]
        [TestCase("54% Spun Viscose, 26% Polyester", "54% Spun Viscose, 26% Polyester, 20% Other")]
        [TestCase("100%COTTON", "100% Cotton")]
        [TestCase("64% Rayon, 26% Polyester, 10% Co", "64% Rayon, 26% Polyester, 10% Cotton")]
        [TestCase("39.15% Bamboo, 37.88% Cotton, 14", "39% Bamboo, 38% Cotton, 23% Other")]
        [TestCase("51%POLYACRYLIC 21%VISC 11%POLY", "51% Polyacrylic, 21% Viscose, 11% Polyester, 17% Other")]
        [TestCase("51$POLYACRYLIC 21%VIS 11%POLY 9%", "51% Polyacrylic, 21% Viscose, 11% Polyester, 17% Other")]
        [TestCase("Half Drop 100% Cotton", "100% Cotton")]
        [TestCase("36% Viscose 24% Polyester 30% Li", "36% Viscose, 30% Linen, 24% Polyester, 10% Other")]
        [TestCase("Face 100% PVC, Backing 65% Polye", "100% PVC")]
        [TestCase("100% Cotton with 100% Rayon Embr", "100% Cotton")]
        [TestCase("36% Acrylic, 32% Polyester, 32%", "36% Acrylic, 32% Polyester, 32% Other")]
        [TestCase("57% Viscose Spun, 43%POLY", "57% Viscose Spun, 43% Polyester")]
        [TestCase("39.74% Polyester, 32.5% Rayon, 2", "40% Polyester, 32% Rayon, 28% Other")]
        [TestCase("42% Polyester Filament, 32% Spun", "42% Polyester Filament, 32% Spun Viscose, 26% Other")]
        [TestCase("100% Silk Base Fabric Embroidere", "100% Silk")]
        [TestCase("89% COTTON 11% RAYON", "89% Cotton, 11% Rayon")]
        [TestCase("100% Bamboo", "100% Bamboo")]
        [TestCase("50% Polyester, 45% Spun Rayon, 5", "50% Polyester, 45% Spun Rayon, 5% Other")]
        [TestCase("5% Cotton Applique", "5% Cotton, 95% Other")]
        [TestCase("52%RAYON 26% NYLON 22% POLYESTER", "52% Rayon, 26% Nylon, 22% Polyester")]
        [TestCase("50% Rayom 27% Nylon 23% Polyeste", "50% Rayon, 27% Nylon, 23% Polyester")]
        [TestCase("51% Bamboo/Rayon 49% organic Cot", "51% Bamboo/Rayon, 49% Organic Cotton")]
        [TestCase("48.13% Rayon 42.42% Egymer Cotto", "48% Rayon, 42% Egymer, 10% Other")]
        [TestCase("100% Polyester, 100% Rayon Embro", "100% Polyester")]
        [TestCase("743% Viscose Chenille, 42% Polye", "42% Polyester, 58% Other")]
        [TestCase("51% Bamboo/Rayon, 49% Organic Co", "51% Bamboo/Rayon, 49% Organic Cotton")]
        [TestCase("88% Rayon, 15% Polyester", "88% Rayon, 12% Other")]

        // not working
        [TestCase("Linen, 1% Silk", null)]
        [TestCase("44% POLY FIL 30% ACRY 26% VISC", null)]
        [TestCase("er .10% Nylon", null)]
        [TestCase("54.000\" (137cm) Wide 100% Acryl", null)]
        [TestCase("Chenille, 11% Spun Viscose, 3% V", null)]
        public void TestRMCocoContent(string input, string expected)
        {
            var product = new RMCocoProduct(_values);
            product.VendorProperties[ProductPropertyType.Content] = input;

            Assert.AreEqual(expected, product.Content);
        }

        [TestCase("EBONY", "Ebony")]
        [TestCase("BLUE BROWN", "Blue Brown")]
        [TestCase("306 CORDOVAN", "Cordovan")]
        [TestCase("101", null)]
        [TestCase("LIZARD I", "Lizard I")]
        [TestCase("CHOCOLATE 53.5", "Chocolate")]
        [TestCase("123377", null)]
        [TestCase("FLAX/BLACK", "Flax/Black")]
        [TestCase("PUMPKIN LATTEWD WRG B", "Pumpkin Lattewd Wrg B")]
        [TestCase("ANTIQUE GOLD 55", "Antique Gold")]
        [TestCase("BLK WALNUT", "Black Walnut")]
        [TestCase("MERLOT EMB WD", "Merlot Emb Wd")]
        [TestCase("DK GOLD", "Dark Gold")]
        [TestCase("CHESTNUT OR ACORN", "Chestnut Or Acorn")]
        [TestCase("S50", null)]
        [TestCase("S488", null)]
        [TestCase("S825 SAME AS S525", null)]
        [TestCase("770 ACORN", "Acorn")]
        [TestCase("DE9 PURPLE PEAK", "Purple Peak")]
        [TestCase("PM9 CITY OF GOLD", "City Of Gold")]
        [TestCase("Q76 DUCKLING", "Duckling")]
        [TestCase("102 5", null)]
        [TestCase("JP7 FLINTSTONE - CFA", "Flintstone")]
        [TestCase("QO6 JESUIT - CFA", "Jesuit")]
        [TestCase("6 STEPPING STONE SEND SFA", "Stepping Stone Send")]
        [TestCase("FRINGE Q45 FEMME - CFA", "Femme")]
        [TestCase("TASSEL G17 WARRENDER", "Warrender")]
        [TestCase("TASSEL", null)]
        [TestCase("CORD", null)]
        [TestCase("1050 SFA", null)]
        [TestCase("101 DEC.CORD WITH LIP", null)]
        [TestCase("1014 - CFA", null)]
        [TestCase("1127 ROPE", null)]
        [TestCase("TSSL RD1 WEATHERED WOOD", "Weathered Wood")]
        [TestCase("TSL RC9 STEPPING STONE", "Stepping Stone")]
        public void TestRMCocoColorName(string input, string output)
        {
            var product = new RMCocoProduct(_values);
            product.VendorProperties[ProductPropertyType.ColorName] = input;
            Assert.AreEqual(output, product.ColorName);
        }

        [TestCase("SUNRISE", null)]
        [TestCase("QO6", "QO6")]
        [TestCase("8099", "8099")]
        public void TestRMCocoColorNumber(string input, string output)
        {
            var product = new RMCocoProduct(_values);
            product.VendorProperties[ProductPropertyType.ColorNumber] = input;
            Assert.AreEqual(output, product.ColorNumber);
        }

        [TestCase("INSPIRE", "Inspire")]
        [TestCase("Q553", null)]
        [TestCase("1095CB", null)]
        [TestCase("W079123", null)]
        [TestCase("W079123", null)]
        public void TestRMCocoPatternName(string input, string output)
        {
            var product = new RMCocoProduct(_values);
            product.VendorProperties[ProductPropertyType.PatternName] = input;
            Assert.AreEqual(output, product.PatternName);
        }
    }
}

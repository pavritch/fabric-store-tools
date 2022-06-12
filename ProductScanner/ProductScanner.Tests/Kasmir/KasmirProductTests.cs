using FabricUpdater.Core.Scanning.FileLoading;
using FabricUpdater.Core.Scanning.ProductProperties;
using FabricUpdater.Vendors.Kasmir;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Kasmir
{
    [TestFixture]
    public class KasmirProductTests
    {
        private ProductConfigValues _values = new ProductConfigValues(string.Empty, string.Empty, string.Empty, 0, 0);

        [TestCase("- GRAND DEBUT VOLUME III", "Grand Debut Volume III")]
        [TestCase("- SILKEN INSPIRATIONS VOL1", "Silken Inspirations Vol 1")]
        [TestCase("- METAMORPHOSIS VOLUME IV", "Metamorphosis Volume IV")]
        public void TestKasmirBook(string input, string output)
        {
            var product = new KasmirProduct(_values);
            product.VendorProperties[ProductPropertyType.Book] = input;
            Assert.AreEqual(output, product.Book);
        }

        [TestCase("Embroidery/Crewel, Lattice/Scrollwork, Stripe", null)]
        [TestCase("Ottoman/Faille, StriÃ©, Stripe, Wovens", null)]
        [TestCase("Plaid/Check, Wovens", null)]
        [TestCase("Damask/Imberline, Lattice/Scrollwork, Made In USA, Prints", null)]
        public void TestKasmirStyle(string input, string output)
        {
            var product = new KasmirProduct(_values);
            product.VendorProperties[ProductPropertyType.Description] = input;
            Assert.AreEqual(output, product.Style);
        }

        [TestCase("L55 R45", null)]
        [TestCase("AC100", null)]
        [TestCase("C68 P32", null)]
        [TestCase("R50 C40 V30 AC7", null)]
        [TestCase("P50 40VIS 010", null)]
        [TestCase("R65 SR30 VIS5", null)]
        [TestCase("F: D100 B:C100", null)]
        [TestCase("R75 C25 C100", null)]
        [TestCase("P79 R21 LB100", null)]
        [TestCase("AC59 P41 LB100", null)]
        [TestCase("R74 P26 LB100", null)]
        [TestCase("P100, Emb:R100", null)]
        [TestCase("R66 P34 LB100", null)]
        [TestCase("Cotton 100", "100% Cotton")]

        // not yet implemented
        [TestCase("P93 C7 ACB100", null)]
        [TestCase("P52 N48, EMB:R100", null)]
        [TestCase("P100 LB", null)]
        [TestCase("DED P38 R13", null)]
        [TestCase("SR 100", "100% SR")]
        public void TestKasmirContent(string input, string output)
        {
            var product = new KasmirProduct(_values);
            product.VendorProperties[ProductPropertyType.Content] = input;
            Assert.AreEqual(output, product.Content);
        }
    }
}
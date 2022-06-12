using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.FieldTypes;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class RugWeaveTests
    {
        [TestCase("Hand-woven", "Hand Woven")]
        [TestCase("Hand-tufted", "Hand Tufted")]
        [TestCase("Hand-Woven", "Hand Woven")]
        [TestCase("Flatweave", "Flat Weave")]
        [TestCase("Handmade", "Handmade")]
        [TestCase("Hand-knotted", "Hand Knotted")]
        [TestCase("Hand-Tufted", "Hand Tufted")]
        [TestCase("Dhurrie", "Dhurrie")]
        [TestCase("Throws", "Throws")]
        [TestCase("Hand Woven", "Hand Woven")]
        [TestCase("Hand Knotted", "Hand Knotted")]
        [TestCase("Power Loomed", "Power Loomed")]
        [TestCase("Hand Loomed", "Hand Loomed")]
        [TestCase("Machine Made", "Machine Made")]
        [TestCase("Hand Tufted", "Hand Tufted")]
        [TestCase("Hand Hooked", "Hand Hooked")]
        [TestCase("Hand Made", "Hand Made")]
        [TestCase("HAND TUFTED - LOOP & CUT", "Hand Tufted")]
        [TestCase("Hand WOVEN", "Hand Woven")]
        [TestCase("Hand Woven Flat Weave", "Hand Woven, Flatweave")]
        [TestCase("Loom Knotted", "Loom Knotted")]
        [TestCase("Hand Loom", "Hand Loom")]
        [TestCase("Machine Woven", "Machine Woven")]
        [TestCase("Hand TUFTED", "Hand Tufted")]
        [TestCase("Hand woven flat weave", "Hand Woven, Flatweave")]
        [TestCase("Power-loomed", "Power Loomed")]
        [TestCase("HAND KNOTTED", "Hand Knotted")]
        public void TestRugWeaveParse(string input, string output)
        {
            var weave = new RugWeave(input);
            Assert.AreEqual(output, weave.GetFormattedWeave());
        }
    }
}
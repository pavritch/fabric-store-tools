using NUnit.Framework;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
        [TestCase("Set of 12 Leatherbound Books", "Leatherbound Books (Set of 12)")]
        [TestCase("Set of 2 Casted Trays", "Casted Trays (Set of 2)")]
        [TestCase("Assorted Vases Set of 6 - Lime", "Assorted Vases - Lime (Set of 6)")]
        [TestCase("Set Of 2 Angular Side Tables", "Angular Side Tables (Set of 2)")]
        [TestCase("(Set of 2) Champagne and Green Silk Floral Nosegays in Bronze Metal Cones", "Champagne and Green Silk Floral Nosegays in Bronze Metal Cones (Set of 2)")]
        [TestCase("(Set of 3) Single Artichoke in a Matte Black Plum Pot", "Single Artichoke in a Matte Black Plum Pot (Set of 3)")]
        [TestCase("Set of 3 Concrete-Lite Planter Boxes", "Concrete-Lite Planter Boxes (Set of 3)")]
        [TestCase("Chinoiserie Green Crystal Shell Vases - Set of 3", "Chinoiserie Green Crystal Shell Vases - (Set of 3)")]
        public void TestSetMethod(string input, string output)
        {
            Assert.AreEqual(output, input.MoveSetInfo());
        }
    }
}
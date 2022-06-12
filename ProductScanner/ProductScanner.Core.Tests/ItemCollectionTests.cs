using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.FieldTypes;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class ItemColorTests
    {
        [TestCase("AQU BLUE", "Aqua Blue")]
        [TestCase("FOREST GRE", "Forest Green")]
        [TestCase("FOREST GRN", "Forest Green")]
        [TestCase("FOREST GREEN", "Forest Green")]
        [TestCase("POMEGRANAT", "Pomegranate")]
        [TestCase("Burgandy", "Burgundy")]
        [TestCase("Choclate", "Chocolate")]

        [TestCase("D.Green", "Dark Green")]
        [TestCase("D. Green", "Dark Green")]
        [TestCase("Dk Blue", "Dark Blue")]
        [TestCase("DkBlue", "Dark Blue")]
        [TestCase("Dk. Blue", "Dark Blue")]
        [TestCase("Dk.Blue", "Dark Blue")]

        [TestCase("LT.BROWN", "Light Brown")]
        [TestCase("L. Blue", "Light Blue")]
        [TestCase("L.Blue", "Light Blue")]

        [TestCase("M. Red", "Medium Red")]
        [TestCase("M.Red", "Medium Red")]
        [TestCase("Med Brown", "Medium Brown")]

        [TestCase("Bge", "Beige")]
        [TestCase("Crèam", "Cream")]
        [TestCase("Crème", "Creme")]
        [TestCase("Dark Beig", "Dark Beige")]
        [TestCase("Lavander", "Lavender")]
        [TestCase("Bone Foler Wht", "Bone Foler White")]
        [TestCase("Chamimile", "Chamomile")]
        [TestCase("Fuchisa", "Fuchsia")]
        [TestCase("Gls", "Glass")]
        [TestCase("Cloud Gry", "Cloud Gray")]
        [TestCase("Light Gy", "Light Gray")]
        [TestCase("Tilled Soil Brn", "Tilled Soil Brown")]
        [TestCase("Wrought Irn Nvy", "Wrought Iron Navy")]
        public void TestFormatting(string input, string output)
        {
            var color = new ItemColor(input);
            Assert.AreEqual(output, color.GetFormattedColor());
        }
    }

    [TestFixture]
    public class ItemCollectionTests
    {
        [TestCase("ARTISAN COLLECTION", "Artisan")]
        [TestCase("ARTISAN ", "Artisan")]
        [TestCase("PERSIAN GARDEN COLLECTION", "Persian Garden")]
        [TestCase("Treasure II", "Treasure II")]
        public void TestFormatting(string input, string output)
        {
            var collection = new ItemCollection(input);
            Assert.AreEqual(output, collection.GetFormatted());
        }
    }
}
using System.Linq;
using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.FieldTypes;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class RugMaterialTests
    {
        [TestCase("Wool", "Wool")]
        [TestCase("New Zealand Wool", "New Zealand Wool")]
        [TestCase("Art Silk + Jute", "Art Silk, Jute")]
        [TestCase("Art Silk", "Art Silk")]
        [TestCase("Cotton + Jute", "Cotton, Jute")]
        [TestCase("New Zealand Wool + Polyester", "New Zealand Wool, Polyester")]
        [TestCase("Jute", "Jute")]
        [TestCase("Polyester + Viscose + Wool", "Polyester, Viscose, Wool")]
        [TestCase("Wool+ Viscose", "Wool, Viscose")]
        [TestCase("Imported Wool", "Imported Wool")]
        [TestCase("Polyester", "Polyester")]
        [TestCase("Polyester + Cotton", "Polyester, Cotton")]
        [TestCase("New Zealand Wool + Art silk", "New Zealand Wool, Art Silk")]
        [TestCase("Wool + viscose", "Wool, Viscose")]
        [TestCase("Wool + Viscose", "Wool, Viscose")]
        [TestCase("Viscose", "Viscose")]
        [TestCase("Wool + Polyester", "Wool, Polyester")]
        [TestCase("Cotton + Jute + Polyester", "Cotton, Jute, Polyester")]
        [TestCase("Cotton/Velvet", "Cotton, Velvet")]
        [TestCase("Silk Textured Fabric", "Silk Textured Fabric")]
        [TestCase("Jute, Cotton, Foil, Leather", "Jute, Cotton, Foil, Leather")]
        [TestCase("Jute + Polyester", "Jute, Polyester")]
        [TestCase("Viscose + Wool", "Viscose, Wool")]
        [TestCase("Polyester Pile", "Polyester Pile")]
        [TestCase("Jute + Wool", "Jute, Wool")]
        [TestCase("Jute + Leather", "Jute, Leather")]
        [TestCase("Reclaimed Wood +Steel", "Reclaimed Wood, Steel")]
        [TestCase("Reclaimed Wood", "Reclaimed Wood")]
        [TestCase("Acrylic + Polyester", "Acrylic, Polyester")]
        [TestCase("Imported Wool + Polyester", "Imported Wool, Polyester")]
        [TestCase("Art Silk + Polyester", "Art Silk, Polyester")]
        [TestCase("Wool + Polyester + Rayon", "Wool, Polyester, Rayon")]
        [TestCase("Polypropylene", "Polypropylene")]
        [TestCase("WOOL & VISCOSE", "Wool, Viscose")]
        [TestCase("Reversible Polypropylene", "Reversible Polypropylene")]
        [TestCase("POLYESTER COTTON", "Polyester Cotton")]
        [TestCase("Wool and Viscose", "Wool, Viscose")]
        [TestCase("NEW ZEALAND WOOL COTTON", "New Zealand Wool Cotton")]
        [TestCase("Flat Weave Wool Pile", "Wool Pile")]
        [TestCase("Flat Weave WOOL COTTON", "Wool Cotton")]
        [TestCase("Wool/ Banana Silk", "Wool, Banana Silk")]
        [TestCase("Flat Weave Wool and Banana Silk", "Wool, Banana Silk")]
        [TestCase("Flat Weave Wool Viscose Cotton", "Wool Viscose Cotton")]
        [TestCase("Wool/ Viscose", "Wool, Viscose")]
        [TestCase("Polyester / Cotton", "Polyester, Cotton")]
        [TestCase("Polypropylene/Olefin", "Polypropylene, Olefin")]
        [TestCase("POLYPROPYLENE", "Polypropylene")]
        [TestCase("Flatweave Wool Cotton", "Wool Cotton")]
        [TestCase("Loom Knotted Viscose Pile", "Viscose Pile")]
        [TestCase("BANANA SILK PILE", "Banana Silk Pile")]
        [TestCase("Wool&Viscose", "Wool, Viscose")]
        [TestCase("Viscose And Chenille", "Viscose, Chenille")]
        [TestCase("Silk, Linen & Wool", "Silk, Linen, Wool")]
        [TestCase("Silk Linen Wool", "Silk Linen Wool")]
        [TestCase("Seagrass with Cotton Border and Polypropylene Backing", "Seagrass With Cotton Border")]
        [TestCase("Wool / Cotton / Silk", "Wool, Cotton, Silk")]
        [TestCase("Polypropylene / Jute Back", "Polypropylene")]
        [TestCase("Polyester pile, cotton backing", "Polyester Pile")]
        [TestCase("Nylon", "Nylon")]
        [TestCase("Nylon/Polypropylene", "Nylon, Polypropylene")]
        [TestCase("100% Hard-Twist Wool", "100% Hard-Twist Wool")]
        [TestCase("100% Heat set Polypropylene", "100% Heat-Set Polypropylene")]
        [TestCase("100% Heat-Set Polypropylene", "100% Heat-Set Polypropylene")]
        [TestCase("100% Semi-Worsted New Zealand Wool", "100% Semi-Worsted New Zealand Wool")]
        [TestCase("Hand Spun, Semi Worsted New Zealand wool", "Hand Spun, Semi-Worsted New Zealand Wool")]
        [TestCase("Polypropelene", "Polypropylene")]
        [TestCase("Visocse Pile", "Viscose Pile")]
        [TestCase("Fiber Content:", "Fiber Content")]
        [TestCase("UV Polyester", "UV Polyester")]
        [TestCase("Uv Polyester", "UV Polyester")]
        [TestCase("Uv Polyester", "UV Polyester")]

        [TestCase("100% Premium blended wool", "100% Premium Blended Wool")]
        [TestCase("100% premium blended wool.", "100% Premium Blended Wool")]
        [TestCase("40% Polyester, 40% Wool, 20% Cotton", "40% Polyester, 40% Wool, 20% Cotton")]
        [TestCase("80% Polypropylene 20% Polyester", "80% Polypropylene, 20% Polyester")]
        [TestCase("80% New Zealand Wool , 20% Cotton", "80% New Zealand Wool, 20% Cotton")]
        [TestCase("80% Wool 20% Cotton", "80% Wool, 20% Cotton")]
        [TestCase("80% Wool, 20% Cotton", "80% Wool, 20% Cotton")]
        [TestCase("60% Wool, 20% Art Silk, 20% Cotton", "60% Wool, 20% Art Silk, 20% Cotton")]
        [TestCase("60% Viscose ,20% Wool, 20% Cotton", "60% Viscose, 20% Wool, 20% Cotton")]
        [TestCase("55% Jute, 35% Wool, 10% Cotton", "55% Jute, 35% Wool, 10% Cotton")]
        [TestCase("50% Wool,30% Art Silk,10% Viscose,10% Cotton", "50% Wool, 30% Art Silk, 10% Viscose, 10% Cotton")]
        [TestCase("100% Polyester", "100% Polyester")]
        [TestCase("100% Cotton", "100% Cotton")]
        [TestCase("100 % Silk", "100% Silk")]
        [TestCase("100% Wool Pile", "100% Wool Pile")]
        [TestCase("70% Jute and 30%Wool", "70% Jute, 30% Wool")]
        [TestCase("100% POLYPROPLYENE", "100% Polypropylene")]
        [TestCase("100% HEATSET POLYPROPLENE", "100% Heatset Polypropylene")]
        [TestCase("78-85% WOOL & 15-25% ARTSILK", "")]
        // for now this is implemented as a one-off in Jaipur
        //[TestCase("87% Handspun Wool13% Art Silk", "87% Handspun Wool, 13% Art Silk")]
        public void TestRugMaterialParse(string input, string output)
        {
            var material = new RugMaterial(input);
            var formatted = material.GetFormattedMaterial();
            var content = formatted.Select(x => (x.Value.HasValue ? x.Value.ToString() + "% " : "") + x.Key).Aggregate((a, b) => a + ", " + b);
            Assert.AreEqual(output, content);
        }

        public void TestRugMaterialWithExtraMaterials()
        {
            var material = new RugMaterial("Viscose / Cotton");
            material.AddMaterial("Wool");
            var formatted = material.GetFormattedMaterial();
            var content = formatted.Select(x => (x.Value.HasValue ? x.Value.ToString() + "% " : "") + x.Key).Aggregate((a, b) => a + ", " + b);
            Assert.AreEqual("Viscose, Cotton, Wool", content);
        }

        public void TestRugMaterialWithExtraMaterialsThatAreDuplicates()
        {
            var material = new RugMaterial("Viscose / Cotton");
            material.AddMaterial("Cotton");
            var formatted = material.GetFormattedMaterial();
            var content = formatted.Select(x => (x.Value.HasValue ? x.Value.ToString() + "% " : "") + x.Key).Aggregate((a, b) => a + ", " + b);
            Assert.AreEqual("Viscose, Cotton", content);
        }
    }
}
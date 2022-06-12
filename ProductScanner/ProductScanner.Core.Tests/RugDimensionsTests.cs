using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class RugDimensionsTests
    {
        [TestCase(20, 43, ProductShapeType.Rectangular, "-2043")]
        [TestCase(24, 48, ProductShapeType.Rectangular, "-24")]
        [TestCase(96, 120, ProductShapeType.Oval, "-810OV")]
        [TestCase(96, 96, ProductShapeType.Round, "-88RD")]
        public void TestSkuSuffix(double width, double length, ProductShapeType shape, string expectedSuffix)
        {
            var dimensions = new RugDimensions(width, length, shape);
            var suffix = dimensions.GetSkuSuffix();
            Assert.AreEqual(expectedSuffix, suffix);
        }

        [TestCase(20, 43, ProductShapeType.Rectangular, "-2043S")]
        [TestCase(24, 48, ProductShapeType.Rectangular, "-24S")]
        [TestCase(96, 120, ProductShapeType.Oval, "-810OVS")]
        [TestCase(96, 96, ProductShapeType.Round, "-88RDS")]
        public void TestSkuSuffixScalloped(double width, double length, ProductShapeType shape, string expectedSuffix)
        {
            var dimensions = new RugDimensions(width, length, shape, true);
            var suffix = dimensions.GetSkuSuffix();
            Assert.AreEqual(expectedSuffix, suffix);
        }

        [TestCase(120, 168, ProductShapeType.Rectangular, "10' x 14' Rectangular")]
        [TestCase(30, 120, ProductShapeType.Runner, "2'6\" x 10' Runner")]
        [TestCase(36, 36, ProductShapeType.Round, "3' x 3' Round")]
        public void TestRugDescription(double widthInches, double lengthInches, ProductShapeType shape, string expected)
        {
            var dimensions = new RugDimensions(widthInches, lengthInches, shape);
            Assert.AreEqual(expected, dimensions.GetDescription());
        }

        [TestCase(36, 36, ProductShapeType.Rectangular, 9)]
        public void TestArea(double widthInches, double lengthInches, ProductShapeType shape, double area)
        {
            var dimensions = new RugDimensions(widthInches, lengthInches, shape);
            Assert.AreEqual(area, dimensions.GetArea());
        }
    }
}

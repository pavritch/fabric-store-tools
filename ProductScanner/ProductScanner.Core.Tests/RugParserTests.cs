using System;
using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class RugParserTests
    {
        // once we have dimensions + shape, it's easy to calculate sku suffix
        [TestCase("1'-8\" X 3'-7\"", 20, 43, ProductShapeType.Rectangular)]
        [TestCase("2'-0\" x 3'-0\"", 24, 36, ProductShapeType.Rectangular)]
        [TestCase("2' X 3'-3\"", 24, 39, ProductShapeType.Rectangular)]
        [TestCase("5'-0\" x 8'-0\"", 60, 96, ProductShapeType.Rectangular)]
        [TestCase("5'-3\" x 8'-0\"", 63, 96, ProductShapeType.Rectangular)]
        [TestCase("5'-3\" x 8'-0\"", 63, 96, ProductShapeType.Rectangular)]
        [TestCase("6' X 9'", 72, 108, ProductShapeType.Rectangular)]
        [TestCase("6'-0\" X 9'-0\"", 72, 108, ProductShapeType.Rectangular)]
        [TestCase("8'-6\" X 13'", 102, 156, ProductShapeType.Rectangular)]
        [TestCase("8'6 \" X 11' 2\"", 102, 134, ProductShapeType.Rectangular)]

        //[TestCase("2.8x4.8", 33.6, 57.6, ProductShapeType.Rectangular)]
        [TestCase("5X8", 60, 96, ProductShapeType.Rectangular)]
        [TestCase("3 x 5", 36, 60, ProductShapeType.Rectangular)]
        [TestCase("5-6 x 6-8", 66, 80, ProductShapeType.Rectangular)]
        [TestCase("7-10 x 10-10", 94, 130, ProductShapeType.Rectangular)]

        [TestCase("24\"X39\"", 24, 39, ProductShapeType.Rectangular)]
        [TestCase("16X27", 16, 27, ProductShapeType.Rectangular)]

        [TestCase("4'RND", 48, 48, ProductShapeType.Round)]
        [TestCase("8' RO", 96, 96, ProductShapeType.Round)]
        [TestCase("6'ROUND", 72, 72, ProductShapeType.Round)]
        [TestCase("7'10\" ROUND", 94, 94, ProductShapeType.Round)]
        [TestCase("7'8\" RND", 92, 92, ProductShapeType.Round)]
        [TestCase("10'RD", 120, 120, ProductShapeType.Round)]
        [TestCase("4'- Round", 48, 48, ProductShapeType.Round)]

        [TestCase("3X3ROUND", 36, 36, ProductShapeType.Round)]
        [TestCase("8X8RD", 96, 96, ProductShapeType.Round)]
        [TestCase("7-10 x 7-10 - round", 94, 94, ProductShapeType.Round)]
        // kind of funky
        //[TestCase("7' 10\" x 7' 10 '  Round", 94, 94, ProductShapeType.Round)]
        [TestCase("8 x 8 round", 96, 96, ProductShapeType.Round)]
        [TestCase("8' x 8 Round'", 96, 96, ProductShapeType.Round)]
        [TestCase("7'8\"X 7'8\" RND", 92, 92, ProductShapeType.Round)]
        [TestCase("4'-0\" X 4'-0\" Round", 48, 48, ProductShapeType.Round)]
        [TestCase("6' X 6' Round", 72, 72, ProductShapeType.Round)]
        [TestCase("4'-0\" x 4'-0\" Round", 48, 48, ProductShapeType.Round)]
        [TestCase("3X3ROUND", 36, 36, ProductShapeType.Round)]

        [TestCase("2'2\" x 3'", 26, 36, ProductShapeType.Rectangular)]
        [TestCase("7'6\" x 10'6\"", 90, 126, ProductShapeType.Rectangular)]
        //[TestCase("6\" Swatch", 6, 6, ProductShapeType.Rectangular)]
        [TestCase("8' x 10' Oval", 96, 120, ProductShapeType.Oval)]
        [TestCase("18\" Sample", 18, 18, ProductShapeType.Sample)]
        [TestCase("7'6\" Square", 90, 90, ProductShapeType.Square)]
        [TestCase("6'7\" Round", 79, 79, ProductShapeType.Round)]
        [TestCase("8' x 10' Kidney", 96, 120, ProductShapeType.Kidney)]
        [TestCase("7' Square", 84, 84, ProductShapeType.Square)]
        [TestCase("2' x 4' Hearth", 24, 48, ProductShapeType.Heart)]
        [TestCase("8' Octagon", 96, 96, ProductShapeType.Octagon)]
        [TestCase("3X3 STAR", 36, 36, ProductShapeType.Star)]
        //[TestCase("CUSTOM", 96, 120, ProductShapeType.Kidney)]

        [TestCase("22\" X 28\"", 22, 28, ProductShapeType.Rectangular)]
        [TestCase("13.5\" X 18\"", 13.5, 18, ProductShapeType.Rectangular)]
        [TestCase("1.10 x 3.3", 22, 39, ProductShapeType.Rectangular)]
        [TestCase("6.7 x 9.6", 79, 114, ProductShapeType.Rectangular)]
        [TestCase("7.8 Round", 92, 92, ProductShapeType.Round)]
        [TestCase("7.10 Round", 94, 94, ProductShapeType.Round)]
        [TestCase("8Round", 96, 96, ProductShapeType.Round)]
        [TestCase("7.8 Square", 92, 92, ProductShapeType.Square)]
        [TestCase("7.8 square", 92, 92, ProductShapeType.Square)]
        [TestCase("5'6\" ROUND", 66, 66, ProductShapeType.Round)]
        [TestCase("6'0\" ROUND", 72, 72, ProductShapeType.Round)]
        [TestCase("6'0\" X 6'0\" RND", 72, 72, ProductShapeType.Round)]
        [TestCase("11'8\" X 14'8\"", 140, 176, ProductShapeType.Rectangular)]
        [TestCase("5 x 7.6", 60, 90, ProductShapeType.Rectangular)]
        [TestCase("10 x 13", 120, 156, ProductShapeType.Rectangular)]
        [TestCase("2.6 x 10", 30, 120, ProductShapeType.Runner)]
        [TestCase("8. x 8. Square", 96, 96, ProductShapeType.Square)]
        [TestCase("4. x 5.9", 48, 69, ProductShapeType.Rectangular)]
        [TestCase("22 x 90", 22, 90, ProductShapeType.Runner)]
        [TestCase("2.3X4.5", 27, 53, ProductShapeType.Rectangular)]
        [TestCase("8' X 8' SQR", 96, 96, ProductShapeType.Square)]
        [TestCase("8'0\" X 8'0\" SQR", 96, 96, ProductShapeType.Square)]

        [TestCase("2'6\" x 8'", 30, 96, ProductShapeType.Runner)]
        [TestCase("5'-3\" X 7'-6\" Oval", 63, 90, ProductShapeType.Oval)]
        [TestCase("5'-1\" X 5'-1\" Square", 61, 61, ProductShapeType.Square)]
        [TestCase("2 ft. 3 in. x 8 ft.", 27, 96, ProductShapeType.Runner)]
        [TestCase("8' RD", 96, 96, ProductShapeType.Round)]
        [TestCase("8'6 Square", 102, 102, ProductShapeType.Square)]
        [TestCase("8'Square", 96, 96, ProductShapeType.Square)]
        [TestCase("1'6\" x 1'6\" Square", 18, 18, ProductShapeType.Sample)]
        [TestCase("4'0\" x 4'0\" Round", 48, 48, ProductShapeType.Round)]

        [TestCase("9’-6” X 13’-6” ", 114, 162, ProductShapeType.Rectangular)]
        [TestCase("5X8SHAPED", 60, 96, ProductShapeType.Rectangular)]
        [TestCase("3X8 RUNNER", 36, 96, ProductShapeType.Runner)]

        [TestCase("3X3 HE", 36, 36, ProductShapeType.Heart)]
        [TestCase("1'8\"x2'6\" OVL", 20, 30, ProductShapeType.Oval)]
        [TestCase("2'6\"x6' RNR", 30, 72, ProductShapeType.Runner)]

        [TestCase("5'9\" SQ.", 69, 69, ProductShapeType.Square)]
        public void TestParseDimensions(string input, double widthInches, double lengthInches, ProductShapeType shape)
        {
            var result = RugParser.ParseDimensions(input);
            Assert.IsTrue(Math.Abs(widthInches - result.WidthInInches) < 0.1);
            Assert.IsTrue(Math.Abs(lengthInches - result.LengthInInches) < 0.1);
            Assert.AreEqual(shape, result.Shape);
        }

        [TestCase("6' X 6' Round (Scalloped)", 72, 72, ProductShapeType.Round)]
        [TestCase("2'-3\" X 8' (Scalloped)", 27, 96, ProductShapeType.Runner)]
        [TestCase("6' X 6' Square (Scalloped)", 72, 72, ProductShapeType.Square)]
        [TestCase("6' X 6' Round (Scalloped)", 72, 72, ProductShapeType.Round)]
        public void TestParseDimensionsForScalloped(string input, double widthInches, double lengthInches, ProductShapeType shape)
        {
            var result = RugParser.ParseDimensions(input, shape);
            Assert.IsTrue(result.IsScalloped);
            Assert.IsTrue(Math.Abs(widthInches - result.WidthInInches) < 0.1);
            Assert.IsTrue(Math.Abs(lengthInches - result.LengthInInches) < 0.1);
            Assert.AreEqual(shape, result.Shape);
        }

        [TestCase("2.6x9", ProductShapeType.Oval, 30, 108, ProductShapeType.Oval)]
        public void TestParseDimensionsWithShape(string input, ProductShapeType shapeInput, double widthInches, double lengthInches, ProductShapeType shape)
        {
            var result = RugParser.ParseDimensions(input, shapeInput);
            Assert.IsTrue(Math.Abs(widthInches - result.WidthInInches) < 0.1);
            Assert.IsTrue(Math.Abs(lengthInches - result.LengthInInches) < 0.1);
            Assert.AreEqual(shape, result.Shape);
        }
    }
}
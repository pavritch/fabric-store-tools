using System;
using System.Data.OleDb;
using System.Linq;
using AllstateFloral;
using NUnit.Framework;
using ProductScanner.Core;

namespace Vendors.Tests.York
{
    public class AllstateFloralDimensionTests
    {
        [TestCase("5.5\" Squirrel", 0, 5.5, 0, 0)]
        [TestCase("15\" Mr. Frog Holding Heart", 0, 15, 0, 0)]
        [TestCase("4.5\"Hx12\"L Polyresin Shell", 0, 4.5, 12, 0)]
        [TestCase("13.5\"Wx50.5\"L Moss Table", 13.5, 0, 50.5, 0)]
        [TestCase("22\"Wx5'L Paper Birch Sheet", 22, 0, 60, 0)]
        [TestCase("6.25\"Hx4.5\"D Bunny Cement Pot", 0, 6.25, 0, 4.5)]
        [TestCase("2\"Hx12\"Wx12\"L Moss Tile", 12, 2, 12, 0)]
        [TestCase("3.75\"Wx5.75\"L Paper Egg", 3.75, 0, 5.75, 0)]
        [TestCase("19\"Dx23.5\"H Glass Mosaic", 0, 23.5, 0, 19)]
        [TestCase("5' Bead Garland", 0, 60, 0, 0)]
        [TestCase("5.5\"D Dried Look Boxwood Ball", 0, 0, 0, 5.5)]
        public void TestParse(string input, double width, double height, double length, double depth)
        {
            var parser = new AllstateParser();
            var result = parser.Parse(input);

            Assert.AreEqual(width, result.Width);
            Assert.AreEqual(height, result.Height);
            Assert.AreEqual(length, result.Length);
            Assert.AreEqual(depth, result.Depth);
        }
    }

    public class AllstateParser
    {
        public AllstateDimensions Parse(string description)
        {
            var lastQuote = Math.Max(description.LastIndexOf("\""), description.LastIndexOf("'"));
            var dimensions = description.Substring(0, lastQuote + 2);
            if (!dimensions.Contains("x") && !dimensions.Contains("D") && !dimensions.Contains("L")) 
                return new AllstateDimensions(string.Empty, dimensions.Trim(), string.Empty, string.Empty);
            return GetDimensions(dimensions);
        }

        private AllstateDimensions GetDimensions(string description)
        {
            var split = description.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            var dimensionsText = split.First();
            var splitDims = dimensionsText.Split(new[] {'x'}).ToList();
            return new AllstateDimensions(splitDims.FirstOrDefault(x => x.Contains("W")),
                splitDims.FirstOrDefault(x => x.Contains("H")),
                splitDims.FirstOrDefault(x => x.Contains("L")),
                splitDims.FirstOrDefault(x => x.Contains("D")));
        }
    }
}
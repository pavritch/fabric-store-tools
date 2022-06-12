using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Utilities.Extensions;

namespace Utilities.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("7.75 rd", 7.75)]
        [TestCase("13", 13)]
        public void TestOnlyNumericPart(string input, double expected)
        {
            Assert.AreEqual(expected, input.TakeOnlyNumericPart());
        }

        [TestCase(@"\u003Cdiv class=\u0022product-dimensions\u0022\u003E\n                \u003Cp\u003EDimensions: 13\u0022 Dia x 18\u0022 H\u003C\/p\u003E\n    \n        \n        \n        \n        \n        \u003C\/div\u003E", 
            @"<div class=""product-dimensions""><p>Dimensions: 13"" Dia x 18"" H</p></div>")]
        [TestCase("\\u0022product-dimensions\\u0022\\u003E\\n                \\u003Cp\\u003EDimensions: 16\\u0022 Dia x 15\\u0022 H\\u003C\\/p\\u003E\\n    \\n        \\n        \\n        \\n        \\n        \\u003C\\/div\\u003E\"", 
            @"<div class=""product-dimensions""><p>Dimensions: 13"" Dia x 18"" H</p></div>")]
        public void TestRemoveUnicode(string input, string expected)
        {
            Assert.AreEqual(expected, input.ReplaceUnicode());
        }

        [TestCase("14132 - DESANI - 12 x 24", "DESANI")]
        [TestCase("21671 Aniceta IV 20x27", "Aniceta IV")]
        public void TestRemoveAfterLast(string input, string expected)
        {
            var res = input.RemovePattern(@"^\d+")
                .Trim('-', ' ')
                .RemovePattern(@"\d+x\d+$")
                .RemovePattern(@"\d+ x \d+$")
                .Trim('-', ' ');
            Assert.AreEqual(expected, res);
        }
    }

    [TestFixture]
    public class ListExtensionTests
    {
        [Test]
        public void TestIndexOfNextNumber()
        {
            var list = new List<string> {"20", "New", "Zealand", "Wool", "40", "Cotton", "40", "Polyester"};

            Assert.AreEqual(4, list.IndexOfNextNumber(1));
            Assert.AreEqual(0, list.IndexOfNextNumber(0));
            Assert.AreEqual(6, list.IndexOfNextNumber(6));

            Assert.AreEqual(-1, list.IndexOfNextNumber(8));
        }
    }
}

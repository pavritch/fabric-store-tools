using System.Collections.Generic;
using FabricUpdater.Core;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
        [Test]
        public void TestFormatContentTypesWithUnsortedValues()
        {
            var dict = new Dictionary<string, int>
            {
                { "Rayon", 21 },
                { "Cotton", 60 },
                { "Viscose", 10 },
                { "Acrylic", 9 }
            };
            Assert.AreEqual("60% Cotton, 21% Rayon, 10% Viscose, 9% Acrylic", dict.FormatContentTypes());
        }

        [Test]
        public void TestFormatContentTypesWithLessThan100()
        {
            var dict = new Dictionary<string, int>
            {
                { "Rayon", 21 },
                { "Cotton", 60 },
                { "Viscose", 10 },
                { "Acrylic", 5 }
            };
            Assert.AreEqual("60% Cotton, 21% Rayon, 10% Viscose, 5% Acrylic, 4% Other", dict.FormatContentTypes());
        }

        [Test]
        public void TestFormatContentTypesWithLessThan100AndExistingOther()
        {
            var dict = new Dictionary<string, int>
            {
                { "Rayon", 21 },
                { "Cotton", 60 },
                { "Other", 10 },
                { "Acrylic", 5 }
            };
            Assert.AreEqual("60% Cotton, 21% Rayon, 5% Acrylic, 14% Other", dict.FormatContentTypes());
        }

        [Test]
        public void TestFormatContentTypesWithOther()
        {
            var dict = new Dictionary<string, int>
            {
                { "Acrylic", 9 },
                { "Rayon", 21 },
                { "Other", 10 },
                { "Cotton", 60 }
            };
            Assert.AreEqual("60% Cotton, 21% Rayon, 9% Acrylic, 10% Other", dict.FormatContentTypes());
        }

        [Test]
        public void TestFormatContentTypesWithGreaterThan100()
        {
            var dict = new Dictionary<string, int>
            {
                { "Cotton", 100 },
                { "Rayon", 100 }
            };
            Assert.AreEqual("100% Cotton", dict.FormatContentTypes());
        }
    }
}
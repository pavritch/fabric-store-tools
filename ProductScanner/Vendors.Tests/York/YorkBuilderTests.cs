using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;
using York;
using York.Details;
using York.Metadata;

namespace Vendors.Tests.York
{
    public class YorkFileDownloadTests
    {
        //private Mock<IStorageProvider<YorkVendor>> _storageProvider;
        //private YorkInventoryFileDownloader _downloader;

        [Test]
        public void Test()
        {
            //_storageProvider = new Mock<IStorageProvider<YorkVendor>>();
            //_downloader = new YorkInventoryFileDownloader(_storageProvider.Object);

            //_storageProvider.Setup(x => x.GetStaticFolder()).Returns(@"C:\Dropbox\Dev\StaticRoot\InsideFabric\York");
            //_downloader.Download();
        }
    }

    public class YorkBuilderTests
    {
        private YorkProductBuilder _builder = new YorkProductBuilder(new YorkPriceCalculator(), null);


        [TestCase("27 in. x 27ft. = 60.75 sq.ft", 27, 324)]
        [TestCase("27in. x 27ft. = 60.75 sq.ft", 27, 324)]
        [TestCase("27.5in. x 27.2 ft. = 60.75 sq.ft", 27.5, 326.4)]
        [TestCase("27.5in. x 27.2  ft. = 60.75 sq.ft", 27.5, 326.4)]
        [TestCase("27.5 in. x 27.2ft. = 60.75 sq.ft", 27.5, 326.4)]
        [TestCase("20.5 in. x 33ft. = 56 sq.ft", 20.5, 396)]
        [TestCase("20 in. x 24 ft. = 56 sq.ft", 20, 288)]
        [TestCase("33 ft. x 21 in.", 21, 396)]
        [TestCase("20 1/2 in. x 33ft. = 56 sq.ft", 20.5, 396)]
        [TestCase("mural dimensions: 11 feet x 54 inches", 54, 132)]
        [TestCase("single spool dimension 4.5 inches x 15 feet; design repeat: 12 inches", 4.5, 180)]
        [TestCase("single spool dimension 6.75 inches x 15 feet; design repeat: 20.5 inches: 4", 6.75, 180)]
        [TestCase("10 1/2'h x 6'w", 72, 126)]
        [TestCase("10 1/2'w x 6'h", 126, 72)]
        [TestCase("15'w x 9'h", 180, 108)]
        [TestCase("120\" x 72\"", 120, 72)]
        [TestCase("120\"w x 72\"h", 120, 72)]
        public void TestDimensions(string start, double width, double length)
        {
            var data = new ScanData();
            data[ScanField.Dimensions] = start;

            var result = _builder.Build(data) as FabricProduct;
            //Assert.AreEqual(width, result.Dimensions.Width);
            //Assert.AreEqual(length, result.Dimensions.Length);
        }

        [TestCase("63.5 x 44", 63.5, 44)]
        [TestCase("47 5/8 x 47 5/8", 47.625, 47.625)]
        [TestCase("34 3/4x66 3/4-OD 74x42", 47.625, 47.625)]
        [TestCase("34 ¼ X 37 ¼", 34.25, 37.25)]
        [TestCase("28 1/8 X 62 1/8", 28.125, 62.125)]
        public void TestDimensionsArtAndFrame(string input, double expectedLength, double expectedWidth)
        {
            input = input.Replace("¼", "1/4");
            var dims = input.ToLower().Split('x').Select(x => x.Trim()).ToList();

            var length = ExtensionMethods.MeasurementFromFraction(dims.First()).ToDoubleSafe();
            var width = ExtensionMethods.MeasurementFromFraction(dims.Last()).ToDoubleSafe();

            Assert.AreEqual(expectedLength, length);
            Assert.AreEqual(expectedWidth, width);
        }

        [Test]
        public void TestCaptureDimensions()
        {
            var input = "21014 Clair Font II 40x60";
            var result = input.CaptureWithinMatchedPattern(@"(?<capture>(\d+x\d+))");
        }
    }
}

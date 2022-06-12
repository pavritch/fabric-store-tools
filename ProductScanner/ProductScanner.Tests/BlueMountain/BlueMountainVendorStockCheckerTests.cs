using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.BlueMountain;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.BlueMountain
{
    [TestFixture]
    public class BlueMountainVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private BlueMountainStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new BlueMountainStockChecker();
        }

        [TestCase("bc1580012", 200, StockCheckStatus.InStock, 344f)]
        [TestCase("bc1580012", 400, StockCheckStatus.PartialStock, 344f)]
        [TestCase("bc1586559", 5, StockCheckStatus.OutOfStock, 0)]
        [TestCase("bc1585896", 5, StockCheckStatus.InStock, 32)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity}, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("BlueMountain\\TestFiles\\{0}.html", filename));
        }
    }
}
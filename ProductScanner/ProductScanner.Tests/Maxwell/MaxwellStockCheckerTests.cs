using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Maxwell;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Maxwell
{
    [TestFixture]
    public class MaxwellStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private MaxwellStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new MaxwellStockChecker();
        }

        [TestCase("260040", 5, StockCheckStatus.InStock, 14.5f)]
        [TestCase("ag7325", 5, StockCheckStatus.InStock, 8.8f)]
        [TestCase("ag7325", 10, StockCheckStatus.PartialStock, 8.8f)]
        [TestCase("ae3019", 5, StockCheckStatus.OutOfStock, 0f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Maxwell\\TestFiles\\{0}.html", filename));
        }
    }
}
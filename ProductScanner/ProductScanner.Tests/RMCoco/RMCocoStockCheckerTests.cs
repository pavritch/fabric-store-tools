using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.RMCoco;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.RMCoco
{
    [TestFixture]
    public class RMCocoStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private RmCocoStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new RmCocoStockChecker();
        }

        [TestCase("1053cb_257", 2, StockCheckStatus.OutOfStock, 0f)]
        [TestCase("1075cb_270", 5, StockCheckStatus.InStock, 96.87f)]
        [TestCase("1075cb_270", 100, StockCheckStatus.PartialStock, 96.87f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("RMCoco\\TestFiles\\{0}.html", filename));
        }
    }
}
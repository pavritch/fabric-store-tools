using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Scalamandre;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Scalamandre
{
    [TestFixture]
    public class ScalamandreStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private ScalamandreStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new ScalamandreStockChecker();
        }

        [TestCase("16535-001", 2, StockCheckStatus.OutOfStock, 0f)]
        [TestCase("16529-001", 5, StockCheckStatus.InStock, 100f)]
        [TestCase("16529-001", 110, StockCheckStatus.PartialStock, 100f)]
        [TestCase("30157M-011", 5, StockCheckStatus.OutOfStock, 0f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Scalamandre\\TestFiles\\{0}.html", filename));
        }
    }
}
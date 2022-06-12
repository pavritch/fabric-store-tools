using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Stout;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Stout
{
    [TestFixture]
    public class StoutStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private StoutStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new StoutStockChecker();
        }

        [TestCase("gent-1", 2, StockCheckStatus.OutOfStock, 0f)]
        [TestCase("filo-7", 5, StockCheckStatus.InStock, 18f)]
        [TestCase("filo-7", 25, StockCheckStatus.PartialStock, 18f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Stout\\TestFiles\\{0}.html", filename));
        }
    }
}
using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.RobertAllen;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.RobertAllen
{
    [TestFixture]
    public class RobertAllenStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private RobertAllenStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new RobertAllenStockChecker();
        }

        [TestCase("105654", 2, StockCheckStatus.OutOfStock, 0f)]
        [TestCase("105797", 5, StockCheckStatus.InStock, 55f)]
        [TestCase("105983", 100, StockCheckStatus.PartialStock, 29f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("RobertAllen\\TestFiles\\{0}.html", filename));
        }
    }
}
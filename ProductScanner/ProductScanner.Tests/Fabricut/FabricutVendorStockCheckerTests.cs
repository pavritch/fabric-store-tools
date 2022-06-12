using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Fabricut;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Fabricut
{
    [TestFixture]
    public class FabricutVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private FabricutStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new FabricutStockChecker();
        }

        [TestCase("3555005", 5, StockCheckStatus.Discontinued, 0)]
        [TestCase("2101901", 5, StockCheckStatus.InStock, 58f)]
        [TestCase("3753003", 200, StockCheckStatus.PartialStock, 161f)]
        [TestCase("0080703", 5, StockCheckStatus.OutOfStock, 0)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Fabricut\\TestFiles\\{0}.html", filename));
        }
    }
}
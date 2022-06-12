using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Duralee;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Duralee
{
    [TestFixture]
    public class DuraleeVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private DuraleeStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new DuraleeStockChecker();
        }

        [TestCase("14041-430", "stock-14041", 5, StockCheckStatus.PartialStock, 3)]
        [TestCase("14041-132", "stock-14041", 5, StockCheckStatus.OutOfStock, 0)]
        [TestCase("14041-83", "stock-14041", 5, StockCheckStatus.InStock, 5.75f)]
        [TestCase("14295-450", "stock-14295", 5, StockCheckStatus.OutOfStock, 0)]
        public async void TestCorrectStatus(string mpn, string filename, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(LoadFile(filename)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Duralee\\TestFiles\\{0}.html", filename));
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.SilverState;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.SilverState
{
    [TestFixture]
    public class SilverStateStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private SilverStateStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new SilverStateStockChecker();
        }

        [TestCase("40843", 2, StockCheckStatus.OutOfStock, 0f, "6/16/2014")]
        [TestCase("40792", 5, StockCheckStatus.InStock, 35f, null)]
        [TestCase("40792", 50, StockCheckStatus.PartialStock, 35f, null)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand, string expectedDate)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
            Assert.AreEqual(ParseNextAvailable(expectedDate), results.MoreExpectedOn);
        }

        private DateTime? ParseNextAvailable(string nextAvailable)
        {
            if (nextAvailable == null) return null;
            return DateTime.Parse(nextAvailable);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("SilverState\\TestFiles\\{0}.html", filename));
        }
    }
}
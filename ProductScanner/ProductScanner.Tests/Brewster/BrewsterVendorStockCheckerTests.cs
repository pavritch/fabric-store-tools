using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Brewster;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Brewster
{
    [TestFixture]
    public class BrewsterVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private BrewsterStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new BrewsterStockChecker();
        }

        [TestCase("174-58883", 5, StockCheckStatus.OutOfStock, null)]
        [TestCase("395108", 5, StockCheckStatus.NotSupported, null)]
        [TestCase("982-75363", 5, StockCheckStatus.Discontinued, null)]
        [TestCase("wb1033", 5, StockCheckStatus.Discontinued, null)]
        [TestCase("wa2268", 5, StockCheckStatus.OutOfStock, "6/27/2014")]
        [TestCase("wb1039", 5, StockCheckStatus.InStock, null)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, string nextDate)
        {
            _mockWebClient.Setup(x => x.PostValuesTaskAsync(It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity}, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(ParseNextAvailable(nextDate), results.MoreExpectedOn);
        }

        private DateTime? ParseNextAvailable(string nextAvailable)
        {
            if (nextAvailable == null) return null;
            return DateTime.Parse(nextAvailable);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Brewster\\TestFiles\\{0}.html", filename));
        }
    }
}
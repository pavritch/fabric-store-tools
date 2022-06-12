using System.Collections.Specialized;
using System.IO;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Core.WebsiteEntities;
using FabricUpdater.Vendors.ClarenceHouse;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.ClarenceHouse
{
    [TestFixture]
    public class ClarenceHouseStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private Mock<IPlatformDatabase> _mockDataContext;
        private ClarenceHouseStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _mockDataContext = new Mock<IPlatformDatabase>();
            _mockDataContext.Setup(x => x.GetCredentialAsync(63)).ReturnsAsync(new VendorCredential());
            _stockChecker = new ClarenceHouseStockChecker(_mockDataContext.Object);
        }

        [TestCase("34348-13", 5, StockCheckStatus.PartialStock, 4)]
        [TestCase("34348-13", 3, StockCheckStatus.InStock, 4)]
        [TestCase("cyec-2005", 5, StockCheckStatus.OutOfStock, 0)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.PostValuesTaskAsync(It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .ReturnsAsync(LoadFile("list-" + mpn));
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>()))
                .ReturnsAsync(LoadFile(mpn));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity}, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("ClarenceHouse\\TestFiles\\{0}.html", filename));
        }
    }
}
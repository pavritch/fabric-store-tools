using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Core.WebsiteEntities;
using FabricUpdater.Vendors.RalphLauren;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.RalphLauren
{
    [TestFixture]
    public class RalphLaurenStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private Mock<IPlatformDatabase> _mockDataContext;
        private RalphLaurenStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _mockDataContext = new Mock<IPlatformDatabase>();
            _mockDataContext.Setup(x => x.GetCredentialAsync(52)).ReturnsAsync(new VendorCredential());
            _stockChecker = new RalphLaurenStockChecker(_mockDataContext.Object);
        }

        [TestCase("lcf60319f", 5, StockCheckStatus.InStock, 6)]
        [TestCase("lfy11733f", 3, StockCheckStatus.OutOfStock, 0)]
        [TestCase("lfy60110f", 5, StockCheckStatus.InStock, 48)]
        [TestCase("rll60952l", 5, StockCheckStatus.OutOfStock, 0)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.PostValuesTaskAsync(It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .Returns(Task.FromResult(LoadFile("list-" + mpn)));
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity}, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("RalphLauren\\TestFiles\\{0}.html", filename));
        }
    }
}
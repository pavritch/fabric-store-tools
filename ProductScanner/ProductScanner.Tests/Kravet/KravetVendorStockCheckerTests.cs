using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Core.WebsiteEntities;
using FabricUpdater.Vendors.Kravet;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Kravet
{
    [TestFixture]
    public class KravetVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private Mock<IPlatformDatabase> _mockDataContext;
        private KravetStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _mockDataContext = new Mock<IPlatformDatabase>();
            _mockDataContext.Setup(x => x.GetCredentialAsync(5))
                .Returns(Task.FromResult(new VendorCredential()));
            _stockChecker = new KravetStockChecker(_mockDataContext.Object);
        }

        [TestCase("discontinued", "33462.11.0", 5, StockCheckStatus.Discontinued)]
        [TestCase("success", "33462.11.0", 5, StockCheckStatus.InStock)]
        [TestCase("failure", "33462.11.0", 5, StockCheckStatus.OutOfStock)]
        [TestCase("invalid", "33462.11.0", 5, StockCheckStatus.InvalidProduct)]
        public async void TestCorrectStatus(string filename, string mpn, int quantity, StockCheckStatus expectedStatus)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(filename)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Kravet\\TestFiles\\{0}.html", filename));
        }
    }
}
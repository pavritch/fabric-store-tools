using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.Interfaces;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Core.WebsiteEntities;
using FabricUpdater.Vendors.FSchumacher;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.FSchumacher
{
    [TestFixture]
    public class FSchumacherVendorStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private Mock<IPlatformDatabase> _mockDataContext;
        private FSchumacherStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _mockDataContext = new Mock<IPlatformDatabase>();
            _mockDataContext.Setup(x => x.GetCredentialAsync(30)).ReturnsAsync(new VendorCredential());
            _stockChecker = new FSchumacherStockChecker(_mockDataContext.Object);
        }

        [TestCase("5003300", 5, StockCheckStatus.InStock)]
        [TestCase("5003400", 200, StockCheckStatus.InStock)]
        [TestCase("5004381", 5, StockCheckStatus.OutOfStock)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("FSchumacher\\TestFiles\\{0}.html", filename));
        }
    }
}
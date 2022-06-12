using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.Norwall;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Norwall
{
    [TestFixture]
    public class NorwallStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private NorwallStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new NorwallStockChecker();
        }

        [TestCase("35200", 2, StockCheckStatus.InStock)]
        [TestCase("35201", 5, StockCheckStatus.OutOfStock)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus)
        {
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Norwall\\TestFiles\\{0}.html", filename));
        }
    }
}
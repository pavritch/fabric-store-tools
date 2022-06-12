using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.StockChecks;
using FabricUpdater.Core.StockChecks.DTOs;
using FabricUpdater.Vendors.York;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.York
{
    [TestFixture]
    public class YorkStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private YorkStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync("http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WBRAND3+webrtn=BRANDD3+ml=LANSA:XHTML+partition=YWP+language=ENG+sid="))
                .Returns(Task.FromResult(LoadFile("search")));
            _stockChecker = new YorkStockChecker();
        }

        [TestCase("255750", 2, StockCheckStatus.InStock, 156f)]
        [TestCase("255750", 200, StockCheckStatus.PartialStock, 156f)]
        [TestCase("255859", 5, StockCheckStatus.InStock, 1108f)]
        [TestCase("255934", 25, StockCheckStatus.OutOfStock, 0f)]
        public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        {
            _mockWebClient.Setup(x => x.PostValuesTaskAsync(It.IsAny<string>(), It.IsAny<NameValueCollection>()))
                .Returns(Task.FromResult(LoadFile("list-" + mpn)));
            _mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(LoadFile(mpn)));
            var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity }, _mockWebClient.Object);
            Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        }

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("York\\TestFiles\\{0}.html", filename));
        }
    }
}
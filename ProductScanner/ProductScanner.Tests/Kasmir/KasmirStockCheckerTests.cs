using System.IO;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Vendors.Kasmir;
using Moq;
using NUnit.Framework;

namespace FabricUpdater.Vendors.Tests.Kasmir
{
    [TestFixture]
    public class KasmirStockCheckerTests
    {
        private Mock<IWebClientEx> _mockWebClient;
        private KasmirStockChecker _stockChecker;

        [SetUp]
        public void Init()
        {
            _mockWebClient = new Mock<IWebClientEx>();
            _stockChecker = new KasmirStockChecker();
        }

        // maybe come back to these tests, but for now too much mocking involved
        //[TestCase("10291-vitality-daybreak", 5, StockCheckStatus.InStock, 35.75f)]
        //[TestCase("10305-flair-mineral", 5, StockCheckStatus.InStock, 29f)]
        //[TestCase("7781-denim", 5, StockCheckStatus.PartialStock, 3.5f)]
        //[TestCase("94199-bagel", 5, StockCheckStatus.OutOfStock, 0f)]
        //[TestCase("74292-gooseberry", 5, StockCheckStatus.InStock, 0f)]
        //public async void TestCorrectStatus(string mpn, int quantity, StockCheckStatus expectedStatus, float quantityOnHand)
        //{
            //_mockWebClient.Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>())).ReturnsAsync(LoadFile(mpn));
            //var results = await _stockChecker.CheckStockAsync(new StockCheck {MPN = mpn, Quantity = quantity });
            //Assert.AreEqual(expectedStatus, results.StockCheckStatus);
            //Assert.AreEqual(quantityOnHand, results.QuantityOnHand);
        //}

        private string LoadFile(string filename)
        {
            return File.ReadAllText(string.Format("Kasmir\\TestFiles\\{0}.html", filename));
        }
    }
}
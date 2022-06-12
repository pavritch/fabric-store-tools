using ProductScanner.Core.VendorTesting.TestTypes;

namespace Stout.Tests
{
    public class StoutValidProductFileTest : ValidUrlTest<StoutVendor>
    {
        public StoutValidProductFileTest()
            : base(@"http://www.estout.com/downloadfiles/downloader.asp?name=2015_Active_Fabrics_and_Trimmings.xls&d=e&force=true&file=product_downloads\2015_Active_Fabrics_and_Trimmings.xls") { }
    }
}

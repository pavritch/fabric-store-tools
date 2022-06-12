using ProductScanner.Core.VendorTesting.TestTypes;

namespace AndrewMartin.Tests
{
    public class CheckSearchPage : ValidUrlTest<AndrewMartinVendor>
    {
        public CheckSearchPage() : base("http://www.andrewmartin.co.uk/fabric-showroom/fabric-designs.php") { }
    }

    public class CheckDetailsPage : ValidUrlTest<AndrewMartinVendor>
    {
        public CheckDetailsPage() : base("http://www.andrewmartin.co.uk/montgomery-fabric.php") { }
    }
}
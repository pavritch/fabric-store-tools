using System.Configuration;
using ProductScanner.Core;

namespace MoesHome
{
    public class MoesHomeVendor : HomewareVendor
    {
        public MoesHomeVendor() : base(176, "Moes Home", "MC")
        {
            PublicUrl = "http://www.moeshomecollection.com/";
            UsesIMAP = true;
            UsesStaticFiles = true;
            //DiscoveryNotes = "All data comes from public site after login";

            // Ecommerce cost is stocking dealer cost  x 1.12. and the MAP is ecommerce cost x 2.
            // Images can be downloaded from the following link:  https://www.dropbox.com/sh/nu6tyilb422r1lg/AACr-45eTMU5ndNQDktNwOpwa?dl=0
            // Products with -M2 or M4, wholesale price is by piece, with a purchase minimum of 2 or 4
            // Inventory comes from emailed spreadsheet

            // I don't think we need any login info - looks like everything is through the spreadsheets
        }
    }
}

using NUnit.Framework;
using ProductScanner.Core.Scanning.Products.FieldTypes;

namespace ProductScanner.Core.Tests
{
    [TestFixture]
    public class BookTests
    {
        [TestCase("SOMETHING VOL1", "Something Vol 1", 0, "")]
        [TestCase("Book #3002", "", "3002")]
        [TestCase("Book #D3009", "", "3009")]
        [TestCase("D2418 Brookside Coll", "D2418 Brookside")]
        [TestCase("5361 Comp. Cotton Col.Vol IV", "5361 Comp. Cotton Vol IV")]
        [TestCase("0033 Wide Line Vol. II", "Wide Line Vol II", 0, "0033")]
        [TestCase("0050 Sheerline Sheers:Volii", "0050 Sheerline Sheers Vol II")]
        [TestCase("0061 The William Wegman Coll.", "0061 The William Wegman")]

        // Fabricut
        [TestCase("CHROMATICS XXI", "Chromatics XXI", 5239, "5239")]
        [TestCase("INSPRIATIONS VI", "Inspirations VI", 0, "")]
        [TestCase("ID DRAPES NOT SAMPLED IN A BOOK", "", 0, "")]
        [TestCase("SAMPLED IN SHOWROOMS ONLY", "", 0, "")]
        [TestCase("VERVAIN SAMPLED IN SHOWROOMS ONLY", "", 0, "")]
        [TestCase("THIS ITEM IS NOT IN A BOOK", "", 0, "")]
        [TestCase("FABRICUT NO SAMPLE BOOK LOADED", "", 0, "")]
        [TestCase("** NO SAMPLE BOOK LOADED **", "", 0, "")]
        [TestCase("JC PENNEY NO SAMPLE BOOK LOADED", "", 0, "")]
        [TestCase("SAVILE ROW - MENSWEAR", "Savile Row - Menswear", 0, "")]
        [TestCase("FABRICUT/TREND TAPE GALLERY", "Fabricut/Trend Tape Gallery", 0, "")]
        [TestCase("FABRICUT/TREND TAPE GALLERY VOL. II", "Fabricut/Trend Tape Gallery Vol II", 0, "")]
        [TestCase("COLOR STUDIO VOLUME III", "Color Studio Volume III", 0, "")]
        [TestCase("BELLA DURA WEB ONLY", "Bella Dura", 0, "")]
        [TestCase("JOANN WEB ONLY", "Joann", 0, "")]
        [TestCase("SOLIDS BY COLOR 06/2013", "", 0, "")]
        [TestCase("VIGNETTES VOL. XIII (7 BOOKS)", "", 0, "")]
        [TestCase("COLOR STUDIO CHENILLES VOL. III", "", 0, "")]
        [TestCase("EXPRESSIONS VOL.IV", "", 0, "")]
        [TestCase("ORGANDY & OLAY", "", 0, "")]
        [TestCase("OUTDOOR PRINTS & VELVETS WEB ONLY", "", 0, "")]
        [TestCase("ATTENTION TO DETAIL TRIM COLLECTION", "", 0, "")]
        [TestCase("SAMPLED AS ROADLINE ONLY", "", 0, "")] // I guess this is a book name?
        [TestCase("COLORATIONS 2", "", 0, "")]
        [TestCase("ASTONISH / DESTINY", "", 0, "")]
        [TestCase("CHARLOTTE MOSS VOLUME I & II", "", 0, "")]
        [TestCase("COLLIER CAMPBELL EXOTIC INSPIRATION GALL", "", 0, "")]
        [TestCase("SILK N SPECTRA 54", "", 0, "")]
        [TestCase("S HARRIS SHOWROOM ONLY PRODUCT", "", 0, "")] // emailed Ben
        [TestCase("EMPRESS-ORGANZA", "", 0, "")]

        // Greenhouse
        [TestCase("L09: Classic Leather", "Classic Leather", 0, "L09")]
        [TestCase("C73: greenhouseColorStudio", "Classic Leather", 0, "L09")]
        [TestCase("C47: greenhouseColorbook", "Classic Leather", 0, "L09")]
        [TestCase("399: Faux Leather", "Classic Leather", 0, "L09")]
        [TestCase("404: Contract &amp; Hospitality", "Classic Leather", 0, "L09")]
        [TestCase("C11: Contract &amp; Residential V", "Classic Leather", 0, "L09")]
        public void TestFormatting(string bookInput, int numberInput, string bookOutput, string numberOutput)
        {
            var book = new BookInfo(bookInput, numberInput);
            Assert.AreEqual(bookOutput, book.GetName());
            Assert.AreEqual(numberOutput, book.GetNumber());
        }
    }
}
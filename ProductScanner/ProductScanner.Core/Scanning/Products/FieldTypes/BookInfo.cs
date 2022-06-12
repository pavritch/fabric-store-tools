using ExcelLibrary.BinaryFileFormat;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class BookInfo
    {
        private readonly string _book;
        private readonly int _bookNumber;

        public BookInfo(string book, int bookNumber = 0)
        {
            _book = book;
            _bookNumber = bookNumber;
        }

        public string GetName()
        {
            var book = _book;

            book = book.TitleCase().RomanNumeralCase();

            book = book.Replace("Vol.", "Vol");

            book = book.Replace("VOL1", "Vol 1");
            book = book.Replace("VOL2", "Vol 2");
            book = book.Replace("VOL3", "Vol 3");

            book = book.RemovePattern("Coll.$");
            book = book.RemovePattern("Collection.$");

            book = book.RemovePattern("Coll$");
            book = book.RemovePattern("Collection$");

            book = book.ReplaceWholeWord("Coll.", "");
            book = book.ReplaceWholeWord("Col.", "");

            book = book.ReplaceWholeWord("Coll", "");
            book = book.ReplaceWholeWord("Col", "");
            book = book.ReplaceWholeWord("Collection", "");


            book = book.ReplaceWholeWord("-", " ");

            // TODO: Should go into some kind of global misspelling/abbreviation list
            book = book.ReplaceWholeWord("Dk", "Dark");
            book = book.ReplaceWholeWord("Lt", "Light");
            book = book.ReplaceWholeWord("Neut", "Neutral");
            book = book.ReplaceWholeWord("Plds", "Plaids");
            book = book.ReplaceWholeWord("Strps", "Stripes");
            book = book.ReplaceWholeWord("Blu", "Blue");
            book = book.ReplaceWholeWord("Brwn/Gld", "Brown/Gold");
            book = book.ReplaceWholeWord("Pl", "Plum");

            return book.Trim();
        }

        public string GetNumber()
        {
            if (_bookNumber == 0)
            {
                // look in _book first
                return string.Empty;
            }

            return string.Empty;
        }
    }
}
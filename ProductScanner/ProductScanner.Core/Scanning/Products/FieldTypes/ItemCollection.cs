using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class ItemCollection
    {
        private readonly string _value;

        public ItemCollection(string value)
        {
            _value = value;
        }

        public string GetFormatted()
        {
            return _value.TitleCase().Replace("Collection", "").Trim().RomanNumeralCase();
        }
    }
}
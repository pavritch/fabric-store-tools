namespace ProductScanner.Core.Scanning.ProductProperties
{
    public class ProductProperty
    {
        public ProductPropertyType ProductPropertyType { get; set; }
        public string Value { get; set; }

        public ProductProperty(ProductPropertyType productPropertyType, string value)
        {
            ProductPropertyType = productPropertyType;
            Value = value;
        }
    }
}
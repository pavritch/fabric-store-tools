using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public static class RugProductVariantFeaturesBuilder
    {
        public static RugProductVariantFeatures Build(RugDimensions dimensions)
        {
            var features = new RugProductVariantFeatures();
            features.IsScalloped = dimensions.IsScalloped;
            features.IsSample = dimensions.Shape == ProductShapeType.Sample;
            features.Shape = dimensions.Shape.ToString();
            features.Description = dimensions.GetDescription();
            features.Width = dimensions.WidthInInches;
            features.Length = dimensions.LengthInInches;
            features.AreaSquareFeet = dimensions.GetArea();
            return features;
        }
    }

    public class RugWeave
    {
        // probably need to develop a set of valid values...
        private readonly string _value;
        public RugWeave(string value)
        {
            _value = value;
        }

        public string GetFormattedWeave()
        {
            var formatted = _value.Replace("-", " ");
            formatted = formatted.TitleCase();
            formatted = formatted.Replace("Flatweave", "Flat Weave");

            // things that are not weaves
            formatted = formatted.Replace("Solids", "");
            formatted = formatted.Replace("Textured", "");
            formatted = formatted.Replace("Naturals", "");

            formatted = formatted.Trim('/', ' ');
            return formatted;
        }
    }
}
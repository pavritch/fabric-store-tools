using System;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class RugDimensions
    {
        public bool IsScalloped { get; set; }
        public double WidthInInches { get; set; }
        public double LengthInInches { get; set; }
        public ProductShapeType Shape { get; set; }

        public RugDimensions(double widthInInches, double lengthInInches, ProductShapeType shape, bool isScalloped = false)
        {
            if (widthInInches <= 0.1 || lengthInInches <= 0.1) throw new ArgumentException("Invalid rug dimensions");

            WidthInInches = widthInInches;
            LengthInInches = lengthInInches;
            Shape = shape;
            IsScalloped = isScalloped;
        }

        public string GetSkuSuffix()
        {
            var shapeSuffix = "";
            if (Shape == ProductShapeType.Oval) shapeSuffix = "OV";
            if (Shape == ProductShapeType.Round) shapeSuffix = "RD";
            if (Shape == ProductShapeType.Octagon) shapeSuffix = "OCT";
            if (Shape == ProductShapeType.Square) shapeSuffix = "SQ";
            if (Shape == ProductShapeType.Runner) shapeSuffix = "RUN";

            var scalloped = IsScalloped ? "S" : "";

            // I think the rule is going to be - if divisible by 12, show as feet
            if ((int) WidthInInches%12 == 0 && (int) LengthInInches%12 == 0)
                return string.Format("-{0}{1}{2}{3}", (int) WidthInInches/12, (int) LengthInInches/12, shapeSuffix, scalloped);
            return string.Format("-{0}{1}{2}{3}", WidthInInches, LengthInInches, shapeSuffix, scalloped);
        }

        public string GetDescription()
        {
            var shapeName = Shape.DescriptionAttr();

            var widthFeet = (int) WidthInInches/12;
            var widthInches = (int) WidthInInches%12;
            var lengthFeet = (int) LengthInInches/12;
            var lengthInches = (int) LengthInInches%12;

            var widthDisplay = widthInches == 0 ? widthFeet + "'" : widthFeet + "'" + widthInches + "\"";
            var lengthDisplay = lengthInches == 0 ? lengthFeet + "'" : lengthFeet + "'" + lengthInches + "\"";
            if (widthDisplay == lengthDisplay)
                return string.Format("{0} {1}", widthDisplay, shapeName);
            return string.Format("{0} x {1} {2}", widthDisplay, lengthDisplay, shapeName);
        }

        public string GetDotFormatted()
        {
            var widthFeet = (int) WidthInInches/12;
            var widthInches = (int) WidthInInches%12;
            var lengthFeet = (int) LengthInInches/12;
            var lengthInches = (int) LengthInInches%12;
            return string.Format("{0}.{1} x {2}.{3}", widthFeet, widthInches, lengthFeet, lengthInches);
        }

        public double GetArea()
        {
            return Math.Round(WidthInInches*LengthInInches/144, 2);
        }
    }
}
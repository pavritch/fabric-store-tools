using System;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class RollDimensions
    {
        // stored in inches!
        public double Width { get; set; }
        public double Length { get; set; }
        public int RollCount { get; set; }

        private readonly string _rollType;

        public RollDimensions(double widthInInches, double lengthInInches, int rollCount = 1)
        {
            Length = lengthInInches;
            Width = widthInInches;

            RollCount = rollCount;
            _rollType = rollCount == 1 ? "single roll" :
                rollCount == 2 ? "double roll" : "triple roll";
            if (RollCount > 3)
            {
                _rollType = string.Format("{0} rolls", RollCount);
            }
        }

        public string GetRollType()
        {
            if (RollCount == 1) return "Single Roll";
            if (RollCount == 2) return "Double Roll";
            if (RollCount == 3) return "Triple Roll";
            return string.Format("{0} rolls", RollCount);
        }

        public string GetCoverageFormatted()
        {
            if (Width == 0 || Length == 0) return string.Empty;

            return GetCoverage() + string.Format(" square feet");
        }

        public string GetTotalCoverageFormatted()
        {
            if (Width == 0 || Length == 0) return string.Empty;
            var totalCoverage = GetCoverage();
            return totalCoverage + " square feet";
        }

        public double GetCoverage()
        {
            return Math.Round(Length*Width/144, 2);
        }

        public string GetLengthInYardsFormatted()
        {
            if (Length <= 0) return string.Empty;
            return (Length/36) + " yards";
        }

        public string GetLengthTotalInYardsFormatted(int multiplier)
        {
            if (Length <= 0) return string.Empty;
            return Math.Round(multiplier*Length/36, 2) + " yards";
        }

        public int GetNumRolls()
        {
            if (_rollType.Contains("double")) return 2;
            if (_rollType.Contains("triple")) return 3;
            return 1;
        }

        public string Format()
        {
            if (Width == 0 || Length == 0) return string.Empty;
            return string.Format("{0} in. x {1} ft.", Width, Math.Round(Length / 12, 2));
        }

        public string FormatTotal(int multiplier)
        {
            if (Width == 0 || Length == 0) return string.Empty;
            return string.Format("{0} in. x {1} ft.", Width, multiplier * Math.Round(Length / 12, 2));
        }
    }
}
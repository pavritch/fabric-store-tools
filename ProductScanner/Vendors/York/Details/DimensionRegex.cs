using System;

namespace York.Details
{
    public class DimensionRegex
    {
        public string Regex { get; set; }
        public Func<double, double> WidthFunc { get; set; }
        public Func<double, double> LengthFunc { get; set; }

        public DimensionRegex(string regex, Func<double, double> widthFunc, Func<double, double> lengthFunc)
        {
            Regex = regex;
            WidthFunc = widthFunc;
            LengthFunc = lengthFunc;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core
{
    public static class RugParser
    {
        private static List<string> _roundKeys = new List<string> { "rnd", "round", "rd", "ro" };
        private static List<string> _squareKeys = new List<string> {"square", "sqr", "sq"};
        private static List<string> _sampleKeys = new List<string> {"sample"};
        private static List<string> _ovalKeys = new List<string> {"oval", "ovl"};
        private static List<string> _kidneyKeys = new List<string> {"kidney"};
        private static List<string> _heartKeys = new List<string> {"heart", "he"};
        private static List<string> _octagonKeys = new List<string> {"octagon"};
        private static List<string> _runnerKeys = new List<string> {"rnr"};
        private static List<string> _starKeys = new List<string> {"star"};
        private static List<string> _scallopedKeys = new List<string> {"scalloped"};
        private static List<string> _otherKeys = new List<string> {"shape"};

        public static RugDimensions ParseDimensions(string size, ProductShapeType shape = ProductShapeType.Rectangular)
        {
            // remove all spaces and dashes, and put in lower case?
            var formattedSize = size.Replace("'-", "'").Replace("’", "'").Replace("”", "\"").Replace(" ", "").ToLower();
            formattedSize = formattedSize.Replace("ft.", "'");
            formattedSize = formattedSize.Replace("in.", "\"");
            formattedSize = formattedSize.Replace("shaped", "");
            var isScalloped = _scallopedKeys.Any(formattedSize.Contains);

            // in the case that no units are listed, a dot usually represents feet.inches
            if (!HasUnit(formattedSize))
            {
                formattedSize = formattedSize.Replace(".", "-");
            }

            var isRound = _roundKeys.Any(formattedSize.Contains);
            if (isRound || shape == ProductShapeType.Round)
            {
                _roundKeys.ForEach(x => formattedSize = formattedSize.Replace(x, ""));
                // slice everything prior to an 'x'
                formattedSize = formattedSize.Substring(formattedSize.IndexOf("x") + 1);

                var inches = GetInches(formattedSize);
                if (inches <= 0) return null;
                return new RugDimensions(inches, inches, ProductShapeType.Round, isScalloped);
            }

            var isSquare = _squareKeys.Any(formattedSize.Contains);
            if (isSquare || shape == ProductShapeType.Square)
            {
                _squareKeys.ForEach(x => formattedSize = formattedSize.Replace(x, ""));

                var dimensions = formattedSize.Split(new[] {'x'});
                var width = dimensions.First();
                var inches = GetInches(width);
                var isSampleDimensions = (int)inches == 18;
                return new RugDimensions(inches, inches, 
                    isSampleDimensions ? ProductShapeType.Sample :
                    ProductShapeType.Square, 
                    isScalloped);
            }

            // if we have an 'x', there are two dimensions to work with
            if (formattedSize.Contains("x"))
            {
                var isOval = _ovalKeys.Any(formattedSize.Contains) || shape == ProductShapeType.Oval;
                var isKidney = _kidneyKeys.Any(formattedSize.Contains);
                var isHeart = _heartKeys.Any(formattedSize.Contains);
                var isStar = _starKeys.Any(formattedSize.Contains);
                var isRunner = _runnerKeys.Any(formattedSize.Contains);

                formattedSize = RemoveShapes(_ovalKeys, formattedSize);
                formattedSize = RemoveShapes(_kidneyKeys, formattedSize);
                formattedSize = RemoveShapes(_heartKeys, formattedSize);
                formattedSize = RemoveShapes(_starKeys, formattedSize);
                formattedSize = formattedSize.Replace("runner", "");

                var dimensions = formattedSize.Split(new[] {'x'});
                var width = dimensions.First();
                var length = dimensions.Last();
                var widthInInches = GetInches(width);
                var lengthInInches = GetInches(length);

                var isRunnerDimensions = (widthInInches/lengthInInches) < 0.4;

                return new RugDimensions(widthInInches, lengthInInches, 
                    isOval ? ProductShapeType.Oval : 
                    isKidney ? ProductShapeType.Kidney : 
                    isHeart ? ProductShapeType.Heart : 
                    isStar ? ProductShapeType.Star : 
                    (isRunnerDimensions || isRunner) ? ProductShapeType.Runner :
                    shape, isScalloped);
            }

            if (_sampleKeys.Any(formattedSize.Contains))
            {
                formattedSize = RemoveShapes(_sampleKeys, formattedSize);
                var inches = GetInches(formattedSize);
                return new RugDimensions(inches, inches, ProductShapeType.Sample);
            }

            if (_otherKeys.Any(formattedSize.Contains))
            {
                formattedSize = RemoveShapes(_otherKeys, formattedSize);
                var inches = GetInches(formattedSize);
                return new RugDimensions(inches, inches, ProductShapeType.Other);
            }

            if (_octagonKeys.Any(formattedSize.Contains))
            {
                var inches = GetInches(formattedSize);
                return new RugDimensions(inches, inches, ProductShapeType.Octagon);
            }

            if (_starKeys.Any(formattedSize.Contains))
            {
                var inches = GetInches(formattedSize);
                return new RugDimensions(inches, inches, ProductShapeType.Star);
            }

            return null;
        }

        private static double GetInches(string width)
        {
            // we aren't given any info on what the actual unit is
            if (!HasUnit(width))
            {
                var widthDouble = width.ToDoubleSafe();
                if (widthDouble >= 16) return widthDouble;
                return widthDouble*12;
            }

            // 5-6
            if (width.Contains("-") && !width.Contains("'"))
            {
                var ftIn = width.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                if (ftIn.Count() == 1) return (double)(ftIn.First().ToDecimalSafe()*12);
                return ftIn.First().ToDoubleSafe()*12 + ftIn.Last().ToDoubleSafe();
            }

            // 14'0"
            // 8'
            // 24"
            // 2ft.3in.
            var match = Regex.Match(width, @"((?<feet>\d+)')?((?<inches>.*))?");
            var feet = match.Groups["feet"].Success ? Convert.ToDouble(match.Groups["feet"].Value) : 0;
            var inchesMatch = match.Groups["inches"].Value.Replace(@"""", "").Replace("-", "");
            var inches = 0d;
            if (inchesMatch.IsDouble()) inches = Convert.ToDouble(inchesMatch);
            return feet*12 + inches;
        }

        private static bool HasUnit(string unit)
        {
            return (unit.Contains("'") || unit.Contains("\"") || unit.Contains("ft") || unit.Contains("in") || unit.Contains("-"));
        }

        private static string RemoveShapes(List<string> keys, string formattedSize)
        {
            keys.ForEach(x => formattedSize = formattedSize.Replace(x, ""));
            return formattedSize;
        }
    }
}
using System;
using System.Linq;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core
{
    public static class RugExtensions
    {
        public static ProductShapeType GetShape(this string shape)
        {
            if (string.IsNullOrWhiteSpace(shape))
                return ProductShapeType.Rectangular;

            // does it tell us directly

            if (shape.ContainsIgnoreCase("Sample")) return ProductShapeType.Sample;
            if (shape.ContainsIgnoreCase("Star")) return ProductShapeType.Star;
            if (shape.ContainsIgnoreCase("Heart")) return ProductShapeType.Heart;
            if (shape.ContainsIgnoreCase("Runner")) return ProductShapeType.Runner;
            if (shape.ContainsIgnoreCase("Round")) return ProductShapeType.Round;
            if (shape.ContainsIgnoreCase("Rnd")) return ProductShapeType.Round;
            if (shape.ContainsIgnoreCase("Square")) return ProductShapeType.Square;
            if (shape.ContainsIgnoreCase("Rectangle")) return ProductShapeType.Rectangular;
            if (shape.ContainsIgnoreCase("Rectangular")) return ProductShapeType.Rectangular;
            if (shape.ContainsIgnoreCase("Octagon")) return ProductShapeType.Octagon;
            if (shape.ContainsIgnoreCase("Oval")) return ProductShapeType.Oval;
            if (shape.ContainsIgnoreCase("Basket")) return ProductShapeType.Basket;
            if (shape.ContainsIgnoreCase("Novelty")) return ProductShapeType.Novelty;
            if (shape.ContainsIgnoreCase("Animal")) return ProductShapeType.Animal;
            
            // try to infer

            shape = shape.Replace(@"""", "");
            shape = shape.Replace(@"'", "");

            var dimensions = shape.Split(new[] {'x', 'X'});
            if (dimensions.Count() == 1)
            {
                var first = dimensions.First();
                var second = dimensions.Last();
                if (first == second) 
                    return ProductShapeType.Square;

                return ProductShapeType.Rectangular;
            }
            return ProductShapeType.Rectangular;
        }
    }
}
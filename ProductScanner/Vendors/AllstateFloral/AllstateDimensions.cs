using ProductScanner.Core;
using Utilities.Extensions;

namespace AllstateFloral
{
    public class AllstateDimensions
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }

        public AllstateDimensions(double width, double height, double length, double depth)
        {
            Width = width;
            Height = height;
            Length = length;
            Depth = depth;
        }

        public AllstateDimensions(string width, string height, string length, string depth)
        {
            Width = width == null ? 0 : width.Replace("W", "").RemoveQuotes().ToDoubleSafe();
            Height = height == null ? 0 : height.Replace("H", "").RemoveQuotes().ToDoubleSafe();
            Length = length == null ? 0 : length.Replace("L", "").RemoveQuotes().ToDoubleSafe();
            Depth = depth == null ? 0 : depth.Replace("D", "").RemoveQuotes().ToDoubleSafe();

            if (width.ContainsIgnoreCase("'")) Width *= 12;
            if (height.ContainsIgnoreCase("'")) Height *= 12;
            if (length.ContainsIgnoreCase("'")) Length *= 12;
            if (depth.ContainsIgnoreCase("'")) Depth *= 12;
        }

        public bool IsEmpty()
        {
            return Width <= 0 && Length <= 0 && Height <= 0 && Depth <= 0;
        }
    }
}
using System;
using System.Linq;
using ProductScanner.Core;

namespace CyanDesign
{
    public class CyanDimensions
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public double Ext { get; set; }

        public CyanDimensions(string dimensions)
        {
            var splits = dimensions.Split(new[] {" x "}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var split in splits)
            {
                if (split.Contains("(w)")) Width = split.Replace("\"(w)", "").ToDoubleSafe();
                if (split.Contains("(h)")) Height = split.Replace("\"(h)", "").ToDoubleSafe();
                if (split.Contains("(l)")) Length = split.Replace("\"(l)", "").ToDoubleSafe();
                if (split.Contains("(dia)")) Diameter = split.Replace("\"(dia)", "").ToDoubleSafe();
                if (split.Contains("(ext)")) Ext = split.Replace("\"(ext)", "").ToDoubleSafe();
            }
        }
    }
}
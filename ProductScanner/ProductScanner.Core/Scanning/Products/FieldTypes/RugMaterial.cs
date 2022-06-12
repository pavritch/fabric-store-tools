using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class RugMaterial
    {
        private readonly string _value;
        private readonly List<string> _otherMaterials = new List<string>(); 

        public RugMaterial(string value)
        {
            _value = value;
        }

        public void AddMaterial(string material)
        {
            _otherMaterials.Add(material);
        }

        public Dictionary<string, int?> GetFormattedMaterial()
        {
            // get rid of anything related to weave/backing
            var formatted = _value.TitleCase().Trim(new []{'.', ':'});
            formatted = formatted.Replace("â„¢", "");
            formatted = formatted.Replace("Flat Weave", "");
            formatted = formatted.Replace("Flatweave", "");
            formatted = formatted.Replace("Loom Knotted", "");
            formatted = formatted.Replace("Heat Set", "Heat-Set");
            formatted = formatted.Replace("Semi Worsted", "Semi-Worsted");
            formatted = formatted.Replace("Polypropelene", "Polypropylene");
            formatted = formatted.Replace("Polypropelyne", "Polypropylene");
            formatted = formatted.Replace("Polyproplyene", "Polypropylene");
            formatted = formatted.Replace("Polyproplene", "Polypropylene");
            formatted = formatted.Replace("Polyprolylene", "Polypropylene");
            formatted = formatted.Replace("Polypropelyene", "Polypropylene");
            formatted = formatted.Replace("Polypropylene", "Polypropylene");
            formatted = formatted.Replace("Poly-Acrylic", "Polyacrylic");
            formatted = formatted.Replace("Pp Acrylic", "Polyacrylic");
            formatted = formatted.Replace("Poly Acrylic", "Polyacrylic");
            formatted = formatted.Replace("Poly Acrlic", "Polyacrylic");
            formatted = formatted.Replace("Poly-Acylic", "Polyacrylic");
            formatted = formatted.Replace("Visocse", "Viscose");

            // do parsing of percentages, etc...
            if (formatted.ContainsDigit()) return ParseValues(formatted);

            // look for delimiter in case of multiples
            var list = formatted.Split(new[] { "/", "+", "&", " and ", ",", " And " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            list = list.Where(x => !x.ContainsIgnoreCase("back")).Select(FormatSingleItem).ToList();
            list.AddRange(_otherMaterials);
            list = list.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return list.ToDictionary(x => x, y => (int?)null);
        }

        public string FormatAsString()
        {
            var materials = GetFormattedMaterial();
            return materials.Select(x => x.Key).Aggregate((a, b) => a + ", " + b);
        }

        private string FormatSingleItem(string material)
        {
            material = material.Trim().TitleCase();
            material = material.ReplaceWholeWord("Pvc", "PVC");
            material = material.ReplaceWholeWord("Uv", "UV");
            return material;
        }

        private Dictionary<string, int?> ParseValues(string formatted)
        {
            formatted = formatted.Replace("%", " ");
            formatted = formatted.Replace(",", " ");
            formatted = formatted.Replace(" and ", " ");

            var parts = formatted.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var dict = new Dictionary<string, int?>();
            var numberIndexes = parts.Select((value, index) => new {value, index}).Where(x => x.value.IsDouble()).Select(x => x.index).ToList();
            foreach (var startIndex in numberIndexes)
            {
                var number = parts[startIndex];

                // grab all words until the next numeric value
                var endIndex = parts.IndexOfNextNumber(startIndex + 1);

                // need an easy way to slice!
                var contentWords = parts.Skip(startIndex + 1).Take(endIndex - startIndex - 1);
                if (endIndex == -1)
                    contentWords = parts.Skip(startIndex + 1);
                var content = string.Join(" ", contentWords);
                dict.Add(content, (int)Math.Round(Convert.ToDouble(number)));
            }
            return dict;
        }
    }
}
using System;
using System.Collections.Generic;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public interface IPropFormatter
    {
        string Format();
    }

    public class Country : IPropFormatter
    {
        private readonly string _value;
        private readonly Dictionary<string, string> _maps = new Dictionary<string, string>
        {
            { "BE", "Belgium" },
            { "Belguim", "Belgium" },
            { "Brasil", "Brazil" },
            { "CN", "China" },
            { "CHINA3", "China" },
            { "EG", "Egypt" },
            { "FR", "France" },
            { "France Import", "France" },
            { "Greige Dyed In U.S.A.", "USA" },
            { "Greige Pakistanu.S.A", "USA" },
            { "Greige Printed U.S.A", "USA" },
            { "Holland Import", "Holland" },
            { "india", "India" },
            { "Inda", "India" },
            { "Idia", "India" },
            { "IN", "India" },
            { "Itlay", "Italy" },
            { "KOREA", "Korea" },
            { "Korea Import", "Korea" },
            { "K.S.A.", "Saudi Arabia" },
            { "Malaysa", "Malaysia" },
            { "MX", "Mexico" },
            { "Peoples Of China", "China" },
            { "Philippinnes", "Philippines" },
            { "Printedunitedkingdom", "United Kingdom" },
            { "Republic Of China", "China" },
            { "S.Korea", "South Korea" },
            { "S. Korea", "South Korea" },
            { "Spain & Canary Islands", "Spain and Canary Islands" },
            { "Taiean", "Taiwan" },
            { "Turkmenista", "Turkmenistan" },
            { "Turkeminista", "Turkmenistan" },
            { "Turkeministan", "Turkmenistan" },
            { "Uae", "United Arab Emirates" },
            { "Uk", "United Kingdom" },
            { "UK", "United Kingdom" },
            { "United Kinddom", "United Kingdom" },
            { "United Kingd", "United Kingdom" },
            { "United Kngdm", "United Kingdom" },
            { "United Arab", "United Arab Emirates" },
            { "United Arab Emi", "United Arab Emirates" },
            { "U.S.A", "USA" },
            { "U.S.", "USA" },
            { "US", "USA" },
            { "Us", "USA" },
            { "US/CN", "USA" },
            { "USE", "USA" },
            { "U..S.A.", "USA" },
        }; 

        public Country(string value)
        {
            _value = value;
        }

        public string Format()
        {
            if (string.IsNullOrWhiteSpace(_value) 
                || string.Equals(_value, "Unknown", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(_value, "?", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(_value, "Nice", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(_value, "None", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(_value, "To Be Determined At Lot Level", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(_value, "No Country Of Origin", StringComparison.OrdinalIgnoreCase))
                return null;

            // not sure why this is needed but didn't want to remove it
            var value = _value.TruncateStartingWith("/");
            value = value.Replace("Made in ", "");
            if (value.ContainsIgnoreCase("USA")) return "USA";
            if (value.ContainsIgnoreCase("U.S.A.")) return "USA";
            if (value.ContainsIgnoreCase("United States")) return "USA";
            if (value.ContainsIgnoreCase("U.K.")) return "United Kingdom";
            if (value.ContainsIgnoreCase("United Kingdom")) return "United Kingdom";
            if (_maps.ContainsKey(value)) return _maps[value];

            return value;
        }
    }
}
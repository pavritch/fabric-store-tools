using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace TrendLighting
{
    public class TrendBullet
    {
        // if the text of a bullet element matches any keys, then put that bullet into the defined field
        public List<string> Keys { get; set; }
        public ScanField ScanField { get; set; }
        public Func<string, string> Process { get; set; }

        public TrendBullet(List<string> keys, ScanField scanField, Func<string, string> process)
        {
            Keys = keys;
            ScanField = scanField;
            Process = process;
        }

        public bool Matches(string text)
        {
            return Keys.Any(text.ContainsIgnoreCase);
        }
    }
}
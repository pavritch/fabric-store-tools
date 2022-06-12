using System.Collections.Generic;

namespace PeggyPlatner.Metadata
{
    public class PPPatternMetadata
    {
        public List<string> Application { get; set; }
        public List<string> Composition { get; set; }
        public List<string> Style { get; set; }
        public List<string> Color { get; set; }

        public PPPatternMetadata()
        {
            Application = new List<string>();
            Composition = new List<string>();
            Style = new List<string>();
            Color = new List<string>();
        }
    }
}
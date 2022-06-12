using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website
{
    public class PhraseVariant
    {
        public string Phrase { get; set; }
        public int Weight { get; set; }

        public PhraseVariant()
        {

        }

        public PhraseVariant(string Phrase, int Weight=1)
        {
            if (Weight == 0)
                throw new Exception("Weight cannot be 0.");

            if (string.IsNullOrWhiteSpace(Phrase))
                throw new Exception("Phrase variant cannot be null or empty.");

            this.Phrase = Phrase;
            this.Weight = Weight;
        }
    }
}

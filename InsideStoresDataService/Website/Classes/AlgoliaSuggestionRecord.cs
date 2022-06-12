using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{

    public class AlgoliaSuggestionRecord
    {
        public string phrase { get; set; }
        public int rank { get; set; }

        public AlgoliaSuggestionRecord(string phrase, int rank=1)
        {
            this.phrase = phrase;
            this.rank = rank;
        }

    }
}
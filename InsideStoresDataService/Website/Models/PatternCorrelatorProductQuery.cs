using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class PatternCorrelatorProductQuery : ProductQueryBase
    {
        public string Pattern { get; set; }
        public int? ExcludeProductID { get; set; }
        public bool SkipMissingImages { get; set; }

        public PatternCorrelatorProductQuery()
        {
        }

        public PatternCorrelatorProductQuery(string pattern, bool skipMissingImages, int? excludedProductID=null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListByPatternCorrelator;
            this.Pattern = pattern;
            this.SkipMissingImages = skipMissingImages;
            this.ExcludeProductID = excludedProductID;
        }
    }
}
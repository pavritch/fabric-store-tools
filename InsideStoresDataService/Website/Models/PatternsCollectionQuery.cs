using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class PatternsCollectionQuery : CollectionQueryBase
    {
        public int ManufacturerID { get; set; }
        public string Filter { get; set; }

        public PatternsCollectionQuery()
        {
        }

        public PatternsCollectionQuery(int manufacturerID, string filter = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListPatternsByManufacturer;
            this.ManufacturerID = manufacturerID;
            this.Filter = filter;
        }
    }
}
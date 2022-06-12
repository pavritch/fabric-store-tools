using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class CollectionsCollectionQuery : CollectionQueryBase
    {
        public int ManufacturerID { get; set; }
        public string Filter { get; set; }

        public CollectionsCollectionQuery()
        {
        }

        public CollectionsCollectionQuery(int manufacturerID, string filter = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListCollectionsByManufacturer;
            this.ManufacturerID = manufacturerID;
            this.Filter = filter;
        }
    }
}
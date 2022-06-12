using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class ProductCollectionProductQuery : ProductQueryBase
    {
        public int CollectionID { get; set; }
        public string Filter { get; set; }

        public ProductCollectionProductQuery()
        {
        }

        public ProductCollectionProductQuery(int collectionID, string filter = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListByProductCollection;
            this.CollectionID = collectionID;
            this.Filter = filter;
        }
    }
}
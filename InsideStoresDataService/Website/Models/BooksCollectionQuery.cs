using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class BooksCollectionQuery : CollectionQueryBase
    {
        public int ManufacturerID { get; set; }
        public string Filter { get; set; }

        public BooksCollectionQuery()
        {
        }

        public BooksCollectionQuery(int manufacturerID, string filter = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListBooksByManufacturer;
            this.ManufacturerID = manufacturerID;
            this.Filter = filter;
        }
    }
}
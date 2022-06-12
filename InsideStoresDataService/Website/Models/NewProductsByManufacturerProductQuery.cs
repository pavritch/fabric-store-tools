using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class NewProductsByManufacturerProductQuery : ProductQueryBase
    {
        public int? ManufacturerID { get; set; }
        public int Days { get; set; }
        public string Filter { get; set; }

        public NewProductsByManufacturerProductQuery()
        {
        }

        public NewProductsByManufacturerProductQuery(int? manufacturerID, int days, string filter = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListNewProductsByManufacturer;
            this.ManufacturerID = manufacturerID;
            this.Days = days;
            this.Filter = filter;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class DiscontinuedProductQuery : ProductQueryBase
    {
        /// <summary>
        /// Not presently supported.
        /// </summary>
        public int? ManufacturerID { get; set; }

        public DiscontinuedProductQuery()
        {
        }

        /// <summary>
        /// List of all discontinued products. By ManufacturerdID not yet supported.
        /// </summary>
        /// <param name="manufacturerID"></param>
        public DiscontinuedProductQuery(int? manufacturerID = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListDiscontinuedProducts;
            this.ManufacturerID = manufacturerID;
        }
    }
}
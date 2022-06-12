using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class MissingImagesProductQuery : ProductQueryBase
    {
        /// <summary>
        /// Not presently supported.
        /// </summary>
        public int? ManufacturerID { get; set; }

        public MissingImagesProductQuery()
        {
        }

        /// <summary>
        /// List of all products missing images. By ManufacturerdID not yet supported.
        /// </summary>
        /// <param name="manufacturerID"></param>
        public MissingImagesProductQuery(int? manufacturerID = null)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListProductsMissingImages;
            this.ManufacturerID = manufacturerID;
        }
    }
}
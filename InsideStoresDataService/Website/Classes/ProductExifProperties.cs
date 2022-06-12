using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// For embedding meta data into images.
    /// </summary>
    public class ProductExifProperties
    {
        public string Description { get; set; }
        public string Artist { get; set; }
        public string Keywords { get; set; }
        public string Comment { get; set; }
    } 
}
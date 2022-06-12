using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class ManufacturerProductQuery : ProductQueryBase
    {
        public int ManufacturerID { get; set; }

        /// <summary>
        /// If true, then the FilterByxxxx properties will be filled in and will not be null.
        /// </summary>
        public bool IsFiltered { get; set; }

        // lists of category IDs
        public List<int> FilterByTypeList { get; set; }
        public List<int> FilterByPatternList { get; set; }
        public List<int> FilterByColorList { get; set; }

        public ManufacturerProductQuery()
        {
            IsFiltered = false;
            FilterByTypeList = null;
            FilterByPatternList = null;
            FilterByColorList = null;
        }

        public ManufacturerProductQuery(int manufacturerID) : this()
        {
            QueryMethod = QueryRequestMethods.ListByManufacturer;
            this.ManufacturerID = manufacturerID;
        }
    }
}
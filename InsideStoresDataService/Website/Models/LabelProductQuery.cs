using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class LabelProdutQuery : ProductQueryBase
    {
        /// <summary>
        /// Results only from this manufacturer.
        /// </summary>
        public int ManufacturerID { get; set; }

        /// <summary>
        /// Find products from manufacturer with this label with stated value.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Find products for manufacturer with this value for the stated label.
        /// </summary>
        public string Value { get; set; }


        public LabelProdutQuery()
        {
        }

        public LabelProdutQuery(int manufacturerID, string label, string value)
            : this()
        {
            QueryMethod = QueryRequestMethods.ListByLabelValueWithinManufacturer;
            this.ManufacturerID = manufacturerID;
            this.Label = label;
            this.Value = value;
        }
    }
}
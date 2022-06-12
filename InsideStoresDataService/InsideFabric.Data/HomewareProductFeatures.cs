using System.Collections.Generic;

namespace InsideFabric.Data
{
    public class HomewareProductFeatures
    {
        /// <summary>
        /// ASCII text description as set of 0 to N paragraphs. Null if none.
        /// </summary>
        /// <remarks>
        /// The website display logic will turn each string into a paragraph by wrapping in p tags.
        /// The difference between here and traditional manufacturer description is that here we are allowing more than one paragraph.

        /// </remarks>
        public List<string> Description { get; set; }

        private double? _depth;
        /// <summary>
        /// Depth in inches. Null if none provided
        /// </summary>
        public double? Depth
        {
            get { return _depth; }
            set
            {
                if (value == 0)
                    _depth = null;
                else
                    _depth = value;
            }
        }

        private double? _height;
        /// <summary>
        /// Height in inches. Null if none provided
        /// </summary>
        public double? Height
        {
            get { return _height; }
            set
            {
                if (value == 0) 
                    _height = null;
                else
                    _height = value;
            }
        }

        private double? _width;
        /// <summary>
        /// Height in inches. Null if none provided
        /// </summary>
        public double? Width
        {
            get { return _width; }
            set
            {
                if (value == 0) 
                    _width = null;
                else
                    _width = value;
            }
        }

        private double? _shippingWeight;
        /// <summary>
        /// Shipping Weight in lbs. Null if none provided
        /// </summary>
        public double? ShippingWeight 
        {
            get { return _shippingWeight; }
            set
            {
                if (value == 0)
                    _shippingWeight = null;
                else
                    _shippingWeight = value;
            }
        }

        public Dictionary<string, string> Features { get; set; }
        public List<string> Bullets { get; set; }
        public int Category { get; set; }

        public string CareInstructions { get; set; }
        public string Color { get; set; }
        public string UPC { get; set; }

        public string LeadTime { get; set; }

        public string Brand { get; set; }
        public string PleaseNote { get; set; }
    }
}


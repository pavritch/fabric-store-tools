using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace InsideFabric.Data
{

    /// <summary>
    /// This data is persisted to pv.ExtensionData4 for the individual variants.
    /// </summary>
    public class RugProductVariantFeatures
    {
        /// <summary>
        /// string value of enum ProductShapeType in ProductScanner project. 
        /// </summary>
        /// <remarks>
        /// Same as pv.Dimensions in SQL
        /// Maintained as string to eliminate any cross dependencies between projects.
        /// </remarks>
        public string Shape { get; set; }

        /// <summary>
        /// Indicates if the edges of the rug are scalloped.
        /// </summary>
        /// <remarks>
        /// Use this flag in addition to the product's primary shape (round, rectangular, etc.)
        /// </remarks>
        public bool IsScalloped { get; set; }

        /// <summary>
        /// Short description of this variant.
        /// </summary>
        /// <remarks>
        /// Same as pv.Description in SQL
        /// Examples: 2'6" x 8' Rectangular, 2' x 8' Runner, 18" Sample, 8' Octagon
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// This image filename in GUID.jpg format to concretely associate an image to this variant.
        /// </summary>
        /// <remarks>
        /// Really should be filled in, but possibily could fall back to matching on shape.
        /// The reason this association is important is that there are slight nuances to the looks/layout based on sizes,
        /// so not all rectangles are created equal, etc.
        /// </remarks>
        public string ImageFilename { get; set; }

        /// <summary>
        /// Width of rug in inches. A value is required. 
        /// </summary>
        /// <remarks>
        /// Mostly used for filters and calculations.
        /// </remarks>
        public double Width { get; set; }

        /// <summary>
        /// Length of rug in inches. A value is required.
        /// </summary>
        /// <remarks>
        /// Mostly used for filters and calculations.
        /// </remarks>
        public double Length { get; set; }

        /// <summary>
        /// Pile hight or thickness in inches.
        /// </summary>
        public double? PileHeight { get; set; }

        /// <summary>
        /// Set to true to indicate this variant is actually a "sample" rug.
        /// </summary>
        /// <remarks>
        /// The only time this should be true is when Shape is "Sample"
        /// Customers are required to pay for rug samples. These are just super tiny rugs - typically about 18x18 inches.
        /// </remarks>
        public bool IsSample { get; set; }

        /// <summary>
        /// UPC for this specific product variant, when known - else null.
        /// </summary>
        public string UPC { get; set; }

        /// <summary>
        /// The surface area of the product in square feet. Simple length x width calc.
        /// Used to facilitate certain sorting.
        /// </summary>
        public double AreaSquareFeet { get; set; } 

        // shipping information will not always be known, but if it is, should be indicated here.
        // internal logic will format as needed for display. Doubles are used to faciliate calculations.

        public double? ShippingWeight { get; set; } // pounds
        public double? ShippingWidth { get; set; } // inches
        public double? ShippingLength { get; set; } // inches
        public double? ShippingHeight { get; set; } // inches
    }

}
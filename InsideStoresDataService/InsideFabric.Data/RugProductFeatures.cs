using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace InsideFabric.Data
{

    public class RugProductFeatures
    {
        /// <summary>
        /// ASCII text description as set of 0 to N paragraphs. Null if none.
        /// </summary>
        /// <remarks>
        /// The website display logic will turn each string into a paragraph by wrapping in p tags.
        /// The difference between here and traditional manufacturer description is that here we are allowing more than one paragraph.
        /// </remarks>
        public List<string> Description { get; set; }

        /// <summary>
        /// Color indicated by manufacaturer. Null if none provided.
        /// </summary>
        /// <remarks>
        /// This is in our usual color linq:   Blue, Yellow / Ivory, etc.
        /// Will be displayed on detail pages.
        /// </remarks>
        public string Color { get; set; }

        /// <summary>
        /// An ordered parsed out list of colors. Initial caps. Null if none provided.
        /// </summary>
        /// <remarks>
        /// Website display logic can join these elements together as needed.
        /// </remarks>
        public List<string> Colors { get; set; }

        /// <summary>
        /// Main color (Red, Blue, Green, etc.) provided by manufacturer. Otherwise null.
        /// </summary>
        /// <remarks>
        /// Might be displayed on page. Should be presentable in our ususal way of things. But we generally anticipate
        /// using machine learning to classify color groups in some consistent way across all vendors.
        /// </remarks>
        public string ColorGroup { get; set; }

        /// <summary>
        /// The weave (construction) in the words of the manufacturer, cleaned up as needed.
        /// </summary>
        /// <remarks>
        /// Really expecting this to be identified for all products. Should have values like hand tufted, etc.
        /// Will be displayed in the words of the manufacture on detail pages, but will be analysed by machine learning
        /// for filters.
        /// </remarks>
        public string Weave { get; set; }

        /// <summary>
        /// Country where manufacturered. Null if not provided.
        /// </summary>
        public string CountryOfOrigin { get; set; }

        /// <summary>
        /// ASCII text care instructions for this item, else null.
        /// </summary>
        public string CareInstructions { get; set; }

        /// <summary>
        /// ASCII warranty for this item if different from manufacturer default, else null.
        /// </summary>
        /// <remarks>
        /// When null, which is usually the case, the general manufacturer's warranty will be displayed.
        /// </remarks>
        public string Warranty { get; set; }

        /// <summary>
        /// The name of the collection when the manufacturer has indicated the rug is a member of a collection. Otherwise, null.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// List of tags to associate with this rug. Typically comes from what might be found
        /// in Style|Design|Category
        /// </summary>
        /// <remarks>
        /// Take steps to use a clean/normalized schema of identifiers. These will be both displayed on detail pages and
        /// interpreted by the classification engine for discovery and building filters, etc.
        /// Must not include phrases like close-out or clearance. Each phrase should be its own tag.
        /// </remarks>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Name of the designer of this rug, else null if none  indicated.
        /// </summary>
        public string Designer { get; set; }

        /// <summary>
        /// If provided, the kind of backing (Cotten Canvas, Latex, Felt, Jute, Rubber, Polypropylene. Otherwise null.
        /// </summary>
        /// <remarks>
        /// Do not include phrases/abrev that will confuse consumers: Yes, No, ST, MO, IM, etc.
        /// </remarks>
        public string Backing { get; set; }

        /// <summary>
        /// The marketing name for this pattern when not some numeric or alphanumeric value. Otherwise null.
        /// </summary>
        /// <remarks>
        /// It's certainly possible to have both a pattern name and a pattern number for a product.
        /// </remarks>
        public string PatternName { get; set; }

        /// <summary>
        /// When the pattern is numeric or alphanumeric. Otherwise null.
        /// </summary>
        /// <remarks>
        /// It's certainly possible to have both a pattern name and a pattern number for a product.
        /// </remarks>
        public string PatternNumber { get; set; }

        /// <summary>
        /// The list of N materials/content used to manufacture the product - one element per material.
        /// </summary>
        /// <remarks>
        /// The material name should be initial caps per usual. The int value will be the percentage content, else null when not specified.
        /// </remarks>
        public Dictionary<string, int?> Material { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace InsideFabric.Data
{

    /// <summary>
    /// This class stores properties for an analyzed image (CEDD, dominant colors, etc.)
    /// </summary>
    /// <remarks>
    /// Purposely using very primitive types here. Keeps the JSON issues to a minimum.
    /// </remarks>
    public class ImageFeatures
    {
        private const int CURRENT_SCHEMA_VERSION = 1;

        /// <summary>
        /// The schema version of this structure - to help with figuring out how to deal with enhanced schemas down the road.
        /// </summary>
        /// <remarks>
        /// First version is one.
        /// </remarks>
        public int Version { get; set; }

        /// <summary>
        /// The name (filename.ext) of the file that was examined to create this set of data.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Time stamp to track when modifications were last made.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        /// 144 bytes of CEDD descriptor.
        /// </summary>
        public byte[] CEDD {get; set;}

        /// <summary>
        /// The Haar wavelet transform (15-bits used)
        /// </summary>
        /// <remarks>
        /// Used for fast proximity matching. This transform uses both Color and Texture information from CEDD.
        /// </remarks>
        public uint TinyCEDD { get; set; }

        /// <summary>
        /// The most dominant colors, created using a pal size of 4. Goes from light to dark.
        /// </summary>
        /// <remarks>
        /// This is an ordered list in #AARRGGBB format. The count of colors could vary between images.
        /// </remarks>
        public List<string> DominantColors { get; set; }

        /// <summary>
        /// Single best overall color, created using a pal size of 1.
        /// </summary>
        public string BestColor { get; set; }

        public ImageFeatures()
        {
            Version = CURRENT_SCHEMA_VERSION;
            DateLastModified = DateTime.Now;
        }

    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Website
{
    /// <summary>
    /// The supported file formats for product feeds.
    /// </summary>
    /// <remarks>
    /// All feeds are available in the following formats.
    /// </remarks>
    public enum ProductFeedFormats
    {
        [Description("text/plain")]
        txt,

        // application/x-zip-compressed
        // application/octet-stream
        [Description("application/x-zip-compressed")]
        zip,

        [Description("text/csv")]
        csv,
    }
}
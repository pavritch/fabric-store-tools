using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Indicates method which can be run from web interface.
    /// </summary>
    /// <remarks>
    /// Mostly used as a marker.
    /// </remarks>
    public class RunProductActionAttribute : Attribute
    {
        public string Name { get; set; }

        public RunProductActionAttribute()
        {

        }

        public RunProductActionAttribute(string Name)
        {
            this.Name = Name;
        }
    }
}
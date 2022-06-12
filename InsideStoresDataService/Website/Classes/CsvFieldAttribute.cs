using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CsvFieldAttribute : System.Attribute
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }

        public CsvFieldAttribute(string Name, bool IsRequired = false)
        {
            this.Name = Name;
            this.IsRequired = IsRequired;
        }
    }
}
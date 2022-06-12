using System;

namespace ProductScanner.Core.Scanning.ProductProperties
{

    /// <summary>
    /// Used to indicate a property on VendorProduct should be evaluated
    /// and added to the ProductProperties collection.
    /// </summary>
    /// <remarks>
    /// If a name is supplied, it is used for the inserted property name.
    /// Otherwise, the decorated member name will be used to find
    /// an enum in ProductPropertyType with that same name.
    /// 
    /// In all cases, the actual dictionary key is obtained from the
    /// description attribute of the respective ProductPropertyType.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ProductPropertyAttribute : Attribute
    {
        public ProductPropertyType? Name { get; set; }

        public ProductPropertyAttribute()
        {
            Name = null;
        }

        public ProductPropertyAttribute(ProductPropertyType Name) : this()
        {
            this.Name = Name;
        }
    }

}

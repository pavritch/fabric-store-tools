using System;

namespace ProductScanner.Core.Scanning.ProductProperties
{
    /// <summary>
    /// Marker attribute to indicate which Enum ProductPropertyType cannot be
    /// added to the ProductProperties dictionary.
    /// </summary>
    /// <remarks>
    /// Used to ensure we don't let the wrong properties end up in the public
    /// store by mistake.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ExcludedProductPropertyAttribute : Attribute
    {
    }
}

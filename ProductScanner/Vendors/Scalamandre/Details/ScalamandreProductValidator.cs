using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Scalamandre.Details
{
    public class ScalamandreProductValidator : DefaultProductValidator<ScalamandreVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            var colorName = product.PublicProperties[ProductPropertyType.Color];
            if (colorName.ContainsIgnoreCase("Replaced with"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.ObsoleteProduct);
            }
            return validation;
        }
    }
}
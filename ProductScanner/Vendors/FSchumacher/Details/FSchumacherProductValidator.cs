using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace FSchumacher.Details
{
    public class FSchumacherProductValidator : DefaultProductValidator<FSchumacherVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if (product.PublicProperties[ProductPropertyType.Collection].ContainsIgnoreCase("Contract") || 
                product.PublicProperties[ProductPropertyType.PatternName].ContainsIgnoreCase("Contract"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }
}
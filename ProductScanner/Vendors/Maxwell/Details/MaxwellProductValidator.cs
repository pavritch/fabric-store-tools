using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Maxwell.Details
{
    public class MaxwellProductValidator : DefaultProductValidator<MaxwellVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if (product.PrivateProperties[ProductPropertyType.Note].ContainsIgnoreCase("only available in Canada"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }
}
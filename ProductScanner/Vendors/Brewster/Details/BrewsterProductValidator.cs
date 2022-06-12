using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Brewster.Details
{
    public class BrewsterProductValidator : DefaultProductValidator<BrewsterVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if (product.Name.ContainsIgnoreCase("Lincrusta"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }

            if (product.PublicProperties[ProductPropertyType.Collection].ContainsIgnoreCase("Fetco"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.HighShippingCost);
            }

            return validation;
        }
    }
}
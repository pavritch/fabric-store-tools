using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Fabricut.Details
{
    public class SHarrisProductValidator : DefaultProductValidator<SHarrisVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            // exclude all SHarris wallcovering products
            var validation = base.ValidateProduct(product);
            if (product.ProductGroup == ProductGroup.Wallcovering)
            {
                //validation.ExcludedReasons.Add(ExcludedReason.WallcoveringExcluded);
            }
            return validation;
        }
    }
}
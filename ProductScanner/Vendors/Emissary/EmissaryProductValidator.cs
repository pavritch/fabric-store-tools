using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Emissary
{
    public class EmissaryProductValidator : DefaultProductValidator<EmissaryVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            var shippingMethod = product.PrivateProperties[ProductPropertyType.ShippingMethod];
            if (shippingMethod.ContainsIgnoreCase("Truckline") || shippingMethod == "T")
            {
                validation.ExcludedReasons.Add(ExcludedReason.FreightOnly);
            }
            return validation;
        }
    }
}
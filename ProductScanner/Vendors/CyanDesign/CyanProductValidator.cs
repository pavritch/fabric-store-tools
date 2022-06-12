using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace CyanDesign
{
    public class CyanProductValidator : DefaultProductValidator<CyanDesignVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if (GetFreightOnly(product as HomewareProduct) == "Y")
            {
                validation.ExcludedReasons.Add(ExcludedReason.FreightOnly);
            }
            return validation;
        }

        private string GetFreightOnly(HomewareProduct product)
        {
            if (product.ProductFeatures.Features.ContainsKey("Freight Only"))
                return product.ProductFeatures.Features["Freight Only"];
            return string.Empty;
        }
    }
}
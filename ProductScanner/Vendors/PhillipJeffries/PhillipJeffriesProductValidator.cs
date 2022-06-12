using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;
using System.Linq;

namespace PhillipJeffries
{
    public class PhillipJeffriesProductValidator : DefaultProductValidator<PhillipJeffriesVendor>
    {
        private readonly PhillipJeffriesExcludedProductsFileLoader _fileLoader;

        public PhillipJeffriesProductValidator(PhillipJeffriesExcludedProductsFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var excluded = _fileLoader.LoadData().Select(x => x[ScanField.ItemNumber]).ToList();

            var validation = base.ValidateProduct(product);
            if (product.Name.ContainsIgnoreCase("Zen Washi"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }

            if (excluded.Contains(product.PublicProperties[ProductPropertyType.ItemNumber]))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }

            if (product.PublicProperties[ProductPropertyType.Book].ContainsIgnoreCase("PJBINDER-GAL"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }
}
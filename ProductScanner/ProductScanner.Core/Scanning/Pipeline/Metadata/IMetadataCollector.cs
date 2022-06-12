using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Metadata
{
    // this will do the scrapes that look at menus, etc, and fill in details on products
    // sometimes this work is done during the initial discovery as well
    public interface IMetadataCollector<T> where T : Vendor
    {
        Task<List<ScanData>> PopulateMetadata(List<ScanData> products);
    }

    public interface IProductValidator<T> where T : Vendor
    {
        ProductValidationResult ValidateProduct(VendorProduct product);
    }

    public class ProductValidationResult
    {
        public bool IsValid() { return !ExcludedReasons.Any(); }
        public VendorProduct Product { get; set; }
        public List<ExcludedReason> ExcludedReasons { get; set; }

        public ProductValidationResult(VendorProduct product, List<ExcludedReason> excludedReasons)
        {
            Product = product;
            ExcludedReasons = excludedReasons;
        }
    }

    public class VariantValidationResult
    {
        public bool IsValid() { return !ExcludedReasons.Any(); }
        public VendorVariant Variant { get; set; }
        public List<ExcludedReason> ExcludedReasons { get; set; }

        public VariantValidationResult(VendorVariant variant, List<ExcludedReason> excludedReasons)
        {
            Variant = variant;
            ExcludedReasons = excludedReasons;
        }
    }

    public interface IVariantValidator<T> where T : Vendor
    {
        VariantValidationResult ValidateVariant(VendorVariant variant);
    }

    public class DefaultVariantValidator<T> : IVariantValidator<T> where T : Vendor, new()
    {
        public VariantValidationResult ValidateVariant(VendorVariant variant)
        {
            return new VariantValidationResult(variant, variant.GetExcludedReasons());
        }
    }

    public class ErrorCheckMetadataCollector<T> : IMetadataCollector<T> where T : Vendor
    {
        private readonly IMetadataCollector<T> _metadataCollector;
        private readonly ICheckpointService<T> _checkpointService;

        public ErrorCheckMetadataCollector(IMetadataCollector<T> metadataCollector, ICheckpointService<T> checkpointService)
        {
            _metadataCollector = metadataCollector;
            _checkpointService = checkpointService;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var results = new List<ScanData>(); 
            Exception error = null;
            try
            {
                results = await _metadataCollector.PopulateMetadata(products);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                await _checkpointService.RemoveAsync();
                throw error;
            }
            return results;
        }
    }
}
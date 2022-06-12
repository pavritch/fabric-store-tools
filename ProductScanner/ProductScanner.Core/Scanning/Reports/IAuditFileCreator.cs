using System.Collections.Generic;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Reports
{
    public interface IAuditFileCreator<T> where T : Vendor
    {
        void BuildProductAuditFile(List<VendorVariant> products);
        void BuildNewProductsAuditFile(List<VendorVariant> variants);
        void BuildPropertyAuditFile(List<ScanData> scanDatas);
        void BuildCSVFinalAnalysisFile(List<VendorVariant> variants);
        void BuildCSVRawAnalysisFile(List<ScanData> scanDatas);
        void BuildFilteredProductAuditFile(List<VendorVariant> variants);
        void BuildValidationResultsFile(List<ProductValidationResult> validationResults);
        void BuildValidationResultsFile(List<VariantValidationResult> validationResults);
    }
}
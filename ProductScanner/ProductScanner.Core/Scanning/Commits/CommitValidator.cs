using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Sessions;

namespace ProductScanner.Core.Scanning.Commits
{
    public interface ICommitValidator<T> where T : Vendor
    {
        bool Validate(List<StoreProduct> sqlProducts, CommitData commit);
    }

    // make sure none of our metrics are way off (>30% disc, etc...)
    public class CommitValidator<T> : ICommitValidator<T> where T : Vendor, new()
    {
        private readonly IVendorScanSessionManager<T> _sessionManager;

        public CommitValidator(IVendorScanSessionManager<T> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public bool Validate(List<StoreProduct> sqlProducts, CommitData commit)
        {
            var isValid = CheckDiscontinuedRatio(sqlProducts, commit);
            return isValid;
        }

        public bool CheckDiscontinuedRatio(List<StoreProduct> sqlProducts, CommitData commit)
        {
            var vendor = new T();
            if (vendor.RunDiscontinuedPercentageCheck)
            {
                var liveProductsCount = sqlProducts.Count(x => !x.IsDiscontinued);
                if (commit.Discontinued.Count/(double) liveProductsCount > 0.3)
                {
                    _sessionManager.Log(EventLogRecord.Error("More than 30% of products marked as discontinued"));
                    return false;
                }
            }
            return true;
        }
    }
}
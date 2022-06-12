using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{

    /// <summary>
    /// The kinds of pages we know how to display.
    /// </summary>
    public enum ContentPageTypes
    {
        Home,
        StoreDashboard,
        StoreCommitSummary,
        StoreCommitBatch,
        StoreScanSummary,
        StoreLoginsSummary,
        VendorDashboard,
        VendorScan,
        VendorCommits,
        VendorStockCheck,
        VendorTests,
        VendorProperties,
        VendorCommitBatch,
        VendorScanLog,
        VendorScanLogs,
    }
}

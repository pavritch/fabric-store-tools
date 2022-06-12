using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    public enum ScanningOperationEvent
    {
        Started,
        Suspended,
        Cancelled,
        Resumed,
        Finished,
        Failed,
        ReportedError,
        CreatedCheckpoint,
        CachedFilesChanged,
        StaticFilesChanged,
        LoggedEvent,
        SubmittedCommitBatch,
        DiscardedOrDeletedCommitBatch,
        CommittedBatch,
    }
    
    /// <summary>
    /// Message broadcast by VendorModel logic to let others know things are taking place.
    /// </summary>
    /// <remarks>
    /// Receivers can query the model to find out futher details.
    /// </remarks>
    class ScanningOperationNotification : IMessage
    {
        public IVendorModel Vendor { get; private set; }
        public ScanningOperationEvent ScanningEvent { get; private set; }

        public ScanningOperationNotification(IVendorModel vendor, ScanningOperationEvent scanningEvent)
        {
            this.Vendor = vendor;
            this.ScanningEvent = scanningEvent;
        }
    }
}

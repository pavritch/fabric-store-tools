using System.ComponentModel;

namespace ProductScanner.Core.Scanning.Commits
{

    /// <summary>
    /// Outcome for when commiting a batch to store SQL.
    /// </summary>
    /// <remarks>
    /// Descriptions can be used for UX.
    /// </remarks>
    public enum CommitBatchResult
    {
        // no result - default start state
        [Description("None")]
        None,

        // trouble with access to SQL, cannot continue.
        [Description("Database Error")]
        DatabaseError,

        // batch no longer in SQL. 
        [Description("Batch Not Found")]
        NotFound,

        // the batch belongs to some other vendor (would actually mean a programming bug)
        [Description("Incorrect Vendor")]
        IncorrectVendor,

        // the batch is not presently marked as pending. can only commit pending batches
        [Description("Not Pending")]
        NotPending,

        // likley batch is already in progress through another parallel operation. Unlikely, but possible.
        [Description("Access Denied")]
        AccessDenied,

        [Description("Successful")]
        Successful,

        // not quite sure what this signifies just yet. would sort of have to be something pretty bad
        // since having a few failed records still would be a successful result.
        // Could be this represents that an exception was thrown.
        [Description("Failed")]
        Failed,

        // operation cancelled by operator. 
        // still need to determine if we will actually allow batches to be cancelled mid-way through.
        // In theory, batches are intended to be all or nothing. However, would seem necessary to be
        // able to stop immediately is something is corrupting SQL. Needs more thought...
        [Description("Cancelled")]
        Cancelled,
    }
}

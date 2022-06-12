using System.ComponentModel;

namespace ProductScanner.Core.PlatformEntities
{
    /// <summary>
    /// The disposition of batches submitted by the scanner.
    /// </summary>
    /// <remarks>
    /// The description is used for displaying in UX.
    /// </remarks>
    public enum CommitBatchStatus
    {
        [Description("Pending")]
        Pending,

        [Description("Discarded")]
        Discarded,

        [Description("Failed")]
        Failed, // so far not used

        [Description("Committed")]
        Committed
    }
}
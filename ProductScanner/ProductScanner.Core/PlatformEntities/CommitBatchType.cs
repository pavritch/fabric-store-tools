using System.ComponentModel;

namespace ProductScanner.Core.PlatformEntities
{
    /// <summary>
    /// The kinds of batches the scanner knows how to submit/persist.
    /// </summary>
    /// <remarks>
    /// The description is used for displaying in UX.
    /// </remarks>
    public enum CommitBatchType
    {
        [Description("Price Updates")]
        PriceUpdate,

        [Description("In Stock")]
        InStock,

        [Description("Out of Stock")]
        OutOfStock,

        [Description("Images")]
        Images,

        [Description("Discontinued")]
        Discontinued,

        [Description("New Products")]
        NewProducts,

        [Description("Full Updates")]
        FullUpdate,

        [Description("Removed Variants")]
        RemovedVariants,

        [Description("New Variants")]
        NewVariants
    } 
}
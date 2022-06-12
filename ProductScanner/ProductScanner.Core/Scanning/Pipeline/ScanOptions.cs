using System;
using System.ComponentModel;

namespace ProductScanner.Core.Scanning.Pipeline
{
    /// <summary>
    /// Operation options (flags) which can be presented to the scanner runtime.
    /// </summary>
    [Flags]
    public enum ScanOptions
    {
        None = 0,

        [Description("Do not save results.")]
        DoNotSaveResults = 1,

        [Description("Generate reports.")]
        GenerateReports = 2,

        [Description("Simulate no existing store products.")]
        SimulateZeroStoreProducts = 4,

        [Description("Skip images on full update records.")]
        SkipImagesOnFullUpdateRecords = 8,

        [Description("Search for missing images.")]
        SearchForMissingImages = 16,

        [Description("Perform full product updates.")]
        FullProductUpdate = 32,

        [Description("Allow SKU changes on full updates.")]
        AllowSkuChangeOnUpdates = 64,

        [Description("Delete cached files before starting.")]
        DeleteCachedFilesBeforeStarting = 128,

        [Description("Simulate no discontinued products.")]
        SimulateZeroDiscontinuedProducts = 256
    }
}

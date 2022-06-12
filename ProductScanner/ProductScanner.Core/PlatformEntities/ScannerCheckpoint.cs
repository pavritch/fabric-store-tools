using System;
using System.ComponentModel.DataAnnotations.Schema;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.Core.PlatformEntities
{
    public class ScannerCheckpoint
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime ScanStarted { get; set; }
        public string Store { get; set; }
        public int DurationInSeconds { get; set; }
        public ScanOptions ScanOptions { get; set; }

        [Index]
        public int VendorId { get; set; }

        [Index]
        public Guid SessionId { get; set; }

        // saved as an instance of CheckpointData - converted to JSON and GZipped
        public byte[] CheckpointData { get; set; }

        public string Schema { get; set; }

        // List<EventLogRecord> as JSON
        public string Log { get; set; }

        public int DelayMSBetweenVendorWebsiteRequests { get; set; }
        public int ErrorCount { get; set; }
        public int MaximumScanningErrorCount { get; set; }
    }
}
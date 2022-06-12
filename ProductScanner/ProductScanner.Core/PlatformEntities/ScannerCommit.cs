using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.PlatformEntities
{
    public class ScannerCommit
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }

        [Index]
        public Guid SessionId { get; set; }
        public string Store { get; set; }

        [Index]
        public int VendorId { get; set; }
        public CommitBatchType BatchType { get; set; }
        public byte[] CommitData { get; set; }
        public int QtySubmitted { get; set; }
        public int? QtyCommitted { get; set; }
        public DateTime? DateCommitted { get; set; }
        public CommitBatchStatus SessionStatus { get; set; }
        public bool IsProcessing { get; set; }
        public string Schema { get; set; }
        public string Log { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductScanner.Core.PlatformEntities
{
    public class ScanLog
    {
        public int Id { get; set; }

        [Index]
        public Guid SessionId { get; set; }
        public string Log { get; set; }
        public DateTime Created { get; set; }

        public string Store { get; set; }
        public int VendorId { get; set; }
    }
}
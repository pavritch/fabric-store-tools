using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Simple DTO class used to convey statistics about a file storage folder (cache or static files).
    /// </summary>
    /// <remarks>
    /// Includes all subfolders of the target storage location.
    /// </remarks>
    public class FileStorageMetrics
    {
        /// <summary>
        /// When this data was created - in case needed for cache timing.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// File count.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Total size in bytes of the files in the storage tree.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Timestamp of the oldest file in the target storage tree. Null if no files.
        /// </summary>
        public DateTime? Oldest { get; set; }

        /// <summary>
        /// Timestamp of the newest file in the target storage tree. Null if no files.
        /// </summary>
        public DateTime? Newest { get; set; }

        public FileStorageMetrics()
        {
            Created = DateTime.Now;
        }
    }
}

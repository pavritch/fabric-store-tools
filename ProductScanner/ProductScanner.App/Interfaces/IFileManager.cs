using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{

    /// <summary>
    /// All disk/file operations in this app go through this single service provider.
    /// </summary>
    public interface IFileManager
    {
        Task<FileStorageMetrics> GetFileStoreMetricsAsync(string rootFolder);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.Core.Scanning.Checkpoints
{
    public interface ICheckpointService
    {
        Task SaveAsync(CheckpointData scanData, ScanOptions options);
        Task<ScannerCheckpoint> GetLatestAsync();
        Task RemoveAsync();
        Task<bool> HasCheckpointAsync();
    }

    public interface ICheckpointService<T> : ICheckpointService where T : Vendor
    {
    }
}
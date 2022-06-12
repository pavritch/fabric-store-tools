using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Sessions;

namespace ProductScanner.Core.Scanning.Checkpoints
{
    public class DatabaseCheckpointService<T> : ICheckpointService<T> where T : Vendor, new()
    {
        private readonly IVendorScanSessionManager<T> _sessionManager; 
        private readonly IPlatformDatabase _database;

        public DatabaseCheckpointService(IVendorScanSessionManager<T> sessionManager, IPlatformDatabase database)
        {
            _sessionManager = sessionManager;
            _database = database;
        }

        public async Task SaveAsync(CheckpointData scanData, ScanOptions options)
        {
            await _database.SaveScannerCheckpointAsync(new T(), scanData, _sessionManager.GetStartTime(), 
                _sessionManager.GetFullLog().ToList(), 
                _sessionManager.GetTotalDuration(), 
                _sessionManager.VendorSessionStats.ErrorCount,
                _sessionManager.MaximumScanningErrorCount,
                await _sessionManager.GetThrottleAsync(),
                options);
        }

        public async Task<ScannerCheckpoint> GetLatestAsync()
        {
            var checkpoint = await _database.GetScannerCheckpointAsync(new T());
            return checkpoint;
        }

        public async Task RemoveAsync()
        {
            await _database.RemoveScannerCheckpointAsync(new T());
        }

        public async Task<bool> HasCheckpointAsync()
        {
            var checkpoint = await _database.GetScannerCheckpointAsync(new T());
            return checkpoint != null;
        }
    }
}
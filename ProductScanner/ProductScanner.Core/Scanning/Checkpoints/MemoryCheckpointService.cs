using System.Threading.Tasks;

namespace ProductScanner.Core.Scanning.Checkpoints
{
    // if we want to store checkpoints in memory
    /*public class MemoryCheckpointService<T> : ICheckpointService<T> where T : Vendor
    {
        private Checkpoint _latestScanData;

        public Task SaveAsync(Checkpoint scanData)
        {
            _latestScanData = scanData;
            return Task.FromResult(0);
        }

        public Task<Checkpoint> GetLatestAsync()
        {
            return Task.FromResult(_latestScanData);
        }

        public Task RemoveAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> HasCheckpointAsync()
        {
            throw new System.NotImplementedException();
        }
    }*/
}
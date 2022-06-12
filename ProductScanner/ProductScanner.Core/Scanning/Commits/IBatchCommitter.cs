using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductScanner.Core.Scanning.Commits
{
    public interface IBatchCommitter
    {
        Task<CommitBatchResult> CommitBatchAsync(int batchId, bool ignoreDuplicates, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null);
    }
}
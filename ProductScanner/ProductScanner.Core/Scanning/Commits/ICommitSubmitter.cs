using System.Threading.Tasks;

namespace ProductScanner.Core.Scanning.Commits
{
    public interface ICommitSubmitter<T> where T : Vendor
    {
        Task<bool> SubmitAsync(CommitData commitData);
    }
}
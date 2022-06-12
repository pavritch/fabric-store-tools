using System.Threading.Tasks;

namespace ProductScanner.Core.Authentication
{
    public interface IAuthenticationTester
    {
        Task RunAll();
    }
}
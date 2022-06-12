using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.VendorTesting.TestTypes
{

    public interface IVendorTestRunner
    {
        Task<AuthenticationResult> RunAuthTest();
        Task<List<TestResult>> RunAllTests();
        List<IVendorTest> GetVendorTests();
    }

    public interface IVendorTestRunner<T> : IVendorTestRunner where T : Vendor
    {
    }

    public class VendorTestRunner<T> : IVendorTestRunner<T> where T : Vendor, new()
    {
        private readonly IVendorTestMediator _testMediator;
        private readonly IVendorAuthenticator<T> _vendorAuthenticator;

        public VendorTestRunner(IVendorTestMediator testMediator, IVendorAuthenticator<T> vendorAuthenticator)
        {
            _testMediator = testMediator;
            _vendorAuthenticator = vendorAuthenticator;
        }

        public async Task<AuthenticationResult> RunAuthTest()
        {
            try
            {
                return await _vendorAuthenticator.LoginAsync();
            }
            catch (Exception)
            {
                return new AuthenticationResult(false);
            }
        }

        public List<IVendorTest> GetVendorTests()
        {
            return _testMediator.GetVendorTests<T>();
        }

        public async Task<List<TestResult>> RunAllTests()
        {
            var authResult = await RunAuthTest();
            if (!authResult.IsSuccessful)
            {
                return new List<TestResult> {new TestResult(TestResultCode.Failed, "Authentication Test Failed")};
            }
            var allTests = GetVendorTests();
            var results = new List<TestResult>();
            foreach (var test in allTests)
            {
                results.Add((await test.RunAsync(authResult.Cookies)).WithTestName(test.GetType().Name));
            }
            return results;
        }
    }

    public abstract class VendorTest<T> : IVendorTest<T> where T : Vendor
    {
        protected readonly IWebClientFactory _webClientFactory;
        protected VendorTest(IWebClientFactory webClientFactory) { _webClientFactory = webClientFactory; }
        public abstract Task<TestResult> RunAsync(CookieCollection cookies);
        public abstract string GetDescription();
    }
}
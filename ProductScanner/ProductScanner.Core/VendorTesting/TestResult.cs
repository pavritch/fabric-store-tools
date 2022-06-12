namespace ProductScanner.Core.VendorTesting
{
    public class TestResult
    {
        public string Message { get; set; }
        public TestResultCode Code { get; set; }

        public TestResult(TestResultCode code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public TestResult WithTestName(string testName)
        {
            return new TestResult(Code, string.Format("{0}: {1}", testName, Message));
        }

        public static TestResult Success(string message = "")
        {
            return new TestResult(TestResultCode.Successful, message);
        }

        public static TestResult Failed(string message, params object[] formatStrings)
        {
            return new TestResult(TestResultCode.Failed, string.Format(message, formatStrings));
        }

        public static TestResult AuthResult(bool success)
        {
            return new TestResult(success ? TestResultCode.Successful : TestResultCode.Failed,
                success ? "Authentication Test: Successful" : "Authentication Test: Failed");
        }
    }
}
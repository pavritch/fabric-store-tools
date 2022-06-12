namespace ControlPanel
{
    /// <summary>
    /// Class used to report errors for validation models and view models.
    /// </summary>
    /// <remarks>
    /// The validating models will include a collection of these - typically
    /// a dictionary with a list of these per each property with an error.
    /// 
    /// Used to support implementation of INotifyDataErrorInfo.
    /// </remarks>
    public class ValidationErrorInfo
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public ValidationErrorInfo()
        {
        }

        public ValidationErrorInfo(int errorCode, string message)
        {
            ErrorCode = errorCode;
            ErrorMessage = message;
        }

        public override string ToString()
        {
            return ErrorMessage;
        }

    }
}

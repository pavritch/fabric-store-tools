using System;

namespace ProductScanner.Core.Monitoring
{
    public class AuthenticationFailureDTO
    {
        public string VendorDisplayName { get; set; }
        public DateTime DateTime { get; set; }

        public AuthenticationFailureDTO(string vendorDisplayName, DateTime dateTime)
        {
            VendorDisplayName = vendorDisplayName;
            DateTime = dateTime;
        }
    }
}
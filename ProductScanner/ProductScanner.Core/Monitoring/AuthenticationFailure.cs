using System;

namespace ProductScanner.Core.Monitoring
{
    public class AuthenticationFailure
    {
        public Vendor Vendor { get; set; }
        public DateTime DateTime { get; private set; }

        public AuthenticationFailure(Vendor vendor)
        {
            Vendor = vendor;
            DateTime = DateTime.UtcNow;
        }
    }
}
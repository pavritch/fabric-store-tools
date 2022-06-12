using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    /// <summary>
    /// Message is broadcasted each time a vendor model wishes to announce a material change.
    /// </summary>
    /// <remarks>
    /// Interested subdscribers should refresh themselves.
    /// </remarks>
    class VendorChangedNotification : IMessage
    {
        public IVendorModel Vendor { get; private set; }

        public VendorChangedNotification(IVendorModel vendor)
        {
            this.Vendor = vendor;
        }
    }
}

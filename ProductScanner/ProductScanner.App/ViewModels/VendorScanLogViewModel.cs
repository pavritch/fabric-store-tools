using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class VendorScanLogViewModel : VendorContentPageViewModel
    {
        public VendorScanLogViewModel(IVendorModel vendor)
            : base(vendor)
        {
            PageType = ContentPageTypes.VendorScanLog;

        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class StoreScanSummaryViewModel : StoreContentPageViewModel
    {
        public StoreScanSummaryViewModel(IStoreModel store)
            : base(store)
        {
            PageType = ContentPageTypes.StoreScanSummary;
            PageSubTitle = "Scan Summary";
            BreadcrumbTemplate = "{Home}/{Store}/Scans";
            IsNavigationJumpTarget = true;

        }
    }
}
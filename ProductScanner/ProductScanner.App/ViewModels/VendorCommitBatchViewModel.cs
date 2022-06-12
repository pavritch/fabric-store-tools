using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;

namespace ProductScanner.App.ViewModels
{
    public class VendorCommitBatchViewModel : VendorContentPageViewModel
    {
#if DEBUG
        public VendorCommitBatchViewModel() :this(new DesignVendorModel() {Name="Kravet", VendorId=5}, 100)
        {

        }
#endif

        public VendorCommitBatchViewModel(IVendorModel vendor, int batchID) : base(vendor)
        {
            PageType = ContentPageTypes.VendorCommitBatch;

            PageSubTitle = "Commit Batch";
            RequiresToBeCached = false;
            IsNavigationJumpTarget = true;
            PageType = ContentPageTypes.VendorCommitBatch;

            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Commits/Batch";

            this.BatchID = batchID;
        }

        #region Public Properties

        private int? _batchID = null;
        public int? BatchID
        {
            get
            {
                return _batchID;
            }

            set
            {
                if (_batchID == value)
                {
                    return;
                }

                _batchID = value;
                RaisePropertyChanged(() => BatchID);
            }
        }
        #endregion

    }
}
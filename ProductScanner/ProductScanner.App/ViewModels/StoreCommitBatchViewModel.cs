using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using ProductScanner.Core;

namespace ProductScanner.App.ViewModels
{
    public class StoreCommitBatchViewModel : StoreContentPageViewModel
    {
#if DEBUG
        public StoreCommitBatchViewModel()
            : this((new DesignStoreModel { Name = "InsideFabric", Key = StoreType.InsideFabric }) as IStoreModel, 100)
        {
        }
#endif
        public StoreCommitBatchViewModel(IStoreModel store, int batchID)  : base(store)
        {
            PageType = ContentPageTypes.StoreCommitBatch;

            PageSubTitle = "Commit Batch";
            RequiresToBeCached = false;
            IsNavigationJumpTarget = false;

            BreadcrumbTemplate = "{Home}/{Store}/Commits/Batch";

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
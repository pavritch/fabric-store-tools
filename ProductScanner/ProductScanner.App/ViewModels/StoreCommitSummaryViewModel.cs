using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.ViewModels
{
    public class StoreCommitSummaryViewModel : StoreContentPageViewModel
    {

#if DEBUG
        public StoreCommitSummaryViewModel()
            : this((new DesignStoreModel { Name = "InsideFabric", Key = StoreType.InsideFabric }) as IStoreModel)
        {
        }
#endif

        public StoreCommitSummaryViewModel(IStoreModel store)
            : base(store)
        {
            PageType = ContentPageTypes.StoreCommitSummary;
            PageSubTitle = "Commit Summary";
            BreadcrumbTemplate = "{Home}/{Store}/Commits";
            IsNavigationJumpTarget = true;


        }


        public override void Activated()
        {
            Task.Run(async () =>
            {
                // need to populate the value in the UI thread
                var commits = await Store.GetCommitBatchesAsync();
                var commitsCollection = new ObservableCollection<CommitBatchSummary>(commits);
                await DispatcherHelper.RunAsync(() =>
                {
                    CommitsItemsSource = commitsCollection;
                });
            });
        }


        #region Public Properties

        private ObservableCollection<CommitBatchSummary> _commitsItemsSource = null;
        public ObservableCollection<CommitBatchSummary> CommitsItemsSource
        {
            get
            {
                return _commitsItemsSource;
            }

            set
            {
                if (_commitsItemsSource == value)
                {
                    return;
                }

                _commitsItemsSource = value;
                RaisePropertyChanged(() => CommitsItemsSource);
            }
        }

        #endregion

    }
}
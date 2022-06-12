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
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.ViewModels
{
    public class VendorCommitsViewModel : VendorContentPageViewModel
    {
#if DEBUG
        public VendorCommitsViewModel()
            : this(new DesignVendorModel() { Name = "Kravet", VendorId = 5 })
        {

        }
#endif
        public VendorCommitsViewModel(IVendorModel vendor)
            : base(vendor)
        {
            PageType = ContentPageTypes.VendorCommits;
            PageSubTitle = "Commit Batches";
            BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Commits";
            IsNavigationJumpTarget = true;
        }

        public override void Activated()
        {
            Task.Run(async () =>
            {
                // need to populate the value in the UI thread
                var commits = await Vendor.GetCommitBatchesAsync();
                var commitsCollection = new ObservableCollection<CommitBatchSummary>(commits);
                await DispatcherHelper.RunAsync(() =>
                {
                    Debug.WriteLine(string.Format("Populating batch list for: {0}", Vendor.Name));
                    CommitsItemsSource = commitsCollection;
                });
            });
        }


        #region Pulbic Properties

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
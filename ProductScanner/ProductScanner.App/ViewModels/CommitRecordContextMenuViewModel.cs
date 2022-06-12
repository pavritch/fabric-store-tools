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
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App.ViewModels
{
    public class CommitRecordContextMenuViewModel : ViewModelBase
    {

        public CommitRecordContextMenuViewModel(ICommitRecordDetails record)
        {
            this.Record = record;
        }

        private ICommitRecordDetails _record = null;
        public ICommitRecordDetails Record
        {
            get
            {
                return _record;
            }
            set
            {
                Set(() => Record, ref _record, value);
            }
        }


        private RelayCommand _browseStoreUrl;

        /// <summary>
        /// Gets the BrowseStoreUrl.
        /// </summary>
        public RelayCommand BrowseStoreUrl
        {
            get
            {
                return _browseStoreUrl
                    ?? (_browseStoreUrl = new RelayCommand(
                    () =>
                    {
                        if (!BrowseStoreUrl.CanExecute(null))
                        {
                            return;
                        }

                        Messenger.Default.Send(new RequestLaunchBrowser(Record.StoreUrl));                        
                    },
                    () => Record.StoreUrl != null));
            }
        }

        private RelayCommand _browseVendorUrl;

        /// <summary>
        /// Gets the BrowseVendorUrl.
        /// </summary>
        public RelayCommand BrowseVendorUrl
        {
            get
            {
                return _browseVendorUrl
                    ?? (_browseVendorUrl = new RelayCommand(
                    () =>
                    {
                        if (!BrowseVendorUrl.CanExecute(null))
                        {
                            return;
                        }

                        Messenger.Default.Send(new RequestLaunchBrowser(Record.VendorUrl));                        

                    },
                    () => Record.VendorUrl != null));
            }
        }

        private RelayCommand _showDetails;

        /// <summary>
        /// Gets the ShowDetails.
        /// </summary>
        public RelayCommand ShowDetails
        {
            get
            {
                return _showDetails
                    ?? (_showDetails = new RelayCommand(
                    () =>
                    {
                        if (!ShowDetails.CanExecute(null))
                        {
                            return;
                        }

                        var dlg = new ProductScanner.App.Controls.CommitRecordDlg(Record);
                        dlg.ShowDialog();
                    },
                    () => true));
            }
        }
    }
}
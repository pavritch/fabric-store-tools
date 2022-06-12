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
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App.ViewModels
{
    public class VendorContextMenuViewModel : ViewModelBase
    {

        public VendorContextMenuViewModel(IVendorModel vendor)
        {
            this.Vendor = vendor;
        }

        private IVendorModel _vendor = null;
        public IVendorModel Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                Set(() => Vendor, ref _vendor, value);
            }
        }

        #region Scanning Commands

        private RelayCommand _startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(
                    async () =>
                    {
                        if (!StartCommand.CanExecute(null))
                        {
                            return;
                        }

                        if (Vendor == null)
                            return;

                        if (Vendor.HasPendingCommitBatches)
                        {
                            var prompt = string.Format("Discard {0:N0} pending batches?", Vendor.PendingCommitBatchCount);
                            var activity = new ConfirmationPromptActivity(prompt);

                            var okayToDiscard = await activity.Show(activity) == ActivityResult.Success;

                            if (!okayToDiscard)
                                return;

                            var pendingBatches = await Vendor.GetPendingCommitBatchSummariesAsync();
                            foreach (var batch in pendingBatches)
                                await Vendor.DiscardBatchAsync(batch.Id);
                        }

                        var result = await Vendor.StartScanning(ScanOptions.None);

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to start new scanning operation.");
                    },
                    () => Vendor != null && Vendor.IsScanningStartable));
            }
        }

        private RelayCommand _suspendCommand;
        public RelayCommand SuspendCommand
        {
            get
            {
                return _suspendCommand
                    ?? (_suspendCommand = new RelayCommand(
                    async () =>
                    {
                        if (Vendor == null)
                            return;

                        if (!SuspendCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.SuspendScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to suspend current scanning operation.");

                    },
                    () => Vendor != null && Vendor.IsScanningSuspendable));
            }
        }

        private RelayCommand _resumeCommand;
        public RelayCommand ResumeCommand
        {
            get
            {
                return _resumeCommand
                    ?? (_resumeCommand = new RelayCommand(
                    async () =>
                    {
                        if (Vendor == null)
                            return;

                        if (!ResumeCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.ResumeScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to resume current scanning operation.");


                    },
                    () => Vendor != null && Vendor.IsScanningResumable));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    async () =>
                    {
                        if (Vendor == null)
                            return;

                        if (!CancelCommand.CanExecute(null))
                        {
                            return;
                        }

                        var result = await Vendor.CancelScanning();

                        if (result != ScanningActionResult.Success)
                            App.Current.ReportErrorAlert("Failed to cancel current scanning operation.");

                    },
                    () => Vendor != null && Vendor.IsScanningCancellable));
            }
        }

        #endregion

        #region Pending Batch Commands

        private RelayCommand _commitPendingBatchesCommand;
        public RelayCommand CommitPendingBatchesCommand
        {
            get
            {
                return _commitPendingBatchesCommand
                    ?? (_commitPendingBatchesCommand = new RelayCommand(
                    () =>
                    {
                        if (!CommitPendingBatchesCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => false));
            }
        }

        private RelayCommand _discardPendingBatchesCommand;
        public RelayCommand DiscardPendingBatchesCommand
        {
            get
            {
                return _discardPendingBatchesCommand
                    ?? (_discardPendingBatchesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DiscardPendingBatchesCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => false));
            }
        }

        private RelayCommand _deletePendingBatchesCommand;
        public RelayCommand DeletePendingBatchesCommand
        {
            get
            {
                return _deletePendingBatchesCommand
                    ?? (_deletePendingBatchesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DeletePendingBatchesCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => false));
            }
        }
        #endregion

        #region Misc Commands
        private RelayCommand _clearLogCommand;

        /// <summary>
        /// Gets the ClearLogCommand.
        /// </summary>
        /// <remarks>
        /// As implemented, clears more than just log. Also resets/clears to full idle state.
        /// </remarks>
        public RelayCommand ClearLogCommand
        {
            get
            {
                return _clearLogCommand
                    ?? (_clearLogCommand = new RelayCommand(
                    () =>
                    {
                        if (Vendor == null)
                            return;

                        if (!ClearLogCommand.CanExecute(null))
                        {
                            return;
                        }

                        Vendor.ClearScanningState();
                    },
                    () => Vendor != null && Vendor.IsScanningLogClearable));
            }
        }

        private RelayCommand _clearWarningCommand;
        public RelayCommand ClearWarningCommand
        {
            get
            {
                return _clearWarningCommand
                    ?? (_clearWarningCommand = new RelayCommand(
                    () =>
                    {
                        if (!ClearWarningCommand.CanExecute(null))
                        {
                            return;
                        }

                        Vendor.ClearWarning();
                    },
                    () => Vendor != null && Vendor.HasWarning));
            }
        }

        private RelayCommand _deleteCachedFilesCommand;
        public RelayCommand DeleteCachedFilesCommand
        {
            get
            {
                return _deleteCachedFilesCommand
                    ?? (_deleteCachedFilesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DeleteCachedFilesCommand.CanExecute(null))
                        {
                            return;
                        }

                        VendorScanFilesTabViewModel.DeleteCachedFilesActivity(Vendor);
                    },
                    () => Vendor != null && Vendor.IsFileCacheClearable));
            }
        }
        #endregion

        #region Navigation Commands

        private RelayCommand _scannerPageCommand;
        public RelayCommand ScannerPageCommand
        {
            get
            {
                return _scannerPageCommand
                    ?? (_scannerPageCommand = new RelayCommand(
                    () =>
                    {
                        if (!ScannerPageCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => false));
            }
        }
        #endregion

    }
}
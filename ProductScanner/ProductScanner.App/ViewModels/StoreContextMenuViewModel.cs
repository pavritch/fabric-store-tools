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
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App.ViewModels
{
    public class StoreContextMenuViewModel : ViewModelBase
    {

        public StoreContextMenuViewModel(IStoreModel store)
        {
            this.Store = store;
            HookMessages();
        }

        private IStoreModel _store = null;
        public IStoreModel Store
        {
            get
            {
                return _store;
            }
            set
            {
                Set(() => Store, ref _store, value);
            }
        }


        private void HookMessages()
        {
            Messenger.Default.Register<VendorChangedNotification>(this, (msg) =>
            {
                if (!msg.Vendor.ParentStore.Equals(Store))
                    return;

                Refresh();
            });

            Messenger.Default.Register<ScanningOperationNotification>(this, (msg) =>
            {
                if (!msg.Vendor.ParentStore.Equals(Store))
                    return;

                Refresh();
            });
        }

        private void Refresh()
        {
            StartCommand.RaiseCanExecuteChanged();
            SuspendCommand.RaiseCanExecuteChanged();
            ResumeCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();

            DeleteCachedFilesCommand.RaiseCanExecuteChanged();
            ClearLogCommand.RaiseCanExecuteChanged();

            DeletePendingBatchesCommand.RaiseCanExecuteChanged();
            DiscardPendingBatchesCommand.RaiseCanExecuteChanged();
            CommitPendingBatchesCommand.RaiseCanExecuteChanged();
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

                        var prompt = string.Format("Start {0:N0} scanning operations.", Store.Vendors.Where(e => e.Status == Core.DataInterfaces.VendorStatus.AutoPilot).Count(e => e.IsScanningStartable));

                        var inputsControl = new Controls.StartScanningInputs();
                        var inputs = inputsControl.DataContext as StartScanningInputsViewModel;
                        ConfirmationPromptActivity activity;
                        activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                        {
                            inputs.IsDisabled = true;
                            // cancel not supported, token ignored
                            var options = inputs.Options.Where(e => e.IsChecked).Select(e => e.Option).Cast<int>().Sum();
                            var result = await Store.StartAll((ScanOptions)options);

                            return result ? ActivityResult.Success : ActivityResult.Failed;

                        })
                        {
                            CustomElement = inputsControl,
                        };

                        await activity.Show(activity);

                    },
                    () => Store.Vendors.Any(e => e.IsScanningStartable && e.Status == Core.DataInterfaces.VendorStatus.AutoPilot)));
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

                        if (!SuspendCommand.CanExecute(null))
                        {
                            return;
                        }

                        var prompt = string.Format("Suspend {0:N0} scanning operations.", Store.ScanningVendorsCount);

                        var activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                        {
                            // cancel not supported, token ignored

                            var result = await Store.SuspendAll();
                            return result == true ? ActivityResult.Success : ActivityResult.Failed;

                        }, false);

                        await activity.Show(activity);

                    },
                    () => Store.IsAnyScanning));
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
                    
                        if (!ResumeCommand.CanExecute(null))
                        {
                            return;
                        }

                        var prompt = string.Format("Resume {0:N0} scanning operations.", Store.SuspendedVendorsCount);

                        var activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                        {
                            // cancel not supported, token ignored

                            var result = await Store.ResumeAll();
                            return result == true ? ActivityResult.Success : ActivityResult.Failed;

                        }, false);

                        await activity.Show(activity);

                    },
                    () => Store.IsAnySuspended));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    async  () =>
                    {

                        if (!CancelCommand.CanExecute(null))
                        {
                            return;
                        }

                        var prompt = string.Format("Cancel {0:N0} active operations.", Store.IsScanningOrSuspendedCount);

                        var activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                        {
                            // cancel not supported, token ignored

                            var result = await Store.CancelAll();
                            return result == true ? ActivityResult.Success : ActivityResult.Failed;

                        }, false);

                        await activity.Show(activity);
                    },
                    () => Store.IsScanningOrSuspendedCount > 0));
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

                        // makes all the finished, failed, cancelled vendors go back to a clean state,
                        // clean logs, etc.

                        foreach (var v in Store.Vendors.Where(e => e.IsScanningLogClearable))
                        {
                            v.ClearWarning();
                            v.ClearScanningState();
                        }

                    },
                    () => Store.Vendors.Where(e => e.IsScanningLogClearable).Count() > 0));
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

                        StoreDashboardViewModel.DeleteCachedFilesActivity(Store);
                    },
                    () => Store.Vendors.Where(e => e.IsFullyImplemented && !e.IsScanning && !e.IsSuspended).Count() > 0));
            }
        }
        #endregion

       
    }
}
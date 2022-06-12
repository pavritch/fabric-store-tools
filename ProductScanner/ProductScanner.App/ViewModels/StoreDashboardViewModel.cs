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

namespace ProductScanner.App.ViewModels
{
    public class StoreDashboardViewModel : StoreContentPageViewModel
    {
#if DEBUG
        public StoreDashboardViewModel()
            : this((new DesignStoreModel { Name = "InsideFabric", Key = StoreType.InsideFabric}) as IStoreModel)
        {
            // dev only - normally the navigation system needs to create one of these viewmodels with a reference
            // to the desired store.

            VendorCount = 32;
            ScanningCount = 4;
            CommitsCount = 7;
            SuspendedCount = 2;
            DisabledCount = 2;
        }
#endif
        public StoreDashboardViewModel(IStoreModel store)
            : base(store)
        {
            PageSubTitle = "Dashboard";
            PageType = ContentPageTypes.StoreDashboard;
            RequiresToBeCached = true;
            IsNavigationJumpTarget = true;
            Heading = string.Format("Dashboard for {0}.", Store.Name);
            BreadcrumbTemplate = "{Home}/{Store}/Dashboard";
            if (!IsInDesignMode)
            {
                HookMessages();
            }
            Refresh();
        }

        private void Refresh()
        {
            if (Store == null)
                return;

            VendorCount = Store.Vendors.Count();
            ScanningCount = Store.Vendors.Where(e => e.ScannerState == ScannerState.Scanning).Count();
            CommitsCount = Store.Vendors.Where(e => e.ScannerState == ScannerState.Committable).Count();
            SuspendedCount = Store.Vendors.Where(e => e.ScannerState == ScannerState.Suspended).Count();
            DisabledCount = Store.Vendors.Where(e => e.ScannerState == ScannerState.Disabled).Count();
            InvalidateButtons();
        }

        private void HookMessages()
        {
            MessengerInstance.Register<VendorChangedNotification>(this, (msg) =>
            {
                Refresh();
            });

            MessengerInstance.Register<ScanningOperationNotification>(this, (msg) =>
            {
                Refresh();
            });
        }

        private void InvalidateButtons()
        {
            ClearIdleStatesCommand.RaiseCanExecuteChanged();
        }

        private async Task<bool> RefreshAsync()
        {
            IsRefreshing = true;

            // show visual for at least some minimal amount

            var tasks = new List<Task>()
                {
                    Task.Delay(600),
                    //Task.Run(() => Refresh())
                };

            await Task.WhenAll(tasks);
            await DispatcherHelper.RunAsync(() =>
            {
                IsRefreshing = false;
            });

            return true;
        }


        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                Set(() => IsRefreshing, ref _isRefreshing, value);
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="Heading" /> property's name.
        /// </summary>
        public const string HeadingPropertyName = "Heading";

        private string _heading = null;

        /// <summary>
        /// Sets and gets the Heading property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Heading
        {
            get
            {
                return _heading;
            }

            set
            {
                if (_heading == value)
                {
                    return;
                }

                _heading = value;
                RaisePropertyChanged(() => Heading);
            }
        }


        /// <summary>
        /// Total number of known vendors.
        /// </summary>
        private int _vendorCount = 0;
        public int VendorCount
        {
            get
            {
                return _vendorCount;
            }
            set
            {
                Set(() => VendorCount, ref _vendorCount, value);
            }
        }

        /// <summary>
        /// Number of vendors presently scanning.
        /// </summary>
        private int _scanningCount = 0;
        public int ScanningCount
        {
            get
            {
                return _scanningCount;
            }
            set
            {
                Set(() => ScanningCount, ref _scanningCount, value);
            }
        }

        /// <summary>
        /// Number of vendors which have commits ready.
        /// </summary>
        private int _commitsCount = 0;
        public int CommitsCount
        {
            get
            {
                return _commitsCount;
            }
            set
            {
                Set(() => CommitsCount, ref _commitsCount, value);
            }
        }


        /// <summary>
        /// Number of vendor scans presently in a suspended (resumable) state.
        /// </summary>
        private int _suspendedCount = 0;
        public int SuspendedCount
        {
            get
            {
                return _suspendedCount;
            }
            set
            {
                Set(() => SuspendedCount, ref _suspendedCount, value);
            }
        }


        /// <summary>
        /// Disabled vendors.
        /// </summary>
        private int _disabledCount = 0;
        public int DisabledCount
        {
            get
            {
                return _disabledCount;
            }
            set
            {
                Set(() => DisabledCount, ref _disabledCount, value);
            }
        }


        private RelayCommand _showCompletedScansSummary;
        public RelayCommand ShowCompletedScansSummary
        {
            get
            {
                return _showCompletedScansSummary
                    ?? (_showCompletedScansSummary = new RelayCommand(
                    () =>
                    {
                        if (!ShowCompletedScansSummary.CanExecute(null))
                        {
                            return;
                        }

                        App.Current.ReportErrorAlert("Not Implemented");
                    },
                    () => true));
            }
        }

        private RelayCommand _showLoginsSummary;

        /// <summary>
        /// Show the page of login tests across all vendors for this store.
        /// </summary>
        public RelayCommand ShowLoginsSummary
        {
            get
            {
                return _showLoginsSummary
                    ?? (_showLoginsSummary = new RelayCommand(
                    () =>
                    {
                        if (!ShowLoginsSummary.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.StoreLoginsSummary);

                        
                    },
                    () => true));
            }
        }

        private RelayCommand _showCommitsSummary;

        /// <summary>
        /// Show the page with pending commits across all vendors.
        /// </summary>
        public RelayCommand ShowCommitsSummary
        {
            get
            {
                return _showCommitsSummary
                    ?? (_showCommitsSummary = new RelayCommand(
                    () =>
                    {
                        if (!ShowCommitsSummary.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.StoreCommitSummary);
                        
                    },
                    () => true));
            }
        }

        private RelayCommand _showScanningSummary;

        /// <summary>
        /// Gets the ShowScanSummary.
        /// </summary>
        public RelayCommand ShowScanningSummary
        {
            get
            {
                return _showScanningSummary
                    ?? (_showScanningSummary = new RelayCommand(
                    () =>
                    {
                        if (!ShowScanningSummary.CanExecute(null))
                        {
                            return;
                        }

                        RequestNavigation(ContentPageTypes.StoreScanSummary);
                        
                    },
                    () => true));
            }
        }


        private RelayCommand _showTestsSummary;
        public RelayCommand ShowTestsSummary
        {
            get
            {
                return _showTestsSummary
                    ?? (_showTestsSummary = new RelayCommand(
                    () =>
                    {
                        if (!ShowTestsSummary.CanExecute(null))
                        {
                            return;
                        }

                        App.Current.ReportErrorAlert("Not Implemented");
                    },
                    () => true));
            }
        }

        // not used for now
        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                    async () =>
                    {
                        if (!RefreshCommand.CanExecute(null))
                        {
                            return;
                        }

                        await RefreshAsync();

                    },
                    () => !IsRefreshing));
            }
        }


        private RelayCommand _exportCommand;

        /// <summary>
        /// Gets the ExportCommand.
        /// </summary>
        public RelayCommand ExportCommand
        {
            get
            {
                return _exportCommand
                    ?? (_exportCommand = new RelayCommand(
                    () =>
                    {
                        if (!ExportCommand.CanExecute(null))
                        {
                            return;
                        }

                        Task.Run(() =>
                        {
                            var vendorRecords = Store.Vendors.Select(e => new VendorExportRecord(e)).ToList();
                            var suggestedName = string.Format("{0} Vendors.xlsx", Store.Name);
                            ExportManager.SaveExcelFile(vendorRecords, suggestedName);
                        });

                    },
                    () => true));
            }
        }
        /// <summary>
        /// Invoke to spin through all idle vendors and clear out any prior scanning state.
        /// </summary>
        private RelayCommand _clearIdleStatesCommand;
        public RelayCommand ClearIdleStatesCommand
        {
            get
            {
                return _clearIdleStatesCommand
                    ?? (_clearIdleStatesCommand = new RelayCommand(
                    () =>
                    {
                        if (!ClearIdleStatesCommand.CanExecute(null))
                        {
                            return;
                        }

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

        private RelayCommand _deleteCacheFilesCommand;
        public RelayCommand DeleteCacheFilesCommand
        {
            get
            {
                return _deleteCacheFilesCommand
                    ?? (_deleteCacheFilesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DeleteCacheFilesCommand.CanExecute(null))
                        {
                            return;
                        }

                        DeleteCachedFilesActivity(Store);
                    },
                    () => Store.Vendors.Where(e => e.IsFileCacheClearable).Count() > 0));
            }
        }

        private RelayCommand _checkCredentialsCommand;

        public RelayCommand CheckCredentialsCommand
        {
            get
            {
                return _checkCredentialsCommand
                    ?? (_checkCredentialsCommand = new RelayCommand(
                    () =>
                    {
                        if (!CheckCredentialsCommand.CanExecute(null))
                        {
                            return;
                        }

                        CheckCredentialsActivity();
                    },
                    () => true));
            }
        }

        private RelayCommand _startAllCommand;

        public RelayCommand StartAllCommand
        {
            get
            {
                return _startAllCommand
                    ?? (_startAllCommand = new RelayCommand(
                    () =>
                    {
                        if (!StartAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => true));
            }
        }

        private RelayCommand _suspendAllCommand;

        public RelayCommand SuspendAllCommand
        {
            get
            {
                return _suspendAllCommand
                    ?? (_suspendAllCommand = new RelayCommand(
                    () =>
                    {
                        if (!SuspendAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => true));
            }
        }

        private RelayCommand _resumeAllCommand;

        public RelayCommand ResumeAllCommand
        {
            get
            {
                return _resumeAllCommand
                    ?? (_resumeAllCommand = new RelayCommand(
                    () =>
                    {
                        if (!ResumeAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => true));
            }
        }

        private RelayCommand _cancellAllCommand;

        public RelayCommand CancelAllCommand
        {
            get
            {
                return _cancellAllCommand
                    ?? (_cancellAllCommand = new RelayCommand(
                    () =>
                    {
                        if (!CancelAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        
                    },
                    () => true));
            }
        }


        /// <summary>
        /// Clean up cached files.
        /// </summary>
        public static async void DeleteCachedFilesActivity(IStoreModel store)
        {

            var inputsControl = new ProductScanner.App.Controls.DeleteCachedFilesInputs();
            var inputs = inputsControl.DataContext as DeleteCachedFilesInputsViewModel;


            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Delete Cached Files?",
                IsIndeterminateProgress = false,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = inputsControl,
                IsAcceptanceDisabled = false,

                OnAccept = async (a) =>
                {
                    inputs.IsDisabled = true;

                    try
                    {
                        string errMsg = null;

                        var selectedVendors = store.Vendors.Where(e => e.IsFullyImplemented && !e.IsScanning && !e.IsSuspended).OrderBy(e => e.Name).ToList();

                        int countCompleted = 0;
                        foreach (var vendor in selectedVendors)
                        {
                            a.StatusMessage = string.Format("Deleting files for {0}...", vendor.Name);

                            // we do not pass progress - we want to deal with that here at this level just to keep things
                            // simple in terms of trying to compute percentages across multiple vendors, etc.

                            var result = await vendor.DeleteCachedFilesAsync(inputs.DayCount, a.CancelToken, null);

                            if (a.CancelToken.IsCancellationRequested)
                                break;

                            if (result != ActivityResult.Success)
                            {
                                errMsg = string.Format("Error deleting files for {0}. Action terminated.", vendor.Name);
                                break; // skip remaining 
                            }

                            countCompleted++;
                            var startValue = a.PercentComplete;
                            var endValue = selectedVendors.Count == 0 ? 0 : ((double)countCompleted * 100.0) / (double)selectedVendors.Count;
                            var secDelay = .05;
                            var secDuration = .750;
                            var currentClock = 0.0;
                            while (currentClock <= secDuration)
                            {
                                var value = PennerDoubleAnimation.CircEaseInOut(currentClock, startValue, endValue - startValue, secDuration);
                                a.PercentComplete = value;
                                await Task.Delay(TimeSpan.FromSeconds(secDelay));
                                currentClock += secDelay;
                            }
                        }

                        // if was cancelled by user, then we don't need to set cancel here

                        if (!a.CancelToken.IsCancellationRequested)
                        {
                            if (errMsg == null)
                                a.SetCompleted(ActivityResult.Success, "Finished deleting cached files.");
                            else
                                a.SetCompleted(ActivityResult.Failed, errMsg);
                        }
                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }
                    finally
                    {
                        // finalize the task, return from original await
                        a.FinishUp();
                    }
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here
                    // FinishUp() called automatically upon return from this method.
                },
            };


            await activity.Show(activity);
        }

#if false // sample activity
        private async void DeleteCachedFilesActivity()
        {

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Delete All Cached Files?",
                IsIndeterminateProgress = false,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = true,

                OnAccept = (a) =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            a.StatusMessage = "Pretending to delete files...";

                            for (double i = 0; i <= 500.0; i++)
                            {
                                a.PercentComplete = (i / 500.0) * 100.0;
                                await Task.Delay(20);

                                if (a.CancelToken.IsCancellationRequested)
                                    break;
                            }

                            // if was cancelled by user, then we don't need to set cancel here

                            if (!a.CancelToken.IsCancellationRequested)
                                a.SetCompleted(ActivityResult.Success, "Finished deleting all cached files.");
                        }
                        catch (Exception Ex)
                        {
                            a.SetCompleted(ActivityResult.Failed, Ex.Message);
                        }
                        finally
                        {
                            // finalize the task, return from original await
                            a.FinishUp();
                        }
                    });
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here

                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.
                },
            };

            await activity.Show(activity);
        }
#endif

        private async void CheckCredentialsActivity()
        {

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Check Vendor Credentials?",
                IsIndeterminateProgress = false,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = true,

                OnAccept = (a) =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            a.StatusMessage = "Pretending to check credentials...";

                            for (double i = 0; i <= 500.0; i++)
                            {
                                a.PercentComplete = (i / 500.0) * 100.0;
                                await Task.Delay(20);

                                if (a.CancelToken.IsCancellationRequested)
                                    break;
                            }

                            // if was cancelled by user, then we don't need to set cancel here

                            if (!a.CancelToken.IsCancellationRequested)
                                a.SetCompleted(ActivityResult.Success, "Finished checking credentials.");
                        }
                        catch (Exception Ex)
                        {
                            a.SetCompleted(ActivityResult.Failed, Ex.Message);
                        }
                        finally
                        {
                            // finalize the task, return from original await
                            a.FinishUp();
                        }
                    });
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here

                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.
                },
            };

            await activity.Show(activity);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Telerik.Windows.Controls;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// VM for the home page.
    /// </summary>
    /// <remarks>
    /// Okay to recreate each time.
    /// </remarks>
    public class HomeDashboardViewModel : ContentPageViewModel
    {
#if DEBUG
        public HomeDashboardViewModel()
            : this(new DesignAppModel() as IAppModel)
        {
            StoreCount = 5;
            VendorCount = 32;
            ScanningCount = 22;
            CommitsCount = 7;
            SuspendedCount = 2;
            DisabledCount = 2;
        }
#endif

        // the gridview on this page binds directly to IAppModel.Stores,
        // and data for all columns comes diectly from the individual Store models.

        public HomeDashboardViewModel(IAppModel appModel)
        {
            PageTitle = "Product Scanner";
            PageSubTitle = "Dashboard";
            BreadcrumbTemplate = "{Home}";

            this.AppModel = appModel;

            if (!IsInDesignMode)
            {
                Refresh();
                HookMessages();
            }
        }


        #region Local Methods
        private void HookMessages()
        {
            MessengerInstance.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch (msg.Kind)
                {
                    case Announcement.AppModelRefreshCompleted:
                        Refresh();
                        break;

                    default:
                        break;
                }
            });

            MessengerInstance.Register<VendorChangedNotification>(this, (msg) =>
            {
                Refresh();
            });

            MessengerInstance.Register<ScanningOperationNotification>(this, (msg) =>
            {
                Refresh();
            });
        }


        private void Refresh()
        {
            if (AppModel == null)
                return;

            StoreCount = AppModel.Stores.Count;
            VendorCount = AppModel.Stores.SelectMany(e => e.Vendors).Count();

            ScanningCount = AppModel.Stores.SelectMany(e => e.Vendors).Where(e => e.ScannerState == ScannerState.Scanning).Count();
            CommitsCount = AppModel.Stores.SelectMany(e => e.Vendors).Where(e => e.ScannerState == ScannerState.Committable).Count();
            SuspendedCount = AppModel.Stores.SelectMany(e => e.Vendors).Where(e => e.ScannerState == ScannerState.Suspended).Count();
            DisabledCount = AppModel.Stores.SelectMany(e => e.Vendors).Where(e => e.ScannerState == ScannerState.Disabled).Count();

            InvalidateButtons();
        }

        private void InvalidateButtons()
        {
            SuspendAllCommand.RaiseCanExecuteChanged();
            CancelAllCommand.RaiseCanExecuteChanged();
            ClearAllWarningsCommand.RaiseCanExecuteChanged();
        }

        private async Task<bool> RefreshAsync()
        {
            IsRefreshing = true;

            // show visual for at least some minimal amount

            var tasks = new List<Task>()
                {
                    Task.Delay(600),
                    Task.Run(() => Refresh())
                };

            await Task.WhenAll(tasks);
            await DispatcherHelper.RunAsync(() =>
            {
                IsRefreshing = false;
            });

            return true;
        } 
        #endregion

        #region Public Properties

        private IAppModel _appModel = null;
        public IAppModel AppModel
        {
            get
            {
                return _appModel;
            }
            set
            {
                Set(() => AppModel, ref _appModel, value);
            }
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
        /// Total store count.
        /// </summary>
        private int _storeCount = 0;
        public int StoreCount
        {
            get
            {
                return _storeCount;
            }
            set
            {
                Set(() => StoreCount, ref _storeCount, value);
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
        #endregion

        #region Commands
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

        private RelayCommand _suspendAllCommand;
        public RelayCommand SuspendAllCommand
        {
            get
            {
                return _suspendAllCommand
                    ?? (_suspendAllCommand = new RelayCommand(
                    async () =>
                    {
                        if (!SuspendAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        var prompt = string.Format("Suspend {0:N0} scanning operations.", AppModel.IsScanningCount);

                        var activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                            {
                                // cancel not supported, token ignored

                                var result = await AppModel.SuspendAll();
                                return result==true ? ActivityResult.Success : ActivityResult.Failed;

                            }, false);

                        await activity.Show(activity);
                    },
                    () => AppModel.IsAnyScanning));
            }
        }

        private RelayCommand _cancelAllCommand;

        /// <summary>
        /// Gets the CancelAllCommand.
        /// </summary>
        public RelayCommand CancelAllCommand
        {
            get
            {
                return _cancelAllCommand
                    ?? (_cancelAllCommand = new RelayCommand(
                    async () =>
                    {
                        if (!CancelAllCommand.CanExecute(null))
                        {
                            return;
                        }

                        var prompt = string.Format("Cancel {0:N0} active operations.", AppModel.IsScanningOrSuspendedCount);

                        var activity = new ConfirmationPromptActivity(prompt, async (cancelToken) =>
                        {
                            // cancel not supported, token ignored

                            var result = await AppModel.CancelAll();
                            return result == true ? ActivityResult.Success : ActivityResult.Failed;

                        }, false);

                        await activity.Show(activity);
                        
                    },
                    () => AppModel.IsScanningOrSuspendedCount > 0));
            }
        }

        /// <summary>
        /// Clear all warnings across all stores, all vendors.
        /// </summary>
        private RelayCommand _clearAllWarningsCommand;
        public RelayCommand ClearAllWarningsCommand
        {
            get
            {
                return _clearAllWarningsCommand
                    ?? (_clearAllWarningsCommand = new RelayCommand(
                    () =>
                    {
                        if (!ClearAllWarningsCommand.CanExecute(null))
                        {
                            return;
                        }

                        foreach(var v in AppModel.Stores.SelectMany(e => e.Vendors))
                            v.ClearWarning();

                    },
                    () => AppModel.VendorsWithWarningsCount > 0));
            }
        }
        #endregion
    }
}
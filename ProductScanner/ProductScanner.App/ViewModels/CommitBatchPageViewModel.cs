using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Commits;
using Utilities.Extensions;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// A common control to view and commit a single batch. 
    /// </summary>
    /// <remarks>
    /// Can be used on store and vendor pages.
    /// </remarks>
    public class CommitBatchPageViewModel : ViewModelBase 
    {
        private IAppModel appModel;
        private IScannerDatabaseConnector dbScanner;
        private IStoreDatabaseConnector dbStore;

        public CommitBatchPageViewModel(IAppModel appModel, IScannerDatabaseConnector dbScanner, IStoreDatabaseConnector dbStore)
        {
            this.appModel = appModel;
            this.dbScanner = dbScanner;
            this.dbStore = dbStore;

            LogEvents = new ObservableCollection<string>();
            IsShowingGridView = true;

            if (IsInDesignMode)
            {
                SetBatchID(100, false);

                // BatchID = 4232;
                // Status = "Ready to GO";
                // DateCreated = DateTime.Now.AddDays(-1).ToString();
                // DateCommitted = DateTime.Now.AddDays(0).ToString();
                // QtySubmitted = 1234.ToString("N0");
                // QtyCommitted = 1233.ToString("N0"); 
                // BatchType = CommitBatchType.NewProducts;
                // Vendor
                // IsPending = true;
            }
            else
            {
                IsBusy = true;
            }
        }

        #region Public Methods
        public async void SetBatchID(int? batchID, bool isRefreshing=false)
        {
            if (!isRefreshing)
                IsBusy = true;

            if (!IsInDesignMode)
                InvalidateButtons();

            BatchID = batchID;

            // retrieve the full batch record from the scanner database
            var details = await dbScanner.GetCommitBatchAsync(batchID.GetValueOrDefault());

            // update some header information so the UX fills in quickly even though we're
            // still needing to fetch the supplemental columns.

            // all needs to run on the UX thread

            BatchDetails = details;

            if (BatchDetails == null)
            {
                Status = "Invalid Batch";
                DateCreated = "n/a";
                DateCommitted = "n/a";
                QtySubmitted = "0";
                QtyCommitted = "0";
                BatchType = default(CommitBatchType);
                Vendor = null;
                IsPending = false;
                LogEvents = new ObservableCollection<string>();
                ViewData = new ObservableCollection<IViewData>();
                return;
            }

            var viewDataType = GetViewDataType(details.BatchType);

            Vendor = appModel.Stores.SelectMany(e => e.Vendors).First(e => e.Vendor.Id == BatchDetails.VendorId);

            Status = BatchDetails.Status.DescriptionAttr();
            DateCreated = BatchDetails.Created.ToString();
            DateCommitted = BatchDetails.DateCommitted.HasValue ? BatchDetails.DateCommitted.Value.ToString() : "Never";
            QtySubmitted = BatchDetails.QtySubmitted.ToString("N0");
            QtyCommitted = BatchDetails.QtyCommitted.GetValueOrDefault().ToString("N0");
            BatchType = BatchDetails.BatchType;
            VendorName = ShowVendorName ? Vendor.Name : null;
            IsPending = BatchDetails.Status == CommitBatchStatus.Pending;
            SetFreezeColumnsSupport(viewDataType);

            var tasks = new List<Task>()
            {
                CreateViewDataAsync(viewDataType, Vendor.ParentStore.Key, BatchDetails)
            };

            // let the WPF UX transition take place before loading the grid with tons of data
            if (!isRefreshing)
                tasks.Add(Task.Delay(1000)); 

            await Task.WhenAll(tasks);

            // can skip the view creation below when refreshing since the data does not change
            // based on what does or does not get committed/discarded when some action invoked

            if (!isRefreshing)
            {
                // take the raw data and transform into something much richer for the UX and for exporting 

                List<IViewData> dataList = (tasks[0] as Task<List<IViewData>>).Result;
                var dataCollection = new ObservableCollection<IViewData>(dataList);

                // the data and view must be populated on the UX thread

                ViewData = dataCollection;
                ViewDataGrid = CreateView(viewDataType, dataCollection);
            }

            // the log data can change upon a refresh since the activity could have written to the log

            var logList = new List<string>();

            if (!string.IsNullOrWhiteSpace(BatchDetails.Log))
                logList = BatchDetails.Log.ConvertToListOfLines();

            LogEvents = new ObservableCollection<string>(logList);

            if (!isRefreshing)
                IsBusy = false;

            if (!IsInDesignMode)
                InvalidateButtons();
        } 
        #endregion

        #region Local Methods

        /// <summary>
        /// Return the Type for the named batch using reflection.
        /// </summary>
        private Type GetViewDataType(CommitBatchType batchType)
        {
            var asy = Assembly.GetExecutingAssembly();

            foreach (var t in asy.TypesWithAttributes(typeof(ViewDataAttribute)))
            {
                var attr = t.GetCustomAttribute<ViewDataAttribute>();

                Debug.Assert(attr != null);

                if (attr.BatchType == batchType)
                    return t;
            }

            // should never get here unless programming bug
            throw new ArgumentException("GetViewDataType(CommitBatchType batchType)");
        }

        /// <summary>
        /// Given a kind of batch, create the corresponding view (user control) with the current data set.
        /// </summary>
        /// <param name="viewDataType"></param>
        /// <returns></returns>
        private Control CreateView(Type viewDataType, ObservableCollection<IViewData> data)
        {
            // must create this view on the UX thread

            var attr = viewDataType.GetCustomAttribute<ViewDataAttribute>();
            var parameters = new object[] { data };
            var viewControl = Activator.CreateInstance(attr.Viewer, parameters) as Control;
            return viewControl;
        }

        private void SetFreezeColumnsSupport(Type viewDataType)
        {
            var attr = viewDataType.GetCustomAttribute<ViewDataAttribute>();
            if (attr.IsFreezeColumnsSupported)
            {
                IsFreezeColumnsEnabled = false;
                IsFreezeColumnsSupported = true;
            }
        }

        private Task<List<IViewData>> CreateViewDataAsync(Type viewDataType, StoreType storeKey, CommitBatchDetails batchDetails)
        {
            // need to activate the type and invoke the common static method to retreive the data.
            // public static new Task<List<IViewData>> CreateDataSetAsync(IStoreDatabaseConnector dbStore, string storeKey, byte[] gzipJsonData)

            var parameters = new object[] {dbStore, Vendor.ParentStore.Key, batchDetails.CommitData};
            var result = viewDataType.GetMethod("CreateDataSetAsync", BindingFlags.Public | BindingFlags.Static).Invoke(null, parameters) as Task<List<IViewData>>;

            return result;
        }

        private void InvalidateButtons()
        {
            CommitCommand.RaiseCanExecuteChanged();
            DiscardCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
            ShowGridViewCommand.RaiseCanExecuteChanged();
            ShowLogViewCommand.RaiseCanExecuteChanged();
        }

        private void Refresh()
        {
            SetBatchID(BatchID, true);
        }

        private bool IsExternalBlockingActivty()
        {
            return false; // TODO-LIVE: remove this line!

            // do not allow committing anything while scanning is taking place.
            // in theory, when a scan begins, batches are auto-staled and should be removed.
            //return Vendor.IsScanning;
        }

        private async void CommitBatchActivity()
        {
            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Commit Batch?",
                IsIndeterminateProgress = false,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = new ProductScanner.App.Controls.CommitBatchInputs(),
                OnAccept = async (a) =>
                {
                    var inputs = (a.CustomElement as ProductScanner.App.Controls.CommitBatchInputs).DataContext as CommitBatchInputsViewModel;
                    inputs.IsDisabled = true;

                    Progress<ActivityProgress> progressIndicator = new Progress<ActivityProgress>((p) =>
                    {
                        // so far, have not needed to deal with UX thread issue here - would have expected
                        // a problem, but since not, the code is easier because it runs synchronous with the caller, so leaving it this way

                        if (a.CancelToken.IsCancellationRequested)
                            return;

                        // if a message is provided, then update the status with it
                        if (p.Message != null)
                        {
                            a.StatusMessage = p.Message;
                        }
                        a.PercentComplete = p.PercentCompleted;
                    });

                    try
                    {
                        var result = await Vendor.CommitBatchAsync(BatchID.Value, BatchDetails.QtySubmitted, inputs.IgnoreDuplicates, a.CancelToken, progressIndicator);

                        if (a.CancelToken.IsCancellationRequested)
                            return;

                        if (result == CommitBatchResult.Successful)
                            a.SetCompleted(ActivityResult.Success, "Finished committing batch.");
                        else
                            a.SetCompleted(ActivityResult.Failed, result.DescriptionAttr());

                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }

                    // finalize the task, return from original await
                    a.FinishUp();
                    Refresh();
                },

                OnCancel = (a) =>
                {
                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.

                    if (a.HasEverRun)
                        Refresh();
                },
            };

            await activity.Show(activity);
        }

        private async void DiscardBatchActivity()
        {

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = "Discard Batch?",
                IsIndeterminateProgress = true,
                PercentComplete = 0.0,
                IsCancellable = false,
                StatusMessage = string.Empty,
                IsAutoClose = false,

                OnAccept = async (a) =>
                {
                    try
                    {
                        a.StatusMessage = "Discarding batch...";
                            
                        await Task.Delay(700); // for user experience
                        var result = await Vendor.DiscardBatchAsync(BatchID.Value);

                        await DispatcherHelper.RunAsync(() =>
                        {
                            if (result == CommitBatchResult.Successful)
                            {
                                a.SetCompleted(ActivityResult.Success, "Finished discarding batch.");
                            }
                            else
                            {
                                a.SetCompleted(ActivityResult.Failed, result.DescriptionAttr());
                            }
                        });

                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }

                    // finalize the task, return from original await

                    a.FinishUp();
                    Refresh();
                },

                OnCancel = (a) =>
                {
                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.

                    if (a.HasEverRun)
                        Refresh();
                },
            };

            await activity.Show(activity);
        }

        #endregion

        #region Public Properties

        private ObservableCollection<IViewData> _viewData = null;
        public ObservableCollection<IViewData> ViewData
        {
            get
            {
                return _viewData;
            }

            set
            {
                if (_viewData == value)
                {
                    return;
                }

                _viewData = value;
                RaisePropertyChanged(() => ViewData);
            }
        }

        private Control _viewDataGrid = null;
        public Control ViewDataGrid
        {
            get
            {
                return _viewDataGrid;
            }

            set
            {
                if (_viewDataGrid == value)
                {
                    return;
                }

                _viewDataGrid = value;
                RaisePropertyChanged(() => ViewDataGrid);
            }
        }
        
        /// <summary>
        /// Separate property so can control independent of Vendor model.
        /// </summary>
        private string _vendorName = null;
        public string VendorName
        {
            get
            {
                return _vendorName;
            }

            set
            {
                if (_vendorName == value)
                {
                    return;
                }

                _vendorName = value;
                RaisePropertyChanged(() => VendorName);
            }
        }
        /// <summary>
        /// Orignal record fetched from SQL.
        /// </summary>
        private CommitBatchDetails _batchDetails = null;
        public CommitBatchDetails BatchDetails
        {
            get
            {
                return _batchDetails;
            }

            set
            {
                if (_batchDetails == value)
                {
                    return;
                }

                _batchDetails = value;
                RaisePropertyChanged(() => BatchDetails);
            }
        }


        private bool _showVendorName = false;
        public bool ShowVendorName
        {
            get
            {
                return _showVendorName;
            }

            set
            {
                if (_showVendorName == value)
                {
                    return;
                }

                _showVendorName = value;
                RaisePropertyChanged(() => ShowVendorName);
                if (Vendor != null)
                    VendorName = value ? Vendor.Name : null;
            }
        }

        /// <summary>
        /// Determins if the freeze columns checkbox is visible.
        /// </summary>
        private bool _isFreezeColumnsSupported = false;
        public bool IsFreezeColumnsSupported
        {
            get
            {
                return _isFreezeColumnsSupported;
            }

            set
            {
                if (_isFreezeColumnsSupported == value)
                {
                    return;
                }

                _isFreezeColumnsSupported = value;
                RaisePropertyChanged(() => IsFreezeColumnsSupported);
            }
        }

        /// <summary>
        /// This is the checkbox setting - two way.
        /// </summary>
        private bool _isFreezeColumnsEnabled = false;
        public bool IsFreezeColumnsEnabled
        {
            get
            {
                return _isFreezeColumnsEnabled;
            }

            set
            {
                if (_isFreezeColumnsEnabled == value)
                {
                    return;
                }

                _isFreezeColumnsEnabled = value;
                // tell the child view control
                MessengerInstance.Send(new AnnouncementMessage(value ? Announcement.RequestFreezeGridColumns : Announcement.RequestUnFreezeGridColumns));
                RaisePropertyChanged(() => IsFreezeColumnsEnabled);
            }
        }
        /// <summary>
        /// When is busy loading and preprocessing data.
        /// </summary>
        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
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


        private ObservableCollection<string> _logEvents = null;
        public ObservableCollection<string> LogEvents
        {
            get
            {
                return _logEvents;
            }

            set
            {
                if (_logEvents == value)
                {
                    return;
                }

                _logEvents = value;
                RaisePropertyChanged(() => LogEvents);
            }
        }

        /// <summary>
        /// Indicates if batch is still pending commit - which determines if buttons show/work.
        /// </summary>
        private bool _isPending = false;
        public bool IsPending
        {
            get
            {
                return _isPending;
            }

            set
            {
                if (_isPending == value)
                {
                    return;
                }

                _isPending = value;
                RaisePropertyChanged(() => IsPending);
            }
        }

        private bool _isShowingGridView = true;
        public bool IsShowingGridView
        {
            get
            {
                return _isShowingGridView;
            }

            set
            {
                if (_isShowingGridView == value)
                {
                    return;
                }

                _isShowingGridView = value;
                RaisePropertyChanged(() => IsShowingGridView);
            }
        }

        private bool _isShowingLogView = false;
        public bool IsShowingLogView
        {
            get
            {
                return _isShowingLogView;
            }

            set
            {
                if (_isShowingLogView == value)
                {
                    return;
                }

                _isShowingLogView = value;
                RaisePropertyChanged(() => IsShowingLogView);
            }
        }


        private string _dateCreated = null;
        public string DateCreated
        {
            get
            {
                return _dateCreated;
            }

            set
            {
                if (_dateCreated == value)
                {
                    return;
                }

                _dateCreated = value;
                RaisePropertyChanged(() => DateCreated);
            }
        }


        private string _dateCommitted = null;
        public string DateCommitted
        {
            get
            {
                return _dateCommitted;
            }

            set
            {
                if (_dateCommitted == value)
                {
                    return;
                }

                _dateCommitted = value;
                RaisePropertyChanged(() => DateCommitted);
            }
        }

        private string _qtySubmitted = null;
        public string QtySubmitted
        {
            get
            {
                return _qtySubmitted;
            }

            set
            {
                if (_qtySubmitted == value)
                {
                    return;
                }

                _qtySubmitted = value;
                RaisePropertyChanged(() => QtySubmitted);
            }
        }

        private string _qtyCommitted = null;
        public string QtyCommitted
        {
            get
            {
                return _qtyCommitted;
            }

            set
            {
                if (_qtyCommitted == value)
                {
                    return;
                }

                _qtyCommitted = value;
                RaisePropertyChanged(() => QtyCommitted);
            }
        }
        private CommitBatchType _batchType = default(CommitBatchType);
        public CommitBatchType BatchType
        {
            get
            {
                return _batchType;
            }

            set
            {
                if (_batchType == value)
                {
                    return;
                }

                _batchType = value;
                RaisePropertyChanged(() => BatchType);
            }
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
                if (_vendor == value)
                {
                    return;
                }

                _vendor = value;
                RaisePropertyChanged(() => Vendor);
            }
        }

        /// <summary>
        /// Slightly (maybe) changed status text to be more suitable for this page.
        /// </summary>
        /// <remarks>
        /// Intended for upper right corner display field.
        /// </remarks>
        private string _status = null;
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                RaisePropertyChanged(() => Status);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _commitCommand;
        public RelayCommand CommitCommand
        {
            get
            {
                return _commitCommand
                    ?? (_commitCommand = new RelayCommand(
                    () =>
                    {
                        if (!CommitCommand.CanExecute(null))
                        {
                            return;
                        }

                        CommitBatchActivity();
                    },
                    () => !IsBusy && IsPending && !IsExternalBlockingActivty()));
            }
        }

        private RelayCommand _discardCommand;
        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                    () =>
                    {
                        if (!DiscardCommand.CanExecute(null))
                        {
                            return;
                        }

                        DiscardBatchActivity();
                    },
                    () => !IsBusy && IsPending && !IsExternalBlockingActivty()));
            }
        }

        private RelayCommand _showGridViewCommand;
        public RelayCommand ShowGridViewCommand
        {
            get
            {
                return _showGridViewCommand
                    ?? (_showGridViewCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowGridViewCommand.CanExecute(null))
                        {
                            return;
                        }

                        IsShowingGridView = true;
                        IsShowingLogView = false;
                        
                    },
                    () => !IsBusy));
            }
        }

        private RelayCommand _showLogViewCommand;
        public RelayCommand ShowLogViewCommand
        {
            get
            {
                return _showLogViewCommand
                    ?? (_showLogViewCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowLogViewCommand.CanExecute(null))
                        {
                            return;
                        }

                        IsShowingGridView = false;
                        IsShowingLogView = true;
                    },
                    () => !IsBusy));
            }
        }

        private RelayCommand _exportCommand;
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

                        if (IsShowingGridView)
                        {
                            // export the grid as a spreadsheet

                            if (ViewData == null)
                                return;

                            var suggestedName = string.Format("{0} {1} Batch {2} Details.xlsx", Vendor.Name, BatchDetails.BatchType.DescriptionAttr(), BatchID);

                            Task.Run(() =>
                            {
                                // do not run on UX thread else could freeze UX

                                // need to call the common static method to export data 
                                // public static new void Export(IEnumerable<IViewData> data, string suggestedName)

                                var viewDataType = GetViewDataType(BatchType);
                                var parameters = new object[] { ViewData, suggestedName };
                                viewDataType.GetMethod("Export", BindingFlags.Public | BindingFlags.Static).Invoke(null, parameters);
                            });

                            // tried to get this working with message bus plus generics - but gave up.
                            //var msg = new RequestExportExcelFile<DiscontinuedViewData>(ViewData.Cast<DiscontinuedViewData>(), suggestedName);
                            //MessengerInstance.Send(msg);
                        }
                        else
                        {
                            // export the log as a text file
                            var log = LogEvents.ToList();
                            var suggestedName = string.Format("{0} {1} Batch {2} Log.txt", Vendor.Name, BatchDetails.BatchType.DescriptionAttr(), BatchID);
                            MessengerInstance.Send(new RequestExportTextFile(log, suggestedName));
                        }

                    },
                    () => !IsBusy));
            }
        }


        private RelayCommand _exportJSONCommand;
        public RelayCommand ExportJSONCommand
        {
            get
            {
                return _exportJSONCommand
                    ?? (_exportJSONCommand = new RelayCommand(
                    () =>
                    {
                        if (!ExportJSONCommand.CanExecute(null))
                        {
                            return;
                        }

                        // export the JSON collection

                        if (ViewData == null)
                            return;

                        var suggestedName = string.Format("{0} {1} Batch {2} JSON.txt", Vendor.Name, BatchDetails.BatchType.DescriptionAttr(), BatchID);

                        Task.Run(async () =>
                        {
                            var details = await dbScanner.GetCommitBatchAsync(BatchID.GetValueOrDefault());
                            var json =  details.CommitData.UnGZipMemoryToString().ConvertToListOfLines();
                            MessengerInstance.Send(new RequestExportTextFile(json, suggestedName));
                        });

                    },
                    () => !IsBusy));
            }
        }
        #endregion
    }
}